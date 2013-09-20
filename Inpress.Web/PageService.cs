using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Inpress.Web
{
    public class PageService
    {
        private readonly Project project;
        private readonly string configPath = "/src/assets/json";
        private readonly string pagePath = "/src/assets/json/pages";

        public PageService(int projectId)
        {
            var repository = new ProjectRepository();
            this.project = repository.GetAll().Where(p => p.Id == projectId).SingleOrDefault();
        }

        public void Save(Page page, string encodedHtml)
        {
            var target = HttpContext.Current.Server.MapPath(Path.Combine(project.Location + pagePath, page.Id + ".txt"));

            if (string.IsNullOrEmpty(encodedHtml)) throw new ArgumentNullException("encodedHtml");
            if (File.Exists(target)) throw new IOException("This page already exists");

            var decodedHtml = System.Uri.UnescapeDataString(encodedHtml).Trim().Replace(System.Environment.NewLine, "");
            var json = JsonConvert.SerializeObject(JObject.Parse(ParseHtml(page, decodedHtml)), Formatting.Indented);
            
            File.WriteAllText(target, json);
        }

        public void Delete(string id)
        {
            var page = Get(id);
            if (File.Exists(page.Path)) File.Delete(page.Path);
        }
        
        public IList<Page> GetAll()
        {
            var fullPath = HttpContext.Current.Server.MapPath(project.Location + pagePath);
            var pages = new List<Page>();

            foreach (var file in Directory.GetFiles(fullPath))
            {
                using (var sr = new StreamReader(file))
                {
                    var line = sr.ReadToEnd();
                    var json = JObject.Parse(line);
                    var page = new Page()
                    {
                        Id = json["id"].ToString(),
                        ParentId = (json["parent"] == null) ? null : json["parent"].ToString(),
                        Title = (json["title"] == null) ? json["id"].ToString() : json["title"].ToString(),
                        Path = file,
                        ProjectId = project.Id
                    };
                    pages.Add(page);
                }
            }

            return pages;
        }

        public Page Get(string id)
        {
            return GetAll().Where(p => p.Id == id).SingleOrDefault();
        }

        public string GetPageTree()
        {
            var sb = new StringBuilder();
            var pages = GetAll().Where(p => string.IsNullOrEmpty(p.ParentId));

            if (pages.Count() > 0)
            {
                sb.Append("<ul class=\"tree\">");
                foreach (var page in pages)
                {
                    CreateItem(1, sb, page);
                }
                sb.Append("</ul>");
            }

            return sb.ToString();
        }

        public void CreatePreview(string encodedHtml)
        {
            if (string.IsNullOrEmpty(encodedHtml) || encodedHtml == "undefined") throw new ArgumentNullException("encodedHtml");
            
            var decodedHtml = System.Uri.UnescapeDataString(encodedHtml);
            var path = GetIndexLocation(true);

            if (!string.IsNullOrEmpty(path))
            {
                var sb = new StringBuilder();
                using (var sr = new StreamReader(path))
                {
                    string line;
                    do
                    {
                        line = sr.ReadLine();
                        sb.AppendLine(line);
                    } while (!line.Contains("<body>"));

                    sb.Append(decodedHtml.Trim());

                    do
                    {
                        line = sr.ReadLine();
                    } while (!line.Contains("</body>"));

                    sb.Append(line);
                    sb.Append(sr.ReadToEnd());
                }

                using (var sr = new StreamWriter(path))
                {
                    sr.Write(sb.ToString());
                }
            }
            else
            {
                //TODO: Create our new index
            }
        }

        public string GetIndexLocation(bool physicalPath)
        {
            var extensions = new string[] { ".html", ".htm" };

            foreach (var ext in extensions)
            {
                var path = project.Location + "/Index" + ext;
                var fullPath = HttpContext.Current.Server.MapPath(path);
                if (File.Exists(fullPath))
                    if (physicalPath)
                        return fullPath;
                    else
                        return path;
            }

            return null;
        }

        public string GetIndexLocation()
        {
            return GetIndexLocation(false);
        }

        #region Private methods

        private string ParseHtml(Page page, string html)
        {
            var doc = new HtmlDocument();
            var sb = new StringBuilder();
            var additionalJson = "";

            if (!string.IsNullOrEmpty(page.ParentId)) additionalJson += ",\"parent\":\"" + page.ParentId + "\"";

            doc.LoadHtml(html);
            CreateJson(sb, ParseNode(doc.DocumentNode.FirstChild, additionalJson)); // Get the first node

            return sb.ToString();
        }

        private void CreateJson(StringBuilder sb, string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(json);
                else
                    sb.Append(", " + json);
            }
        }

        private string ParseNode(HtmlNode node, string additionalJson)
        {
            if (node.Name != "#text")
            {
                var id = node.Id;
                var element = node.Name;
                var attributes = "";
                var content = "";

                if (node.Attributes.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var attribute in node.Attributes)
                    {
                        CreateJson(sb, "{ \"" + attribute.Name + "\":\"" + attribute.Value + "\" }");
                    }
                    if (!string.IsNullOrEmpty(sb.ToString()))
                        attributes = string.Format(", \"attributes\":[ {0} ]", sb.ToString());
                }

                if (node.ChildNodes.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var child in node.ChildNodes)
                    {
                        CreateJson(sb, ParseNode(child));
                    }

                    if (!string.IsNullOrEmpty(sb.ToString()))
                        content = string.Format(", \"content\": [ {0} ]", sb.ToString());
                }

                var json = "{ \"id\":\"" + id + "\", \"element\":\"" + element + "\" " + attributes + content + additionalJson + " }";
                return json;
            }
            else
            {
                var text = node.InnerText.Trim().Replace(System.Environment.NewLine, "");
                return string.IsNullOrEmpty(text) ? null : "\"" + text + "\""; // Return just text
            }
        }

        private string ParseNode(HtmlNode node)
        {
            return ParseNode(node, null);
        }

        private string BuildChildren(string parentId, int level)
        {
            var sb = new StringBuilder();
            var pages = GetAll().Where(p => p.ParentId == parentId);

            if (pages.Count() > 0)
            {
                sb.Append("<ul>");
                foreach (var page in pages)
                {
                    CreateItem(level, sb, page);
                }
                sb.Append("</ul>");
            }

            return sb.ToString();
        }

        private void CreateItem(int level, StringBuilder sb, Page page)
        {
            sb.Append("<li id=\"order-" + page.Order + "\"><span class=\"tick\"></span><span class=\"actions\"><div class=\"btn-group\"><a class=\"view-page btn btn-default btn-small\" href=\"#\" data-id=\"" + page.Id + "\">View</a><a class=\"edit-page btn btn-default btn-small\" href=\"/Pages/Edit?id=" + page.Id + "\">Edit</a><a class=\"delete-page btn btn-default btn-small\" href=\"#\" data-id=\"" + page.Id + "\">Delete</a></div></span><div class=\"item\" id=\"" + page.Id + "\" title=\"" + page.Title + "\"><span class=\"arr indent" + level + "\"></span><span class=\"title\">" + page.Title + "</span></div>");
            sb.Append(BuildChildren(page.Id, level + 1));
            sb.Append("</li>");
        }

        #endregion

    }
}

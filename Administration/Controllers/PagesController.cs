using Inpress.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Administration.Controllers
{
    public class PagesController : Controller
    {

        private readonly ProjectService projectService;
        private readonly PageService service;
        private readonly int projectId = 1;

        public PagesController()
        {
            //TODO: The session is not available. Think of a new way to get our project id
            this.service = new PageService(projectId);
            this.projectService = new ProjectService();
        }

        //
        // GET: /Pages/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Pages/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Pages/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Pages/Create

        [HttpPost]
        public ActionResult Create(Page page, string html)
        {
            try
            {
                TempData["html"] = html; // Save our html incase there is an error
                service.Save(page, html); // Save our html

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message.ToString());
                return View(page);
            }
        }

        //
        // GET: /Pages/Edit/homePage

        public ActionResult Edit(string id)
        {
            return View(service.Get(id));
        }

        //
        // POST: /Pages/Edit/homePage

        [HttpPost]
        public ActionResult Edit(Page page)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // AJAX: /Pages/Delete/homePage

        public JsonResult Delete(string id)
        {
            try
            {
                service.Delete(id);
                return new JsonResult { Data = new { success = true } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, error = ex.Message } };
            }
        }

        //
        // AJAX: /Pages/GetPageTree/

        public JsonResult GetPageTree()
        {
            try
            {
                var tree = service.GetPageTree();
                return new JsonResult { Data = new { success = true, tree = tree } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, error = ex.Message } };
            }
        }

        //
        // AJAX: /Pages/CreatePreview/

        public JsonResult CreatePreview(string html)
        {
            try
            {
                var path = service.GetIndexLocation();
                service.CreatePreview(html);
                return new JsonResult { Data = new { success = true, path = path } };
            }
            catch (ArgumentNullException)
            {
                return new JsonResult { Data = new { success = false, error = "You have failed to supply a required field." } };
            }
            catch (Exception ex)
            {
                return new JsonResult { Data = new { success = false, error = ex.Message } };
            }
        }
	}
}
using System.ComponentModel.DataAnnotations;

namespace Inpress.Web
{
    public class Page
    {
        public int ProjectId { get; set; }
        [Required] [Display(Name = "Page Id")] public string Id { get; set; }
        [Display(Name = "Parent")] public string ParentId { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
    }
}

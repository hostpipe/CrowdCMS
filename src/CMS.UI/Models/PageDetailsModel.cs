using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class PageDetailsModel
    {
        public int ContentID { get; set; }
        public int SiteMapID { get; set; }
        [Display(Name = "Page Domain")]
        public int DomainID { get; set; }
        [Display(Name = "Page Name")]
        [Required(ErrorMessage = "Page Name is required.")]
        public string Name { get; set; }
        [Display(Name = "Menu Text")]
        public string MenuText { get; set; }
        public bool TopLevel { get; set; }
        [Display(Name = "Choose A Parent Page")]
        public int ParentID { get; set; }
        [Display(Name = "Page URL")]
        [RegularExpression("^[a-zA-Z_\\-]{1}[\\w_/\\-]*", ErrorMessage = "Path can contain only letters, numbers and characters '_', '/', '-' and cannot start with '/'.")]
        [Required(ErrorMessage = "Path is required.")]
        public string Path { get; set; }
        [Display(Name = "Menu")]
        public bool Menu { get; set; }
        [Display(Name = "Footer")]
        public bool Footer { get; set; }
        public bool IsHomePage { get; set; }
        [Display(Name = "Custom Layout")]
        public int CustomLayout { get; set; }
    }
}
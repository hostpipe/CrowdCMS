using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class AdminMenuItemModel
    {
        public int AdminMenuID { get; set; }
        [Required(ErrorMessage="Menu text is required.")]
        [Display(Name="Menu Text")]
        public string MenuText { get; set; }
        [Required(ErrorMessage="URL is required.")]
        public string URL { get; set; }
        [Required(ErrorMessage="Please select parent item.")]
        [Display(Name="Parent")]
        public int ParentID { get; set; }
    }
}
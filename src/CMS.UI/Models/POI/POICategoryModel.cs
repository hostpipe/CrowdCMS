using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class POICategoryModel
    {
        public int POICategoryID { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Display(Name = "Is Live")]
        public bool IsLive { get; set; }
    }
}
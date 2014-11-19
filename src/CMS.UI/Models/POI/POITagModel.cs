using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class POITagModel
    {
        public int POITagID { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Display(Name = "POI Tags Group")]
        [Required(ErrorMessage = "Title is required")]
        [Range(1, Int16.MaxValue, ErrorMessage = "Title is required")]
        public int POITagsGroupID { get; set; }
    }
}
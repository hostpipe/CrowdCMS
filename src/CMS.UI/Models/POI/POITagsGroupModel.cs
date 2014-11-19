using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class POITagsGroupModel
    {
        public int POITagsGroupID { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public virtual List<SelectListItem> POITags { get; set; }
    }
}
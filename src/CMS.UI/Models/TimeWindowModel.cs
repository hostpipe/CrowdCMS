using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class TimeWindowModel
    {
        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required.")]
        [RegularExpression(@"[0-9]*[\.]?[0-9]+", ErrorMessage = "Price must be a Numbers only.")]
        public string Price { get; set; }

        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Start date is required.")]
        public string StartDate { get; set; }

        [Display(Name = "End Date")]
        public string EndDate { get; set; }

        public int ProductPriceID { get; set; }
        public int ProductPriceTimeWindowID { get; set; }
    } 
}
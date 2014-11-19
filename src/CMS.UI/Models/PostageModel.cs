using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class PostageModel
    {
        public int? PostageID { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string PST_Description { get; set; }
        [Display(Name = "Cost")]
        public decimal? PST_Amount { get; set; }
        [Display(Name = "Weight")]
        public int? PST_PostageWeight { get; set; }
        [Required]
        [Display(Name = "Zone")]
        public int PST_PostageZone { get; set; }
        [Display(Name = "Band")]
        public int? PST_PostageBand { get; set; }
        [Required]
        [Display(Name = "Domain")]
        public int PST_DomainID { get; set; }

    }
}
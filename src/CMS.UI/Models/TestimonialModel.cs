using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class TestimonialModel
    {
        public int TestimonialID { get; set; }
        [Required(ErrorMessage = "Client name is required.")]
        [Display(Name="Client")]
        public string Client { get; set; }
        [Required(ErrorMessage = "Company name is required.")]
        [Display(Name="Company")]
        public string Company { get; set; }
        [Required(ErrorMessage = "Date is required.")]
        [Display(Name = "Date / Time")]
        public string TestimonialDate { get; set; }
        public string TestimonialContent { get; set; }
        public bool IsLive { get; set; }
    }
}
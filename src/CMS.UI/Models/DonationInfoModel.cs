using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class DonationInfoModel
    {
        public int DonationInfoID { get; set; }
        [Required]
        [Display(Name = "Select Domain")]
        public int DI_DomainID { get; set; }
        [Required]
        [Display(Name = "Amount")]
        public decimal DI_Amount { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string DI_Title { get; set; }
        [Display(Name = "Description")]
        public string DI_Description { get; set; }
        [Required]
        [Display(Name = "Select Donation Type")]
        public int DI_DonationTypeID { get; set; }
        [Display(Name = "Live")]
        public bool DI_Live { get; set; }
    }
}
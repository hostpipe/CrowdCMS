using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.BL.Entity;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class FormModel
    {
        public FormModel()
        {
            this.Captcha = true;
        }

        public int DomainID { get; set; }
        public int FormID { get; set; }
        public int DestinationPageID { get; set; }

        [Display(Name = "Conversion Tracking Code")]
        public string TrackingCode { get; set; }

        [Display(Name = "Form Title")]
        [Required(ErrorMessage = "Form title is required.")]
        public string Name { get; set; }
        public string Description { get; set; }

        public bool Captcha { get; set; }
        public string RecipientEmail { get; set; }
        public string CompanyName { get; set; }
        public string PublicKey { get; set; }

        public List<tbl_FormItem> FormItems { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class DomainLinkModel
    {
        public int DomainID { get; set; }
        [Display(Name = "Domain Name")]
        [Required(ErrorMessage = "Domain Name is required.")]
        [RegularExpression("^([a-zA-Z0-9]([a-zA-Z0-9\\-]{0,61}[a-zA-Z0-9])?\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Domain Link should have a correct format.")]
        public string DomainLink { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class CountryModel
    {
        public int CountryID { get; set; }
        [Display(Name = "Country Code")]
        [Required(ErrorMessage = "Code Is required.")]
        [RegularExpression("^[a-z,A-Z]{1,4}$", ErrorMessage = "Code is invalid.")]
        public string Code { get; set; }
        [Display(Name="Name")]
        [Required(ErrorMessage = "Country name is required.")]
        public string Country { get; set; }
        [Display(Name="Postage Zone")]
        public int? PostageZoneID { get; set; }
        [Display(Name="Domain")]
        [Required(ErrorMessage = "Domain is required.")]
        public int DomainID { get; set; }
        [Display(Name="Default Country")]
        public bool IsDefault { get; set; }
    }
}
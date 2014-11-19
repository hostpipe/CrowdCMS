using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class AddressModel
    {
        public int AddressID { get; set; }
        public string Title { get; set; }
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Surname is required.")]
        public string Surname { get; set; }
        //[Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; }
        [Display(Name = "Address 1")]
        [Required(ErrorMessage = "Address 1 is required.")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "Address 3")]
        public string Address3 { get; set; }
        [Display(Name = "Town/City")]
        [Required(ErrorMessage = "Town / City is required.")]
        public string Town { get; set; }
        public string County { get; set; }
        [Required(ErrorMessage = "Postcode is required.")]
        public string Postcode { get; set; }
        public string Country { get; set; }
        [Required(ErrorMessage = "Country is required.")]
        [Display(Name = "Country")]
        public int CountryID { get; set; }

        public SelectList countryList;
    }
}

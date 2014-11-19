using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.BL;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace CMS.UI.Models
{
    public class DonationCheckoutModel
    {
        public DonationType Type { get; set; }
        [Required(ErrorMessage = "Amount is required.")]
        [RegularExpression(@"[0-9]*[\.]?[0-9]+", ErrorMessage = "Amount must be a number.")]
        public string Amount { get; set; }
        public bool GiftAid { get; set; }
        [Display(Name = "Preferably Starting On *")]
        public string StartDate { get; set; }

        [Display(Name = "Title *")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Display(Name = "Firstname *")]
        [Required(ErrorMessage = "Firstname is required.")]
        public string FirstName { get; set; }
        [Display(Name = "Surname *")]
        [Required(ErrorMessage = "Surname is required.")]
        public string Surname { get; set; }
        [Display(Name = "Phone *")]
        [Required(ErrorMessage = "Phone is required.")]
        public string Phone { get; set; }
        [Display(Name = "Address 1 *")]
        [Required(ErrorMessage = "Address 1 is required.")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "Address 3")]
        public string Address3 { get; set; }
        [Display(Name = "Town/City *")]
        [Required(ErrorMessage = "Town/City is required.")]
        public string City { get; set; }
        [Display(Name = "Postcode *")]
        [Required(ErrorMessage = "Postcode is required.")]
        public string Postcode { get; set; }
        [Display(Name = "County *")]
        [Required(ErrorMessage = "County is required.")]
        public string State { get; set; }
        [Display(Name = "Country *")]
        [Required(ErrorMessage = "Country is required.")]
        public int CountryID { get; set; }
        [Display(Name = "Email Address *")]
        [Required(ErrorMessage = "Email Address is required.")]
        [Email(ErrorMessage = "Email should have correct format.")]
        public string EmailAddress { get; set; }

        [Display(Name = "Select Payment Type *")]
        [Required(ErrorMessage = "Payment Type is required.")]
        [Range(1, Int16.MaxValue, ErrorMessage = "Payment Type is required.")]
        public int PaymentDomainID { get; set; }
    }
}
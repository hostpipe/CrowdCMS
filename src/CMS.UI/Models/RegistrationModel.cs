using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Please enter your surname.")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [Email(ErrorMessage = "Email Address should have correct email form.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [Email(ErrorMessage = "Email Address should have correct email form.")]
        [Compare("Email", ErrorMessage = "Emails are not the same.")]
        [Display(Name = "Confirm Email")]
        public string ConfEmail { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [Compare("Password", ErrorMessage = "Passwords are not the same.")]
        [Display(Name = "Confirm Password")]
        public string ConfPassword { get; set; }
        [Display(Name = "I Would Like To Receive Newsletter")]
        public bool Newsletter { get; set; }
        [Display(Name = "Permission To Pass Details To 3rd Parties")]
        public bool DetailsFor3rdParties { get; set; }

        [Required(ErrorMessage = "Address 1 is required.")]
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "Address 3")]
        public string Address3 { get; set; }
        [Required(ErrorMessage = "Town is required.")]
        [Display(Name = "Town")]
        public string Town { get; set; }
        [Required(ErrorMessage = "Postcode is required.")]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; }
        [Required(ErrorMessage = "County is required.")]
        [Display(Name = "County")]
        public string County { get; set; }
        [Range(1 , int.MaxValue, ErrorMessage = "Country is required.")]
        [Display(Name = "Country")]
        public int CountryID { get; set; }
    }
}
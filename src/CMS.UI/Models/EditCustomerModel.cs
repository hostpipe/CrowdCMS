using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Web.Mvc;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class EditCustomerModel
    {
        public EditCustomerModel() { }
        public EditCustomerModel(tbl_Customer customer)
        {
            Title = customer.CU_Title;
            FirstName = customer.CU_FirstName;
            Surname = customer.CU_Surname;
            Email = customer.CU_Email;
            Phone = customer.CU_Telephone;
            SubscribeNewsletter = customer.CU_Subscription;
            DetailsFor3rdParties = customer.CU_DetailsFor3rdParties;
        }

        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Please enter your surname.")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [Email(ErrorMessage = "Email Address should have correct email form.")]
        public string Email { get; set; }
        [Display(Name="Phone")]
        public string Phone { get; set; }
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }
        [Display(Name = "New Password")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Passwords are not the same.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
        [Display(Name = "I Would Like To Receive Newsletter")]
        public bool SubscribeNewsletter { get; set; }
        [Display(Name = "Permission To Pass Details To 3rd Parties")]
        public bool DetailsFor3rdParties { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace CMS.UI.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage="Email is required")]
        [Email(ErrorMessage = "Email Address should have correct email form.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
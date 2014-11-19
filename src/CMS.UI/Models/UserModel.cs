using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace CMS.UI.Models
{
    public class UserModel
    {
        [Required(ErrorMessage="UserID is required.")]
        public int UserID { get; set; }
        [Required(ErrorMessage="Username is required.")]
        [Display(Name="Username")]
        public string UserName { get; set; }
        [Required(ErrorMessage="Email Address is required.")]
        [Display(Name="Email Address")]
        [Email(ErrorMessage = "Email Address should have correct email form.")]
        public string Email { get; set; }
        public string Password { get; set; }
        [Required(ErrorMessage="User Group has to be assigned.")]
        [Display(Name="User Group")]
        public int UserGroupID { get; set; }
    }
}
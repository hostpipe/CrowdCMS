using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class CustomerModel
    {
        [Display(Name = "Name:")]
        public string UserName { get; set; }
        [Display(Name = "Email:")]
        public string Email { get; set; }
        [Display(Name = "Phone:")]
        public string Phone { get; set; }

        public List<tbl_Address> addresses;
        public List<tbl_Gallery> galleries;
    }
}
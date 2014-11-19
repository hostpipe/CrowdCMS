using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace CMS.UI.Models
{
    public class CommentModel
    {
        public int SitemapID { get; set; }
        [Required(ErrorMessage="Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [Email(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; }
        public string Website { get; set; }
        public string Message { get; set; }
    }
}
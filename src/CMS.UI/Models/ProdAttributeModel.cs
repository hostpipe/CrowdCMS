using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class ProdAttributeModel
    {
        public int AttributeID { get; set; }
        [Display(Name = "Attribute Title")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        public List<ProdAttributeValueModel> Values { get; set; }
        [Display(Name = "Attribute Value")]
        [Required(ErrorMessage = "Value is required.")]
        public string Value { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class FormItemValueModel
    {
        public int FormItemValueID { get; set; }
        public int FormItemID { get; set; }
        [Required(ErrorMessage = "Value is required.")]
        [RegularExpression("\\d+", ErrorMessage = "Value has to be a number.")]
        public int Value { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; }
        [Required(ErrorMessage = "Order is required.")]
        [RegularExpression("\\d+", ErrorMessage = "Order has to be a number.")]
        public int Order { get; set; }
    }
}
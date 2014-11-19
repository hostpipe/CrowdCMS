using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.BL.Entity;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class FormItemModel
    {
        public int DomainID { get; set; }
        public int FormID { get; set; }
        public int FormItemID { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Text is required.")]
        public string Text { get; set; }
        public bool Required { get; set; }
        public int TypeID { get; set; }
        public List<FormItemValueModel> ItemValues { get; set; }
        public string Placeholder { get; set; }
    }
}
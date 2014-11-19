using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class DiscountModel
    {
        public int DiscountID { get; set; }
        [Required(ErrorMessage = "Discount Code is required.")]
        [MaxLength(20, ErrorMessage = "Max length is 20 characters.")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(20, ErrorMessage = "Max length is 100 characters.")]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Value number is required.")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Value must be greater than 0.")]
        public decimal Value { get; set; }
        [Display(Name = "Is Percentage")]
        public bool IsPercentage { get; set; }
        public string Start { get; set; }
        public string Expire { get; set; }
        [Required(ErrorMessage = "Domain is required.")]
        [Display(Name = "Domain")]
        public int DomainID { get; set; }
    }
}
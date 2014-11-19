using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CMS.BL;
using DataAnnotationsExtensions;

namespace CMS.UI.Models
{
    public class CreditCardModel
    {
        [Display(Name = "Card Type *")]
        [Required(ErrorMessage = "Card type is required.")]
        public CardType CardType { get; set; }
        [Display(Name = "Name On Card *")]
        [Required(ErrorMessage = "Name is required.")]
        public string CardHolder { get; set; }
        [Display(Name = "Card Number *")]
        [Required(ErrorMessage = "Card number is required.")]
        [CreditCard(ErrorMessage = "The Card Number is not a valid credit card number.")]
        public string CardNumber { get; set; }
        [Display(Name = "Start Date")]
        [RegularExpression("^[0-9]{4}$", ErrorMessage = "Start date is invalid.")]
        public string StartDate { get; set; }
        [Display(Name = "Expiry Date *")]
        [RegularExpression("^[0-9]{4}$", ErrorMessage = "Expiry date is invalid.")]
        [Required(ErrorMessage = "Expiry date is required.")]
        public string ExpiryDate { get; set; }
        [Display(Name = "Issue Number")]
        [RegularExpression("^[0-9]{1,2}$", ErrorMessage = "Issue Number is invalid.")]
        public string IssueNumber { get; set; }
        [Display(Name = "Security Code *")]
        [RegularExpression("^[0-9]{3}$", ErrorMessage = "Security Code is invalid.")]
        [Required(ErrorMessage = "Security code is required.")]
        public string CV2 { get; set; }
    }
}
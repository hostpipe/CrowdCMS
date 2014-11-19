using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace CMS.UI.Models
{
    public class StripeCheckoutViewModel
    {
        [Required]
        [StringLength(20, ErrorMessage = "Please enter valid credit card number")]
        public string CreditCardNumber { get; set; }

        [Required]        
        public int Cvv { get; set; }

        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Required]
        [Min(1900)]
        public int ExpiryYear { get; set; }

        public int OrderId { get; set; }
    }
}
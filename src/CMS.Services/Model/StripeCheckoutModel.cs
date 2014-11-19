namespace CMS.Services.Model
{
    public class StripeCheckoutModel
    {
        public string CreditCardNumber { get; set; }

        public int Cvv { get; set; }

        public int ExpiryMonth { get; set; }

        public int ExpiryYear { get; set; }

        public int OrderId { get; set; }
    }
}

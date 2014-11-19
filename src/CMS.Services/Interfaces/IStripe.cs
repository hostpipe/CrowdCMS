using CMS.Services.Model;

namespace CMS.Services.Interfaces
{
    public interface IStripe
    {
        StripeResult ProcessPayment(StripeCheckoutModel model, string apiKey, int domainId);
    }
}

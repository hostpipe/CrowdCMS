using System.Linq;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Repository;
using CMS.Services.Interfaces;
using CMS.Services.Model;
using Stripe;

namespace CMS.Services
{
    public class Stripe : ServiceBase, IStripe
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ICustomerRepository _customerRepository;

        private const string DefaultCurrency = "gbp";

        public Stripe()
        {
            _ordersRepository = new OrdersRepository(Context);
            _customerRepository = new CustomerRepository(Context);
        }

        #region Public

        public StripeResult ProcessPayment(StripeCheckoutModel model, string apiKey, int domainId)
        {
            var order = _ordersRepository.GetByID(model.OrderId);
           
            if (order == null)
            {
                return new StripeResult { IsError = true, Message = "No order has been found" };
            }
                                   
            try
            {
                var customerStripeId = "";

                var customer = _customerRepository.GetAll().Where(c => c.CU_DomainID == domainId && c.CU_Email == order.CustomerEMail && c.CU_StripeId != null).ToList().LastOrDefault();

                if (customer != null)
                {
                    customerStripeId = customer.CU_StripeId;
                }
                else
                {
                    var newStripeCustomer = CreateStripeCustomer(apiKey, order, model);

                    if (newStripeCustomer == null)
                    {
                        return new StripeResult { IsError = true, Message = "Problem with payment service" };
                    }

                    customerStripeId = newStripeCustomer.Id;
                }
                order.tbl_Customer.CU_StripeId = customerStripeId;
                

                var stripeCharge = ChargeStripeCustomer(apiKey, order.Amount, customerStripeId);

                if (stripeCharge.FailureCode != null)
                {
                    order.Status = stripeCharge.FailureMessage;
                    order.O_StatusID = (int)OrderStatus.PaymentFailed;
                    Context.SaveChanges();
                    return new StripeResult {IsError = true, Message = stripeCharge.FailureMessage};
                }

                order.Status = "ok";
                order.O_StatusID = (int)OrderStatus.Paid;
                Context.SaveChanges();

                return new StripeResult{IsError = false, Message = "Success"};
            }
            catch (StripeException ex)
            {
                return new StripeResult {IsError = true, Message = ex.Message};
            }          
        }

        #endregion

        #region Private

        private StripeCharge ChargeStripeCustomer(string apiKey, decimal amount, string customerId)
        {
            var myCharge = new StripeChargeCreateOptions
            {
                Amount = (int?)amount * 100,
                Currency = DefaultCurrency,
                CustomerId = customerId
            };

            var chargeService = new StripeChargeService(apiKey);

            var stripeCharge = chargeService.Create(myCharge);
            return stripeCharge;
        }

        private StripeCustomer CreateStripeCustomer(string apiKey, tbl_Orders order, StripeCheckoutModel model)
        {
            var myCustomer = new StripeCustomerCreateOptions
            {
                Email = order.CustomerEMail,
                Description = order.BillingFullName,
                CardNumber = model.CreditCardNumber,
                CardExpirationYear = model.ExpiryYear.ToString(),
                CardExpirationMonth = model.ExpiryMonth.ToString()
            };

            var customerService = new StripeCustomerService(apiKey);
            var stripeCustomer = customerService.Create(myCustomer);

            return stripeCustomer;
        }

        #endregion
    }
}

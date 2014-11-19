using System;
using System.Web.Mvc;
using CMS.BL.Entity;
using CMS.Services;
using CMS.Services.Interfaces;
using CMS.Services.Model;
using CMS.UI.Models;

namespace CMS.UI.Controllers
{
    public class StripeController : BaseWebController
    {
        private readonly IDomain _domainService;
        private readonly IECommerce _eCommerceService;
        private readonly IStripe _stripeService;

        public StripeController(IDomain domainService,
            IECommerce ecommerceService,
            IUser userService,
            IWebContent webContentService,
            IStripe stripeService)
            : base(domainService, ecommerceService, userService, webContentService)
        {
            _domainService = domainService;
            _eCommerceService = ecommerceService;
            _stripeService = stripeService;
        }

        [HttpGet]
        #if RELEASE
            [RequireHttps]
        #endif
        public ActionResult Payment(int orderID = 0)
        {
            if (orderID <= 0)
            {
                return RedirectToRoute("Website", new {action = "PaymentError", orderID = orderID.ToString()});
            }

            tbl_Orders order = _eCommerceService.GetOrderByID(orderID);

            if (order == null)
            {
                throw new Exception("No order has been found");
            }

            return View();
        }

        [HttpPost]
        #if RELEASE
            [RequireHttps]
        #endif
        [ValidateAntiForgeryToken]
        public ActionResult Payment(StripeCheckoutViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var apiKey = _domainService.GetSettingsValue(BL.SettingsKey.stripeApiKey, DomainID);
                var result =_stripeService.ProcessPayment(new StripeCheckoutModel
                {
                    CreditCardNumber = viewModel.CreditCardNumber,
                    Cvv = viewModel.Cvv,
                    ExpiryMonth = viewModel.ExpiryMonth,
                    ExpiryYear = viewModel.ExpiryYear,
                    OrderId = viewModel.OrderId
                }, apiKey, DomainID);

                if (!result.IsError)
                {
                    return RedirectToAction("ThankYou", "Website", new { orderID = viewModel.OrderId });
                }

                ModelState.AddModelError("",result.Message);
            }
            return View();
        }
    }
}

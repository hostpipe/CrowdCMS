using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SagePayMvc;
using CMS.BL.Entity;
using CMS.Services;
using CMS.Services.Extensions;
using CMS.Utils.Diagnostics;

namespace CMS.UI.Controllers
{
    public class SagePayController : BaseWebController
    {
        private readonly IDomain DomainService;
        private readonly IECommerce ECommerceService;
        private readonly IUser UserService;

        private const string StateCountryCode = "US";

        private readonly VspServerMode ServerMode;
        private readonly PaymentType PaymentType;
        private Configuration SagePayConfiguration;

        public SagePayController(IDomain domainService,
            IECommerce ecommerceService,
            IUser userService,
            IWebContent webContentService)
            : base(domainService, ecommerceService, userService, webContentService)
        {
            this.DomainService = domainService;
            this.ECommerceService = ecommerceService;
            this.UserService = userService;

            switch (this.DomainService.GetSettingsValue(BL.SettingsKey.sagePayMode, this.DomainID).ToLowerInvariant())
            {
                case "test":
                    ServerMode = VspServerMode.Test;
                    break;
                case "live":
                    ServerMode = VspServerMode.Live;
                    break;
                default:
                    ServerMode = VspServerMode.Simulator;
                    break;
            }

            switch (this.DomainService.GetSettingsValue(BL.SettingsKey.sagePayMethod, this.DomainID).ToLowerInvariant())
            {
                case "direct":
                    PaymentType = PaymentType.Direct;
                    break;
                default:
                    PaymentType = PaymentType.Server;
                    break;
            }
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            SagePayConfiguration = new SagePayMvc.Configuration()
            {
                FailedAction = "PaymentFailed",
                FailedController = "SagePay",
                Mode = ServerMode,
                NotificationAction = "PaymentNotification",
                NotificationController = "SagePay",
                NotificationHostName = requestContext.HttpContext.Request.Url.Host,
                PaymentType = PaymentType,
                SuccessAction = "PaymentSuccess",
                SuccessController = "SagePay",
                VendorName = this.DomainService.GetSettingsValue(BL.SettingsKey.sagePayVendorName, this.DomainID)
            };
        }

        public ActionResult Payment(int orderID)
        {
            string errorMessage = string.Empty;
            tbl_Orders order = ECommerceService.GetOrderByID(orderID);
            if (order == null)
                return RedirectToAction("PaymentError", "Website", new { orderID = orderID, errorMessage = "Can not find order." });

            SagePayMvc.Configuration.Configure(SagePayConfiguration);

            var shoppingBasket = new ShoppingBasket(order.OrderID.ToString());
            shoppingBasket.Add(new BasketItem(1, "Order number: " + order.OrderID, order.TotalAmountToPay));
            
            var billingAddress = new Address() {
                Address1 = order.BillingAddress1,
                Address2 = order.BillingAddress2 + " " + order.BillingAddress3,
                City = order.BillingCity,
                Country = order.BillingCountry,
                Firstnames = order.BillingFirstnames,
                Phone = order.BillingPhone,
                PostCode = order.BillingPostCode,
                State = order.BillingCountry == StateCountryCode ? order.BillingState.Substring(0, 2) : String.Empty,
                Surname = order.BillingSurname
            };

            var deliveryAddress = order.IsDeliverable ? new Address() {
                Address1 = order.DeliveryAddress1,
                Address2 = order.DeliveryAddress2 + " " + order.DeliveryAddress3,
                City = order.DeliveryCity,
                Country = order.DeliveryCountry,
                Firstnames = order.DeliveryFirstnames,
                Phone = order.DeliveryPhone,
                PostCode = order.DeliveryPostCode,
                State = order.DeliveryCountry == StateCountryCode ? order.DeliveryState.Substring(0, 2) : String.Empty,
                Surname = order.DeliverySurname
            } : null;

            var creditCard = SessionManager.CreditCard;
            var cardInfo = creditCard == null ? new CreditCardInfo() : new CreditCardInfo()
                {
                    CardHolder = creditCard.CardHolder,
                    CardNumber = creditCard.CardNumber,
                    CardType = creditCard.CardType.ToString(),
                    CV2 = creditCard.CV2,
                    ExpiryDate = creditCard.ExpiryDate
                };

            string vendorTxCode = Guid.NewGuid().ToString();

            TransactionRegistrar request;
            TransactionRegistrationResponse response;
            string currency = DomainService.GetSettingsValue(BL.SettingsKey.sagePayCurrencyCode, this.DomainID);
            try
            {
                request = new SagePayMvc.TransactionRegistrar(SagePayConfiguration, UrlResolver.Current, new HttpRequestSender());
                response = request.Send(Request.RequestContext, vendorTxCode, shoppingBasket, billingAddress, order.IsDeliverable ? deliveryAddress : billingAddress, 
                    order.CustomerEMail, cardInfo, PaymentFormProfile.Normal, currency, order.GiftAid.GetValueOrDefault(false));
            }
            catch(Exception e)
            {
                errorMessage = "Sage Pay payment error.";
                Log.Fatal(errorMessage, e);
                return RedirectToAction("PaymentError", "Website", new { orderID = order.OrderID, errorMessage });
            }
            
            if (response != null)
            {
                ECommerceService.UpdateOrderPayment(vendorTxCode, response.AddressResult, String.Empty, response.AVSCV2, response.CAVV, response.CV2Result, order.GiftAid.GetValueOrDefault(false), 
                    response.PostCodeResult, String.Empty, string.Empty, response.SecurityKey, response.Status.ToString(), response.TxAuthNo, response.VPSTxId, response.ThreeDSecureStatus.ToString(), 
                    BL.SagePayTxType.PAYMENT.ToString(), currency, orderID);

                if (PaymentType == PaymentType.Server && response.Status == ResponseType.Ok && response.TxAuthNo != 0)
                    return Redirect(response.NextURL);
                else if (PaymentType == PaymentType.Direct && (response.Status == ResponseType.Ok || response.Status == ResponseType.Registered || response.Status == ResponseType.Authenticated) && response.TxAuthNo != 0)
                {
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Paid);
                    return RedirectToAction("ThankYou", "Website", new { orderID = order.OrderID });
                }
                else if (PaymentType == PaymentType.Direct && response.Status == ResponseType.ThreeDAuth && !string.IsNullOrWhiteSpace(response.ACSURL))
                {
                    return RedirectToAction("OrderSummaryWithIFrame", "Website", new { md = response.MD, pareq = response.PAReq, vendorTxCode = vendorTxCode, ACSURL = response.ACSURL });
                }

                errorMessage = response.StatusDetail;
                Log.Warn(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}' ", vendorTxCode, response.Status.ToString(), response.StatusDetail));
            }
            else
                ECommerceService.UpdateOrderPayment(vendorTxCode, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, (bool?)null, String.Empty,
                    String.Empty, String.Empty, String.Empty, ResponseType.Error.ToString(), 0, String.Empty, String.Empty, BL.SagePayTxType.PAYMENT.ToString(), String.Empty, orderID);

            return RedirectToAction("PaymentError", "Website", new { orderID = order.OrderID, errorMessage });
        }

        public ActionResult PaymentFailed(SagePayResponse response)
        {
            tbl_Orders order = null;
            if (response != null)
                order = ECommerceService.GetOrderByVendorCode(response.VendorTxCode, this.DomainID);

            return RedirectToAction("PaymentError", "Website", new { orderID = order != null ? order.OrderID : 0, errorMessage = order.Status });
        }

        public ActionResult PaymentNotification(SagePayResponse response)
        {
            if (response != null)
            {
                tbl_Orders order = ECommerceService.GetOrderByVendorCode(response.VendorTxCode, this.DomainID);
                if (order != null)
                {
                    if (response.IsSignatureValid(order.SecurityKey, DomainService.GetSettingsValue(BL.SettingsKey.sagePayVendorName, this.DomainID)))
                    {
                        long txAuthCode = 0;
                        long.TryParse(response.TxAuthNo, out txAuthCode);

                        ECommerceService.UpdateOrderPayment(response.VendorTxCode, response.AddressResult, response.AddressStatus, response.AVSCV2, response.CAVV, 
                            response.CV2Result, response.GiftAid.Equals("1") ? true : false, response.PostCodeResult, response.Last4Digits, response.PayerStatus, 
                            order.SecurityKey, response.Status.ToString(), txAuthCode, response.VPSTxId, response.ThreeDSecureStatus, order.TxType, order.Currency, order.OrderID);

                        switch (response.Status)
	                    {
                            case ResponseType.Abort:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_Aborted);
                                break;
                            case ResponseType.Authenticated:
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Paid);
                                break;
                            case ResponseType.Invalid:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_Invalid);
                                break;
                            case ResponseType.Malformed:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_Malformed);
                                break;
                            case ResponseType.NotAuthed:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_NotAuthed);
                                break;
                            case ResponseType.Ok:
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Paid);
                                break;
                            case ResponseType.Registered:
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Paid);
                                break;
                            case ResponseType.Rejected:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_Rejected);
                                break;
                            case ResponseType.Unknown:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_Unknown);
                                break;
		                    default:
                                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.SagePay_Error);
                                break;
	                    }

                        return new SagePayMvc.ActionResults.ValidOrderResult(response.VendorTxCode, response);
                    }

                    Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'. Invalid signature.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                    return new SagePayMvc.ActionResults.InvalidSignatureResult(response.VendorTxCode);
                }

                Log.Error(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}'. Can not find order in our database.", response.VendorTxCode, response.Status.ToString(), response.StatusDetail));
                return new SagePayMvc.ActionResults.TransactionNotFoundResult(response.VendorTxCode);
            }

            Log.Error("Payment failed, no response.");
            return new SagePayMvc.ActionResults.ErrorResult();
        }

        public ActionResult PaymentSuccess(SagePayResponse response)
        {
            tbl_Orders order = null;
            if (response != null)
                order = ECommerceService.GetOrderByVendorCode(response.VendorTxCode, DomainID);

            return RedirectToAction("ThankYou", "Website", new { orderID = order != null ? order.OrderID : 0 });
        }

        public ActionResult ThreeDSecureForm(string md, string pareq, string vendorTxCode, string ACSURL)
        {
            this.ViewBag.MD = md;
            this.ViewBag.PaReq = pareq;
            this.ViewBag.TermUrl = Url.RouteUrl(null, new System.Web.Routing.RouteValueDictionary(new { controller = "SagePay", action = "ThreeDSecureCallback", vendorTxCode = vendorTxCode }), Request.Url.Scheme, Request.Url.Host);
            this.ViewBag.ACSURL = ACSURL;

            return View("ThreeDSecureForm");
        }

        public ActionResult ThreeDSecureCallbackForm(string url)
        {
            this.ViewBag.Url = url;
            return View("ThreeDSecureCallbackForm");
        }

        public ActionResult ThreeDSecureCallback(CardholdersBankMessage message, string vendorTxCode)
        {
            string errorMessage = String.Empty, url = string.Empty;

            tbl_Orders order = null;
            if (!String.IsNullOrWhiteSpace(vendorTxCode))
                order = ECommerceService.GetOrderByVendorCode(vendorTxCode, DomainID);

            if (order == null)
            {
                errorMessage = String.Format("3D Secure callback failed. Can not find order for {0}", vendorTxCode);
                Log.Warn(errorMessage);
                url = Url.RouteUrl(null, new { controller = "Website", action = "PaymentError", orderID = 0, errorMessage });
                return RedirectToAction("ThreeDSecureCallbackForm", new { url });
            }

            if (message == null)
            {
                errorMessage = String.Format("3D Secure callback failed. NULL Response from bank. Order vendor code: {0}", vendorTxCode);
                Log.Warn(errorMessage);
                url = Url.RouteUrl(null, new { controller = "Website", action = "PaymentError", orderID = order.OrderID, errorMessage });
                return RedirectToAction("ThreeDSecureCallbackForm", new { url });
            }

            TransactionRegistrar request;
            TransactionRegistrationResponse response;
            try
            {
                request = new SagePayMvc.TransactionRegistrar(SagePayConfiguration, UrlResolver.Current, new HttpRequestSender());
                response = request.Send3DSecureResult(new ThreeDSecureResult() { MD = message.MD, PARes = message.PaRes });
            }
            catch (Exception e)
            {
                errorMessage = "Sage Pay Direct 3DSecure Callback payment error.";
                Log.Fatal(errorMessage, e);
                url = Url.RouteUrl(null, new { controller = "Website", action = "PaymentError", orderID = order.OrderID, errorMessage });
                return RedirectToAction("ThreeDSecureCallbackForm", new { url });
            }

            if (response != null)
            {
                ECommerceService.UpdateOrderPayment(vendorTxCode, response.AddressResult, String.Empty, response.AVSCV2, response.CAVV, response.CV2Result, order.GiftAid.GetValueOrDefault(false), 
                    response.PostCodeResult, String.Empty, string.Empty, response.SecurityKey, response.Status.ToString(), response.TxAuthNo, response.VPSTxId, response.ThreeDSecureStatus.ToString(), 
                    BL.SagePayTxType.PAYMENT.ToString(), order.Currency, order.OrderID);

                if ((response.Status == ResponseType.Ok || response.Status == ResponseType.Registered || response.Status == ResponseType.Authenticated) && response.TxAuthNo != 0)
                {
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Paid);
                    url = Url.RouteUrl(null, new { controller = "Website", action = "ThankYou", orderID = order.OrderID });
                    return RedirectToAction("ThreeDSecureCallbackForm", new { url });
                }

                errorMessage = response.StatusDetail;
                Log.Warn(String.Format("Payment failed for order '{0}', status: '{1}', details '{2}', 3DSecure status: '{3}', response from bank: '{4}', md: {5}", vendorTxCode, response.Status.ToString(), response.StatusDetail, response.ThreeDSecureStatus.ToString(), message.PaRes, message.MD));
            }
            else
                ECommerceService.UpdateOrderPayment(vendorTxCode, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, (bool?)null, String.Empty,
                    String.Empty, String.Empty, String.Empty, ResponseType.Error.ToString(), 0, String.Empty, String.Empty, BL.SagePayTxType.PAYMENT.ToString(), String.Empty, order.OrderID);

            url = Url.RouteUrl(null, new { controller = "Website", action = "PaymentError", orderID = order.OrderID, errorMessage });
            return RedirectToAction("ThreeDSecureCallbackForm", new { url });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.Services;
using System.Web.Mvc;
using CMS.UI.Common.Payment;
using CMS.BL.Entity;
using CMS.Utils.Diagnostics;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using CMS.BL;

namespace CMS.UI.Controllers
{
    public class PayPalController : BaseWebController
    {
        private readonly IDomain DomainService;
        private readonly IECommerce ECommerceService;
        private readonly IUser UserService;

        public PayPalController(IDomain domainService,
            IECommerce ecommerceService,
            IUser userService,
            IWebContent webContentService)
            : base(domainService, ecommerceService, userService, webContentService)
        {
            this.DomainService = domainService;
            this.ECommerceService = ecommerceService;
            this.UserService = userService;
        }

        #region Properties

        protected string PayPalApiUrlNvp
        {
            get { return string.Format("{0}/nvp", DomainService.GetSettingsValue(SettingsKey.payPalApiUrl, this.DomainID)); }
        }

        public string PayPalLandingPageUrl
        {
            get
            {
                string payPalLandingPage = Url.Action("LandingPage", "PayPal");
                if (Request.Url.Host.Contains("localhost"))
                {
                    return String.Format("http://{0}{1}", Request.Url.Authority, payPalLandingPage);
                }
                else
                {
                    return String.Format("http://{0}{1}", Domain.DO_Domain, payPalLandingPage);
                }
            }
        }

        #endregion

        public ActionResult Payment(int orderID = 0)
        {
            if (orderID > 0)
            {
                tbl_Orders order = ECommerceService.GetOrderByID(orderID);
                if (order == null)
                {
                    throw new Exception("No order has been found");
                }
                else
                {
                    string currencyCode = DomainService.GetSettingsValue(BL.SettingsKey.payPalCurrencyCode, this.DomainID);
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Initialized, currencyCode);
                    string payPalUrl = SetExpressCheckout(order, PayPalLandingPageUrl, PayPalLandingPageUrl,
                                                          DomainService.GetSettingsValue(BL.SettingsKey.payPalUsername, this.DomainID),
                                                          DomainService.GetSettingsValue(BL.SettingsKey.payPalPassword, this.DomainID),
                                                          DomainService.GetSettingsValue(BL.SettingsKey.payPalSignature, this.DomainID),
                                                          currencyCode,
                                                          PayPalApiUrlNvp,
                                                          DomainService.GetSettingsValue(BL.SettingsKey.payPalCgiUrl, this.DomainID),
                                                          order.OrderID.ToString(),
                                                          DomainService.GetSettingsValue(BL.SettingsKey.payPalLanguageCode, this.DomainID));

                    if (!string.IsNullOrEmpty(payPalUrl))
                    {
                        return new RedirectResult(payPalUrl);
                    }
                }
            }

            return RedirectToRoute("Website", new { action = "PaymentError", orderID = orderID.ToString() });
        }

        public ActionResult LandingPage()
        {
            NameValueCollection variables = Request.QueryString;
            string token = HttpUtility.UrlEncode(variables[PayPalConsts.Token]);
            string payerID = HttpUtility.UrlEncode(variables[PayPalConsts.PayerId]);

            tbl_Orders order = string.IsNullOrEmpty(token) ? null : ECommerceService.GetOrderBySecurityKey(token, DomainID);
            if (order != null)
                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_LandingPage);

            string paymentMessage = string.Empty;
            string actionName = "PaymentError";
            string orderID = (order != null) ? order.OrderID.ToString() : "[unknown]";

            if (string.IsNullOrEmpty(payerID))
            {
                paymentMessage = String.Format("PayPal payment canceled for order {0}", orderID);
                Log.Info(paymentMessage);
                if (order != null)
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_LandingPage_PayerUnknown);

                return RedirectToRoute("Website", new { action = "OrderSummary" });
            }
            else
            {
                string currencyCode = DomainService.GetSettingsValue(BL.SettingsKey.payPalCurrencyCode, this.DomainID);
                PayPalPaymentStatus status = DoExpressCheckoutPayment(
                    order,
                    token,
                    payerID,
                    currencyCode,
                    PayPalLandingPageUrl,
                    PayPalLandingPageUrl,
                    DomainService.GetSettingsValue(BL.SettingsKey.payPalUsername, this.DomainID),
                    DomainService.GetSettingsValue(BL.SettingsKey.payPalPassword, this.DomainID),
                    DomainService.GetSettingsValue(BL.SettingsKey.payPalSignature, this.DomainID),
                    PayPalApiUrlNvp,
                    order.OrderID.ToString());


                switch (status.ACK.ToUpper())
                {
                    case "SUCCESS":
                    case "SUCCESSWITHWARNING":
                        {
                            paymentMessage = String.Format("PayPal payment success, pending status [{0}] ", status.PaymentStatus);
                            Log.Info(paymentMessage);
                            if (order != null)
                                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.Paid, currencyCode);

                            actionName = "ThankYou";
                        } break;
                    case "FAILURE":
                    case "FAILUREWITHWARNING":
                    case "WARNING":
                        {
                            paymentMessage = String.Format("PayPal payment failed [{0}] ", status.ErrorMessage);
                            Log.Fatal(paymentMessage);
                            ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_DoExpressCheckout_Failure, currencyCode);
                        } break;
                    default:
                        {
                            paymentMessage = String.Format("PayPal first step unknown result [{0}] ", status.ACK + ": " + status.ErrorMessage);
                            Log.Fatal(paymentMessage);
                            ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_DoExpressCheckout_Unknown, currencyCode);
                        } break;
                }
            }

            return RedirectToRoute("Website", new { action = actionName, orderID = orderID });
        }

        private NVPCodec InitializeEncoder(tbl_Orders order)
        {
            decimal totalItemsPrice = order.tbl_OrderContent.Sum(oc => oc.OC_TotalPrice) + order.DependentOrders.Sum(o => o.TotalAmount);

            string totalAmount = Math.Round(order.TotalAmountToPay, 2).ToString().Replace(',', '.');
            string totalTaxAmount = Math.Round(order.TotalTaxAmount, 2).ToString().Replace(',', '.');
            string totalItemsAmount = order.O_IsCustomAmount ? order.TotalAmountToPay.ToString().Replace(',', '.') : Math.Round(totalItemsPrice, 2).ToString().Replace(',', '.');
            string totalShippingAmount = Math.Round(order.TotalDeliveryAmount, 2).ToString().Replace(',', '.');

            NVPCodec encoder = new NVPCodec();
            encoder[PayPalConsts.Version] = "98.0";
            encoder[PayPalConsts.TotalAmount] = totalAmount;
            encoder[PayPalConsts.DeliveryAmount] = totalShippingAmount;
            encoder[PayPalConsts.ItemsTotalAmount] = totalItemsAmount;

            if (order.DiscountAmount != 0)
            {
                decimal discount = order.DiscountAmount > totalItemsPrice ? totalItemsPrice : order.DiscountAmount;
                encoder[PayPalConsts.DiscountAmount] = Math.Round(-Math.Abs(discount), 2).ToString().Replace(',', '.');
            }

            // if order items should be listed in order details displayed on PayPal account
            if (DomainService.GetSettingsValueAsBool(BL.SettingsKey.payPalSendOrderItems, this.DomainID) && !order.O_IsCustomAmount)
            {
                int i = 0;
                foreach (var oc in order.tbl_OrderContent)
                {
                    encoder[PayPalConsts.GetItemNumberKey(i)] = oc.tbl_Products==null ? oc.tbl_ProductPrice.PR_ProductID.ToString() : oc.tbl_Products.P_ProductCode;
                    encoder[PayPalConsts.GetItemNameKey(i)] = oc.OC_Title ?? "";
                    encoder[PayPalConsts.GetItemDescriptionKey(i)] = oc.OC_Description ?? "";
                    encoder[PayPalConsts.GetItemQuantityKey(i)] = oc.OC_Quantity.ToString();
                    encoder[PayPalConsts.GetItemsTotalAmountKey(i)] = Math.Round(oc.OC_TotalPrice / oc.OC_Quantity.GetValueOrDefault(1), 2).ToString().Replace(',', '.');

                    i++;
                }
                if (order.DependentOrders.Any())
                {
                    foreach (var donation in order.DependentOrders)
                    {
                        encoder[PayPalConsts.GetItemNumberKey(i)] = donation.OrderID.ToString();
                        encoder[PayPalConsts.GetItemNameKey(i)] = String.Format("Donation to order {0}", order.OrderID);
                        encoder[PayPalConsts.GetItemDescriptionKey(i)] = "donation";
                        encoder[PayPalConsts.GetItemQuantityKey(i)] = "1";
                        encoder[PayPalConsts.GetItemsTotalAmountKey(i)] = Math.Round(donation.TotalAmount, 2).ToString().Replace(',', '.');

                        i++;
                    }
                }
            }

            return encoder;
        }

        private string SetExpressCheckout(tbl_Orders order, string payPalReturnUrl, string payPalCancelUrl, string payPalUser, string payPalPassword, string payPalSignature, string payPalCurrencyCode, string payPalNvpSetExpressUrl, string payPalHost, string invoiceID, string languageCode)
        {
            string resultToLog;

            NVPCodec encoder = InitializeEncoder(order);
            encoder[PayPalConsts.Method] = "SetExpressCheckout";
            encoder[PayPalConsts.NoShipping] = "1";
            encoder[PayPalConsts.ReturnUrl] = payPalReturnUrl;
            encoder[PayPalConsts.CancelUrl] = payPalCancelUrl;
            encoder[PayPalConsts.User] = payPalUser;
            encoder[PayPalConsts.Pwd] = payPalPassword;
            encoder[PayPalConsts.Signature] = payPalSignature;
            encoder[PayPalConsts.CurrencyCode] = payPalCurrencyCode;
            encoder[PayPalConsts.InvoiceID] = invoiceID;

            if (!string.IsNullOrEmpty(languageCode))
            {
                encoder[PayPalConsts.LocaleCode] = languageCode;
            }

            string settings = encoder.GetSettings();
            Log.Info(settings);

            string encoded = encoder.Encode();
            string result = resultToLog = HttpPost(payPalNvpSetExpressUrl, encoded, 30000);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(result);

            string token = decoder[PayPalConsts.Token];
            if (decoder[PayPalConsts.ACK].ToLower() == "success" || decoder[PayPalConsts.ACK].ToLower() == "successwithwarning")
            {
                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_SetExpressCheckout_Success);
                ECommerceService.UpdateOrderSecurityKey(token, order.OrderID);
                return String.Format("{0}?cmd=_express-checkout&{1}={2}&{3}={4}", payPalHost, PayPalConsts.Token.ToLower(), token, PayPalConsts.Useraction, PayPalConsts.Commit);
            }
            else
            {
                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_SetExpressCheckout_Failure);
                resultToLog = Server.UrlDecode(resultToLog).Replace("&", Environment.NewLine);
                Log.Error(String.Format("PayPal payment - SetExpressCheckout failed: {0}", resultToLog));
            }

            return String.Empty;
        }

        public PayPalPaymentStatus DoExpressCheckoutPayment(tbl_Orders order, string token, string payerId, string payPalCurrencyCode, string payPalReturnUrl, string payPalCancelUrl, string payPalUser, string payPalPassword, string payPalSignature, string payPalNvpSetExpressUrl, string invoiceID)
        {
            string resultToLog;

            NVPCodec encoder = InitializeEncoder(order);
            encoder[PayPalConsts.Method] = "DoExpressCheckoutPayment";
            encoder[PayPalConsts.Token] = token;
            encoder[PayPalConsts.PaymentAction] = "Sale";
            encoder[PayPalConsts.PayerId] = payerId;
            encoder[PayPalConsts.CurrencyCode] = payPalCurrencyCode;
            encoder[PayPalConsts.ReturnUrl] = payPalReturnUrl;
            encoder[PayPalConsts.CancelUrl] = payPalCancelUrl;
            encoder[PayPalConsts.User] = payPalUser;
            encoder[PayPalConsts.Pwd] = payPalPassword;
            encoder[PayPalConsts.Signature] = payPalSignature;
            encoder[PayPalConsts.InvoiceID] = invoiceID;

            string encoded = encoder.Encode();
            string result = resultToLog = HttpPost(payPalNvpSetExpressUrl, encoded, 30000);

            NVPCodec decoder = new NVPCodec();
            decoder.Decode(result);

            PayPalPaymentStatus payPalStatus = new PayPalPaymentStatus();

            payPalStatus.TransactionID = decoder[PayPalConsts.TransactionID];
            payPalStatus.Token = decoder[PayPalConsts.Token];
            payPalStatus.ACK = decoder[PayPalConsts.ACK];
            payPalStatus.PaymentStatus = decoder[PayPalConsts.PaymentStatus];
            payPalStatus.PendingReason = decoder[PayPalConsts.PendingReason];
            payPalStatus.ErrorCode = decoder[PayPalConsts.ErrorCode];
            payPalStatus.ErrorMessage = decoder[PayPalConsts.ErrorMessage];

            if (payPalStatus.ACK.ToLower() != "success")
            {
                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, BL.PaymentStatus.PayPal_DoExpressCheckout_Failure);
                resultToLog = Server.UrlDecode(resultToLog).Replace("&", Environment.NewLine);
                Log.Error(String.Format("PayPal payment - DoExpressCheckoutPayment failed: {0}", resultToLog));
            }

            return payPalStatus;
        }

        private string HttpPost(string url, string postData, int timeout)
        {
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Timeout = timeout;
            objRequest.Method = "POST";
            objRequest.ContentLength = postData.Length;

            using (StreamWriter myWriter = new StreamWriter(objRequest.GetRequestStream()))
            {
                myWriter.Write(postData);
            }
            using (WebResponse response = objRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }

    }
}
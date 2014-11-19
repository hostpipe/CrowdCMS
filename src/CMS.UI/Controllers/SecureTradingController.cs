using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CMS.Services;
using CMS.BL.Entity;
using System.Net;
using System.IO;
using CMS.BL;
using System.Threading;
using System.Globalization;
using CMS.UI.Common.Payment;
using CMS.Utils.Cryptography;
using System.Text;
using CMS.Utils.Diagnostics;

namespace CMS.UI.Controllers
{
    public class SecureTradingController : BaseWebController
    {
        private readonly IDomain DomainService;
        private readonly IECommerce ECommerceService;
        private readonly IUser UserService;

        public SecureTradingController(IDomain domainService,
            IECommerce ecommerceService,
            IUser userService,
            IWebContent webContentService)
            : base(domainService, ecommerceService, userService, webContentService)
        {
            this.DomainService = domainService;
            this.ECommerceService = ecommerceService;
            this.UserService = userService;
        }

        public ActionResult Payment(int orderID = 0)
        {
            if (orderID > 0)
            {
                tbl_Orders order = ECommerceService.GetOrderByID(orderID);
                if (order == null)
                    throw new Exception("No order has been found");

                string currencyCode = DomainService.GetSettingsValue(SettingsKey.secureTradingCurrencyCode, this.DomainID);

                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.Initialized, currencyCode);
                return new RedirectResult(UrlConstructor(order));
            }
            return RedirectToRoute("Website", new { action = "PaymentError", orderID = orderID.ToString() });
        }

        public ActionResult PaymentSuccess(string errorcode, int orderreference, string sitereference, string transactionreference, string responsesitesecurity)
        {
            if (!VerifyRedirectSecurityCode(errorcode, orderreference, sitereference, transactionreference, responsesitesecurity))
            {
                Log.Error(String.Format("Success. Security code not verified. Data: {0}, {1}, {2}, {3}, {4}", errorcode, orderreference, sitereference, transactionreference, responsesitesecurity));
                return RedirectToRoute("Website", new { action = "PaymentError", orderID = orderreference });
            }

            var order = ECommerceService.GetOrderByID(orderreference);
            if (order == null)
                throw new Exception("No order has been found");

            ECommerceService.UpdateOrderVendorTxCodeAndStatus(order.OrderID, transactionreference, errorcode);
            if (errorcode == SecureTradingConsts.ErrorCode_Paid)
            {
                ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.Paid);
                return RedirectToAction("ThankYou", "Website", new { orderID = order.OrderID });
            }
            return RedirectToRoute("Website", new { action = "PaymentError", orderID = order.OrderID });
        }

        public ActionResult PaymentError(string errorcode, int orderreference, string sitereference, string transactionreference, string responsesitesecurity)
        {
            if (!VerifyRedirectSecurityCode(errorcode, orderreference, sitereference, transactionreference, responsesitesecurity))
            {
                Log.Error(String.Format("Error. Security code not verified. Data: {0}, {1}, {2}, {3}, {4}", errorcode, orderreference, sitereference, transactionreference, responsesitesecurity));
                return RedirectToRoute("Website", new { action = "PaymentError", orderID = orderreference });
            }

            var order = ECommerceService.GetOrderByID(orderreference);
            if (order == null)
                throw new Exception("No order has been found");

            ECommerceService.UpdateOrderVendorTxCodeAndStatus(order.OrderID, transactionreference, errorcode);
            ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.SecureTrading_Error);
            return RedirectToRoute("Website", new { action = "PaymentError", orderID = order.OrderID });
        }

        public ActionResult PaymentDeclined(string errorcode, int orderreference, string sitereference, string transactionreference, string responsesitesecurity)
        {
            if (!VerifyRedirectSecurityCode(errorcode, orderreference, sitereference, transactionreference, responsesitesecurity))
            {
                Log.Error(String.Format("Decline. Security code not verified. Data: {0}, {1}, {2}, {3}, {4}", errorcode, orderreference, sitereference, transactionreference, responsesitesecurity));
                return RedirectToRoute("Website", new { action = "PaymentError", orderID = orderreference });
            }

            var order = ECommerceService.GetOrderByID(orderreference);
            if (order == null)
                throw new Exception("No order has been found");

            ECommerceService.UpdateOrderVendorTxCodeAndStatus(order.OrderID, transactionreference, errorcode);
            ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.SecureTrading_Declined);
            return RedirectToRoute("Website", new { action = "PaymentError", orderID = order.OrderID });
        }

        [HttpPost]
        public ActionResult AuthNotification()
        {
            var s = Request.Form;
            Log.Debug(s.ToString());
            var dic = s.AllKeys.ToDictionary(k => k, v => s[v]);
            if (VerifyNotificationSecurityCode(dic))
            {
                var order = ECommerceService.GetOrderByID(Int32.Parse(dic[SecureTradingConsts.OrderReference]));
                if (order == null)
                {
                    Log.Error("Invalid Order Reference. Securite code successfully validate");
                    return new EmptyResult();
                }
                if (dic[SecureTradingConsts.ErrorCode] == SecureTradingConsts.ErrorCode_Paid)
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.Paid);
                else if (dic[SecureTradingConsts.ErrorCode] == SecureTradingConsts.ErrorCode_Declined)
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.SecureTrading_Declined);
                else
                    ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.SecureTrading_Error);
                ECommerceService.UpdateOrderVendorTxCodeAndStatus(order.OrderID, dic[SecureTradingConsts.TransactionReference], dic[SecureTradingConsts.ErrorCode]);
            }
            return new EmptyResult();
        }

        private string UrlConstructor(tbl_Orders order)
        {
            string currencyCode = DomainService.GetSettingsValue(SettingsKey.secureTradingCurrencyCode, this.DomainID);
            NVPCodec encoder = InitializeEncoder(order, currencyCode);
            return "https://payments.securetrading.net/process/payments/choice?" + encoder.Encode();
        }

        private NVPCodec InitializeEncoder(tbl_Orders order, string currencyCode)
        {
            NVPCodec encoder = new NVPCodec();

            encoder[SecureTradingConsts.SiteReference] = GetReferenceSite(order.O_DomainID);
            encoder[SecureTradingConsts.CurrencyIso3a] = currencyCode;
            encoder[SecureTradingConsts.MainAmount] = order.TotalAmountToPay.ToString("C");
            encoder[SecureTradingConsts.Version] = "1";

            encoder[SecureTradingConsts.OrderReference] = order.OrderID.ToString();

            //encoder[SecureTradingConsts.BillingPremise]
            if (order.BillingAddress1 != null)
                encoder[SecureTradingConsts.BillingStreet] = order.BillingAddress1;
            if (order.BillingCity != null)
                encoder[SecureTradingConsts.BillingTown] = order.BillingCity;
            if (order.BillingState != null)
                encoder[SecureTradingConsts.BillingCounty] = order.BillingState;
            if (order.BillingPostCode != null)
                encoder[SecureTradingConsts.BillingPostCode] = order.BillingPostCode;

            if (order.BillingFirstnames != null)
                encoder[SecureTradingConsts.BillingFirstName] = order.BillingFirstnames;
            if (order.BillingSurname != null)
                encoder[SecureTradingConsts.BillingLastName] = order.BillingSurname;
            if (order.BillingCountry != null)
                encoder[SecureTradingConsts.BillingCountryIso2a] = order.BillingCountry;
            if (order.CustomerEMail != null)
                encoder[SecureTradingConsts.BillingEmail] = order.CustomerEMail;

            if (order.DeliveryAddress1 != null)
                encoder[SecureTradingConsts.CustomerStreet] = order.IsDeliverable ? order.DeliveryAddress1 : order.BillingAddress1;
            if (order.DeliveryCity != null)
                encoder[SecureTradingConsts.CustomerTown] = order.IsDeliverable ? order.DeliveryCity : order.BillingCity;
            if (order.DeliveryState != null)
                encoder[SecureTradingConsts.CustomerCounty] = order.IsDeliverable ? order.DeliveryState : order.BillingState;
            if (order.DeliveryPostCode != null)
                encoder[SecureTradingConsts.CustomerPostCode] = order.IsDeliverable ? order.DeliveryPostCode : order.BillingPostCode;
            if (order.DeliveryCountry != null)
                encoder[SecureTradingConsts.CustomerCountryIso2a] = order.IsDeliverable ? order.DeliveryCountry : order.BillingCountry;

            encoder[SecureTradingConsts.SiteSecurity] = CreateRequestSecurityCode(order, currencyCode);

            return encoder;
        }

        private string CreateRequestSecurityCode(tbl_Orders order, string currencyCode)
        {
            StringBuilder s = new StringBuilder();
            s.Append(order.TotalAmountToPay.ToString("C"))
            .Append(currencyCode)
            .Append(order.OrderID)
            .Append(GetReferenceSite(order.O_DomainID))
            .Append(DomainService.GetSettingsValue(SettingsKey.secureTradingRedirectPassword, this.DomainID));

            return Sha256.GetSHA256Hash(s.ToString());
        }

        private bool VerifyRedirectSecurityCode(string errorcode, int orderreference, string sitereference, string transactionreference, string responsesitesecurity)
        {
            StringBuilder s = new StringBuilder();
            s.Append(errorcode)
            .Append(orderreference)
            .Append(sitereference)
            .Append(transactionreference)
            .Append(DomainService.GetSettingsValue(SettingsKey.secureTradingRedirectPassword, this.DomainID));

            return Sha256.VerifySHA256Hash(s.ToString(), responsesitesecurity);
        }

        private bool VerifyNotificationSecurityCode(Dictionary<string, string> dic)
        {
            StringBuilder s = new StringBuilder();
            s.Append(dic[SecureTradingConsts.ErrorCode])
            .Append(dic[SecureTradingConsts.MainAmount])
            .Append(dic[SecureTradingConsts.OrderReference])
            .Append(dic[SecureTradingConsts.SiteReference])
            .Append(dic[SecureTradingConsts.TransactionReference])
            .Append(DomainService.GetSettingsValue(SettingsKey.secureTradingNotificationPassword, this.DomainID));

            return Sha256.VerifySHA256Hash(s.ToString(), dic[SecureTradingConsts.ResponseSiteSecurity]);
        }

        private string GetReferenceSite(int domainID)
        {
            var mode = DomainService.GetSettingsValue(SettingsKey.secureTradingMode, this.DomainID);
            return (mode == SecureTradingMode.live.ToString()) ?
                DomainService.GetSettingsValue(SettingsKey.secureTradingSiteReference, this.DomainID) :
                DomainService.GetSettingsValue(SettingsKey.secureTradingTestSiteReference, this.DomainID);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Common.Payment
{
    public class PayPalConsts
    {

        // Request
        public static string Method = "METHOD";
        public static string Version = "VERSION";

        public static string TotalAmount = "PAYMENTREQUEST_0_AMT";
        public static string TotalTaxAmount = "PAYMENTREQUEST_0_TAXAMT";
        public static string DeliveryAmount = "PAYMENTREQUEST_0_SHIPPINGAMT";
        public static string DiscountAmount = "PAYMENTREQUEST_0_SHIPDISCAMT";

        public static string ReturnUrl = "RETURNURL";
        public static string CancelUrl = "CANCELURL";
        public static string User = "USER";
        public static string Pwd = "PWD";
        public static string Signature = "SIGNATURE";
        public static string CurrencyCode = "PAYMENTREQUEST_0_CURRENCYCODE";
        public static string PaymentRequest0CurrencyCode = "PAYMENTREQUEST_0_CURRENCYCODE";
        public static string PaymentAction = "PAYMENTREQUEST_0_PAYMENTACTION";
        public static string PaymentRequest0PaymentAction = "PAYMENTREQUEST_0_PAYMENTACTION";
        public static string Token = "TOKEN";
        public static string NoShipping = "NOSHIPPING";
        public static string Useraction = "useraction";
        public static string Commit = "commit";

        private static string ItemName = "L_PAYMENTREQUEST_0_NAME{0}";
        private static string ItemNumber = "L_PAYMENTREQUEST_0_NUMBER{0}";
        private static string ItemDescription = "L_PAYMENTREQUEST_0_DESC{0}";
        private static string ItemAmount = "L_PAYMENTREQUEST_0_AMT{0}";
        private static string ItemQuantity = "L_PAYMENTREQUEST_0_QTY{0}";

        public static string ItemsTotalAmount = "PAYMENTREQUEST_0_ITEMAMT";
        public static string ItemsTotalTaxAmount = "PAYMENTREQUEST_0_TAXAMT";

        //Response
        public static string PayerId = "PAYERID";
        public static string ErrorCode = "PAYMENTINFO_0_ERRORCODE";
        public static string ErrorMessage = "PAYMENTINFO_0_ERRORMESSAGE";
        public static string TransactionID = "PAYMENTINFO_0_TRANSACTIONID";
        public static string ACK = "ACK";
        public static string PaymentStatus = "PAYMENTINFO_0_PAYMENTSTATUS";
        public static string PendingReason = "PAYMENTINFO_0_PENDINGREASON";

        // Others
        public static string InvoiceID = "PAYMENTREQUEST_0_INVNUM";
        public static string LocaleCode = "LOCALECODE";

        public static string GetItemNameKey(int i)
        { 
            return string.Format(ItemName, i);
        }

        public static string GetItemNumberKey(int i)
        {
            return string.Format(ItemNumber, i);
        }

        public static string GetItemDescriptionKey(int i)
        {
            return string.Format(ItemDescription, i);
        }

        public static string GetItemsTotalAmountKey(int i)
        {
            return string.Format(ItemAmount, i);
        }

        public static string GetItemQuantityKey(int i)
        {
            return string.Format(ItemQuantity, i);
        }
    }
}
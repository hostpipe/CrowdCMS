using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Common.Payment
{
    public class SecureTradingConsts
    {
        //Required
        public static string SiteReference = "sitereference";
        public static string CurrencyIso3a = "currencyiso3a";
        public static string MainAmount = "mainamount";
        public static string Version = "version";
        //Billing Address
        public static string BillingPremise = "billingpremise";
        public static string BillingStreet = "billingstreet";
        public static string BillingTown = "billingtown";
        public static string BillingCounty = "billingcounty";
        public static string BillingPostCode = "billingpostcode";

        //Billing Fields
        public static string BillingPrefixName = "billingprefixname";
        public static string BillingFirstName = "billingfirstname";
        public static string BillingLastName = "billinglastname"; 
        public static string BillingCountryIso2a = "billingcountryiso2a";
        public static string BillingEmail = "billingemail";

        //Customer Fields
        public static string CustomerPremise = "customerpremise";
        public static string CustomerStreet = "customerstreet";
        public static string CustomerTown = "customertown";
        public static string CustomerCounty = "customercounty";
        public static string CustomerPostCode = "customerpostcode";
        public static string CustomerCountryIso2a = "customercountryiso2a";

        //Order
        public static string OrderReference = "orderreference";
        public static string SiteSecurity = "sitesecurity";

        public static string ResponseSiteSecurity = "responsesitesecurity";

        public static string TransactionReference = "transactionreference";
        public static string ErrorCode = "errorcode";
        public static string NotificationReference = "notificationreference";


        //Errors
        public static string ErrorCode_Paid = "0";
        public static string ErrorCode_Declined = "7000";
        
    }
}
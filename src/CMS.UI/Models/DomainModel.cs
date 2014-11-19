using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class DomainModel
    {
        public int DomainID { get; set; }
        [Display(Name = "Company Address")]
        public string DO_CompanyAddress { get; set; }
        [Display(Name = "Company Name")]
        public string DO_CompanyName { get; set; }
        [Display(Name = "Company Telephone")]
        public string DO_CompanyTelephone { get; set; }

        [Display(Name = "Stylesheet")]
        public string DO_CSS { get; set; }
        [Display(Name = "Default Language ID")]
        public int? DO_DefaultLangID { get; set; }
        [Display(Name = "Domain")]
        [Required(ErrorMessage = "Domain Name is required.")]
        [RegularExpression("^([a-zA-Z0-9]([a-zA-Z0-9\\-]{0,61}[a-zA-Z0-9])?\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Domain Name should have a correct format.")]
        public string DO_Domain { get; set; }
        [Display(Name = "Email Address")]
        public string DO_Email { get; set; }
        [Display(Name = "Google Analytics")]
        public string DO_GoogleAnalytics { get; set; }
        [Display(Name = "Google Analytics Code")]
        [AllowHtml]
        public string DO_GoogleAnalyticsCode { get; set; }
        [Display(Name = "Google Analytics")]
        public bool DO_GoogleAnalyticsVisible { get; set; }
        public string DO_Robots { get; set; }
        [Display(Name = "Home Page ID")]
        public int DO_HomePageID { get; set; }
        [Display(Name = "Site Launch Year")]
        public int? DO_LaunchYear { get; set; }
        [Display(Name = "Share This ID")]
        public string DO_ShareThis { get; set; }
        [Display(Name = "Use special pages without prefixes in url")]
        public bool DO_CustomRouteHandler { get; set; }

        [Display(Name = "API Key")]
        public string DO_ConsumerKey { get; set; }
        [Display(Name = "API Secret")]
        public string DO_ConsumerSecret { get; set; }
        [Display(Name = "Access Token Secret")]
        public string DO_TwitterSecret { get; set; }
        [Display(Name = "Access Token")]
        public string DO_TwitterToken { get; set; }
        [Display(Name = "Update Twitter")]
        public bool DO_UpdateTwitter { get; set; }

        [Display(Name = "Theme")]
        public string DO_Theme { get; set; }
        [Display(Name = "Development Mode")]
        public bool DO_DevelopmentMode { get; set; }

        [Display(Name = "Enable MailChimp")]
        public bool DO_EnableMailChimp { get; set; }
        [Display(Name = "MailChimp API Key")]
        public string DO_MailChimpAPIKey { get; set; }
        [Display(Name = "MailChimp List ID")]
        public string DO_MailChimpListID { get; set; }

        [Display(Name = "Enable CommuniGator")]
        public bool DO_EnableCommuniGator { get; set; }
        [Display(Name = "CommuniGator UserName")]
        public string DO_CommuniGatorUserName { get; set; }
        [Display(Name = "CommuniGator Password")]
        public string DO_CommuniGatorPassword { get; set; }

        [Display(Name = "Default Events View")]
        public int EventViewType { get; set; }
        [Display(Name = "Enable Events Sale")]
        public bool EnableEventSale { get; set; }
        [Display(Name = "Enable Products Sale")]
        public bool EnableProductSale { get; set; }

        [Display(Name = "Enables PayPal Payment Method")]
        public bool IsPaypalPayment { get; set; }
        [Display(Name = "Enables SagePay Payment Method")]
        public bool IsSagePayPayment { get; set; }
        [Display(Name = "Enables SecureTrading Payment Method")]
        public bool IsSecureTradingPayment { get; set; }
        [Display(Name = "Enables Cookie Consent")]
        public bool IsCookieConsentEnabled { get; set; }
        [Display(Name = "Enables Stripe Payment Method")]
        public bool IsStripePayment { get; set; }

        public List<SettingsValueModel> SettingsValues { get; set; }
        public List<SocialModel> Social { get; set; }
    }
}
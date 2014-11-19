using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Text;

using CMS.BL;
using CMS.BL.Entity;
using CMS.Services;
using CMS.Services.Extensions;
using CMS.UI.Models;
using CMS.UI.Security;
using CMS.Utils;
using CMS.Utils.Extension;
using CMS.Utils.Drawing;
using CMS.Utils.Diagnostics;
using CMS.Utils.Data;

namespace CMS.UI.Controllers
{
    [Authentication]
    public class AdmnController : BaseController
    {
        private readonly IUser UserService;
        private readonly IDomain DomainService;
        private readonly IECommerce ECommerceService;
        private readonly IWebContent WebContentService;
        private readonly IWebPages WebPagesService;
        private readonly IPOI POIService;
        private readonly ITemplate TemplateService;
        private readonly IPortfolio PortfolioService;
        private readonly IGallery GalleryService;

        public AdmnController(IUser userService,
            IDomain domainService,
            IWebContent webContentService,
            IWebPages webPagesService,
            IECommerce ecommerceService,
            IPOI poiService,
            ITemplate templateService,
            IPortfolio portfolioService,
            IGallery galleryService)
            : base(domainService)
        {
            this.UserService = userService;
            this.DomainService = domainService;
            this.WebContentService = webContentService;
            this.WebPagesService = webPagesService;
            this.ECommerceService = ecommerceService;
            this.POIService = poiService;
            this.TemplateService = templateService;
            this.PortfolioService = portfolioService;
            this.GalleryService = galleryService;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            this.ViewBag.SubscriptionEnabled = this.Domain != null ? this.Domain.IsAnyCRMEnabled : false;
            this.ViewBag.DefaultWyswigContent = DomainService.GetSettingsValue(SettingsKey.adDefaultWYSIWYGContent);
            this.ViewBag.DomainCSSFile = this.Domain != null ? this.Domain.DO_CSS : "";
        }

        public ActionResult Home()
        {
            this.ViewBag.UserName = AdminUser.UserName;
            return View();
        }

        public ActionResult Menu()
        {
            return PartialView("/Views/Admn/AdminTopMenu.cshtml", GetMenuOrdered(this.AdminUser.UserGroupID));
        }

        public ActionResult LeftMenu(string url, int domainID = 0)
        {
            // TODO: implement javascirpt manager using message manager
            url = String.IsNullOrEmpty(url) ? Request.Url.AbsolutePath : url;
            tbl_AdminMenu menuItem = DomainService.GetAdminMenuItemByUrl(url);
            if (menuItem == null)
                return null;
            List<CMSMenuModel> model;

            switch (menuItem.AM_URL)
            {
                case LeftMenuUrl.AdminMenu:
                    model = LeftMenuModelMapper.MapAdminMenu(DomainService.GetAllAdminMenuItems());
                    break;
                case LeftMenuUrl.Countries:
                    model = LeftMenuModelMapper.MapCountries(GetDomains(domainID));
                    break;
                case LeftMenuUrl.EventCategories:
                    model = LeftMenuModelMapper.MapEventCategories(GetDomains(domainID), this.AdminUser);
                    break;
                case LeftMenuUrl.Forms:
                    model = LeftMenuModelMapper.MapFormItems(GetDomains(domainID), WebPagesService.GetAllForms(), WebPagesService.GetAllFormItems(), this.AdminUser);
                    break;
                case LeftMenuUrl.FormSubmission:
                    model = LeftMenuModelMapper.MapForms(GetDomains(domainID), WebPagesService.GetAllForms(), this.AdminUser);
                    break;
                case LeftMenuUrl.Discount:
                    model = LeftMenuModelMapper.MapDiscounts(ECommerceService.GetAllDiscounts(domainID));
                    break;
                case LeftMenuUrl.Domains:
                    model = LeftMenuModelMapper.MapDomains(DomainService.GetAllDomains());
                    break;
                case LeftMenuUrl.DonationInfo:
                    model = LeftMenuModelMapper.MapDonationInfo(GetDomains(domainID), this.AdminUser);
                    break;
                case LeftMenuUrl.News:
                    model = LeftMenuModelMapper.MapNews(GetDomains(domainID), this.AdminUser);
                    break;
                case LeftMenuUrl.Postage:
                    model = LeftMenuModelMapper.MapPostage(GetDomains(domainID), this.AdminUser);
                    break;
                case LeftMenuUrl.ProdAttributes:
                    model = LeftMenuModelMapper.MapProdAttributes(ECommerceService.GetAllProdAttributes());
                    break;
                case LeftMenuUrl.ProdCategories:
                    model = LeftMenuModelMapper.MapProdCategories(GetDomains(domainID), this.AdminUser);
                    break;
                case LeftMenuUrl.Sections:
                    model = LeftMenuModelMapper.MapSections(GetDomains(domainID), this.AdminUser);
                    break;
                case LeftMenuUrl.Testimonials:
                    model = LeftMenuModelMapper.MapTestimonials(WebPagesService.GetAllTestimonials());
                    break;
                case LeftMenuUrl.Users:
                    model = LeftMenuModelMapper.MapUsers(UserService.GetAllUsers(), this.AdminUser);
                    break;
                case LeftMenuUrl.UserGroups:
                    model = LeftMenuModelMapper.MapUserGroups(UserService.GetAllUserGroupsOrdered());
                    break;
                case LeftMenuUrl.Tax:
                    model = LeftMenuModelMapper.MapTaxes(ECommerceService.GetAllTaxes());
                    break;
                case LeftMenuUrl.EventTypes:
                    model = LeftMenuModelMapper.MapEventTypes(ECommerceService.GetAllEventTypes());
                    break;
                case LeftMenuUrl.POICategories:
                    model = LeftMenuModelMapper.MapPOICategories(POIService.GetAllPOICategories());
                    break;
                case LeftMenuUrl.POITags:
                    model = LeftMenuModelMapper.MapPOITags(POIService.GetAllPOITagsGroups());
                    break;
                case LeftMenuUrl.POITagsGroups:
                    model = LeftMenuModelMapper.MapPOITagsGroups(POIService.GetAllPOITagsGroups());
                    break;
                case LeftMenuUrl.POIs:
                    model = LeftMenuModelMapper.MapPOIs(POIService.GetAllPOIs());
                    break;
                case LeftMenuUrl.Templates:
                    model = LeftMenuModelMapper.MapTemplates(TemplateService.GetAll());
                    break;
                case LeftMenuUrl.Portfolio:
                    model = LeftMenuModelMapper.MapPortfolioItems(PortfolioService.GetAll());
                    break;
                case LeftMenuUrl.Gallery:
                    model = LeftMenuModelMapper.MapGalleryItems(GalleryService.GetAll());
                    break;
                default:
                    model = new List<CMSMenuModel>();
                    break;
            }
            return PartialView(model);
        }

        #region Login

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Home", "Admn");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var admin = UserService.GetUserByEmailAndPassword(loginModel.Email, loginModel.Password);
                if (admin != null)
                {
                    var principal = new CustomPrincipalSerializeModel()
                    {
                        Email = admin.US_Email,
                        UserID = admin.AdminUserID,
                        UserName = admin.US_UserName,
                        IsAdmn = true
                    };
                    var principalString = new JavaScriptSerializer().Serialize(principal);

                    var authTicket = new FormsAuthenticationTicket(1, admin.US_UserName, DateTime.Now, DateTime.Now.AddDays(SettingsManager.CookieExpireTime), true, principalString, FormsAuthentication.FormsCookiePath);
                    var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(cookie);

                    if (String.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Home", "Admn");
                    return Redirect(returnUrl);
                }
                else
                    ModelState.AddModelError("", "Email or Password is not valid.");
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgottenPassword()
        {
            this.ViewBag.MailSent = false;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgottenPassword(string Email)
        {
            var user = UserService.GetUserByEmail(Email);
            if (user != null)
            {
                var newPassword = Password.Create(SettingsManager.PassLength);
                user = UserService.SaveUser(user.US_Email, user.US_UserName, newPassword, user.US_UserGroupID, user.AdminUserID);
                MailingService.SendForgottenPassword(user.US_UserName, newPassword, user.US_Email, HttpContext.Request.Url.Host, "/Admn/");
                this.ViewBag.MailSent = true;
            }
            else
            {
                this.ViewBag.MailSent = false;
                ModelState.AddModelError("", "Email address not found. Please check you have entered it correctly and try again.");
            }

            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Home", "Admn");
        }

        #endregion

        #region Domains

        [AccessRightsAuthorizeAttribute]
        public ActionResult Domains()
        {
            return View();
        }

        public ActionResult DomainSettings(int domainID = 0)
        {
            tbl_Domains domain = DomainService.GetDomainByID(domainID);

            this.ViewBag.Pages = (domain != null) ?
                WebContentService.GetSitemapListByContent(domain.DomainID, domain.DO_HomePageID) :
                new List<ExtendedSelectListItem>();
            this.ViewBag.EventViewTypes = Enum.GetValues(typeof(EventViewType)).Cast<EventViewType>()
                .Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();

            this.ViewBag.Themes = new SelectList(Directory.GetDirectories(Server.MapPath("/Themes")).Select(d => new { dir = d.Split('\\').Last() }), "dir", "dir");

            if (domain == null)
            {

                var list = DomainService.GetDefaultSettingsValuesList().Select(x => new SettingsValueModel()
                {
                    SV_DomainID = 0,
                    SV_Value = x.SE_Value,
                    SV_SettingsID = x.SettingID,
                    SettingsValueID = 0,
                    Settings = new SettingsModel()
                    {
                        SE_Category = x.SE_Category,
                        SE_Type = x.SE_Type,
                        SE_Variable = x.SE_Variable,
                        SE_VariableLabel = x.SE_VariableLabel,
                        SE_Description = x.SE_Description
                    }
                }).ToList();

                var socialList = DomainService.GetDefaultSocialValues(0).Select(s => new SocialModel()
                {
                    BackColour = s.S_BackColour,
                    ForeColour = s.S_ForeColour,
                    DefaultBackColour = s.S_DefaultBackColour,
                    DefaultForeColour = s.S_DefaultForeColour,
                    Title = s.S_Title,
                    Live = false,
                    IconClass = s.S_IconClass,
                    URL = s.S_URL,
                    BorderRadius = s.S_BorderRadius,
                    DomainID = s.S_DomainID
                }).ToList();

                return PartialView(new DomainModel()
                {
                    IsPaypalPayment = true,
                    IsSagePayPayment = true,
                    SettingsValues = list,
                    EnableEventSale = true,
                    EnableProductSale = true,
                    DO_EnableMailChimp = false,
                    DO_EnableCommuniGator = false,
                    EventViewType = (int)EventViewType.Calendar
                });
            }
            else
            {
                var list = domain.tbl_SettingsValue.Select(x => new SettingsValueModel()
                {
                    SV_DomainID = domain.DomainID,
                    SV_Value = x.SV_Value,
                    SV_SettingsID = x.SV_SettingsID,
                    SettingsValueID = x.SettingsValueID,
                    Settings = new SettingsModel()
                    {
                        SE_Category = x.tbl_Settings.SE_Category,
                        SE_Type = x.tbl_Settings.SE_Type,
                        SE_Variable = x.tbl_Settings.SE_Variable,
                        SE_VariableLabel = x.tbl_Settings.SE_VariableLabel,
                        SE_Description = x.tbl_Settings.SE_Description
                    }
                }).ToList();

                var socialList = domain.tbl_Social.Select(x => new SocialModel()
                {
                    SocialID = x.SocialID,
                    BackColour = x.S_BackColour,
                    ForeColour = x.S_ForeColour,
                    DefaultBackColour = x.S_DefaultBackColour,
                    DefaultForeColour = x.S_DefaultForeColour,
                    DefaultBorderRadius = x.S_DefaultBorderRadius,
                    Title = x.S_Title,
                    Live = x.S_Live,
                    IconClass = x.S_IconClass,
                    URL = x.S_URL,
                    DomainID = x.S_DomainID
                }).ToList();

                List<tbl_PaymentDomain> payments = ECommerceService.GetAllPaymentsDomain(domain.DomainID);

                var result = new DomainModel()
                {
                    DomainID = domain.DomainID,
                    DO_CompanyAddress = domain.DO_CompanyAddress,
                    DO_CompanyName = domain.DO_CompanyName,
                    DO_CompanyTelephone = domain.DO_CompanyTelephone,
                    DO_ConsumerKey = domain.DO_ConsumerKey,
                    DO_ConsumerSecret = domain.DO_ConsumerSecret,
                    DO_CSS = domain.DO_CSS,
                    DO_DefaultLangID = domain.DO_DefaultLangID,
                    DO_Domain = domain.DO_Domain,
                    DO_Email = domain.DO_Email,
                    DO_GoogleAnalytics = domain.DO_GoogleAnalytics,
                    DO_GoogleAnalyticsCode = domain.DO_GoogleAnalyticsCode,
                    DO_GoogleAnalyticsVisible = domain.DO_GoogleAnalyticsVisible,
                    DO_Robots = domain.DO_Robots,
                    DO_HomePageID = domain.DO_HomePageID,
                    DO_LaunchYear = domain.DO_LaunchYear,
                    DO_ShareThis = domain.DO_ShareThis,
                    DO_TwitterSecret = domain.DO_TwitterSecret,
                    DO_TwitterToken = domain.DO_TwitterToken,
                    DO_UpdateTwitter = domain.DO_UpdateTwitter,
                    DO_Theme = domain.DO_Theme,
                    DO_EnableMailChimp = domain.DO_EnableMailChimp,
                    DO_MailChimpAPIKey = domain.DO_MailChimpAPIKey,
                    DO_MailChimpListID = domain.DO_MailChimpListID,
                    DO_EnableCommuniGator = domain.DO_EnableCommuniGator,
                    DO_CommuniGatorPassword = domain.DO_CommuniGatorPassword,
                    DO_CommuniGatorUserName = domain.DO_CommuniGatorUserName,
                    EventViewType = domain.DO_DefaultEventView,
                    EnableEventSale = domain.DO_EnableEventSale,
                    EnableProductSale = domain.DO_EnableProductSale,
                    IsCookieConsentEnabled = domain.DO_IsCookieConsentEnabled,
                    DO_CustomRouteHandler = domain.DO_CustomRouteHandler,
                    
                    IsPaypalPayment =
                        payments.Where(m => m.tbl_PaymentType.PT_Code == PaymentType.PayPal.ToString())
                            .Select(m => m.PD_Live)
                            .FirstOrDefault(),
                    IsSagePayPayment =
                        payments.Where(m => m.tbl_PaymentType.PT_Code == PaymentType.SagePay.ToString())
                            .Select(m => m.PD_Live)
                            .FirstOrDefault(),
                    IsSecureTradingPayment =
                        payments.Where(m => m.tbl_PaymentType.PT_Code == PaymentType.SecureTrading.ToString())
                            .Select(m => m.PD_Live)
                            .FirstOrDefault(),
                    IsStripePayment =
                        payments.Where(m => m.tbl_PaymentType.PT_Code == PaymentType.Stripe.ToString())
                            .Select(m => m.PD_Live)
                            .FirstOrDefault(),

                    SettingsValues = list,
                    Social = socialList
                };

                return PartialView(result);

            }
        }

        [HttpPost]
        public ActionResult AddDomain(DomainModel domainSettings)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });

            var val = domainSettings.SettingsValues.Select(x => new tbl_SettingsValue()
            {
                SV_DomainID = x.SV_DomainID,
                SV_Value = x.SV_Value,
                SV_SettingsID = x.SV_SettingsID,
                SettingsValueID = x.SettingsValueID
            }).ToList();

            EventViewType eventView;
            bool parsed = Enum.TryParse<EventViewType>(domainSettings.EventViewType.ToString(), true, out eventView);

            tbl_Domains domain = DomainService.SaveDomain(domainSettings.DO_CompanyAddress, domainSettings.DO_CompanyName, domainSettings.DO_CompanyTelephone, domainSettings.DO_ConsumerKey,
                    domainSettings.DO_ConsumerSecret, domainSettings.DO_CSS, String.Empty, String.Empty, domainSettings.DO_DefaultLangID.GetValueOrDefault(0), String.Empty, String.Empty,
                    domainSettings.DO_Domain, domainSettings.DO_Email, domainSettings.DO_GoogleAnalytics, domainSettings.DO_GoogleAnalyticsCode, domainSettings.DO_GoogleAnalyticsVisible, domainSettings.DO_Robots, String.Empty,
                    domainSettings.DO_HomePageID, domainSettings.DO_LaunchYear, true, domainSettings.DO_ShareThis, domainSettings.DO_TwitterSecret, domainSettings.DO_TwitterToken, String.Empty,
                    domainSettings.DO_UpdateTwitter, domainSettings.DO_EnableMailChimp, domainSettings.DO_MailChimpAPIKey, domainSettings.DO_MailChimpListID, domainSettings.DO_EnableCommuniGator, 
                    domainSettings.DO_CommuniGatorUserName, domainSettings.DO_CommuniGatorPassword, val, domainSettings.IsPaypalPayment, domainSettings.IsSagePayPayment, domainSettings.IsSecureTradingPayment,
                    parsed ? eventView : EventViewType.Calendar, domainSettings.EnableEventSale, domainSettings.EnableProductSale, domainSettings.DO_Theme, domainSettings.DO_DevelopmentMode, 0, false,
                    domainSettings.IsStripePayment, null, domainSettings.DO_CustomRouteHandler);

            if (domain != null)
                BundleConfig.LoadBundles();

            return Json(new { success = domain != null, domainID = domain != null ? domain.DomainID : 0 });
        }

        [HttpPost]
        public ActionResult UpdateDomain(DomainModel domainSettings)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });
            if (domainSettings.DomainID == 0)
                return null;
            var val = domainSettings.SettingsValues.Select(x => new tbl_SettingsValue()
            {
                SV_DomainID = x.SV_DomainID,
                SV_Value = x.SV_Value,
                SV_SettingsID = x.SV_SettingsID,
                SettingsValueID = x.SettingsValueID
            }).ToList();

            var socials = domainSettings.Social != null ? domainSettings.Social.Select(x => new tbl_Social()
            {
                SocialID = x.SocialID,
                S_BackColour = x.BackColour,
                S_ForeColour = x.ForeColour,
                S_Live = x.Live,
                S_DefaultBackColour = x.DefaultBackColour,
                S_DefaultForeColour = x.DefaultForeColour,
                S_DefaultBorderRadius = x.DefaultBorderRadius,
                S_Title = x.Title,
                S_DomainID = domainSettings.DomainID,
                S_IconClass = x.IconClass,
                S_URL = x.URL ?? String.Empty,
                S_BorderRadius = x.BorderRadius
            }).ToList() : new List<tbl_Social>();

            EventViewType eventView;
            bool parsed = Enum.TryParse<EventViewType>(domainSettings.EventViewType.ToString(), true, out eventView);

            tbl_Domains domain = DomainService.SaveDomain(domainSettings.DO_CompanyAddress, domainSettings.DO_CompanyName, domainSettings.DO_CompanyTelephone, domainSettings.DO_ConsumerKey,
                    domainSettings.DO_ConsumerSecret, domainSettings.DO_CSS, String.Empty, String.Empty, domainSettings.DO_DefaultLangID.GetValueOrDefault(0), String.Empty, String.Empty,
                    domainSettings.DO_Domain, domainSettings.DO_Email, domainSettings.DO_GoogleAnalytics, domainSettings.DO_GoogleAnalyticsCode, domainSettings.DO_GoogleAnalyticsVisible, domainSettings.DO_Robots, String.Empty,
                    domainSettings.DO_HomePageID, domainSettings.DO_LaunchYear, true, domainSettings.DO_ShareThis, domainSettings.DO_TwitterSecret, domainSettings.DO_TwitterToken, String.Empty,
                    domainSettings.DO_UpdateTwitter, domainSettings.DO_EnableMailChimp, domainSettings.DO_MailChimpAPIKey, domainSettings.DO_MailChimpListID, domainSettings.DO_EnableCommuniGator,
                    domainSettings.DO_CommuniGatorUserName, domainSettings.DO_CommuniGatorPassword, val, domainSettings.IsPaypalPayment, domainSettings.IsSagePayPayment, domainSettings.IsSecureTradingPayment,
                    parsed ? eventView : EventViewType.Calendar, domainSettings.EnableEventSale, domainSettings.EnableProductSale, domainSettings.DO_Theme, domainSettings.DO_DevelopmentMode, domainSettings.DomainID, 
                    domainSettings.IsCookieConsentEnabled, domainSettings.IsStripePayment, socials, domainSettings.DO_CustomRouteHandler);

            if (domain != null)
                BundleConfig.LoadBundles();

            return Json(new { success = domain != null, domainID = domain != null ? domain.DomainID : 0 });
        }

        [HttpPost]
        public ActionResult DeleteDomain(int domainID, bool forced = false)
        {
            bool canDelete = DomainService.CanDeleteDomain(domainID);
            if (canDelete || forced)
                return Json(new { success = DomainService.DeleteDomain(domainID) });
            else if (!canDelete && !forced)
                return Json(new { success = false, warning = true, error = "This domain is already linked to other parts of the system.", domainID = domainID });

            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult GetDomainsList(int? selectedDomainID, bool currentDomain = false)
        {
            if (currentDomain == true)
                selectedDomainID = this.DomainID;
            var domains = DomainService.GetAllDomainsAsSelectList(selectedDomainID.GetValueOrDefault(0));
            return Json(new { success = true, domains = domains, selected = domains.Where(d => d.Selected).Select(d => d.Value).FirstOrDefault() });
        }

        [HttpPost]
        public JsonResult GetOrderTypesList(int typeId = 1)
        {
            var types = Enum.GetValues(typeof(OrderType)).Cast<OrderType>().Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString(), Selected = (int)c == typeId }).OrderBy(l => l.Value).ToList();
            return Json(new { success = true, types = types, selected= types.FirstOrDefault(x=> x.Selected).Value });
        }

        [HttpPost]
        public ActionResult GetSitemapListByDomain(int domainID, int sitemapID = 0)
        {
            var sections = WebContentService.GetSitemapListByContent(domainID, sitemapID);
            foreach (var section in sections)
            {
                int level = section.Level;
                while (--level > 0)
                    section.Text = "\xA0\xA0" + section.Text;
            }
            return Json(new { success = sections != null, sections, sitemapID });
        }

        [HttpPost]
        public JsonResult GetSelectSettingsOptions(int settingID, int domainID)
        {
            var setting = DomainService.GetSettingsByID(settingID);
            if (setting == null)
                return Json(new { success = false });

            SelectList options = null;
            if (setting.SE_Variable == SettingsKey.cookieConsentSinglePage.ToString())
            {
                var list = WebContentService.GetSitemapListURLByContent(domainID, setting.tbl_SettingsValue.FirstOrDefault(s => s.SV_DomainID == domainID).SV_Value);
                options = new SelectList(list, "Value", "Text");
                return Json(new { success = true, options = options });
            }

            var settingsList = DomainService.GetSettingsOptions(settingID);
            if (settingsList == null)
                return Json(new { success = false });
            options = new SelectList(settingsList, "SO_Value", "SO_Name");
            return Json(new { success = true, options = options });
        }

        public ActionResult DomainLinks(int domainID)
        {
            this.ViewBag.Domain = DomainService.GetDomainByID(domainID);
            return PartialView(new DomainLinkModel { DomainID = domainID });
        }

        [HttpPost]
        public ActionResult AddDomainLink(DomainLinkModel link)
        {
            if (!ModelState.IsValid)
                return null;
            var dbDomainLink = DomainService.SaveDomainLink(link.DomainID, link.DomainLink);
            return (dbDomainLink != null) ?
                RedirectToAction("DomainLinks", new { domainID = dbDomainLink.DL_DomainID }) :
                null;
        }

        [HttpPost]
        public ActionResult DeleteDomainLinks(int[] domainLinksIDs)
        {
            var domainID = DomainService.DeleteDomainLinks(domainLinksIDs);
            return (domainID > 0) ?
                RedirectToAction("DomainLinks", new { domainID = domainID }) :
                null;
        }

        public ActionResult PaymentLogosConfig(int domainID)
        {
            var model = ECommerceService.GetAllPaymentsDomain(domainID).Select(m => (new PaymentLogoModel()
            {
                Description = m.tbl_PaymentType.PT_Description,
                Name = m.tbl_PaymentType.PT_Name,
                PaymentDomainID = m.PaymentDomainID,
                DomainID = m.PD_DomainID,
                Code = m.tbl_PaymentType.PT_Code,
                FilePath = (m.PD_Logo != null) ?
                    String.Format("/{0}/{1}/{2}", SettingsManager.Payment.PaymentLogosPath.Trim('/'), domainID, m.PD_Logo) :
                    ""
            })).ToList();
            return PartialView(model);
        }

        #endregion

        #region Donations

        [AccessRightsAuthorizeAttribute]
        public ActionResult Donations()
        {
            return View();
        }

        public ActionResult DonationLeftMenu(string search, string startDate, string endDate, int domainID = 0, int statusID = 0)
        {
            List<tbl_Orders> orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, null, null, ProductType.Donation);
            List<CMSMenuModel> model = LeftMenuModelMapper.MapDonations(orders);
            return PartialView("LeftMenu", model);
        }

        public ActionResult DonationDetails(int orderID)
        {
            this.ViewBag.IsPopup = false;
            this.ViewBag.Methodes = ECommerceService.GetDespatchMethodes();
            return PartialView(ECommerceService.GetOrderByID(orderID));
        }

        #endregion Donations

        #region DonationsInfo

        [HttpPost]
        public JsonResult SaveDonationInfo(DonationInfoModel donationInfo)
        {
            if (ModelState.IsValid)
            {
                var val = ECommerceService.SaveDonationInfo(new tbl_DonationInfo()
                {
                    DI_Amount = donationInfo.DI_Amount,
                    DI_Title = donationInfo.DI_Title,
                    DI_DomainID = donationInfo.DI_DomainID,
                    DI_Description = donationInfo.DI_Description,
                    DonationInfoID = donationInfo.DonationInfoID,
                    DI_DonationTypeID = donationInfo.DI_DonationTypeID,
                    DI_Live = donationInfo.DI_Live
                });
                if (val != null)
                    return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult DeleteDonationInfo(int donationInfoID)
        {
            return Json(new { success = ECommerceService.DeleteDonationInfo(donationInfoID) });
        }

        [HttpPost]
        public ActionResult DeleteDonationInfoImage(int donationInfoID)
        {
            var donationInfo = ECommerceService.GetDonationInfoByID(donationInfoID);
            if (donationInfo == null)
                return Json(new { success = false });

            DeleteFile(donationInfo.DI_ImagePath);
            ECommerceService.UpdateDonationInfoImagePath(String.Empty, donationInfoID);
            return Json(new { success = true });
        }

        public ActionResult DonationInfoSettings(int donationInfoID = 0)
        {
            ViewBag.Domains = DomainService.GetAllDomainsAsSelectList(0);
            var type = ECommerceService.GetAllDonationType();
            ViewBag.DonationTypes = new SelectList(type, "DonationTypeID", "DT_Name");
            return PartialView();
        }

        public ActionResult DonationInfo()
        {
            return View();
        }

        public ActionResult DonationInfoImage(int donationInfoID)
        {
            var val = ECommerceService.GetDonationInfoByID(donationInfoID);
            return PartialView(val);
        }

        [HttpPost]
        public JsonResult GetDonationInfoData(int donationInfoID)
        {
            if (donationInfoID == 0)
                return Json(new { success = false });
            var entity = ECommerceService.GetDonationInfoByID(donationInfoID);
            if (entity == null)
                return Json(new { success = false });
            var model = new DonationInfoModel()
            {
                DI_Amount = entity.DI_Amount,
                DI_Title = entity.DI_Title,
                DI_Description = entity.DI_Description,
                DI_DomainID = entity.DI_DomainID,
                DI_DonationTypeID = entity.DI_DonationTypeID,
                DI_Live = entity.DI_Live,
                DonationInfoID = entity.DonationInfoID,
            };
            return Json(new { success = true, donation = model });
        }

        [HttpPost]
        public JsonResult SaveDonationInfoImage(HttpPostedFileBase file, int donationInfoID)
        {
            if (file != null)
            {
                var donationInfo = ECommerceService.GetDonationInfoByID(donationInfoID);
                if (donationInfo == null)
                    return Json(new { success = false });

                DeleteFile(donationInfo.DI_ImagePath);

                var path = String.Format("{0}{1}{2}", SettingsManager.DonationInfo.Path, donationInfoID, Path.GetFileName(file.FileName));
                donationInfo = ECommerceService.UpdateDonationInfoImagePath(path, donationInfoID);
                if (donationInfo != null)
                {
                    SaveImageFile(file, path);
                    return Json(new { success = true }, "text/html");
                }
            }
            return Json(new { success = false }, "text/html");
        }

        #endregion DonationsInfo

        #region Users

        [AccessRightsAuthorizeAttribute]
        public ActionResult Users()
        {
            this.ViewBag.UserGroups = new SelectList(UserService.GetAllUserGroupsOrdered(), "UserGroupID", "UG_Type");
            return View();
        }

        [HttpPost]
        public ActionResult GetUser(int userID)
        {
            tbl_AdminUsers user = UserService.GetUserByID(userID);
            return (user == null) ?
                null :
                Json(new { UserID = user.AdminUserID, UserName = user.US_UserName, Email = user.US_Email, UserGroupID = user.US_UserGroupID });
        }

        [HttpPost]
        public ActionResult GetPassword()
        {
            return Json(new { password = Password.Create(SettingsManager.PassLength) });
        }

        [HttpPost]
        [DeleteUserAuthorize]
        public ActionResult DeleteUser(int userID)
        {
            return Json(new { success = UserService.DeleteUser(userID) });
        }

        [HttpPost]
        [AddUserAuthorize]
        public ActionResult AddUser(UserModel userModel)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });
            if (!UserService.CanUseUserEmail(userModel.Email, userModel.UserID))
                return Json(new { success = false, error = "This email is already used by another user." });

            var user = UserService.SaveUser(userModel.Email, userModel.UserName, userModel.Password, userModel.UserGroupID, 0);
            return Json((user != null) ?
                    new { success = true, userID = user.AdminUserID } :
                    new { success = false, userID = 0 }
                );
        }

        [HttpPost]
        [EditUserAuthorize]
        public ActionResult UpdateUser(UserModel userModel)
        {
            if (ModelState.IsValid && userModel.UserID > 0)
            {
                if (UserService.CanUseUserEmail(userModel.Email, userModel.UserID))
                {
                    var user = UserService.SaveUser(userModel.Email, userModel.UserName, userModel.Password, userModel.UserGroupID, userModel.UserID);
                    return Json((user != null) ?
                            new { success = true, userID = user.AdminUserID } :
                            new { success = false, userID = 0 }
                         );
                }
                return Json(new { success = false, error = "This email is already used by another user." });
            }
            return Json(new { success = false });
        }

        #endregion

        #region User Groups

        [AccessRightsAuthorizeAttribute]
        public ActionResult UserGroups()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetUserGroup(int groupID)
        {
            tbl_UserGroups userGroup = UserService.GetUserGroupByID(groupID);
            return Json((userGroup == null) ?
                new
                {
                    GroupID = 0,
                    GroupName = String.Empty,
                    MenuItems = DomainService.GetAdminMenuOrdered(0),
                    Permissions = DomainService.GetPermissions(0)
                } :
                new
                {
                    GroupID = userGroup.UserGroupID,
                    GroupName = userGroup.UG_Type,
                    MenuItems = DomainService.GetAdminMenuOrdered(userGroup.UserGroupID),
                    Permissions = DomainService.GetPermissions(userGroup.UserGroupID)
                });
        }

        [HttpPost]
        public JsonResult DeleteUserGroup(int groupID, bool force = false)
        {
            int usersAmount = UserService.GetUsersAmountForUserGroup(groupID);
            return (!force && usersAmount > 0) ?
                    Json(new { error = true, users = usersAmount, groupID = groupID }) :
                    Json(new { success = UserService.DeleteUserGroup(groupID) }
                );
        }

        [HttpPost]
        public ActionResult AddUserGroup(UserGroupModel model, int[] menuItems, int[] permissions)
        {
            tbl_UserGroups userGroup = UserService.SaveUserGroup(model.GroupName, menuItems, permissions, 0);
            return Json((userGroup != null) ?
                    new { success = true, userGroupID = userGroup.UserGroupID } :
                    new { success = false, userGroupID = 0 }
                );
        }

        [HttpPost]
        public ActionResult UpdateUserGroup(UserGroupModel model, int[] menuItems, int[] permissions)
        {
            if (model.UserGroupID == 0)
                return Json(new { success = false });

            tbl_UserGroups userGroup = UserService.SaveUserGroup(model.GroupName, menuItems, permissions, model.UserGroupID);
            return Json((userGroup != null) ?
                    new { success = true, userGroupID = userGroup.UserGroupID } :
                    new { success = false, userGroupID = 0 }
                );
        }

        #endregion

        #region Admin Menu a.k.a. "Modules"

        [AccessRightsAuthorizeAttribute]
        public ActionResult AdminMenu()
        {
            this.ViewBag.AdminMenu = DomainService.GetAdminMenuOrdered(0);
            return View();
        }

        [HttpPost]
        public ActionResult GetMenuItems()
        {
            var menuItems = DomainService.GetAdminMenuOrdered(0);
            foreach (var menuItem in menuItems)
            {
                int level = menuItem.Level;
                while (level-- > 0)
                    menuItem.Text = "\xA0\xA0" + menuItem.Text;
            }
            menuItems.Insert(0, new ExtendedSelectListItem { Value = "0", Text = "...", Selected = false });
            return Json(new { menuItems });
        }

        [HttpPost]
        public ActionResult GetMenuItem(int menuItemID)
        {
            var menuItem = DomainService.GetAdminMenuItemByID(menuItemID);
            return (menuItem != null) ?
                Json(new AdminMenuItemModel
                {
                    AdminMenuID = menuItem.AdminMenuID,
                    MenuText = menuItem.AM_MenuText,
                    URL = menuItem.AM_URL,
                    ParentID = menuItem.AM_ParentID
                }) :
                null;
        }

        [HttpPost]
        public ActionResult DeleteMenuItem(int menuItemID)
        {
            return Json(new { success = DomainService.DeleteAdminMenuItem(menuItemID) });
        }

        [HttpPost]
        public ActionResult ToggleVisibility(int menuItemID)
        {
            return Json(new { success = DomainService.SaveAdminMenuItemVisibility(menuItemID) != null });
        }

        [HttpPost]
        public ActionResult SaveOrder(int[] orderedMenuItemIDs)
        {
            return Json(new { success = DomainService.SaveAdminMenuItemsOrder(orderedMenuItemIDs) });
        }

        [HttpPost]
        public ActionResult AddMenuItem(AdminMenuItemModel itemModel)
        {
            tbl_AdminMenu menuItem = DomainService.SaveAdminMenuItem(itemModel.MenuText, itemModel.ParentID, itemModel.URL, 0);
            return Json(new { success = menuItem != null, adminMenuID = menuItem != null ? menuItem.AdminMenuID : 0 });
        }

        [HttpPost]
        public ActionResult UpdateMenuItem(AdminMenuItemModel itemModel)
        {
            if (itemModel.AdminMenuID == 0)
                return Json(new { success = false });

            tbl_AdminMenu menuItem = DomainService.SaveAdminMenuItem(itemModel.MenuText, itemModel.ParentID, itemModel.URL, itemModel.AdminMenuID);
            return Json(new { success = menuItem != null, adminMenuID = menuItem != null ? menuItem.AdminMenuID : 0 });
        }

        #endregion

        #region Page Content

        [AccessRightsAuthorizeAttribute]
        public ActionResult Sections(int sectionID = 0)
        {
            this.ViewBag.SectionID = sectionID;
            return View();
        }

        public ActionResult SectionSettings(int sectionID, int contentID = 0)
        {
            return PartialView(WebContentService.GetContentBySitemapID(sectionID, contentID));
        }

        public ActionResult PageDetails(int contentID)
        {
            tbl_Content content = WebContentService.GetContentByID(contentID);
            int customLayoutId = 0;

            if (content != null && content.tbl_SiteMap.SM_CustomLayoutID != null)
            {
                customLayoutId = (int)content.tbl_SiteMap.SM_CustomLayoutID;
            }

            this.ViewBag.CustomLayouts = WebPagesService.GetAllCustomlayoutsAsSelectlist(customLayoutId);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null ? content.tbl_SiteMap.SM_DomainID : 0));

            return PartialView((content == null) ?
                new PageDetailsModel
                {
                    Footer = false,
                    Menu = false,
                    TopLevel = true,
                    IsHomePage = false
                } :
                new PageDetailsModel
                {
                    DomainID = content.tbl_SiteMap.SM_DomainID,
                    Footer = content.tbl_SiteMap.SM_Footer,
                    Menu = content.tbl_SiteMap.SM_Menu,
                    MenuText = content.C_MenuText,
                    Name = content.tbl_SiteMap.SM_Name,
                    TopLevel = content.tbl_SiteMap.SM_ParentID == 0,
                    ParentID = content.tbl_SiteMap.SM_ParentID,
                    Path = content.tbl_SiteMap.SM_Path,
                    ContentID = content.ContentID,
                    SiteMapID = content.tbl_SiteMap.SiteMapID,
                    IsHomePage = content.C_TableLinkID == this.Domain.DO_HomePageID,
                    CustomLayout = content.tbl_SiteMap.SM_CustomLayoutID == null ? 0 : (int)content.tbl_SiteMap.SM_CustomLayoutID
                });
        }

        public ActionResult SEOFields(int contentID)
        {
            tbl_Content content = WebContentService.GetContentByID(contentID);

            this.ViewBag.Priorities = new List<SelectListItem> { 
                new SelectListItem { Value = "0.1", Text = "0.1" },
                new SelectListItem { Value = "0.2", Text = "0.2" },
                new SelectListItem { Value = "0.3", Text = "0.3" },
                new SelectListItem { Value = "0.4", Text = "0.4" },
                new SelectListItem { Value = "0.5", Text = "0.5", Selected = true },
                new SelectListItem { Value = "0.6", Text = "0.6" },
                new SelectListItem { Value = "0.7", Text = "0.7" },
                new SelectListItem { Value = "0.8", Text = "0.8" },
                new SelectListItem { Value = "0.9", Text = "0.9" },
                new SelectListItem { Value = "1.0", Text = "1.0"},
            };

            return PartialView((content == null) ?
                new SEOFieldsModel
                {
                    IsPageContent = true,
                    SiteMap = false
                } :
                new SEOFieldsModel
                {
                    Title = content.C_Title,
                    Desc = content.C_Description,
                    Keywords = content.C_Keywords,
                    MetaData = content.C_MetaData,
                    R301 = content.tbl_SiteMap.SM_301,
                    SiteMap = content.tbl_SiteMap.SM_Sitemap,
                    Priority = content.tbl_SiteMap.SM_Priority.GetValueOrDefault((decimal)0.1).ToString("F1"),
                    IsPageContent = true
                });
        }

        [HttpPost]
        [ValidateInput(false)]
        [AddContentAuthorize]
        public ActionResult AddSection(PageDetailsModel detailsModel, SEOFieldsModel seoModel, string content)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });

            tbl_SiteMap parent = WebContentService.GetSitemapByID(detailsModel.ParentID);
            string url = (parent != null ? parent.SM_URL : String.Empty) + "/" + detailsModel.Path.Trim('/');
            if (WebContentService.CheckSitemapUniqueUrl(url, detailsModel.SiteMapID, detailsModel.DomainID))
                return Json(new { success = false, error = "Please change 'Page URL'. There is another page with the same URL already registered." });

            tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, detailsModel.DomainID, String.Empty, detailsModel.Menu,
                detailsModel.Footer, detailsModel.MenuText, null, seoModel.Priority, string.Empty, detailsModel.Path, true, seoModel.SiteMap,
                ContentType.Content, detailsModel.Name, detailsModel.TopLevel ? 0 : detailsModel.ParentID, 0);

            if (section != null && section.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID))
                WebContentService.UpdateSitemapsParents(section.SiteMapID, ProductType.Item);
            else if (section != null && section.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID))
                WebContentService.UpdateSitemapsParents(section.SiteMapID, ProductType.Event);

            tbl_Content tContent = (section != null) ?
                WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, detailsModel.MenuText,
                    seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, string.Empty, false, section.SiteMapID, 0) : null;

            return Json(new
            {
                success = section != null && tContent != null,
                sectionID = (section != null) ? section.SiteMapID : 0,
                contentID = (tContent != null) ? tContent.ContentID : 0
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        [EditContentAuthorize]
        public ActionResult UpdateSection(PageDetailsModel detailsModel, SEOFieldsModel seoModel, string content)
        {
            if (ModelState.IsValid && detailsModel.SiteMapID > 0)
            {
                tbl_SiteMap parent = WebContentService.GetSitemapByID(detailsModel.ParentID);
                string url = (parent != null ? parent.SM_URL : String.Empty) + "/" + detailsModel.Path.Trim('/');
                if (WebContentService.CheckSitemapUniqueUrl(url, detailsModel.SiteMapID, detailsModel.DomainID))
                    return Json(new { success = false, error = "Please change 'Page URL'. There is another page with the same URL already registered." });

                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, detailsModel.DomainID, String.Empty, detailsModel.Menu,
                    detailsModel.Footer, detailsModel.MenuText, null, seoModel.Priority, string.Empty, detailsModel.Path, true, seoModel.SiteMap,
                    ContentType.Content, detailsModel.Name, detailsModel.TopLevel ? 0 : detailsModel.ParentID, detailsModel.SiteMapID);

                if (section != null && section.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID))
                    WebContentService.UpdateSitemapsParents(section.SiteMapID, ProductType.Item);
                else if (section != null && section.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID))
                    WebContentService.UpdateSitemapsParents(section.SiteMapID, ProductType.Event);

                if (section.SM_IsPredefined)
                    RouteConfig.RegisterRoutes(RouteTable.Routes);

                tbl_Content tContent = section != null ?
                    WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, detailsModel.MenuText,
                        seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, string.Empty, false, section.SiteMapID, 0) : null;

               WebContentService.UpdateSiteMapCustomLayout(section.SiteMapID, detailsModel.CustomLayout);

                return Json(new
                {
                    success = section != null && tContent != null,
                    sectionID = (section != null) ? section.SiteMapID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult SaveSectionOrder(int[] orderedMenuItemIDs)
        {
            return Json(new { success = WebContentService.SaveSitemapsOrder(orderedMenuItemIDs) });
        }

        [HttpPost]
        [DeleteContentAuthorize]
        public ActionResult DeleteSection(int sectionID)
        {
            return Json(new { success = WebContentService.DeleteSection(sectionID) });
        }

        [HttpPost]
        public ActionResult GetSitemapList(int domainID, int sectionID = 0)
        {
            var sitemap = WebContentService.GetSitemapByID(sectionID);
            var sections = WebContentService.GetSitemapListByContent(domainID, sitemap != null ? sitemap.SM_ParentID : 0);
            int id = 0;
            foreach (var section in sections)
            {
                int level = section.Level;
                while (--level > 0)
                    section.Text = "\xA0\xA0" + section.Text;
                if (section.Selected)
                    id = Int32.Parse(section.Value);
            }
            sections.Insert(0, new ExtendedSelectListItem { Text = "...", Value = "0" });
            return Json(new { sections = sections, parentID = id });
        }

        public ActionResult GetParentURL(int parentID)
        {
            return Json(WebContentService.GetSitemapParentUrlByID(parentID));
        }

        public ActionResult PageContentVersion(int sectionID, int contentID = 0)
        {
            tbl_SiteMap section = WebContentService.GetSitemapByID(sectionID);
            this.ViewBag.ContentID = contentID;
            this.ViewBag.CanDeleteVersion = this.AdminUser.HasPermission(Permission.DeleteContent);
            return PartialView("ContentVersion", section);
        }

        [HttpPost]
        [DeleteContentAuthorize]
        public ActionResult DeletePageVersion(int contentID)
        {
            return Json(new { success = WebContentService.DeleteContent(contentID) });
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult ApprovePageVersion(int sectionID, int contentID = 0)
        {
            return Json(new { success = WebContentService.ApproveContent(sectionID, contentID) != null, sectionID = sectionID });
        }

        public ActionResult Preview(int sectionID, int contentID = 0)
        {
            return RedirectToRoute("Preview", new { sectionID = sectionID, contentID = contentID });
        }

        public ActionResult SectionImages(int sectionID)
        {
            ViewBag.SitemapList = WebContentService.GetSitemapByDomainID(this.DomainID).OrderBy(m => m.SM_OrderBy).Select(m => new SelectListItem
                    {
                        Text = m.SM_Name,
                        Value = m.SiteMapID.ToString(),
                        Selected = false
                    });
            var siteMap = WebContentService.GetSitemapByID(sectionID);
            this.ViewBag.OnlyOneImage = false; // siteMap != null ? siteMap.IsType(ContentType.Blog) : false;
            this.ViewBag.CanUploadImages = this.AdminUser.HasPermission(Permission.EditContent);
            this.ViewBag.CanDeleteImage = this.AdminUser.HasPermission(Permission.EditContent);
            this.ViewBag.UploadAction = Url.Action("SavePageImages", "Admn");
            return PartialView("SitemapImages", siteMap);
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult SavePageImageCaption(int imageID, string heading, string description, string sLinkID)
        {
            int parsedLinkID = 0;
            int? linkID = null;
            if (Int32.TryParse(sLinkID, out parsedLinkID))
            {
                if (parsedLinkID > 0)
                    linkID = parsedLinkID;
            }
            tbl_Image image = GalleryService.UpdateImageDescription(imageID, heading, description, linkID);
            if (image == null)
                return Json(new { success = false });

            return Json(new { success = true });
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult SavePageImages(HttpPostedFileBase file1, HttpPostedFileBase file2, HttpPostedFileBase file3, int siteMapID, string heading1, string heading2, string heading3, string description1, string description2, string description3, int? linkID1 = null, int? linkID2 = null, int? linkID3 = null)
        {
            if (linkID1 == 0)
                linkID1 = null;
            if (linkID2 == 0)
                linkID2 = null;
            if (linkID3 == 0)
                linkID3 = null;
            bool success = true;
            if (file1 != null)
                success &= SaveImage(file1, siteMapID, heading1, description1, linkID1);
            if (file2 != null)
                success &= SaveImage(file2, siteMapID, heading2, description2, linkID2);
            if (file3 != null)
                success &= SaveImage(file3, siteMapID, heading3, description3, linkID3);
            return Json(new { success = success }, "text/html");
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult DeletePageImage(int imageID)
        {
            return Json(new { success = DeleteImage(imageID) });
        }

        #endregion

        #region News

        [AccessRightsAuthorizeAttribute]
        public ActionResult News(int sectionID = 0)
        {
            this.ViewBag.SectionID = sectionID;
            return View();
        }

        public ActionResult NewsSettings(int sectionID, int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentBySitemapID(sectionID, contentID);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null ? content.tbl_SiteMap.SM_DomainID : 0));
            this.ViewBag.IsTwitterOn = this.Domain != null ? this.Domain.DO_UpdateTwitter : false;
            return PartialView(content);
        }

        public ActionResult SEONewsFields(int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentByID(contentID);
            return PartialView("SEOFields", (content == null) ?
                new SEOFieldsModel { IsPageContent = false } :
                new SEOFieldsModel
                {
                    Title = content.C_Title,
                    Desc = content.C_Description,
                    Keywords = content.C_Keywords,
                    MetaData = content.C_MetaData,
                    R301 = content.tbl_SiteMap.SM_301,
                    IsPageContent = false
                });
        }

        [HttpPost]
        [ValidateInput(false)]
        [AddNewsAuthorize]
        public ActionResult AddNews(int domainID, string blogDate, SEOFieldsModel seoFields, string content, string tweet, string blogPublishDate)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false });
            var date = blogDate.ParseDateTime();

            DateTime? publishDate = null;
            DateTime tryPublishDate;
                if(!String.IsNullOrEmpty(blogPublishDate))
                    if(DateTime.TryParse(blogPublishDate, out tryPublishDate))
                        publishDate = tryPublishDate;

            var url = String.Format("/{0}/{1}/{2}", date.Year, date.Month, seoFields.Title);
            if (WebContentService.CheckSitemapUniqueUrl(url, 0, domainID))
                return Json(new { success = false, error = "Please change 'Title'. There is another page with the same URL already registered." });

            var news = WebContentService.GetSitemapByUrl(WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID), domainID).FirstOrDefault();

            tbl_SiteMap section = WebContentService.SaveSiteMap(seoFields.R301, 1, 0, domainID, String.Empty, false, false, seoFields.Title,
                date, "0.5", String.Empty, url, true, true, ContentType.Blog, seoFields.Title, news != null ? news.SiteMapID : 0, 0,(int)SiteMapType.News, publishDate: publishDate);
            tbl_Content tContent = (section != null) ?
                WebContentService.SaveContent(String.Empty, content, seoFields.Desc, 0, String.Empty, seoFields.Keywords, seoFields.Title,
                    seoFields.MetaData, 0, String.Empty, String.Empty, String.Empty, seoFields.Title, tweet, !String.IsNullOrEmpty(tweet), section.SiteMapID, 0) :
                null;

            return Json(new
            {
                success = section != null && tContent != null,
                sectionID = (section != null) ?
                    section.SiteMapID :
                    0,
                contentID = (tContent != null) ?
                    tContent.ContentID :
                    0
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        [EditNewsAuthorize]
        public ActionResult UpdateNews(int domainID, string blogDate, SEOFieldsModel seoFields, string content, string tweet, int sitemapID, string blogPublishDate)
        {
            if (ModelState.IsValid && sitemapID != 0)
            {
                var date = blogDate.ParseDateTime();

                DateTime? publishDate = null;
                DateTime tryPublishDate;
                if(!String.IsNullOrEmpty(blogPublishDate))
                    if(DateTime.TryParse(blogPublishDate, out tryPublishDate))
                        publishDate = tryPublishDate;

                var url = String.Format("/{0}/{1}/{2}", date.Year, date.Month, seoFields.Title);
                if (WebContentService.CheckSitemapUniqueUrl(url, sitemapID, domainID))
                    return Json(new { success = false, error = "Please change 'Title'. There is another page with the same URL already registered." });

                var news = WebContentService.GetSitemapByUrl(WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID), domainID).FirstOrDefault();

                tbl_SiteMap section = WebContentService.SaveSiteMap(seoFields.R301, 1, 0, domainID, String.Empty, false, false, seoFields.Title,
                    date, "0.5", String.Empty, url, true, true, ContentType.Blog, seoFields.Title, news != null ? news.SiteMapID : 0, sitemapID, (int)SiteMapType.News, publishDate: publishDate);

                tbl_Content tContent = section != null ? WebContentService.SaveContent(String.Empty, content, seoFields.Desc, 0, String.Empty, seoFields.Keywords, seoFields.Title,
                    seoFields.MetaData, 0, String.Empty, String.Empty, String.Empty, seoFields.Title, tweet, !String.IsNullOrEmpty(tweet), section.SiteMapID, 0) : null;

                return Json(new
                {
                    success = section != null && tContent != null,
                    sectionID = (section != null) ?
                        section.SiteMapID :
                        0,
                    contentID = (tContent != null) ?
                        tContent.ContentID :
                        0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [EditNewsCategoriesAuthorizeAttribute]
        public ActionResult SaveNewsCategories(int sitemapID, int[] categoryIDs)
        {
            return Json(new { success = WebContentService.SaveCategoriesForSitemap(sitemapID, categoryIDs) });
        }

        [HttpPost]
        [DeleteNewsAuthorize]
        public ActionResult DeleteBlog(int sectionID)
        {
            return Json(new { success = WebContentService.DeleteSection(sectionID) });
        }

        [HttpPost]
        public ActionResult GetTags(int sectionID)
        {
            return Json(new { tags = WebContentService.GetTagsBySectionID(sectionID) });
        }

        [EditNewsAuthorize]
        public ActionResult AddTag(string tagName, int sectionID)
        {
            return Json(new { success = WebContentService.SaveTag(tagName, sectionID) != null });
        }

        [EditNewsAuthorize]
        public ActionResult DeleteTag(int tagID)
        {
            return Json(new { success = WebContentService.DeleteTag(tagID) });
        }

        [HttpPost]
        public ActionResult GetCategories(int sectionID)
        {
            return Json(new { categories = WebContentService.GetAllCategories(sectionID) });
        }

        [EditNewsAuthorize]
        public ActionResult AddCategory(string name, int sitemapID)
        {
            var category = WebContentService.SaveCategory(name, 0);
            return (category == null) ?
                Json(new { success = false }) :
                // new category has to be selected by default
                Json(new
                {
                    success = (sitemapID == 0) ?
                        true :
                        WebContentService.AddCategoryToSitemap(sitemapID, category.CategoryID),
                    item = new SelectListItem { Selected = true, Text = category.CA_Title, Value = category.CategoryID.ToString() }
                });
        }

        [EditNewsAuthorize]
        public ActionResult DeleteCategory(int categoryID)
        {
            return (WebContentService.BlogAmountInCategory(categoryID) > 0) ?
                Json(new { success = false, error = "Cannot delete because some blogs have been assigned against this category." }) :
                Json(new { success = WebContentService.DeleteCategory(categoryID) });
        }

        public ActionResult NewsComments(int sitemapID = 0)
        {
            return PartialView(WebContentService.GetNewsComments(sitemapID));
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult AuthoriseNewsComment(int commentID, int sitemapID)
        {
            return Json(new { success = WebContentService.AuthoriseComment(commentID) != null, commentID = commentID, sitemapID = sitemapID });
        }

        [HttpPost]
        [EditNewsAuthorize]
        public ActionResult DeleteNewsComment(int commentID, int sitemapID)
        {
            return Json(new { success = WebContentService.DeleteComment(commentID), commentID = commentID, sitemapID = sitemapID });
        }

        [HttpPost]
        [EditNewsAuthorize]
        public ActionResult ToggleNewsVisibility(int sitemapID, int contentID = 0)
        {
            bool success = false;
            tbl_SiteMap sitemap = ToogleNewsVisiblityAndTweet(sitemapID);

            if (sitemap.SM_Live && !WebContentService.IsAnyContentApproved(sitemapID))
            {
                success = (contentID > 0 && sitemap.tbl_Content.Any(c => c.ContentID == contentID)) ?
                    WebContentService.ApproveContent(sitemapID, contentID) != null :
                    WebContentService.ApproveContent(sitemapID) != null;
            }

            return Json(new { success });
        }

        public ActionResult BlogContentVersion(int sectionID, int contentID = 0)
        {
            tbl_SiteMap section = WebContentService.GetSitemapByID(sectionID);
            this.ViewBag.ContentID = contentID;
            this.ViewBag.CanDeleteVersion = this.AdminUser.HasPermission(Permission.DeleteNews);
            return PartialView("ContentVersion", section);
        }

        [HttpPost]
        [DeleteNewsAuthorize]
        public ActionResult DeleteBlogVersion(int contentID)
        {
            bool success = false;
            if (contentID > 0)
            {
                tbl_SiteMap sitemap = WebContentService.GetSitemapByContentID(contentID);

                success = WebContentService.DeleteContent(contentID);
                if (success && sitemap != null && sitemap.SM_Live && !WebContentService.IsAnyContentApproved(sitemap.SiteMapID))
                {
                    sitemap = WebContentService.SaveSitemapVisibility(sitemap.SiteMapID);
                }
            }

            return Json(new { success });
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult ApproveBlogVersion(int sectionID, int contentID)
        {
            bool success = false;
            if (contentID > 0)
            {
                tbl_SiteMap sitemap = WebContentService.GetSitemapByContentID(contentID);

                success = WebContentService.ApproveContent(sectionID, contentID) != null;
                if (success && sitemap != null && !sitemap.SM_Live)
                {
                    sitemap = ToogleNewsVisiblityAndTweet(sitemap.SiteMapID);
                }
            }

            return Json(new { success });
        }

        public ActionResult BlogImages(int sectionID)
        {
            ViewBag.SitemapList = WebContentService.GetSitemapByDomainID(this.DomainID).OrderBy(m => m.SM_OrderBy).Select(m => new SelectListItem
            {
                Text = m.SM_Name,
                Value = m.SiteMapID.ToString(),
                Selected = false
            });
            var siteMap = WebContentService.GetSitemapByID(sectionID);
            this.ViewBag.OnlyOneImage = false; //(siteMap != null) ? siteMap.IsType(ContentType.Blog) : false;
            this.ViewBag.CanUploadImages = this.AdminUser.HasPermission(Permission.EditNews);
            this.ViewBag.CanDeleteImage = this.AdminUser.HasPermission(Permission.EditNews);
            //this.ViewBag.UploadAction = Url.Action("SaveBlogImage", "Admn");
            this.ViewBag.UploadAction = Url.Action("SavePageImages", "Admn");
            return PartialView("SitemapImages", siteMap);
        }

        [HttpPost]
        [EditNewsAuthorize]
        public ActionResult SaveBlogImage(HttpPostedFileBase file1, int siteMapID)
        {
            if (file1 != null)
            {
                tbl_SiteMap sitemap = WebContentService.GetSitemapByID(siteMapID);
                DeleteImage(sitemap != null ? sitemap.tbl_Image.Select(i => i.ImageID).FirstOrDefault() : 0);
                return Json(new { success = SaveImage(file1, siteMapID, "", "") }, "text/html");
            }
            return Json(new { success = false }, "text/html");
        }

        [HttpPost]
        [EditNewsAuthorize]
        public ActionResult DeleteBlogImage(int imageID)
        {
            return Json(new { success = DeleteImage(imageID) });
        }

        #endregion

        #region Testimonials

        [AccessRightsAuthorizeAttribute]
        public ActionResult Testimonials()
        {
            return View();
        }

        public ActionResult GetTestimonial(int testimonialID)
        {
            tbl_Testimonials testimonial = WebPagesService.GetTestimonialByID(testimonialID);
            return Json((testimonial == null) ?
                new TestimonialModel
                {
                    TestimonialDate = DateTime.Now.ToCustomDateTimeString(),
                    TestimonialContent = this.ViewBag.DefaultWyswigContent
                } :
                new TestimonialModel
                {
                    TestimonialID = testimonial.TestimonialID,
                    Client = testimonial.TE_Client,
                    Company = testimonial.TE_Company,
                    TestimonialContent = testimonial.TE_Content,
                    IsLive = testimonial.TE_Live,
                    TestimonialDate = testimonial.TE_Date.GetValueOrDefault(DateTime.Now).ToCustomDateTimeString()
                });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddTestimonial(TestimonialModel model)
        {
            tbl_Testimonials testimonial = WebPagesService.SaveTestimonial(0, model.TestimonialDate, model.Client, model.Company, model.TestimonialContent);
            return Json(new { success = (testimonial != null), testimonialID = (testimonial != null ? testimonial.TestimonialID : 0) });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateTestimonial(TestimonialModel model)
        {
            if (model.TestimonialID == 0)
                return Json(new { success = false });

            tbl_Testimonials testimonial = WebPagesService.SaveTestimonial(model.TestimonialID, model.TestimonialDate, model.Client, model.Company,
                model.TestimonialContent);
            return Json((testimonial != null) ?
                new { success = true, testimonialID = testimonial.TestimonialID } :
                new { success = false, testimonialID = 0 });
        }

        [HttpPost]
        public ActionResult DeleteTestimonial(int testimonialID)
        {
            return Json(new { success = WebPagesService.DeleteTestimonial(testimonialID), testimonialID = testimonialID });
        }

        [HttpPost]
        public ActionResult ToggleTestimonialVisibility(int testimonialID)
        {
            return Json(new { success = WebPagesService.SaveTestimonialVisibility(testimonialID) != null });
        }

        #endregion

        #region Form

        [AccessRightsAuthorizeAttribute]
        public ActionResult Forms(int domainID = 0)
        {
            this.ViewBag.TextBox = WebPagesService.GetFormItemTypeIDByName(ContactItemTypeName.Textbox);
            this.ViewBag.TextArea = WebPagesService.GetFormItemTypeIDByName(ContactItemTypeName.Textarea);
            this.ViewBag.Subscribe = WebPagesService.GetFormItemTypeIDByName(ContactItemTypeName.Subscribe);
            this.ViewBag.Datebox = WebPagesService.GetFormItemTypeIDByName(ContactItemTypeName.Datebox);
            this.ViewBag.Divider = WebPagesService.GetFormItemTypeIDByName(ContactItemTypeName.Divider);
            return View();
        }

        public ActionResult FormSettings(int formID = 0)
        {
            var form = WebPagesService.GetFormByID(formID);
            FormModel formModel = (form != null) ?
                new FormModel()
                {
                    FormID = form.FormID,
                    DomainID = form.F_DomainID,
                    Name = form.F_Name,
                    Description = form.F_Description,
                    DestinationPageID = form.F_DestinationSitemapID ?? 0,
                    TrackingCode = form.F_Tracking,
                    Captcha = form.F_Captcha
                } :
                null;

            this.ViewBag.Domains = new SelectList(DomainService.GetAllDomains(), "DomainID", "DO_CompanyName", (form != null ? form.F_DomainID : 0));

            return PartialView(formModel);
        }

        public ActionResult FormItemSettings(int formItemID = 0, int formID = 0, int domainID = 0)
        {
            var formItem = WebPagesService.GetFormItemByID(formItemID);
            if (formItem != null)
            {
                domainID = formItem.tbl_Form.F_DomainID;
                formID = formItem.FI_FormID;
            }
            var domains = DomainService.GetAllDomains();
            if (domainID == 0 && domains.Count > 0)
            {
                domainID = domains.First().DomainID;
            }
            var domainForms = WebPagesService.GetAllFormsByDomainID(domainID);
            if (formID == 0 && domainForms.Count > 0)
            {
                formID = domainForms.First().FormID;
            }

            FormItemModel formItemModel = (formItem != null) ?
            new FormItemModel()
            {
                DomainID = formItem.tbl_Form.F_DomainID,
                FormID = formItem.FI_FormID,
                FormItemID = formItem.FormItemID,
                Name = formItem.FI_Name,
                Text = formItem.FI_Text,
                Required = formItem.FI_Required,
                TypeID = formItem.FI_ItemTypeID,
                Placeholder = formItem.FI_Placeholder,
                ItemValues = formItem.tbl_FormItemValues.Select(v => new FormItemValueModel()
                {
                    FormItemValueID = v.FormItemValueID,
                    Value = v.FIV_Value,
                    Text = v.FIV_Text,
                    Selected = v.FIV_Selected,
                    Order = (v.FIV_Order.HasValue) ?
                        v.FIV_Order.Value :
                        0
                }).OrderBy(m => m.Order).ToList()
            } :
            new FormItemModel()
            {
                DomainID = domainID,
                FormID = formID,
                FormItemID = 0,
                Name = "",
                Text = "",
                Required = false,
                TypeID = 0,
                Placeholder = "",
                ItemValues = new List<FormItemValueModel>()
            };

            this.ViewBag.Domains = new SelectList(domains, "DomainID", "DO_CompanyName", formItemModel.DomainID);
            this.ViewBag.DomainForms = new SelectList(domainForms, "FormID", "F_Name", formItemModel.FormID);
            this.ViewBag.FormItemTypes = WebPagesService.GetAllFormItemTypes();
            return PartialView(formItemModel);
        }

        [HttpPost]
        public ActionResult SaveForm(FormModel model)
        {
            tbl_Form form = WebPagesService.SaveForm(model.Name, model.Description, model.DomainID, model.FormID, model.Captcha, model.DestinationPageID==0 ? (int?)null : model.DestinationPageID, model.TrackingCode);
            return Json((form != null) ?
                    new { success = true, formID = form.FormID } :
                    new { success = false, formID = 0 }
                );
        }

        [HttpPost]
        public ActionResult DeleteForm(int formID)
        {
            return Json(new { success = WebPagesService.DeleteForm(formID), formID = formID });
        }

        [HttpPost]
        public ActionResult SaveFormItem(string name, string text, bool required, int typeID, int formID, int formItemID, string placeholder, int DomainID)
        {
            var formItem = WebPagesService.SaveFormItem(name, text, typeID, required, formID, formItemID, placeholder);
            return Json(new { success = (formItem != null), formItemID = (formItem != null ? formItem.FormItemID : 0) });
        }

        [HttpPost]
        public ActionResult DeleteFormItem(int formItemID)
        {
            return Json(new { success = WebPagesService.DeleteFormItem(formItemID), formItemID = formItemID });
        }

        [HttpPost]
        public ActionResult SaveFormItemVisibility(int formItemID)
        {
            return Json(new { success = WebPagesService.SaveFormItemVisibility(formItemID) != null });
        }

        [HttpPost]
        public ActionResult SaveFormItemOrder(int[] orderedFormItemIDs)
        {
            return Json(new { success = WebPagesService.SaveFormItemsOrder(orderedFormItemIDs) });
        }

        [HttpPost]
        public ActionResult SaveFormItemValues(int value, string text, bool selected, int order, int formItemID, int formItemValueID)
        {
            return Json(new { success = WebPagesService.SaveFormItemValue(value, text, selected, order, formItemID, formItemValueID) != null });
        }

        [HttpPost]
        public ActionResult DeleteFormItemValue(int formItemValueID)
        {
            return Json(new { success = WebPagesService.DeleteFormItemValue(formItemValueID) });
        }

        #endregion

        #region Form Submission

        [AccessRightsAuthorizeAttribute]
        public ActionResult FormSubmission()
        {
            return View();
        }

        public ActionResult FormSubmissions(int formID = 0, int columns = 0)
        {
            if (columns == 0)
            {
                columns = 3;
            }
            var formSubmissions = WebPagesService.GetFormSubmissionsByFormID(formID)
                                                 .OrderByDescending(fs => fs.FS_Date).ToList<tbl_FormSubmission>();
            var formsSubmissionsOrdered = new List<FormSubmissionModel>();

            if (formSubmissions.Count > 0)
            {
                List<FormSubmissionModel>[] models = new List<FormSubmissionModel>[columns];
                for (int i = 0; i < formSubmissions.Count; i++)
                {
                    if (models[i % columns] == null) models[i % columns] = new List<FormSubmissionModel>();
                    models[i % columns].Add(new FormSubmissionModel()
                    {
                        FormSubmissionId = formSubmissions[i].FormSubmissionID,
                        FormId = formSubmissions[i].FS_FormID,
                        Message = formSubmissions[i].FS_Value,
                        Received = formSubmissions[i].FS_Date,
                        Email = formSubmissions[i].FS_Email,
                        Read = formSubmissions[i].FS_Read,
                        Column = i % columns
                    });
                }

                for (int i = 0; i < models.Length; i++)
                {
                    if (models[i] != null)
                    {
                        formsSubmissionsOrdered.AddRange(models[i]);
                    }
                }
            }

            this.ViewBag.FormID = formID;
            return PartialView(formsSubmissionsOrdered);
        }

        public ActionResult DownloadFormSubmissionsAsCSV(int formID = 0)
        {
            int columns = 3;
            int previousColumn = 0;
            var formSubmissions = WebPagesService.GetFormSubmissionsByFormID(formID)
                                                .Where(fs => !fs.FS_Downloaded)
                                                .OrderByDescending(fs => fs.FS_Date).ToList<tbl_FormSubmission>();
            var formsSubmissionsOrdered = new List<FormSubmissionModel>();

            if (formSubmissions.Count > 0)
            {
                List<FormSubmissionModel>[] models = new List<FormSubmissionModel>[columns];
                for (int i = 0; i < formSubmissions.Count; i++)
                {
                    if (models[i % columns] == null) models[i % columns] = new List<FormSubmissionModel>();
                    models[i % columns].Add(new FormSubmissionModel()
                    {
                        FormSubmissionId = formSubmissions[i].FormSubmissionID,
                        FormId = formSubmissions[i].FS_FormID,
                        Message = formSubmissions[i].FS_Value,
                        Received = formSubmissions[i].FS_Date,
                        Email = formSubmissions[i].FS_Email,
                        Read = formSubmissions[i].FS_Read,
                        Column = i % columns
                    });
                }

                for (int i = 0; i < models.Length; i++)
                {
                    if (models[i] != null)
                    {
                        formsSubmissionsOrdered.AddRange(models[i]);
                    }
                }

                StringBuilder row = new StringBuilder();
                StringBuilder header = new StringBuilder();
                header.Append("RECEIVED, EMAIL, ");
                char[] charsToTrim = {',',' '};
                List<string> result = new List<string>();

                bool headerCreated = false;
                foreach (var fs in formsSubmissionsOrdered)
                {
                    if (fs.Column != previousColumn)
                    {
                        if (previousColumn > -1)
                        {
                            
                        }
                    }
                    previousColumn = fs.Column;

                    row.Append(fs.Received.ToString() + ",");
                    row.Append(fs.Email + ",");
                    foreach (var r in fs.MessageRows)
                    {
                        row.Append(r.Value + ",");
                        if (!headerCreated)
                        {
                            header.Append(r.Key.ToUpper() + ",");
                        }
                    }
                    if (!headerCreated)
                    {
                        result.Add(header.ToString().Trim().TrimEnd(charsToTrim));
                        headerCreated = true;
                    }
                    result.Add(row.ToString().Trim().TrimEnd(charsToTrim));
                    row.Clear();
                }
                return new FileContentResult(CSVWriter.WriteFileContent(result), "text/csv") { FileDownloadName = "FormData.csv" };
            }

            return null;
        }

        [HttpPost]
        public ActionResult MarkFormSubmissionAsRead(int formSubmissionID, int formID)
        {
            return Json(new
            {
                success = WebPagesService.MarkFormSubmissionAsRead(formSubmissionID) != null,
                formSubmissionID = formSubmissionID,
                formID = formID
            });
        }

        [HttpPost]
        public ActionResult DeleteFormSubmission(int formSubmissionID, int formID)
        {
            return Json(new
            {
                success = WebPagesService.DeleteFormSubmission(formSubmissionID),
                formSubmissionID = formSubmissionID,
                formID = formID
            });
        }

        #endregion

        #region Countries

        [AccessRightsAuthorizeAttribute]
        public ActionResult Countries()
        {
            return View();
        }

        public ActionResult CountrySettings(int countryID = 0)
        {
            tbl_Country country = ECommerceService.GetCountry(countryID);
            CountryModel countryModel = (country != null) ?
                new CountryModel()
                {
                    CountryID = country.CountryID,
                    DomainID = country.C_DomainID.GetValueOrDefault(0),
                    Code = country.C_Code,
                    PostageZoneID = country.C_PostageZoneID,
                    Country = country.C_Country,
                    IsDefault = country.C_IsDefault
                } :
                null;

            return PartialView(countryModel);
        }

        [HttpPost]
        public JsonResult GetCountryData(int countryID)
        {
            tbl_Country country = ECommerceService.GetCountry(countryID);
            return (country == null) ?
                Json(new { success = false }) :
                Json(new
                {
                    success = true,
                    country = new CountryModel()
                    {
                        CountryID = country.CountryID,
                        DomainID = country.C_DomainID.GetValueOrDefault(0),
                        Code = country.C_Code,
                        PostageZoneID = country.C_PostageZoneID,
                        Country = country.C_Country,
                        IsDefault = country.C_IsDefault
                    }
                });
        }

        public ActionResult CountryImport(int domainID = 0)
        {
            ViewBag.domains = DomainService.GetAllDomainsAsSelectList(domainID);
            return PartialView();
        }

        [HttpPost]
        public JsonResult ImportCountries(int sourceDomainID = 0, int destDomainID = 0)
        {
            return (sourceDomainID == destDomainID) ?
                Json(new { success = false, error = "Please change domain selection: source and destination domains cannot be the same." }) :
                Json(new { success = true, imported = ECommerceService.ImportCountries(sourceDomainID, destDomainID) });
        }

        [HttpPost]
        public JsonResult GetCountrySelectors(int domainID)
        {
            var zones = ECommerceService.GetAllPostageZonesAsSelectList(domainID);
            var defZone = ECommerceService.GetDefaultPostageZone(domainID);
            return Json(new
                {
                    success = true,
                    zones = zones,
                    defaultZone = (defZone != null) ?
                        defZone.PostageZoneID :
                        0
                });
        }

        [HttpPost]
        public JsonResult AddCountry()
        {
            CountryModel country = new CountryModel();
            if (TryUpdateModel<CountryModel>(country))
            {
                tbl_Country dbCountry = ECommerceService.SaveCountry(country.IsDefault, country.Code, country.Country,
                    country.DomainID, country.PostageZoneID, 0);
                if (dbCountry != null)
                    return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult UpdateCountry()
        {
            CountryModel country = new CountryModel();
            if (TryUpdateModel<CountryModel>(country) && country.CountryID != 0)
            {
                tbl_Country dbCountry = ECommerceService.SaveCountry(country.IsDefault, country.Code, country.Country,
                    country.DomainID, country.PostageZoneID, country.CountryID);
                if (dbCountry != null)
                    return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult DeleteCountry(int countryID)
        {
            return Json(new { success = ECommerceService.DeleteCountry(countryID) });
        }

        [HttpPost]
        public ActionResult SaveCountriesOrder(int[] orderedCountryIDs)
        {
            return Json(new { success = ECommerceService.SaveCountriesOrder(orderedCountryIDs) });
        }

        [HttpPost]
        public ActionResult GetCountriesListByDomainID(int domainID)
        {
            var countries = ECommerceService.GetAllCountriesAsSelectList(domainID);
            return Json(countries.ToDictionary(x => x.Text, x => x.Text));
        }

        [HttpPost]
        public ActionResult GetCountriesAsSelectList(int domainID)
        {
            return Json(new { countries = ECommerceService.GetAllCountriesAsSelectList(domainID) });
        }

        #endregion Countries

        #region Postage

        [AccessRightsAuthorizeAttribute]
        public ActionResult Postage()
        {
            return View();
        }

        public ActionResult PostageAttributes(int domainID = 0)
        {
            ViewBag.domains = DomainService.GetAllDomainsAsSelectList(domainID);
            return PartialView();
        }

        public ActionResult PostageSettings(int postageID = 0)
        {
            ViewBag.Domains = DomainService.GetAllDomainsAsSelectList(0);
            return PartialView();
        }

        [HttpPost]
        public JsonResult GetPostageData(int postageID = 0)
        {
            if (postageID == 0)
                return Json(new { success = false });
            var entity = ECommerceService.GetPostageByID(postageID);
            if (entity == null)
                return Json(new { success = false });
            var model = new PostageModel()
            {
                PostageID = postageID,
                PST_Description = entity.PST_Description,
                PST_Amount = entity.PST_Amount,
                PST_DomainID = entity.PST_DomainID,
                PST_PostageBand = entity.PST_PostageBandID,
                PST_PostageWeight = entity.PST_PostageWeightID,
                PST_PostageZone = entity.PST_PostageZoneID
            };
            return Json(new { success = true, postage = model });
        }

        [HttpPost]
        public JsonResult AddPostage(PostageModel postage)
        {
            if (ModelState.IsValid)
            {
                //return Json(new { success = postage != null, postageID = postage != null ? .DomainID : 0 });
                var result = ECommerceService.SavePostage(postage.PST_Description, postage.PST_Amount,
                    postage.PST_DomainID, postage.PST_PostageBand, postage.PST_PostageWeight, postage.PST_PostageZone, 0);
                if (result != null)
                    return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult UpdatePostage(PostageModel postage)
        {
            if (ModelState.IsValid)
            {
                if (postage.PostageID == null || postage.PostageID == 0)
                    return Json(new { success = false });
                var result = ECommerceService.SavePostage(postage.PST_Description, postage.PST_Amount,
                    postage.PST_DomainID, postage.PST_PostageBand, postage.PST_PostageWeight, postage.PST_PostageZone, (int)postage.PostageID);
                if (result != null)
                    return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult DeletePostage(int postageID)
        {
            return Json(new { success = ECommerceService.DeletePostage(postageID) });
        }

        [HttpPost]
        public JsonResult DeleteBand(tbl_PostageBands postageBand)
        {
            ECommerceService.DeletePostageBand(postageBand.PostageBandID);
            return Json(new { success = true, id = postageBand.PostageBandID });
        }

        [HttpPost]
        public JsonResult DeleteWeight(tbl_PostageWeights postageWeight)
        {
            ECommerceService.DeletePostageWeight(postageWeight.PostageWeightID);
            return Json(new { success = true, id = postageWeight.PostageWeightID });
        }

        [HttpPost]
        public JsonResult DeleteZone(tbl_PostageZones postageZone)
        {
            ECommerceService.DeletePostageZone(postageZone.PostageZoneID);
            return Json(new { success = true, id = postageZone.PostageZoneID });
        }

        [HttpPost]
        public JsonResult GetPostageAttributesSelectors(int domainID)
        {
            var bands = ECommerceService.GetAllPostageBands(domainID);
            var weights = ECommerceService.GetAllPostageWeights(domainID);
            var zones = ECommerceService.GetAllPostageZones(domainID);

            return Json(new
            {
                bands = (bands.Any()) ?
                    bands.Select(m => new PostageBandModel { PostageBandID = m.PostageBandID, PB_Lower = m.PB_Lower, PB_Upper = m.PB_Upper, PB_DomainID = m.PB_DomainID }) : null,
                weights = (weights.Any()) ?
                    weights.Select(m => new PostageWeightModel { PostageWeightID = m.PostageWeightID, PW_Lower = m.PW_Lower, PW_Upper = m.PW_Upper, PW_DomainID = m.PW_DomainID }) : null,
                zones = (zones.Any()) ?
                    zones.Select(m => new PostageZoneModel { PostageZoneID = m.PostageZoneID, PZ_Name = m.PZ_Name, PZ_DomainID = m.PZ_DomainID, PZ_IsDefault = m.PZ_IsDefault }) : null
            });
        }

        [HttpPost]
        public JsonResult GetPostageSelectors(int domainID)
        {
            var bands = ECommerceService.GetAllPostageBandsAsSelectList(domainID);
            var weights = ECommerceService.GetAllPostageWeightsAsSelectList(domainID);
            var zones = ECommerceService.GetAllPostageZonesAsSelectList(domainID);
            var defZone = ECommerceService.GetDefaultPostageZone(domainID);
            return Json(new
            {
                bands = bands,
                weights = weights,
                zones = zones,
                defaultZone = (defZone != null) ?
                        defZone.PostageZoneID :
                        0
            });
        }

        [HttpPost]
        public JsonResult SaveBand(tbl_PostageBands band)
        {
            if (ModelState.IsValid)
            {
                if (band.PB_Lower > band.PB_Upper)
                    return Json(new { success = false });
                tbl_PostageBands savedBand = ECommerceService.SavePostageBand(band);
                if (savedBand != null)
                {
                    var jsonBand = new PostageBandModel
                    {
                        PostageBandID = savedBand.PostageBandID,
                        PB_DomainID = savedBand.PB_DomainID,
                        PB_Lower = savedBand.PB_Lower,
                        PB_Upper = savedBand.PB_Upper
                    };
                    return Json(new { success = true, band = jsonBand });
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult SaveWeight(tbl_PostageWeights postage)
        {
            if (ModelState.IsValid)
            {
                if (postage.PW_Lower > postage.PW_Upper)
                    return Json(new { success = false });
                tbl_PostageWeights savedPostage = ECommerceService.SavePostageWeight(postage);
                if (savedPostage != null)
                {
                    var jsonWeight = new PostageWeightModel
                    {
                        PostageWeightID = savedPostage.PostageWeightID,
                        PW_DomainID = savedPostage.PW_DomainID,
                        PW_Lower = savedPostage.PW_Lower,
                        PW_Upper = savedPostage.PW_Upper
                    };
                    return Json(new { success = true, weight = jsonWeight });
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult SaveZone(tbl_PostageZones zone)
        {
            if (ModelState.IsValid)
            {
                tbl_PostageZones savedZone = ECommerceService.SavePostageZone(zone);
                if (savedZone != null)
                {
                    var jsonZone = new PostageZoneModel
                    {
                        PostageZoneID = savedZone.PostageZoneID,
                        PZ_DomainID = savedZone.PZ_DomainID,
                        PZ_Name = savedZone.PZ_Name,
                        PZ_IsDefault = savedZone.PZ_IsDefault
                    };
                    return Json(new { success = true, zone = jsonZone });
                }
            }
            return Json(new { success = false });
        }
        #endregion

        #region Payment Domain

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult SavePaymentLogo(HttpPostedFileBase file, int paymentDomainID, int domainID, string code)
        {
            if (file != null)
            {
                string image;

                string currentImg = ECommerceService.GetPaymentDomainLogo(paymentDomainID);
                if (currentImg != null)
                    DeleteFile(String.Format("/{0}/{1}/{2}", SettingsManager.Payment.PaymentLogosPath.Trim('/'), domainID, currentImg));
                image = RandomStr(Path.GetFileName(file.FileName));
                if (SaveImageFile(file, String.Format("/{0}/{1}/{2}", SettingsManager.Payment.PaymentLogosPath.Trim('/'), domainID, image)))
                {
                    image = ECommerceService.SavePaymentLogoImage(paymentDomainID, image);
                    return Json(new
                    {
                        success = true,
                        id = code,
                        src = String.Format("/{0}/{1}/{2}", SettingsManager.Payment.PaymentLogosPath.Trim('/'), domainID, image)
                    }, "text/html");
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult DeletePaymentLogo(int paymentDomainID, int domainID, string code)
        {
            string currentImg = ECommerceService.GetPaymentDomainLogo(paymentDomainID);
            if (currentImg != null)
                DeleteFile(String.Format("/{0}/{1}/{2}", SettingsManager.Payment.PaymentLogosPath.Trim('/'), domainID, currentImg));
            if (ECommerceService.DeletePaymentLogoImage(paymentDomainID))
            {
                return Json(new
                {
                    success = true,
                    id = code,
                    src = "",
                }, "text/html");
            }
            return Json(new
                {
                    succcess = false
                }, "text/html");

        }
        #endregion

        #region Product Categories

        [AccessRightsAuthorize]
        public ActionResult EventCategories()
        {
            this.ViewBag.CategoryCSSFile = (this.Domain != null) ? this.Domain.DO_CSS : "";
            return View();
        }

        [AccessRightsAuthorizeAttribute]
        public ActionResult ProdCategories()
        {
            this.ViewBag.CategoryCSSFile = (this.Domain != null) ? this.Domain.DO_CSS : "";
            return View();
        }

        public ActionResult ProdCategoriesSettings(int categoryID = 0, int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentBySitemapID(categoryID, contentID);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null) ?
                content.tbl_SiteMap.SM_DomainID : 0);
            this.ViewBag.Taxes = ECommerceService.GetAllTaxesAsSelectList((content != null) ?
                content.tbl_SiteMap.tbl_ProdCategories.PC_TaxID.GetValueOrDefault(0) : 0);
            return PartialView(content);
        }

        public ActionResult SEOCategoriesFields(int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentByID(contentID);
            return PartialView("SEOFields", (content == null) ?
                new SEOFieldsModel { IsPageContent = false } :
                new SEOFieldsModel
                {
                    Title = content.C_Title,
                    Desc = content.C_Description,
                    Keywords = content.C_Keywords,
                    MetaData = content.C_MetaData,
                    R301 = content.tbl_SiteMap.SM_301,
                    IsPageContent = false
                });
        }

        public ActionResult CategoriesContentVersion(int sectionID, int contentID = 0)
        {
            tbl_SiteMap section = WebContentService.GetSitemapByID(sectionID);
            this.ViewBag.ContentID = contentID;
            this.ViewBag.CanDeleteVersion = true;
            return PartialView("ContentVersion", section);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddProdCategory(int domainID, int parentID, int taxID, string categoryTitle, bool live, SEOFieldsModel seoModel,
            string content, bool isMenu, MenuDisplayType displayType, bool featured = false, ProductType type = ProductType.Item)
        {
            if (ModelState.IsValid)
            {
                int parentsiteID = parentID;
                var sitemap = WebContentService.GetSitemapByDomainID(domainID);
                string url = "";
                if (parentsiteID == 0)
                {
                    if (type == ProductType.Item)
                        parentsiteID = sitemap.Where(m => m.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID))
                            .Select(m => m.SiteMapID).FirstOrDefault();
                    else if (type == ProductType.Event)
                        parentsiteID = sitemap.Where(m => m.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID))
                            .Select(m => m.SiteMapID).FirstOrDefault();
                    url = String.Format("/{0}", categoryTitle);
                }
                else
                {
                    var parentsite = sitemap.Where(m => m.SiteMapID == parentsiteID).FirstOrDefault();
                    if (parentsite == null)
                        return Json(new { success = false, error = "Parent Category ID is invalid" });
                    url = String.Format("{0}/{1}", parentsite.SM_URL, categoryTitle);
                }
                if (WebContentService.CheckSitemapUniqueUrl(url, 0, domainID))
                    return Json(new { success = false, error = "Please change 'Title'. There is another page with the same URL already registered." });

                SiteMapType sitemapType = type == ProductType.Event ? SiteMapType.EventShop : SiteMapType.ProductShop; 
                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, domainID, String.Empty, isMenu, false, seoModel.Title,
                    null, "0.5", String.Empty, url, true, true, ContentType.Category, seoModel.Title, parentsiteID, 0, (int)sitemapType, false, displayType);

                tbl_Content tContent = (section != null) ?
                    WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, seoModel.Title,
                        seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) :
                    null;

                tbl_ProdCategories category = ECommerceService.SaveProdCategory(categoryTitle, live, parentID, taxID == 0 ? (int?)null : taxID, section.SiteMapID, type, featured);

                return Json(new
                {
                    success = section != null && tContent != null && category != null,
                    categoryID = (category != null) ? category.CategoryID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateProdCategory(int domainID, int parentID, int taxID, string categoryTitle, bool live, SEOFieldsModel seoModel,
            string content, int categoryID, bool isMenu, MenuDisplayType displayType, ProductType type, bool featured = false)
        {
            if (ModelState.IsValid && categoryID > 0)
            {
                int parentsiteID = parentID;
                var sitemap = WebContentService.GetSitemapByDomainID(domainID);
                string url = "";
                if (parentsiteID == 0)
                {
                    if (type == ProductType.Item)
                        parentsiteID = sitemap.Where(m => m.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID))
                            .Select(m => m.SiteMapID).FirstOrDefault();
                    else if (type == ProductType.Event)
                        parentsiteID = sitemap.Where(m => m.SM_URL == WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID))
                            .Select(m => m.SiteMapID).FirstOrDefault();
                    url = String.Format("/{0}", categoryTitle);
                }
                else
                {
                    var parentsite = sitemap.Where(m => m.SiteMapID == parentsiteID).FirstOrDefault();
                    if (parentsite == null)
                        return Json(new { success = false, error = "Parent Category ID is invalid" });
                    url = String.Format("{0}/{1}", parentsite.SM_URL, categoryTitle);
                }

                if (WebContentService.CheckSitemapUniqueUrl(url, categoryID, domainID))
                    return Json(new { success = false, error = "Please change 'Category Title'. There is another page with the same URL already registered." });
                if (parentID == categoryID)
                    return Json(new { success = false, error = "Category parent cannot be same as category." });

                SiteMapType sitemapType = type == ProductType.Event ? SiteMapType.EventShop : SiteMapType.ProductShop; 
                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, domainID, String.Empty, isMenu, false, seoModel.Title,
                    null, "0.5", String.Empty, url, true, true, ContentType.Category, seoModel.Title, parentsiteID, categoryID, (int)sitemapType, false, displayType);

                tbl_Content tContent = (section != null) ?
                        WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, seoModel.Title,
                            seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) :
                        null;

                tbl_ProdCategories category = ECommerceService.SaveProdCategory(categoryTitle, live, parentID, (taxID == 0) ? (int?)null : taxID, categoryID, type, featured);

                return Json(new
                {
                    success = section != null && tContent != null && category != null,
                    categoryID = (category != null) ? category.CategoryID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteProdCategory(int categoryID)
        {
            bool success = ECommerceService.DeleteProdCategory(categoryID);
            success = success && WebContentService.DeleteSection(categoryID);

            return Json(new { success });
        }

        [HttpPost]
        public ActionResult GetProdCategoriesList(int domainID, int categoryID = 0, ProductType type = ProductType.Item)
        {
            var category = ECommerceService.GetProdCategoryByID(categoryID);
            var categories = ECommerceService.GetProdCategoriesForDomainAsSelectList(domainID, (category != null) ? category.PC_ParentID.GetValueOrDefault(0) : 0, type);
            foreach (var cat in categories)
            {
                int level = cat.Level;
                while (--level > 0)
                    cat.Text = "&nbsp;&nbsp;" + cat.Text;
            }
            categories.Insert(0, new ExtendedSelectListItem { Text = "Top Level Category", Value = "0", Selected = false });
            return Json(new { categories, pCategoryID = (category != null ? category.PC_ParentID.GetValueOrDefault(0) : 0) });
        }

        [HttpPost]
        public ActionResult SaveProdCategoriesOrder(int[] orderedCategoriesIDs)
        {
            return Json(new { success = ECommerceService.SaveCategoriesOrder(orderedCategoriesIDs) });
        }

        [HttpPost]
        public ActionResult DeleteProdCategoryVersion(int contentID)
        {
            return Json(new { success = WebContentService.DeleteContent(contentID) });
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult ApproveProdCategoryVersion(int categoryID, int contentID)
        {
            return Json(new
            {
                success = WebContentService.ApproveContent(categoryID, contentID) != null,
                categoryID = categoryID
            });
        }

        public ActionResult ProdCategoryImage(int categoryID)
        {
            tbl_ProdCategories category = ECommerceService.GetProdCategoryByID(categoryID);
            return PartialView(category);
        }

        [HttpPost]
        public ActionResult SaveCategoryImage(HttpPostedFileBase file, string description, string altText, int categoryID)
        {
            var category = ECommerceService.GetProdCategoryByID(categoryID);
            if (category == null)
                return Json(new { success = false }, "text/html");

            if (category.tbl_ProductImages != null)
            {
                int imageID = category.tbl_ProductImages.ImageID;
                ECommerceService.SaveProdCategoryImage(categoryID, null);
                DeleteProdImage(imageID);
            }

            var image = SaveProdImage(file, description, altText, 0);
            if (image != null)
            {
                ECommerceService.SaveProdCategoryImage(categoryID, image.ImageID);
                return Json(new { success = true }, "text/html");
            }

            return Json(new { success = false }, "text/html");
        }

        [HttpPost]
        public ActionResult DeleteCategoryImage(int categoryID, int imageID)
        {
            var category = ECommerceService.SaveProdCategoryImage(categoryID, null);
            var result = DeleteProdImage(imageID);
            return Json(new { success = result && category != null });
        }

        #endregion

        #region Product Attributes

        [AccessRightsAuthorizeAttribute]
        public ActionResult ProdAttributes()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAttribute(int attributeID)
        {
            var attribute = ECommerceService.GetProdAttributeByID(attributeID);
            return (attribute == null) ?
                null :
                Json(new ProdAttributeModel
                {
                    AttributeID = attribute.AttributeID,
                    Title = attribute.A_Title,
                    Values = attribute.tbl_ProdAttValue.Select(v => new ProdAttributeValueModel { Value = v.AV_Value, ValueID = v.AttributeValueID }).ToList()
                });
        }

        [HttpPost]
        public ActionResult AddAttribute(string title)
        {
            var attribute = ECommerceService.SaveProdAttribute(title, 0);
            return Json(new { success = attribute != null, attributeID = attribute != null ? attribute.AttributeID : 0 });
        }

        [HttpPost]
        public ActionResult UpdateAttribute(string title, int attributeID)
        {
            return (attributeID == 0) ?
                Json(new { success = false }) :
                Json(new { success = ECommerceService.SaveProdAttribute(title, attributeID) != null, attributeID });
        }

        [HttpPost]
        public ActionResult DeleteAttribute(int attributeID)
        {
            return Json(new { success = ECommerceService.DeleteProdAttribute(attributeID) });
        }

        [HttpPost]
        public ActionResult AddAttributeValue(string value, int attributeID)
        {
            return Json(new { success = ECommerceService.SaveProdAttValue(value, 0, attributeID) != null });
        }

        [HttpPost]
        public ActionResult UpdateAttributeValue(string value, int attributeValueID)
        {
            return (attributeValueID == 0) ?
                Json(new { success = false }) :
                Json(new { success = ECommerceService.SaveProdAttValue(value, attributeValueID) != null });
        }

        [HttpPost]
        public ActionResult DeleteAttributeValue(int attributeValueID)
        {
            return Json(new { success = ECommerceService.DeleteProdAttValue(attributeValueID) });
        }

        #endregion

        #region Discount

        [AccessRightsAuthorizeAttribute]
        public ActionResult Discount()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetDiscount(int discountID)
        {
            var discount = ECommerceService.GetDiscountByID(discountID);
            return (discount == null) ?
                null :
                Json(new DiscountModel
                {
                    Value = discount.D_Value,
                    IsPercentage = discount.D_IsPercentage,
                    Code = discount.D_Code,
                    Title = discount.D_Title,
                    Description = discount.D_Description,
                    DiscountID = discount.DiscountID,
                    Start = (discount.D_Start.HasValue) ? discount.D_Start.Value.ToCustomDateString() : String.Empty,
                    Expire = (discount.D_Expiry.HasValue) ? discount.D_Expiry.Value.ToCustomDateString() : String.Empty,
                    DomainID = discount.D_DomainID
                });
        }

        [HttpPost]
        public ActionResult AddDiscount(DiscountModel model)
        {
            if (ModelState.IsValid)
            {
                var discount = ECommerceService.SaveDiscount(model.Value, model.IsPercentage, model.Code, model.Description, model.Title, model.Start, model.Expire, model.DomainID, 0);
                return Json((discount != null) ?
                        new { success = true, discountID = discount.DiscountID } :
                        new { success = false, discountID = 0 }
                    );
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult UpdateDiscount(DiscountModel model)
        {
            if (ModelState.IsValid && model.DiscountID > 0)
            {
                var discount = ECommerceService.SaveDiscount(model.Value, model.IsPercentage, model.Code, model.Description, model.Title, model.Start, model.Expire, model.DomainID, model.DiscountID);
                return Json((discount != null) ?
                        new { success = true, discountID = discount.DiscountID } :
                        new { success = false, discountID = 0 }
                    );
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteDiscount(int discountID)
        {
            return Json(new { success = ECommerceService.DeleteDiscount(discountID) });
        }

        #endregion

        #region Product

        [AccessRightsAuthorizeAttribute]
        public ActionResult Events()
        {
            return Products(ProductType.Event);
        }

        [AccessRightsAuthorizeAttribute]
        public ActionResult Products(ProductType type = ProductType.Item)
        {
            this.ViewBag.ProductType = type;
            this.ViewBag.ProductCSSFile = (this.Domain != null) ? this.Domain.DO_CSS : "";
            return View("Products");
        }

        public ActionResult ProductsLeftMenu(ProductType type, string search = "")
        {
            List<CMSMenuModel> model = LeftMenuModelMapper.MapProducts(ECommerceService.SearchProducts(search, type), this.AdminUser);
            return PartialView("LeftMenu", model);
        }

        [HttpPost]
        public JsonResult GetCategoriesList(int productID, string type, int domainID = 0)
        {
            var product = ECommerceService.GetProductByID(productID);
            ProductType productType = type.ToLowerInvariant().Equals("product") ? ProductType.Item : ProductType.Event;
            var extendedCategories = ECommerceService.GetProdCategoriesForDomainAsSelectList(domainID, 0, productType);
            var categories = new List<SelectListItem>();
            foreach (var cat in extendedCategories)
            {
                int level = cat.Level;
                while (--level > 0)
                    cat.Text = "\xA0\xA0" + cat.Text;
                categories.Add(new SelectListItem() { Selected = cat.Selected, Text = cat.Text, Value = cat.Value });
            }
            return Json(new
            {
                success = true,
                categoriesList = categories,
                selectedCategoryID = (product != null) ?
                    product.P_CategoryID.GetValueOrDefault(0) :
                    0
            });
        }

        [HttpPost]
        public JsonResult GetEventTypes(int productID, int contentID)
        {
            var types = ECommerceService.GetAllEventTypes();
            var model = types.Select(m => new EventTypeModel
            {
                EventTypeID = m.EventTypeID,
                Title = m.ET_Title,
                Description = m.ET_Description
            }).ToList();
            tbl_Content content = WebContentService.GetContentBySitemapID(productID, contentID);
            var selected = (content != null) ?
                content.tbl_SiteMap.tbl_Products.P_EventTypeID.GetValueOrDefault(0) :
                            0;
            return Json(new { success = true, eventTypes = model, selected = selected });
        }

        [HttpPost]
        public JsonResult ProductSettingsJson(ProductType type, int productID = 0, int contentID = 0, bool includeDeleted = false)
        {
            tbl_Content content = WebContentService.GetContentBySitemapID(productID, contentID);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null) ?
                content.tbl_SiteMap.SM_DomainID : 0);
            this.ViewBag.Taxes = ECommerceService.GetAllTaxesAsSelectList((content != null) ?
                content.tbl_SiteMap.tbl_Products.P_TaxID.GetValueOrDefault(0) : 0);
            var product = new ProductModel
                {
                    CategoryID = content.tbl_SiteMap.tbl_Products.P_CategoryID.GetValueOrDefault(0),
                    Content = content.C_Content,
                    ContentID = content.ContentID,
                    DomainID = content.tbl_SiteMap.SM_DomainID,
                    Deliverable = content.tbl_SiteMap.tbl_Products.P_Deliverable,
                    Purchasable = content.tbl_SiteMap.tbl_Products.P_CanPurchase,
                    Featured = content.tbl_SiteMap.tbl_Products.P_Featured,
                    Live = content.tbl_SiteMap.tbl_Products.P_Live,
                    Offer = content.tbl_SiteMap.tbl_Products.P_Offer.GetValueOrDefault(false),
                    ProductCode = content.tbl_SiteMap.tbl_Products.P_ProductCode,
                    ProductID = content.tbl_SiteMap.tbl_Products.ProductID,
                    SitemapID = content.tbl_SiteMap.SiteMapID,
                    ProductTitle = content.tbl_SiteMap.tbl_Products.P_Title,
                    TaxID = content.tbl_SiteMap.tbl_Products.P_TaxID.GetValueOrDefault(0),
                    StockControl = content.tbl_SiteMap.tbl_Products.P_StockControl,
                    ProductType = type,
                    EventTypeID = content.tbl_SiteMap.tbl_Products.P_EventTypeID
                };
            if (type == ProductType.Event)
            {
                product.EventTypes =
                    ECommerceService.GetAllEventTypesAsSelectList((content != null) ?
                            content.tbl_SiteMap.tbl_Products.P_EventTypeID.GetValueOrDefault(0) :
                            0);
            }

            return Json(
                new ProductModel
                {
                    CategoryID = content.tbl_SiteMap.tbl_Products.P_CategoryID.GetValueOrDefault(0),
                    Content = content.C_Content,
                    ContentID = content.ContentID,
                    DomainID = content.tbl_SiteMap.SM_DomainID,
                    Deliverable = content.tbl_SiteMap.tbl_Products.P_Deliverable,
                    Purchasable = content.tbl_SiteMap.tbl_Products.P_CanPurchase,
                    Featured = content.tbl_SiteMap.tbl_Products.P_Featured,
                    Live = content.tbl_SiteMap.tbl_Products.P_Live,
                    Offer = content.tbl_SiteMap.tbl_Products.P_Offer.GetValueOrDefault(false),
                    ProductCode = content.tbl_SiteMap.tbl_Products.P_ProductCode,
                    ProductID = content.tbl_SiteMap.tbl_Products.ProductID,
                    SitemapID = content.tbl_SiteMap.SiteMapID,
                    ProductTitle = content.tbl_SiteMap.tbl_Products.P_Title,
                    TaxID = content.tbl_SiteMap.tbl_Products.P_TaxID.GetValueOrDefault(0),
                    StockControl = content.tbl_SiteMap.tbl_Products.P_StockControl,
                    ProductType = type,
                    EventTypeID = content.tbl_SiteMap.tbl_Products.P_EventTypeID
                });
        }

        public ActionResult ProductSettings(ProductType type, int productID = 0, int contentID = 0, bool includeDeleted = false)
        {
            tbl_Content content = WebContentService.GetContentBySitemapID(productID, contentID);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null) ?
                content.tbl_SiteMap.SM_DomainID : 0);
            this.ViewBag.Taxes = ECommerceService.GetAllTaxesAsSelectList((content != null) ?
                content.tbl_SiteMap.tbl_Products.P_TaxID.GetValueOrDefault(0) : 0);

            if (type == ProductType.Event)
            {
                this.ViewBag.EventTypes =
                    ECommerceService.GetAllEventTypesAsSelectList((content != null) ?
                            content.tbl_SiteMap.tbl_Products.P_EventTypeID.GetValueOrDefault(0) :
                            0);
            }

            return PartialView((content == null) ?
                new ProductModel
                {
                    StockControl = true,
                    ProductType = type,
                    Content = this.ViewBag.DefaultWyswigContent
                } :
                new ProductModel
                {
                    CategoryID = content.tbl_SiteMap.tbl_Products.P_CategoryID.GetValueOrDefault(0),
                    Content = content.C_Content,
                    ContentID = content.ContentID,
                    DomainID = content.tbl_SiteMap.SM_DomainID,
                    Deliverable = content.tbl_SiteMap.tbl_Products.P_Deliverable,
                    Purchasable = content.tbl_SiteMap.tbl_Products.P_CanPurchase,
                    Featured = content.tbl_SiteMap.tbl_Products.P_Featured,
                    Live = content.tbl_SiteMap.tbl_Products.P_Live,
                    Offer = content.tbl_SiteMap.tbl_Products.P_Offer.GetValueOrDefault(false),
                    ProductCode = content.tbl_SiteMap.tbl_Products.P_ProductCode,
                    ProductID = content.tbl_SiteMap.tbl_Products.ProductID,
                    SitemapID = content.tbl_SiteMap.SiteMapID,
                    ProductTitle = content.tbl_SiteMap.tbl_Products.P_Title,
                    TaxID = content.tbl_SiteMap.tbl_Products.P_TaxID.GetValueOrDefault(0),
                    StockControl = content.tbl_SiteMap.tbl_Products.P_StockControl,
                    ProductType = type,
                    EventTypeID = content.tbl_SiteMap.tbl_Products.P_EventTypeID,
                    AffiliateLink = content.tbl_SiteMap.tbl_Products.P_AffliateLink
                });
        }

        public ActionResult SEOProductFields(int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentByID(contentID);
            return PartialView("SEOFields",
                (content == null) ?
                 new SEOFieldsModel { IsPageContent = false } :
                 new SEOFieldsModel
                 {
                     Title = content.C_Title,
                     Desc = content.C_Description,
                     Keywords = content.C_Keywords,
                     MetaData = content.C_MetaData,
                     R301 = content.tbl_SiteMap.SM_301,
                     IsPageContent = false
                 });
        }

        public ActionResult ProductContentVersion(int sectionID, int contentID = 0)
        {
            tbl_SiteMap section = WebContentService.GetSitemapByID(sectionID);
            this.ViewBag.ContentID = contentID;
            this.ViewBag.CanDeleteVersion = true;
            return PartialView("ContentVersion", section);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductModel productModel, SEOFieldsModel seoModel)
        {
            if (ModelState.IsValid)
            {
                var category = ECommerceService.GetProdCategoryByID(productModel.CategoryID);
                if (category == null)
                    return Json(new { success = false });
                if (WebContentService.CheckSitemapUniqueUrl(ProductUrl(productModel, category), 0, productModel.DomainID))
                    return Json(new { success = false, error = "Please change 'Title'. There is another page with the same URL already registered." });

                SiteMapType sitemapType = productModel.ProductType == ProductType.Event ? SiteMapType.EventShop : SiteMapType.ProductShop; 
                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, productModel.DomainID, String.Empty, false, false, seoModel.Title,
                    null, "0.5", String.Empty, String.Format("{0}/{1}", category.tbl_SiteMap.SM_URL, productModel.ProductTitle), true, true, ContentType.Product, seoModel.Title,
                    category.tbl_SiteMap.SiteMapID, 0, (int)sitemapType);

                tbl_Content tContent = section != null ? WebContentService.SaveContent(String.Empty, productModel.Content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, seoModel.Title,
                    seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) : null;

                tbl_Products product = ECommerceService.SaveProduct(0, productModel.CategoryID, String.Empty, DateTime.UtcNow, 1, productModel.Offer, productModel.ProductCode,
                    null, String.Empty, productModel.TaxID == 0 ? (int?)null : productModel.TaxID, productModel.ProductTitle, productModel.Live, productModel.StockControl,
                    productModel.ProductType, productModel.EventTypeID, productModel.Deliverable, productModel.Purchasable, productModel.Featured, productModel.AffiliateLink, section.SiteMapID);

                return Json(new
                {
                    success = section != null && tContent != null && product != null,
                    productID = (product != null) ?
                        product.ProductID :
                        0,
                    contentID = (tContent != null) ?
                        tContent.ContentID :
                        0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateProduct(ProductModel productModel, SEOFieldsModel seoModel)
        {
            if (ModelState.IsValid && productModel.ProductID != 0)
            {
                var category = ECommerceService.GetProdCategoryByID(productModel.CategoryID);
                if (category == null)
                    return Json(new { success = false });

                if (WebContentService.CheckSitemapUniqueUrl(ProductUrl(productModel, category), productModel.SitemapID, productModel.DomainID))
                    return Json(new { success = false, error = "Please change 'Title'. There is another page with the same URL already registered." });

                SiteMapType sitemapType = productModel.ProductType == ProductType.Event ? SiteMapType.EventShop : SiteMapType.ProductShop; 
                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, productModel.DomainID, String.Empty, false, false, seoModel.Title,
                    null, "0.5", String.Empty, String.Format("{0}/{1}", category.tbl_SiteMap.SM_URL, productModel.ProductTitle), true, true, ContentType.Product,
                    seoModel.Title, category.tbl_SiteMap.SiteMapID, productModel.SitemapID, (int)sitemapType);

                tbl_Content tContent = section != null ? WebContentService.SaveContent(String.Empty, productModel.Content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, seoModel.Title,
                    seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) : null;

                tbl_Products product = ECommerceService.SaveProduct(0, productModel.CategoryID, String.Empty, DateTime.UtcNow, 1, productModel.Offer, productModel.ProductCode,
                    null, String.Empty, productModel.TaxID == 0 ? (int?)null : productModel.TaxID, productModel.ProductTitle, productModel.Live, productModel.StockControl,
                    productModel.ProductType, productModel.EventTypeID, productModel.Deliverable, productModel.Purchasable, productModel.Featured, productModel.AffiliateLink, section.SiteMapID);

                return Json(
                    new
                    {
                        success = section != null && tContent != null && product != null,
                        productID = (product != null) ?
                            product.ProductID :
                            0,
                        contentID = (tContent != null) ?
                            tContent.ContentID :
                            0
                    });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult SaveProductsOrder(int[] orderedProductIDs)
        {
            return Json(new { success = ECommerceService.SaveProductOrder(orderedProductIDs) });
        }

        [HttpPost]
        [DeleteContentAuthorize]
        public ActionResult DeleteProduct(int productID)
        {
            var deleted = WebContentService.DeleteSection(productID);
            return Json(new { success = deleted && ECommerceService.DeleteProduct(productID) });
        }

        [HttpPost]
        [DeleteContentAuthorize]
        public ActionResult DeleteProductVersion(int contentID)
        {
            return Json(new { success = WebContentService.DeleteContent(contentID) });
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult ApproveProductVersion(int productID, int contentID = 0)
        {
            return Json(new { success = WebContentService.ApproveContent(productID, contentID) != null, productID = productID });
        }

        [HttpPost]
        public ActionResult GetAttributesList()
        {
            return Json(new { attributes = ECommerceService.GetAllProdAttributesList() });
        }

        [HttpPost]
        public ActionResult AddAttributeToProduct(int productID, int attributeID)
        {
            return Json(new { success = ECommerceService.AddProductAttrbiute(productID, attributeID) });
        }

        [HttpPost]
        public ActionResult RemoveAttributeFromProduct(int productID, int attributeID)
        {
            return Json(new { success = ECommerceService.RemoveProductAttrbiute(productID, attributeID) });
        }

        public ActionResult ProductStock(int productID)
        {
            var product = ECommerceService.GetProductByID(productID);
            this.ViewBag.TaxIncluded = DomainService.GetSettingsValueAsBool(SettingsKey.priceSaveIncludesVAT, this.DomainID);
            return PartialView(product);
        }

        [HttpPost]
        public ActionResult GetSaleDates(int productPriceID)
        {
            var productPrice = ECommerceService.GetProductPriceByID(productPriceID);
            if (productPrice == null)
                return null;

            return PartialView("~/Views/Partials/ProductPriceSaleDates.cshtml", productPrice);
        }

        public ActionResult GetTimeWindow(int productPriceTimeWindowID, int productPriceID)
        {
            var productPriceTimeWindow = ECommerceService.GetProductPriceTimeWindowByID(productPriceTimeWindowID);
            var model = productPriceTimeWindow == null ? new TimeWindowModel() { ProductPriceID = productPriceID } : new TimeWindowModel
            {
                Price = productPriceTimeWindow.TW_Price.ToString().Replace(',', '.'),
                EndDate = (productPriceTimeWindow.TW_EndDate.HasValue) ? productPriceTimeWindow.TW_EndDate.Value.ToCustomDateTimeString() : String.Empty,
                StartDate = productPriceTimeWindow.TW_StartDate.ToCustomDateTimeString(),
                ProductPriceID = productPriceTimeWindow.TW_ProductPriceID,
                ProductPriceTimeWindowID = productPriceTimeWindow.ProductPriceTimeWindowID
            };

            return PartialView("~/Views/Partials/ProductPriceTimeWindow.cshtml", model);
        }

        [HttpPost]
        public ActionResult AddStockUnitToProduct(int productID)
        {
            return Json(new { success = ECommerceService.SaveProductStockUnit(productID, 0, String.Empty, String.Empty, false, "0", "0", "0", String.Empty, 0, "0", String.Empty, String.Empty,"0") != null });
        }

        [HttpPost]
        public ActionResult CreateProductStockMatrix(int productID, int[] prodAttValueIDs)
        {
            return Json(new { success = ECommerceService.CreateStockUnitsMatrix(productID, prodAttValueIDs) });
        }

        [HttpPost]
        public ActionResult AddTimeWindowToProductPrice(TimeWindowModel model)
        {
            if (ModelState.IsValid)
            {
                var prodPrice = ECommerceService.SaveProductPriceTimeWindow(model.Price, model.StartDate, model.EndDate, model.ProductPriceID, 0);
                return Json(
                    (prodPrice != null) ?
                        new { success = true, priceID = prodPrice.TW_ProductPriceID } :
                        new { success = false, priceID = 0 });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult UpdateTimeWindowToProductPrice(TimeWindowModel model)
        {
            if (ModelState.IsValid && model.ProductPriceID != 0 && model.ProductPriceTimeWindowID != 0)
            {
                var prodPrice = ECommerceService.SaveProductPriceTimeWindow(model.Price, model.StartDate, model.EndDate, model.ProductPriceID, model.ProductPriceTimeWindowID);
                return Json((prodPrice != null) ?
                       new { success = true, priceID = prodPrice.TW_ProductPriceID } :
                       new { success = false, priceID = 0 });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteTimeWindow(int timeWindowID)
        {
            return Json(new { success = ECommerceService.DeleteProductPriceTimeWindow(timeWindowID) });
        }

        [HttpPost]
        public ActionResult DeleteAllStockUnits(int productID)
        {
            return Json(new { success = ECommerceService.DeleteAllProductStockUnits(productID) });
        }

        [HttpPost]
        public JsonResult DeleteSelectedStockUnits(int[] priceID)
        {
            return Json(new { success = ECommerceService.DeleteProductPrice(priceID) });
        }

        [HttpPost]
        public ActionResult RemoveStockUnitFromProduct(int priceID)
        {
            return Json(new { success = ECommerceService.DeleteProductPrice(priceID) });
        }

        [HttpPost]
        public ActionResult UpdateProductStockUnit(int productID, int priceID, string barcode, string delivery, bool onSale, string price,
            string RRP, string satePrice, string SKU, int stock, string weight, string endDate, string startDate, string priceForRegular, int[] attrValues)
        {
            return Json(new { success = ECommerceService.SaveProductStockUnit(productID, priceID, barcode, delivery, onSale, price, RRP, satePrice, SKU, stock, weight, endDate, startDate, priceForRegular, attrValues) != null });
        }

        public ActionResult ProductImages(int productID)
        {
            return PartialView(ECommerceService.GetProductByID(productID));
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult SaveProductImage(HttpPostedFileBase file, string description, string altText, int productID)
        {
            return Json(new { success = SaveProdImage(file, description, altText, productID) != null }, "text/html");
        }

        [HttpPost]
        [EditContentAuthorize]
        public ActionResult DeleteProductImage(int imageID)
        {
            return Json(new { success = DeleteProdImage(imageID) });
        }

        public ActionResult ProductAssociations(int productID)
        {
            var product = ECommerceService.GetProductByID(productID);
            var productAss = ECommerceService.GetAllProducts()
                .Except(new List<tbl_Products>() { product });

            this.ViewBag.Products = (productAss != null) ?
                    productAss.Select(p => new SelectListItem
                    {
                        Selected = false,
                        Text = (p.P_ProductTypeID == (int)ProductType.Item) ?
                            p.P_Title + " (p)" :
                            p.P_Title + " (e)",
                        Value = p.ProductID.ToString()
                    }).ToList() :
                    new List<SelectListItem>();
            return PartialView(product);
        }

        [HttpPost]
        public ActionResult SaveProductAssociation(int productID, int associatedProductID)
        {
            return Json(new { success = ECommerceService.SaveAssociatedProduct(productID, associatedProductID) != null });
        }

        [HttpPost]
        public ActionResult DeleteProductAssociation(int productID, int associatedProductID)
        {
            return Json(new { success = ECommerceService.DeleteProductAssociation(productID, associatedProductID) });
        }

        #endregion

        #region Customer

        [AccessRightsAuthorizeAttribute]
        public ActionResult Customer()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewCustomer()
        {
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList(0);
            return PartialView();
        }

        public ActionResult CustomersLeftMenu(string search, string character, bool isRegistered, bool isUnRegistered, int domainID = 0, bool isDormant = false)
        {
            List<CMSMenuModel> model = LeftMenuModelMapper.MapCustomers(UserService.SearchCustomers(search, character, domainID, isRegistered, isUnRegistered, isDormant).OrderBy(c => c.CU_Surname).ThenBy(c => c.CU_FirstName).ToList());
            return PartialView("LeftMenu", model);
        }

        public ActionResult GetCustomersCSV(string search, bool isRegistered, bool isUnRegistered, int domainID = 0, bool isDormant = false)
        {
            List<tbl_Customer> model = UserService.SearchCustomers(search, String.Empty, domainID, isRegistered, isUnRegistered, isDormant).OrderBy(c => c.CU_Surname).ThenBy(c => c.CU_FirstName).ToList();

            List<string> result = PrepareCustomersCSVFile(model);

            return new FileContentResult(CSVWriter.WriteFileContent(result), "text/csv") { FileDownloadName = SettingsManager.CSVCustomersFile };
        }

        [HttpPost]
        public ActionResult AddNewCustomer(EditCustomerAdminModel model)
        {
            ModelState.Clear();
            model.Address.FirstName = model.FirstName;
            model.Address.Surname = model.Surname;

            if (TryValidateModel(model) && TryValidateModel(model.Address))
            {
                if (!UserService.CanUseCustomerEmail(model.Email, model.DomainID))
                    return Json(new { success = false, error = "This email is already registered in our database." });

                if (String.IsNullOrEmpty(model.Password))
                    return Json(new { success = false, error = "Please provide password for new customer." });

                var domain = DomainService.GetDomainByID(model.DomainID);
                if (domain == null)
                    return Json(new { success = false, error = "We can not find selected domain in our database." });

                var customer = UserService.SaveCustomer(model.Email, model.FirstName, model.Surname, model.Phone, model.Title, 
                    model.Password, model.DomainID, 0, true, model.DetailsFor3rdParties, model.adminNote);
                if (customer != null && model.Address != null)
                    ECommerceService.SaveAddress(customer.CustomerID, model.Address.CountryID, model.Address.County, customer.CU_FirstName, customer.CU_Surname, customer.CU_Title, 
                        model.Address.Address1, model.Address.Address2, model.Address.Address3, model.Address.Postcode, model.Address.Phone, model.Address.Town, 0);

                if (domain.IsAnyCRMEnabled)
                    UserService.SubscribeNewsletter(model.Email, model.SubscribeNewsletter, model.DomainID);


                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Please check if all provided values are correct." });
        }

        [HttpPost]
        public ActionResult UpdateCustomer(EditCustomerAdminModel model)
        {
            ModelState.Clear();
            model.Address.FirstName = model.FirstName;
            model.Address.Surname = model.Surname;

            if (TryValidateModel(model) && TryValidateModel(model.Address))
            {
                var domain = DomainService.GetDomainByID(model.DomainID);
                if (domain == null)
                    return Json(new { success = false, error = "We can not find selected domain in our database." });

                if (model.Registered && !UserService.CanUseCustomerEmail(model.Email, model.DomainID, model.CustomerID))
                    return Json(new { success = false, error = "This email is already registered in our database." });

                var customer = UserService.SaveCustomer(model.Email, model.FirstName, model.Surname, model.Phone, model.Title, model.Password, 
                    model.DomainID, model.CustomerID, model.Registered, model.DetailsFor3rdParties, model.adminNote);
                if (customer != null && model.Address != null)
                    ECommerceService.SaveAddress(customer.CustomerID, model.Address.CountryID, model.Address.County, customer.CU_FirstName, customer.CU_Surname, customer.CU_Title,
                        model.Address.Address1, model.Address.Address2, model.Address.Address3, model.Address.Postcode, model.Address.Phone, model.Address.Town, 0);

                if (domain.IsAnyCRMEnabled)
                    UserService.SubscribeNewsletter(model.Email, model.SubscribeNewsletter, model.DomainID);

                return Json(new { success = true });
            }
            return Json(new { success = false, error = "Please check if all provided values are correct." });
        }

        [HttpPost]
        public ActionResult GetCustomer(int customerID)
        {
            tbl_Customer customer = UserService.GetCustomerByID(customerID);
            return (customer == null) ?
                Json(new { success = false }) :
                Json(new
                {
                    success = true,
                    customer = new
                    {
                        Name = customer.FullName,
                        Details = new EditCustomerAdminModel(customer),
                        Countries = ECommerceService.GetAllCountriesAsSelectList(customer.CU_DomainID),
                        Orders = customer.tbl_Orders.OrderByDescending(m => m.O_Timestamp).Select(o => new
                        {
                            ProductType = o.O_ProductTypeID.GetValueOrDefault(0),
                            OrderID = o.OrderID,
                            OrderDate = (o.O_Timestamp.HasValue) ?
                                        o.O_Timestamp.Value.ToString() :
                                        string.Empty,
                            Price = o.GetPriceString(),
                            Products = o.tbl_OrderContent.Select(oc => new
                            {
                                ProductPriceID = oc.OC_ProdPriceID.ToString(),
                                ProdName = oc.OC_Title,
                                ProdPrice = oc.GetItemPriceString(),
                                Amount = oc.OC_Quantity.GetValueOrDefault(0).ToString(),
                                TotalPrice = oc.GetPriceString()
                            })
                        })
                    }
                });
        }

        [HttpPost]
        public ActionResult GetCustomerInfo(int customerID)
        {
            tbl_Customer customer = UserService.GetCustomerByID(customerID);
            return (customer == null) ?
                Json(new { success = false }) :
                Json(new
                {
                    success = true,
                    Email = customer.CU_Email,
                    FirstName = customer.CU_FirstName,
                    Surname = customer.CU_Surname,
                    Title = customer.CU_Title,
                    Phone = customer.CU_Telephone,
                    Addresses = customer.tbl_Address.Select(a => new SelectListItem { Text = a.FullText, Value = a.AddressID.ToString() }).ToList()
                });
        }

        [HttpPost]
        public ActionResult GetOrderDetails(int orderID, bool isDonation = false)
        {
            this.ViewBag.IsPopup = true;
            this.ViewBag.IsDonation = isDonation;
            this.ViewBag.OrderType = isDonation ? "Donation" : "Order";
            return PartialView("OrderDetails", ECommerceService.GetOrderByID(orderID));
        }

        [HttpPost]
        public ActionResult ToggleDormantFlag(int customerID)
        {
            var customer = UserService.GetCustomerByID(customerID);
            if (customer == null)
                return Json(new { success = false });

            customer = UserService.SaveDormantFlag(customerID, !customer.CU_IsDormant);
            return Json(new { success = customer != null });
        }

        [HttpPost]
        public ActionResult GetCustomers(int domainID)
        {
            var customers = UserService.GetCustomersByDomainAsSelectList(domainID);
            if (customers != null) 
                return Json(new { success = true, customers });

            return Json(new { success = false });
        }

        #endregion

        #region Address

        [HttpPost]
        public ActionResult GetAddressesByCustomer(int customerID)
        {
            tbl_Customer customer = UserService.GetCustomerByID(customerID);
            return (customer == null) ?
                Json(new { success = false, aaData = new { } }) :
                Json(new
                {
                    success = true,
                    aaData = customer.tbl_Address.Select(a => new string[] {
                        a.AddressID.ToString(),
                        a.A_Title,
                        a.A_FirstName, 
                        a.A_Surname,
                        a.A_Phone,
                        a.A_Line1, 
                        a.A_Line2, 
                        a.A_Line3,
                        a.A_Town,
                        a.A_County,
                        a.A_Postcode,
                        a.tbl_Country != null ? a.tbl_Country.C_Country : a.A_Country
                    })
                });
        }

        [HttpPost]
        public ActionResult GetAddressByID(int addressID)
        {
            tbl_Address address = ECommerceService.GetAddressByID(addressID);
            return (address == null) ?
                Json(new { success = false }) :
                Json(new
                {
                    success = true,
                    Address1 = address.A_Line1,
                    Address2 = address.A_Line2,
                    Address3 = address.A_Line3,
                    Town = address.A_Town,
                    PostCode = address.A_Postcode,
                    County = address.A_County,
                    Country = address.A_Country,
                    CountryID = address.A_CountryID
                });
        }

        [HttpPost]
        public ActionResult AddAddress(AddressModel address, int customerID)
        {
            if (ModelState.IsValid)
            {
                var customer = UserService.GetCustomerByID(customerID);
                if (customer == null)
                    return Json(new { success = false, error = "System can not find customer." });

                var addr = ECommerceService.SaveAddress(customerID, address.CountryID, address.County, address.FirstName, address.Surname,
                    address.Title, address.Address1, address.Address2, address.Address3, address.Postcode, address.Phone, address.Town, 0);

                return Json(new { success = addr != null });
            }
            var errorMessage = ModelState.Aggregate(String.Empty, (message, property) => (message += property.Value.Errors.Aggregate(String.Empty, (errors, error) => (errors += error.ErrorMessage))));
            return Json(new { success = false, error = "Please check if all provided values are correct. " + errorMessage });
        }

        [HttpPost]
        public ActionResult UpdateAddress(AddressModel address, int customerID)
        {
            if (ModelState.IsValid && address.AddressID != 0)
            {
                var customer = UserService.GetCustomerByID(customerID);
                if (customer == null)
                    return Json(new { success = false, error = "System can not find customer." });

                var country = ECommerceService.GetCountry(customer.CU_DomainID, address.Country);

                ECommerceService.SaveAddress(customerID, country != null ? country.CountryID : 0, address.County, address.FirstName, address.Surname,
                    address.Title, address.Address1, address.Address2, address.Address3, address.Postcode, address.Phone, address.Town, address.AddressID);

                return Json(new { success = true });
            }
            var errorMessage = ModelState.Aggregate(String.Empty, (message, property) => (message += property.Value.Errors.Aggregate(String.Empty, (errors, error) => ( errors += error.ErrorMessage ))));
            return Json(new { success = false, error = "Please check if all provided values are correct. " + errorMessage });
        }

        #endregion

        #region Order

        [AccessRightsAuthorizeAttribute]
        public ActionResult Orders()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewOrder()
        {
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList(0);
            this.ViewBag.CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>().Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();

            return PartialView();
        }

        public ActionResult OrdersLeftMenu(string search, string startDate, string endDate, int? domainID, int? statusID, int? categoryID, int? productID, int typeID = 0)
        {
            List<CMSMenuModel> model = new List<CMSMenuModel>();
            
            if (typeID == (int)OrderType.Donation)
            {
                List<tbl_Orders> orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, null, null, ProductType.Donation);
                model = LeftMenuModelMapper.MapDonations(orders);
            }
            else
            {
                if (string.IsNullOrEmpty(search) && startDate == null && endDate == null && statusID == null & categoryID == null && productID == null)
                    startDate = DateTime.Now.AddMonths(-3).ToShortDateString();

                List<tbl_Orders> orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, categoryID, productID);
                model = LeftMenuModelMapper.MapOrders(orders);
            }

            return PartialView("LeftMenu", model);
        }

        public ActionResult OrdersSummary(string search, string startDate, string endDate, int? domainID, int? statusID, int? categoryID, int? productID, int typeID = 1)
        {
            List<tbl_Orders> orders = new List<tbl_Orders>();

            if (typeID == (int)OrderType.Donation)
            {
                orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, null, null, ProductType.Donation);
                this.ViewBag.Donation = true;
            }
            else
            {
                orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, categoryID, productID);
            }
            
            return PartialView(orders);
        }

        public ActionResult GetEmailsAsCSV(string search, string startDate, string endDate, int? domainID, int? statusID, int? categoryID, int? productID)
        {
            List<tbl_Orders> orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, categoryID, productID);

            return new FileContentResult(CSVWriter.WriteFileContent(orders.Select(o => o.CustomerEMail).Distinct().ToList()), "text/csv") { FileDownloadName = SettingsManager.CSVEmailFile };
        }

        public ActionResult GetOrderSummaryAsCSV(string search, string startDate, string endDate, int? domainID, int? statusID, int? categoryID, int? productID, int? typeID)
        {
            List<tbl_Orders> orders = new List<tbl_Orders>();
            StringBuilder sb = new StringBuilder();
            

            if (typeID == (int)OrderType.Donation)
            {
                orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, null, null, ProductType.Donation);
                sb.AppendLine("DONATION ID, TIMESTAMP, CUSTOMER NAME, CUSTOMER EMAIL, DONATION STATUS, TOTAL");
            }
            else
            {
                orders = ECommerceService.SearchOrders(search, startDate, endDate, domainID, statusID, categoryID, productID);
                sb.AppendLine("ORDER ID, TIMESTAMP, CUSTOMER NAME, CUSTOMER EMAIL, ORDER STATUS, CATEGORY, PRODUCT CODE, PRODUCT, QUANTITY, PRICE, TOTAL");
            }

            List<string> orderLines = new List<string>();
            
            foreach (tbl_Orders order in orders)
            {
                if (typeID == (int)OrderType.Donation)
                {
                    sb.Append(order.OrderID.ToString() + ",");
                    sb.Append(order.O_Timestamp.ToString() + ",");
                    sb.Append((order.tbl_Customer != null ? order.BillingFullName : String.Empty) + ",");
                    sb.Append(order.CustomerEMail.ToString() + ",");
                    sb.Append(order.CurrentOrderStatus.ToString() + ",");
                    sb.Append(order.GetPriceString().Replace(',', '.')).Append(Environment.NewLine);
                }
                else
                {
                    List<tbl_OrderContent> orderContent = order.tbl_OrderContent.ToList();
                    if (orderContent.Count > 0)
                    {
                        foreach (tbl_OrderContent orderitem in orderContent)
                        {
                            sb.Append(order.OrderID.ToString() + ",");
                            sb.Append(order.O_Timestamp.ToString() + ",");
                            sb.Append((order.tbl_Customer != null ? order.BillingFullName : String.Empty) + ",");
                            sb.Append(order.CustomerEMail.ToString() + ",");
                            sb.Append(order.CurrentOrderStatus.ToString() + ",");
                            sb.Append((orderitem.tbl_Products != null ? orderitem.tbl_Products.tbl_ProdCategories.PC_Title : String.Empty) + ",");
                            sb.Append((orderitem.tbl_Products != null ? orderitem.tbl_Products.P_ProductCode : String.Empty) + ",");
                            sb.Append(orderitem.OC_Title + ",");
                            sb.Append(orderitem.OC_Quantity.ToString() + ",");
                            sb.Append(orderitem.OC_Price.ToString().Replace(',', '.') + ",");
                            sb.Append(orderitem.OC_TotalPrice.ToString().Replace(',', '.')).Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        sb.Append(order.OrderID.ToString() + ",");
                        sb.Append(order.O_Timestamp.ToString() + ",");
                        sb.Append((order.tbl_Customer != null ? order.BillingFullName : String.Empty) + ",");
                        sb.Append(order.CustomerEMail.ToString() + ",");
                        sb.Append(order.CurrentOrderStatus.ToString() + ",");
                        sb.Append(",,,,,").Append(Environment.NewLine);
                    }
                }
            }
            orderLines.Add(sb.ToString());
            return new FileContentResult(CSVWriter.WriteFileContent(orderLines),"text/csv") { FileDownloadName=SettingsManager.CSVOrderSummaryFile };
        }

        [HttpPost]
        public JsonResult GetOrderStatuses(int statusID = 0)
        {
            var statuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString(), Selected = (int)c == statusID }).OrderBy(l => l.Text).ToList();
            return Json(new { success = true, statuses = statuses });
        }

        public ActionResult OrderDetails(int orderID, bool isDonation = false)
        {
            this.ViewBag.IsPopup = false;
            this.ViewBag.Methodes = ECommerceService.GetDespatchMethodes();
            this.ViewBag.IsDonation = isDonation;
            this.ViewBag.OrderType = isDonation ? "Donation" : "Order";
            return PartialView(ECommerceService.GetOrderByID(orderID));
        }

        public ActionResult OrderDeliveryDetails(int orderID)
        {
            return PartialView(ECommerceService.GetOrderByID(orderID));
        }

        [HttpPost]
        public ActionResult CreateBasket(int domainID)
        {
            if (SessionManager.AdminBasket != null)
            {
                ECommerceService.DeleteBasket(SessionManager.AdminBasket.BasketID);
            }

            SessionManager.AdminBasket = ECommerceService.SaveBasket(0, null, SessionManager.SessionID, domainID, 0);

            var customers = UserService.GetCustomersExtendedByDomainAsSelectList(domainID);
            var bcountries = ECommerceService.GetAllCountriesAsSelectList(domainID);
            var dcountries = ECommerceService.GetPostageCountriesAsSelectList(domainID);
            var payments = ECommerceService.GetAllPaymentDomainAsSelectList(domainID, true);
            var sagePayDomainID = ECommerceService.GetPaymentDomainIDByCode(domainID, PaymentType.SagePay);
            var isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, domainID) == SagePayPaymentType.Direct.ToString();

            return Json(new { success = SessionManager.AdminBasket != null, customers, billingCountries = bcountries, deliveryCountries = dcountries, payments, sagePayDomainID, isDirect });
        }

        [HttpPost]
        public ActionResult GetProductCategoriesByDomainID(int domainID = 0)
        {
            var categories = ECommerceService.GetProdCategoriesForDomainAsSelectList(domainID, 0, ProductType.AllProducts);
            foreach (var category in categories)
            {
                int level = category.Level;
                while (--level > 0)
                    category.Text = "\xA0\xA0" + category.Text;
            }

            return Json(new { success = true, categories = categories });
        }

        [HttpPost]
        public ActionResult GetProductsByCategory(int categoryID)
        {
            var category = ECommerceService.GetProdCategoryByID(categoryID);
            if (category == null)
                return Json(new { success = false });

            return Json(new { 
                success = true,
                products = new SelectList(category.tbl_Products.Where(p => !p.P_Deleted).Select(p => new { id = p.ProductID, text = "[" + p.P_ProductCode + "] " + p.P_Title }), "id", "text") 
            });
        }

        [HttpPost]
        public ActionResult GetPricesByProduct(int productID)
        {
            var product = ECommerceService.GetProductByID(productID);
            if (product == null)
                return Json(new { success = false });

            List<SelectListItem> prices = new List<SelectListItem>();
            foreach (var price in product.tbl_ProductPrice)
            {
                string text = String.Empty;
                string seed = String.Empty;
                string attributes = price.tbl_ProdPriceAttributes.Select(ppa => new
                    {
                        name = ppa.tbl_ProdAttValue.tbl_ProdAttributes.A_Title,
                        value = ppa.tbl_ProdAttValue.AV_Value
                    }).Aggregate(seed, (total, item) => (total += String.Format("{0}:{1};", item.name, item.value)));

                if (price.tbl_Products.tbl_ProductTypes.PT_Name.Equals(ProductType.Event.ToString()) && price.PR_EventStartDate.HasValue)
                {
                    string date = "Start date:" + price.PR_EventStartDate.Value.ToCustomDateTimeString() + ";";
                    date += price.PR_EventEndDate.HasValue ? "End date:" + price.PR_EventEndDate.Value.ToCustomDateTimeString() + ";" : String.Empty;
                    attributes += date;
                }
                text += String.IsNullOrEmpty(attributes) ? String.Empty : attributes + ", ";
                if (product.P_StockControl)
                    text += String.Format("Price: {1}, Amount: {2}", attributes, price.GetPriceString(), price.PR_Stock.GetValueOrDefault(0));
                else
                    text += String.Format("Price: {1}", attributes, price.GetPriceString());

                prices.Add(new SelectListItem { Text = text, Value = price.PriceID.ToString() });
            }


            return Json(new
            {
                success = true,
                prices = new SelectList(prices, "Value", "Text")
            });
        }

        [HttpPost]
        public ActionResult AddDiscountToBasket(string discountCode)
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            if (String.IsNullOrEmpty(discountCode))
                return Json(new { success = false, error = "Please specify promotional code" });

            if (ECommerceService.GetDiscountByCode(discountCode, SessionManager.AdminBasket.B_DomainID) == null)
                return Json(new { success = false, error = "Promotional code is incorrect." });

            var basket = ECommerceService.AddDiscountToBasket(SessionManager.AdminBasket.BasketID, discountCode, SessionManager.AdminBasket.B_DomainID);
            if (basket == null || basket.tbl_Discount == null)
                return Json(new { success = false, error = "Promotional code is incorrect" });

            SessionManager.AdminBasket = basket;

            return Json(new
            {
                success = true,
                isDiscount = SessionManager.AdminBasket.tbl_Discount != null,
                discount = SessionManager.AdminBasket.GetDiscountAmountString(),
                discountText = String.Format(basket.tbl_Discount.D_IsPercentage ? "{0} ({1}%)" : "{0} ({1:C})", basket.tbl_Discount.D_Title, basket.tbl_Discount.D_Value),
                totalPrice = SessionManager.AdminBasket.GetPriceString()
            });
        }

        [HttpPost]
        public ActionResult RemoveDiscountFromBasket()
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            var basket = ECommerceService.RemoveDiscountFromBasket(SessionManager.AdminBasket.BasketID);
            if (basket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            SessionManager.AdminBasket = basket;
            return Json(new { success = true, totalPrice = SessionManager.AdminBasket.GetPriceString() });
        }

        [HttpPost]
        public ActionResult AddProductToOrder(int priceID, int amount)
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            var price = ECommerceService.GetProductPriceByID(priceID);
            if (price == null)
                return Json(new { success = false, error = "Please select product." });

            if (amount <= 0)
                return Json(new { success = false, error = "Please specify correct amount." });

            string selectedDate = null;
            if (price.tbl_Products.tbl_ProductTypes.PT_Name.Equals(ProductType.Event))
                selectedDate = price.PR_EventStartDate.GetValueOrDefault().ToString() + (price.PR_EventEndDate.HasValue ? "_" + price.PR_EventEndDate.Value.ToString() : String.Empty);

            var basket = ECommerceService.AddContentToBasket(SessionManager.AdminBasket.BasketID, price.PR_ProductID, amount, price.tbl_ProdPriceAttributes.Select(pp => pp.PPA_ProdAttValID).ToArray(), selectedDate);
            if (basket == null)
                return Json(new { success = false, error = "There are not enough products in our stock or if it is event, it has already started or ended." });

            SessionManager.AdminBasket = basket;

            List<object> products = new List<object>();
            foreach (var content in SessionManager.AdminBasket.tbl_BasketContent)
            {
                if (content.tbl_ProductPrice != null)
                {
                    string seed = String.Empty;
                    string attributes = content.tbl_ProductPrice.tbl_ProdPriceAttributes.Select(ppa => new
                        {
                            name = ppa.tbl_ProdAttValue.tbl_ProdAttributes.A_Title,
                            value = ppa.tbl_ProdAttValue.AV_Value
                        }).Aggregate(seed, (total, item) => (total += String.Format("{0}:{1};", item.name, item.value)));

                    if (content.tbl_ProductPrice.tbl_Products.tbl_ProductTypes.PT_Name.Equals(ProductType.Event.ToString()) && content.tbl_ProductPrice.PR_EventStartDate.HasValue)
                    {
                        string date = "Start date:" + content.tbl_ProductPrice.PR_EventStartDate.Value.ToCustomDateTimeString() + ";";
                        date += content.tbl_ProductPrice.PR_EventEndDate.HasValue ? "End date:" + content.tbl_ProductPrice.PR_EventEndDate.Value.ToCustomDateTimeString() + ";" : String.Empty;
                        attributes += date;
                    }

                    products.Add(new
                    {
                        name = content.BC_Title,
                        attributes = attributes,
                        amount = content.BC_Quantity,
                        price = content.GetPriceString(),
                        itemPrice = content.GetItemPriceString(),
                        basketContent = content.BaskContentID
                    });
                }
            }

            return Json(new
            {
                success = true,
                products = products,
                totalPrice = SessionManager.AdminBasket.GetPriceString(),
                discountPrice = SessionManager.AdminBasket.GetDiscountAmountString(),
                deliveryPrice = SessionManager.AdminBasket.GetDeliveryAmountString()
            });
        }

        [HttpPost]
        public ActionResult DeleteProductFromOrder(int basketContentID)
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            if (!ECommerceService.DeleteBasketContent(basketContentID))
                return Json(new { success = false, error = "There is no such product in basket." });

            SessionManager.AdminBasket = ECommerceService.GetBasketByID(SessionManager.AdminBasket.BasketID);

            List<object> products = new List<object>();
            foreach (var content in SessionManager.AdminBasket.tbl_BasketContent)
            {
                if (content.tbl_ProductPrice != null)
                {
                    string seed = String.Empty;
                    string attributes = content.tbl_ProductPrice.tbl_ProdPriceAttributes.Select(ppa => new
                        {
                            name = ppa.tbl_ProdAttValue.tbl_ProdAttributes.A_Title,
                            value = ppa.tbl_ProdAttValue.AV_Value
                        }).Aggregate(seed, (total, item) => (total += String.Format("{0}:{1};", item.name, item.value)));

                    if (content.tbl_ProductPrice.tbl_Products.tbl_ProductTypes.PT_Name.Equals(ProductType.Event.ToString()) && content.tbl_ProductPrice.PR_EventStartDate.HasValue)
                    {
                        string date = "Start date:" + content.tbl_ProductPrice.PR_EventStartDate.Value.ToCustomDateTimeString() + ";";
                        date += content.tbl_ProductPrice.PR_EventEndDate.HasValue ? "End date:" + content.tbl_ProductPrice.PR_EventEndDate.Value.ToCustomDateTimeString() + ";" : String.Empty;
                        attributes += date;
                    }

                    products.Add(new
                    {
                        name = content.BC_Title,
                        attributes = attributes,
                        amount = content.BC_Quantity,
                        price = content.GetPriceString(),
                        itemPrice = content.GetItemPriceString(),
                        basketContent = content.BaskContentID
                    });
                }
            }
            return Json(new
            {
                success = true,
                products = products,
                totalPrice = SessionManager.AdminBasket.GetPriceString(),
                discountPrice = SessionManager.AdminBasket.GetDiscountAmountString(),
                deliveryPrice = SessionManager.AdminBasket.GetDeliveryAmountString()
            });
        }

        [HttpPost]
        public ActionResult SaveCustomerInfoForOrder(NewOrderModel model)
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(model.VatNumber))
                    ECommerceService.SaveEUVATForBasket(SessionManager.AdminBasket.BasketID, model.VatNumber);

                SessionManager.AdminBasket = ECommerceService.UpdateBasket(model.BillingAddress1, model.BillingAddress2, model.BillingAddress3, model.BillingCity,
                    model.BillingCountryID, model.BillingTitle, model.BillingFirstName, model.BillingPhone, model.BillingPostcode, model.BillingState, model.BillingSurname, 
                    model.DeliveryAddress1, model.DeliveryAddress2, model.DeliveryAddress3, model.DeliveryCity, string.Empty, model.DeliveryCountryID, model.DeliveryTitle, 
                    model.DeliveryFirstName, model.DeliveryPhone, model.DeliveryPostcode, model.DeliveryState, model.DeliverySurname, String.Empty, model.CustomerEmail, 
                    model.CustomerID.GetValueOrDefault(0), model.BillingAddressTheSame, false, false, SessionManager.AdminBasket.BasketID);

                SelectList postages = new SelectList(new List<SelectListItem>());
                int? zoneID = ECommerceService.GetPostageZoneID(model.DeliveryCountryID);
                if (zoneID.HasValue)
                {
                    string postageType = DomainService.GetSettingsValue(SettingsKey.postageTypeSetting, this.DomainID);
                    postages = ECommerceService.GetAvailablePostageAsSelectList(SessionManager.AdminBasket.GetPrice(), SessionManager.AdminBasket.GetWeight(),
                        SessionManager.AdminBasket.B_DomainID, postageType, zoneID.GetValueOrDefault(0));
                }

                return Json(new { success = true, postages = postages });
            }
            return Json(new { success = false, error = "There was an issue with model, please ensure that all fields are correctly filled in." });
        }

        [HttpPost]
        public ActionResult SavePostageForOrder(int? postageID)
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            var basket = ECommerceService.UpdateBasketPostage(postageID, SessionManager.AdminBasket.BasketID);
            if (basket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            SessionManager.AdminBasket = basket;
            return Json(new
            {
                success = true,
                totalPrice = basket.GetPriceString(),
                discountPrice = basket.GetDiscountAmountString(),
                deliveryPrice = basket.GetDeliveryAmountString()
            });
        }

        [HttpPost]
        public ActionResult SaveNewOrder(string instructions, bool isPayment = false, int paymentDomainID = 0, CreditCardModel creditCardInfo = null,
            bool isCustomPrice = false, string customPrice = "", int? addressID = null, CashPayment cashPayment = CashPayment.Cash)
        {
            if (SessionManager.AdminBasket == null)
                return Json(new { success = false, error = "System cannot find your basket." });

            if (!ECommerceService.IsEnoughOnStock(SessionManager.AdminBasket.tbl_BasketContent))
                return Json(new { success = false, error = "Quantity of order items exceeds current stock ammount." });

            SessionManager.AdminBasket = ECommerceService.UpdateBasketDeliveryNotes(instructions, SessionManager.AdminBasket.BasketID);

            tbl_Orders order = ECommerceService.SaveOrder(0, null, isPayment ? (int?)null : (int)cashPayment, SessionManager.AdminBasket.BasketID, AdminUser.UserID, addressID.GetValueOrDefault(0));

            if (order == null)
                return Json(new { success = false, error = "There was a problem saving new order." });

            if (isCustomPrice)
            {
                decimal price = 0;
                bool parsed = Decimal.TryParse(customPrice.ChangeDecimalSeparator(), out price);
                if (parsed)
                    ECommerceService.SaveCustomTotalAmount(price, order.OrderID);
            }

            if (isPayment)
            {
                var selectedPaymentType = ECommerceService.GetPaymentDomainByID(paymentDomainID);
                if (selectedPaymentType == null)
                {
                    return Json(new { success = false, error = "Please select payment type." });
                }

                var isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, SessionManager.AdminBasket.B_DomainID) == SagePayPaymentType.Direct.ToString();
                PaymentType key = (PaymentType)Enum.Parse(typeof(PaymentType), selectedPaymentType.tbl_PaymentType.PT_Code);
                switch (key)
                {
                    case PaymentType.SagePay:
                        if (isDirect)
                        {
                            if (creditCardInfo == null)
                                return Json(new { success = false, error = "Please fill in credit card information." });

                            SessionManager.CreditCard = creditCardInfo;
                        }

                        return Json(new { success = true, url = Url.RouteUrl("SagePay", new { action = "Payment", orderID = order.OrderID }) });

                    case PaymentType.PayPal:
                        return Json(new { success = true, url = Url.RouteUrl("PayPal", new { action = "Payment", orderID = order.OrderID }) });

                    case PaymentType.SecureTrading:

                        return Json(new { success = true, url = Url.RouteUrl("SecureTrading", new { action = "Payment", orderID = order.OrderID }) });
                }
            }
            else
            {
                order = ECommerceService.UpdateOrderPaymentStatus(order.OrderID, PaymentStatus.Paid);
                var domain = DomainService.GetDomainByID(SessionManager.AdminBasket.B_DomainID);
                MailingService.SendOrderConfirmationAdmin(order, domain != null ? domain.DO_Email : String.Empty);
                MailingService.SendOrderConfirmation(order);
            }

            if (order == null)
                return Json(new { success = false, error = "There was a problem updating payment status for order." });

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult SaveNewDespatchMethod(string method)
        {
            return Json(new { success = ECommerceService.SaveNewDespathMethod(method) != null });
        }

        [HttpPost]
        public ActionResult UpdateOrderTracking(int despathMethodID, string deliveryDate, string trackingUrl, string trackingRef, int orderID)
        {
            return Json(new { success = ECommerceService.UpdateOrderTracking(despathMethodID, deliveryDate, trackingUrl, trackingRef, orderID) != null });
        }

        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderID, int statusID)
        {
            return Json(new { success = ECommerceService.UpdateOrderStatus(orderID, statusID) != null });
        }

        [HttpPost]
        public ActionResult DeleteOldBaskets()
        {
            return Json(new { success = ECommerceService.DeleteUnusedBaskets() });
        }

        [HttpPost]
        public ActionResult CancelOrder(int orderID, bool isEmail = false)
        {
            var order = ECommerceService.CancelOrder(orderID);
            if (order != null && order.CurrentOrderStatus == OrderStatus.Canceled)
            {
                if (isEmail)
                    MailingService.SendOrderCancelConfirmation(order);
                
                return Json(new { success = true });
            }
            else
                return Json(new { success = false });
        }

        #endregion

        #region Tax rates

        [AccessRightsAuthorizeAttribute]
        public ActionResult TaxRates()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetTax(int taxID)
        {
            tbl_Tax tax = ECommerceService.GetTaxByID(taxID);
            return (tax == null) ?
                Json(new { success = false }) :
                Json(new
                {
                    success = true,
                    title = tax.TA_Title,
                    taxID = tax.TaxID,
                    percentage = tax.TA_Percentage.GetValueOrDefault(0)
                });
        }

        [HttpPost]
        public ActionResult AddTax(string title, string percentage)
        {
            var per = Decimal.Parse(percentage);
            if (ECommerceService.CanAddTax(title, per))
            {
                tbl_Tax tax = ECommerceService.SaveTax(title, per, 0);
                return Json((tax != null) ?
                    new { success = true, taxID = tax.TaxID } :
                    new { success = false, taxID = 0 }
                    );
            }
            return Json(new { success = false, error = "This values are already used for another tax rate." });
        }

        [HttpPost]
        public ActionResult UpdateTax(string title, string percentage, int taxID)
        {
            if (taxID == 0)
                return Json(new { success = false });
            var per = Decimal.Parse(percentage);
            if (ECommerceService.CanAddTax(title, per, taxID))
            {
                tbl_Tax tax = ECommerceService.SaveTax(title, per, taxID);
                return Json((tax != null) ?
                    new { success = true, taxID = tax.TaxID } :
                    new { success = false, taxID = 0 }
                    );
            }
            return Json(new { success = false, error = "This values are already used for another tax rate." });
        }

        [HttpPost]
        public ActionResult DeleteTax(int taxID)
        {
            return Json(new { success = ECommerceService.DeleteTax(taxID) });
        }

        #endregion

        #region EventTypes

        [AccessRightsAuthorizeAttribute]
        public ActionResult EventTypes()
        {
            return View();
        }

        public ActionResult EventTypesSettings(int eventTypeID = 0)
        {
            tbl_EventTypes eventType = ECommerceService.GetEventTypeByID(eventTypeID);
            return PartialView(eventType);
        }

        [HttpPost]
        public JsonResult GetEventType(int eventTypeID)
        {
            var eventType = ECommerceService.GetEventTypeByID(eventTypeID);
            return (eventType != null) ?
                Json(new
                {
                    success = true,
                    eventType = new EventTypeModel
                    {
                        Title = eventType.ET_Title,
                        Description = eventType.ET_Description,
                        EventTypeID = eventType.EventTypeID
                    }
                }) :
                Json(new { success = false });
        }

        [HttpPost]
        public JsonResult SaveEventType(EventTypeModel model)
        {
            tbl_EventTypes eventType = ECommerceService.SaveEventType(model.Title, model.Description, model.EventTypeID);
            return Json((eventType != null) ?
                    new { success = true, eventTypeID = eventType.EventTypeID } :
                    new { success = false, eventTypeID = 0 }
                );
        }

        [HttpPost]
        public ActionResult AddEventType(tbl_EventTypes model)
        {
            if (model.EventTypeID > 0)
                return Json(new { success = false });

            tbl_EventTypes eventType = ECommerceService.SaveEventType(model.ET_Title, model.ET_Description, 0);
            return Json((eventType != null) ?
                    new { success = true, eventTypeID = eventType.EventTypeID } :
                    new { success = false, eventTypeID = 0 }
                );
        }

        [HttpPost]
        public ActionResult UpdateEventType(tbl_EventTypes model)
        {
            if (model.EventTypeID < 1)
                return Json(new { success = false });

            tbl_EventTypes eventType = ECommerceService.SaveEventType(model.ET_Title, model.ET_Description, model.EventTypeID);
            return Json((eventType != null) ?
                    new { success = true, eventTypeID = eventType.EventTypeID } :
                    new { success = false, eventTypeID = 0 }
               );
        }

        [HttpPost]
        public ActionResult DeleteEventType(int eventTypeID)
        {
            return Json(new { success = ECommerceService.DeleteEventType(eventTypeID) });
        }

        public ActionResult EventTypeImage(int eventTypeID)
        {
            tbl_EventTypes eventType = ECommerceService.GetEventTypeByID(eventTypeID);
            return PartialView(eventType);
        }

        [HttpPost]
        public JsonResult SaveEventTypeImage(HttpPostedFileBase file, int eventTypeID)
        {
            if (file != null)
            {
                var eventType = ECommerceService.GetEventTypeByID(eventTypeID);
                if (eventType == null)
                    return Json(new { success = false });

                DeleteFile(eventType.ET_ImagePath);

                var path = String.Format("{0}{1}{2}", SettingsManager.EventTypeIcon.Path, eventTypeID, Path.GetFileName(file.FileName));
                if (SaveImageFile(file, path))
                {
                    eventType = ECommerceService.UpdateEventTypeImagePath(path, eventTypeID);
                    return Json(new { success = eventType != null }, "text/html");
                }
            }
            return Json(new { success = false }, "text/html");
        }

        [HttpPost]
        public ActionResult DeleteEventTypeImage(int eventTypeID)
        {
            var eventType = ECommerceService.GetEventTypeByID(eventTypeID);
            if (eventType == null)
                return Json(new { success = false });

            DeleteFile(eventType.ET_ImagePath);
            ECommerceService.UpdateEventTypeImagePath(String.Empty, eventTypeID);
            return Json(new { success = true });
        }

        #endregion

        #region POI

        [AccessRightsAuthorizeAttribute]
        public ActionResult POIs()
        {
            this.ViewBag.Categories = POIService.GetAllPOICategoriesAsSelectList();
            return View();
        }

        [HttpPost]
        public ActionResult GetPOI(int poiID)
        {
            var poi = POIService.GetPOIByID(poiID);
            return (poi == null) ?
                Json(new { success = false, poi = new POIModel() }) :
                Json(new
                {
                    success = true,
                    poi = new POIModel
                    {
                        POIID = poi.POIID,
                        Title = poi.POI_Title,
                        CategoryID = poi.POI_CategoryID,

                        Description = poi.POI_Description,
                        Latitude = poi.POI_Latitude,
                        Longitude = poi.POI_Longitude,
                        Phone = poi.POI_Phone,
                        SitemapID = poi.POI_SitemapID,
                        DomainID = poi.tbl_SiteMap != null ? poi.tbl_SiteMap.SM_DomainID : 0,

                        AddressID = poi.POI_AddressID,
                        Address1 = poi.tbl_Address.A_Line1,
                        Address2 = poi.tbl_Address.A_Line2,
                        Address3 = poi.tbl_Address.A_Line3,
                        Country = poi.tbl_Address.A_Country,
                        County = poi.tbl_Address.A_County,
                        Postcode = poi.tbl_Address.A_Postcode,
                        Town = poi.tbl_Address.A_Town
                    }
                });
        }

        [HttpPost]
        public ActionResult GetPOITagsForPOI(int poiID)
        {
            var groups = POIService.GetAllPOITagsGroups();
            List<POITagsGroupModel> model = new List<POITagsGroupModel>();
            foreach (var item in groups)
            {
                model.Add(new POITagsGroupModel
                {
                    Title = item.POITG_Title,
                    POITagsGroupID = item.POITagsGroupID,
                    POITags = item.tbl_POITags.Select(m => new SelectListItem
                    {
                        Text = m.POIT_Title,
                        Value = m.POITagID.ToString(),
                        Selected = (m.tbl_POI.FirstOrDefault(s => s.POIID == poiID) != null)
                    }).ToList()
                });
            }
            return Json(new { tags = model });
        }

        [HttpPost]
        public ActionResult AddPOI(POIModel model)
        {
            if (ModelState.IsValid)
            {
                var poi = POIService.SavePOI(model.Title, model.Description, model.CategoryID, model.Latitude, model.Longitude, model.Phone, model.AddressID,
                    model.Address1, model.Address2, model.Address3, model.Town, model.Postcode, model.County, model.Country, model.TagsIDs, model.SitemapID, 0);

                return Json((poi != null) ?
                    new { success = true, poiID = poi.POIID } :
                    new { success = false, poiID = 0 }
                );
            }
            return Json(new { sucsess = false, poiID = 0 });
        }

        [HttpPost]
        public ActionResult UpdatePOI(POIModel model)
        {
            if (ModelState.IsValid && model.POIID > 0)
            {
                var poi = POIService.SavePOI(model.Title, model.Description, model.CategoryID, model.Latitude, model.Longitude, model.Phone, model.AddressID,
                    model.Address1, model.Address2, model.Address3, model.Town, model.Postcode, model.County, model.Country, model.TagsIDs, model.SitemapID, model.POIID);

                return Json((poi != null) ?
                    new { success = true, poiID = poi.POIID } :
                    new { success = false, poiID = 0 }
                );
            }
            return Json(new { sucsess = false, poiID = 0 });
        }

        [HttpPost]
        public ActionResult DeletePOI(int poiID)
        {
            return Json(new { success = POIService.DeletePOI(poiID) });
        }

        [HttpPost]
        public ActionResult GetPOIFiles(int poiID)
        {
            var poi = POIService.GetPOIByID(poiID);
            if (poi == null)
                return Json(new { success = false });

            var files = new List<POIFileModel>();
            if (poi.tbl_POIFiles != null)
            {
                foreach (var file in poi.tbl_POIFiles)
                {
                    files.Add(new POIFileModel
                    {
                        Extension = file.POIF_Extension,
                        FileID = file.POIFileID,
                        FileName = file.POIF_Name,
                        Path = Url.Action("GetPOIFile", "Admn", new { fileID = file.POIFileID })
                    });
                }
            }
            return Json(new { success = true, poiID = poi.POIID, files = files });
        }

        public ActionResult GetPOIFile(int fileID)
        {
            var file = POIService.GetPOIFileByID(fileID);
            if (file == null)
                return new HttpNotFoundResult("No file found");

            return new FileContentResult(file.POIF_Content, "application/octet-stream") { FileDownloadName = file.POIF_Name };
        }

        [HttpPost]
        public JsonResult SavePOIFile(int poiID, HttpPostedFileBase file)
        {
            var poi = POIService.GetPOIByID(poiID);
            if (poi == null || file == null)
                return Json(new { success = false });

            string name = Path.GetFileName(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            var dbFile = POIService.SavePOIFile(poiID, file.InputStream, extension, name, 0);

            return Json(new { success = dbFile != null });
        }

        [HttpPost]
        public JsonResult DeletePOIFile(int fileID)
        {
            return Json(new { success = POIService.DeletePOIFile(fileID) });
        }

        #endregion

        #region POI Categories

        [AccessRightsAuthorizeAttribute]
        public ActionResult POICategories()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetPOICategory(int poiCategoryID)
        {
            var poiCategory = POIService.GetPOICategoryByID(poiCategoryID);
            return (poiCategory == null) ?
                Json(new { success = false, category = new POICategoryModel() }) :
                Json(new
                {
                    success = true,
                    category = new POICategoryModel
                    {
                        Title = poiCategory.POIC_Title,
                        POICategoryID = poiCategory.POICategoryID,
                        IsLive = poiCategory.POIC_IsLive
                    }
                });
        }

        [HttpPost]
        public ActionResult AddPOICategory(POICategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var category = POIService.SavePOICategory(model.Title, model.IsLive, 0);
                return Json((category != null) ?
                    new { success = true, poiCategoryID = category.POICategoryID } :
                    new { success = false, poiCategoryID = 0 }
                );
            }
            return Json(new { sucsess = false, poiCategoryID = 0 });
        }

        [HttpPost]
        public ActionResult UpdatePOICategory(POICategoryModel model)
        {
            if (ModelState.IsValid && model.POICategoryID > 0)
            {
                var category = POIService.SavePOICategory(model.Title, model.IsLive, model.POICategoryID);
                return Json((category != null) ?
                    new { success = true, poiCategoryID = category.POICategoryID } :
                    new { success = false, poiCategoryID = 0 }
                );
            }
            return Json(new { sucsess = false, poiCategoryID = 0 });
        }

        [HttpPost]
        public ActionResult DeletePOICategory(int poiCategoryID)
        {
            return Json(new { success = POIService.DeletePOICategory(poiCategoryID) });
        }

        #endregion

        #region POI Tags

        [AccessRightsAuthorizeAttribute]
        public ActionResult POITags()
        {
            this.ViewBag.Groups = POIService.GetAllPOITagsGroupsAsSelectList();
            return View();
        }

        [HttpPost]
        public ActionResult GetPOITag(int poiTagID)
        {
            var tag = POIService.GetPOITagByID(poiTagID);
            return (tag == null) ?
                Json(new { success = false, tag = new POITagModel() }) :
                Json(new
                {
                    success = true,
                    tag = new POITagModel
                    {
                        Title = tag.POIT_Title,
                        POITagID = tag.POITagID,
                        POITagsGroupID = tag.POIT_GroupID
                    }
                });
        }

        [HttpPost]
        public ActionResult AddPOITag(POITagModel model)
        {
            if (ModelState.IsValid)
            {
                var tag = POIService.SavePOITag(model.Title, model.POITagsGroupID, 0);
                return Json((tag != null) ?
                    new { success = true, poiTagID = tag.POITagID } :
                    new { success = false, poiTagID = 0 }
                );
            }
            return Json(new { sucsess = false, poiTagID = 0 });
        }

        [HttpPost]
        public ActionResult UpdatePOITag(POITagModel model)
        {
            if (ModelState.IsValid && model.POITagID > 0)
            {
                var tag = POIService.SavePOITag(model.Title, model.POITagsGroupID, model.POITagID);
                return Json((tag != null) ?
                    new { success = true, poiTagID = tag.POITagID } :
                    new { success = false, poiTagID = 0 }
                );
            }
            return Json(new { sucsess = false, poiTagID = 0 });
        }

        [HttpPost]
        public ActionResult DeletePOITag(int poiTagID)
        {
            return Json(new { success = POIService.DeletePOITag(poiTagID) });
        }

        #endregion

        #region POI TagsGroups

        [AccessRightsAuthorizeAttribute]
        public ActionResult POITagsGroups()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetPOITagsGroup(int poiTagsGroupID)
        {
            var group = POIService.GetPOITagsGroupByID(poiTagsGroupID);
            return (group == null) ?
                Json(new { success = false, group = new POITagsGroupModel() }) :
                Json(new
                {
                    success = true,
                    group = new POITagsGroupModel
                    {
                        Title = group.POITG_Title,
                        POITagsGroupID = group.POITagsGroupID
                    }
                });
        }

        [HttpPost]
        public ActionResult AddPOITagsGroup(POITagsGroupModel model)
        {
            if (ModelState.IsValid)
            {
                var group = POIService.SavePOITagsGroup(model.Title, 0);
                return Json((group != null) ?
                    new { success = true, poiTagsGroupID = group.POITagsGroupID } :
                    new { success = false, poiTagsGroupID = 0 }
                );
            }
            return Json(new { sucsess = false, poiTagsGroupID = 0 });
        }

        [HttpPost]
        public ActionResult UpdatePOITagsGroup(POITagsGroupModel model)
        {
            if (ModelState.IsValid && model.POITagsGroupID > 0)
            {
                var group = POIService.SavePOITagsGroup(model.Title, model.POITagsGroupID);
                return Json((group != null) ?
                    new { success = true, poiTagsGroupID = group.POITagsGroupID } :
                    new { success = false, poiTagsGroupID = 0 }
                );
            }
            return Json(new { sucsess = false, poiTagsGroupID = 0 });
        }

        [HttpPost]
        public ActionResult DeletePOITagsGroup(int poiTagsGroupID)
        {
            return Json(new { success = POIService.DeletePOITagGroup(poiTagsGroupID) });
        }

        #endregion

        #region Portfolio

        [AccessRightsAuthorizeAttribute]
        public ActionResult Portfolio()
        {
            this.ViewBag.PortfolioCSSFile = (this.Domain != null) ? this.Domain.DO_CSS : "";
            return View();
        }

        public ActionResult PortfolioSettings(int portfolioID = 0, int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentBySitemapID(portfolioID, contentID);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null) ?
                content.tbl_SiteMap.SM_DomainID : 0);
            this.ViewBag.PortfolioCategories = PortfolioService.GetAllPortfolioCategoriesAsSelectList((content != null) ? content.tbl_SiteMap.tbl_Portfolio.PO_PortfolioCategoryID : 0);
            return PartialView(content);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdatePortfolio(int domainID, string portfolioTitle, int portfolioCategoryID, bool live, SEOFieldsModel seoModel,
            string content, int portfolioID, bool showlink, string link)
        {
            if (ModelState.IsValid && portfolioID > 0)
            {
                var parent = WebContentService.GetSitemapByType(SiteMapType.Portfolio, this.DomainID);
                string url = String.Format("/{1}", portfolioTitle);
                if (WebContentService.CheckSitemapUniqueUrl(url, portfolioID, domainID))
                    return Json(new { success = false, error = "Please change the 'Portfolio Title'. There is another page with the same URL already registered." });

                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, domainID, String.Empty, false, false, portfolioTitle,
                    null, "0.5", String.Empty, portfolioTitle, true, true, ContentType.Portfolio, seoModel.Title, parent.SiteMapID, portfolioID, 
                    (int)SiteMapType.Portfolio, false, MenuDisplayType.UnderParent);

                tbl_Content tContent = (section != null) ?
                        WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, portfolioTitle,
                            seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) :
                        null;

                tbl_Portfolio portfolio = PortfolioService.SavePortfolio(portfolioTitle, portfolioCategoryID, link, showlink, String.Empty, false, string.Empty, live, portfolioID);

                return Json(new
                {
                    success = section != null && tContent != null && portfolio != null,
                    portfolioID = (portfolio != null) ? portfolio.PortfolioID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddPortfolio(int domainID, string portfolioTitle, int portfolioCategoryID, bool live, SEOFieldsModel seoModel,
            string content, bool showlink, string link)
        {
            if (ModelState.IsValid)
            {
                var parent = WebContentService.GetSitemapByType(SiteMapType.Portfolio, this.DomainID);
                string url = String.Format("/{1}", portfolioTitle);
                if (WebContentService.CheckSitemapUniqueUrl(url, 0, domainID))
                    return Json(new { success = false, error = "Please change the 'Portfolio Title'. There is another page with the same URL already registered." });

                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, domainID, String.Empty, false, false, portfolioTitle,
                    null, "0.5", String.Empty, portfolioTitle, true, true, ContentType.Portfolio, seoModel.Title, parent.SiteMapID, 0, (int)SiteMapType.Portfolio, false, MenuDisplayType.UnderParent);

                tbl_Content tContent = (section != null) ?
                    WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, portfolioTitle,
                        seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) :
                    null;

                tbl_Portfolio portfolio = PortfolioService.SavePortfolio(portfolioTitle, portfolioCategoryID, link, showlink, string.Empty, false, string.Empty, live, section.SiteMapID);

                return Json(new
                {
                    success = section != null && tContent != null && portfolio != null,
                    categoryID = (portfolio != null) ? portfolio.PortfolioID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult TogglePortfolioVisibility(int portfolioID)
        {
            tbl_Portfolio portfolio = PortfolioService.SaveVisibility(portfolioID);
            if (portfolio != null)
                return Json(new { success = true });
            else
                return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeletePortfolio(int portfolioID)
        {
            bool success = PortfolioService.DeletePortfolio(portfolioID);
            success = success && WebContentService.DeleteSection(portfolioID);

            return Json(new { success });
        }

        [HttpPost]
        public ActionResult DeletePortfolioVersion(int contentID)
        {
            return Json(new { success = WebContentService.DeleteContent(contentID) });
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult ApprovePortfolioVersion(int portfolioID, int contentID = 0)
        {
            return Json(new { success = WebContentService.ApproveContent(portfolioID, contentID) != null, portfolioID = portfolioID });
        }

        [HttpPost]
        public ActionResult SavePortfolioOrder(int[] orderedPortfolioIDs)
        {
            return Json(new { success = PortfolioService.SaveOrder(orderedPortfolioIDs) });
        }

        [HttpPost]
        public ActionResult PortfolioScreenGrab(string url)
        {
            ViewBag.Image = ScreenGrab.url2png_v6(SettingsManager.Url2Png.Key, SettingsManager.Url2Png.Secret, url, "png", 300);
            return PartialView();
        }

        public ActionResult PortfolioImage(int portfolioID)
        {
            this.ViewBag.CanDeleteImage = this.AdminUser.HasPermission(Permission.EditContent);
            tbl_Portfolio portfolio = PortfolioService.GetByID(portfolioID);
            return PartialView(portfolio);
        }

        [HttpPost]
        public ActionResult UploadPortfolioImage(HttpPostedFileBase file, string altText, int portfolioID)
        {
            tbl_Portfolio portfolio = PortfolioService.GetByID(portfolioID);
            if (portfolio == null)
                return Json(new { success = false }, "text/html");

            tbl_PortfolioImage portfolioImage = SavePortfolioImage(file,"", altText, portfolioID);
            if(portfolioImage != null)
                return Json(new { success = true });

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeletePortfolioImage(int portfolioImageID, int portfolioID)
        {
            tbl_PortfolioImage portfolioImage = PortfolioService.GetPortfolioImageByID(portfolioImageID);
            if (portfolioImage == null)
                return Json(new { success = false });

            if (PortfolioService.DeletePortfolioImage(portfolioImageID))
                return Json(new { success = true, portfolioID = portfolioID });

            return Json(new { success = false, portfolioID = portfolioID });
        }

        [HttpPost]
        public ActionResult SaveImageOrder(int[] orderedPortfolioImageIDs)
        {
            return Json(new { success = PortfolioService.SavePortfolioImageOrder(orderedPortfolioImageIDs) });
        }

        [HttpPost]
        public JsonResult GetPortfolioCategories(int portfolioID = 0)
        {
            List<tbl_PortfolioCategory> categories = PortfolioService.GetAllPortfolioCategoriesOrdered();
            List<PortfolioCategoryModel> model = categories.Select(m => new PortfolioCategoryModel
            {
                PortfolioCategoryID = m.PortfolioCategoryID,
                Title = m.POC_Title
            }).ToList();
            tbl_Content content = WebContentService.GetContentBySitemapID(portfolioID);
            int selected = (content != null) ? content.tbl_SiteMap.tbl_Portfolio.PO_PortfolioCategoryID : 0;
            return Json(new { success = true, portfolioCategories = model, selected = selected });
        }

        [HttpPost]
        public JsonResult GetPortfolioCategory(int portfolioCategoryID)
        {
            var portfolioCategory = PortfolioService.GetPortfolioCategory(portfolioCategoryID);
            return (portfolioCategory != null) ?
                Json(new
                {
                    success = true,
                    portfolioCategories = new PortfolioCategoryModel
                    {
                        Title = portfolioCategory.POC_Title,
                        PortfolioCategoryID = portfolioCategory.PortfolioCategoryID
                    }
                }) :
                Json(new { success = false });
        }

        [HttpPost]
        public JsonResult SavePortfolioCategory(PortfolioCategoryModel model)
        {
            tbl_PortfolioCategory category = PortfolioService.SavePortfolioCategory(model.Title, model.PortfolioCategoryID);
            return Json((category != null) ?
                    new { success = true, portfolioCategoryID = category.PortfolioCategoryID } :
                    new { success = false, portfolioCategoryID = 0 }
                );
        }

        [HttpPost]
        public JsonResult DeletePortfolioCategory(int portfolioCategoryID)
        {
            tbl_PortfolioCategory portfolioCategory = PortfolioService.GetPortfolioCategory(portfolioCategoryID);
            if (portfolioCategory == null)
                return Json(new { success = false });
            if (portfolioCategory.tbl_Portfolio != null)
            {
                if (portfolioCategory.tbl_Portfolio.Count() > 0)
                {
                    return Json(new { success = false, error = "This category is assigned to one or more portfolios.  Please unassign from all portfolios before deleting" });
                }
            }
            return Json(new { success = PortfolioService.DeletePortfolioCategory(portfolioCategoryID) });

            
        }

        #endregion

        #region Gallery

        [AccessRightsAuthorizeAttribute]
        public ActionResult Gallery()
        {
            this.ViewBag.GalleryCSSFile = (this.Domain != null) ? this.Domain.DO_CSS : "";
            return View();
        }

        public ActionResult GallerySettings(int galleryID = 0, int contentID = 0)
        {
            tbl_Content content = WebContentService.GetContentBySitemapID(galleryID, contentID);
            this.ViewBag.Domains = DomainService.GetAllDomainsAsSelectList((content != null) ?
                content.tbl_SiteMap.SM_DomainID : 0);
            this.ViewBag.GalleryCategories = GalleryService.GetAllGalleryCategoriesAsSelectList((content != null) ? content.tbl_SiteMap.tbl_Gallery.G_GalleryCategoryID : 0);
            return PartialView(content);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateGallery(int domainID, string galleryTitle, int galleryCategoryID, bool live,
            SEOFieldsModel seoModel, MenuDisplayType displayType, string content, int galleryID, int customerID=0)
        {
            if (ModelState.IsValid && galleryID > 0)
            {
                var parent = WebContentService.GetSitemapByType(SiteMapType.Gallery, this.DomainID);
                string url = String.Format("/{0}", galleryTitle);
                if (WebContentService.CheckSitemapUniqueUrl(url, galleryID, domainID))
                    return Json(new { success = false, error = "Please change the 'Gallery Title'. There is another page with the same URL already registered." });

                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, domainID, String.Empty, false, false, galleryTitle,
                    null, "0.5", String.Empty, galleryTitle, true, true, ContentType.Gallery, seoModel.Title, parent.SiteMapID, galleryID, (int)SiteMapType.Gallery, false, displayType);

                tbl_Content tContent = (section != null) ?
                        WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, galleryTitle,
                            seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) :
                        null;

                tbl_Gallery gallery = GalleryService.SaveGallery(galleryTitle, live, customerID, galleryCategoryID, galleryID);

                return Json(new
                {
                    success = section != null && tContent != null && gallery != null,
                    galleryID = (gallery != null) ? gallery.GalleryID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddGallery(int domainID, string galleryTitle, int galleryCategoryID, bool live, SEOFieldsModel seoModel,
            string content, MenuDisplayType displayType, int customerID=0)
        {
            if (ModelState.IsValid)
            {
                var parent = WebContentService.GetSitemapByType(SiteMapType.Gallery, this.DomainID);
                string url = String.Format("/{0}", galleryTitle);
                if (WebContentService.CheckSitemapUniqueUrl(url, 0, domainID))
                    return Json(new { success = false, error = "Please change the 'Gallery Title'. There is another page with the same URL already registered." });

                tbl_SiteMap section = WebContentService.SaveSiteMap(seoModel.R301, 1, 0, domainID, String.Empty, false, false, galleryTitle,
                    null, "0.5", String.Empty, galleryTitle, true, true, ContentType.Gallery, seoModel.Title, 0, 0, (int)SiteMapType.Gallery, false, displayType);

                tbl_Content tContent = (section != null) ?
                    WebContentService.SaveContent(String.Empty, content, seoModel.Desc, 0, String.Empty, seoModel.Keywords, galleryTitle,
                        seoModel.MetaData, 0, String.Empty, String.Empty, String.Empty, seoModel.Title, String.Empty, false, section.SiteMapID, 0) :
                    null;

                tbl_Gallery gallery = GalleryService.SaveGallery(galleryTitle,live,customerID,galleryCategoryID,section.SiteMapID);

                return Json(new
                {
                    success = section != null && tContent != null && gallery != null,
                    galleryID = (gallery != null) ? gallery.GalleryID : 0,
                    contentID = (tContent != null) ? tContent.ContentID : 0
                });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult ToggleGalleryVisibility(int galleryID)
        {
            tbl_Gallery gallery = GalleryService.SaveGalleryVisibility(galleryID);
            if (gallery != null)
                return Json(new { success = true });
            else
                return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult DeleteGallery(int galleryID)
        {
            bool success = GalleryService.DeleteGallery(galleryID);
            success = success && WebContentService.DeleteSection(galleryID);

            return Json(new { success });
        }

        [HttpPost]
        public ActionResult DeleteGalleryVersion(int contentID)
        {
            return Json(new { success = WebContentService.DeleteContent(contentID) });
        }

        [HttpPost]
        [ApproveContentAuthorize]
        public ActionResult ApproveGalleryVersion(int galleryID, int contentID = 0)
        {
            return Json(new { success = WebContentService.ApproveContent(galleryID, contentID) != null, galleryID = galleryID });
        }

        [HttpPost]
        public ActionResult SaveGalleryOrder(int[] orderedGalleryIDs)
        {
            return Json(new { success = GalleryService.SaveGalleryOrder(orderedGalleryIDs) });
        }

        [HttpPost]
        public JsonResult GetGalleryCategories(int galleryID = 0)
        {
            List<tbl_GalleryCategory> categories = GalleryService.GetAllGalleryCategoriesOrdered();
            List<GalleryCategoryModel> model = categories.Select(m => new GalleryCategoryModel
            {
                GalleryCategoryID = m.GalleryCategoryID,
                Title = m.GC_Title
            }).ToList();
            tbl_Content content = WebContentService.GetContentBySitemapID(galleryID);
            int selected = (content != null) ? content.tbl_SiteMap.tbl_Gallery.G_GalleryCategoryID : 0;
            return Json(new { success = true, galleryCategories = model, selected = selected });
        }

        [HttpPost]
        public JsonResult GetGalleryCategory(int galleryCategoryID)
        {
            var galleryCategory = GalleryService.GetGalleryCategory(galleryCategoryID);
            return (galleryCategory != null) ?
                Json(new
                {
                    success = true,
                    galleryCategories = new GalleryCategoryModel
                    {
                        Title = galleryCategory.GC_Title,
                        GalleryCategoryID = galleryCategory.GalleryCategoryID
                    }
                }) :
                Json(new { success = false });
        }

        [HttpPost]
        public JsonResult SaveGalleryCategory(GalleryCategoryModel model)
        {
            tbl_GalleryCategory category = GalleryService.SaveGalleryCategory(model.Title, model.GalleryCategoryID);
            return Json((category != null) ?
                    new { success = true, galleryCategoryID = category.GalleryCategoryID } :
                    new { success = false, galleryCategoryID = 0 }
                );
        }

        [HttpPost]
        public JsonResult DeleteGalleryCategory(int galleryCategoryID)
        {
            tbl_GalleryCategory category = GalleryService.GetGalleryCategory(galleryCategoryID);
            if (category == null)
                return Json(new { success = false });
            if (category.tbl_Gallery != null)
            {
                if (category.tbl_Gallery.Count() > 0)
                {
                    return Json(new { success = false, error = "This category is assigned to one or more galleries.  Please unassign from all galleries before deleting" });
                }
            }
            return Json(new { success = GalleryService.DeleteGalleryCategory(galleryCategoryID) });


        }

        #endregion

        #region Gallery Image

        public ActionResult GalleryImage(int galleryID)
        {
            this.ViewBag.CanDeleteImage = this.AdminUser.HasPermission(Permission.EditContent);
            tbl_Gallery gallery = GalleryService.GetByID(galleryID);
            return PartialView(gallery);
        }

        [HttpPost]
        public ActionResult UploadGalleryImage(IEnumerable<HttpPostedFileBase> files, string description, int galleryID, int[] tagIDs)
        {
            bool success = true;
            tbl_Gallery gallery = GalleryService.GetByID(galleryID);
            if (gallery == null)
                return Json(new { success = false }, "text/html");

            foreach (HttpPostedFileBase file in files)
            {
                tbl_GalleryImage galleryImage = SaveGalleryImage(file, String.Empty, description, galleryID);
                if (galleryImage == null)
                    success = false;
                else
                    GalleryService.UpdateImageTags(galleryImage.GI_ImageID, tagIDs);
            }
            return Json(new { success = success });
        }

        [HttpPost]
        public ActionResult SaveGalleryImage(string description, int[] tagIDs, int imageID)
        {
            tbl_Image image = GalleryService.GetImageByID(imageID);
            if (image == null)
                return Json(new { success = false });

            bool success = true;
            image = GalleryService.UpdateImageDescription(imageID, String.Empty, description);
            success &= image != null;

            image = GalleryService.UpdateImageTags(imageID, tagIDs);
            success &= image != null;

            return Json(new { success });
        }

        [HttpPost]
        public ActionResult DeleteGalleryImage(int galleryImageID, int galleryID)
        {
            tbl_GalleryImage galleryImage = GalleryService.GetGalleryImageByID(galleryImageID);
            if (galleryImage == null)
                return Json(new { success = false });

            if (GalleryService.DeleteGalleryImage(galleryImageID))
                return Json(new { success = true, galleryID = galleryID });

            return Json(new { success = false, galleryID = galleryID });
        }

        [HttpPost]
        public ActionResult SaveGalleryImageOrder(int[] orderedGalleryImageIDs)
        {
            return Json(new { success = GalleryService.SaveGalleryImageOrder(orderedGalleryImageIDs) });
        }

        [HttpPost]
        public ActionResult GetImageTags(int imageID)
        {
            return Json(new { tags = GalleryService.GetAllTagsByImageID(imageID) });
        }

        [HttpPost]
        public ActionResult AddImageTag(string name, int imageID)
        {
            var tag = GalleryService.SaveTag(name, true, 0);
            return (tag == null) ?
                Json(new { success = false }) :
                // new tag is selected by default
                Json(new
                {
                    success = (imageID == 0) ? true :
                        GalleryService.AddImageTags(imageID, new int[] { tag.GalleryTagID }) != null,
                    item = new SelectListItem { Selected = true, Text = tag.GT_Title, Value = tag.GalleryTagID.ToString() }
                });
        }

        #endregion

        #region Gallery Tags


        [HttpPost]
        public ActionResult GetGalleryTags(int galleryID)
        {
            return Json(new { tags = GalleryService.GetAllTagsByGalleryID(galleryID) });
        }

        [HttpPost]
        public ActionResult AddGalleryTag(string name, int galleryID)
        {
            var galleryTag = GalleryService.SaveTag(name, false, 0);
            return (galleryTag == null) ?
                Json(new { success = false }) :
                // new tag is selected by default
                Json(new
                {
                    success = (galleryID == 0) ?
                        true :
                        GalleryService.AddTagToGallery(galleryID, galleryTag.GalleryTagID),
                    item = new SelectListItem { Selected = true, Text = galleryTag.GT_Title, Value = galleryTag.GalleryTagID.ToString() }
                });
        }

        [HttpPost]
        public ActionResult DeleteGalleryTag(int tagID)
        {
            return Json(new { success = GalleryService.DeleteTag(tagID) });
        }

        [HttpPost]
        public ActionResult SaveGalleryTags(int galleryID, int contentID, int[] tagIDs)
        {
            return Json(new { success = GalleryService.SaveTagsForGallery(galleryID,tagIDs), galleryID = galleryID, contentID = contentID });        
        }


        #endregion

        #region Templates

        [AccessRightsAuthorizeAttribute]
        public ActionResult Templates()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteTemplate(int templateID)
        {
            return Json(new { success = TemplateService.DeleteTempate(templateID) });
        }

        [HttpPost]
        public ActionResult GetTemplate(int templateID)
        {
            tbl_Templates template = TemplateService.GetByID(templateID);
            return Json((template == null) ? new TemplatesModel() : new TemplatesModel(template));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddTemplate(TemplatesModel model)
        {
            if (ModelState.IsValid)
            {
                tbl_Templates template = TemplateService.SaveTemplate(0, model.Name, model.Header, model.UseHeader, model.Footer, model.UseFooter, model.Live);
                return Json(new { success = (template != null), templateID = (template != null ? template.TemplateID : 0) });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateTemplate(TemplatesModel model)
        {
            if (ModelState.IsValid && model.TemplateID != 0)
            {
                tbl_Templates template = TemplateService.SaveTemplate(model.TemplateID, model.Name, model.Header, model.UseHeader, model.Footer, model.UseFooter, model.Live);
                return Json(new { success = (template != null), templateID = (template != null ? template.TemplateID : 0) });
            }
            return Json(new { success = false });
        }

        #endregion


        #region Private Methods

        private List<tbl_Domains> GetDomains(int domainID)
        {
            return (domainID != 0) ?
                new List<tbl_Domains>() { DomainService.GetDomainByID(domainID) } :
                DomainService.GetAllDomains();
        }

        private tbl_SiteMap ToogleNewsVisiblityAndTweet(int sitemapID)
        {
            tbl_SiteMap sitemap = WebContentService.SaveSitemapVisibility(sitemapID);
            if (sitemap.SM_Live)
                SendTweet(sitemap.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted)
                    .OrderByDescending(c => c.C_ModificationDate)
                    .Select(c => c.SUB_Tweet).FirstOrDefault());
            return sitemap;
        }

        private bool SaveImage(HttpPostedFileBase file, int? sitemapID, string heading, string desc, int? linkID = null)
        {
            if (file != null && IsImage(file))
            {
                string name = file.FileName.Split('\\').Last();
                //var image = WebContentService.SaveImage(0, desc, 0, 0, false, null, SettingsManager.Images.PageImagesPath, siteMapID, name, 0);
                var image = GalleryService.SaveImage(0, heading, desc, 0, 0, false, null, SettingsManager.Images.OriginalImagePath, sitemapID, name, 0, linkID);
                //if (image != null && SaveImageFile(file, SettingsManager.Images.PageImagesPath + image.I_Thumb))
                if (image != null && SaveImageFile(file, SettingsManager.Images.OriginalImagePath + image.I_Thumb))
                    return true;
            }
            return false;
        }

        private string RandomStr(string filename)
        {
            string rStr = Path.GetRandomFileName().Replace(".", "");
            return rStr + filename;
        }

        private tbl_ProductImages SaveProdImage(HttpPostedFileBase file, string description, string view, int productID)
        {
            if (file != null && IsImage(file))
            {
                tbl_ProductImages image = ECommerceService.SaveProductImage(productID, description, file.FileName, false, view);
                if (image != null && SaveImageFile(file, SettingsManager.Images.OriginalImagePath + image.I_Name))
                    return image;
            }
            return null;
        }

        private tbl_PortfolioImage SavePortfolioImage(HttpPostedFileBase file, string heading, string description, int portfolioID)
        {
            if(file !=null && IsImage(file))
            {
                tbl_Image image = GalleryService.SaveImage(0, heading, description, 0, 0, false, 0, SettingsManager.Images.OriginalImagePath, null, file.FileName.Split('\\').Last(), 0, null);
                if(image != null && SaveImageFile(file,SettingsManager.Images.OriginalImagePath + image.I_Thumb))
                {
                    return PortfolioService.SavePortfolioImage(portfolioID, image.ImageID, 0);
                }
            }
            return null;
        }

        private tbl_GalleryImage SaveGalleryImage(HttpPostedFileBase file, string heading, string description, int galleryID)
        {
            if (file != null && IsImage(file))
            {
                tbl_Image image = GalleryService.SaveImage(0, heading, description, 0, 0, false, 0, SettingsManager.Images.OriginalImagePath, null, file.FileName.Split('\\').Last(), 0, null);
                if (image != null && SaveImageFile(file, SettingsManager.Images.OriginalImagePath + image.I_Thumb))
                {
                    return GalleryService.SaveGalleryImage(galleryID, image.ImageID, 0);
                }
            }
            return null;
        }

        private bool SaveImageFile(HttpPostedFileBase file, string path)
        {
            if (file == null || String.IsNullOrEmpty(path) || !IsImage(file))
                return false;
            if (!Directory.Exists(Path.GetDirectoryName(Server.MapPath(path))))
                Directory.CreateDirectory(Path.GetDirectoryName(Server.MapPath(path)));
            try
            {
                Stream fileStream = SettingsManager.Images.ResizeOriginal ?
                    ImageUtils.Resize(SettingsManager.Images.MaxOriginalWidth, SettingsManager.Images.MaxOriginalHeight, file.InputStream, file.FileName) : file.InputStream;

                FileStream writeStream = new FileStream(Server.MapPath(path), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.CopyTo(writeStream);
                writeStream.Flush();
                writeStream.Close();
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Save file error", e);
                return false;
            }
        }

        private bool DeleteImage(int imageID)
        {
            var image = GalleryService.GetImageByID(imageID);
            if (image != null)
            {
                if (GalleryService.DeleteImage(imageID))
                    DeleteFile(image.I_Path + image.I_Thumb);

                return true;
            }
            return false;
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file.ContentType.Contains("image"))
                return true;

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" }; // add more if u like...

            foreach (var item in formats)
                if (file.FileName.Contains(item))
                    return true;
            return false;
        }

        private bool DeleteProdImage(int imageID)
        {
            var image = ECommerceService.GetProductImageByID(imageID);
            if (image != null)
            {
                if (ECommerceService.DeleteProductImage(imageID))
                    DeleteFile(SettingsManager.Images.OriginalImagePath + image.I_Name);
                return true;
            }
            return false;
        }

        private bool DeleteFile(string path)
        {
            try
            {
                System.IO.File.Delete(Server.MapPath(path));
                return true;
            }
            catch (DirectoryNotFoundException e)
            {
                // do nothing if directory is not found, image removed only from database
                Log.Error("Error while deleting image, directory not found: " + path, e);
                return false;
            }
            catch (Exception e)
            {
                Log.Error("Error while deleting image: " + path, e);
                return false;
            }
        }

        private List<WebsiteMenuModel> GetMenuOrdered(int? userGroupID)
        {
            var menu = new List<WebsiteMenuModel>();
            if (userGroupID.HasValue)
            {
                var groupMenuList = DomainService.GetAllAdminMenuItemsByAccessrights(userGroupID.Value);
                foreach (var item in groupMenuList.Where(am => am.AM_ParentID == 0).OrderBy(am => am.AM_Order))
                {
                    menu.Add(new WebsiteMenuModel()
                    {
                        Name = item.AM_MenuText,
                        Title = item.AM_MenuText,
                        Url = item.AM_URL,
                        Target = string.Empty,
                        SubMenuItems = CreateChildMenuOrdered(groupMenuList, item.AdminMenuID, userGroupID.Value)
                    });
                }
            }

            menu.Add(new WebsiteMenuModel()
            {
                Name = "Open Website",
                Title = "Open Website",
                Target = "_blank",
                Url = "/",
            });

            menu.Add(new WebsiteMenuModel()
            {
                Name = "Logout",
                Title = "Logout",
                Url = "/Admn/Logout",
            });

            return menu;
        }

        private List<WebsiteMenuModel> CreateChildMenuOrdered(List<tbl_AdminMenu> items, int parentID, int userGroupID)
        {
            var menu = new List<WebsiteMenuModel>();
            foreach (var item in items.Where(am => am.AM_ParentID == parentID).OrderBy(am => am.AM_Order))
            {
                menu.Add(new WebsiteMenuModel
                {
                    Name = item.AM_MenuText,
                    Title = item.AM_MenuText,
                    Url = item.AM_URL,
                    Target = string.Empty,
                    SubMenuItems = CreateChildMenuOrdered(items, item.AdminMenuID, userGroupID)
                });
            }
            return menu;
        }

        private static string ProductUrl(ProductModel productModel, tbl_ProdCategories category)
        {
            return String.Format("{0}/{1}", category.tbl_SiteMap.SM_URL, productModel.ProductTitle);
        }

        private static List<string> PrepareCustomersCSVFile(List<tbl_Customer> model)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();

            result.Add("CustomerID,CU_Email,CU_RegisterDate,CU_FirstName,CU_Surname,CU_Title,CU_Telephone,CU_DomainID,CU_Subscription,CU_Registered,CU_DetailsFor3rdParties,CU_IsDormant," +
                        "AddressID,A_Line1,A_Line2,A_Line3,A_Town,A_County,A_Postcode,A_Country,A_FirstName,A_Surname,A_Title,A_Phone,A_CountryID");

            foreach (var item in model)
            {
                sb = new StringBuilder();
                sb.Append(item.CustomerID);
                sb.Append(",");
                sb.Append(item.CU_Email);
                sb.Append(",");
                sb.Append(item.CU_RegisterDate);
                sb.Append(",");
                sb.Append(item.CU_FirstName);
                sb.Append(",");
                sb.Append(item.CU_Surname);
                sb.Append(",");
                sb.Append(item.CU_Title);
                sb.Append(",");
                sb.Append(item.CU_Telephone);
                sb.Append(",");
                sb.Append(item.CU_DomainID);
                sb.Append(",");
                sb.Append(item.CU_Subscription);
                sb.Append(",");
                sb.Append(item.CU_Registered);
                sb.Append(",");
                sb.Append(item.CU_DetailsFor3rdParties);
                sb.Append(",");
                sb.Append(item.CU_IsDormant);
                sb.Append(",");

                if (item.tbl_Address.Count > 0)
                {
                    tbl_Address customerAddress = item.tbl_Address.First();

                    sb.Append(customerAddress.AddressID);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Line1);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Line2);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Line3);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Town);
                    sb.Append(",");
                    sb.Append(customerAddress.A_County);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Postcode);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Country);
                    sb.Append(",");
                    sb.Append(customerAddress.A_FirstName);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Surname);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Title);
                    sb.Append(",");
                    sb.Append(customerAddress.A_Phone);
                    sb.Append(",");
                    sb.Append(customerAddress.A_CountryID);
                }
                else
                {
                    sb.Append(",,,,,,,,,,,,");
                }

                result.Add(sb.ToString());

            }
            return result;
        }

        #endregion
    }
}

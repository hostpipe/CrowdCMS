using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using CMS.BL;
using CMS.BL.Entity;
using CMS.Services;
using CMS.Services.Extensions;
using CMS.UI.Common;
using CMS.UI.Models;
using CMS.UI.Security;
using CMS.Utils;
using CMS.Utils.Cryptography;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using CMS.BL.Comparers;
using CMS.Utils.Diagnostics;
using CMS.Utils.Extension;
using Newtonsoft.Json;

namespace CMS.UI.Controllers
{
    [Authentication]
    [CookieConsent]
    public class WebsiteController : BaseWebController
    {
        private readonly IDomain DomainService;
        private readonly IWebContent WebContentService;
        private readonly IECommerce ECommerceService;
        private readonly IWebPages WebPagesService;
        private readonly IUser UserService;
        private readonly IPOI POIService;
        private readonly ITemplate TemplateService;
        private readonly IPortfolio PortfolioService;
        private readonly IGallery GalleryService;

        public WebsiteController(IDomain domainService,
            IWebContent webContentService,
            IECommerce ecommerceService,
            IWebPages webPagesService,
            IUser userService,
            IPOI poiService,
            ITemplate templateService,
            IPortfolio portfolioService,
            IGallery galleryService)
            : base(domainService, ecommerceService, userService, webContentService)
        {
            this.DomainService = domainService;
            this.WebContentService = webContentService;
            this.ECommerceService = ecommerceService;
            this.WebPagesService = webPagesService;
            this.UserService = userService;
            this.POIService = poiService;
            this.TemplateService = templateService;
            this.PortfolioService = portfolioService;
            this.GalleryService = galleryService;

        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Content(int sectionID = 0, int contentID = 0)
        {
            sectionID = Request.Url.LocalPath == "/" && this.Domain != null ? this.Domain.DO_HomePageID : sectionID;
            var user = this.AdminUser != null ? this.AdminUser.IsAdmn : false;
            tbl_Content content = WebContentService.GetContentBySitemapUrl(Request.Url.LocalPath, this.DomainID, sectionID, contentID, user, this.Domain.DO_HomePageID);

            if (content == null)
            {
                content = WebContentService.GetContentByRedirectUrl(Request.Url.LocalPath, this.DomainID, contentID);
                if (content != null)
                    return RedirectPermanent(content.tbl_SiteMap.SM_URL);
            }

            if (content == null)
                throw new HttpException(404, "Page Not Found");
                //return null;

            //check for post data
            if (Request.HttpMethod == "POST") //possible form submission
            {
                string postedFormID = Request.Form["formid"];
                int formID = 0;
                Int32.TryParse(postedFormID, out formID);
                if (formID > 0)
                {
                    tbl_Form form = WebPagesService.GetFormByID(formID);
                    if (form != null)
                    {
                        this.ViewBag.Tracking = form.F_Tracking;
                    }
                }
            }

            this.ViewBag.IsHomePage = (content.C_TableLinkID == this.Domain.DO_HomePageID);

            if (Request.Url.LocalPath == "/" && content.tbl_SiteMap.SM_IsPredefined)
            {
                switch (content.tbl_SiteMap.SM_TypeID.GetValueOrDefault(0))
                {
                    case (int)SiteMapType.News:
                        return Blog(String.Empty, String.Empty, String.Empty);
                    case (int)SiteMapType.ProductShop:
                        return ProdCategories();
                    case (int)SiteMapType.Portfolio:
                        return Portfolio();
                    case (int)SiteMapType.Gallery:
                        return Gallery();
                    case (int)SiteMapType.EventShop:
                        return EventsCategories();
                    case (int)SiteMapType.Sitemap:
                        return Sitemap();
                    case (int)SiteMapType.Testimonials:
                        return Testimonials();
                    case (int)SiteMapType.Subscribe:
                        return Subscribe();
                    case (int)SiteMapType.Donation:
                        return DonationCategories();
                    case (int)SiteMapType.PointsOfInterest:
                        return POIs();
                }
            }

            if (content.tbl_SiteMap.SM_TypeID != null && content.tbl_SiteMap.SM_TypeID != (int)SiteMapType.Sitemap && content.tbl_SiteMap.SM_TypeID != (int)SiteMapType.ContactUs)
                throw new HttpException(404, "Page Not Found");

            ContentModel contentModel = ContentModelFactory(content);
            this.ViewBag.Title = content.C_Title;
            this.ViewBag.PageID = contentModel.Content.tbl_SiteMap.SiteMapID;

            this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);
            return View(contentModel);
        }

        [AllowAnonymous]
        public ActionResult Breadcrumbs(string url = "")
        {
            url = String.IsNullOrEmpty(url) ? this.Request.Url.AbsolutePath : url;
            ViewBag.CompanyName = this.Domain != null ? this.Domain.DO_CompanyName : string.Empty;
            return PartialView("Breadcrumbs", FindBreadcrumbByUrl(url, GetMenuAllOrdered()));
        }

        [AllowAnonymous]
        public ActionResult Menu()
        {
            return PartialView("Menu", GetMenuOrdered(true, false, false, false, true, false));
        }

        [AllowAnonymous]
        public ActionResult SubMenu(int sitemapID = 0)
        {
            return PartialView("SubMenu", GetSubMenuOrdered(true, false, false, sitemapID, false, true, false));
        }

        [AllowAnonymous]
        public ActionResult GetTweets(int count = 3)
        {
            List<string> tweets = null;
            var domain = this.Domain;
            if (domain != null)
            {
                TweetManager tweet = new TweetManager(domain.DO_TwitterToken, domain.DO_TwitterSecret, domain.DO_ConsumerKey, domain.DO_ConsumerSecret);
                tweets = tweet.GetLatestTweets(count);
            }
            return PartialView("/Views/Partials/GetTweets.cshtml", tweets);
        }

        [AllowAnonymous]
        public ActionResult Footer()
        {
            var paymentModel = ECommerceService.GetAllPaymentsDomain(this.DomainID).Where(s => !String.IsNullOrWhiteSpace(s.PD_Logo))
                .Select(m => new PaymentLogoModel
            {
                Code = m.tbl_PaymentType.PT_Code,
                Name = m.tbl_PaymentType.PT_Name,
                FilePath = String.Format("/{0}/{1}/{2}", SettingsManager.Payment.PaymentLogosPath.Trim('/'), this.DomainID, m.PD_Logo)
            }).ToList();
            var template = TemplateService.GetLive();
            ViewBag.CompanyName = this.Domain != null ? this.Domain.DO_CompanyName : string.Empty;
            ViewBag.Telephone = this.Domain != null ? this.Domain.DO_CompanyTelephone : string.Empty;
            ViewBag.Address = this.Domain != null ? this.Domain.DO_CompanyAddress : string.Empty;
            ViewBag.Email = this.Domain != null ? this.Domain.DO_Email: string.Empty;
            return PartialView("Footer",
                new FooterModel
                {
                    WebsiteMenu = GetMenuOrdered(false, true, false),
                    PaymentMethods = paymentModel,
                    FooterContent = template != null ? template.T_Footer : String.Empty,
                    UseTemplate = template != null ? template.T_UseFooter : false
                });
        }

        [AllowAnonymous]
        public ActionResult Sitemap()
        {
            this.ViewBag.SitemapPage = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Sitemap, this.DomainID).TrimEnd('/'), this.DomainID);
            return View("Sitemap", GetMenuOrdered(false, false, true, true, true, true));
        }

        [AllowAnonymous]
        public ActionResult WebsiteUrls(string time)
        {
            var list = CreateElements(GetMenuAllOrdered()).OrderBy<LinkListItem, string>(x => x.title).ToList();
            return PartialView("UrlsList", list);
        }

        [AllowAnonymous]
        public ActionResult CustomEditorObjects(int currentDomainID, string selector)
        {
            if (currentDomainID == 0)
            {
                currentDomainID = DomainID;
            }
            return Json(new
            {
                selector = selector,
                // tinyMCE: link_list
                link_list = CreateElements(GetMenuOrdered(false, false, true)).OrderBy(x => x.title).ToArray(),
                // tinyMCE: customform_list
                customForms = WebPagesService.GetAllForms().OrderBy(f => f.F_DomainID).ThenBy(f => f.F_Name).Select(f => new LinkListItem
            {
                title = String.Format("{0} ({1})", f.F_Name, f.tbl_Domains.DO_Domain),
                value = f.FormID.ToString()
            }).ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult XMLSitemap()
        {
            if (!Directory.Exists(Server.MapPath(SettingsManager.Sitemap.Folder)))
                Directory.CreateDirectory(Server.MapPath(SettingsManager.Sitemap.Folder));

            string domain = this.Domain != null ? "http://" + this.Domain.DO_Domain : string.Empty;
            string path = Server.MapPath(SettingsManager.Sitemap.Folder) + SettingsManager.Sitemap.File;
            SitemapXML.Create(GetMenuOrdered(false, false, true, true, true, true), path, domain);
            return File(path, "text/xml");
        }

        [AllowAnonymous]
        public ActionResult Search(string keyword)
        {
            List<tbl_Content> textsContents = WebContentService.SearchContent(keyword, this.DomainID);
            this.ViewBag.Header = String.Format("Search Results for '{0}'", keyword);
            this.ViewBag.BlogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/'));
            this.ViewBag.NoNewsFound = (textsContents.Count() == 0) ? String.Format("No Search Results for '{0}'", keyword) : string.Empty;
            return View("SearchResults", textsContents);
        }

        [AllowAnonymous]
        public FileContentResult RobotsTXT(string keyword)
        {
            var contentBuilder = new StringBuilder();

            if (!this.DevelopmentMode)
            {
                string robots = this.Domain.DO_Robots ?? String.Empty;
                contentBuilder.AppendLine(robots);
                contentBuilder.AppendLine(String.Format("Sitemap: http://{0}/sitemap.xml",this.Domain.DO_Domain));
            }
            else
            {
                contentBuilder.AppendLine("User-agent: *");
                contentBuilder.AppendLine("Disallow: /");
            }
            return File(Encoding.UTF8.GetBytes(contentBuilder.ToString()), "text/plain");

        }

        [AllowAnonymous]
        public ActionResult AllowCookies(string ReturnUrl)
        {
            CookieConsent.SetCookieConsent(Response, true);
            return Redirect(ReturnUrl);
        }

        [AllowAnonymous]
        public ActionResult NoCookies(string ReturnUrl)
        {
            CookieConsent.SetCookieConsent(Response, false);
            // if we got an ajax submit, just return 200 OK, else redirect back
            if (Request.IsAjaxRequest())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
            else
                return Redirect(ReturnUrl);
        }


        #region Customer

        public ActionResult DisplayCustomerAccount()
        {
            var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
            List<tbl_Gallery> galleryList = GalleryService.GetCustomerGalleries(customer.CustomerID);
            var addressList = ECommerceService.GetAllAddresses(customer.CustomerID).ToList();
            var customerModel = new CustomerModel
            {
                UserName = String.Format("{0} {1}", customer.CU_FirstName, customer.CU_Surname),
                Email = customer.CU_Email,
                Phone = customer.CU_Telephone,
                addresses = addressList,
                galleries = galleryList
            };

            return View(customerModel);
        }

        public ActionResult CustomerGallery(int galleryID)
        {
            var gallery = GalleryService.GetByID(galleryID);
            if(gallery != null)
                return View(gallery);

            return View();
        }

        public ActionResult EditCustomerAccount()
        {
            var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
            return customer == null ? View() : View(new EditCustomerModel(customer));
        }

        [HttpPost]
        public ActionResult EditCustomerAccount(EditCustomerModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
                if (customer == null)
                {
                    ModelState.AddModelError("", "Can not find customer.");
                    return View();
                }

                if (!String.IsNullOrEmpty(model.Password))
                {
                    if (String.IsNullOrEmpty(model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "You have to provide your current password before change it.");
                        return View();
                    }
                    var password = Sha512.GetSHA512Hash(model.CurrentPassword);
                    if (password != customer.CU_Password)
                    {
                        ModelState.AddModelError("CurrentPassword", "Provided current password is not valid.");
                        return View();
                    }
                }
                if (customer.CU_Email != model.Email)
                {
                    var email = UserService.GetCustomerByEmail(model.Email, DomainID);
                    if (email != null)
                    {
                        ModelState.AddModelError("Email", "This email is already registered in our database.");
                        return View();
                    }
                }
                UserService.SaveCustomer(model.Email, model.FirstName, model.Surname, model.Phone, model.Title, model.Password, DomainID, customer.CustomerID, true, model.DetailsFor3rdParties, "");
                if (this.Domain.IsAnyCRMEnabled)
                    UserService.SubscribeNewsletter(model.Email, model.SubscribeNewsletter, this.DomainID);
                return RedirectToAction("Content");
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Content");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var customer = UserService.GetCustomerByEmailAndPassword(model.Email, model.Password, DomainID);
                if (customer != null)
                {
                    var principal = new CustomPrincipalSerializeModel();
                    principal.Email = customer.CU_Email;
                    principal.UserID = customer.CustomerID;
                    principal.UserName = String.Format("{0} {1}", customer.CU_FirstName, customer.CU_Surname);
                    principal.IsAdmn = false;
                    var principalString = JsonConvert.SerializeObject(principal);

                    var authTicket = new FormsAuthenticationTicket(1, customer.CU_Email, DateTime.Now, DateTime.Now.AddDays(SettingsManager.CookieExpireTime), true, principalString, FormsAuthentication.FormsCookiePath);
                    var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(cookie);

                    if (String.IsNullOrEmpty(returnUrl))
                        return RedirectToAction("Content");
                    return Redirect(returnUrl);
                }
                else
                    ModelState.AddModelError("", "Email or Password is not valid.");
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Content");
        }

        [AllowAnonymous]
        public ActionResult Registration()
        {
            this.ViewBag.Countries = ECommerceService.GetAllCountriesAsSelectList(this.DomainID);
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Registration(RegistrationModel model, string returnUrl)
        {
            this.ViewBag.Countries = ECommerceService.GetAllCountriesAsSelectList(this.DomainID);

            if (ModelState.IsValid)
            {
                var customer = UserService.GetCustomerByEmail(model.Email, DomainID);
                if (customer != null)
                {
                    ModelState.AddModelError("", "This email is already registered in our database.");
                    return View();
                }

                customer = UserService.SaveCustomer(model.Email, model.FirstName, model.Surname, String.Empty, String.Empty, model.Password, DomainID, 0, true, model.DetailsFor3rdParties, "");
                if (customer != null)
                {
                    ECommerceService.SaveAddress(customer.CustomerID, model.CountryID, model.County, model.FirstName, model.Surname, String.Empty,
                        model.Address1, model.Address2, model.Address3, model.Postcode, String.Empty, model.Town, 0);

                    var principal = new CustomPrincipalSerializeModel();
                    principal.Email = customer.CU_Email;
                    principal.UserID = customer.CustomerID;
                    principal.UserName = String.Format("{0} {1}", customer.CU_FirstName, customer.CU_Surname);
                    principal.IsAdmn = false;
                    var principalString = JsonConvert.SerializeObject(principal);

                    var authTicket = new FormsAuthenticationTicket(1, customer.CU_Email, DateTime.Now, DateTime.Now.AddDays(SettingsManager.CookieExpireTime), true, principalString, FormsAuthentication.FormsCookiePath);
                    var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(cookie);

                    if (this.Domain.IsAnyCRMEnabled)
                        UserService.SubscribeNewsletter(customer.CU_Email, model.Newsletter, this.DomainID);

                    MailingService.SendWelcomeMessage(principal.UserName, model.Email, this.Domain.DO_CompanyName, this.Domain.DO_Domain, this.Domain.DO_CompanyTelephone);
                }
                else
                    ModelState.AddModelError("", "There was an error saving new customer.");

                if (String.IsNullOrEmpty(returnUrl))
                    return RedirectToRoute("RegisterConfirmation");
                return Redirect(returnUrl);
            }
            return View();
        }

        public ActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Subscribe()
        {
            return View("Subscribe");
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult Subscribe(string email)
        {
            if (email == null)
                return Json(new { success = false });
            if (this.Domain.IsAnyCRMEnabled && UserService.SubscribeNewsletter(email, true, this.DomainID))
                return Json(new { success = true });
            return Json(new { success = false });
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
            var customer = UserService.GetCustomerByEmail(Email, DomainID);
            if (customer != null)
            {
                var newPassword = Password.Create(SettingsManager.PassLength);
                customer = UserService.SaveCustomer(customer.CU_Email, customer.CU_FirstName, customer.CU_Surname, customer.CU_Telephone, customer.CU_Title, newPassword,
                    customer.CU_DomainID, customer.CustomerID, true, customer.CU_DetailsFor3rdParties, customer.CU_AdminNote);
                MailingService.SendForgottenPassword(customer.CU_FirstName + " " + customer.CU_Surname, newPassword, customer.CU_Email, HttpContext.Request.Url.Host, "/");
                this.ViewBag.MailSent = true;
            }
            else
            {
                this.ViewBag.MailSent = false;
                ModelState.AddModelError("", "Email address not found. Please check you have entered it correctly and try again.");
            }

            return View();
        }

        [HttpPost]
        public JsonResult GetAddressList()
        {
            var id = this.AdminUser.UserID;
            if (id == 0)
                return Json(new { success = false });

            var list = ECommerceService.GetAllAddressesAsSelectList(id, 0);
            if (list == null)
                return Json(new { success = false });

            var basket = FindBasket();
            int countryID = (basket != null) ?
                basket.B_DeliveryCountryID.GetValueOrDefault(0) :
                0;
            var selectedAddresses = ECommerceService.GetAllAddressesAsSelectList(id, countryID);

            return Json(new { success = true, addresses = list, d_addresses = selectedAddresses });
        }

        [HttpPost]
        public JsonResult GetSelectedAddress(int addressID)
        {
            var address = ECommerceService.GetAddressByID(addressID);
            if (address == null || address.A_CustomerID != this.AdminUser.UserID)
                return Json(new { success = false });

            var jsonAddress = new AddressModel
            {
                CountryID = address.A_CountryID.GetValueOrDefault(0),
                FirstName = address.A_FirstName,
                Surname = address.A_Surname,
                Title = address.A_Title,
                Address1 = address.A_Line1,
                Address2 = address.A_Line2,
                Address3 = address.A_Line3,
                Phone = address.A_Phone,
                Town = address.A_Town,
                AddressID = addressID,
                County = address.A_County,
                Postcode = address.A_Postcode
            };
            return Json(new { success = true, address = jsonAddress });
        }

        public ActionResult Addresses(int id = 0)
        {
            var model = new AddressModel();
            if (id != 0)
            {
                var temp = ECommerceService.GetAddressByID(id);
                if (temp == null || temp.A_CustomerID != this.AdminUser.UserID)
                    return RedirectToAction("Content");

                model.Address1 = temp.A_Line1;
                model.Address2 = temp.A_Line2;
                model.Address3 = temp.A_Line3;
                model.Country = temp.A_Country;
                model.CountryID = temp.A_CountryID.GetValueOrDefault(0);
                model.County = temp.A_County;
                model.Title = temp.A_Title;
                model.FirstName = temp.A_FirstName;
                model.Surname = temp.A_Surname;
                model.Phone = temp.A_Phone;
                model.Postcode = temp.A_Postcode;
                model.Town = temp.A_Town;
                model.AddressID = temp.AddressID;
            }

            model.countryList = ECommerceService.GetAllCountriesAsSelectList(DomainID, model.CountryID);
            return View(model);
        }

        [HttpPost]
        public ActionResult Addresses(AddressModel addressModel)
        {
            if (ModelState.IsValid)
            {
                var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
                ECommerceService.SaveAddress(customer.CustomerID, addressModel.CountryID, addressModel.County, addressModel.FirstName, addressModel.Surname,
                    addressModel.Title, addressModel.Address1, addressModel.Address2, addressModel.Address3, addressModel.Postcode, addressModel.Phone,
                    addressModel.Town, addressModel.AddressID);
                return RedirectToRoute("account");
            }
            return View(addressModel);
        }

        public ActionResult DeleteAddress(int addressID)
        {
            var address = ECommerceService.GetAddressByID(addressID);
            if (address != null && address.A_CustomerID == this.AdminUser.UserID)
                ECommerceService.DeleteAddress(addressID);
            return RedirectToRoute("account");
        }

        public ActionResult OrderHistory()
        {
            return View(ECommerceService.GetAllOrdersOfCustomer(this.AdminUser.UserID));
        }

        #endregion


        #region Blog

        [AllowAnonymous]
        public ActionResult Blog(string year, string month, string title)
        {
            var blogUrl = WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            this.ViewBag.BlogUrl = blogUrl;
            //var blogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : blogUrl;
            //this.ViewBag.BlogBaseUrl = blogUrl;

            if (!String.IsNullOrEmpty(year) && !String.IsNullOrEmpty(month) && !String.IsNullOrEmpty(title))
            {
                tbl_Content blog = WebContentService.GetContentBySitemapUrl(String.Format("/{0}/{1}/{2}", year, month, title), this.DomainID);
                if (blog == null)
                    throw new HttpException(404, "Page Not Found");

                this.ViewBag.Title = blog.C_Title;
                this.ViewBag.ShareThisID = this.Domain != null ? this.Domain.DO_ShareThis : String.Empty;
                this.ViewBag.Content = ContentModelFactory(blog);
                this.ViewBag.DynamicLayout = GetLayout(blog, WebPagesService);
                return View("BlogItem", blog);
            }
            else
            {
                this.ViewBag.BlogPage = WebContentService.GetContentBySitemapUrl("/" + blogUrl, this.DomainID);
                if (!String.IsNullOrEmpty(year) && !String.IsNullOrEmpty(month) && String.IsNullOrEmpty(title))
                {
                    List<tbl_Content> blogs = WebContentService.GetContentBySitemapDate(year, month, this.DomainID);
                    this.ViewBag.Title = ParseDate(year, month).ToString("yyyy MMMM");
                    this.ViewBag.Header = String.Format("Archive For '{0}'", ParseDate(year, month).ToString("MMMM, yyyy"));
                    this.ViewBag.NoNewsFound = (blogs == null || blogs.Count == 0) ? String.Format("No news for '{0}'", this.ViewBag.Title) : string.Empty;
                    return View("Blog", blogs);
                }
                else
                {
                    List<tbl_Content> blogs = WebContentService.GetContentByContentType(ContentType.Blog, this.DomainID, 10);
                    //this.ViewBag.Title = String.Format("{0}'s Latest News", this.Domain != null ? this.Domain.DO_Domain : String.Empty);
                    this.ViewBag.Header = "Latest News"; 
                    this.ViewBag.Title = "Latest News";
                    this.ViewBag.NoNewsFound = (blogs == null || blogs.Count == 0) ? "No latest news" : string.Empty;
                    return View("Blog", blogs);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult HomepageBlogs()
        {
            var blogUrl = WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            this.ViewBag.BlogUrl = blogUrl;
            //var blogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : blogUrl;
            //this.ViewBag.BlogBaseUrl = blogUrl;
            List<tbl_Content> blogs = WebContentService.GetContentByContentType(ContentType.Blog, this.DomainID, 3);
            return PartialView("BlogSummary", blogs);

        }

        [AllowAnonymous]
        public ActionResult BlogCategory(string name)
        {
            List<tbl_Content> blogs = WebContentService.GetContentByCategoryUrl(name, this.DomainID);
            tbl_Categories category = WebContentService.GetCategoryByURL(name);

            this.ViewBag.Header = String.Format("Archive for the '{0}' Category", category != null ? category.CA_Title : name);
            this.ViewBag.Title = category != null ? category.CA_Title : name;
            this.ViewBag.NoNewsFound = (blogs == null || blogs.Count == 0) ? String.Format("No news for '{0}' category", name) : string.Empty;

            this.ViewBag.BlogUrl = WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');

            //var blogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : blogUrl;
            //this.ViewBag.BlogBaseUrl = blogUrl;

            this.ViewBag.BlogPage = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).TrimEnd('/'), this.DomainID);
            return View("Blog", blogs);
        }

        [AllowAnonymous]
        public ActionResult BlogSearch(string keyword)
        {
            List<tbl_Content> blogs = WebContentService.GetContentByContentType(ContentType.Blog, this.DomainID).Where(c => (c.C_Content.Contains(keyword) || c.C_Title.Contains(keyword))).ToList();
            this.ViewBag.Header = String.Format("Search Results for '{0}'", keyword);
            this.ViewBag.NoNewsFound = blogs == null || blogs.Count() == 0 ? String.Format("No Search Results for '{0}'", keyword) : string.Empty;

            this.ViewBag.BlogUrl = WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');

            //var blogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : blogUrl;
            //this.ViewBag.BlogBaseUrl = blogUrl;

            this.ViewBag.BlogPage = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).TrimEnd('/'), this.DomainID);
            return View("Blog", blogs);
        }

        [AllowAnonymous]
        public ActionResult BlogTag(string name)
        {
            List<tbl_Content> blogs = WebContentService.GetContentByTagUrl(name, this.DomainID);
            tbl_Tags tag = WebContentService.GetTagByURL(name);
            this.ViewBag.Header = String.Format("Posts Tagged '{0}'", tag != null ? tag.TA_Title : name);
            this.ViewBag.Title = name;

            this.ViewBag.BlogUrl = WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');

            //var blogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : blogUrl;
            //this.ViewBag.BlogBaseUrl = blogUrl;

            this.ViewBag.BlogPage = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).TrimEnd('/'), this.DomainID);
            return View("Blog", blogs);
        }

        [AllowAnonymous]
        public ActionResult BlogSideBar()
        {
            this.ViewBag.Categories = WebContentService.GetAllCategoriesLive(this.DomainID);
            this.ViewBag.UniqueTags = WebContentService.GetUniqueTags(this.DomainID);
            List<tbl_Content> blogs = WebContentService.GetContentByContentType(ContentType.Blog, this.DomainID);
            this.ViewBag.RecentPosts = (blogs != null) ?
                blogs.Take(SettingsManager.Blog.RecentItemsAmount).ToList() :
                new List<tbl_Content>();
            this.ViewBag.Archive = (blogs != null) ?
                blogs.Where(b => b.tbl_SiteMap.SM_Date.HasValue)
                    .Select(b => new DateTime(b.tbl_SiteMap.SM_Date.Value.Year, b.tbl_SiteMap.SM_Date.Value.Month, 1))
                    .Distinct().ToList() :
                new List<DateTime>();
            this.ViewBag.BlogUrl = WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');

            //var blogUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/');
            //this.ViewBag.BlogUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : blogUrl;
            //this.ViewBag.BlogBaseUrl = blogUrl;

            return PartialView("BlogSideBar");
        }

        [AllowAnonymous]
        public ActionResult CommentForm(int sitemapID)
        {
            this.ViewBag.SitemapID = sitemapID;
            this.ViewBag.PublicKey = DomainService.GetSettingsValue(SettingsKey.recaptcha_public_key);
            return PartialView("CommentForm");
        }

        [AllowAnonymous]
        public ActionResult SaveComment(CommentModel comment)
        {
            RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper(DomainService.GetSettingsValue(SettingsKey.recaptcha_private_key));
            if (String.IsNullOrEmpty(recaptchaHelper.Response))
            {
                return Json(new { error = "Captcha answer cannot be empty." }, "text/html");
            }

            RecaptchaVerificationResult recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();
            if (recaptchaResult != RecaptchaVerificationResult.Success)
            {
                return Json(new { error = "Incorrect captcha answer." }, "text/html");
            }

            if (ModelState.IsValid)
            {
                tbl_Comments dbComment = WebContentService.SaveComment(comment.Name, comment.Email, comment.Website, comment.Message, comment.SitemapID, 0);
                if (dbComment == null)
                    return Json(new { success = false });

                string companyName = this.Domain != null ? this.Domain.DO_CompanyName : string.Empty;
                string domain = this.Domain != null ? this.Domain.DO_Domain : string.Empty;
                var postUrl = String.Format("http://{0}{1}", domain, dbComment.tbl_SiteMap.SM_URL);
                var adminUrl = String.Format("http://{0}{1}", domain, "/Admn/News");
                MailingService.SendNewComment(postUrl, adminUrl, companyName, domain, dbComment.tbl_SiteMap.tbl_Domains.DO_Email);
                return Json(new { success = comment != null }, "text/html");
            }
            return Json(new { success = false }, "text/html");
        }

        [AllowAnonymous]
        public ActionResult GetBlogRss()
        {
            List<tbl_Content> blogs = WebContentService.GetContentByContentType(ContentType.Blog, this.DomainID).Take(SettingsManager.Blog.RssItemsAmount).ToList();
            List<SyndicationItem> rssItems = new List<SyndicationItem>();
            foreach (var blogItem in blogs)
            {
                Uri url = new Uri("http://" + (this.Domain != null ? this.Domain.DO_Domain : String.Empty) + blogItem.tbl_SiteMap.SM_URL);
                var rssItem = new SyndicationItem(blogItem.C_Title, blogItem.C_Description, url, blogItem.ContentID.ToString(), DateTime.Now);
                rssItem.Authors.Add(new SyndicationPerson(blogItem.tbl_SiteMap.SM_NotifyEmail));
                rssItem.PublishDate = new DateTimeOffset(blogItem.tbl_SiteMap.SM_Date.GetValueOrDefault(DateTime.Now));
                rssItems.Add(rssItem);
            }

            SyndicationFeed feed = new SyndicationFeed(rssItems);
            if (this.Domain != null)
            {
                feed.Copyright = new TextSyndicationContent(String.Format("Copyright {0}", DateTime.Now.Year));
                feed.Description = new TextSyndicationContent("All the latest news from " + this.Domain.DO_CompanyName);
                feed.ImageUrl = null;
                feed.Language = "en-gb";
                feed.Links.Add(new SyndicationLink(new Uri("http://" + this.Domain.DO_Domain)));
                feed.Title = new TextSyndicationContent(this.Domain.DO_CompanyName);
            }

            return new RssResult(feed);
        }

        #endregion Blog

        #region Portfolio

        [AllowAnonymous]
        public ActionResult Portfolio()
        {
            var portfolio = new List<tbl_Portfolio>();

            tbl_Content content = null;
            portfolio.AddRange(PortfolioService.GetAll().Where(po => po.PO_Live).OrderBy(po => po.tbl_PortfolioCategory.POC_Title).ThenBy(po => po.PO_Order));
            content = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).TrimEnd('/'), this.DomainID);
            this.ViewBag.PortfolioUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).Trim('/');
            //this.ViewBag.PortfolioUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).Trim('/'));

            if (content == null)
                throw new HttpException(404, "Page Not Found");


            this.ViewBag.PortfolioPage = content;
            this.ViewBag.Content = ContentModelFactory(content);
            this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);
            return View("Portfolio", portfolio);
        }

        [AllowAnonymous]
        public ActionResult PortfolioItem(string query)
        {
            var url = String.Format("/{0}", query).TrimEnd('/');
            tbl_Content content = WebContentService.GetContentBySitemapUrl(url, this.DomainID);

            this.ViewBag.PortfolioUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).Trim('/');
            //this.ViewBag.PortfolioUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).Trim('/'));
            this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);
            return View(content);
        }

        [AllowAnonymous]
        public ActionResult PortfolioSummary()
        {
            var portfolio = new List<tbl_Portfolio>();

            tbl_Content content = null;
            portfolio.AddRange(PortfolioService.GetAll().Where(po => po.PO_Live).OrderBy(po => po.tbl_PortfolioCategory.POC_Title).ThenBy(po => po.PO_Order));
            content = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).TrimEnd('/'), this.DomainID);
            this.ViewBag.PortfolioUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).Trim('/');
            //this.ViewBag.PortfolioUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.Portfolio, this.DomainID).Trim('/'));
            this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);
            return PartialView("~/Views/Partials/PortfolioSummary.cshtml", portfolio);
        }
        #endregion

        #region Product

        [AllowAnonymous]
        public ActionResult EventsCategories()
        {
            return ProdCategories(ProductType.Event);
        }


        [AllowAnonymous]
        public ActionResult ProdCategories(ProductType type = ProductType.Item)
        {
            var categories = new List<tbl_ProdCategories>();

            tbl_Content content = null;
            if (type == ProductType.Item && this.Domain.DO_EnableProductSale)
            {
                categories.AddRange(ECommerceService.GetProdCategoriesForDomain(this.DomainID, 0, false, ProductType.Item));
                content = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).TrimEnd('/'), this.DomainID);
                this.ViewBag.ShopUrl = "/" + (this.Domain.DO_CustomRouteHandler ? string.Empty : (WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/') + '/'));
            }
            else if (type == ProductType.Event && this.Domain.DO_EnableEventSale)
            {
                categories.AddRange(ECommerceService.GetProdCategoriesForDomain(this.DomainID, 0, false, ProductType.Event));
                content = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).TrimEnd('/'), this.DomainID);
                this.ViewBag.ShopUrl = "/" + (this.Domain.DO_CustomRouteHandler ? string.Empty : (WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/') + '/'));
            }

            if (content == null)
                throw new HttpException(404, "Page Not Found");


            this.ViewBag.CategoriesPage = content;
            this.ViewBag.Content = ContentModelFactory(content);
            this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);

            return View("ProdCategories", categories);
        }

        [AllowAnonymous]
        public ActionResult FeaturedCategories()
        {
            List<tbl_ProdCategories> featured = ECommerceService.GetFeaturedProdCategories();
            this.ViewBag.ShopUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/');
            return PartialView("FeaturedCategories", featured);
        }

        [AllowAnonymous]
        public ActionResult Events(string query)
        {
            return Products(query, ProductType.Event);
        }

        [AllowAnonymous]
        public ActionResult Products(string query, ProductType productType = ProductType.Item)
        {
            var url = String.Format("/{0}", query).TrimEnd('/');
            tbl_Content content = WebContentService.GetContentBySitemapUrl(url, this.DomainID);

            int priceID = 0;

            if (content == null)
                content = ParseProductQuery(query, out priceID);

            this.ViewBag.ShopUrl = this.Domain.DO_CustomRouteHandler 
                ? "/" 
                : string.Format("/{0}/", ((productType == ProductType.Item) 
                    ? WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/')
                    : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/')));

            if (content.tbl_SiteMap.IsType(ContentType.Category))
            {
                if (!content.tbl_SiteMap.tbl_ProdCategories.PC_Live)
                    throw new HttpException(404, "Page Not Found");
                ProductType type = content.tbl_SiteMap.tbl_ProdCategories.tbl_ProductTypes.PT_Name.Equals(ProductType.Item.ToString()) ? ProductType.Item : ProductType.Event;
                if ((type == ProductType.Event && (productType != ProductType.Event || !this.Domain.DO_EnableEventSale)) ||
                         (type == ProductType.Item && (productType != ProductType.Item || !this.Domain.DO_EnableProductSale)))
                {
                    Log.Debug(String.Format("Requested item is not enabled, CategoryID = {0}", content.tbl_SiteMap.tbl_ProdCategories.CategoryID));
                    throw new HttpException(404, "Page Not Found");
                }
                var categories = new List<tbl_ProdCategories>();
                categories.AddRange(ECommerceService.GetProdCategoriesForDomain(this.DomainID, content.tbl_SiteMap.tbl_ProdCategories.CategoryID, false, type));
                this.ViewBag.Categories = categories;
                this.ViewBag.ViewType = Enum.Parse(typeof(EventViewType), this.Domain.DO_DefaultEventView.ToString());
                this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);

                return View("ProdCategory", content.tbl_SiteMap.tbl_ProdCategories);
            }
            else if (content.tbl_SiteMap.IsType(ContentType.Product))
            {
                if (!content.tbl_SiteMap.tbl_Products.P_Live)
                    throw new HttpException(404, "Page Not Found");
                ProductType type = content.tbl_SiteMap.tbl_Products.tbl_ProductTypes.PT_Name.Equals(ProductType.Item.ToString()) ? ProductType.Item : ProductType.Event;
                if ((type == ProductType.Event && (productType != ProductType.Event || !this.Domain.DO_EnableEventSale)) ||
                         (type == ProductType.Item && (productType != ProductType.Item || !this.Domain.DO_EnableProductSale)))
                {
                    Log.Debug(String.Format("Requested item is not enabled, ProductID = {0}", content.tbl_SiteMap.tbl_Products.ProductID));
                    throw new HttpException(404, "Page Not Found");
                }

                if (type == ProductType.Event)
                {
                    var price = ECommerceService.GetProductPriceByID(priceID);

                    var dates = content.tbl_SiteMap.tbl_Products.tbl_ProductPrice.Where(pp => IsActual(pp))
                        .Select(m => new { m.PR_EventStartDate, m.PR_EventEndDate }).OrderBy(d => d.PR_EventStartDate)
                        .ThenBy(d => d.PR_EventEndDate).Distinct().AsEnumerable();
                    this.ViewBag.eventDates = dates.Select(m =>
                        (!m.PR_EventEndDate.HasValue || (m.PR_EventEndDate == m.PR_EventStartDate)) ?
                        new SelectListItem
                        {
                            Text = String.Format("{0:d MMM HH:mm}", m.PR_EventStartDate),
                            Value = String.Format("{0}", m.PR_EventStartDate),
                            Selected = (price != null) ? m.PR_EventEndDate == price.PR_EventEndDate && m.PR_EventStartDate == price.PR_EventStartDate : false
                        } :
                        new SelectListItem
                        {
                            Text = String.Format("{0:d MMM HH:mm} - {1:d MMM HH:mm}",
                                m.PR_EventStartDate, m.PR_EventEndDate),


                            Value = String.Format("{0}_{1}", m.PR_EventStartDate, m.PR_EventEndDate),

                            Selected = (price != null) ?
                                m.PR_EventEndDate == price.PR_EventEndDate && m.PR_EventStartDate == price.PR_EventStartDate : false
                        }
                    ).ToList();
                }
                this.ViewBag.PriceID = priceID;
                this.ViewBag.ProductSaleEnabled = this.Domain.DO_EnableProductSale;
                this.ViewBag.EventSaleEnabled = this.Domain.DO_EnableEventSale;
                this.ViewBag.ProductShopUrl = this.Domain.DO_CustomRouteHandler ? "/" : string.Format("/{0}/", WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/'));
                this.ViewBag.EventShopUrl = this.Domain.DO_CustomRouteHandler ? "/" : string.Format("/{0}/", WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/'));

                return View("Product", content.tbl_SiteMap.tbl_Products);
            }
            return View("Product");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetEventView(EventViewType type, int categoryID)
        {
            if (type == EventViewType.Calendar)
                return PartialView("~/Views/Partials/EventsCalendar.cshtml");

            List<tbl_Products> products = new List<tbl_Products>();

            var category = ECommerceService.GetProdCategoryByID(categoryID);
            if (category != null)
                products = category.tbl_Products.GetWithContent(ProductType.Event).ToList();

            products = products.Where(p => p.tbl_ProductPrice.Any(pp => IsActual(pp))).ToList();
            return PartialView("EventsList", products);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetEventsData(int categoryID, long start, long end)
        {
            System.DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime startDate = date.AddSeconds(start);
            DateTime endDate = date.AddSeconds(end);

            var category = ECommerceService.GetProdCategoryByID(categoryID);
            var productPrices = category.tbl_Products.GetWithContent(ProductType.Event)
                .Where(p => p.P_Live)
                .SelectMany(p => p.tbl_ProductPrice)
                .Where(pp => IsActual(pp) &&
                    ((pp.PR_EventStartDate.HasValue && pp.PR_EventStartDate.Value >= startDate && pp.PR_EventStartDate.Value <= endDate) ||
                    (pp.PR_EventEndDate.HasValue && pp.PR_EventEndDate.Value >= startDate && pp.PR_EventEndDate.Value <= endDate)))
                .Distinct(new ProductPriceComparer()).ToList();

            var shopUrl = WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/');

            List<CalendarItem> events = new List<CalendarItem>();
            foreach (var item in productPrices)
            {
                var prices = item.tbl_Products.tbl_ProductPrice
                    .Where(m => m.PR_EventStartDate == item.PR_EventStartDate && m.PR_EventEndDate == item.PR_EventEndDate).OrderBy(p => p.GetPrice());

                var max = prices.First().GetPrice();
                var min = prices.Last().GetPrice();
                var title = (min == max) ?
                    String.Format("{0} ({1})", item.tbl_Products.P_Title, prices.First().GetPriceString(1, true)) :
                    String.Format("{0} (from {1} to {2})", item.tbl_Products.P_Title, prices.First().GetPriceString(1, true), prices.Last().GetPriceString(1, true));

                events.Add(new CalendarItem
                {
                    id = item.PriceID,
                    title = title,
                    start = item.PR_EventStartDate.GetValueOrDefault().ToCustomDateTimeString(),
                    end = item.PR_EventEndDate.HasValue ? item.PR_EventEndDate.Value.ToCustomDateTimeString() : item.PR_EventStartDate.GetValueOrDefault().AddDays(1).Date.Subtract(new TimeSpan(0, 1, 0)).ToCustomDateTimeString(),
                    url = String.Format("/{0}/{1}/{2}", shopUrl, item.tbl_Products.tbl_SiteMap.SM_URL.Trim('/'), item.PriceID),
                    editable = false
                });
            }

            return Json(events);
        }
        [AllowAnonymous]
        public ActionResult GetAllEventsData(long start = 0, long end = 0)
        {
            System.DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            System.DateTime thisDay = DateTime.Today;
            var thisMonthStart = thisDay.AddDays(1 - thisDay.Day);
            var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
            DateTime startDate = start == 0 ? thisMonthStart : baseDate.AddSeconds(start);
            DateTime endDate = end == 0 ? thisMonthEnd : baseDate.AddSeconds(end);

            this.ViewBag.ShopUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/');

            List<tbl_Products> allProducts = ECommerceService.GetAllProductsByType(ProductType.Event).SelectMany(
                p => p.tbl_ProductPrice).Where(p => (p.PR_EventStartDate > DateTime.UtcNow && p.PR_EventStartDate.Value >= startDate && p.PR_EventStartDate.Value <= endDate)).OrderBy(
                p => p.PR_EventStartDate).Select(pp => pp.tbl_Products).Distinct().Take(3).ToList();
            return PartialView("~/Views/Partials/EventsSummary.cshtml", allProducts);
        }

        [AllowAnonymous]
        public ActionResult FeaturedProducts()
        {
            List<tbl_Products> featured = ECommerceService.GetFeaturedProductsByType(ProductType.Item);
            this.ViewBag.ShopUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/');
            return PartialView("FeaturedProducts", featured);
        }

        [AllowAnonymous]
        public ActionResult GetAllSubCats()
        {
            return PartialView("SubMenu", GetCategoryMenuOrdered(0, true, ProductType.Item));
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetPrice(int productID, int amount, int[] attrs, string selectedDate)
        {
            var price = ECommerceService.GetProductPrice(productID, attrs, selectedDate);
            if (price == null)
                return Json(new { success = false, warning = true, message = "There is no product for selected attributes." });

            if (price.tbl_Products.P_StockControl)
            {
                int currentAmmount = 0;
                var basket = FindBasket();
                if (basket != null)
                {
                    var content = basket.tbl_BasketContent.FirstOrDefault(m => m.tbl_ProductPrice.PriceID == price.PriceID);
                    if (content != null)
                        currentAmmount = content.BC_Quantity;
                }
                return Json(new { success = true, price = price.GetPriceString(amount), insufficientStock = price.PR_Stock.GetValueOrDefault(0) - currentAmmount < amount });
            }
            return Json(new { success = true, price = price.GetPriceString(amount), insufficientStock = false });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult AddToBasket(int productID, int amount, int[] attrs, string selectedDate)
        {
            var prod = ECommerceService.GetProductByID(productID);
            if ((prod.P_ProductTypeID == (int)ProductType.Event && !this.Domain.DO_EnableEventSale) ||
             (prod.P_ProductTypeID == (int)ProductType.Item && !this.Domain.DO_EnableProductSale))
                return Json(new { success = false });

            if (amount < 1)
                return Json(new { success = false });

            tbl_Basket basket = FindBasket();

            if (basket != null)
                basket = ECommerceService.AddContentToBasket(basket.BasketID, productID, amount, attrs, selectedDate);
            else
            {
                basket = ECommerceService.SaveBasketWithContent((Request.IsAuthenticated && !AdminUser.IsAdmn) ? this.AdminUser.UserID : 0, (int?)null, Session.SessionID, DomainID, 0, productID, amount, attrs, selectedDate);
                if (basket != null)
                    CookieManager.BasketCookie = basket.BasketID;
            }
            return Json(new { success = basket != null });
        }

        #endregion


        #region Donation

        [AllowAnonymous]
        public ActionResult DonationCategories()
        {
            this.ViewBag.CategoriesPage = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Donation, this.DomainID).TrimEnd('/'), this.DomainID);
            // Only single donation type for now
            var types = new SelectList(ECommerceService.GetAllDonationType().Where(dt => dt.DT_Name.Equals(DonationType.Single.ToString())), "DonationTypeID", "DT_Name");
            this.ViewBag.DonationsType = types;
            var model = ECommerceService.GetAllDonationsInfoForDomain(this.DomainID, false).Where(di => di.tbl_DonationType.DT_Name.Equals(DonationType.Single.ToString())).ToList();
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult DonationCheckout(DonationType type, decimal amount)
        {
            if (type != DonationType.Single)
                return new HttpNotFoundResult("Only single donation allowed.");

            var model = new DonationCheckoutModel
            {
                Amount = amount.ToString("F2"),
                Type = type
            };

            this.ViewBag.Donations = ECommerceService.GetAllDonationsInfoForDomainByType(this.DomainID, DonationType.Single);
            this.ViewBag.Countries = ECommerceService.GetAllCountriesAsSelectList(this.DomainID);

            if (type == DonationType.Monthly)
            {
                //this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true)).ToList();
            }
            else
            {
                bool isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, this.DomainID).ToLowerInvariant().Equals(SagePayPaymentType.Direct.ToString().ToLowerInvariant());
                this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true);
                this.ViewBag.IsDirect = isDirect;
                this.ViewBag.SagePay = ECommerceService.GetPaymentDomainIDByCode(this.DomainID, PaymentType.SagePay);
                if (isDirect)
                    this.ViewBag.CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>().Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult DonationCheckout(DonationCheckoutModel model, CreditCardModel cardModel)
        {
            ModelState.Clear();

            this.ViewBag.Donations = ECommerceService.GetAllDonationsInfoForDomainByType(this.DomainID, DonationType.Single);
            this.ViewBag.Countries = ECommerceService.GetAllCountriesAsSelectList(this.DomainID);

            bool isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, this.DomainID).ToLowerInvariant().Equals(SagePayPaymentType.Direct.ToString().ToLowerInvariant());
            if (model != null && model.Type == DonationType.Monthly)
            {
                //this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true)).ToList();
            }
            else
            {
                this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true);
                this.ViewBag.IsDirect = isDirect;
                this.ViewBag.SagePay = ECommerceService.GetPaymentDomainIDByCode(this.DomainID, PaymentType.SagePay);
                if (isDirect)
                    this.ViewBag.CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>().Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();
            }

            if (model.Type != DonationType.Single)
            {
                ModelState.AddModelError("", "Some of the values are incorrect.");
                return View(model);
            }

            var selectedPaymentType = ECommerceService.GetPaymentDomainByID(model.PaymentDomainID);
            if (selectedPaymentType == null)
            {
                ModelState.AddModelError("PaymentDomainID", "Please select payment type.");
                return View(model);
            }

            if (selectedPaymentType.tbl_PaymentType.PT_Code == PaymentType.SagePay.ToString() && isDirect &&
                (cardModel == null || !TryValidateModel(cardModel)))
            {
                ModelState.AddModelError("", "There was a problem saving card details.");
                return View(model);
            }

            decimal amount = 0;
            if (!Decimal.TryParse(model.Amount.ChangeDecimalSeparator(), out amount) && amount > 0)
            {
                ModelState.AddModelError("Amount", "Please specify correct amount");
                return View(model);
            }

            if (TryValidateModel(model))
            {
                int customerID = Request.IsAuthenticated && !AdminUser.IsAdmn ? AdminUser.UserID : 0;
                tbl_Orders order = ECommerceService.SaveOrderForDonation(0, this.DomainID, model.Address1, model.Address2, model.Address3, model.City, model.CountryID, model.FirstName,
                    model.Phone, model.Postcode, model.State, model.Surname, model.EmailAddress, customerID, amount, model.GiftAid, selectedPaymentType.PaymentDomainID, DonationType.Single, null);

                PaymentType key = (PaymentType)Enum.Parse(typeof(PaymentType), selectedPaymentType.tbl_PaymentType.PT_Code);
                switch (key)
                {
                    case PaymentType.SagePay:
                        if (isDirect)
                            SessionManager.CreditCard = cardModel;

                        return RedirectToRoute("SagePay", new { action = "Payment", orderID = order.OrderID });

                    case PaymentType.PayPal:
                        return RedirectToRoute("PayPal", new { action = "Payment", orderID = order.OrderID });

                    case PaymentType.SecureTrading:

                        return RedirectToRoute("SecureTrading", new { action = "Payment", orderID = order.OrderID });
                    default:
                        return View(model);
                }
            }
            ModelState.AddModelError("", "Some of the values are incorrect.");
            return View(model);
        }

        #endregion Donation


        #region Basket

        [AllowAnonymous]
        public ActionResult BasketSummary()
        {
            return Basket(true);
        }

        [AllowAnonymous]
        public ActionResult Basket(bool isSummary = false)
        {
            this.ViewBag.DisplayTax = DomainService.GetSettingsValueAsBool(SettingsKey.useTax, this.DomainID);
            // for shop buttons
            this.ViewBag.ProductSaleEnabled = this.Domain.DO_EnableProductSale;
            this.ViewBag.ProductUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID);
            this.ViewBag.EventSaleEnabled = this.Domain.DO_EnableEventSale;
            this.ViewBag.EventUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID);

            return (isSummary) ? (ActionResult)PartialView("BasketSummary", FindBasket()) : View();
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetBasketData()
        {
            tbl_Basket basket = FindBasket();
            if (basket == null || basket.tbl_BasketContent.Count == 0)
                return Json(new { success = false });

            var basketItems = basket.tbl_BasketContent.Select(bc => new
            {
                BasketContentID = bc.BaskContentID,
                CategoryTitle = bc.tbl_ProductPrice != null ? bc.tbl_ProductPrice.tbl_Products.tbl_ProdCategories.PC_Title : String.Empty,
                ProductTitle = bc.tbl_ProductPrice != null ? bc.tbl_ProductPrice.tbl_Products.P_Title : String.Empty,
                Attributes = bc.tbl_ProductPrice != null ?
                    bc.tbl_ProductPrice.tbl_ProdPriceAttributes.Select(pa => pa.tbl_ProdAttValue)
                        .Aggregate(String.Empty, (total, seed) => (total += String.Format("{0}: {1} <br />", seed.tbl_ProdAttributes.A_Title, seed.AV_Value))) : String.Empty,
                ItemSinglePrice = bc.GetItemPriceString(true),
                TaxValue = bc.GetTaxValueString(),
                Quantity = bc.BC_Quantity,
                Min = bc.tbl_ProductPrice != null ? bc.tbl_ProductPrice.tbl_Products.P_MinQuantity : 1,
                Max = bc.tbl_ProductPrice != null && bc.tbl_ProductPrice.tbl_Products.P_StockControl ? bc.tbl_ProductPrice.PR_Stock.GetValueOrDefault(Int32.MaxValue) : Int32.MaxValue,
                ItemPrice = bc.GetPriceString(true)
            });

            bool isDiscount = basket.B_DiscountID.HasValue && basket.B_DiscountID.Value > 0;
            string promotionalCodeFormat = isDiscount ? basket.tbl_Discount.D_IsPercentage ? "{0} ({1}%)" : "{0} ({1}:C)" : String.Empty;
            string promotionalCode = isDiscount ? String.Format(promotionalCodeFormat, basket.tbl_Discount.D_Title, basket.tbl_Discount.D_Value) : String.Empty;
            string discountAmount = basket.GetDiscountAmountString();

            bool isDeliverable = basket.IsDeliverable;
            string deliveryCost = basket.GetDeliveryAmountString(true);

            string euVAT = basket.B_VATNumber;
            string totalTax = basket.GetTaxAmountString();
            string totalPrice = basket.GetPrice(true).ToString("C");

            var countries = isDeliverable ? ECommerceService.GetPostageCountriesAsSelectList(this.DomainID, basket.B_DeliveryCountryID.GetValueOrDefault(0)) : new List<SelectListItem>();
            countries.Insert(0, new SelectListItem() { Text = "Select country", Value = "0" });

            return Json(new
            {
                success = true,
                basketItems,
                isDiscount,
                promotionalCode,
                discountAmount,
                isDeliverable,
                deliveryCost,
                euVAT,
                totalTax,
                totalPrice,
                countries,
                countryID = basket.B_DeliveryCountryID.GetValueOrDefault(0)
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetAvailablePostage(int countryID = 0)
        {
            tbl_Basket basket = FindBasket();
            if (basket == null || !basket.IsDeliverable)
                return Json(new { success = false });

            bool useTax = DomainService.GetSettingsValueAsBool(SettingsKey.useTax, this.DomainID);
            bool includeVat = DomainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, this.DomainID);

            if (countryID != 0)
                basket = ECommerceService.UpdateBasketDeliveryCountry(countryID, basket.BasketID);

            int? zoneID = ECommerceService.GetPostageZoneID(countryID);
            if (!zoneID.HasValue)
                return Json(new { success = false });

            decimal maxTax = basket.tbl_BasketContent.Max(bc => bc.GetTaxValue());
            string postageType = DomainService.GetSettingsValue(SettingsKey.postageTypeSetting, this.DomainID);
            List<SelectListItem> postages = ECommerceService.GetAvailablePostage(basket.GetPrice(), basket.GetWeight(), this.DomainID, postageType, zoneID.GetValueOrDefault(0)).Select(p => new SelectListItem()
            {
                Text = String.Format("{0} {1:C}", p.PST_Description, useTax && includeVat ? p.PST_Amount + (p.PST_Amount * maxTax / 100) : p.PST_Amount),
                Value = p.PostageID.ToString()
            }).ToList();
            postages.Insert(0, new SelectListItem() { Text = "Select delivery", Value = "0" });

            if (basket.tbl_Postage != null && !postages.Select(p => p.Value).Contains(basket.B_PostageID.Value.ToString()))
                basket = ECommerceService.UpdateBasketPostage(null, basket.BasketID);

            if (basket == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                postages = postages,
                postageID = basket.B_PostageID.GetValueOrDefault(0)
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult UpdateProductAmount(int contentID, int amount)
        {
            if (amount < 1)
                return Json(new { success = false });
            var content = ECommerceService.UpdateProductQuantity(contentID, amount);
            return Json(new { success = content != null });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult UpdateBasketPostage(int? postageID)
        {
            tbl_Basket basket = FindBasket();
            if (basket == null || !basket.IsDeliverable)
                return Json(new { success = false });

            basket = ECommerceService.UpdateBasketPostage(postageID, basket.BasketID);
            return Json(new { success = basket != null });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetBasketPrices()
        {
            tbl_Basket basket = FindBasket();
            if (basket == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                totalPrice = basket.GetPrice(true).ToString("C"),
                totalTax = basket.GetTaxAmountString(),
                deliveryCost = basket.GetDeliveryAmountString(true),
                discountAmount = basket.GetDiscountAmountString()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult CanProceedToCheckout(string EUVAT)
        {
            tbl_Basket basket = FindBasket();
            if (basket == null)
                return Json(new { success = false, error = "Can not find your basket." });

            if (basket.IsDeliverable && (!basket.B_DeliveryCountryID.HasValue || basket.B_DeliveryCountryID.Value == 0))
                return Json(new { success = false, error = "Select delivery country" });

            if (basket.IsDeliverable && (!basket.B_PostageID.HasValue || basket.B_PostageID.Value == 0))
                return Json(new { success = false, error = "Select delivery option" });

            return Json(new { success = ECommerceService.SaveEUVATForBasket(basket.BasketID, EUVAT) });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult AddDiscount(string code)
        {
            var basket = FindBasket();
            if (basket == null)
                return Json(new { success = false });

            basket = ECommerceService.AddDiscountToBasket(basket.BasketID, code, this.DomainID);
            return Json(new { success = basket != null });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult RemoveDiscount()
        {
            var basket = FindBasket();
            if (basket == null)
                return Json(new { success = false });

            basket = ECommerceService.RemoveDiscountFromBasket(basket.BasketID);
            return Json(new { success = basket != null });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult RemoveFromBasket(int contentID)
        {
            return Json(new { success = ECommerceService.DeleteBasketContent(contentID) });
        }

        #endregion


        #region Checkout

#if RELEASE
        [RequireHttps]
#endif
        [AllowAnonymous]
        public ActionResult Checkout()
        {
            var basket = FindBasket();
            if (basket == null)
                return RedirectToAction("Content");

            var model = new CheckoutModel(basket);
            var country = ECommerceService.GetCountry(model.DeliveryCountryID);
            this.ViewBag.SelectedCountry = (country != null) ? country.C_Country : String.Empty;
            this.ViewBag.Countries = ECommerceService.GetAllCountriesAsSelectList(DomainID);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Checkout(CheckoutModel model)
        {
            tbl_Basket basket = FindBasket();

            if (ModelState.IsValid)
            {
                int customerID = Request.IsAuthenticated && !AdminUser.IsAdmn ? AdminUser.UserID : 0;
                if (model.BillingAddressTheSame)
                {
                    basket = ECommerceService.UpdateBasket(model.DeliveryAddress1, model.DeliveryAddress2, model.DeliveryAddress3, model.DeliveryCity, model.DeliveryCountryID,
                        model.DeliveryTitle, model.DeliveryFirstName, model.DeliveryPhone, model.DeliveryPostcode, model.DeliveryState, model.DeliverySurname,
                        model.DeliveryAddress1, model.DeliveryAddress2, model.DeliveryAddress3, model.DeliveryCity, model.DeliveryCountry, model.DeliveryCountryID,
                        model.DeliveryTitle, model.DeliveryFirstName, model.DeliveryPhone, model.DeliveryPostcode, model.DeliveryState, model.DeliverySurname,
                        model.Instructions, model.Email, customerID, model.BillingAddressTheSame, model.Subscription, model.Permission, model.BasketID);
                }
                else
                {
                    basket = ECommerceService.UpdateBasket(model.BillingAddress1, model.BillingAddress2, model.BillingAddress3, model.BillingCity, model.BillingCountryID.GetValueOrDefault(0),
                        model.BillingTitle, model.BillingFirstName, model.BillingPhone, model.BillingPostcode, model.BillingState, model.BillingSurname,
                        model.DeliveryAddress1, model.DeliveryAddress2, model.DeliveryAddress3, model.DeliveryCity, model.DeliveryCountry, model.DeliveryCountryID,
                        model.DeliveryTitle, model.DeliveryFirstName, model.DeliveryPhone, model.DeliveryPostcode, model.DeliveryState, model.DeliverySurname,
                        model.Instructions, model.Email, customerID, model.BillingAddressTheSame, model.Subscription, model.Permission, model.BasketID);
                }
                return RedirectToRoute("orderSummary");
            }

            if (basket == null)
                return RedirectToAction("Content");

            this.ViewBag.Countries = ECommerceService.GetAllCountriesAsSelectList(DomainID);
            var country = ECommerceService.GetCountry(model.DeliveryCountryID);
            this.ViewBag.SelectedCountry = country != null ? country.C_Country : String.Empty;
            return View(model);
        }

#if RELEASE
        [RequireHttps]
#endif
        [AllowAnonymous]
        public ActionResult OrderSummary()
        {
            // TODO: view refactor using knockout and ajax without reloading page everytime, get discount info using ajax

            this.ViewBag.SagePay3DSecure = false;

            bool isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, this.DomainID).ToLowerInvariant()
                .Equals(SagePayPaymentType.Direct.ToString().ToLowerInvariant());
            this.ViewBag.DisplayTax = DomainService.GetSettingsValueAsBool(SettingsKey.useTax, this.DomainID);
            this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true);
            this.ViewBag.SagePay = ECommerceService.GetPaymentDomainIDByCode(this.DomainID, PaymentType.SagePay);
            this.ViewBag.IsDirect = isDirect;
            if (isDirect)
                this.ViewBag.CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>()
                    .Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();

            CheckoutModel model = new CheckoutModel(FindBasket()) { IsCheckoutStep = false };

            var dCountry = ECommerceService.GetCountry(model.DeliveryCountryID);
            model.DeliveryCountry = (dCountry != null) ? dCountry.C_Country : model.DeliveryCountry;
            var bCountry = ECommerceService.GetCountry(model.BillingCountryID.GetValueOrDefault(0));
            model.BillingCountry = (bCountry != null) ? bCountry.C_Country : model.BillingCountry;

            return View(model);
        }

#if RELEASE
        [RequireHttps]
#endif
        [AllowAnonymous]
        public ActionResult OrderSummaryWithIFrame(string md, string pareq, string vendorTxCode, string ACSURL)
        {
            this.ViewBag.MD = md;
            this.ViewBag.PaReq = pareq;
            this.ViewBag.VendorTxCode = vendorTxCode;
            this.ViewBag.ACSURL = ACSURL;
            this.ViewBag.SagePay3DSecure = true;

            bool isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, this.DomainID).ToLowerInvariant()
                .Equals(SagePayPaymentType.Direct.ToString().ToLowerInvariant());
            this.ViewBag.DisplayTax = DomainService.GetSettingsValueAsBool(SettingsKey.useTax, this.DomainID);
            this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true);
            this.ViewBag.SagePay = ECommerceService.GetPaymentDomainIDByCode(this.DomainID, PaymentType.SagePay);
            this.ViewBag.IsDirect = isDirect;
            if (isDirect)
                this.ViewBag.CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>()
                    .Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();

            tbl_Orders order = null;
            if (!String.IsNullOrWhiteSpace(vendorTxCode))
                order = ECommerceService.GetOrderByVendorCode(vendorTxCode, DomainID);
            if (order == null)
                return View("OrderSummary", null);

            CheckoutModel model = new CheckoutModel(FindBasket()) { IsCheckoutStep = false };
            var dCountry = ECommerceService.GetCountry(model.DeliveryCountryID);
            model.DeliveryCountry = (dCountry != null) ? dCountry.C_Country : model.DeliveryCountry;
            var bCountry = ECommerceService.GetCountry(model.BillingCountryID.GetValueOrDefault(0));
            model.BillingCountry = (bCountry != null) ? bCountry.C_Country : model.BillingCountry;
            model.DonationAmount = order.DependentOrders.Select(o => o.TotalAmountToPay).Sum().ToString();
            model.PaymentDomainID = order.O_PaymentDomainID.GetValueOrDefault(0);
            model.TermsAndConditionsConfirmed = true;
            model.GiftAid = order.DependentOrders.Count > 0 ? order.DependentOrders.First().GiftAid.GetValueOrDefault(false) : false;

            return View("OrderSummary", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult OrderSummary(CheckoutModel model, CreditCardModel cardModel)
        {
            ModelState.Clear();
            this.ViewBag.DisplayTax = DomainService.GetSettingsValueAsBool(SettingsKey.useTax, this.DomainID);

            bool isDirect = DomainService.GetSettingsValue(SettingsKey.sagePayMethod, this.DomainID).ToLowerInvariant()
                .Equals(SagePayPaymentType.Direct.ToString().ToLowerInvariant());
            this.ViewBag.PaymentTypes = ECommerceService.GetAllPaymentDomainAsSelectList(this.DomainID, true);
            this.ViewBag.SagePay = PaymentType.SagePay.ToString().ToLowerInvariant();
            this.ViewBag.IsDirect = isDirect;
            if (isDirect)
                this.ViewBag.CardTypes = Enum.GetValues(typeof(CardType)).Cast<CardType>()
                    .Select(c => new SelectListItem { Text = c.ToString(), Value = ((int)c).ToString() }).ToList();

            tbl_Basket basket = FindBasket();
            if (basket == null)
            {
                ModelState.AddModelError("", "There is no basket assigned to you.");
                return View();
            }

            model.CopyValuesFromBasket(basket);

            if (!model.TermsAndConditionsConfirmed)
            {
                ModelState.AddModelError("TermsAndConditionsConfirmed", "Please agree to our terms and conditions.");
                return View(model);
            }

            var selectedPaymentType = ECommerceService.GetPaymentDomainByID(model.PaymentDomainID);
            if (selectedPaymentType == null)
            {
                ModelState.AddModelError("PaymentDomainID", "Please select payment type.");
                return View(model);
            }

            if (selectedPaymentType.tbl_PaymentType.PT_Code == PaymentType.SagePay.ToString() && isDirect &&
                (cardModel == null || !TryValidateModel(cardModel)))
            {
                ModelState.AddModelError("", "There was a problem saving card details.");
                return View(model);
            }
            if (!ECommerceService.IsEnoughOnStock(basket.tbl_BasketContent))
            {
                ModelState.AddModelError("", "Quantity of order oversize current stock ammount");
                return View(model);
            }

            if (TryValidateModel(model))
            {
                //int customerID = Request.IsAuthenticated && !AdminUser.IsAdmn ? AdminUser.UserID : 0;
                tbl_Orders order = ECommerceService.SaveOrder(0, model.PaymentDomainID, (int?)null, basket.BasketID);
                if (order == null)
                {
                    ModelState.AddModelError("", "There was a problem saving new order.");
                    return View(model);
                }

                if (!String.IsNullOrEmpty(model.DonationAmount))
                {
                    decimal donationAmount = 0;
                    bool parsed = Decimal.TryParse(model.DonationAmount.ChangeDecimalSeparator(), out donationAmount);
                    if (parsed && donationAmount > 0)
                    {
                        if (model.BillingAddressTheSame && model.IsDeliverable)
                            ECommerceService.SaveOrderForDonation(0, this.DomainID, model.DeliveryAddress1, model.DeliveryAddress2,
                                model.DeliveryAddress3, model.DeliveryCity, model.DeliveryCountryID, model.DeliveryFirstName,
                                model.DeliveryPhone, model.DeliveryPostcode, model.DeliveryState, model.DeliverySurname, model.Email,
                                order.CustomerID, donationAmount, model.GiftAid, selectedPaymentType.PaymentDomainID,
                                DonationType.Single, order.OrderID);
                        else
                            ECommerceService.SaveOrderForDonation(0, this.DomainID, model.BillingAddress1, model.BillingAddress2,
                                model.BillingAddress3, model.BillingCity, model.BillingCountryID.Value, model.BillingFirstName,
                                model.BillingPhone, model.BillingPostcode, model.BillingState, model.BillingSurname, model.Email,
                                order.CustomerID, donationAmount, model.GiftAid, selectedPaymentType.PaymentDomainID,
                                DonationType.Single, order.OrderID);
                    }
                }

                PaymentType key = (PaymentType)Enum.Parse(typeof(PaymentType), selectedPaymentType.tbl_PaymentType.PT_Code);
                switch (key)
                {
                    case PaymentType.SagePay:
                        if (isDirect)
                            SessionManager.CreditCard = cardModel;

                        return RedirectToRoute("SagePay", new { action = "Payment", orderID = order.OrderID });

                    case PaymentType.PayPal:
                        return RedirectToRoute("PayPal", new { action = "Payment", orderID = order.OrderID });

                    case PaymentType.SecureTrading:
                        return RedirectToRoute("SecureTrading", new { action = "Payment", orderID = order.OrderID });

                    case PaymentType.Stripe:
                        return RedirectToRoute("Stripe", new { action = "Payment", orderID = order.OrderID });
                    default:
                        return View(model);
                }
            }
            ModelState.AddModelError("", "Some of the values are incorrect.");
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ThankYou(int? orderID)
        {
            ViewBag.OrderID = orderID.ToString();
            var order = ECommerceService.GetOrderByID(orderID.GetValueOrDefault(0));
            if (order != null)
            {
                MailingService.SendOrderConfirmation(order);
                MailingService.SendOrderConfirmationAdmin(order, this.Domain != null ? this.Domain.DO_Email : String.Empty);
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult PaymentError(string orderID, string errorMessage)
        {
            this.ViewBag.OrderID = orderID;
            this.ViewBag.ErrorMessage = errorMessage;
            return View();
        }

        #endregion


        #region Testimonials

        [AllowAnonymous]
        public ActionResult Testimonials()
        {
            List<tbl_Testimonials> testimonials = WebPagesService.GetAllTestimonialsLive();
            this.ViewBag.TestimonialsPage = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Testimonials, this.DomainID).TrimEnd('/'), this.DomainID);
            return View("Testimonials", testimonials);
        }

        [AllowAnonymous]
        public ActionResult TestimonialsPartial()
        {
            return PartialView("Testimonials", WebPagesService.GetAllTestimonialsLive());
        }

        #endregion


        #region Custom / Contact Form

        [AllowAnonymous]
        public ActionResult ContactUsPartial()
        {
            this.ViewBag.ContactUsUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.ContactUs, this.DomainID).Trim('/');
            ViewBag.CompanyName = this.Domain != null ? this.Domain.DO_CompanyName : string.Empty;
            ViewBag.Telephone = this.Domain != null ? this.Domain.DO_CompanyTelephone : string.Empty;
            ViewBag.Address = this.Domain != null ? this.Domain.DO_CompanyAddress : string.Empty;
            ViewBag.Email = this.Domain != null ? this.Domain.DO_Email : string.Empty;
            return PartialView("ContactUs");
        }

        [AllowAnonymous]
        public ActionResult SaveFormSubmission(FormCollection values)
        {
            string submittedFormID = values["FormID"];
            bool redirect = false;
            string url = "";

            tbl_Form form = new tbl_Form();

            int FormID;
            if (Int32.TryParse(submittedFormID, out FormID))
            {
                form = WebPagesService.GetFormByID(FormID);
            }

            if (form.F_Captcha)
            {
                RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper(DomainService.GetSettingsValue(SettingsKey.recaptcha_private_key));
                if (String.IsNullOrEmpty(recaptchaHelper.Response))
                {
                    return Json(new { error = "Captcha answer cannot be empty.", formID = submittedFormID }, "text/html");
                }

                RecaptchaVerificationResult recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();
                if (recaptchaResult != RecaptchaVerificationResult.Success)
                {
                    return Json(new { error = "Incorrect captcha answer.", formID = submittedFormID }, "text/html");
                }
            }
            if (ModelState.IsValid)
            {
                string recipients = this.Domain != null ? this.Domain.DO_Email : String.Empty;
                if (!string.IsNullOrEmpty(values["RecipientEmail"]) && values["RecipientEmail"].Contains("@"))
                {
                    recipients = values["RecipientEmail"];
                }
                if (!string.IsNullOrEmpty(values["subscribe"]) && this.Domain.IsAnyCRMEnabled)
                {
                    var customerEmail = values.AllKeys.FirstOrDefault(m => m != "RecipientEmail" && (m.Contains("email") || m.Contains("Email")));
                    if (!string.IsNullOrEmpty(customerEmail))
                        UserService.SubscribeNewsletter(values[customerEmail], true, this.DomainID);
                    else
                    {
                        var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
                        if (customer != null)
                            UserService.SubscribeNewsletter(customer.CU_Email, true, this.DomainID);
                    }
                }
                MailingService.SendCustomForm(WebPagesService.SaveFormSubmission(values, recipients, DateTime.UtcNow,
                    Convert.ToInt32(submittedFormID)), recipients);

                if (form.tbl_SiteMap != null)
                {
                    //SiteMapType type = (SiteMapType)form.tbl_SiteMap.SM_TypeID;
                    url = String.Format("/{0}", form.tbl_SiteMap.SM_URL.Trim('/'));
                    redirect = true;
                }

                return Json(new { success = true, formID = submittedFormID, redirect = redirect, url = url }, "text/html");
            }
            return Json(new { success = false, formID = submittedFormID }, "text/html");
        }

        #endregion


        #region POI

        [AllowAnonymous]
        public ActionResult POIs()
        {
            var poisContent = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.PointsOfInterest, this.DomainID).TrimEnd('/'), this.DomainID);
            return View(ContentModelFactory(poisContent));
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetPOITags()
        {
            var groups = POIService.GetAllPOITagsGroups();
            var filterModel = new List<POIWebFilterModel>();
            for (int i = 0; i < groups.Count; i += 2)
            {
                var model = new POIWebFilterModel();
                model.IsColumn1 = true;
                model.Column1 = new POIWebTagGroupModel
                {
                    Label = groups[i].POITG_Title,
                    Tags = new SelectList(groups[i].tbl_POITags, "POITagID", "POIT_Title")
                };
                if ((i + 1) < groups.Count)
                {
                    model.IsColumn2 = true;
                    model.Column2 = new POIWebTagGroupModel
                    {
                        Label = groups[i + 1].POITG_Title,
                        Tags = new SelectList(groups[i + 1].tbl_POITags, "POITagID", "POIT_Title")
                    };
                }
                filterModel.Add(model);
            }

            var categories = POIService.GetAllPOICategoriesLiveAsSelectList();

            return Json(new { success = true, tagGroups = filterModel, categories = categories });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetPOI(int poiID)
        {
            var poi = POIService.GetPOIByID(poiID);
            return PartialView("POIInfoWindow", poi);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult POIsSearch(string search, int? categoryID, int[] tagIDs)
        {
            var tags = POIService.SearchPOIs(search, categoryID, tagIDs)
                .Select(p => new POIWebModel
                {
                    POIID = p.POIID,
                    Latitude = p.POI_Latitude,
                    Longitude = p.POI_Longitude,
                    Title = p.POI_Title,
                    CategoryTitle = p.tbl_POICategories.POIC_Title,
                    Content = p.POI_Content,
                    Description = p.POI_Description,
                    Phone = p.POI_Phone
                });

            return Json(new { success = true, tags = tags });
        }

        #endregion

        #region Gallery

        [AllowAnonymous]
        public ActionResult Gallery()
        {
            var gallery = GalleryService.GetAllPublic();

            string sitemapUrl = WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID);
            if (sitemapUrl != null) { sitemapUrl = sitemapUrl.TrimEnd('/'); }
            tbl_Content content = null;
            content = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).TrimEnd('/'), this.DomainID);
            this.ViewBag.GalleryUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).Trim('/');
            //content = WebContentService.GetContentBySitemapUrl(sitemapUrl, this.DomainID);
            //this.ViewBag.GalleryUrl = this.Domain.DO_CustomRouteHandler ? "/" : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).Trim('/'));

            if (content == null)
                throw new HttpException(404, "Page Not Found");


            this.ViewBag.GalleryPage = content;
            this.ViewBag.Content = ContentModelFactory(content);
            this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);
            return View(gallery);
        }

        [AllowAnonymous]
        public ActionResult GalleryItem(string query)
        {
            var url = String.Format("/{0}", query).TrimEnd('/');
            tbl_Content content = WebContentService.GetContentBySitemapUrl(url, ContentType.Gallery, this.DomainID);
            if (content != null)
            {
                if (content.tbl_SiteMap.tbl_Gallery.G_CustomerID > 0) //customer galleries are private
                {
                    throw new HttpException(404, "Page Not Found");
                }

                this.ViewBag.GalleryUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).Trim('/');
                //this.ViewBag.GalleryUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).Trim('/'));
                this.ViewBag.DynamicLayout = GetLayout(content, WebPagesService);

                return View(content);
            }
            throw new HttpException(404, "Page Not Found");
        }

        [AllowAnonymous]
        public ActionResult GallerySummary()
        {
            var gallery = GalleryService.GetAllPublic();

            tbl_Content content = null;
            content = WebContentService.GetContentBySitemapUrl(WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).TrimEnd('/'), this.DomainID);
            this.ViewBag.GalleryUrl = "/" + WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).Trim('/');
            //this.ViewBag.GalleryUrl = this.Domain.DO_CustomRouteHandler ? string.Empty : ("/" + WebContentService.GetSitemapUrlByType(SiteMapType.Gallery, this.DomainID).Trim('/'));
            return PartialView("GallerySummary", gallery);
        }

        #endregion

        #region Private Methods

        private bool IsActual(tbl_ProductPrice pp)
        {
            return pp.PR_EventEndDate.HasValue ? pp.PR_EventEndDate.Value > DateTime.Now : pp.PR_EventStartDate.GetValueOrDefault() > DateTime.Now;
        }

        private ContentModel ContentModelFactory(tbl_Content content)
        {
            return new ContentModel()
            {
                Content = content,
                CSS = content.tbl_SiteMap.SM_CSS,
                Html = content.C_Content,
                HtmlElements = String.IsNullOrEmpty(content.C_Content) ? new List<object>() :
                    ProcessHtmlContent(content.C_Content,
                        this.Domain != null ? this.Domain.DO_CompanyName : string.Empty,
                        DomainService.GetSettingsValue(SettingsKey.recaptcha_public_key))
            };
        }

        private static List<LinkListItem> CreateElements(List<WebsiteMenuModel> pages)
        {
            List<LinkListItem> list = new List<LinkListItem>();
            foreach (var page in pages)
            {
                list.Add(new LinkListItem { title = page.Title, value = page.Url });

                if (page.SubMenuItems.Count > 0)
                    list.AddRange(CreateElements(page.SubMenuItems));
            }

            return list;
        }

        private List<WebsiteMenuModel> FindBreadcrumbByUrl(string url, List<WebsiteMenuModel> list)
        {
            List<WebsiteMenuModel> breadcrumbs = new List<WebsiteMenuModel>();
            foreach (WebsiteMenuModel m in list)
            {
                if (m.Url == url)
                {
                    breadcrumbs.Add(m);
                    return breadcrumbs;
                }
                else if (m.SubMenuItems != null && m.SubMenuItems.Count > 0)
                {
                    List<WebsiteMenuModel> sublist = FindBreadcrumbByUrl(url, m.SubMenuItems);
                    if (sublist.Count > 0)
                    {
                        breadcrumbs.Add(m);
                        breadcrumbs.AddRange(sublist);
                    }
                }
            }

            return breadcrumbs;
        }

        private DateTime ParseDate(string year, string month)
        {
            int y = 0, m = 0;
            int.TryParse(year, out y);
            int.TryParse(month, out m);
            return new DateTime(y, m, 1);
        }

        public List<object> ProcessHtmlContent(string content, string companyName, string publicKey)
        {
            List<object> htmlElems = new List<object>();
            string html = string.Empty;
            string tagCustomForm = string.Empty;
            const string TAG_BEGIN_CUSTOMFORM = "[%customform";
            const string TAG_END_CUSTOMFORM = "/%]";
            int beginIdx = -1;
            int endIdx = -1;
            int cursor = 0;
            if (content == null)
                return null;
            do
            {
                if (content.Length >= beginIdx + TAG_BEGIN_CUSTOMFORM.Length)
                {
                    beginIdx = content.IndexOf(TAG_BEGIN_CUSTOMFORM, cursor);
                    endIdx = content.IndexOf(TAG_END_CUSTOMFORM, beginIdx + TAG_BEGIN_CUSTOMFORM.Length);
                }

                if (beginIdx > -1 && endIdx > -1 && endIdx > beginIdx)
                {
                    if (cursor < beginIdx)
                    {
                        htmlElems.Add(content.Substring(cursor, beginIdx - cursor));
                    }

                    tagCustomForm = content.Substring(beginIdx, endIdx - beginIdx + TAG_END_CUSTOMFORM.Length);
                    FormModel formModel = GetFormModel(tagCustomForm, companyName, publicKey);
                    if (formModel != null)
                    {
                        htmlElems.Add(formModel);
                    }
                    beginIdx = cursor = endIdx + TAG_END_CUSTOMFORM.Length;
                }
                else
                {
                    htmlElems.Add(content.Substring(cursor, content.Length - cursor));
                    beginIdx = endIdx = -1;
                }
            }
            while (beginIdx > -1 && endIdx > -1);

            return htmlElems;
        }

        private tbl_Content ParseProductQuery(string query, out int priceID)
        {
            int index = query.LastIndexOf('/');
            if (index < 1)
                throw new HttpException(404, "Page Not Found");
            string rawPriceID = query.Substring(index + 1);
            string url = String.Format("/{0}", query.Substring(0, index).TrimEnd('/'));
            tbl_Content content = WebContentService.GetContentBySitemapUrl(url, this.DomainID);
            if (content != null &&
                 content.tbl_SiteMap.IsType(ContentType.Product) && Int32.TryParse(rawPriceID, out priceID))
                return content;
            throw new HttpException(404, "Page Not Found");
        }

        private FormModel GetFormModel(string form, string companyName, string publicKey)
        {
            string formIDValue = GetAttributeValue(form, "data-formID");
            string formRecipientValue = GetAttributeValue(form, "data-recipient");

            int customFormID = 0;
            if (Int32.TryParse(formIDValue, out customFormID))
            {
                tbl_Form customform = WebPagesService.GetFormByID(customFormID);
                List<tbl_FormItem> customformitems = WebPagesService.GetAllFormItemsLiveByFormID(customFormID);
                if (customform != null && customformitems != null)
                {
                    return new FormModel()
                    {
                        FormID = customFormID,
                        DomainID = customform.F_DomainID,
                        Name = customform.F_Name,
                        Description = customform.F_Description,
                        FormItems = customformitems,
                        RecipientEmail = formRecipientValue,
                        CompanyName = companyName,
                        PublicKey = publicKey,
                        Captcha = customform.F_Captcha
                    };
                }
            }

            return null;
        }

        private string GetAttributeValue(string tag, string attributeName)
        {
            int beginIdx = tag.IndexOf(attributeName, 0);
            beginIdx = tag.IndexOf("\"", beginIdx);
            int endIdx = tag.IndexOf("\"", beginIdx + 1);

            return tag.Substring(beginIdx + 1, endIdx - beginIdx - 1).Trim();
        }

        #endregion
    }
}
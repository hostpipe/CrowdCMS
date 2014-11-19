using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CMS.Services;
using CMS.BL.Entity;
using CMS.UI.Common;
using CMS.UI.Models;
using CMS.BL;

namespace CMS.UI.Controllers
{
    public class BaseWebController : BaseController
    {
        private readonly IDomain DomainService;
        private readonly IUser UserService;
        private readonly IECommerce ECommerceService;
        private readonly IWebContent WebContentService;
        protected bool DevelopmentMode = false;

        public BaseWebController(IDomain domainService,
            IECommerce ecommerceService,
            IUser userService,
            IWebContent webContentService)
            : base(domainService)
        {
            this.DomainService = domainService;
            this.ECommerceService = ecommerceService;
            this.UserService = userService;
            this.WebContentService = webContentService;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (this.Domain != null)
            {
                this.ViewBag.Theme = String.IsNullOrEmpty(this.Domain.DO_Theme) ? SettingsManager.DefaultWebTheme : this.Domain.DO_Theme;
                this.ViewBag.SubscriptionEnabled = this.Domain.IsAnyCRMEnabled;
                this.ViewBag.IsGoogleAnalytics = this.Domain.DO_GoogleAnalyticsVisible;
                if (this.Domain.DO_GoogleAnalyticsVisible)
                {
                    this.ViewBag.GoogleAnalytics = this.Domain.DO_GoogleAnalyticsCode;
                }
                this.ViewBag.HomepageID = this.Domain.DO_HomePageID;
                this.ViewBag.NoIndex = false;
                this.DevelopmentMode = Request.Url != null &&
                                        !Request.Url.ToString().ToLowerInvariant().Contains("dev.") &&
                                        !Request.Url.ToString().ToLowerInvariant().Contains("charserver.co.uk");
                if (this.DevelopmentMode || this.Domain.DO_DevelopmentMode)
                {
                    this.ViewBag.NoIndex = true;
                }
                this.ViewBag.LaunchYear = this.Domain.DO_LaunchYear ?? DateTime.Now.Year;

                this.ViewBag.IsCookieConsentEnabled = this.Domain.DO_IsCookieConsentEnabled;           
                if (this.Domain.DO_IsCookieConsentEnabled)
                {
                    // This property is now set in tbl_Domains.cs, and should hold the value
                    // fetched from SettingsValue (a string, "true" or "false".) Please
                    // see the history for the lines replaced by this change -- the code was
                    // modified for optimisation purposes (our investigations showed that
                    // this block is getting hit for-each HTML partial that is loaded, which
                    // in turn incurred x2 DB queries to find out the value for the cookie consent.
                    //this.ViewBag.CookieConsentAllSites = this.Domain.CookieConsentAllSites;
                    //this.ViewBag.CookieConsentSinglePage = this.Domain.CookieConsentSinglePage;

                    if (ControllerContext.IsChildAction)
                    {
                        this.ViewBag.CookieConsentAllSite =
                            this.ControllerContext.ParentActionViewContext.ViewBag.CookieConsentAllSite;
                        this.ViewBag.CookieConsentSinglePage = this.ControllerContext.ParentActionViewContext.ViewBag.CookieConsentSinglePage;
                    }
                    else
                    {
                        this.ViewBag.CookieConsentAllSites = this.Domain.tbl_SettingsValue.FirstOrDefault(x => x.SV_DomainID == this.Domain.DomainID
                                                             && x.SV_SettingsID == this.DomainService.GetSettingsIdByKey(SettingsKey.cookieConsentAllSites)).SV_Value;
                        this.ViewBag.CookieConsentSinglePage = this.Domain.tbl_SettingsValue.FirstOrDefault(x => x.SV_DomainID == this.Domain.DomainID
                                                               && x.SV_SettingsID == this.DomainService.GetSettingsIdByKey(SettingsKey.cookieConsentSinglePage)).SV_Value;
                    }

                }
            }
            else
            {
                this.ViewBag.SubscriptionEnabled = false;
                this.ViewBag.IsGoogleAnalytics = false;
                this.ViewBag.HomepageID = 0;
                this.DevelopmentMode = true;
                this.ViewBag.NoIndex = true;
                this.ViewBag.LaunchYear = DateTime.Now.Year;
            }
            this.ViewBag.IsHomePage = false;
            this.ViewBag.PageID = 0;
            this.ViewBag.Tracking = String.Empty;
            this.ViewBag.GetFullURL = new Func<int, string>(GetFullURL);
        }

        protected tbl_Basket FindBasket()
        {
            tbl_Basket basket = null;

            if (CookieManager.BasketCookie.HasValue)
            {
                basket = ECommerceService.GetBasketByID(CookieManager.BasketCookie.Value);
                if (basket == null)
                    return null;
                int customerID = basket.B_CustomerID ?? 0;
                if (AdminUser != null && !AdminUser.IsAdmn && customerID != AdminUser.UserID)
                {
                    basket = ECommerceService.UpdateBasketCustomerID(AdminUser.UserID, basket.BasketID);
                }
                else if ((AdminUser == null && customerID > 0) || (AdminUser != null && AdminUser.IsAdmn && customerID > 0))
                {
                    basket = ECommerceService.UpdateBasketCustomerID(0, basket.BasketID);
                }
            } 
            else if (Request.IsAuthenticated && !AdminUser.IsAdmn)
            {
                var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
                if (customer != null)
                {
                    basket = customer.tbl_Basket.OrderByDescending(c => c.B_LastAccessed).FirstOrDefault();
                    if (basket != null)
                    {
                        CookieManager.BasketCookie = basket.BasketID;
                    }
                }
            }

            return basket;
        }

        protected tbl_Orders FindOrder()
        {
            tbl_Orders order = null;
            if (Request.IsAuthenticated && !AdminUser.IsAdmn)
            {
                var customer = UserService.GetCustomerByID(this.AdminUser.UserID);
                if (customer != null)
                {
                    order = customer.tbl_Orders.OrderByDescending(c => c.OrderID).FirstOrDefault();
                }
            }

            return order;
        }

        protected List<WebsiteMenuModel> GetCategoryMenuOrdered(int parentID, bool includeAllSubMenus = false, ProductType type = ProductType.Item)
        {
            var menu = new List<WebsiteMenuModel>();
            List<tbl_ProdCategories> allCats = ECommerceService.GetProdCategoriesForDomain(this.DomainID, parentID, false, type);
            foreach (var item in allCats)
            {
                var content = item.tbl_SiteMap.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault();
                string url = "";
                if (type == ProductType.Item)
                    url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/'), item.tbl_SiteMap.SM_URL);
                else if (type == ProductType.Event)
                    url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/'), item.tbl_SiteMap.SM_URL);
                if (content != null)
                {
                    menu.Add(new WebsiteMenuModel
                    {
                        Name = content.C_MenuText,
                        Url = url,
                        Title = content.C_Title,
                        ModificationDate = content.C_ModificationDate,
                        Priority = item.tbl_SiteMap.SM_Priority.GetValueOrDefault((decimal)0.5),
                        SubMenuItems = GetCategoryMenuOrdered(item.CategoryID, includeAllSubMenus, type)
                    });
                }
            }
            return menu;
        }

        protected List<WebsiteMenuModel> GetSubMenuOrdered(bool isMenu, bool isFooter, bool isSitemap, int parentID, bool includeArticles = false, bool includeCategory = false, bool includeProducts = false)
        {
            List<tbl_SiteMap> sitemaps = WebContentService.GetSitemapByContentType(ContentType.Content, this.DomainID)
                .Where(sm => (isMenu && sm.SM_Menu) || (isFooter && sm.SM_Footer) || (isSitemap && sm.SM_Sitemap)).ToList();

            if (includeArticles)
                sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Blog, this.DomainID).Where(b => b.SM_Live).OrderByDescending(b => b.SM_Date));
            if (includeCategory)
                sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Category, this.DomainID).Where(c => c.tbl_ProdCategories.PC_Live && (!isMenu || c.SM_Menu)));
            if (includeProducts)
                sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Product, this.DomainID).Where(p => p.tbl_Products.P_Live));

            return CreateChildMenuOrdered(sitemaps, 0);
        }

        protected List<WebsiteMenuModel> GetMenuOrdered(bool isMenu, bool isFooter, bool isSitemap, bool includeArticles = false, bool includeCategory = false, bool includeProducts = false)
        {
            List<tbl_SiteMap> sitemaps = WebContentService.GetSitemapByContentType(ContentType.Content, this.DomainID)
                .Where(sm => (isMenu && sm.SM_Menu) || (isFooter && sm.SM_Footer) || (isSitemap && sm.SM_Sitemap)).ToList();

            if (includeArticles)
                sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Blog, this.DomainID).Where(b => b.SM_Live).OrderByDescending(b => b.SM_Date));
            if (includeCategory)
                sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Category, this.DomainID).Where(c => c.tbl_ProdCategories.PC_Live && (!isMenu || c.SM_Menu)));
            if (includeProducts)
                sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Product, this.DomainID).Where(p => p.tbl_Products.P_Live));

            return CreateChildMenuOrdered(sitemaps, 0);
        }

        protected List<WebsiteMenuModel> GetMenuAllOrdered()
        {
            List<tbl_SiteMap> sitemaps = WebContentService.GetSitemapByContentType(ContentType.Content, this.DomainID).ToList();
            sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Blog, this.DomainID).Where(b => b.SM_Live).OrderByDescending(b => b.SM_Date));
            sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Category, this.DomainID).Where(c => c.tbl_ProdCategories.PC_Live));
            sitemaps.AddRange(WebContentService.GetSitemapByContentType(ContentType.Product, this.DomainID).Where(p => p.tbl_Products.P_Live));

            return CreateChildMenuOrdered(sitemaps, 0);
        }

        protected List<WebsiteMenuModel> CreateChildMenuOrdered(List<tbl_SiteMap> items, int parentID)
        {
            var menu = new List<WebsiteMenuModel>();
            var itemsFiltered = items
                .Where(sm => parentID == 0 ? (sm.SM_ParentID == 0 || sm.IsDirectlyInMenu) : (sm.SM_ParentID == parentID && sm.IsUnderParentInMenu))
                .OrderBy(sm => sm.SM_OrderBy).ToList();

            foreach (var item in itemsFiltered)
            {
                var content = item.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault();
                string url = "";
                if (item.SiteMapID == this.Domain.DO_HomePageID)
                {
                    url = "/";
                }
                else if (item.IsType(ContentType.Blog))
                {
                    url = String.Format("/{0}{1}", WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/'), item.SM_URL);
                    //url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.News, this.DomainID).Trim('/'), item.SM_URL);
                }
                else if (item.IsType(ContentType.Category))
                {
                    if (item.tbl_ProdCategories.PC_ProductTypeID == (int)ProductType.Item)
                        url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/'), item.SM_URL);
                    else if (item.tbl_ProdCategories.PC_ProductTypeID == (int)ProductType.Event)
                        url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/'), item.SM_URL);
                }
                else if (item.IsType(ContentType.Product))
                {
                    if (item.tbl_Products.P_ProductTypeID == (int)ProductType.Item)
                        url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.ProductShop, this.DomainID).Trim('/'), item.SM_URL);
                    else if (item.tbl_Products.P_ProductTypeID == (int)ProductType.Event)
                        url = String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/'), item.SM_URL);
                }
                else
                {
                    url = item.SM_URL;
                }

                if (content != null)
                {
                    menu.Add(new WebsiteMenuModel
                    {
                        Name = content.C_MenuText,
                        Url = url,
                        Title = content.C_Title,
                        ModificationDate = content.C_ModificationDate,
                        Priority = item.SM_Priority.GetValueOrDefault((decimal)0.5),
                        SubMenuItems = CreateChildMenuOrdered(items, item.SiteMapID)
                    });
                }
            }
            return menu;
        }

        protected string GetFullURL(int sitemapID)
        {
            tbl_SiteMap sitemap = WebContentService.GetSitemapByID(sitemapID);
            if (sitemap == null)
                return "";

            string baseUrl = "";

            if (sitemap.SM_ParentID == 0 || this.Domain.DO_CustomRouteHandler)
                return sitemap.SM_URL;

            var parent = WebContentService.GetSitemapByID(sitemap.SM_ParentID);
            while (parent.SM_ParentID > 0)
            {
                parent = WebContentService.GetSitemapByID(parent.SM_ParentID);
            }
            if (parent.SM_TypeID != null)
                baseUrl = "/" + WebContentService.GetSitemapUrlByType((SiteMapType)parent.SM_TypeID, this.DomainID).Trim('/');

            return String.Format("{0}{1}", baseUrl, sitemap.SM_URL);
        }

        protected string GetURL(SiteMapType type, string relativeUrl)
        {
            return String.Format("/{0}{1}", this.Domain.DO_CustomRouteHandler ? string.Empty : WebContentService.GetSitemapUrlByType(SiteMapType.EventShop, this.DomainID).Trim('/'), relativeUrl);
        }
    }
}

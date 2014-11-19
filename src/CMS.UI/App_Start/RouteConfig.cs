using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;
using CMS.Services;
using CMS.BL;
using CMS.UI.Common;
using CMS.BL.Entity;
using System.Linq;

namespace CMS.UI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.Clear();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Account",
                url: "account",
                defaults: new { controller = "Website", action = "DisplayCustomerAccount" }
            );

            routes.MapRoute(
                name: "AddAddress",
                url: "account/addresses/{id}",
                defaults: new { controller = "Website", action = "Addresses", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DeleteAddress",
                url: "account/deleteaddress/{addressID}",
                defaults: new { controller = "Website", action = "DeleteAddress", addressID = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DonateCheckout",
                url: "donate/{type}/{amount}",
                defaults: new { controller = "Website", action = "DonationCheckout" }
            );

            routes.MapRoute(
                name: "GetAddressList",
                url: "shop/checkout/getaddresslist",
                defaults: new { controller = "Website", action = "GetAddressList" }
            );

            routes.MapRoute(
                name: "GetSelectedAddress",
                url: "shop/checkout/getselectedaddress",
                defaults: new { controller = "Website", action = "GetSelectedAddress" }
            );

            routes.MapRoute(
                name: "EditMyDetails",
                url: "account/editmydetails",
                defaults: new { controller = "Website", action = "EditCustomerAccount" }
            );

            routes.MapRoute(
                name: "OrderHistory",
                url: "account/orderhistory",
                defaults: new { controller = "WebSite", action = "OrderHistory" }
            );

            routes.MapRoute(
                name: "Admn",
                url: "admn/{action}/{id}",
                defaults: new { controller = "Admn", action = "Home", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SagePay",
                url: "sagepay/{action}/{orderID}",
                defaults: new { controller = "SagePay", action = "Home", orderID = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PayPal",
                url: "paypal/{action}/{orderID}",
                defaults: new { controller = "PayPal", action = "Home", orderID = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SecureTrading",
                url: "securetrading/{action}/{orderID}",
                defaults: new { controller = "SecureTrading", action = "Home", orderID = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Stripe",
                url: "stripe/{action}/{orderID}",
                defaults: new { controller = "Stripe", action = "Home", orderID = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Preview",
                url: "preview",
                defaults: new { controller = "Website", action = "Content" }
            );

            routes.MapRoute(
                name: "Website",
                url: "website/{action}/{id}",
                defaults: new { controller = "Website", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Search",
                url: "search/{keyword}",
                defaults: new { controller = "Website", action = "Search", keyword = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "Website", action = "Login" }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "Website", action = "Logout" }
            );

            routes.MapRoute(
                name: "Registration",
                url: "registration",
                defaults: new { controller = "Website", action = "Registration" }
            );

            routes.MapRoute(
                name: "PassReminder",
                url: "pass-reminder",
                defaults: new { controller = "Website", action = "ForgottenPassword" }
            );

            routes.MapRoute(
                name: "Basket",
                url: "basket",
                defaults: new { controller = "Website", action = "Basket" }
            );

            routes.MapRoute(
                name: "Checkout",
                url: "checkout",
                defaults: new { controller = "Website", action = "Checkout" }
            );

            routes.MapRoute(
                name: "OrderSummary",
                url: "summaryandpayment",
                defaults: new { controller = "Website", action = "OrderSummary" }
            );

            routes.MapRoute(
                name: "XMLSitemap",
                url: "sitemap.xml",
                defaults: new { controller = "Website", action = "XMLSitemap" }
            );

            routes.MapRoute(
                name: "RegisterConfirmation",
                url: "register-confirmation",
                defaults: new { controller = "Website", action = "RegisterConfirmation" }
            );

            routes.MapRoute(
                name: "RobotsTXT",
                url: "robots.txt",
                defaults: new { controller = "Website", action = "RobotsTXT" }
            );

            var contentService = (IWebContent)DependencyResolver.Current.GetService(typeof(IWebContent));
            var domainService = (IDomain)DependencyResolver.Current.GetService(typeof(IDomain));

            List<tbl_Domains> domains = domainService.GetAllDomains();
#if DEBUG
            var localhostDomain = domainService.GetDomainByID(SettingsManager.LocalHostDomainID);
            if (localhostDomain == null)
            {
                localhostDomain = new tbl_Domains { DomainID = SettingsManager.LocalHostDomainID };
                domains.Add(localhostDomain);
            }
            string processName = Process.GetCurrentProcess().ProcessName.ToLower();
            bool isRunningInIisExpress = processName.Contains("iisexpress") || processName.Contains("webdev.webserver");
            if (isRunningInIisExpress)
                localhostDomain.DO_Domain = "localhost";
#endif
            foreach (var domain in domains)
            {
                
                string newsPath = contentService.GetSitemapUrlByType(SiteMapType.News, domain.DomainID),
                    testimonialsPath = contentService.GetSitemapUrlByType(SiteMapType.Testimonials, domain.DomainID),
                    prodCategoriesPath = contentService.GetSitemapUrlByType(SiteMapType.ProductShop, domain.DomainID),
                    eventCategoriesPath = contentService.GetSitemapUrlByType(SiteMapType.EventShop, domain.DomainID),
                    sitemapPath = contentService.GetSitemapUrlByType(SiteMapType.Sitemap, domain.DomainID),
                    subscribePath = contentService.GetSitemapUrlByType(SiteMapType.Subscribe, domain.DomainID),
                    donationPath = contentService.GetSitemapUrlByType(SiteMapType.Donation, domain.DomainID),
                    poiPath = contentService.GetSitemapUrlByType(SiteMapType.PointsOfInterest, domain.DomainID),
                    portfolioPath = contentService.GetSitemapUrlByType(SiteMapType.Portfolio, domain.DomainID),
                    galleryPath = contentService.GetSitemapUrlByType(SiteMapType.Gallery, domain.DomainID);

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: donationPath.Trim('/'),
                    defaults: new { controller = "Website", action = "DonationCategories" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: subscribePath.Trim('/'),
                    defaults: new { controller = "Website", action = "Subscribe", email = UrlParameter.Optional }
                ));
                if (!domain.DO_CustomRouteHandler)
                {
                    routes.Add(new DomainRoute(
                        domain: domain.DO_Domain,
                        url: eventCategoriesPath.Trim('/'),
                        defaults: new {controller = "Website", action = "EventsCategories"}
                        ));

                    routes.Add(new DomainRoute(
                        domain: domain.DO_Domain,
                        url: prodCategoriesPath.Trim('/'),
                        defaults: new {controller = "Website", action = "ProdCategories"}
                        ));
                }
                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: portfolioPath.Trim('/'),
                    defaults: new { controller = "Website", action = "Portfolio" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}", portfolioPath.Trim('/'), "{*query}"),
                    defaults: new { controller = "Website", action = "PortfolioItem" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: galleryPath.Trim('/'),
                    defaults: new { controller = "Website", action = "Gallery" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}", galleryPath.Trim('/'), "{*query}"),
                    defaults: new { controller = "Website", action = "GalleryItem" }
                ));
                if (!domain.DO_CustomRouteHandler)
                {
                    routes.Add(new DomainRoute(
                        domain: domain.DO_Domain,
                        url: String.Format("{0}/{1}", eventCategoriesPath.Trim('/'), "{*query}"),
                        defaults: new {controller = "Website", action = "Events"}
                    ));

                    routes.Add(new DomainRoute(
                        domain: domain.DO_Domain,
                        url: String.Format("{0}/{1}", prodCategoriesPath.Trim('/'), "{*query}"),
                        defaults: new {controller = "Website", action = "Products"}
                    ));
                }
                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: testimonialsPath.Trim('/'),
                    defaults: new { controller = "Website", action = "Testimonials" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: sitemapPath.Trim('/'),
                    defaults: new { controller = "Website", action = "Sitemap" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}/{2}", newsPath.Trim('/'), SettingsManager.Blog.SearchUrl.Trim('/'), "{keyword}"),
                    defaults: new { controller = "Website", action = "BlogSearch", keyword = UrlParameter.Optional }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}", newsPath.Trim('/'), "feed"),
                    defaults: new { controller = "Website", action = "GetBlogRss" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: newsPath.Trim('/'),
                    defaults: new { controller = "Website", action = "Blog" }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}/{2}", newsPath.Trim('/'), SettingsManager.Blog.CategoryUrl.Trim('/'), "{name}"),
                    defaults: new { controller = "Website", action = "BlogCategory", name = UrlParameter.Optional }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}/{2}", newsPath.Trim('/'), SettingsManager.Blog.TagUrl.Trim('/'), "{name}"),
                    defaults: new { controller = "Website", action = "BlogTag", name = UrlParameter.Optional }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: String.Format("{0}/{1}/{2}/{3}", newsPath.Trim('/'), "{year}", "{month}", "{title}"),
                    defaults: new { controller = "Website", action = "Blog", year = UrlParameter.Optional, month = UrlParameter.Optional, title = UrlParameter.Optional }
                ));

                routes.Add(new DomainRoute(
                    domain: domain.DO_Domain,
                    url: poiPath.Trim('/'),
                    defaults: new { controller = "Website", action = "POIs" }
                ));
            }


            Route customRoute = new Route("{*values}", new CMS.UI.Common.CustomRouteHandler());
            routes.Add("customRouter", customRoute);


            //routes.MapRoute(
            //    name: "Default",
            //    url: "{*values}",
            //    defaults: new { controller = "Website", action = "Content" }
            //);

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
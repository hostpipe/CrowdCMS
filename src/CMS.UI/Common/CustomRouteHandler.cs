using CMS.BL;
using CMS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CMS.UI.Common
{
    public class CustomRouteHandler : IRouteHandler
    {

        public CustomRouteHandler()
        {

        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new CustomHandler(requestContext);
        }
    }

    public class CustomHandler : IHttpHandler
    {
        //IRouter routerService(IRouter) DependencyResolver.Current.GetService(typeof(IRouter));
        IWebContent contentService = (IWebContent)DependencyResolver.Current.GetService(typeof(IWebContent));
        IDomain domainService = (IDomain)DependencyResolver.Current.GetService(typeof(IDomain));

        public RequestContext RequestContext { get; private set; }

        public CustomHandler(RequestContext requestContext)
        {
            RequestContext = requestContext;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var routeValues = RequestContext.RouteData.Values;

            string domainString = Utils.Domain.GetDomain(RequestContext.HttpContext);
            string urlString = Utils.Domain.GetPath(RequestContext.HttpContext);
                // = context.Request.Url.LocalPath;

            CMS.BL.Entity.tbl_Domains domain = domainService.GetDomainByName(domainString);

#if DEBUG
            if (domain == null && "localhost".Equals(domainString) && RequestContext.HttpContext.Request.IsLocal)
            {
                domain = domainService.GetDomainByID(SettingsManager.LocalHostDomainID);
                domain.DO_Domain = "localhost";
            }
#endif

            if (domain == null)
                throw new Exception("Custom route handler exception: Can not find domain for url " + urlString);

            if (!domain.DO_CustomRouteHandler)
            {
                RequestContext.RouteData.Values["controller"] = "Website";
                RequestContext.RouteData.Values["action"] = "Content";
                //default controller/action
            }
            else
            {
                var machingSitemaps = contentService
                    .GetSitemaps((sitemap) => ("/" + urlString).StartsWith(sitemap.SM_URL) && sitemap.SM_DomainID == domain.DomainID)
                    .OrderByDescending((sitemap) => sitemap.SM_URL.Length)
                    .ToList();
                //var machingSitemaps = contentService.GetSitemapByUrl(urlString, domain.DomainID);
                if (machingSitemaps == null || machingSitemaps.Count == 0)
                {
                    RequestContext.RouteData.Values["controller"] = "Website";
                    RequestContext.RouteData.Values["action"] = "Content";
                    //default controller/action
                }
                else
                {
                    var selectedSitemap = machingSitemaps.First();

                    switch (selectedSitemap.SM_TypeID)
                    {
                        //case (int)SiteMapType.Donation:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    RequestContext.RouteData.Values["action"] = "DonationCategories";
                        //    break;
                        //case (int)SiteMapType.Subscribe:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    RequestContext.RouteData.Values["action"] = "Subscribe";
                        //    RequestContext.RouteData.Values["email"] = context.Request.QueryString["email"]; //extract it
                        //    break;
                        //case (int)SiteMapType.PointsOfInterest:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    RequestContext.RouteData.Values["action"] = "POIs";
                        //    break;
                        //case (int)SiteMapType.Sitemap:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    RequestContext.RouteData.Values["action"] = "Sitemap";
                        //    break;
                        //case (int)SiteMapType.Testimonials:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    RequestContext.RouteData.Values["action"] = "Testimonials";
                        //    break;
                        //case (int)SiteMapType.News:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    if (urlString.Contains(SettingsManager.Blog.SearchUrl.Trim('/')))
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "BlogSearch";
                        //        ExtractRouteValues(domain.DO_Domain, String.Format("{0}/{1}/{2}", "[^/]+", SettingsManager.Blog.SearchUrl.Trim('/'), "{keyword}"));
                        //        break;
                        //    }

                        //    if (urlString.EndsWith("feed"))
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "GetBlogRss";
                        //        break;
                        //    }
                        //    if (string.Compare(selectedSitemap.SM_URL.Trim('/'), urlString, true) == 0)
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "Blog";
                        //        ExtractRouteValues(domain.DO_Domain, String.Format("{0}/{1}/{2}", "{year}", "{month}", "{title}"));
                        //        break;
                        //    }

                        //    if (urlString.Contains(SettingsManager.Blog.CategoryUrl.Trim('/')))
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "BlogCategory";
                        //        ExtractRouteValues(domain.DO_Domain, String.Format("{0}/{1}/{2}", "[^/]+", SettingsManager.Blog.CategoryUrl.Trim('/'), "{name}"));
                        //        break;
                        //    }

                        //    if (urlString.Contains(SettingsManager.Blog.TagUrl.Trim('/')))
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "BlogTag";
                        //        ExtractRouteValues(domain.DO_Domain, String.Format("{0}/{1}/{2}", "[^/]+", SettingsManager.Blog.TagUrl.Trim('/'), "{name}"));
                        //        break;
                        //    }

                        //    RequestContext.RouteData.Values["action"] = "Blog";
                        //    ExtractRouteValues(domain.DO_Domain, String.Format("{0}/{1}/{2}/{3}", selectedSitemap.SM_URL.TrimEnd('/'), "{year}", "{month}", "{title}"));
                        //    break;
                        //case (int)SiteMapType.Gallery:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    if (string.Compare(selectedSitemap.SM_URL.Trim('/'), urlString, true) == 0)
                        //    {
                        //        if (selectedSitemap.SM_ContentTypeID == 1)
                        //        {
                        //            RequestContext.RouteData.Values["action"] = "Gallery";
                        //        }
                        //        else
                        //        {
                        //            RequestContext.RouteData.Values["action"] = "GalleryItem";
                        //            ExtractRouteValues(domain.DO_Domain, "/{*query}");
                        //        }
                        //    }
                        //    else
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "GalleryItem";
                        //        ExtractRouteValues(domain.DO_Domain, (selectedSitemap.SM_ContentTypeID == 1 ? selectedSitemap.SM_URL.TrimEnd('/') : "") + "/{*query}");
                        //    }
                        //    break;
                        case (int)SiteMapType.ProductShop:
                            RequestContext.RouteData.Values["controller"] = "Website";
                            if (string.Compare(selectedSitemap.SM_URL.Trim('/'), urlString, true) == 0)
                            {
                                if (selectedSitemap.SM_ContentTypeID == 1)
                                {
                                    RequestContext.RouteData.Values["action"] = "ProdCategories";
                                }
                                else
                                {
                                    RequestContext.RouteData.Values["action"] = "Products";
                                    ExtractRouteValues(domain.DO_Domain, "/{*query}");
                                }
                            }
                            else
                            {
                                RequestContext.RouteData.Values["action"] = "Products";
                                ExtractRouteValues(domain.DO_Domain, (selectedSitemap.SM_ContentTypeID == 1 ? selectedSitemap.SM_URL.TrimEnd('/') : "") + "/{*query}");
                            }
                            break;
                        case (int)SiteMapType.EventShop:
                            RequestContext.RouteData.Values["controller"] = "Website";
                            if (string.Compare(selectedSitemap.SM_URL.Trim('/'), urlString, true) == 0)
                            {
                                if (selectedSitemap.SM_ContentTypeID == 1)
                                {
                                    RequestContext.RouteData.Values["action"] = "EventsCategories";
                                }
                                else
                                {
                                    RequestContext.RouteData.Values["action"] = "Events";
                                    ExtractRouteValues(domain.DO_Domain, "/{*query}");
                                }
                            }
                            else
                            {
                                RequestContext.RouteData.Values["action"] = "Events";
                                ExtractRouteValues(domain.DO_Domain, (selectedSitemap.SM_ContentTypeID == 1 ? selectedSitemap.SM_URL.TrimEnd('/') : "") + "/{*query}");
                            }
                            break;
                        //case (int)SiteMapType.Portfolio:
                        //    RequestContext.RouteData.Values["controller"] = "Website";
                        //    if (string.Compare(selectedSitemap.SM_URL.Trim('/'), urlString, true) == 0)
                        //    {
                        //        if (selectedSitemap.SM_ContentTypeID == 1)
                        //        {
                        //            RequestContext.RouteData.Values["action"] = "Portfolio";
                        //        }
                        //        else
                        //        {
                        //            RequestContext.RouteData.Values["action"] = "PortfolioItem";
                        //            ExtractRouteValues(domain.DO_Domain, "/{*query}");
                        //        }
                        //    }
                        //    else
                        //    {
                        //        RequestContext.RouteData.Values["action"] = "PortfolioItem";
                        //        ExtractRouteValues(domain.DO_Domain, (selectedSitemap.SM_ContentTypeID == 1 ? selectedSitemap.SM_URL.TrimEnd('/') : "") + "/{*query}");
                        //    }
                        //    break;
                        default:
                            RequestContext.RouteData.Values["controller"] = "Website";
                            RequestContext.RouteData.Values["action"] = "Content";
                            break;
                    }
                }
            }
            
            string controllerId = routeValues["Controller"] as string;
            IController controller = null;
            IControllerFactory factory = null;
            try
            {
                factory = ControllerBuilder.Current.GetControllerFactory();
                controller = factory.CreateController(RequestContext, controllerId);
                if (controller != null)
                {
                    controller.Execute(RequestContext);
                }
                else
                {
                    //Exception logging
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                factory.ReleaseController(controller);
            } 
        }

        private void ExtractRouteValues(string domain, string url)
        {
            var items = Utils.Domain.GetRouteData(RequestContext.HttpContext, domain, url);
            if (items != null)
            {
                foreach (var i in items)
                {
                    RequestContext.RouteData.Values.Add(i.Key, i.Value);
                }
            }
        }
    }
}
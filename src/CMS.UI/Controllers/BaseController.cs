using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CMS.Utils;
using CMS.BL.Entity;
using CMS.Services;
using System.Globalization;
using System.Threading;
using CMS.BL;
using System.Web.Routing;


namespace CMS.UI.Controllers
{
    public class BaseController : Controller
    {
        private tbl_Domains _domain;
        private readonly IDomain DomainService;
        private const string DEFAULT_LAYOUT_LOCATION = "/Views/Shared/_Layout.cshtml";

        protected readonly CultureInfo GBCulture = new CultureInfo("en-GB");



        public BaseController(IDomain domainService)
        {
            this.DomainService = domainService;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            GBCulture.NumberFormat.CurrencySymbol = DomainService.GetSettingsValue(SettingsKey.currencySign, this.DomainID);
            Thread.CurrentThread.CurrentCulture = GBCulture;
            Thread.CurrentThread.CurrentUICulture = GBCulture;
        }

        protected virtual CustomPrincipal AdminUser
        {
            get { return HttpContext.User as CustomPrincipal; }
        }

        protected int DomainID
        {
            get
            {
#if DEBUG
                if (Request == null || Request.Url.Host.Contains("localhost") || Request.Url.Host.Contains("10.0.2.2") || Request.Url.Host.Contains("127.0.0.1")) 
                    return SettingsManager.LocalHostDomainID;
#endif

                if (this._domain != null)
                    return this._domain.DomainID;
                else
                {
                    this._domain = DomainService.GetDomainByName(Request.Url.Host);
                    if (this._domain != null && this._domain.DO_Domain != Request.Url.Host)
                        Response.RedirectPermanent(new UriBuilder(Request.Url.Scheme, this._domain.DO_Domain, Request.Url.Port, Request.Url.AbsolutePath).ToString());

                    return this._domain != null ? this._domain.DomainID : 0;
                }
            }
        }

        protected tbl_Domains Domain
        {
            get
            {
                if (Request == null)
                    return null;

                if (this._domain == null)
                {
                    this._domain = DomainService.GetDomainByID(DomainID);
                }

                return this._domain;
            }
        }


        protected void SendTweet(string message)
        {
            var domain = this.Domain;
            if (domain != null && domain.DO_UpdateTwitter && !String.IsNullOrEmpty(message))
            {
                TweetManager tweet = new TweetManager(domain.DO_TwitterToken, domain.DO_TwitterSecret, domain.DO_ConsumerKey, domain.DO_ConsumerSecret);
                tweet.SendTweet(message);
            }
        }

        protected string CheckCustomLayout(int layoutId, int contentId)
        {
            return "";
        }

        protected string GetLayout(tbl_Content content, IWebPages WebPagesService)
        {
            string layoutDir = DEFAULT_LAYOUT_LOCATION;

            if (!string.IsNullOrEmpty(ViewBag.Theme) && System.IO.File.Exists(Server.MapPath(String.Format("/Themes/{0}{1}", ViewBag.Theme, DEFAULT_LAYOUT_LOCATION))))
            {
                layoutDir = String.Format("/Themes/{0}{1}", ViewBag.Theme, DEFAULT_LAYOUT_LOCATION);
            }

            if (content.tbl_SiteMap.SM_CustomLayoutID != null)
            {
                var customLayout = WebPagesService.GetCustomLayoutById((int)content.tbl_SiteMap.SM_CustomLayoutID);

                if (!string.IsNullOrEmpty(ViewBag.Theme) && System.IO.File.Exists(Server.MapPath(String.Format("/Themes/{0}{1}", ViewBag.Theme, customLayout.CL_Directory))))
                {
                    layoutDir = String.Format("/Themes/{0}{1}", ViewBag.Theme, customLayout.CL_Directory);
                }

            }

            return layoutDir;
        }
    }
}

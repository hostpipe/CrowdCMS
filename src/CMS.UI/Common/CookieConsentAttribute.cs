using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.UI.Common
{
    /// <summary>
    /// ASP.NET MVC FilterAttribute for implementing european cookie-law
    /// </summary>
    public class CookieConsentAttribute : ActionFilterAttribute
    {
        public const string CONSENT_COOKIE_NAME = "CookieConsent";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.IsChildAction)
            {
                var viewBag = filterContext.Controller.ViewBag;

                if (viewBag.IsCookieConsentEnabled != null && viewBag.IsCookieConsentEnabled == true)
                {
                    var request = filterContext.HttpContext.Request;

                    if ((viewBag.CookieConsentAllSites != null && viewBag.CookieConsentAllSites == "true") || (!String.IsNullOrEmpty(viewBag.CookieConsentSinglePage) && request.Path == viewBag.CookieConsentSinglePage))
                    {

                        viewBag.AskCookieConsent = true;
                        viewBag.HasCookieConsent = false;

                        // Check if the user has a consent cookie
                        var consentCookie = request.Cookies[CONSENT_COOKIE_NAME];
                        if (consentCookie == null)
                        {
                            // No consent cookie. We first check the Do Not Track header value, this can have the value "0" or "1"
                            string dnt = request.Headers.Get("DNT");

                            // If we receive a DNT header, we accept its value and do not ask the user anymore
                            if (!String.IsNullOrEmpty(dnt))
                            {
                                viewBag.AskCookieConsent = false;
                                if (dnt == "0")
                                {
                                    viewBag.HasCookieConsent = true;
                                }
                            }
                            else
                            {
                                if (IsSearchCrawler(request.Headers.Get("User-Agent")))
                                {
                                    // don't ask consent from search engines, also don't set cookies
                                    viewBag.AskCookieConsent = false;
                                }
                                else
                                {
                                    // first request on the site and no DNT header. 
                                    consentCookie = new HttpCookie(CONSENT_COOKIE_NAME);
                                    consentCookie.Value = "asked";
                                    filterContext.HttpContext.Response.Cookies.Add(consentCookie);
                                }
                            }
                        }
                        else
                        {
                            if ((viewBag.CookieConsentAllSites != null && viewBag.CookieConsentAllSites == "true")
                                || (!String.IsNullOrEmpty(viewBag.CookieConsentSinglePage) && request.Path == viewBag.CookieConsentSinglePage))
                            {
                                // we received a consent cookie
                                viewBag.AskCookieConsent = false;
                                if (consentCookie.Value == "asked")
                                {
                                    // consent is implicitly given
                                    consentCookie.Value = "true";
                                    consentCookie.Expires = DateTime.UtcNow.AddYears(1);
                                    filterContext.HttpContext.Response.Cookies.Set(consentCookie);
                                    viewBag.HasCookieConsent = true;
                                }
                                else if (consentCookie.Value == "true")
                                {
                                    viewBag.HasCookieConsent = true;
                                }
                                else
                                {
                                    // assume consent denied
                                    viewBag.HasCookieConsent = false;
                                }
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(viewBag.CookieConsentSinglePage))
                    {
                        var consentCookie = request.Cookies[CONSENT_COOKIE_NAME];
                        if (request.Path != viewBag.CookieConsentSinglePage && consentCookie != null && consentCookie.Value == "asked")
                        {
                            consentCookie.Value = "asked";
                            consentCookie.Expires = DateTime.UtcNow.AddDays(-1);
                            filterContext.HttpContext.Response.Cookies.Set(consentCookie);
                            viewBag.AskCookieConsent = false;
                            viewBag.HasCookieConsent = false;
                        }
                    }
                }
            }
            base.OnActionExecuting(filterContext);

        }

        private bool IsSearchCrawler(string userAgent)
        {
            if (!String.IsNullOrEmpty(userAgent))
            {
                string[] crawlers = new string[] 
                { 
                    "Baiduspider", 
                    "Googlebot", 
                    "YandexBot", 
                    "YandexImages",
                    "bingbot", 
                    "msnbot", 
                    "Vagabondo", 
                    "SeznamBot",
                    "ia_archiver",
                    "AcoonBot",
                    "Yahoo! Slurp",
                    "AhrefsBot"
                };
                foreach (string crawler in crawlers)
                    if (userAgent.Contains(crawler))
                        return true;
            }
            return false;
        }
    }
}
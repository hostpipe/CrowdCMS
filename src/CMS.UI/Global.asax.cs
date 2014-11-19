using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using CMS.Services;
using CMS.UI.Common.Validation;
using CMS.Utils.Diagnostics;
using CMS.UI.Controllers;
using CMS.UI.Common;

namespace CMS.UI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine()); //Only need to run Razor views

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Insert(0, new TemplateViewEngine());

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            String requestedUrl = HttpContext.Current.Request.Url.AbsolutePath;
            if (requestedUrl != "/" && requestedUrl.EndsWith("/"))
                Response.RedirectPermanent(requestedUrl.TrimEnd('/'), true);
        }

        protected void Application_error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception is HttpException)
            {
                Log.Info(String.Format("404 {0}", Request.Url.AbsolutePath), exception);
            }
            else
            {
                Log.Error("Application error", exception);
            }

            if (Context.IsCustomErrorEnabled)
                ShowCustomErrorPage(exception);
        }

        private void ShowCustomErrorPage(Exception exception)
        {
            HttpException httpException = exception as HttpException;
            if (httpException == null)
                httpException = new HttpException(500, "Internal Server Error", exception);

            Response.Clear();
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");

            switch (httpException.GetHttpCode())
            {

                case 404:
                    routeData.Values.Add("action", "Http404");
                    break;

                default:
                    routeData.Values.Add("action", "Http404");
                    break;
            }

            Server.ClearError();

            var contentService = (IWebContent)DependencyResolver.Current.GetService(typeof(IWebContent));
            var domainService = (IDomain)DependencyResolver.Current.GetService(typeof(IDomain));
            var userService = (IUser)DependencyResolver.Current.GetService(typeof(IUser));
            var ecommerceService = (IECommerce)DependencyResolver.Current.GetService(typeof(IECommerce));
            //var stripeService = (IStripe) DependencyResolver.Current.GetService(typeof (IStripe));

            IController controller = new ErrorController(domainService, contentService, ecommerceService, userService);
            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }
    }
}
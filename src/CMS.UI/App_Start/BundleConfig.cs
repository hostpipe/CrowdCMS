using CMS.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace CMS.UI
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            //  ----- Scripts ------

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.form.js",
                        "~/Scripts/jquery.friendurl.js",
                        "~/Scripts/jquery.scrollTo-1.4.3.1-min.js",
                        "~/Scripts/jquery.cookie.js",
                        "~/Scripts/jquery.elevatezoom.js",
                        "~/Scripts/tilesAndDialogs.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery/web").Include(
                        //"~/Scripts/jquery-{version}.js"
                        "~/Scripts/jquery-2.0.3.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jquery/web/add").Include(
                        "~/Scripts/jquery.form.js",
                        "~/Scripts/jquery.cookie.js",
                        "~/Scripts/jquery.elevatezoom.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryDataTable").Include(
                        "~/Scripts/DataTables-1.9.4/media/js/jquery.dataTables.js",
                        "~/Scripts/DataTables-1.9.4/media/js/plugins.js",
                        "~/Scripts/jquery.jeditable.js"));


            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                        "~/Scripts/jquery-ui-{version}.js"
                //"~/Scripts/jquery-ui-timepicker.js"  <---  error in jqueryUI after bundling and minification
                        ));


            bundles.Add(new ScriptBundle("~/bundles/jqueryVal").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/CMS/CustomValidation.js"));


            bundles.Add(new ScriptBundle("~/bundles/otherLibs").Include(
                        "~/Scripts/knockout-{version}.js",
                        "~/Scripts/knockout.mapping-latest.js",
                        "~/Scripts/CMS/KOBindingHandlers.js",
                        "~/Scripts/underscore-min.js",
                        "~/Scripts/underscore.string.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/web").Include(
                        "~/Scripts/CMS/scripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/web/add").Include(
                        "~/Scripts/fullcalendar.js"));

            bundles.Add(new ScriptBundle("~/bundles/cms").Include(
                        "~/Scripts/masonry.pkgd.min.js",
                        "~/Scripts/jquery-ui-timepicker.js",
                        "~/Scripts/CMS/scripts.js",
                        "~/Scripts/CMS/Core.js",
                        "~/Scripts/CMS/MessageManager.js",
                        "~/Scripts/CMS/UI/Messages.js",
                        "~/Scripts/CMS/UI/TinyMCEManager.js",
                        "~/Scripts/CMS/UI/Admin/LeftMenuManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Content/bootstrap/js/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminuser").Include(
                        "~/Scripts/CMS/UI/Admin/AdminUserManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/usergroups").Include(
                        "~/Scripts/CMS/UI/Admin/UserGroupsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/domains").Include(
                        "~/Scripts/CMS/UI/Admin/DomainsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminmenu").Include(
                        "~/Scripts/CMS/UI/Admin/AdminMenuManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/sections").Include(
                        "~/Scripts/CMS/UI/Admin/SectionManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/news").Include(
                        "~/Scripts/CMS/UI/Admin/NewsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/testimonials").Include(
                        "~/Scripts/CMS/UI/Admin/TestimonialsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/forms").Include(
                        "~/Scripts/CMS/UI/Admin/FormManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/formSubmissions").Include(
                        "~/Scripts/CMS/UI/Admin/FormSubmissionManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/postage").Include(
                        "~/Scripts/CMS/UI/Admin/PostageManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/prodCategories").Include(
                        "~/Scripts/CMS/UI/Admin/ProdCategoriesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/prodAttributes").Include(
                        "~/Scripts/CMS/UI/Admin/ProdAttributesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/discount").Include(
                        "~/Scripts/CMS/UI/Admin/DiscountManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/products").Include(
                        "~/Scripts/CMS/UI/Admin/ProductsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/customers").Include(
                        "~/Scripts/CMS/UI/Admin/CustomersManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/orders").Include(
                        "~/Scripts/CMS/UI/Admin/OrdersManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/taxes").Include(
                        "~/Scripts/CMS/UI/Admin/TaxesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/countries").Include(
                        "~/Scripts/CMS/UI/Admin/CountriesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/eventTypes").Include(
                        "~/Scripts/CMS/UI/Admin/EventTypesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/eventCategories").Include(
                        "~/Scripts/CMS/UI/Admin/EventCategoriesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/donation").Include(
                        "~/Scripts/CMS/UI/Admin/DonationManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/donationInfo").Include(
                        "~/Scripts/CMS/UI/Admin/DonationInfoManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/poiCategories").Include(
                        "~/Scripts/CMS/UI/Admin/POICategoriesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/poiTagsGroups").Include(
                        "~/Scripts/CMS/UI/Admin/POITagsGroupManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/poiTags").Include(
                       "~/Scripts/CMS/UI/Admin/POITagsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/pois").Include(
                       "~/Scripts/CMS/UI/Admin/POIsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/templates").Include(
                       "~/Scripts/CMS/UI/Admin/TemplatesManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/portfolio").Include(
                       "~/Scripts/CMS/UI/Admin/PortfolioManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/gallery").Include(
                       "~/Scripts/CMS/UI/Admin/GalleryManager.js"));


            bundles.Add(new ScriptBundle("~/bundles/blogManager").Include(
                        "~/Scripts/CMS/UI/Web/BlogManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/contactUsManager").Include(
                        "~/Scripts/jquery.form.js",   
                        "~/Scripts/CMS/UI/Web/ContactUsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/donationWebManager").Include(
                        "~/Scripts/CMS/UI/Web/DonationManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/productManager").Include(
                        "~/Scripts/CMS/UI/Web/ProductManager.js"));

/*            bundles.Add(new ScriptBundle("~/bundles/stripeManager").Include(
            "~/Scripts/CMS/UI/Web/StripeManager.js"));*/

            bundles.Add(new ScriptBundle("~/bundles/basketManager").Include(
                        "~/Scripts/CMS/Core.js",
                        "~/Scripts/CMS/UI/Web/BasketManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/checkout").Include(
                        "~/Scripts/CMS/UI/Web/CheckoutManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/subscriptionManager").Include(
                        "~/Scripts/CMS/UI/Web/SubscriptionManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/categoryManager").Include(
                        "~/Scripts/CMS/UI/Web/CategoryManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/eventManager").Include(
                        "~/Scripts/CMS/UI/Web/EventManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/poisWebManager").Include(
                        "~/Scripts/CMS/UI/Web/POIsManager.js"));

            bundles.Add(new ScriptBundle("~/bundles/cookieConsent").Include(
                        "~/Scripts/CMS/UI/Web/CookieConsentAsyncForm.js"));

            //  ----- Styles ------


            bundles.Add(new StyleBundle("~/Content/font-awesome.css").Include(
                        "~/Content/font-awesome/css/font-awesome.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/ui.css").IncludeDirectory("~/Content/themes/base", "jquery-ui.*", false));

            bundles.Add(new StyleBundle("~/Content/admin/admin.css").Include(
                        "~/Content/reset.css",
                        "~/Content/admin/site.css",
                        "~/Content/admin/tilesAndDialogs.css"));

            bundles.Add(new StyleBundle("~/Content/web.css").Include(
                        "~/Content/reset.css"
                        ));

            bundles.Add(new StyleBundle("~/Content/bootstrap/css/bootstrap.css").Include(
                        "~/Content/bootstrap/css/bootstrap.css"));

            BundleConfig.LoadBundles();
        }

        public static void LoadBundles()
        {
            BundleConfig.LoadScriptBundles();
            BundleConfig.LoadStyleBundles();
        }

        public static void LoadStyleBundles()
        {
            var bundles = BundleTable.Bundles.Where(b => b.Path.StartsWith("~/Theme/") && b.Path.EndsWith("/css")).ToList();
            bundles.ForEach(bundle => BundleTable.Bundles.Remove(bundle));

            List<string> themes = new List<string>();

            var domainService = (IDomain)DependencyResolver.Current.GetService(typeof(IDomain));
            if (domainService != null)
                themes = domainService.GetAllDomains().Select(d => d.DO_Theme).ToList();

            var server = HttpContext.Current.Server;

            foreach (var theme in themes)
            {
                var template = theme ?? SettingsManager.DefaultWebTheme;

                string vpath = String.Format("~/Theme/{0}/css", template);
                string path = String.Format("~/Themes/{0}/css", template);

                if (BundleTable.Bundles.Any(b => b.Path == vpath))
                    continue;

                if (!Directory.Exists(server.MapPath(path)))
                    continue;

                Bundle bundle = new Bundle(vpath, new CssMinify());
                bundle.IncludeDirectory(path, "*.css");

                BundleTable.Bundles.Add(bundle);
            }
        }

        public static void LoadScriptBundles()
        {
            var bundles = BundleTable.Bundles.Where(b => b.Path.StartsWith("~/Theme/") && b.Path.EndsWith("/js")).ToList();
            bundles.ForEach(bundle => BundleTable.Bundles.Remove(bundle));

            List<string> themes = new List<string>();

            var domainService = (IDomain)DependencyResolver.Current.GetService(typeof(IDomain));
            if (domainService != null)
                themes = domainService.GetAllDomains().Select(d => d.DO_Theme).ToList();

            var server = HttpContext.Current.Server;

            foreach (var theme in themes)
            {
                var template = theme ?? SettingsManager.DefaultWebTheme;

                string vpath = String.Format("~/Theme/{0}/js", template);
                string path = String.Format("~/Themes/{0}/js", template);

                if (BundleTable.Bundles.Any(b => b.Path == vpath))
                    continue;

                if (!Directory.Exists(server.MapPath(path)))
                    continue;

                Bundle bundle = new Bundle(vpath, new JsMinify());
                bundle.IncludeDirectory(path, "*.js");

                BundleTable.Bundles.Add(bundle);
            }
        }
    }
}
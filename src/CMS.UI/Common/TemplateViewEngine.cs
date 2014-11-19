using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.UI.Common
{
    public class TemplateViewEngine : RazorViewEngine
    {
        public TemplateViewEngine() : base()
        {
            ViewLocationFormats = new[] {
                "~/Themes/%T/Views/{1}/{0}.cshtml",
                "~/Themes/%T/Views/{1}/{0}.vbhtml",
                "~/Themes/%T/Views/Shared/{0}.cshtml",
                "~/Themes/%T/Views/Shared/{0}.vbhtml",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };

            PartialViewLocationFormats = new[] {
                "~/Themes/%T/Views/{1}/{0}.cshtml",
                "~/Themes/%T/Views/{1}/{0}.vbhtml",
                "~/Themes/%T/Views/Partials/{0}.cshtml",
                "~/Themes/%T/Views/Partials/{0}.vbhtml",
                "~/Themes/%T/Views/Shared/{0}.cshtml",
                "~/Themes/%T/Views/Shared/{0}.vbhtml",
                "~/Views/{1}/{0}.cshtml",
                "~/Views/{1}/{0}.vbhtml",
                "~/Views/Partials/{0}.cshtml",
                "~/Views/Partials/{0}.vbhtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Shared/{0}.vbhtml"
            };
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            var theme = controllerContext.Controller.ViewBag.Theme as string;
            return base.CreatePartialView(controllerContext, partialPath.Replace("%T", theme));
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            var theme = controllerContext.Controller.ViewBag.Theme as string;
            return base.CreateView(controllerContext, viewPath.Replace("%T", theme), masterPath);
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            var theme = controllerContext.Controller.ViewBag.Theme as string;
            return base.FileExists(controllerContext, virtualPath.Replace("%T", theme));
        }
    }
}
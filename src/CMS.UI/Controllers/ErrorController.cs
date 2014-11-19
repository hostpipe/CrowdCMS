using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using CMS.Services;

namespace CMS.UI.Controllers
{
    [Authorize]
    public class ErrorController : BaseWebController
    {
        public ErrorController(IDomain domainService,
            IWebContent webContentService,
            IECommerce ecommerceService,
            IUser userService)
            : base(domainService, ecommerceService, userService, webContentService)
        {
        }

        [AllowAnonymous]
        public ActionResult Http404()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View(GetMenuOrdered(false, false, true, true, true, true));
        }
    }
}

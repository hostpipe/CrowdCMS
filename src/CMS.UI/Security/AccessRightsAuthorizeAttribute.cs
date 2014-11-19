using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using CMS.Utils;
using CMS.BL.Entity;
using CMS.Services;
using System.Web.Mvc;

namespace CMS.UI.Security
{
    public class AccessRightsAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            string url = httpContext.Request.Url.AbsolutePath;
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService(typeof(IDomain));
            tbl_AdminMenu menuItem = domainService.GetAdminMenuItemByUrl(url);

            return menuItem.tbl_AccessRights.Where(ar => ar.AR_AdminMenuID.HasValue).Any(ar => ar.AR_UserGroupID == (httpContext.User as CustomPrincipal).UserGroupID);
        }
    }
}
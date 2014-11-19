using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CMS.Services;
using System.Web.Security;
using CMS.Utils;
using System.Web.Script.Serialization;

namespace CMS.UI.Security
{
    public class AuthenticationAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (base.AuthorizeCore(httpContext))
            {
                HttpCookie authCookie = httpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                CustomPrincipalSerializeModel principalModel = new JavaScriptSerializer().Deserialize<CustomPrincipalSerializeModel>(ticket.UserData);

                bool isAdmnRequest = httpContext.Request.Url.AbsolutePath.ToLower().StartsWith("/admn");
                if (isAdmnRequest)
                {
                    return principalModel.IsAdmn;
                }
                else
                    return !principalModel.IsAdmn;
            }
            return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var authCookie = filterContext.HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var userService = (IUser)DependencyResolver.Current.GetService(typeof(IUser));

                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                CustomPrincipalSerializeModel principalModel = new JavaScriptSerializer().Deserialize<CustomPrincipalSerializeModel>(ticket.UserData);
                CustomPrincipal principal = new CustomPrincipal(principalModel, 
                                                                principalModel.IsAdmn ? (int?)userService.GetUserGroupIDByUserID(principalModel.UserID) : null, 
                                                                principalModel.IsAdmn ? userService.GetPermissionsByUserID(principalModel.UserID) : null, 
                                                                AuthorizeCore(filterContext.HttpContext));
                filterContext.HttpContext.User = principal;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);

            string returnUrl = filterContext.HttpContext.Request.Url.AbsolutePath;
            bool isAdmnRequest = filterContext.HttpContext.Request.Url.AbsolutePath.ToLower().StartsWith("/admn");
            if (isAdmnRequest)
                filterContext.HttpContext.Response.Redirect("/Admn/Login?returnUrl=" + returnUrl);
            else
                filterContext.HttpContext.Response.Redirect("/login?returnUrl=" + returnUrl);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.Utils;
using CMS.BL;

namespace CMS.UI.Security
{
    public class AddContentAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.AddContent))
                return true;

            return false;
        }
    }

    public class ApproveContentAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.ApproveContent))
                return true;

            return false;
        }
    }

    public class DeleteContentAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.DeleteContent))
                return true;

            return false;
        }
    }

    public class EditContentAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.EditContent))
                return true;

            return false;
        }
    }

    public class AddNewsAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.AddNews))
                return true;

            return false;
        }
    }

    public class DeleteNewsAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.DeleteNews))
                return true;

            return false;
        }
    }

    public class EditNewsAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.EditNews))
                return true;

            return false;
        }
    }

    public class EditNewsCategoriesAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.AddNews) || (httpContext.User as CustomPrincipal).HasPermission(Permission.EditNews))
                return true;

            return false;
        }
    }

    public class AddUserAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.AddUser))
                return true;

            return false;
        }
    }

    public class DeleteUserAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.DeleteUser))
                return true;

            return false;
        }
    }

    public class EditUserAuthorizeAttribute : SecurityBaseAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            if ((httpContext.User as CustomPrincipal).HasPermission(Permission.EditUser))
                return true;

            return false;
        }
    }
}

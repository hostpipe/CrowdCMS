using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.Utils.Web;

namespace CMS.UI.Common
{

#pragma warning disable 1591
    /// <summary>
    /// Cookie variables
    /// </summary>
    public enum CookieVariables
    {
        BasketCookieName
    }
#pragma warning restore 1591


    public class CookieManager : CookieManagerBase<CookieVariables>
    {
        public static int? BasketCookie
        {
            get { return GetInt(CookieVariables.BasketCookieName); }
            set { Store(CookieVariables.BasketCookieName, value, CookieExpiration.Day, 1); }
        }
    }
}
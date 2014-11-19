using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CMS.Utils.Web
{
    /// <summary>
    /// Base class for session manager - to enable access to session in application
    /// </summary>
    public class SessionManagerBase<T>
    {
        public static void ClearSession()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

        public static void Store(T variable, object value)
        {
            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(variable.ToString());
                HttpContext.Current.Session.Add(variable.ToString(), value);
            }
        }

        public static int? GetInt(T variable)
        {
            if (HttpContext.Current.Session != null)
            {
                object value = HttpContext.Current.Session[variable.ToString()];

                if (value != null)
                {
                    return Convert.ToInt32(value);
                }
            }

            return null;
        }

        public static bool? GetBool(T variable)
        {
            if (HttpContext.Current.Session != null)
            {
                object value = HttpContext.Current.Session[variable.ToString()];

                if (value != null)
                {
                    return Convert.ToBoolean(value);
                }
            }

            return null;
        }

        public static string GetString(T variable)
        {
            if (HttpContext.Current.Session != null)
            {
                object value = HttpContext.Current.Session[variable.ToString()];

                if (value != null)
                {
                    return Convert.ToString(value);
                }
            }

            return null;
        }

        public static object GetObject(T variable)
        {
            if (HttpContext.Current.Session != null)
            {
                return HttpContext.Current.Session[variable.ToString()];
            }
            else
            {
                return null;
            }
        }
    }
}

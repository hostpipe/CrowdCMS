using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using CMS.Utils.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace CMS.Utils.Web
{
    public class CookieManagerBase<T>
    {
        public enum CookieExpiration { Year, Month, Day, Hour, Minute }


        public static void Store(T variable, object value, DateTime expireDate)
        {
            HttpCookie cookie = new HttpCookie(variable.ToString(), value.ToString());
            cookie.Expires = expireDate;
            HttpContext.Current.Response.Cookies.Remove(variable.ToString());
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void StoreBinary(T variable, object value, DateTime expireDate)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, value);
                Store(variable, System.Convert.ToBase64String(memoryStream.ToArray()), expireDate);
            }
            catch (Exception ex)
            {
                Log.Error<CookieManagerBase<T>>(String.Format("Error when setting cookie with name: {0}. Error message: {1}", variable.ToString(), ex.Message), ex);
            }
        }

        public static void Store(T variable, object value, int CookieExpirationTime)
        {
            Store(variable, value, CookieExpiration.Month, CookieExpirationTime);
        }

        public static void StoreBinary(T variable, object value, int CookieExpirationTime)
        {
            StoreBinary(variable, value, CookieExpiration.Month, CookieExpirationTime);
        }

        public static void Store(T variable, object value, CookieExpiration timespan, int expiration)
        {
            DateTime expireDate = DateTime.Today;
            switch (timespan)
            {
                case CookieExpiration.Year: expireDate = DateTime.Today.AddYears(expiration); break;
                case CookieExpiration.Month: expireDate = DateTime.Today.AddMonths(expiration); break;
                case CookieExpiration.Day: expireDate = DateTime.Today.AddDays(expiration); break;
                case CookieExpiration.Hour: expireDate = DateTime.Now.AddHours(expiration); break;
                case CookieExpiration.Minute: expireDate = DateTime.Now.AddMinutes(expiration); break;
            }
            Store(variable, value, expireDate);
        }

        public static void StoreBinary(T variable, object value, CookieExpiration timespan, int expiration)
        {
            DateTime expireDate = DateTime.Today;
            switch (timespan)
            {
                case CookieExpiration.Year: expireDate = DateTime.Today.AddYears(expiration); break;
                case CookieExpiration.Month: expireDate = DateTime.Today.AddMonths(expiration); break;
                case CookieExpiration.Day: expireDate = DateTime.Today.AddDays(expiration); break;
                case CookieExpiration.Hour: expireDate = DateTime.Now.AddHours(expiration); break;
                case CookieExpiration.Minute: expireDate = DateTime.Now.AddMinutes(expiration); break;
            }
            StoreBinary(variable, value, expireDate);
        }
        

        public static int? GetInt(T variable)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[variable.ToString()];
            if (cookie != null)
            {
                try
                {
                    return Convert.ToInt32(cookie.Value);
                }
                catch
                {
                    //removing invalid cookie from response
                    Remove(variable);
                }
            }

            return null;
        }

        public static string GetString(T variable)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[variable.ToString()];

            if (cookie != null)
            {
                return Convert.ToString(cookie.Value);
            }

            return null;
        }

        public static object GetObjectFromBinary(T variable)
        {
            object obj = null;
            string stringValue = GetString(variable);
            if (!String.IsNullOrEmpty(stringValue))
            {
                try
                {
                    byte[] b = Convert.FromBase64String(stringValue);
                    MemoryStream memoryStream = new MemoryStream(b);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    obj = binaryFormatter.Deserialize(memoryStream);
                }
                catch (Exception ex)
                {
                    Log.Error<CookieManagerBase<T>>(String.Format("Error when getting cookie with name: {0}. Error message: {1}", variable.ToString(), ex.Message), ex);
                    Remove(variable);
                }
            }
            return obj;
        }

        public static void Remove(T variable)
        {
            HttpCookie cookie = new HttpCookie(variable.ToString(), null);

            cookie.Expires = DateTime.Today.AddMonths(-1);

            HttpContext.Current.Response.Cookies.Remove(variable.ToString());
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}

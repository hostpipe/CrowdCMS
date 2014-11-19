using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using CMS.Utils.Diagnostics;

namespace CMS.Utils
{
    /// <summary>
    /// Settings manager to get configuration data of application (from web.config, app.config, machine.config)
    /// </summary>
    public class SettingsManagerBase<T>
    {

        #region Public and protected methods

        /// <summary>
        /// Gets string value from AppSettings section
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetStringAppSetting(T key)
        {
            string value = ConfigurationManager.AppSettings[key.ToString()];
            if (string.IsNullOrEmpty(value))
            {
                Log.Debug<SettingsManagerBase<T>>(String.Format("AppSetting parameter [{0}] doesn't exist or is not set", key.ToString()));
            }

            return value;
        }

        protected static string GetStringAppSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                Log.Debug<SettingsManagerBase<T>>(String.Format("AppSetting parameter [{0}] doesn't exist or is not set", key));
            }

            return value;
        }

        /// <summary>
        /// Gets integer value from AppSettings section
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int? GetIntAppSetting(T key)
        {
            string value = GetStringAppSetting(key);
            if (!string.IsNullOrEmpty(value))
            {
                return Convert.ToInt32(value);
            }

            return null;
        }

        /// <summary>
        /// Gets integer value from AppSettings section
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int? GetIntAppSetting(string key)
        {
            string value = GetStringAppSetting(key);
            if (!string.IsNullOrEmpty(value))
            {
                return Convert.ToInt32(value);
            }

            return null;
        }


        /// <summary>
        /// Gets boolesn value from AppSettings section
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool? GetBoolAppSetting(T key)
        {
            string value = GetStringAppSetting(key);
            if (value != null)
            {
                return Convert.ToBoolean(value);
            }

            return null;
        }

        /// <summary>
        /// Gets boolesn value from AppSettings section
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool? GetBoolAppSetting(string key)
        {
            string value = GetStringAppSetting(key);
            if (!String.IsNullOrEmpty(value))
            {
                return Convert.ToBoolean(value);
            }

            return null;
        }


        /// <summary>
        /// Gets connection string from ConnectionStrings section
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetConnectionString(T name)
        {
            string value = ConfigurationManager.ConnectionStrings[name.ToString()].ConnectionString;
            if (string.IsNullOrEmpty(value))
            {
                Log.Warn<SettingsManagerBase<T>>(String.Format("ConnectionString [{0}] doesn't exist or is not set", name.ToString()));
            }

            return value;
        }

        #endregion
    }
}

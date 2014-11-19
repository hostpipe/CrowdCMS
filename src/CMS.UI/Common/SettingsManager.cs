using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.Utils;
using System.Text;

namespace CMS.UI
{

#pragma warning disable 1591
    /// <summary>
    /// Settings variables (from web.config, app.config, machine.config).
    /// </summary>
    public enum SettingsVariables
    {
        CookieExpireTimeInDays,
        DefaultWebTheme,
        PassLength,
        CSVEmailFile,
        CSVOrderSummaryFile,
        DonationInfo_ImagePath,
        Email_Templates_Path,
        Email_Templates_Type,        
        Form_Templates_Path,
        Form_Templates_Type,
        Images_PageImagesPath,
        Images_GalleryPath,
        Images_Product_OriginalPath,
        Images_ResizeOriginal,
        Images_MaxOriginalHeight,
        Images_MaxOriginalWidth,
        Blog_CategoryUrl,
        Blog_SearchUrl,
        Blog_TagUrl,
        Blog_RecentItemsAmount,
        Blog_RssItemsAmount,
        Sitemap_File,
        Sitemap_Folder,
        PaymentLogos_Path,
        EventTypeIcon_Path,
        Url2Png_Key,
        Url2Png_Secret,
        CSVCustomersFile

#if DEBUG
        ,LocalHostDomainID
#endif

    }
#pragma warning restore 1591


    public class SettingsManager : SettingsManagerBase<SettingsVariables>
    {
        public static int CookieExpireTime
        {
            get { return GetIntAppSetting(SettingsVariables.CookieExpireTimeInDays).GetValueOrDefault(31); }
        }

        public static string DefaultWebTheme
        {
            get { return GetStringAppSetting(SettingsVariables.DefaultWebTheme); }
        }

        public static int PassLength
        {
            get { return GetIntAppSetting(SettingsVariables.PassLength).GetValueOrDefault(7); }
        }

        public static string CSVEmailFile
        {
            get { return GetStringAppSetting(SettingsVariables.CSVEmailFile); }
        }

        public static string CSVOrderSummaryFile
        {
            get { return GetStringAppSetting(SettingsVariables.CSVOrderSummaryFile); }
        }

        public static string CSVCustomersFile
        {
            get { return GetStringAppSetting(SettingsVariables.CSVCustomersFile); }
        }

#if DEBUG
        public static int LocalHostDomainID
        {
            get { return GetIntAppSetting(SettingsVariables.LocalHostDomainID).GetValueOrDefault(1); }
        }
#endif

        public struct Sitemap
        {
            public static string File
            {
                get { return GetStringAppSetting(SettingsVariables.Sitemap_File); }
            }

            public static string Folder
            {
                get
                {
                    string value = GetStringAppSetting(SettingsVariables.Sitemap_Folder);
                    return "/" + value.Trim('/') + "/";
                }
            }
        }

        public struct Email
        {
            public struct Templates
            {
                public static string Path
                {
                    get { return GetStringAppSetting(SettingsVariables.Email_Templates_Path); }
                }

                public static string Type
                {
                    get { return GetStringAppSetting(SettingsVariables.Email_Templates_Type); }
                }
            }
        }

        public struct Form
        {
            public struct Templates
            {
                public static string Path
                {
                    get { return GetStringAppSetting(SettingsVariables.Form_Templates_Path); }
                }

                public static string Type
                {
                    get { return GetStringAppSetting(SettingsVariables.Form_Templates_Type); }
                }
            }
        }

        public struct Images
        {
   
            public static string PageImagesPath
            {
                get { return GetStringAppSetting(SettingsVariables.Images_PageImagesPath); }
            }

            public static string GalleryPath
            {
                get { return GetStringAppSetting(SettingsVariables.Images_GalleryPath); }
            }

            public static string OriginalImagePath
            {
                get { return "/" + GetStringAppSetting(SettingsVariables.Images_Product_OriginalPath).Trim('/') + "/"; }
            }

            public static bool ResizeOriginal
            {
                get { return GetBoolAppSetting(SettingsVariables.Images_ResizeOriginal).GetValueOrDefault(false); }
            }

            public static int MaxOriginalHeight
            {
                get { return GetIntAppSetting(SettingsVariables.Images_MaxOriginalHeight).GetValueOrDefault(2000); }
            }

            public static int MaxOriginalWidth
            {
                get { return GetIntAppSetting(SettingsVariables.Images_MaxOriginalWidth).GetValueOrDefault(2000); }
            }
        }

        public struct Payment
        {
            public static string PaymentLogosPath
            {
                get { return GetStringAppSetting(SettingsVariables.PaymentLogos_Path); }
            }
        }

        public struct Blog
        {
            public static string CategoryUrl
            {
                get { return GetStringAppSetting(SettingsVariables.Blog_CategoryUrl); }
            }

            public static string SearchUrl
            {
                get { return GetStringAppSetting(SettingsVariables.Blog_SearchUrl); }
            }

            public static string TagUrl
            {
                get { return GetStringAppSetting(SettingsVariables.Blog_TagUrl); }
            }

            public static int RecentItemsAmount
            {
                get 
                { 
                    return GetIntAppSetting(SettingsVariables.Blog_RecentItemsAmount).GetValueOrDefault(5);
                }
            }

            public static int RssItemsAmount
            {
                get 
                { 
                    return GetIntAppSetting(SettingsVariables.Blog_RssItemsAmount).GetValueOrDefault(10);
                }
            }
        }

        public struct DonationInfo
        {
            public static string Path
            {
                get { return "/" + GetStringAppSetting(SettingsVariables.DonationInfo_ImagePath).Trim('/') + "/"; }
            }
        }

        public struct EventTypeIcon
        {
            public static string Path
            {
                get { return "/" + GetStringAppSetting(SettingsVariables.EventTypeIcon_Path).Trim('/') + "/"; }
            }
        }

        public struct Url2Png
        {
            public static string Key
            {
                get { return GetStringAppSetting(SettingsVariables.Url2Png_Key); }
            }

            public static string Secret
            {
                get { return GetStringAppSetting(SettingsVariables.Url2Png_Secret); }
            }

        }
    }
}
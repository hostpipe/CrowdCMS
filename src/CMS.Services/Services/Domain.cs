using System;
using System.Collections.Generic;
using System.Linq;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Repository;
using CMS.Utils.Extension;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;
using Microsoft.Security.Application;
using CMS.Utils.Diagnostics;
using CMS.Utils;

namespace CMS.Services
{
    public class Domain : ServiceBase, IDomain
    {
        private IAdminMenuRepository AdminMenuRepository { get; set; }
        private IDomainsRepository DomainsRepository { get; set; }
        private IDomainLinkRepository DomainLinkRepository { get; set; }
        private IPaymentDomainRepository PaymentDomainRepository { get; set; }
        private IPaymentTypeRepository PaymentTypeRepository { get; set; }
        private IPermissionsRepository PermissionsRepository { get; set; }
        private ISettingsOptionsRepository SettingsOptionsRepository { get; set; }
        private ISettingsRepository SettingsRepository { get; set; }
        private ISettingsValuesRepository SettingsValuesRepository { get; set; }
        private ISitemapRepository SitemapRepository { get; set; }
        private IContentRepository ContentRepository { get; set; }
        private ISocialRepository SocialRepository { get; set; }
        private ISocialDefaultRepository SocialDefaultRepository { get; set; }

        public Domain()
            : base()
        {
            this.AdminMenuRepository = new AdminMenuRepository(this.Context);
            this.DomainsRepository = new DomainsRepository(this.Context);
            this.DomainLinkRepository = new DomainLinkRepository(this.Context);
            this.PaymentDomainRepository = new PaymentDomainRepository(this.Context);
            this.PaymentTypeRepository = new PaymentTypeRepository(this.Context);
            this.PermissionsRepository = new PermissionsRepository(this.Context);
            this.SettingsRepository = new SettingsRepository(this.Context);
            this.SettingsOptionsRepository = new SettingsOptionsRepository(this.Context);
            this.SettingsValuesRepository = new SettingsValuesRepository(this.Context);
            this.SitemapRepository = new SitemapRepository(this.Context);
            this.ContentRepository = new ContentRepository(this.Context);
            this.SocialRepository = new SocialRepository(this.Context);
            this.SocialDefaultRepository = new SocialDefaultRepository(this.Context);
        }

        #region AdminMenu repo

        public bool DeleteAdminMenuItem(int menuItemID)
        {
            return AdminMenuRepository.DeleteMenuItem(menuItemID);
        }

        public List<tbl_AdminMenu> GetAllAdminMenuItems()
        {
            return AdminMenuRepository.GetAll().ToList();
        }

        public List<tbl_AdminMenu> GetAllAdminMenuItemsByAccessrights(int userGroupID)
        {
            return AdminMenuRepository.GetAll()
                .Where(am => am.AM_Live &&
                    am.tbl_AccessRights.Any(ar => ar.AR_UserGroupID == userGroupID)).ToList();
        }

        public List<ExtendedSelectListItem> GetAdminMenuOrdered(int selectedGroupID = 0)
        {
            return CreateChildMenuOrdered(AdminMenuRepository.GetAll(), 0, selectedGroupID, 1);
        }

        private List<ExtendedSelectListItem> CreateChildMenuOrdered(IQueryable<tbl_AdminMenu> items, int parentID, int selectedGroupID, int level)
        {
            var adminMenuTemp = new List<ExtendedSelectListItem>();
            foreach (var item in items.Where(am => am.AM_ParentID == parentID).OrderBy(am => am.AM_Order))
            {
                adminMenuTemp.Add(new ExtendedSelectListItem()
                {
                    Text = item.AM_MenuText,
                    Value = item.AdminMenuID.ToString(),
                    Selected = (selectedGroupID == 0) ?
                        false :
                        item.tbl_AccessRights.Select(ar => ar.AR_UserGroupID).Contains(selectedGroupID),
                    Level = level
                });

                adminMenuTemp.AddRange(CreateChildMenuOrdered(items, item.AdminMenuID, selectedGroupID, level + 1));
            }
            return adminMenuTemp;
        }

        public tbl_AdminMenu GetAdminMenuItemByID(int menuItemID)
        {
            return AdminMenuRepository.GetByID(menuItemID);
        }

        public tbl_AdminMenu GetAdminMenuItemByUrl(string url)
        {
            url = url.TrimEnd('/');
            return AdminMenuRepository.GetAll().FirstOrDefault(am => am.AM_URL == url);
        }

        public tbl_AdminMenu SaveAdminMenuItem(string menuText, int parentID, string url, int menuItemID)
        {
            return (String.IsNullOrEmpty(menuText) || String.IsNullOrEmpty(url)) ?
                null :
                AdminMenuRepository.SaveMenuItem(menuText, parentID, url, menuItemID);
        }

        public bool SaveAdminMenuItemsOrder(int[] orderedMenuItemIDs)
        {
            return (orderedMenuItemIDs == null) ?
                false :
                AdminMenuRepository.SaveOrder(orderedMenuItemIDs);
        }

        public tbl_AdminMenu SaveAdminMenuItemVisibility(int menuItemID)
        {
            return AdminMenuRepository.SaveVisibility(menuItemID);
        }

        #endregion


        #region Domain repo

        public bool CanDeleteDomain(int domainID)
        {
            return DomainsRepository.CanDeleteDomain(domainID);
        }

        public bool DeleteDomain(int domainID)
        {
            return DomainsRepository.DeleteDomain(domainID);
        }

        public List<tbl_Domains> GetAllDomains()
        {
            return DomainsRepository.GetAll().ToList();
        }

        public SelectList GetAllDomainsAsSelectList(int selectDomainID)
        {
            return new SelectList(DomainsRepository.GetAll().OrderBy(d => d.DO_CompanyName), "DomainID", "DO_CompanyName", selectDomainID);
        }

        public List<tbl_SettingsOptions> GetSettingsOptions(int settingID)
        {
            return SettingsOptionsRepository.GetAllForSetting(settingID);
        }

        public tbl_Domains GetDomainByID(int domainID)
        {
            return DomainsRepository.GetByID(domainID);
        }

        public tbl_Domains GetDomainByName(string domainName)
        {
            tbl_Domains domain = DomainsRepository.GetByDomainName(domainName);
            if (domain == null)
                domain = DomainsRepository.GetByDomainLinkName(domainName);

            return domain;
        }

        public tbl_Domains SaveDomain(string address, string name, string phone, string consumerKey, string consumerSecret, string css, string defaultDesc, string keywords,
            int langID, string title, string desc, string domainName, string email, string googleAnalytics, string googleAnalyticsCode, bool googleAnalyticsVisible,
            string robots, string headline, int homePageID, int? launchYear, bool primaryDomain, string share, string twitterSecret, string twitterToken, string twitterUser, bool updateTwitter,
            bool isMailChimpEnabled, string mailChimpAPIKey, string mailChimpListID, bool isCommuniGatorEnabled, string communiGatorUser, string communiGatorPassword, 
            List<tbl_SettingsValue> settingsValues, bool IsPaypalPayment, bool IsSagePayPayment, bool IsSecureTradingPayment, EventViewType eventView, bool enableEventSale, 
            bool enableProductSale, string theme, bool devMode, int domainID, bool isCoockieConsentEnabled, bool isStripePayment, List<tbl_Social> socialValues, bool customRouteHandler)
        {
            if (String.IsNullOrEmpty(domainName))
                return null;

            var domain = DomainsRepository.SaveDomain(address, name, phone, consumerKey, consumerSecret,
                css, defaultDesc, keywords, langID, title, desc, domainName, email, googleAnalytics, googleAnalyticsCode, 
                googleAnalyticsVisible, robots, headline, homePageID, launchYear, primaryDomain, share, twitterSecret, 
                twitterToken, twitterUser, updateTwitter, isMailChimpEnabled, mailChimpAPIKey, mailChimpListID,
                isCommuniGatorEnabled, communiGatorUser, communiGatorPassword, eventView, enableEventSale, 
                enableProductSale, theme, devMode, isCoockieConsentEnabled, customRouteHandler, domainID);

            if (domain != null)
            {
                if (domainID == 0)
                {
                    foreach (var value in Enum.GetValues(typeof(SiteMapType)))
                    {
                        SiteMapType type = (SiteMapType)Enum.Parse(typeof(SiteMapType), value.ToString());

                        var sitemap = SitemapRepository.SaveSiteMap(string.Empty, 1, 0, domain.DomainID, String.Empty, false, false, type.ToString(), null,
                            (decimal)0.5, String.Empty, FriendlyUrl.CreateFriendlyUrl(type.ToString()), true, false, ContentType.Content, type.ToString(), 0, 0, (int)type, true);

                        if (sitemap != null)
                            ContentRepository.SaveContent(String.Empty, String.Empty, type.ToString(), 0, string.Empty, type.ToString(), type.ToString(),
                                string.Empty, 0, string.Empty, string.Empty, string.Empty, type.ToString(), string.Empty, false, sitemap.SiteMapID, 0);
                    }

                    socialValues = this.GetDefaultSocialValues(domain.DomainID);
                }

                PaymentDomainRepository.UpdateStatus(IsPaypalPayment, PaymentType.PayPal, domain.DomainID);
                PaymentDomainRepository.UpdateStatus(IsSagePayPayment, PaymentType.SagePay, domain.DomainID);
                PaymentDomainRepository.UpdateStatus(IsSecureTradingPayment, PaymentType.SecureTrading, domain.DomainID);
                PaymentDomainRepository.UpdateStatus(isStripePayment, PaymentType.Stripe, domain.DomainID);
                SettingsValuesRepository.Save(settingsValues, domain.DomainID);
                SocialRepository.SaveMultipleSocial(socialValues);
                return domain;
            }

            return null;
        }

        #endregion


        #region Domain Links repo

        public int DeleteDomainLinks(int[] domainLinkIDs)
        {
            var domain = DomainsRepository.GetByDomainLinkID(domainLinkIDs[0]);
            DomainLinkRepository.DeleteDomainLinks(domainLinkIDs);
            return (domain != null) ?
                domain.DomainID :
                0;
        }

        public tbl_DomainLink SaveDomainLink(int domainID, string link)
        {
            return (string.IsNullOrEmpty(link)) ?
                null :
                DomainLinkRepository.SaveDomainLink(domainID, link);
        }

        #endregion

        #region Permissions repo

        public List<SelectListItem> GetPermissions(int selectedGroupID = 0)
        {
            return PermissionsRepository.GetAll().Select(p => new SelectListItem()
            {
                Text = p.P_Name,
                Value = SqlFunctions.StringConvert((double)p.PermissionID).Trim(),
                Selected = (selectedGroupID == 0) ?
                    false :
                    p.tbl_AccessRights.Select(ar => ar.AR_UserGroupID).Contains(selectedGroupID)
            }).ToList();
        }

        #endregion

        #region Settings repo

        public List<tbl_Settings> GetAllSettings()
        {
            return SettingsRepository.GetAll().ToList();
        }

        public tbl_Settings GetSettingsByID(int settingsID)
        {
            return SettingsRepository.GetSettingsByID(settingsID);
        }

        public List<tbl_Settings> GetDefaultSettingsValuesList()
        {
            return SettingsRepository.GetAll().Where(m => m.SE_IsDomain).ToList();
        }

        public string GetSettingsValue(SettingsKey key, int domainID)
        {
            return GetSettingsValue<string>(key, domainID);
        }

        public int GetSettingsValueAsInt(SettingsKey key, int domainID)
        {
            return GetSettingsValue<int>(key, domainID);
        }

        public decimal GetSettingsValueAsDecimal(SettingsKey key, int domainID)
        {
            return GetSettingsValue<decimal>(key, domainID);
        }

        public bool GetSettingsValueAsBool(SettingsKey key, int domainID)
        {
            return GetSettingsValue<bool>(key, domainID);
        }

        public tbl_Settings GetSettingsByValue(string value, int domainID)
        {
            return SettingsRepository.GetSettingsByValue(value);
        }

        public bool SaveSettings(Dictionary<string, string> settings)
        {
            foreach (var item in settings.ToList())
            {
                var val = Sanitizer.GetSafeHtmlFragment(item.Value);
                settings[item.Key] = (val == "true,false") ?
                    "true" :
                    val;
            }
            return SettingsRepository.SaveSettings(settings);
        }

        public int GetSettingsIdByKey(SettingsKey key)
        {
            return SettingsRepository.GetValueByKey(key).SettingID;
        }

        #endregion

        #region Social repo

        public List<tbl_Social> GetAllSocial(int domainID)
        {
            return SocialRepository.GetByDomainID(domainID).ToList();
        }

        public tbl_Social GetSocialByID(int socialID)
        {
            return SocialRepository.GetByID(socialID);
        }

        public tbl_Social SaveSocial(string title, string url, int domainID, int socialID, string foreColour = null, string backColour= null)
        {
            return SocialRepository.SaveSocial(title, url, domainID, socialID, foreColour, backColour);
        }

        public bool SwitchStatus(int socialID)
        {
            return SocialRepository.SwapStatus(socialID);
        }

        public List<tbl_Social> GetDefaultSocialValues(int domainID)
        {
            var defaults = SocialDefaultRepository.GetAllDefaults();
            return defaults.Select(x => new tbl_Social() {
                S_Title = x.S_Title,
                S_IconClass = x.S_IconClass,
                S_Live = false,
                S_URL = x.S_URL,
                S_DefaultBackColour = x.S_DefaultBackColour,
                S_DefaultForeColour = x.S_DefaultForeColour,
                S_ForeColour = x.S_ForeColour,
                S_BackColour = x.S_BackColour,
                S_DomainID = domainID,
                S_BorderRadius = x.S_BorderRadius
            }).ToList();
        }

        #endregion

        #region Private Methodes

        private T GetSettingsValue<T>(SettingsKey key, int domainID)
        {
            if (domainID == 0)
            {
                var settings = SettingsRepository.GetValueByKey(key);
                return (settings == null) ?
                    (T)Activator.CreateInstance(typeof(T), typeof(T) == typeof(string) ? new Char[1] : null) :
                    Parse<T>(settings.SE_Value, key);
            }
            else
            {
                var settings = SettingsValuesRepository.GetValueByKey(key, domainID);
                return (settings == null) ?
                    (T)Activator.CreateInstance(typeof(T), typeof(T) == typeof(string) ? new Char[1] : null) :
                    Parse<T>(settings.SV_Value, key);
            }
        }

        T Parse<T>(string value, SettingsKey key)
        {
            try
            {
                dynamic retValue = null;
                if (typeof(T) == typeof(int))
                    retValue = Int32.Parse(value);
                else if (typeof(T) == typeof(string))
                    retValue = value;
                else if (typeof(T) == typeof(bool))
                    retValue = Boolean.Parse(value);
                else if (typeof(T) == typeof(decimal))
                    retValue = Decimal.Parse(value);
                if (retValue != null)
                    return retValue;
                return (T)Activator.CreateInstance(typeof(T), typeof(T) == typeof(string) ? new Char[1] : null);
            }
            catch (FormatException)
            {
                Log.Error(String.Format("Setting value {0} has invalid format.", key.ToString()));
                throw new Exception(String.Format("Setting value {0} has invalid format.", key.ToString()));
            }
        }

        #endregion
    }
}

using System.Collections.Generic;
using CMS.Utils.Extension;
using System.Web.Mvc;
using CMS.BL;
using CMS.BL.Entity;

namespace CMS.Services
{
    public interface IDomain
    {
        bool DeleteAdminMenuItem(int menuItemID);
        List<tbl_AdminMenu> GetAllAdminMenuItems();
        List<tbl_AdminMenu> GetAllAdminMenuItemsByAccessrights(int userGroupID);
        List<ExtendedSelectListItem> GetAdminMenuOrdered(int selectedGroupID = 0);
        tbl_AdminMenu GetAdminMenuItemByID(int menuItemID);
        tbl_AdminMenu GetAdminMenuItemByUrl(string url);
        tbl_AdminMenu SaveAdminMenuItem(string menuText, int parentID, string url, int menuItemID);
        bool SaveAdminMenuItemsOrder(int[] orderedMenuItemIDs);
        tbl_AdminMenu SaveAdminMenuItemVisibility(int menuItemID);

        bool CanDeleteDomain(int domainID);
        bool DeleteDomain(int domainID);
        List<tbl_Domains> GetAllDomains();
        SelectList GetAllDomainsAsSelectList(int selectDomainID);
        tbl_Domains GetDomainByID(int domainID);
        tbl_Domains GetDomainByName(string domainName);
        tbl_Domains SaveDomain(string address, string name, string phone, string consumerKey, string consumerSecret, string css, string defaultDesc, string keywords,
            int langID, string title, string desc, string domainName, string email, string googleAnalytics, string googleAnalyticsCode, bool googleAnalyticsVisible,
            string robots, string headline, int homePageID, int? launchYear, bool primaryDomain, string share, string twitterSecret, string twitterToken, string twitterUser, bool updateTwitter,
            bool isMailChimpEnabled, string mailChimpAPIKey, string mailChimpListID, bool isCommuniGatorEnabled, string communiGatorUser, string communiGatorPassword,
            List<tbl_SettingsValue> settingsValues, bool IsPaypalPayment, bool IsSagePayPayment, bool IsSecureTradingPayment, EventViewType eventView, bool enableEventSale, 
            bool enableProductSale, string theme, bool devMode, int domainID, bool isCoockieConsentEnabled, bool isStripePayment, List<tbl_Social> socialValues, bool customRouteHandler);

        int DeleteDomainLinks(int[] domainLinkIDs);
        tbl_DomainLink SaveDomainLink(int domainID, string link);

        List<SelectListItem> GetPermissions(int selectedGroupID = 0);

        List<tbl_Settings> GetAllSettings();
        List<tbl_Settings> GetDefaultSettingsValuesList();
        tbl_Settings GetSettingsByID(int settingsID);
        string GetSettingsValue(SettingsKey key, int domainID = 0);
        int GetSettingsValueAsInt(SettingsKey key, int domainID);
        decimal GetSettingsValueAsDecimal(SettingsKey key, int domainID);
        bool GetSettingsValueAsBool(SettingsKey key, int domainID);
        tbl_Settings GetSettingsByValue(string value, int domainID);
        bool SaveSettings(Dictionary<string, string> settings);
        int GetSettingsIdByKey(SettingsKey key);

        List<tbl_SettingsOptions> GetSettingsOptions(int settingID);

        List<tbl_Social> GetAllSocial(int domainID);
        tbl_Social GetSocialByID(int socialID);
        tbl_Social SaveSocial(string title, string url, int domainID, int socialID, string foreColour = null, string backColour = null);
        bool SwitchStatus(int socialID);
        List<tbl_Social> GetDefaultSocialValues(int domainID);
    }
}

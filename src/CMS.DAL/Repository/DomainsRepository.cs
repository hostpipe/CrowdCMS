using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using CMS.BL;

namespace CMS.DAL.Repository
{
    public interface IDomainsRepository
    {
        bool CanDeleteDomain(int domainID);
        bool DeleteDomain(int domainID);
        IQueryable<tbl_Domains> GetAll();
        tbl_Domains GetByID(int domainID);
        tbl_Domains GetByDomainName(string domain);
        tbl_Domains GetByDomainLinkName(string domainLink);
        tbl_Domains GetByDomainLinkID(int domainLinkID);
        tbl_Domains SaveDomain(string address, string name, string phone, string consumerKey, string consumerSecret, string css, string defaultDesc,
            string keywords, int langID, string title, string desc, string domainName, string email, string googleAnalytics, string googleAnalyticsCode,
            bool googleAnalyticsVisible, string robots, string headline, int homePageID, int? launchYear, bool primaryDomain, string share, string twitterSecret,
            string twitterToken, string twitterUser, bool updateTwitter, bool isMailChimpEnabled, string mailChimpAPIKey, string mailChimpListID,
            bool isCommuniGatorEnabled, string communiGatorUser, string communiGatorPassword, EventViewType eventView, bool enableEventSale, 
            bool enableProductSale, string theme, bool devMode, bool isCoockieConsentEnabled, bool customRouteHandler, int domainID = 0);
    }

    public class DomainsRepository : Repository<tbl_Domains>, IDomainsRepository
    {
        public DomainsRepository(IDALContext context) : base(context) { }

        public bool CanDeleteDomain(int domainID)
        {
            return this.DbSet.Any(d => d.DomainID == domainID && (d.tbl_Country.Count > 0 || d.tbl_Customer.Count > 0 || d.tbl_Discount.Count > 0 || d.tbl_Form.Count > 0 || d.tbl_Orders.Count > 0 ||
                d.tbl_Postage.Count > 0 || d.tbl_PostageBands.Count > 0 || d.tbl_PostageWeights.Count > 0 || d.tbl_PostageZones.Count > 0 || d.tbl_SiteMap.Count > 0));
        }

        public bool DeleteDomain(int domainID)
        {
            var domain = this.DbSet.FirstOrDefault(d => d.DomainID == domainID);
            if (domain == null)
                return false;

            domain.DO_Deleted = true;

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Domains> GetAll()
        {
            return this.DbSet.Where(d => !d.DO_Deleted);
        }

        public tbl_Domains GetByID(int domainID)
        {
            return this.DbSet.FirstOrDefault(d => !d.DO_Deleted && d.DomainID == domainID);
        }

        public tbl_Domains GetByDomainName(string domain)
        {
            return this.DbSet.FirstOrDefault(d => !d.DO_Deleted && d.DO_Domain.Equals(domain));
        }

        public tbl_Domains GetByDomainLinkName(string domainLink)
        {
            return this.DbSet.FirstOrDefault(d => !d.DO_Deleted && d.tbl_DomainLink.Any(dl => dl.DL_Domain.Equals(domainLink)));
        }

        public tbl_Domains GetByDomainLinkID(int domainLinkID)
        {
            return this.DbSet.FirstOrDefault(d => !d.DO_Deleted && d.tbl_DomainLink.Any(dl => dl.DomainLinkID == domainLinkID));
        }

        public tbl_Domains SaveDomain(string address, string name, string phone, string consumerKey, string consumerSecret, string css, string defaultDesc,
            string keywords, int langID, string title, string desc, string domainName, string email, string googleAnalytics, string googleAnalyticsCode,
            bool googleAnalyticsVisible, string robots, string headline, int homePageID, int? launchYear, bool primaryDomain, string share, string twitterSecret,
            string twitterToken, string twitterUser, bool updateTwitter, bool isMailChimpEnabled, string mailChimpAPIKey, string mailChimpListID,
            bool isCommuniGatorEnabled, string communiGatorUser, string communiGatorPassword, EventViewType eventView, bool enableEventSale, 
            bool enableProductSale, string theme, bool devMode, bool isCoockieConsentEnabled, bool customRouteHandler, int domainID = 0)
        {
            var domain = this.DbSet.FirstOrDefault(d => d.DomainID == domainID);
            if (domain == null)
            {
                domain = new tbl_Domains();
                this.Create(domain);
            }

            domain.DO_CompanyAddress = address;
            domain.DO_CompanyName = name;
            domain.DO_CompanyTelephone = phone;
            domain.DO_ConsumerKey = consumerKey;
            domain.DO_ConsumerSecret = consumerSecret;
            domain.DO_CSS = css;
            domain.DO_DefaultDescription = defaultDesc;
            domain.DO_DefaultKeywords = keywords;
            domain.DO_DefaultLangID = langID;
            domain.DO_DefaultTitle = title;
            domain.DO_Description = desc;
            domain.DO_Domain = domainName ?? String.Empty;
            domain.DO_Email = email;
            domain.DO_GoogleAnalytics = googleAnalytics ?? String.Empty;
            domain.DO_GoogleAnalyticsCode = googleAnalyticsCode;
            domain.DO_GoogleAnalyticsVisible = googleAnalyticsVisible;
            domain.DO_Robots = robots;
            domain.DO_Headline = headline;
            domain.DO_HomePageID = homePageID;
            domain.DO_LaunchYear = launchYear;
            domain.DO_PrimaryDomain = primaryDomain;
            domain.DO_ShareThis = share;
            domain.DO_TwitterSecret = twitterSecret;
            domain.DO_TwitterToken = twitterToken;
            domain.DO_TwitterUser = twitterUser;
            domain.DO_UpdateTwitter = updateTwitter;
            domain.DO_EnableMailChimp = isMailChimpEnabled;
            domain.DO_MailChimpAPIKey = mailChimpAPIKey;
            domain.DO_MailChimpListID = mailChimpListID;
            domain.DO_EnableCommuniGator = isCommuniGatorEnabled;
            domain.DO_CommuniGatorPassword = communiGatorPassword;
            domain.DO_CommuniGatorUserName = communiGatorUser;
            domain.DO_DefaultEventView = (int)eventView;
            domain.DO_EnableEventSale = enableEventSale;
            domain.DO_EnableProductSale = enableProductSale;
            domain.DO_Theme = theme;
            domain.DO_DevelopmentMode = devMode;
            domain.DO_IsCookieConsentEnabled = isCoockieConsentEnabled;
            domain.DO_CustomRouteHandler = customRouteHandler;


            this.Context.SaveChanges();
            return domain;
        }
    }
}

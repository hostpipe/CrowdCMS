using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Logging;
using CMS.DAL.Repository;
using MailChimp;
using MailChimp.Errors;
using MailChimp.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Services
{
    public class MailChimp : ServiceBase, ICRM
    {
        private IDomainsRepository DomainsRepository { get; set; }

        public MailChimp()
            : base()
        {
            this.DomainsRepository = new DomainsRepository(this.Context);
        }

        public bool Subscribe(string email, int domainID)
        {
            tbl_Domains domain = DomainsRepository.GetByID(domainID);
            if (domain == null || !domain.DO_EnableMailChimp)
                return false;

            MailChimpManager manager = new MailChimpManager(domain.DO_MailChimpAPIKey);
            EmailParameter emailParam = new EmailParameter() { Email = email };
            try
            {
                var val = manager.Subscribe(domain.DO_MailChimpListID, emailParam, updateExisting: true);
                return val != null ? true : false;
            }
            catch (MailChimpAPIException e)
            {
                Log.Error(String.Format("MailChimp Subscribe: Subscription exception: \"{0}\" ,for \"{1}\"", e.MailChimpAPIError.Name, email), e);
            }
            return false;
        }

        public bool UnSubscribe(string email, int domainID)
        {
            tbl_Domains domain = DomainsRepository.GetByID(domainID);
            if (domain == null || !domain.DO_EnableMailChimp)
                return false;

            MailChimpManager manager = new MailChimpManager(domain.DO_MailChimpAPIKey);
            EmailParameter emailParam = new EmailParameter() { Email = email };
            try
            {
                var result = manager.Unsubscribe(domain.DO_MailChimpListID, emailParam);
                return result != null ? result.Complete : false;
            }
            catch (MailChimpAPIException e)
            {
                Log.Error(String.Format("MailChimp UnSubscribe: Subscription exception: \"{0}\" ,for \"{1}\"", e.MailChimpAPIError.Name, email), e);
            }
            return false;
        }

        public bool GetSubscriptionStatus(string email, int domainID)
        {
            tbl_Domains domain = DomainsRepository.GetByID(domainID);
            if (domain == null || !domain.DO_EnableMailChimp)
                return false;

            MailChimpManager manager = new MailChimpManager(domain.DO_MailChimpAPIKey);
            EmailParameter emailParam = new EmailParameter() { Email = email };
            try
            {
                var val = manager.GetListsForEmail(emailParam);
                return val != null && val.Count > 0 ? true : false;
            }
            catch (MailChimpAPIException e)
            {
                Log.Error(String.Format("MailChimp Status: Subscription exception: \"{0}\" ,for \"{1}\"", e.MailChimpAPIError.Name, email), e);
            }
            return false;
        }
    }
}

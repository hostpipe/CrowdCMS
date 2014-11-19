using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IDomainLinkRepository
    {
        IQueryable<tbl_DomainLink> GetAll();
        tbl_DomainLink GetByID(int domainLinkID);
        IQueryable<tbl_DomainLink> GetByDomainID(int domainID);
        bool DeleteDomainLink(int domainLinkID);
        bool DeleteDomainLinks(int[] domainLinkIDs);
        tbl_DomainLink SaveDomainLink(int domainID, string link);
    }

    public class DomainLinkRepository : Repository<tbl_DomainLink>, IDomainLinkRepository
    {
        public DomainLinkRepository(IDALContext context) : base(context) { }

        public bool DeleteDomainLink(int domainLinkID)
        {
            this.Delete(d => d.DomainLinkID == domainLinkID);
            this.Context.SaveChanges();
            return true;
        }

        public bool DeleteDomainLinks(int[] domainLinkIDs)
        {
            this.Delete(d => domainLinkIDs.Contains(d.DomainLinkID));
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_DomainLink> GetAll()
        {
            return this.All();
        }

        public tbl_DomainLink GetByID(int domainLinkID)
        {
            return this.DbSet.FirstOrDefault(d => d.DomainLinkID == domainLinkID);
        }

        public IQueryable<tbl_DomainLink> GetByDomainID(int domainID)
        {
            return this.DbSet.Where(d => d.DL_DomainID == domainID);
        }

        public tbl_DomainLink SaveDomainLink(int domainID, string link)
        {
            tbl_DomainLink domainLink = new tbl_DomainLink()
            {
                DL_DomainID = domainID,
                DL_Domain = link
            };

            this.Create(domainLink);
            this.Context.SaveChanges();
            return domainLink;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPortfolioRepository
    {
        bool DeletePortfolio(int portfolioID);
        IQueryable<tbl_Portfolio> GetAll();
        tbl_Portfolio GetByID(int portfolioID);
        tbl_Portfolio SavePortfolio(string title, int portfolioCategoryID, string link, bool showlink, string description, bool feature, string features, bool live, int portfolioID);
        bool SaveOrder(int[] orderedPortfolioIDs);
        tbl_Portfolio SaveVisibility(int portfolioID);
    }

    public class PortfolioRepository : Repository<tbl_Portfolio>, IPortfolioRepository
    {
        public PortfolioRepository(IDALContext context) : base(context) { }

        public bool DeletePortfolio(int portfolioID)
        {
            var portfolio = this.DbSet.FirstOrDefault(pc => pc.PortfolioID == portfolioID);
            if (portfolio == null)
                return false;

            this.Delete(portfolio);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Portfolio> GetAll()
        {
            return this.All().OrderByDescending(po => po.PO_Feature).ThenBy(po => po.PO_Order);
        }

        public tbl_Portfolio GetByID(int portfolioID)
        {
            return this.DbSet.FirstOrDefault(p => p.PortfolioID == portfolioID);
        }

        public tbl_Portfolio SavePortfolio(string title, int portfolioCategoryID, string link, bool showlink, string description, bool feature, string features, bool live, int portfolioID)
        {
            tbl_Portfolio portfolio = this.DbSet.FirstOrDefault(c => c.PortfolioID == portfolioID);
            var order = this.DbSet.OrderByDescending(c => c.PO_Order).Select(c => c.PO_Order).FirstOrDefault();
            if (portfolio == null)
            {
                portfolio = new tbl_Portfolio()
                {
                    PortfolioID = portfolioID,
                    PO_Order = (short)(1 + order)
                };
                this.Create(portfolio);
            }

            portfolio.PO_Live = live;
            portfolio.PO_Title = title;
            portfolio.PO_Desc = description;
            portfolio.PO_Link = link;
            portfolio.PO_ShowLink = showlink;
            portfolio.PO_Feature = feature;
            portfolio.PO_Features = features;
            portfolio.PO_PortfolioCategoryID = portfolioCategoryID;

            this.Context.SaveChanges();
            return portfolio;
        }

        public bool SaveOrder(int[] orderedPortfolioIDs)
        {
            if (orderedPortfolioIDs == null)
                return false;

            for (int i = 0; i < orderedPortfolioIDs.Count(); i++)
            {
                int portfolioID = orderedPortfolioIDs[i];
                tbl_Portfolio portfolio = this.DbSet.FirstOrDefault(po => po.PortfolioID == portfolioID);
                portfolio.PO_Order = (short)(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }

        public tbl_Portfolio SaveVisibility(int portfolioID)
        {
            tbl_Portfolio portfolio = this.DbSet.FirstOrDefault(po => po.PortfolioID == portfolioID);
            if (portfolio == null)
                return null;

            portfolio.PO_Live = !portfolio.PO_Live;
            this.Context.SaveChanges();
            return portfolio;
        }
    }
}

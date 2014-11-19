using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPortfolioCategoryRepository
    {
        bool DeletePortfolioCategory(int categoryID);
        IQueryable<tbl_PortfolioCategory> GetAll();
        tbl_PortfolioCategory GetByID(int categoryID);
        tbl_PortfolioCategory SavePortfolioCategory(string catTitle, int categoryID);
    }

    public class PortfolioCategoryRepository : Repository<tbl_PortfolioCategory>, IPortfolioCategoryRepository
    {
        public PortfolioCategoryRepository(IDALContext context) : base(context) { }

        public bool DeletePortfolioCategory(int categoryID)
        {
            tbl_PortfolioCategory portfolioCategory = this.DbSet.FirstOrDefault(pi => pi.PortfolioCategoryID == categoryID);
            if (portfolioCategory == null)
                return false;

            this.Delete(portfolioCategory);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_PortfolioCategory> GetAll()
        {
            return this.All();
        }

        public tbl_PortfolioCategory GetByID(int categoryID)
        {
            return this.DbSet.FirstOrDefault(pi => pi.PortfolioCategoryID == categoryID);
        }

        public tbl_PortfolioCategory SavePortfolioCategory(string catTitle, int categoryID)
        {
            tbl_PortfolioCategory portfolioCategory = this.DbSet.FirstOrDefault(pi => pi.PortfolioCategoryID == categoryID);
            if (portfolioCategory == null)
            {
                portfolioCategory = new tbl_PortfolioCategory();
                this.Create(portfolioCategory);
            }

            portfolioCategory.POC_Title = catTitle;

            this.Context.SaveChanges();
            return portfolioCategory;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPortfolioImageRepository
    {
        bool DeletePortfolioImage(int portfolioImageID);
        bool DeletePortfolioImages(int portfolioID);
        IQueryable<tbl_PortfolioImage> GetAll();
        IQueryable<tbl_PortfolioImage> GetAll(int portfolioID);
        tbl_PortfolioImage GetByID(int portfolioImageID);
        tbl_PortfolioImage SavePortfolioImage(int portfolioID, int imageID, int portfolioImageID);
        bool SaveOrder(int[] orderedPortfolioImageIDs);
    }

    public class PortfolioImageRepository : Repository<tbl_PortfolioImage>, IPortfolioImageRepository
    {
        public PortfolioImageRepository(IDALContext context) : base(context) { }

        public bool DeletePortfolioImage(int portfolioImageID)
        {
            var portfolioImage = this.DbSet.FirstOrDefault(pi => pi.PortfolioImageID == portfolioImageID);
            if (portfolioImage == null)
                return false;

            this.Delete(portfolioImage);
            this.Context.SaveChanges();
            return true;
        }

        public bool DeletePortfolioImages(int portfolioID)
        {
            IQueryable<tbl_PortfolioImage> portfolioImages = this.DbSet.Where(pi => pi.PI_PortfolioID == portfolioID);
            if (portfolioImages == null)
                return false;

            foreach (tbl_PortfolioImage portfolioImage in portfolioImages)
            {
                this.Delete(portfolioImage);
            }

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_PortfolioImage> GetAll()
        {
            return this.All().OrderBy(pi => pi.PI_Order);
        }

        public IQueryable<tbl_PortfolioImage> GetAll(int portfolioID)
        {
            return this.All().Where(pi => pi.PI_PortfolioID == portfolioID).OrderBy(pi => pi.PI_Order);
        }

        public tbl_PortfolioImage GetByID(int portfolioImageID)
        {
            return this.DbSet.FirstOrDefault(pi => pi.PortfolioImageID == portfolioImageID);
        }

        public tbl_PortfolioImage SavePortfolioImage(int portfolioID, int imageID, int portfolioImageID)
        {
            tbl_PortfolioImage portfolioImage = this.DbSet.FirstOrDefault(pi => pi.PortfolioImageID == portfolioImageID);
            var order = this.DbSet.Where(pi => pi.PI_PortfolioID == portfolioID).OrderByDescending(pi => pi.PI_Order).Select(pi => pi.PI_Order).FirstOrDefault();
            if (portfolioImage == null)
            {
                portfolioImage = new tbl_PortfolioImage()
                {
                    PI_Order = (short)(1 + order)
                };
                this.Create(portfolioImage);
            }

            portfolioImage.PI_PortfolioID = portfolioID;
            portfolioImage.PI_ImageID = imageID;

            this.Context.SaveChanges();
            return portfolioImage;
        }

        public bool SaveOrder(int[] orderedPortfolioImageIDs)
        {
            if (orderedPortfolioImageIDs == null)
                return false;

            for (int i = 0; i < orderedPortfolioImageIDs.Count(); i++)
            {
                int portfolioImageID = orderedPortfolioImageIDs[i];
                tbl_PortfolioImage portfolioImage = this.DbSet.FirstOrDefault(pi => pi.PortfolioImageID == portfolioImageID);
                portfolioImage.PI_Order = (short)(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }

    }
}

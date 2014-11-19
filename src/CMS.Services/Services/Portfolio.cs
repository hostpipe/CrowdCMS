using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.Services
{
    public class Portfolio : ServiceBase, IPortfolio
    {
        private IPortfolioRepository PortfolioRepository { get; set; }
        private IPortfolioImageRepository PortfolioImageRepository { get; set; }
        private IImageRepository ImageRepository { get; set; }
        private IPortfolioCategoryRepository PortfolioCategoryRepository { get; set; }

         public Portfolio()
            : base()
        {
            this.PortfolioRepository = new PortfolioRepository(this.Context);
            this.PortfolioImageRepository = new PortfolioImageRepository(this.Context);
            this.ImageRepository = new ImageRepository(this.Context);
            this.PortfolioCategoryRepository = new PortfolioCategoryRepository(this.Context);
        }

        #region Portfolio

        public bool DeletePortfolio(int portfolioID) //Delete portfolio Images, image link and portfolio
        {
            var portfolioImages = this.PortfolioImageRepository.GetAll(portfolioID);
            if (portfolioImages != null)
            {
                List<int> ImageIDs = portfolioImages.Select(pi => pi.PI_ImageID).ToList();
                this.PortfolioImageRepository.DeletePortfolioImages(portfolioID);
                foreach (int ImageID in ImageIDs)
                {
                    this.ImageRepository.DeleteImage(ImageID);
                }
            }
            return this.PortfolioRepository.DeletePortfolio(portfolioID);
        }


        public List<tbl_Portfolio> GetAll()
        {
            return this.PortfolioRepository.GetAll().ToList();
        }

        public tbl_Portfolio GetByID(int portfolioID)
        {
            return this.PortfolioRepository.GetByID(portfolioID);
        }

        public tbl_Portfolio GetLive()
        {
            return this.PortfolioRepository.GetAll().FirstOrDefault(po => po.PO_Live);
        }

        public tbl_Portfolio SavePortfolio(string title, int portfolioCategoryID, string link, bool showlink, string description, bool feature, string features, bool live, int portfolioID)
        {
            return this.PortfolioRepository.SavePortfolio(title, portfolioCategoryID, link, showlink, description, feature, features, live, portfolioID);
        }

        public bool SaveOrder(int[] orderedPortfolioIDs)
        {
            return this.PortfolioRepository.SaveOrder(orderedPortfolioIDs);
        }

        public tbl_Portfolio SaveVisibility(int portfolioID)
        {
            return this.PortfolioRepository.SaveVisibility(portfolioID);
        }

        public bool DeletePortfolioImage(int portfolioImageID) //Delete portoflio images and image links
        {
            var portfolioImage = this.PortfolioImageRepository.GetByID(portfolioImageID);
            if (portfolioImage == null)
                return false;
            int ImageID = portfolioImage.PI_ImageID;
            return this.PortfolioImageRepository.DeletePortfolioImage(portfolioImageID) && this.ImageRepository.DeleteImage(portfolioImage.PI_ImageID);
        }

        #endregion

        #region Portfolio Image

        public tbl_PortfolioImage GetPortfolioImageByID(int portfolioImageID)
        {
            return PortfolioImageRepository.GetByID(portfolioImageID);
        }

        public tbl_PortfolioImage SavePortfolioImage(int portfolioID, int imageID, int portfolioImageID)
        {
            return PortfolioImageRepository.SavePortfolioImage(portfolioID, imageID, portfolioImageID);
        }

        public bool SavePortfolioImageOrder(int[] orderedPortfolioImageIDs)
        {
            return this.PortfolioImageRepository.SaveOrder(orderedPortfolioImageIDs);
        }

        #endregion

        #region Portfolio Category

        public List<tbl_PortfolioCategory> GetAllPortfolioCategoriesOrdered()
        {
            return PortfolioCategoryRepository.GetAll().OrderBy(pc => pc.POC_Title).ToList();
        }

        public SelectList GetAllPortfolioCategoriesAsSelectList(int selectedportfolioCategoryID)
        {
            return new SelectList(this.GetAllPortfolioCategoriesOrdered(), "PortfolioCategoryID", "POC_Title", selectedportfolioCategoryID);
        }

        public tbl_PortfolioCategory GetPortfolioCategory(int categoryID)
        {
            return PortfolioCategoryRepository.GetByID(categoryID);
        }

        public tbl_PortfolioCategory SavePortfolioCategory(string catTitle, int categoryID)
        {
            return PortfolioCategoryRepository.SavePortfolioCategory(catTitle, categoryID);
        }

        public bool DeletePortfolioCategory(int categoryID)
        {
            return PortfolioCategoryRepository.DeletePortfolioCategory(categoryID);
        }

        #endregion
    }
}

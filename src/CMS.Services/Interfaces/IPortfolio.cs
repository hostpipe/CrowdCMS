using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.Services
{
    public interface IPortfolio
    {
        bool DeletePortfolio(int portfolioID);
        bool DeletePortfolioImage(int portfolioImageID);
        List<tbl_Portfolio> GetAll();
        tbl_Portfolio GetByID(int portfolioID);
        tbl_Portfolio SavePortfolio(string title, int portfolioCategoryID, string link, bool showlink, string description, bool feature, string features, bool live, int portfolioID);
        bool SaveOrder(int[] orderedPortfolioIDs);
        tbl_Portfolio SaveVisibility(int portfolioID);

        tbl_PortfolioImage GetPortfolioImageByID(int portfolioImageID);
        tbl_PortfolioImage SavePortfolioImage(int portfolioID, int imageID, int portfolioImageID);
        bool SavePortfolioImageOrder(int[] orderedPortfolioImageIDs);

        List<tbl_PortfolioCategory> GetAllPortfolioCategoriesOrdered();
        SelectList GetAllPortfolioCategoriesAsSelectList(int selectedportfolioCategoryID);
        tbl_PortfolioCategory GetPortfolioCategory(int categoryID);
        tbl_PortfolioCategory SavePortfolioCategory(string catTitle, int categoryID);
        bool DeletePortfolioCategory(int categoryID);
    }
}

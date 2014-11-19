using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Repository;
using CMS.Services.Model;
using CMS.Utils.Extension;
using System.Web.Mvc;

namespace CMS.Services
{
    public interface IWebContent
    {
        int BlogAmountInCategory(int categoryID);
        bool DeleteCategory(int categoryID);
        List<SelectListItem> GetAllCategories(int sectionID);
        List<tbl_Categories> GetAllCategoriesLive();
        List<tbl_Categories> GetAllCategoriesLive(int domainID);
        tbl_Categories GetCategoryByID(int categoryID);
        tbl_Categories GetCategoryByURL(string url);
        tbl_Categories SaveCategory(string title, int categoryID);

        tbl_Comments AuthoriseComment(int commentID);
        bool DeleteComment(int commentID);
        List<tbl_Comments> GetNewsComments(int sitemapID);
        tbl_Comments SaveComment(string name, string email, string website, string message, int sitemapID, int commentID);

        tbl_Content ApproveContent(int sectionID, int contentID = 0);
        bool DeleteContent(int contentID);
        bool IsContentApproved(int contentID);
        bool IsAnyContentApproved(int sitemapID);
        List<tbl_Content> GetAllContents();
        tbl_Content GetContentByID(int contentID);
        List<tbl_Content> GetContentByCategoryUrl(string url, int domainID);
        List<tbl_Content> GetContentByTagUrl(string url, int domainID);
        List<tbl_Content> GetContentByContentType(ContentType contentType, int domainID);
        List<tbl_Content> GetContentByContentType(ContentType contentType, int domainID, int top);
        List<tbl_Content> GetContentBySitemapDate(string year, string month, int domainID);
                tbl_Content GetContentBySitemapID(int sectionID, int contentID = 0);
        tbl_Content GetContentBySitemapUrl(string url, int domainID);
        tbl_Content GetContentBySitemapUrl(string url, ContentType type, int domainID);
        tbl_Content GetContentBySitemapUrl(string url, int domainID, int sectionID, int contentID = 0, bool isAdmin = false, int homepageID = 0);
        tbl_Content GetContentByRedirectUrl(string url, int domainID, int contentID = 0);
        tbl_Content SaveContent(string asKeys, string sContent, string desc, int galleryID, string headLine, string keywords, string menuText,
            string metaData, int newsID, string paContent, string pasKeys, string sKeys, string title, string tweet, bool tweeter, int siteMapID, int contentID);
        List<tbl_Content> SearchContent(string keyword, int domainID);

        List<tbl_ContentType> GetAllContentTypes();
        tbl_ContentType GetContentTypeByType(ContentType type);
        List<tbl_SiteMap> GetSitemaps(System.Linq.Expressions.Expression<Func<tbl_SiteMap, bool>> predicate);

        bool AddCategoryToSitemap(int sitemapID, int categoryID);
        bool CheckSitemapUniqueUrl(string url, int currentSitemapID, int domainID);
        bool DeleteSection(int sitemapID);
        List<tbl_SiteMap> GetAllSitemaps();
        tbl_SiteMap GetSitemapByID(int sitemapID);
        List<tbl_SiteMap> GetSitemapByCategoryID(int categoryID, int domainID);
        tbl_SiteMap GetSitemapByContentID(int contentID);
        List<tbl_SiteMap> GetSitemapByContentType(ContentType contentType, int domainID);
        List<ExtendedSelectListItem> GetSitemapListByContent(int domainID, int selectedSitemapID);
        List<ExtendedSelectListItem> GetSitemapListURLByContent(int domainID, string selectedUrl);
        List<tbl_SiteMap> GetSitemapByDate(DateTime date, int domainID);
        List<tbl_SiteMap> GetSitemapByDomainID(int domainID);
        List<tbl_SiteMap> GetSitemapByTag(string tag, int domainID);
        List<tbl_SiteMap> GetSitemapByUrl(string url, int domainID);
        List<tbl_SiteMap> GetSitemapByParentID(int domainID, int parentID);
        int GetSitemapRootID(int domainID, int sitemapID);
        tbl_SiteMap GetSitemapByType(SiteMapType type, int domainID);
        string GetSitemapUrlByType(SiteMapType type, int domainID);
        string GetSitemapParentUrlByID(int sitemapID);
        bool SaveCategoriesForSitemap(int sitemapID, int[] categoryIDs);
        bool SaveSitemapsOrder(int[] orderedSiteMapItemIDs);
        tbl_SiteMap SaveSiteMap(string R301, int languageID, int menuID, int domainID, string css, bool isMenu, bool isFooter, string menuText, DateTime? date,
            string spriority, string notifyEmail, string path, bool requiresApproval, bool isSiteMap, ContentType type, string siteMapName, int parentID, int siteMapID,
            int? typeID = null, bool isPredefined = false, MenuDisplayType menuDisplayType = MenuDisplayType.UnderParent, DateTime? publishDate = null);
        tbl_SiteMap SaveSitemapVisibility(int sitemapID);
        void UpdateSitemapsParents(int shopSitemapID, ProductType type);
        void UpdateSiteMapCustomLayout(int siteMapID,  int customLayoutID);

        List<tbl_Tags> GetAllTags();
        tbl_Tags GetTagByID(int tagID);
        List<SelectListItem> GetTagsBySectionID(int sectionID);
        tbl_Tags GetTagByURL(string url);
        tbl_Tags SaveTag(string title, int sitemapID);
        bool DeleteTag(int tagID);
        List<TagCountModel> GetUniqueTags(int domainId);
    }
}

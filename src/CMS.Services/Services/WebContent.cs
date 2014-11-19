using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;

using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Repository;
using CMS.Services.Model;
using CMS.Utils.Extension;
using Microsoft.Security.Application;
using System.Text.RegularExpressions;
using CMS.Utils;


namespace CMS.Services
{
    public class WebContent : ServiceBase, IWebContent
    {
        private ICategoriesRepository CategoriesRepository { get; set; }
        private ICommentRepository CommentRepository { get; set; }
        private IContentRepository ContentRepository { get; set; }
        private IContentTypeRepository ContentTypeRepository { get; set; }
        private ISitemapRepository SitemapRepository { get; set; }
        private ITagsRepository TagsRepository { get; set; }


        public WebContent()
            : base()
        {
            this.CategoriesRepository = new CategoriesRepository(this.Context);
            this.CommentRepository = new CommentRepository(this.Context);
            this.ContentRepository = new ContentRepository(this.Context);
            this.ContentTypeRepository = new ContentTypeRepository(this.Context);
            this.SitemapRepository = new SitemapRepository(this.Context);
            this.TagsRepository = new TagsRepository(this.Context);

        }


        #region Categories repo

        public int BlogAmountInCategory(int categoryID)
        {
            var category = CategoriesRepository.GetByID(categoryID);
            if (category == null)
                return 0;

            return category.tbl_NewsCategories.Where(n => !n.tbl_SiteMap.SM_Deleted).Count();
        }

        public bool DeleteCategory(int categoryID)
        {
            return CategoriesRepository.DeleteCategory(categoryID);
        }

        public List<SelectListItem> GetAllCategories(int sectionID)
        {
            return CategoriesRepository.GetAll().Select(c =>
                new SelectListItem
                {
                    Text = c.CA_Title,
                    Value = SqlFunctions.StringConvert((double)c.CategoryID).Trim(),
                    Selected = c.tbl_NewsCategories.Any(nc => nc.NC_SiteMapID == sectionID)
                }).ToList();
        }

        public List<tbl_Categories> GetAllCategoriesLive()
        {
            return CategoriesRepository.GetAll().Where(c => c.tbl_NewsCategories.Any(nc => !nc.tbl_SiteMap.SM_Deleted && nc.tbl_SiteMap.SM_Live)).ToList();
        }

        public List<tbl_Categories> GetAllCategoriesLive(int domainID)
        {
            return CategoriesRepository.GetAll().Where(c => c.tbl_NewsCategories.Any(nc => nc.tbl_SiteMap.SM_DomainID == domainID && !nc.tbl_SiteMap.SM_Deleted && nc.tbl_SiteMap.SM_Live)).ToList();
        }

        public tbl_Categories GetCategoryByID(int categoryID)
        {
            return CategoriesRepository.GetByID(categoryID);
        }

        public tbl_Categories GetCategoryByURL(string url)
        {
            return CategoriesRepository.GetByURL(url);
        }

        public tbl_Categories SaveCategory(string title, int categoryID)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            return CategoriesRepository.SaveCategory(title, title.Replace(' ', '-').ToLower(), categoryID);
        }

        #endregion


        #region Comments repo

        public tbl_Comments AuthoriseComment(int commentID)
        {
            return CommentRepository.AuthoriseComment(commentID);
        }

        public bool DeleteComment(int commentID)
        {
            return CommentRepository.DeleteComment(commentID);
        }

        public List<tbl_Comments> GetNewsComments(int sitemapID)
        {
            return CommentRepository.GetNewsComments(sitemapID).OrderBy(c => c.CO_Authorised).ThenByDescending(c => c.CO_Date).ToList();
        }

        public tbl_Comments SaveComment(string name, string email, string website, string message, int sitemapID, int commentID)
        {
            message = ReplaceStringWithLink(message);
            if (!String.IsNullOrEmpty(website) && !website.StartsWith("http://"))
                website = "http://" + website;

            return CommentRepository.SaveComment(name, email, website, message, sitemapID, commentID);
        }

        #endregion


        #region Content repo

        public tbl_Content ApproveContent(int sectionID, int contentID = 0)
        {
            if (contentID == 0)
            {
                tbl_SiteMap section = SitemapRepository.GetByID(sectionID);
                if (section != null)
                    contentID = section.tbl_Content.Where(c => !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).Select(c => c.ContentID).FirstOrDefault();
            }
            return ContentRepository.ApproveContent(contentID);
        }

        public bool DeleteContent(int contentID)
        {
            return ContentRepository.DeleteContent(contentID);
        }

        public bool IsAnyContentApproved(int sitemapID)
        {
            return ContentRepository.IsAnyContentApproved(sitemapID);
        }

        public bool IsContentApproved(int contentID)
        {
            return ContentRepository.IsContentApproved(contentID);
        }

        public List<tbl_Content> GetAllContents()
        {
            return ContentRepository.GetAll().ToList();
        }

        public tbl_Content GetContentByID(int contentID)
        {
            return ContentRepository.GetByID(contentID);
        }

        public List<tbl_Content> GetContentByCategoryUrl(string url, int domainID)
        {
            tbl_Categories category = CategoriesRepository.GetByURL(url);
            if (category == null)
                return null;

            var sitemaps = SitemapRepository.GetByCategoryID(category.CategoryID, domainID).Where(c => c.SM_Live).OrderByDescending(b => b.SM_Date);
            if (sitemaps == null || sitemaps.Count() == 0)
                return null;

            return sitemaps.Select(s => s.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault()).Where(b => b != null).ToList();
        }

        public List<tbl_Content> GetContentByTagUrl(string url, int domainID)
        {
            var sitemaps = SitemapRepository.GetByTag(url, domainID).Where(c => c.SM_Live).OrderByDescending(b => b.SM_Date);
            if (sitemaps == null || sitemaps.Count() == 0)
                return null;

            return sitemaps.Select(s => s.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault()).Where(b => b != null).ToList();
        }

        public List<tbl_Content> GetContentByContentType(ContentType contentType, int domainID)
        {
            tbl_ContentType type = ContentTypeRepository.GetByType(contentType);
            if (type == null)
                return null;

            var sitemaps = SitemapRepository.GetByContentTypeID(type.ContentTypeID, domainID)
                .Where(c => (contentType != ContentType.Blog || (c.SM_Live && (c.SM_PublishDate == null || c.SM_PublishDate <= DateTime.Now))))
                .OrderByDescending(b => b.SM_Date);
            if (sitemaps == null || sitemaps.Count() == 0)
                return null;

            return sitemaps.Select(s => s.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault()).Where(b => b != null).ToList();
        }

        public List<tbl_Content> GetContentByContentType(ContentType contentType, int domainID, int top)
        {
            tbl_ContentType type = ContentTypeRepository.GetByType(contentType);
            if (type == null)
                return null;

            var sitemaps = SitemapRepository.GetByContentTypeID(type.ContentTypeID, domainID)
                .Where(c => (contentType != ContentType.Blog || (c.SM_Live && (c.SM_PublishDate == null || c.SM_PublishDate <= DateTime.Now))))
                .OrderByDescending(b => b.SM_Date).Take(top);
            if (sitemaps == null || sitemaps.Count() == 0)
                return null;

            return sitemaps.Select(s => s.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault()).Where(b => b != null).ToList();
        }

        public List<tbl_Content> GetContentBySitemapDate(string year, string month, int domainID)
        {
            int y = 0, m = 0;
            int.TryParse(year, out y);
            int.TryParse(month, out m);
            var date = new DateTime(y, m, 1);

            var siteMaps = SitemapRepository.GetByDate(date, domainID).Where(b => b.SM_Live && (b.SM_PublishDate == null || b.SM_PublishDate <= DateTime.Now)).OrderByDescending(b => b.SM_Date);
            if (siteMaps == null || siteMaps.Count() == 0)
                return null;

            return siteMaps.Select(s => s.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault()).Where(b => b != null).ToList();
        }

        public tbl_Content GetContentBySitemapID(int sectionID, int contentID = 0)
        {
            tbl_Content content;
            var siteMap = SitemapRepository.GetByID(sectionID);
            if (siteMap == null)
                return null;

            if (contentID == 0)
            {
                content = siteMap.tbl_Content.FirstOrDefault(c => !c.C_Deleted && c.C_Approved);
                if (content == null)
                {
                    content = siteMap.tbl_Content.OrderByDescending(c => c.C_ModificationDate).FirstOrDefault(c => !c.C_Deleted);
                }
            }
            else
                content = siteMap.tbl_Content.FirstOrDefault(c => c.ContentID == contentID);

            return content;
        }

        public tbl_Content GetContentBySitemapUrl(string url, int domainID)
        {
            var siteMap = SitemapRepository.GetByUrl(url, domainID).Where(b => b.SM_PublishDate == null || b.SM_PublishDate <= DateTime.Now).FirstOrDefault();
            return (siteMap == null || (siteMap.IsType(ContentType.Blog) && !siteMap.SM_Live)) ?
                 null:
                 siteMap.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault();
        }

        public tbl_Content GetContentBySitemapUrl(string url, ContentType type, int domainID)
        {
            var sType = type.ToString();
            tbl_SiteMap siteMap = SitemapRepository.GetByUrl(url, domainID).Where(sm => sm.tbl_ContentType.CTP_Value.Equals(sType)).FirstOrDefault();
            return (siteMap == null || (siteMap.IsType(ContentType.Blog) && !siteMap.SM_Live)) ?
                 null :
                 siteMap.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault();
        }

        public tbl_Content GetContentBySitemapUrl(string url, int domainID, int sectionID, int contentID = 0, bool isAdmin = false, int homepageID = 0)
        {
            tbl_SiteMap section = null;
            bool isPreview = false;

            if (String.IsNullOrEmpty(url) || url == "/" || (url.ToLowerInvariant() == "/preview" && isAdmin))
            {
                section = SitemapRepository.GetByID(sectionID);
                isPreview = true;
            }
            else
                section = SitemapRepository.GetByUrl(url, domainID).FirstOrDefault();

            if (section == null || (section.IsType(ContentType.Blog) && !section.SM_Live && !isPreview) || (section.SiteMapID == homepageID && url != "/" && !isPreview))
                return null;

            return (contentID == 0) ?
                section.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault():
                section.tbl_Content.FirstOrDefault(c => c.ContentID == contentID);
        }

        public tbl_Content GetContentByRedirectUrl(string url, int domainID, int contentID = 0)
        {
            tbl_SiteMap section = SitemapRepository.GetByRedirectUrl(url, domainID);

            if (section == null || (section.IsType(ContentType.Blog) && !section.SM_Live))
                return null;

            return (contentID == 0) ?
                section.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault():
                section.tbl_Content.FirstOrDefault(c => c.ContentID == contentID);
        }

        public tbl_Content SaveContent(string asKeys, string sContent, string desc, int galleryID, string headLine, string keywords, string menuText,
            string metaData, int newsID, string paContent, string pasKeys, string sKeys, string title, string tweet, bool tweeter, int siteMapID, int contentID)
        {
            return ContentRepository.SaveContent(Sanitizer.GetSafeHtmlFragment(asKeys), sContent, Sanitizer.GetSafeHtmlFragment(desc), galleryID,
                Sanitizer.GetSafeHtmlFragment(headLine), Sanitizer.GetSafeHtmlFragment(keywords), Sanitizer.GetSafeHtmlFragment(menuText), metaData,
                newsID, Sanitizer.GetSafeHtmlFragment(paContent), Sanitizer.GetSafeHtmlFragment(pasKeys), Sanitizer.GetSafeHtmlFragment(sKeys), Sanitizer.GetSafeHtmlFragment(title),
                Sanitizer.GetSafeHtmlFragment(tweet), tweeter, siteMapID, contentID);
        }

        public List<tbl_Content> SearchContent(string keyword, int domainID)
        {
            tbl_ContentType textType = ContentTypeRepository.GetByType(ContentType.Content);
            tbl_ContentType blogType = ContentTypeRepository.GetByType(ContentType.Blog);
            if (textType == null || blogType == null)
                return null;

            IQueryable<tbl_SiteMap> texts = SitemapRepository.GetByDomainID(domainID)
                .Where(c => (c.SM_ContentTypeID == blogType.ContentTypeID && c.SM_Live) ||
                    c.SM_ContentTypeID == textType.ContentTypeID).OrderByDescending(b => b.SM_Date);
            return texts.Select(b => b.tbl_Content.Where(c => c.C_Approved && !c.C_Deleted).OrderByDescending(c => c.C_ModificationDate).FirstOrDefault())
                .Where(c => c != null && (c.C_Title.Contains(keyword) || 
                    c.C_Content.Contains(keyword) || 
                    c.C_Description.Contains(keyword) || 
                    c.C_Keywords.Contains(keyword) ||
                    c.C_SearchKeywords.Contains(keyword))).ToList();
        }

        #endregion


        #region Content Types repo

        public List<tbl_ContentType> GetAllContentTypes()
        {
            return ContentTypeRepository.GetAll().ToList();
        }

        public tbl_ContentType GetContentTypeByType(ContentType type)
        {
            return ContentTypeRepository.GetByType(type);
        }

        #endregion


        #region Sitemap repo

        public bool AddCategoryToSitemap(int sitemapID, int categoryID)
        {
            if (sitemapID == 0 || categoryID == 0)
                return false;

            return SitemapRepository.AddCategory(sitemapID, categoryID);
        }

        public bool CheckSitemapUniqueUrl(string url, int currentSitemapID, int domainID)
        {
            url = FriendlyUrl.CreateFriendlyUrl(url);
            return SitemapRepository.CheckUniqueUrl(url.TrimEnd('/'), currentSitemapID, domainID);
        }

        public bool DeleteSection(int sitemapID)
        {
            return SitemapRepository.DeleteSection(sitemapID);
        }

        public List<tbl_SiteMap> GetAllSitemaps()
        {
            return SitemapRepository.GetAll().ToList();
        }

        public tbl_SiteMap GetSitemapByID(int sitemapID)
        {
            return SitemapRepository.GetByID(sitemapID);
        }

        public List<tbl_SiteMap> GetSitemapByCategoryID(int categoryID, int domainID)
        {
            return SitemapRepository.GetByCategoryID(categoryID, domainID).ToList();
        }

        public tbl_SiteMap GetSitemapByContentID(int contentID)
        {
            return SitemapRepository.GetByContentID(contentID);
        }

        public List<tbl_SiteMap> GetSitemapByContentType(ContentType contentType, int domainID)
        {
            tbl_ContentType type = ContentTypeRepository.GetByType(contentType);
            if (type == null)
                return new List<tbl_SiteMap>();

            var sitemaps = SitemapRepository.GetByContentTypeID(type.ContentTypeID, domainID).ToList();

            switch (contentType)
            {
                case ContentType.Product:
                    sitemaps = sitemaps.Where(s => s.tbl_Products != null).ToList();
                    break;
                case ContentType.Category:
                    sitemaps = sitemaps.Where(s => s.tbl_ProdCategories != null).ToList();
                    break;
            }

            return sitemaps;
        }

        public List<ExtendedSelectListItem> GetSitemapListByContent(int domainID, int selectedSitemapID)
        {
            tbl_ContentType type = ContentTypeRepository.GetByType(ContentType.Content);
            if (type == null)
                return null;

            List<ExtendedSelectListItem> sections = new List<ExtendedSelectListItem>();
            foreach (var section in SitemapRepository.GetByContentTypeID(type.ContentTypeID, domainID).OrderBy(c => c.SM_OrderBy))
            {
                var parent = sections.FirstOrDefault(s => s.Value == section.SM_ParentID.ToString());
                if (sections.Contains(parent))
                    sections.Insert(sections.IndexOf(parent) + 1, new ExtendedSelectListItem
                    {
                        Text = section.SM_Name,
                        Value = section.SiteMapID.ToString(),
                        Selected = section.SiteMapID == selectedSitemapID,
                        Level = parent != null ? parent.Level + 1 : 1
                    });
                else
                    sections.Add(new ExtendedSelectListItem
                    {
                        Text = section.SM_Name,
                        Value = section.SiteMapID.ToString(),
                        Selected = section.SiteMapID == selectedSitemapID,
                        Level = parent != null ? parent.Level + 1 : 1
                    });
            }
            return sections;
        }

        public List<ExtendedSelectListItem> GetSitemapListURLByContent(int domainID, string selectedUrl)
        {
            tbl_ContentType type = ContentTypeRepository.GetByType(ContentType.Content);
            if (type == null)
                return null;

            List<ExtendedSelectListItem> sections = new List<ExtendedSelectListItem>();
            foreach (var section in SitemapRepository.GetByContentTypeID(type.ContentTypeID, domainID).OrderBy(c => c.SM_OrderBy))
            {
                var parent = sections.FirstOrDefault(s => s.ID == section.SM_ParentID);
                if (sections.Contains(parent))
                {
                    string appendix = String.Empty;
                    int level = parent.Level;
                    while (level > 0) { appendix += "&nbsp;&nbsp;"; level--; }

                    sections.Insert(sections.IndexOf(parent) + 1, new ExtendedSelectListItem
                    {
                        Text = appendix + section.SM_Name,
                        Value = section.SM_URL,
                        Selected = section.SM_URL == selectedUrl,
                        Level = parent.Level + 1,
                        ID = section.SiteMapID
                    });
                }
                else
                    sections.Add(new ExtendedSelectListItem
                    {
                        Text = section.SM_Name,
                        Value = section.SM_URL,
                        Selected = section.SM_URL == selectedUrl,
                        Level = 1,
                        ID = section.SiteMapID
                    });
            }
            return sections;
        }

        public List<tbl_SiteMap> GetSitemapByDate(DateTime date, int domainID)
        {
            return SitemapRepository.GetByDate(date, domainID).ToList();
        }

        public List<tbl_SiteMap> GetSitemapByDomainID(int domainID)
        {
            return SitemapRepository.GetByDomainID(domainID).ToList();
        }

        public List<tbl_SiteMap> GetSitemapByTag(string tag, int domainID)
        {
            return SitemapRepository.GetByTag(tag, domainID).ToList();
        }

        public List<tbl_SiteMap> GetSitemapByUrl(string url, int domainID)
        {
            return SitemapRepository.GetByUrl(url.TrimEnd('/'), domainID).ToList();
        }

        public List<tbl_SiteMap> GetSitemapByParentID(int domainID, int parentID)
        {
            return SitemapRepository.GetByDomainID(domainID).Where(sm => sm.SM_ParentID == parentID).OrderBy(sm => sm.SM_OrderBy).ToList();
        }

        public int GetSitemapRootID(int domainID, int sitemapID)
        {
            tbl_SiteMap sitemap = SitemapRepository.GetByID(sitemapID);
            if (sitemap == null)
                return 0;

            if (sitemap.SM_ParentID == 0)
            {
                return sitemap.SiteMapID;
            }
            else
            {
                return this.GetSitemapRootID(domainID, sitemap.SM_ParentID);
            }
        }

        public tbl_SiteMap GetSitemapByType(SiteMapType type, int domainID)
        {
            return SitemapRepository.GetByType((int)type, domainID);
        }

        public string GetSitemapUrlByType(SiteMapType type, int domainID)
        {
            return SitemapRepository.GetUrlByType((int)type, domainID) ?? type.ToString();
        }

        public string GetSitemapParentUrlByID(int sitemapID)
        {
            return SitemapRepository.GetParentUrlByID(sitemapID);
        }

        public List<tbl_SiteMap> GetSitemaps(System.Linq.Expressions.Expression<Func<tbl_SiteMap, bool>> predicate)
        {
            return SitemapRepository.GetSitemaps(predicate).ToList();
        }

        public bool SaveCategoriesForSitemap(int sitemapID, int[] categoryIDs)
        {
            return (sitemapID == 0) ?
                false:
                SitemapRepository.SaveCategories(sitemapID, categoryIDs);
        }

        public bool SaveSitemapsOrder(int[] orderedSiteMapItemIDs)
        {
            return SitemapRepository.SaveOrder(orderedSiteMapItemIDs);
        }

        public tbl_SiteMap SaveSiteMap(string R301, int languageID, int menuID, int domainID, string css, bool isMenu, bool isFooter, string menuText, DateTime? date,
            string spriority, string notifyEmail, string path, bool requiresApproval, bool isSiteMap, ContentType type, string siteMapName, int parentID, int siteMapID,
            int? typeID = null, bool isPredefined = false, MenuDisplayType menuDisplayType = MenuDisplayType.UnderParent, DateTime? publishDate = null)
        {
            decimal priority = 0;
            Decimal.TryParse(spriority, out priority);

            if (siteMapID > 0)
            {
                var sitemap = SitemapRepository.GetByID(siteMapID);
                if (sitemap != null && sitemap.SM_IsPredefined)
                {
                    isPredefined = true;
                    typeID = sitemap.SM_TypeID;
                }
            }

            return SitemapRepository.SaveSiteMap(R301, languageID, menuID, domainID, Sanitizer.GetSafeHtmlFragment(css), isMenu, isFooter,
                Sanitizer.GetSafeHtmlFragment(menuText), date, priority, Sanitizer.GetSafeHtmlFragment(notifyEmail), FriendlyUrl.CreateFriendlyUrl(path),
                requiresApproval, isSiteMap, type, Sanitizer.GetSafeHtmlFragment(siteMapName), parentID, siteMapID, typeID, isPredefined, (int)menuDisplayType, publishDate);
        }

        public tbl_SiteMap SaveSitemapVisibility(int sitemapID)
        {
            return SitemapRepository.SaveVisibility(sitemapID);
        }

        public void UpdateSitemapsParents(int shopSitemapID, ProductType type)
        {
            SitemapRepository.UpdateParents(shopSitemapID, type);
        }

        public void UpdateSiteMapCustomLayout(int siteMapID,  int customLayoutID)
        {
            SitemapRepository.UpdateCustomLayout(siteMapID, customLayoutID);
        }

        #endregion


        #region Tags repo

        public List<tbl_Tags> GetAllTags()
        {
            return TagsRepository.GetAll().ToList();
        }

        public tbl_Tags GetTagByID(int tagID)
        {
            return TagsRepository.GetByID(tagID);
        }

        public List<SelectListItem> GetTagsBySectionID(int sectionID)
        {
            return TagsRepository.GetBySectionID(sectionID).Select(t => new SelectListItem {
                Text = t.TA_Title, 
                Value = SqlFunctions.StringConvert((double)t.TagID).Trim() 
            }).ToList();
        }

        public tbl_Tags GetTagByURL(string url)
        {
            return TagsRepository.GetByURL(url);
        }

        public tbl_Tags SaveTag(string title, int sitemapID)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            return TagsRepository.SaveTag(title, title.Replace(' ', '-').ToLower(), sitemapID);
        }

        public bool DeleteTag(int tagID)
        {
            return TagsRepository.DeleteTag(tagID);
        }

        /// <summary>
        /// Gets a collection of unique tags and a count, ordered by the most frequently occurring.
        /// </summary>
        /// <returns>
        /// The method returns an instance of TagsRepository.TagsCount class
        /// </returns>
        public List<TagCountModel> GetUniqueTags(int domainId)
        {
            var tagsList =
                TagsRepository.GetAll()
                    .Where(
                        t =>
                            !t.tbl_SiteMap.SM_Deleted && t.tbl_SiteMap.SM_DomainID == domainId && t.tbl_SiteMap.SM_Live &&
                            (t.tbl_SiteMap.SM_PublishDate == null || t.tbl_SiteMap.SM_PublishDate <= DateTime.Now))
                    .GroupBy(g => new {g.TA_Title, g.TA_URL})
                    .Select(ta => new TagCountModel { Title = ta.Key.TA_Title, Url = ta.Key.TA_URL, Count = ta.Count() })
                    .OrderByDescending(o => o.Count).ToList();
            return tagsList;
        }

        #endregion


        #region Private Methodes

        private string ReplaceStringWithLink(string message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                MatchCollection matches = Regex.Matches(message, @"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
                foreach (Match match in matches)
                {
                    string tag = String.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", match.Value, match.Value);
                    message = message.Replace(match.Value, tag);
                }
            }

            return message;
        }

        #endregion
    }
}

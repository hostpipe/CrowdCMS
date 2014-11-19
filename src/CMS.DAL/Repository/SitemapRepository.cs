using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using System.Data.Entity;
using CMS.Utils;

namespace CMS.DAL.Repository
{
    public interface ISitemapRepository
    {
        bool AddCategory(int sitemapID, int categoryID);
        bool CheckUniqueUrl(string url, int currentSitemapID, int domainID);
        bool DeleteSection(int sectionID);
        IQueryable<tbl_SiteMap> GetAll();
        tbl_SiteMap GetByID(int sitemapID);
        IQueryable<tbl_SiteMap> GetByCategoryID(int categoryID, int domainID);
        tbl_SiteMap GetByContentID(int contentID);
        IQueryable<tbl_SiteMap> GetByContentTypeID(int contentTypeID, int domainID);
        IQueryable<tbl_SiteMap> GetByDate(DateTime date, int domainID);
        IQueryable<tbl_SiteMap> GetByDomainID(int domainID);
        IQueryable<tbl_SiteMap> GetByTag(string tagUrl, int domainID);
        IQueryable<tbl_SiteMap> GetByUrl(string url, int domainID);
        tbl_SiteMap GetByRedirectUrl(string url, int domainID);
        tbl_SiteMap GetByType(int typeID, int domainID);
        string GetUrlByType(int typeID, int domainID);
        string GetParentUrlByID(int sitemapID);
        IQueryable<tbl_SiteMap> GetSitemaps(System.Linq.Expressions.Expression<Func<tbl_SiteMap, bool>> predicate);
        bool SaveCategories(int sitemapID, int[] categoryIDs);
        bool SaveOrder(int[] orderedSiteMapItemIDs);
        tbl_SiteMap SaveSiteMap(string R301, int languageID, int menuID, int domainID, string css, bool isMenu, bool isFooter, string menuText, DateTime? date,
            decimal priority, string notifyEmail, string path, bool requiresApproval, bool isSiteMap, ContentType type, string siteMapName, int parentID, int siteMapID,
            int? typeID = null, bool isPredefined = false, int menuDisplayTypeID = 1, DateTime? publishDate = null);
        tbl_SiteMap SaveVisibility(int sitemapID);
        void UpdateParents(int shopSitemapID, ProductType type);
        void UpdateCustomLayout(int siteMapID, int customLayouID);

        IQueryable<tbl_SiteMapTypes> GetAllTypes();
    }

    public class SitemapRepository : Repository<tbl_SiteMap>, ISitemapRepository
    {
        public SitemapRepository(IDALContext context) : base(context) { }

        public bool AddCategory(int sitemapID, int categoryID)
        {
            var sitemap = this.DbSet.FirstOrDefault(sm => sm.SiteMapID == sitemapID);
            if (sitemap == null)
                return false;

            if (!sitemap.tbl_NewsCategories.Select(nc => nc.NC_CategoryID).Contains(categoryID))
            {
                sitemap.tbl_NewsCategories.Add(new tbl_NewsCategories { NC_CategoryID = categoryID, NC_SiteMapID = sitemapID });
                this.Context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool CheckUniqueUrl(string url, int currentSitemapID, int domainID)
        {
            return this.DbSet.Where(s => s.SM_DomainID == domainID && !s.SM_Deleted && s.SiteMapID != currentSitemapID).Any(s => s.SM_URL == url);
        }

        public bool DeleteSection(int sectionID)
        {
            var section = DbSet.FirstOrDefault(c => c.SiteMapID == sectionID);
            if (section == null)
                return false;

            foreach (var content in section.tbl_Content)
            {
                content.C_Deleted = true;
            }

            section.SM_Deleted = true;
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_SiteMap> GetAll()
        {
            return this.All();
        }

        public tbl_SiteMap GetByID(int sitemapID)
        {
            return this.DbSet.FirstOrDefault(s => s.SiteMapID == sitemapID);
        }

        public IQueryable<tbl_SiteMap> GetByCategoryID(int categoryID, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.tbl_NewsCategories.Any(c => c.tbl_Categories.CategoryID == categoryID) && s.SM_DomainID == domainID);
        }

        public tbl_SiteMap GetByContentID(int contentID)
        {
            return this.DbSet.FirstOrDefault(s => !s.SM_Deleted && s.tbl_Content.Select(c => c.ContentID).Contains(contentID));
        }

        public IQueryable<tbl_SiteMap> GetByContentTypeID(int contentTypeID, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.SM_ContentTypeID == contentTypeID && s.SM_DomainID == domainID);
        }

        public IQueryable<tbl_SiteMap> GetByDate(DateTime date, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.SM_Date.HasValue && s.SM_Date.Value.Year == date.Year && s.SM_Date.Value.Month == date.Month && s.SM_DomainID == domainID);
        }

        public IQueryable<tbl_SiteMap> GetByDomainID(int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.SM_DomainID == domainID);
        }

        public IQueryable<tbl_SiteMap> GetByTag(string tagUrl, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.tbl_Tags.Any(t => t.TA_URL.Equals(tagUrl)) && s.SM_DomainID == domainID);
        }

        public IQueryable<tbl_SiteMap> GetByUrl(string url, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.SM_DomainID == domainID && s.SM_URL.Equals(url));
        }
        
        public tbl_SiteMap GetByRedirectUrl(string url, int domainID)
        {
            return this.DbSet.FirstOrDefault(s => !s.SM_Deleted && s.SM_DomainID == domainID && s.SM_301.Contains(url));
        }

        public tbl_SiteMap GetByType(int typeID, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.SM_DomainID == domainID && s.SM_TypeID == typeID && s.SM_ParentID == 0 && s.SM_IsPredefined).FirstOrDefault();
        }

        public string GetUrlByType(int typeID, int domainID)
        {
            return this.DbSet.Where(s => !s.SM_Deleted && s.SM_DomainID == domainID && s.SM_TypeID == typeID && s.SM_ParentID == 0 && s.SM_IsPredefined).Select(s => s.SM_URL).OrderBy(s => s.Length).FirstOrDefault();
        }

        public string GetParentUrlByID(int sitemapID)
        {
            var section = this.DbSet.FirstOrDefault(s => s.SiteMapID == sitemapID);
            return section != null ? section.SM_URL + "/" : "/";
        }

        public IQueryable<tbl_SiteMap> GetSitemaps(System.Linq.Expressions.Expression<Func<tbl_SiteMap, bool>> predicate)
        {
            return this.DbSet.Where(predicate);
        }

        public bool SaveCategories(int sitemapID, int[] categoryIDs)
        {
            var sitemap = this.DbSet.FirstOrDefault(sm => sm.SiteMapID == sitemapID);
            if (sitemap == null)
                return false;

            foreach (var category in sitemap.tbl_NewsCategories.ToList())
            {
                this.Context.Set<tbl_NewsCategories>().Remove(category);
            }
            this.Context.SaveChanges();

            if (categoryIDs != null)
            {
                foreach (var id in categoryIDs)
                {
                    sitemap.tbl_NewsCategories.Add(new tbl_NewsCategories { NC_CategoryID = id, NC_SiteMapID = sitemapID });
                }
            }

            this.Context.SaveChanges();
            return true;
        }

        public bool SaveOrder(int[] orderedSiteMapItemIDs)
        {
            if (orderedSiteMapItemIDs == null)
                return false;

            for (int i = 0; i < orderedSiteMapItemIDs.Count(); i++)
            {
                var sectionID = orderedSiteMapItemIDs[i];
                var siteMap = this.DbSet.FirstOrDefault(sm => sm.SiteMapID == sectionID);
                siteMap.SM_OrderBy = i + 1;
            }

            this.Context.SaveChanges();
            return true;
        }

        public tbl_SiteMap SaveSiteMap(string R301, int languageID, int menuID, int domainID, string css, bool isMenu, bool isFooter, string menuText, DateTime? date,
            decimal priority, string notifyEmail, string path, bool requiresApproval, bool isSiteMap, ContentType type, string siteMapName, int parentID, int siteMapID,
            int? typeID = null, bool isPredefined = false, int menuDisplayTypeID = 1, DateTime? publishDate = null)
        {
            var section = DbSet.FirstOrDefault(sm => sm.SiteMapID == siteMapID);
            if (section == null)
            {
                section = new tbl_SiteMap()
                {
                    SM_OrderBy = (type == ContentType.Blog) ?
                        0 :
                        this.DbSet.Max(sm => sm.SM_OrderBy).GetValueOrDefault(0) + 1,
                    SM_News = false,
                    SM_Deleted = false,
                    SM_Live = false
                };
                this.Create(section);
            }

            section.SM_LanguageID = languageID;
            section.SM_MenuID = menuID;
            section.SM_Name = siteMapName;
            section.SM_ParentID = parentID;
            section.SM_301 = R301;
            string cType = type.ToString();
            section.SM_ContentTypeID = this.Context.Set<tbl_ContentType>().Where(ct => ct.CTP_Value.Equals(cType))
                .Select(ct => ct.ContentTypeID).FirstOrDefault();
            section.SM_CSS = css;
            section.SM_DomainID = domainID;
            section.SM_Footer = isFooter;
            section.SM_Menu = isMenu;
            section.SM_MenuText = StringManipulation.TruncateLongString(menuText,50); //menuText cannot be longer than 50 characters
            section.SM_NotifyEmail = notifyEmail;
            section.SM_Path = path.Trim('/');
            section.SM_Priority = priority;
            section.SM_RequiresApproval = requiresApproval;
            section.SM_Sitemap = isSiteMap;
            section.SM_Date = date;
            section.SM_TypeID = typeID;
            section.SM_IsPredefined = isPredefined;
            section.SM_MenuDisplayTypeID = menuDisplayTypeID;
            section.SM_PublishDate = publishDate;

            if (type == ContentType.Content)
            {
                var parent = DbSet.FirstOrDefault(sm => sm.SiteMapID == parentID);
                section.SM_URL = (parent != null ? parent.SM_URL + "/" : "/") + path.Trim('/');
            }
            else if (type == ContentType.Category)
            {
                section.SM_URL = "/" + path.Trim('/');
                if (section.SiteMapID > 0)
                    UpdateChild(section);
            }
            else
                section.SM_URL = "/" + path.Trim('/');

            this.Context.SaveChanges();
            return section;
        }

        private void UpdateChild(tbl_SiteMap parent)
        {
            foreach (var item in DbSet.Where(s => !s.SM_Deleted && s.SM_ParentID == parent.SiteMapID))
            {
                if (item.IsType(ContentType.Product))
                {
                    item.SM_URL = FriendlyUrl.CreateFriendlyUrl(String.Format("{0}/{1}", parent.SM_URL, item.tbl_Products.P_Title));
                    item.SM_Path = item.SM_URL.Trim('/');
                }
                else if (item.IsType(ContentType.Category))
                {
                    item.SM_URL = FriendlyUrl.CreateFriendlyUrl(String.Format("{0}/{1}", parent.SM_URL, item.tbl_ProdCategories.PC_Title));
                    item.SM_Path = item.SM_URL.Trim('/');
                    UpdateChild(item);
                }
            }
        }

        public void UpdateParents(int shopSitemapID, ProductType type)
        {
            string sCategory = ContentType.Category.ToString(), sType = type.ToString();
            foreach (var item in DbSet.Where(s => !s.SM_Deleted && s.SM_ParentID == 0 && s.tbl_ContentType.CTP_Value == sCategory && s.tbl_ProdCategories.tbl_ProductTypes.PT_Name == sType))
            {
                item.SM_ParentID = shopSitemapID;
            }

            this.Context.SaveChanges();
        }

        public void UpdateCustomLayout(int siteMapID, int customLayouID)
        {
            var sitemap = DbSet.SingleOrDefault(x => x.SiteMapID == siteMapID);
            if (customLayouID > 0)
            {
                sitemap.SM_CustomLayoutID = customLayouID;
            }
            else
            {
                sitemap.SM_CustomLayoutID = null;
            }

            this.Context.SaveChanges();
        }

        public tbl_SiteMap SaveVisibility(int sitemapID)
        {
            var sitemap = DbSet.FirstOrDefault(sm => sm.SiteMapID == sitemapID);
            if (sitemap == null)
                return null;

            sitemap.SM_Live = !sitemap.SM_Live;
            this.Context.SaveChanges();
            return sitemap;
        }

        public IQueryable<tbl_SiteMapTypes> GetAllTypes()
        {
            return this.Context.Set<tbl_SiteMapTypes>().AsQueryable();
        }
    }
}

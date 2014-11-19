using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IContentRepository
    {
        tbl_Content ApproveContent(int contentID);
        bool DeleteContent(int contentID);
        bool IsContentApproved(int contentID);
        bool IsAnyContentApproved(int sitemapID);
        IQueryable<tbl_Content> GetAll();
        tbl_Content GetByID(int contentID);
        tbl_Content SaveContent(string asKeys, string sContent, string desc, int galleryID, string headLine, string keywords, string menuText,
            string metaData, int newsID, string paContent, string pasKeys, string sKeys, string title, string tweet, bool tweeter, int siteMapID, int contentID);
    }

    public class ContentRepository : Repository<tbl_Content>, IContentRepository
    {
        public ContentRepository(IDALContext context) : base(context) { }

        public tbl_Content ApproveContent(int contentID)
        {
            var content = DbSet.FirstOrDefault(c => c.ContentID == contentID);
            if (content == null)
                return null;

            foreach (var con in content.tbl_SiteMap.tbl_Content)
                con.C_Approved = false;

            content.C_Approved = true;
            this.Context.SaveChanges();
            return content;
        }

        public bool DeleteContent(int contentID)
        {
            var content = DbSet.FirstOrDefault(c => c.ContentID == contentID);
            if (content == null)
                return false;

            content.C_Deleted = true;
            this.Context.SaveChanges();
            return true;
        }

        public bool IsContentApproved(int contentID)
        {
            return DbSet.FirstOrDefault(c => c.ContentID == contentID && c.C_Approved) != null;
        }

        public bool IsAnyContentApproved(int sitemapID)
        {
            return this.DbSet.Where(c => c.C_TableLinkID == sitemapID && c.C_Approved && !c.C_Deleted).Count() > 0;
        }

        public IQueryable<tbl_Content> GetAll()
        {
            return this.All();
        }

        public tbl_Content GetByID(int contentID)
        {
            return this.DbSet.FirstOrDefault(c => c.ContentID == contentID);
        }

        public tbl_Content SaveContent( string asKeys, string sContent, string desc, int galleryID, string headLine, string keywords, string menuText, 
            string metaData, int newsID, string paContent, string pasKeys, string sKeys, string title, string tweet, bool tweeter, int siteMapID, int contentID)
        {
            var content = DbSet.FirstOrDefault(c => c.ContentID == contentID);
            if (content == null)
            {
                content = new tbl_Content() {
                    C_TableLinkID = siteMapID,
                    C_Approved = false
                };
                this.Create(content);
            }

            content.C_ArchiveSearchKeys = asKeys;
            content.C_Content = sContent;
            content.C_Description = desc;
            content.C_GalleryID = galleryID;
            content.C_Headline = headLine;
            content.C_Keywords = keywords;
            content.C_LanguageID = 1;
            content.C_ModificationDate = DateTime.UtcNow;
            content.C_MenuText = menuText;
            content.C_MetaData = metaData;
            content.C_NewsSectionID = newsID;
            content.C_PAContent = paContent;
            content.C_PASearchKeys = pasKeys;
            content.C_SearchKeywords = sKeys;
            content.C_Title = title;
            content.SUB_Tweet = tweet;
            content.SUB_Twitter = tweeter;

            this.Context.SaveChanges();
            return content;
        }
    }
}

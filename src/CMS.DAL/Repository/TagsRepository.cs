using CMS.BL.Entity;
using CMS.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DAL.Repository
{
    public interface ITagsRepository
    {
        bool DeleteTag(int tagID);
        IQueryable<tbl_Tags> GetAll();
        tbl_Tags GetByID(int tagID);
        IQueryable<tbl_Tags> GetBySectionID(int sectionID);
        tbl_Tags GetByURL(string url);
        tbl_Tags SaveTag(string title, string url, int sitemapID);
    }

    public class TagsRepository : Repository<tbl_Tags>, ITagsRepository
    {
        public TagsRepository(IDALContext context) : base(context) { }

        public bool DeleteTag(int tagID)
        {
            var tag = this.DbSet.FirstOrDefault(t => t.TagID == tagID);
            if (tag == null)
                return false;

            this.Delete(tag);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Tags> GetAll()
        {
            return this.All();
        }

        public tbl_Tags GetByID(int tagID)
        {
            return this.DbSet.FirstOrDefault(t => t.TagID == tagID);
        }

        public IQueryable<tbl_Tags> GetBySectionID(int sectionID)
        {
            return this.DbSet.Where(t => t.TA_SiteMapID == sectionID);
        }

        public tbl_Tags GetByURL(string url)
        {
            return this.DbSet.FirstOrDefault(t => t.TA_URL.Equals(url));
        }

        public tbl_Tags SaveTag(string title, string url, int sitemapID)
        {
            var tag = new tbl_Tags
            {
                TA_SiteMapID = sitemapID,
                TA_Title = title,
                TA_URL = url
            };

            this.Create(tag);
            this.Context.SaveChanges();
            return tag;
        }
    }
}

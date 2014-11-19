using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPOITagsRepository
    {
        bool Delete(int poiTagID);
        IQueryable<tbl_POITags> GetAll();
        tbl_POITags GetByID(int poiTagID);
        tbl_POITags Save(string title, int poiTagsGroupID, int poiTagID);
    }

    public class POITagsRepository : Repository<tbl_POITags>, IPOITagsRepository
    {
        public POITagsRepository(IDALContext context) : base(context) { }

        public bool Delete(int poiTagID)
        {
            var tag = this.DbSet.FirstOrDefault(t => t.POITagID == poiTagID);
            if (tag == null)
                return false;

            foreach (var link in tag.tbl_POI.ToList())
            {
                tag.tbl_POI.Remove(link);
            }
            this.DbSet.Remove(tag);

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_POITags> GetAll()
        {
            return this.All();
        }

        public tbl_POITags GetByID(int poiTagID)
        {
            return this.DbSet.FirstOrDefault(t => t.POITagID == poiTagID);
        }

        public tbl_POITags Save(string title, int poiTagsGroupID, int poiTagID)
        {
            var tag = this.DbSet.FirstOrDefault(t => t.POITagID == poiTagID);
            if (tag == null)
            {
                tag = new tbl_POITags();
                this.Create(tag);
            }

            tag.POIT_Title = title;
            tag.POIT_GroupID = poiTagsGroupID;

            this.Context.SaveChanges();
            return tag;
        }
    }
}

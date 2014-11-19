using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPOITagsGroupsRepository
    {
        bool Delete(int poiTagGroupID);
        IQueryable<tbl_POITagsGroups> GetAll();
        tbl_POITagsGroups GetByID(int poiTagGroupID);
        tbl_POITagsGroups Save(string title, int poiTagGroupID);
    }

    public class POITagsGroupsRepository : Repository<tbl_POITagsGroups>, IPOITagsGroupsRepository
    {
        public POITagsGroupsRepository(IDALContext context) : base(context) { }

        public bool Delete(int poiTagGroupID)
        {
            var group = this.DbSet.FirstOrDefault(g => g.POITagsGroupID == poiTagGroupID);
            if (group == null)
                return false;

            foreach (var tag in group.tbl_POITags.ToList())
            {
                foreach (var link in tag.tbl_POI.ToList())
                {
                    tag.tbl_POI.Remove(link);
                }
                this.Context.Set<tbl_POITags>().Remove(tag);
            }
            this.DbSet.Remove(group);

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_POITagsGroups> GetAll()
        {
            return this.All();
        }

        public tbl_POITagsGroups GetByID(int poiTagGroupID)
        {
            return this.DbSet.FirstOrDefault(g => g.POITagsGroupID == poiTagGroupID);
        }

        public tbl_POITagsGroups Save(string title, int poiTagGroupID)
        {
            var group = this.DbSet.FirstOrDefault(g => g.POITagsGroupID == poiTagGroupID);
            if (group == null)
            {
                group = new tbl_POITagsGroups();
                this.Create(group);
            }

            group.POITG_Title = title;

            this.Context.SaveChanges();
            return group;
        }
    }
}

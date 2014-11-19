using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPOIRepository
    {
        bool Delete(int poiID);
        IQueryable<tbl_POI> GetAll();
        tbl_POI GetByID(int poiID);
        tbl_POI Save(string title, string description, int categoryID, int addressID, string latitude, string longitude, string phone, int? sitemapID, int poiID);
        tbl_POI SaveTags(int[] tagsIDs, int poiID);
    }

    public class POIRepository : Repository<tbl_POI>, IPOIRepository
    {
        public POIRepository(IDALContext context) : base(context) { }

        public bool Delete(int poiID)
        {
            var poi = this.DbSet.FirstOrDefault(t => t.POIID == poiID);
            if (poi == null)
                return false;

            foreach (var file in poi.tbl_POIFiles.ToList())
            {
                this.Context.Set<tbl_POIFiles>().Remove(file);
            }
            foreach (var link in poi.tbl_POITags.ToList())
            {
                poi.tbl_POITags.Remove(link);
            }
            this.DbSet.Remove(poi);

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_POI> GetAll()
        {
            return this.All();
        }

        public tbl_POI GetByID(int poiID)
        {
            return this.DbSet.FirstOrDefault(t => t.POIID == poiID);
        }

        public tbl_POI Save(string title, string description, int categoryID, int addressID, string latitude, string longitude, string phone, int? sitemapID, int poiID)
        {
            var poi = this.DbSet.FirstOrDefault(t => t.POIID == poiID);
            if (poi == null)
            {
                poi = new tbl_POI();
                this.Create(poi);
            }

            poi.POI_Title = title;
            poi.POI_CategoryID = categoryID;
            poi.POI_AddressID = addressID;
            poi.POI_Description = description;
            poi.POI_Latitude = latitude;
            poi.POI_Longitude = longitude;
            poi.POI_Phone = phone;
            poi.POI_SitemapID = sitemapID;


            this.Context.SaveChanges();
            return poi;
        }

        public tbl_POI SaveTags(int[] tagsIDs, int poiID)
        {
            var poi = this.DbSet.FirstOrDefault(t => t.POIID == poiID);
            if (poi == null)
                return null;

            foreach (var link in poi.tbl_POITags.ToList())
            {
                poi.tbl_POITags.Remove(link);
            }
            this.Context.SaveChanges();

            foreach (var tagID in tagsIDs)
            {
                var tag = this.Context.Set<tbl_POITags>().FirstOrDefault(t => t.POITagID == tagID);
                if (tag != null)
                    poi.tbl_POITags.Add(tag);
            }

            this.Context.SaveChanges();
            return poi;
        }
    }
}

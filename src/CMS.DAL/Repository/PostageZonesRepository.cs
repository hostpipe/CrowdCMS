using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPostageZonesRepository
    {
        IQueryable<tbl_PostageZones> GetAll();

        bool Delete(int postageZoneID);

        tbl_PostageZones Save(tbl_PostageZones postageZone);
    }
    public class PostageZonesRepository : Repository<tbl_PostageZones>, IPostageZonesRepository
    {
        public PostageZonesRepository(IDALContext context) : base(context) { }

        public bool Delete(int postageZoneID)
        {
            var zone = this.DbSet.FirstOrDefault(m => m.PostageZoneID == postageZoneID);
            if(zone == null)
                return false;
            this.DbSet.Remove(zone);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_PostageZones> GetAll()
        {
            return this.All();
        }

        public tbl_PostageZones Save(tbl_PostageZones postageZone)
        {
            var post = this.CreateOrUpdate(postageZone, postageZone.PostageZoneID);
            if (post.PZ_IsDefault == true)
            {
                var def = this.DbSet.FirstOrDefault(m => m.PZ_IsDefault == true && m.PZ_DomainID == postageZone.PZ_DomainID && m.PostageZoneID != postageZone.PostageZoneID);
                if (def != null)
                    def.PZ_IsDefault = false;
            }
            this.Context.SaveChanges();
            return post;
        }
    }
}

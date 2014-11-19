using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPostageRepository
    {
        IQueryable<tbl_Postage> GetAll();
        tbl_Postage GetByID(int ID);
        tbl_Postage Save(string description, decimal? amount, int PST_DomainID, int? PST_PostageBandID, int? PST_PostageWeightID, int PST_PostageZoneID, int postageID);

        bool Delete(int postageID);
    }

    public class PostageRepository : Repository<tbl_Postage>, IPostageRepository
    {
        public PostageRepository(IDALContext context) : base(context) { }

        public bool Delete(int postageID)
        {
            var post = this.DbSet.FirstOrDefault(m => m.PostageID == postageID);
            if (post == null)
                return false;
            this.DbSet.Remove(post);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Postage> GetAll()
        {
            return this.All();
        }

        public tbl_Postage GetByID(int ID)
        {
            return this.DbSet.FirstOrDefault(m => m.PostageID == ID);
        }

        public tbl_Postage Save(string description, decimal? amount, int PST_DomainID, int? PST_PostageBandID, int? PST_PostageWeightID, int PST_PostageZoneID, int postageID)
        {
            var postage = this.DbSet.FirstOrDefault(m => m.PostageID == postageID);
            if (postage == null)
            {
                postage = new tbl_Postage();
                this.Create(postage);
            }
            postage.PST_Description = description;
            postage.PST_Amount = amount;
            postage.PST_DomainID = PST_DomainID;
            postage.PST_PostageBandID = PST_PostageBandID;
            postage.PST_PostageWeightID = PST_PostageWeightID;
            postage.PST_PostageZoneID = PST_PostageZoneID;
            this.Context.SaveChanges();
            return postage;
        }


    }
}

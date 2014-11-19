using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using System.Data;

namespace CMS.DAL.Repository
{
    public interface IPostageBandsRepository
    {
        IQueryable<tbl_PostageBands> GetAll();

        bool Delete(int postageBandID);

        tbl_PostageBands Save(tbl_PostageBands postageBand);
    }

    public class PostageBandsRepository : Repository<tbl_PostageBands>, IPostageBandsRepository
    {
        public PostageBandsRepository(IDALContext context) : base(context) { }

        public bool Delete(int postageBandID)
        {
            var band = this.DbSet.FirstOrDefault(m => m.PostageBandID == postageBandID);
            if (band == null)
                return false;
            this.DbSet.Remove(band);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_PostageBands> GetAll()
        {
            return this.All();
        }

        public tbl_PostageBands Save(tbl_PostageBands postageBand)
        {
            var post = this.CreateOrUpdate(postageBand, postageBand.PostageBandID);
            this.Context.SaveChanges();
            return post;
        }
    }
}

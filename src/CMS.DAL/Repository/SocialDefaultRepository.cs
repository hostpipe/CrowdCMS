using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ISocialDefaultRepository : IRepository<tbl_Social_Defaults>
    {
        IQueryable<tbl_Social_Defaults> GetAllDefaults();
    }

    public class SocialDefaultRepository : Repository<tbl_Social_Defaults>, ISocialDefaultRepository
    {
        public SocialDefaultRepository(IDALContext context) : base(context) { }


        public IQueryable<tbl_Social_Defaults> GetAllDefaults()
        {
            return this.DbSet.AsQueryable();
        }
    }
}

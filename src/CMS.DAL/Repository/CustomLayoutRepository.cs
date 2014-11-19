using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ICustomLayoutRepository
    {
        IQueryable<tbl_CustomLayout> GetALL();
        tbl_CustomLayout GetByID(int layoutId);
        tbl_CustomLayout SaveCustomLayout(int layoutId, string dir, string name);
    }

    public class CustomLayoutRepository : Repository<tbl_CustomLayout>, ICustomLayoutRepository
    {
        public CustomLayoutRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_CustomLayout> GetALL()
        {
            return this.All();
        }

        public tbl_CustomLayout GetByID(int layoutId)
        {
            return this.DbSet.FirstOrDefault(c => c.CustomLayoutID == layoutId);
        }

        public tbl_CustomLayout SaveCustomLayout(int layoutId, string dir, string name)
        {
            var customLayout = this.DbSet.FirstOrDefault(c => c.CustomLayoutID == layoutId);
            if (customLayout == null)
            {
                customLayout = new tbl_CustomLayout()
                {
                    CL_DisplayName = name,
                    CL_Directory = dir
                };
                Create(customLayout);
            }
            else
            {
                customLayout.CL_DisplayName = name;
                customLayout.CL_Directory = dir;
            }

            Context.SaveChanges();
            return customLayout;

        }
    }
}

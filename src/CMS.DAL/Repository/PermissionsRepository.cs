using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPermissionsRepository
    {
        IQueryable<tbl_Permissions> GetAll();
    }

    public class PermissionsRepository : Repository<tbl_Permissions>, IPermissionsRepository
    {
        public PermissionsRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_Permissions> GetAll()
        {
            return this.All();
        }
    }
}

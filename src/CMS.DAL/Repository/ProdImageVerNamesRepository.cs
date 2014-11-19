using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IProdImageVerNamesRepository
    {
        tbl_ProdImageVerNames GetVersionByName(string name);
    }

    public class ProdImageVerNamesRepository : Repository<tbl_ProdImageVerNames>, IProdImageVerNamesRepository
    {
        public ProdImageVerNamesRepository(IDALContext context) : base(context) { }

        public tbl_ProdImageVerNames GetVersionByName(string name)
        {
            return this.DbSet.FirstOrDefault(pivn => pivn.VN_Name.Equals(name));
        }
    }
}

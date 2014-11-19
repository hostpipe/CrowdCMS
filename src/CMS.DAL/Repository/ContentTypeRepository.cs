using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IContentTypeRepository
    {
        IQueryable<tbl_ContentType> GetAll();
        tbl_ContentType GetByType(ContentType type);
    }

    public class ContentTypeRepository : Repository<tbl_ContentType>, IContentTypeRepository
    {
        public ContentTypeRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_ContentType> GetAll()
        {
            return this.All();
        }

        public tbl_ContentType GetByType(ContentType type)
        {
            var sType = type.ToString();
            return this.DbSet.FirstOrDefault(ct => ct.CTP_Value.Equals(sType));
        }
    }
}

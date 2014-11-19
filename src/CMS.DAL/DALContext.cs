using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Interface;
using CMS.BL.Entity;
using CMS.DAL.Repository;

namespace CMS.DAL
{
    public class DALContext : IDALContext
    {
        private DB dbContext = null;

        public DALContext()
        {
            dbContext = new DB();
        }

        public DB DBContext { get { return dbContext; } }

        public int SaveChanges()
        {
            try
            {
                return dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            if (dbContext != null)
                dbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

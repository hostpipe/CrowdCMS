using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;

namespace CMS.DAL.Repository
{
    public interface IDonationTypeRepository
    {
        IQueryable<tbl_DonationType> GetAll();
    }

    public class DonationTypeRepository : Repository<tbl_DonationType>, IDonationTypeRepository
    {
        public DonationTypeRepository(DALContext context) : base(context) { }

        public IQueryable<tbl_DonationType> GetAll()
        {
            return this.All();
        }
    }
}

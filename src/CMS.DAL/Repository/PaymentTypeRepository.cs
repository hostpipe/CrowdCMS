using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using CMS.BL;

namespace CMS.DAL.Repository
{
    public interface IPaymentTypeRepository
    {
        tbl_PaymentType GetByID(int paymentTypeID);

        tbl_PaymentType GetByCode(PaymentType type);
    }

    public class PaymentTypeRepository : Repository<tbl_PaymentType>, IPaymentTypeRepository
    {
        public PaymentTypeRepository(IDALContext context) : base(context) { }

        public tbl_PaymentType GetByCode(PaymentType type)
        {
            var stringType = type.ToString();
            return this.DbSet.FirstOrDefault(m => m.PT_Code == stringType);
        }

        public tbl_PaymentType GetByID(int paymentTypeID)
        {
            return this.DbSet.FirstOrDefault(pt => pt.PaymentTypeID == paymentTypeID);
        }
    }
}

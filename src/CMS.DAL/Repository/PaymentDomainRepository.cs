using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPaymentDomainRepository
    {
        IQueryable<tbl_PaymentDomain> GetAllByDomainID(int domainID, bool? isLive);
        tbl_PaymentDomain GetByID(int paymentDomainID);
        int GetIDByCode(int domainID, PaymentType code);
        bool UpdateStatus(bool isLive, PaymentType type, int domainID);
        bool UpdateStatus(bool isLive, int paymentTypeID, int domainID);

        bool DeleteImage(int paymentDomainID);
        string SaveImage(int paymentDomainID, string fileName);

        string GetPaymentLogoByDomainID(int paymentDomainID);
    }
    public class PaymentDomainRepository : Repository<tbl_PaymentDomain>, IPaymentDomainRepository
    {
        public PaymentDomainRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_PaymentDomain> GetAllByDomainID(int domainID, bool? isLive)
        {
            return this.DbSet.Where(pd => pd.PD_DomainID == domainID && (!isLive.HasValue || pd.PD_Live == isLive));
        }

        public tbl_PaymentDomain GetByID(int paymentDomainID)
        {
            return this.DbSet.FirstOrDefault(pd => pd.PaymentDomainID == paymentDomainID);
        }

        public int GetIDByCode(int domainID, PaymentType code)
        {
            string scode = code.ToString();
            return this.DbSet.Where(pd => pd.PD_DomainID == domainID && pd.tbl_PaymentType.PT_Code.Equals(scode)).Select(pd => pd.PaymentDomainID).FirstOrDefault();
        }

        public bool UpdateStatus(bool isLive, PaymentType type, int domainID)
        {
            var stringType = type.ToString();
            var item = this.DbSet.FirstOrDefault(m => m.PD_DomainID == domainID && m.tbl_PaymentType.PT_Code == stringType);
            if (item == null)
            {
                var paymentType = this.Context.Set<tbl_PaymentType>().FirstOrDefault(m => m.PT_Code == stringType);
                if (paymentType == null)
                    return false;

                var element = new tbl_PaymentDomain()
                {
                    PD_DomainID = domainID,
                    PD_Live = isLive,
                    PD_PaymentTypeID = paymentType.PaymentTypeID
                };
                this.DbSet.Add(element);
            }
            else
                item.PD_Live = isLive;
            this.Context.SaveChanges();
            return true;
        }

        public bool UpdateStatus(bool isLive, int paymentTypeID, int domainID)
        {
            var item = this.DbSet.FirstOrDefault(m => m.PD_DomainID == domainID && m.PD_PaymentTypeID == paymentTypeID);
            if (item == null)
            {
                var element = new tbl_PaymentDomain()
                {
                    PD_DomainID = domainID,
                    PD_Live = isLive,
                    PD_PaymentTypeID = paymentTypeID
                };
                this.DbSet.Add(element);
            }
            else
                item.PD_Live = isLive;
            this.Context.SaveChanges();
            return true;
        }

        public string SaveImage(int paymentDomainID, string fileName)
        {
            var item = this.DbSet.FirstOrDefault(m => m.PaymentDomainID == paymentDomainID);
            if (item != null)
            {
                item.PD_Logo = fileName;
                this.Context.SaveChanges();
                return fileName;
            }
            return null;
        }

        public bool DeleteImage(int paymentDomainID)
        {
            var item = this.DbSet.FirstOrDefault(m => m.PaymentDomainID == paymentDomainID);
            if (item != null)
            {
                item.PD_Logo = null;
                this.Context.SaveChanges();
                return true;
            }
            return false;
        }

        public string GetPaymentLogoByDomainID(int paymentDomainID)
        {
            var item = this.DbSet.FirstOrDefault(m => m.PaymentDomainID == paymentDomainID);
            if (item != null)
                return item.PD_Logo;
            return null;
        }
    }
}

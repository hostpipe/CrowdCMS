using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IDiscountRepository
    {
        bool DeleteDiscount(int discountID);
        IQueryable<tbl_Discount> GetAll();
        tbl_Discount GetByCode(string code, int domainID);
        tbl_Discount GetByID(int discountID);
        tbl_Discount SaveDiscount(decimal value, bool isPercentage, string code, string desc, string title, DateTime? start, DateTime? expire, int domainID, int discountID);
    }

    public class DiscountRepository : Repository<tbl_Discount>, IDiscountRepository
    {
        public DiscountRepository(IDALContext context) : base(context) { }

        public bool DeleteDiscount(int discountID)
        {
            var discount = this.DbSet.FirstOrDefault(d => d.DiscountID == discountID);
            if (discount == null)
                return false;

            this.Delete(discount);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Discount> GetAll()
        {
            return this.All();
        }

        public tbl_Discount GetByID(int discountID)
        {
            return this.DbSet.FirstOrDefault(d => d.DiscountID == discountID);
        }

        public tbl_Discount GetByCode(string code, int domainID)
        {
            return this.DbSet.FirstOrDefault(d => d.D_DomainID == domainID && d.D_Code.Equals(code) &&
                (!d.D_Start.HasValue || DateTime.Now >= d.D_Start.Value) && 
                (!d.D_Expiry.HasValue || DateTime.Now <= d.D_Expiry.Value));
        }

        public tbl_Discount SaveDiscount(decimal value, bool isPercentage, string code, string desc, string title, DateTime? start, DateTime? expire, int domainID, int discountID)
        {
            var discount = this.DbSet.FirstOrDefault(d => d.DiscountID == discountID);
            if (discount == null)
            {
                discount = new tbl_Discount();
                this.Create(discount);
            }

            discount.D_Value = value;
            discount.D_IsPercentage = isPercentage;
            discount.D_Code = code;
            discount.D_Description = desc;
            discount.D_Title = title;
            discount.D_Start = start;
            discount.D_Expiry = expire.HasValue ? expire.Value.AddDays(1) : expire;
            discount.D_DomainID = domainID;

            this.Context.SaveChanges();
            return discount;
        }
    }
}

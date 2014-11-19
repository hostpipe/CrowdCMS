using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ITaxRepository
    {
        bool CanAddTax(string name, decimal percentage, int taxID = 0);
        bool DeleteTax(int taxID);
        IQueryable<tbl_Tax> GetAll();
        tbl_Tax GetByID(int taxID);
        tbl_Tax GetByName(string name);
        tbl_Tax SaveTax(string title, decimal? percentage, int taxID);
    }

    public class TaxRepository : Repository<tbl_Tax>, ITaxRepository
    {
        public TaxRepository(IDALContext context) : base(context) { }

        public bool CanAddTax(string name, decimal percentage, int taxID = 0)
        {
            return this.DbSet.Where(t => t.TaxID != taxID).All(t => !t.TA_Title.Equals(name) || t.TA_Percentage != percentage);
        }

        public bool DeleteTax(int taxID)
        {
            var tax = this.DbSet.FirstOrDefault(t => t.TaxID == taxID);
            if (tax == null)
                return false;

            foreach (var category in tax.tbl_ProdCategories.ToList())
                category.PC_TaxID = null;

            foreach (var product in tax.tbl_Products.ToList())
                product.P_TaxID = null;

            this.Delete(tax);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Tax> GetAll()
        {
            return this.All();
        }

        public tbl_Tax GetByID(int taxID)
        {
            return this.DbSet.FirstOrDefault(t => t.TaxID == taxID);
        }

        public tbl_Tax GetByName(string name)
        {
            return this.DbSet.FirstOrDefault(t => t.TA_Title.Equals(name));
        }

        public tbl_Tax SaveTax(string title, decimal? percentage, int taxID)
        {
            tbl_Tax tax = this.DbSet.FirstOrDefault(t => t.TaxID == taxID);
            if (tax == null)
            {
                tax = new tbl_Tax();
                this.Create(tax);
            }

            tax.TA_Percentage = percentage;
            tax.TA_Title = title;

            this.Context.SaveChanges();
            return tax;
        }
    }
}

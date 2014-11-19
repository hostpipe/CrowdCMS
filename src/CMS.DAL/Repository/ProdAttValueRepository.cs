using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IProdAttValueRepository
    {
        bool DeleteProdAttValue(int prodAttValueID);
        IQueryable<tbl_ProdAttValue> GetAll();
        tbl_ProdAttValue GetByID(int prodAttValueID);
        tbl_ProdAttValue SaveProdAttValue(string value, decimal priceMod, int attributeValueID, int attributeID = 0);
    }

    public class ProdAttValueRepository : Repository<tbl_ProdAttValue>, IProdAttValueRepository
    {
        public ProdAttValueRepository(IDALContext context) : base(context) { }

        public bool DeleteProdAttValue(int prodAttValueID)
        {
            var prodAttValue = this.DbSet.FirstOrDefault(v => v.AttributeValueID == prodAttValueID);
            if (prodAttValue == null)
                return false;

            this.Delete(prodAttValue);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_ProdAttValue> GetAll()
        {
            return this.All();
        }

        public tbl_ProdAttValue GetByID(int prodAttValueID)
        {
            return this.DbSet.FirstOrDefault(v => v.AttributeValueID == prodAttValueID);
        }

        public tbl_ProdAttValue SaveProdAttValue(string value, decimal priceMod, int attributeValueID, int attributeID = 0)
        {
            var attValue = this.DbSet.FirstOrDefault(var => var.AttributeValueID == attributeValueID);
            if (attValue == null)
            {
                attValue = new tbl_ProdAttValue()
                {
                    AV_AttributeID = attributeID
                };
                this.Create(attValue);
            }

            attValue.AV_Value = value;
            attValue.AV_PriceMod = priceMod;

            this.Context.SaveChanges();
            return attValue;
        }
    }
}

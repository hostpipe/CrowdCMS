using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IProdAttributesRepository
    {
        bool DeleteProdAttribute(int prodAttributeID);
        IQueryable<tbl_ProdAttributes> GetAll();
        tbl_ProdAttributes GetByID(int prodAttributeID);
        tbl_ProdAttributes SaveProdAttribute(string title, int attributeID);
    }

    public class ProdAttributesRepository : Repository<tbl_ProdAttributes>, IProdAttributesRepository
    {
        public ProdAttributesRepository(IDALContext context) : base(context) { }

        public bool DeleteProdAttribute(int prodAttributeID)
        {
            var prodAttribute = this.DbSet.FirstOrDefault(pa => pa.AttributeID == prodAttributeID);
            if (prodAttribute == null)
                return false;

            foreach (var value in prodAttribute.tbl_ProdAttValue.ToList())
            {
                foreach (var ppa in value.tbl_ProdPriceAttributes.ToList())
                {
                    this.Context.Set<tbl_ProdPriceAttributes>().Remove(ppa);
                }
                this.Context.Set<tbl_ProdAttValue>().Remove(value);
            }
            
            foreach (var link in prodAttribute.tbl_ProdAttLink.ToList())
            {
                this.Context.Set<tbl_ProdAttLink>().Remove(link);
            }


            this.Delete(prodAttribute);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_ProdAttributes> GetAll()
        {
            return this.All();
        }

        public tbl_ProdAttributes GetByID(int prodAttributeID)
        {
            return this.DbSet.FirstOrDefault(pa => pa.AttributeID == prodAttributeID);
        }

        public tbl_ProdAttributes SaveProdAttribute(string title, int attributeID)
        {
            var attribute = this.DbSet.FirstOrDefault(pa => pa.AttributeID == attributeID);
            if (attribute == null)
            {
                attribute = new tbl_ProdAttributes();
                this.Create(attribute);
            }

            attribute.A_Title = title;

            this.Context.SaveChanges();
            return attribute;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IBasketContentRepository
    {
        bool DeleteBasketContent(int basketContentID);
        tbl_BasketContent GetByID(int basketContentID);
        tbl_BasketContent SaveBasketContent(int basketID, bool? notAvailable, decimal price, int prodPriceID, int quantity, string sku, string title, string shortDescription, string description, int basketContentID);
    }

    public class BasketContentRepository : Repository<tbl_BasketContent>, IBasketContentRepository
    {
        public BasketContentRepository(IDALContext context) : base(context) { }

        public bool DeleteBasketContent(int basketContentID)
        {
            tbl_BasketContent basketContent = this.DbSet.FirstOrDefault(b => b.BaskContentID == basketContentID);
            if (basketContent == null)
                return false;

            this.Delete(basketContent);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_BasketContent GetByID(int basketContentID)
        {
            return this.DbSet.FirstOrDefault(b => b.BaskContentID == basketContentID);
        }

        public tbl_BasketContent SaveBasketContent(int basketID, bool? notAvailable, decimal price, int prodPriceID, int quantity, string sku, string title, string shortDescription, string description, int basketContentID)
        {
            var basketContent = this.DbSet.FirstOrDefault(b => b.BaskContentID == basketContentID);
            if (basketContent == null)
            {
                basketContent = new tbl_BasketContent();
                this.Create(basketContent);
            }

            basketContent.BC_BasketID = basketID;
            basketContent.BC_Title = title;
            basketContent.BC_Descripiton = shortDescription;
            basketContent.BC_NotAvailable = notAvailable;
            basketContent.BC_Price = price;
            basketContent.BC_ProdPriceID = prodPriceID;
            basketContent.BC_Quantity = quantity;
            basketContent.BC_SKU = sku;
            

            this.Context.SaveChanges();
            return basketContent;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IOrderContentRepository
    {
        bool DeleteOrderContent(int orderContentID);
        tbl_OrderContent GetByID(int orderContentID);
        tbl_OrderContent SaveOrderContent(int orderID, decimal? delivery, string desc, decimal? price, int? priceID, short? quantity, string sku, decimal? tax, string title,
            int orderContentID, decimal totalPrice, int productID, string attributes);
    }

    public class OrderContentRepository: Repository<tbl_OrderContent>, IOrderContentRepository
    {
        public OrderContentRepository(IDALContext context) : base(context) { }

        public bool DeleteOrderContent(int orderContentID)
        {
            tbl_OrderContent orderContent = this.DbSet.FirstOrDefault(o => o.OrderContentID == orderContentID);
            if (orderContent == null)
                return false;

            this.Delete(orderContent);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_OrderContent GetByID(int orderContentID)
        {
            return this.DbSet.FirstOrDefault(o => o.OrderContentID == orderContentID);
        }

        public tbl_OrderContent SaveOrderContent(int orderID, decimal? delivery, string desc, decimal? price, int? priceID, short? quantity, string sku, decimal? tax, string title, 
            int orderContentID, decimal totalPrice, int productID, string attributes)
        {
            var orderContent = this.DbSet.FirstOrDefault(o => o.OrderContentID == orderContentID);
            if (orderContent == null)
            {
                orderContent = new tbl_OrderContent();
                this.Create(orderContent);
            }
            orderContent.OC_OrderID = orderID;
            orderContent.OC_Delivery = delivery;
            orderContent.OC_Description = desc;
            orderContent.OC_Price = price;
            orderContent.OC_ProdPriceID = priceID;
            orderContent.OC_Quantity = quantity;
            orderContent.OC_SKU = sku;
            orderContent.OC_Status = (int?)null;
            orderContent.OC_Tax = tax;
            orderContent.OC_Title = title;
            orderContent.OC_TotalPrice = totalPrice;
            orderContent.OC_ProductID = productID;
            orderContent.OC_ProductAttributes = attributes;

            this.Context.SaveChanges();
            return orderContent;
        }
    }
}

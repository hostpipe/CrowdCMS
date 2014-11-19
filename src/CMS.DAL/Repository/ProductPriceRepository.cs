using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using System.Data.Objects.DataClasses;

namespace CMS.DAL.Repository
{
    public interface IProductPriceRepository
    {
        bool CheckIfStockUnitExists(int productID, List<int> attrValuesIDs);
        bool DeletePrice(int priceID);
        bool DeletePrice(int[] priceID);
        bool DeletePriceTimeWindow(int priceTimeWindowID);
        bool DeleteAllStockUnits(int productID);
        tbl_ProductPrice GetByID(int productPriceID);
        tbl_ProductPriceTimeWindow GetTimeWindowByID(int productPriceTimeWindowID);
        tbl_ProductPrice SaveAttrValueForStockUnit(int stockUnitID, int[] attrValueIDs);
        tbl_ProductPrice SaveStockUnit(int productID, string barcode, string delivery, bool onSale, decimal price, decimal? RRP, decimal? salePrice,
            string SKU, int? stock, decimal? weight, DateTime? endDate, DateTime? startdate, int priceID, decimal priceForRegular);
        tbl_ProductPriceTimeWindow SavePriceForTimeWindow(decimal price, DateTime startDate, DateTime? endDate, int productPriceID, int productPriceTimeWindowID);

        bool DecreaseStockUnits(EntityCollection<tbl_OrderContent> orderContent);
        bool IncreaseStockUnits(EntityCollection<tbl_OrderContent> orderContent);
        bool IsEnoughOnStock(EntityCollection<tbl_BasketContent> basketContent);
    }

    public class ProductPriceRepository : Repository<tbl_ProductPrice>, IProductPriceRepository
    {
        public ProductPriceRepository(IDALContext context) : base(context) { }

        public bool CheckIfStockUnitExists(int productID, List<int> attrValuesIDs)
        {
            var prices = this.DbSet.Where(p => p.PR_ProductID == productID);
            if (prices == null || prices.Count() == 0)
                return false;

            return prices.Any(p => attrValuesIDs.All(id => p.tbl_ProdPriceAttributes.Any(ppa => ppa.PPA_ProdAttValID == id)) &&
                p.tbl_ProdPriceAttributes.All(ppa => attrValuesIDs.Any(id => id == ppa.PPA_ProdAttValID)));
        }

        public bool DeletePrice(int priceID)
        {
            var price = this.DbSet.FirstOrDefault(p => p.PriceID == priceID);
            if (price == null)
                return false;

            foreach (var attrValue in price.tbl_ProdPriceAttributes.ToList())
            {
                this.Context.Set<tbl_ProdPriceAttributes>().Remove(attrValue);
            }
            foreach (var basketContent in price.tbl_BasketContent.ToList())
            {
                this.Context.Set<tbl_BasketContent>().Remove(basketContent);
            }
            foreach (var orderContent in price.tbl_OrderContent.ToList())
            {
                orderContent.OC_ProdPriceID = null;
            }
            foreach (var timewindow in price.tbl_ProductPriceTimeWindow.ToList())
            {
                this.Context.Set<tbl_ProductPriceTimeWindow>().Remove(timewindow);
            }

            this.DbSet.Remove(price);
            this.Context.SaveChanges();
            return true;
        }

        public bool DeletePrice(int[] priceID)
        {
            bool success = true;
            foreach (var item in priceID)
            {
                success &= DeletePrice(item);
            }
            if (!success)
                return false;
            this.Context.SaveChanges();
            return true;
        }

        public bool DeletePriceTimeWindow(int priceTimeWindowID)
        {
            var priceTimeWindow = this.Context.Set<tbl_ProductPriceTimeWindow>().FirstOrDefault(p => p.ProductPriceTimeWindowID == priceTimeWindowID);
            if (priceTimeWindow == null)
                return false;

            this.Context.Set<tbl_ProductPriceTimeWindow>().Remove(priceTimeWindow);
            this.Context.SaveChanges();
            return true;
        }

        public bool DeleteAllStockUnits(int productID)
        {
            var prices = this.DbSet.Where(p => p.PR_ProductID == productID);
            if (prices == null || prices.Count() == 0)
                return false;

            foreach (var price in prices.ToList())
            {
                foreach (var attrValue in price.tbl_ProdPriceAttributes.ToList())
                {
                    this.Context.Set<tbl_ProdPriceAttributes>().Remove(attrValue);
                }
                foreach (var basketContent in price.tbl_BasketContent.ToList())
                {
                    this.Context.Set<tbl_BasketContent>().Remove(basketContent);
                }
                foreach (var orderContent in price.tbl_OrderContent.ToList())
                {
                    orderContent.OC_ProdPriceID = null;
                }
                foreach (var timewindow in price.tbl_ProductPriceTimeWindow.ToList())
                {
                    this.Context.Set<tbl_ProductPriceTimeWindow>().Remove(timewindow);
                }

                this.DbSet.Remove(price);
            }
            this.Context.SaveChanges();
            return true;
        }

        public tbl_ProductPrice GetByID(int productPriceID)
        {
            return this.DbSet.FirstOrDefault(pp => pp.PriceID == productPriceID);
        }

        public tbl_ProductPriceTimeWindow GetTimeWindowByID(int productPriceTimeWindowID)
        {
            return this.Context.Set<tbl_ProductPriceTimeWindow>().FirstOrDefault(p => p.ProductPriceTimeWindowID == productPriceTimeWindowID);
        }

        public tbl_ProductPrice SaveAttrValueForStockUnit(int stockUnitID, int[] attrValueIDs)
        {
            var price = this.DbSet.FirstOrDefault(p => p.PriceID == stockUnitID);
            if (price == null)
                return null;

            if (attrValueIDs == null || attrValueIDs.Count() == 0)
                return null;

            foreach (var attrValue in price.tbl_ProdPriceAttributes.ToList())
            {
                this.Context.Set<tbl_ProdPriceAttributes>().Remove(attrValue);
            }

            foreach (var attrValueID in attrValueIDs)
            {
                price.tbl_ProdPriceAttributes.Add(new tbl_ProdPriceAttributes { PPA_ProdAttValID = attrValueID, PPA_ProdPriceID = stockUnitID });
            }

            this.Context.SaveChanges();
            return price;
        }

        public bool DecreaseStockUnits(EntityCollection<tbl_OrderContent> orderContent)
        {
            foreach (var item in orderContent)
            {
                if (item.OC_ProdPriceID == null)
                    return false;
                var productPrice = this.DbSet.FirstOrDefault(m => m.PriceID == item.OC_ProdPriceID);
                if (productPrice == null)
                    return false;
                if(!item.tbl_ProductPrice.tbl_Products.P_StockControl)
                    continue;
                if (productPrice.PR_Stock - item.OC_Quantity < 0)
                    return false;
                productPrice.PR_Stock -= item.OC_Quantity;
            }
            this.Context.SaveChanges();
            return true;
        }

        public bool IncreaseStockUnits(EntityCollection<tbl_OrderContent> orderContent)
        {
            foreach (var item in orderContent)
            {
                if (item.tbl_ProductPrice == null)
                    continue;
                var productPrice = item.tbl_ProductPrice;
                if (!productPrice.tbl_Products.P_StockControl)
                    continue;
                productPrice.PR_Stock += item.OC_Quantity;
            }
            this.Context.SaveChanges();
            return true;
        }

        public tbl_ProductPrice SaveStockUnit(int productID, string barcode, string delivery, bool onSale, decimal price, decimal? RRP, decimal? salePrice, 
            string SKU, int? stock, decimal? weight, DateTime? eventEndDate, DateTime? eventStartdate, int priceID, decimal priceForRegular)
        {
            if (!this.Context.Set<tbl_Products>().Any(p => p.ProductID == productID))
                return null;

            var dbPrice = this.DbSet.FirstOrDefault(p => p.PriceID == priceID);
            if (dbPrice == null)
            {
                dbPrice = new tbl_ProductPrice
                {
                    PR_ProductID = productID
                };
                this.Create(dbPrice);
            }

            dbPrice.PR_Barcode = barcode;
            dbPrice.PR_Delivery = delivery;
            dbPrice.PR_OnSale = onSale;
            dbPrice.PR_Price = price;
            dbPrice.PR_ProductID = productID;
            dbPrice.PR_RRP = RRP;
            dbPrice.PR_SalePrice = salePrice;
            dbPrice.PR_SKU = SKU;
            dbPrice.PR_Stock = stock;
            dbPrice.PR_Weight = weight;
            dbPrice.PR_EventEndDate = eventEndDate;
            dbPrice.PR_EventStartDate = eventStartdate;
            dbPrice.PR_PriceForRegularPlan = priceForRegular;

            this.Context.SaveChanges();
            return dbPrice;
        }

        public tbl_ProductPriceTimeWindow SavePriceForTimeWindow(decimal price, DateTime startDate, DateTime? endDate, int productPriceID, int productPriceTimeWindowID)
        {
            if (!this.Context.Set<tbl_ProductPrice>().Any(p => p.PriceID == productPriceID))
                return null;

            var dbPriceTimeWindow = this.Context.Set<tbl_ProductPriceTimeWindow>().FirstOrDefault(p => p.ProductPriceTimeWindowID == productPriceTimeWindowID);
            if (dbPriceTimeWindow == null)
            {
                dbPriceTimeWindow = new tbl_ProductPriceTimeWindow
                {
                    TW_ProductPriceID = productPriceID
                };
                this.Context.Set<tbl_ProductPriceTimeWindow>().Add(dbPriceTimeWindow);
            }

            dbPriceTimeWindow.TW_Price = price;
            dbPriceTimeWindow.TW_EndDate = endDate;
            dbPriceTimeWindow.TW_StartDate = startDate;

            this.Context.SaveChanges();
            return dbPriceTimeWindow;
        }

        public bool IsEnoughOnStock(EntityCollection<tbl_BasketContent> basketContent)
        {
            foreach (var item in basketContent)
            {
                var prod = this.DbSet.FirstOrDefault(m => m.PriceID == item.BC_ProdPriceID);
                if(prod == null)
                    return false;
                if (!prod.tbl_Products.P_StockControl)
                    continue;
                if (prod.PR_Stock == null || prod.PR_Stock < item.BC_Quantity)
                    return false;
            }
            return true;
        }
    }
}

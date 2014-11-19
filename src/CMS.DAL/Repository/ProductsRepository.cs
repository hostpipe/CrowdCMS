using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using CMS.BL;

namespace CMS.DAL.Repository
{
    public interface IProductsRepository
    {
        bool AddAttrbiute(int productID, int attributeID);
        bool DeleteProduct(int productID);
        bool DeleteProductAssociation(int productID, int associatedProductID);
        IQueryable<tbl_Products> GetAll();
        IQueryable<tbl_Products> GetAllByType(ProductType type);
        tbl_Products GetByID(int productID);
        tbl_ProductPrice GetProductPrice(int productID, int[] attrs, string selectedDate);
        tbl_ProductTypes GetProductTypeByName(ProductType type);
        bool RemoveAttrbiute(int productID, int attributeID);
        tbl_Products SaveAssociatedProduct(int productID, int associatedProductID);
        tbl_Products SaveProduct(decimal? basePrice, int? categoryID, string desc, DateTime lastModifiedDate, int? minQuantity,
            bool? offer, string productCode, int? groupID, string shortDesc, int? taxID, string title, bool live, bool stockControl,
            ProductType type, int? eventTypeID, bool deliverable, bool purchasable, bool featured, string affiliate, int productID);
        bool SaveOrder(int[] orderedProductsIDs);
        tbl_Products SaveVisibility(int productID);
    }

    public class ProductsRepository : Repository<tbl_Products>, IProductsRepository
    {
        public ProductsRepository(IDALContext context) : base(context) { }

        public bool AddAttrbiute(int productID, int attributeID)
        {
            var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return false;

            if (product.tbl_ProdAttLink.Any(pal => pal.PA_AttributeID == attributeID))
                return false;

            if (!this.Context.Set<tbl_ProdAttributes>().Any(pa => pa.AttributeID == attributeID))
                return false;

            product.tbl_ProdAttLink.Add(new tbl_ProdAttLink { PA_ProductID = productID, PA_AttributeID = attributeID });
            this.Context.SaveChanges();
            return true;
        }

        public bool DeleteProduct(int productID)
        {
            var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return false;

            foreach (var association in product.tbl_ProdAss.ToList())
            {
                this.Context.Set<tbl_ProdAss>().Remove(association);
            }
            foreach (var association in product.tbl_ProdAss1.ToList())
            {
                this.Context.Set<tbl_ProdAss>().Remove(association);
            }

            foreach (var attr in product.tbl_ProdAttLink.ToList())
            {
                this.Context.Set<tbl_ProdAttLink>().Remove(attr);
            }

            foreach (var price in product.tbl_ProductPrice.ToList())
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

                this.Context.Set<tbl_ProductPrice>().Remove(price);
            }

            product.P_Deleted = true;
            this.Context.SaveChanges();
            return true;
        }

        public bool DeleteProductAssociation(int productID, int associatedProductID)
        {
            var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return false;

            var association = product.tbl_ProdAss.FirstOrDefault(pa => pa.PAS_ProductID1 == associatedProductID || pa.PAS_ProductID2 == associatedProductID);
            if (association == null)
                association = product.tbl_ProdAss1.FirstOrDefault(pa => pa.PAS_ProductID1 == associatedProductID || pa.PAS_ProductID2 == associatedProductID);
            this.Context.Set<tbl_ProdAss>().Remove(association);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Products> GetAll()
        {
            return this.DbSet.Where(p => !p.P_Deleted && !p.tbl_ProdCategories.PC_Deleted);
        }

        public IQueryable<tbl_Products> GetAllByType(ProductType type)
        {
            string stype = type.ToString();
            return this.DbSet.Where(p => !p.P_Deleted && !p.tbl_ProdCategories.PC_Deleted && p.tbl_ProductTypes.PT_Name.Equals(stype));
        }

        public tbl_Products GetByID(int productID)
        {
            return this.DbSet.FirstOrDefault(p => p.ProductID == productID);
        }

        public tbl_ProductPrice GetProductPrice(int productID, int[] attrs, string selectedDate)
        {
            var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return null;
            if (!String.IsNullOrEmpty(selectedDate))
            {
                string[] date = selectedDate.Split('_');
                if (date.Length == 1)
                {
                    DateTime startDate;
                    if (!DateTime.TryParse(date[0], out startDate))
                        return null;
                    return product.tbl_ProductPrice.FirstOrDefault(pp => (pp.tbl_ProdPriceAttributes.All(ppa => attrs.Contains(ppa.PPA_ProdAttValID))) &&
                        pp.PR_EventStartDate == startDate);
                }
                else if (date.Length == 2)
                {
                    DateTime startDate;
                    DateTime endDate;
                    if (!DateTime.TryParse(date[0], out startDate))
                        return null;
                    if (!DateTime.TryParse(date[1], out endDate))
                        return null;
                    return product.tbl_ProductPrice.FirstOrDefault(pp => (pp.tbl_ProdPriceAttributes.All(ppa => attrs.Contains(ppa.PPA_ProdAttValID))) &&
                        pp.PR_EventStartDate == startDate &&
                        pp.PR_EventEndDate == endDate);
                }
                return null;
            }
            return product.tbl_ProductPrice.FirstOrDefault(pp => pp.tbl_ProdPriceAttributes.All(ppa => attrs.Contains(ppa.PPA_ProdAttValID)));
        }

        public tbl_ProductTypes GetProductTypeByName(ProductType type)
        {
            string stype = type.ToString();
            return this.Context.Set<tbl_ProductTypes>().FirstOrDefault(pt => pt.PT_Name.Equals(stype));
        }

        public bool RemoveAttrbiute(int productID, int attributeID)
        {
            var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return false;

            var attrLink = product.tbl_ProdAttLink.FirstOrDefault(pa => pa.PA_AttributeID == attributeID);
            if (attrLink == null)
                return false;

            foreach (var ppAttr in product.tbl_ProductPrice.SelectMany(pp => pp.tbl_ProdPriceAttributes.Where(ppa => ppa.tbl_ProdAttValue.AV_AttributeID == attributeID)).ToList())
            {
                this.Context.Set<tbl_ProdPriceAttributes>().Remove(ppAttr);
            }

            this.Context.Set<tbl_ProdAttLink>().Remove(attrLink);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_Products SaveAssociatedProduct(int productID, int associatedProductID)
        {
            var product = DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return null;

            if (!(product.tbl_ProdAss.Any(p => p.PAS_ProductID1 == associatedProductID || p.PAS_ProductID2 == associatedProductID) ||
                product.tbl_ProdAss1.Any(p => p.PAS_ProductID1 == associatedProductID || p.PAS_ProductID2 == associatedProductID)))
            {
                product.tbl_ProdAss.Add(new tbl_ProdAss() { PAS_ProductID1 = productID, PAS_ProductID2 = associatedProductID });
            }
            this.Context.SaveChanges();
            return product;
        }

        public tbl_Products SaveProduct(decimal? basePrice, int? categoryID, string desc, DateTime lastModifiedDate, int? minQuantity,
            bool? offer, string productCode, int? groupID, string shortDesc, int? taxID, string title, bool live, bool stockControl,
            ProductType type, int? eventTypeID, bool deliverable, bool purchasable, bool featured, string affiliate, int productID)
        {
            var productType = GetProductTypeByName(type);
            if (productType == null)
                return null;

            var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
            var order = this.DbSet.OrderByDescending(p => p.P_Order).Select(p => p.P_Order).FirstOrDefault();
            if (product == null)
            {
                product = new tbl_Products()
                {
                    ProductID = productID,
                    P_Order = 1 + order.GetValueOrDefault(0),
                    P_Deleted = false
                };
                this.Create(product);
            }

            product.P_BasePrice = basePrice;
            product.P_CategoryID = categoryID;
            product.P_Description = desc;
            product.P_LastModifiedDate = lastModifiedDate;
            product.P_MinQuantity = minQuantity;
            product.P_Offer = offer;
            product.P_ProductCode = productCode;
            product.P_ProductGroupID = groupID;
            product.P_ShortDesc = shortDesc;
            product.P_TaxID = taxID;
            product.P_Title = title;
            product.P_Live = live;
            product.P_StockControl = stockControl;
            product.P_EventTypeID = eventTypeID == 0 ? null : eventTypeID;
            product.P_ProductTypeID = productType.ProductTypeID;
            product.P_Deliverable = deliverable;
            product.P_CanPurchase = purchasable;
            product.P_Featured = featured;
            product.P_AffliateLink = affiliate;

            this.Context.SaveChanges();
            return product;
        }

        public bool SaveOrder(int[] orderedProductsIDs)
        {
            if (orderedProductsIDs == null)
                return false;

            for (int i = 0; i < orderedProductsIDs.Count(); i++)
            {
                var productID = orderedProductsIDs[i];
                var product = this.DbSet.FirstOrDefault(p => p.ProductID == productID);
                product.P_Order = i + 1;
            }

            this.Context.SaveChanges();
            return true;
        }

        public tbl_Products SaveVisibility(int productID)
        {
            var product = DbSet.FirstOrDefault(p => p.ProductID == productID);
            if (product == null)
                return null;

            product.P_Live = !product.P_Live;
            this.Context.SaveChanges();
            return product;
        }
    }
}

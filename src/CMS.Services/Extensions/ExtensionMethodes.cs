using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Repository;
using CMS.BL;
using CMS.DAL;
using System.Web.Mvc;
using System.Globalization;
using System.Data.Objects.DataClasses;

namespace CMS.Services.Extensions
{
    public static class ExtensionMethodes
    {
        private const string currencyFormat = "{0:C}";
        private const string percentageFormat = "{0:0}%";

        #region Product tbl

        public static IEnumerable<tbl_Products> GetWithContent(this EntityCollection<tbl_Products> table, ProductType type)
        {
            return table.Where(p => !p.P_Deleted && p.P_Live && 
                p.tbl_ProductTypes.PT_Name.Equals(type.ToString()) &&
                p.tbl_SiteMap.tbl_Content.Any(c => c.C_Approved && !c.C_Deleted)).OrderBy(p => p.P_Order);
        }

        #endregion Product tbl


        #region ProductAssociation

        public static IEnumerable<tbl_ProdAss> GetOfType(this EntityCollection<tbl_ProdAss> table, int productID, ProductType type)
        {
            if (type == ProductType.Event)
            {
                return table.Where(m => m.tbl_Products.ProductID == productID ?
                    m.tbl_Products1.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Event.ToString() &&
                        m.tbl_Products1.tbl_ProductPrice.Any(pp => pp.PR_EventStartDate > DateTime.Now) :
                    m.tbl_Products.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Event.ToString() &&
                        m.tbl_Products.tbl_ProductPrice.Any(pp => pp.PR_EventStartDate > DateTime.Now));
            }
            else // if(type == ProductType.Item)
            {
                return table.Where(m => m.tbl_Products.ProductID == productID ?
                    m.tbl_Products1.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Item.ToString() :
                    m.tbl_Products.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Item.ToString());
            }
        }

        public static IEnumerable<tbl_ProdAss> GetOfType(this List<tbl_ProdAss> table, int productID, ProductType type)
        {
            if (type == ProductType.Event)
            {
                return table.Where(m => m.tbl_Products.ProductID == productID ?
                    m.tbl_Products1.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Event.ToString() &&
                        m.tbl_Products1.tbl_ProductPrice.Any(pp => pp.PR_EventStartDate > DateTime.Now) :
                    m.tbl_Products.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Event.ToString() &&
                        m.tbl_Products.tbl_ProductPrice.Any(pp => pp.PR_EventStartDate > DateTime.Now));
            }
            else // if(type == ProductType.Item)
            {
                return table.Where(m => m.tbl_Products.ProductID == productID ?
                    m.tbl_Products1.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Item.ToString() :
                    m.tbl_Products.tbl_ProductTypes.PT_Name == CMS.BL.ProductType.Item.ToString());
            }

        }

        #endregion ProductAssociation


        #region ProductPrice tbl

        public static string GetPriceString(this tbl_ProductPrice table, int amount = 1, bool hideTax = false)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table, amount);
            return PriceManager.FormatPrice(priceAndTax.Item1, priceAndTax.Item2, table.tbl_Products.tbl_SiteMap.SM_DomainID, hideTax);
        }

        public static string GetTaxValueString(this tbl_ProductPrice table)
        {
            return String.Format(percentageFormat, PriceManager.GetTaxPercentage(table));
        }

        public static string GetTaxAmountString(this tbl_ProductPrice table, int amount = 1)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table, amount);
            return String.Format("{0:C}", priceAndTax.Item2);
        }

        public static decimal GetPrice(this tbl_ProductPrice table, int amount = 1)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table, amount);
            return priceAndTax.Item1;
        }

        public static decimal GetTaxValue(this tbl_ProductPrice table)
        {
            return PriceManager.GetTaxPercentage(table);
        }

        public static decimal GetTaxAmount(this tbl_ProductPrice table, int amount = 1)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table, amount);
            return priceAndTax.Item2;
        }

        public static decimal GetCurrentPriceForEvent(this tbl_ProductPrice price)
        {
            //TODO: Time Zone handling
            if (price == null)
                return 0;

            var timeWindow = price.tbl_ProductPriceTimeWindow.FirstOrDefault(m => m.TW_StartDate <= DateTime.Now && m.TW_EndDate >= DateTime.Now);
            return (timeWindow == null) ?
                price.PR_Price :
                timeWindow.TW_Price;
        }

        public static IOrderedEnumerable<tbl_ProductPrice> Order(this EntityCollection<tbl_ProductPrice> table)
        {
            string type = table.Select(m => m.tbl_Products).First().tbl_ProductTypes.PT_Name;
            if (type == ProductType.Item.ToString())
            {
                return table.OrderBy(p => p.PR_Price);
            }
            else // if (type == ProductType.Event.ToString())
            {
                return table.OrderBy(p => p.GetCurrentPriceForEvent());
            }
        }

        public static decimal MinPrice(this EntityCollection<tbl_ProductPrice> table)
        {
            string type = table.Select(m => m.tbl_Products).First().tbl_ProductTypes.PT_Name;
            if (type == ProductType.Item.ToString())
            {
                return table.Select(m => m.PR_Price).Min();
            }
            else // if (type == ProductType.Event.ToString())
            {
                return table.Select(m => m.GetCurrentPriceForEvent()).Min();
            }
        }

        public static decimal MaxPrice(this EntityCollection<tbl_ProductPrice> table)
        {
            string type = table.Select(m => m.tbl_Products).First().tbl_ProductTypes.PT_Name;
            if (type == ProductType.Item.ToString())
            {
                return table.Select(m => m.PR_Price).Max();
            }
            else // if (type == ProductType.Event.ToString())
            {
                return table.Select(m => m.GetCurrentPriceForEvent()).Max();
            }
        }

        #endregion


        #region BasketContent tbl

        public static string GetPriceString(this tbl_BasketContent table, bool hideTax = false)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, table.BC_Quantity);
            return PriceManager.FormatPrice(priceAndTax.Item1, priceAndTax.Item2, table.tbl_Basket.B_DomainID, hideTax);
        }

        public static string GetItemPriceString(this tbl_BasketContent table, bool hideTax = false)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, 1);
            return PriceManager.FormatPrice(priceAndTax.Item1, priceAndTax.Item2, table.tbl_Basket.B_DomainID, hideTax);
        }

        public static string GetTaxValueString(this tbl_BasketContent table)
        {
            return String.Format(percentageFormat, PriceManager.GetTaxPercentage(table.tbl_ProductPrice));
        }

        public static string GetTaxAmountString(this tbl_BasketContent table)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, table.BC_Quantity);
            return String.Format("{0:C}", priceAndTax.Item2);
        }

        public static decimal GetPrice(this tbl_BasketContent table, bool? includeTax = null)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();
            var priceIncludesTax = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, table.tbl_Basket.B_DomainID);
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, table.BC_Quantity);

            if (includeTax.HasValue)
            {
                if (includeTax.Value)
                    return priceIncludesTax ?
                        priceAndTax.Item1 :
                        priceAndTax.Item1 + priceAndTax.Item2;
                return priceIncludesTax ?
                    priceAndTax.Item1 - priceAndTax.Item2 :
                    priceAndTax.Item2;
            }
            return priceAndTax.Item1;
        }

        public static decimal GetTaxValue(this tbl_BasketContent table)
        {
            return PriceManager.GetTaxPercentage(table.tbl_ProductPrice);
        }

        public static decimal GetTaxAmount(this tbl_BasketContent table)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, table.BC_Quantity);
            return priceAndTax.Item2;
        }

        #endregion


        #region Basket tbl

        public static decimal GetPrice(this tbl_Basket table, bool? includeTax = null)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();
            var priceIncludesTax = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, table.B_DomainID);

            Tuple<decimal, decimal> priceAndTax = GetBasketPriceAndTax(table);

            if (includeTax.HasValue)
            {
                if (includeTax.Value)
                    return priceIncludesTax ? priceAndTax.Item1 : priceAndTax.Item1 + priceAndTax.Item2;
                return priceIncludesTax ? priceAndTax.Item1 - priceAndTax.Item2 : priceAndTax.Item2;
            }
            return priceAndTax.Item1;
        }

        public static string GetPriceString(this tbl_Basket table, bool hideTax = false)
        {
            Tuple<decimal, decimal> priceAndTax = GetBasketPriceAndTax(table);
            return PriceManager.FormatPrice(priceAndTax.Item1, priceAndTax.Item2, table.B_DomainID, hideTax);
        }

        public static decimal GetProductsPrice(this tbl_Basket table, bool? includeTax = null)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();
            var priceIncludesTax = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, table.B_DomainID);

            Tuple<decimal, decimal> priceAndTax = GetBasketPriceAndTax(table, true);

            if (includeTax.HasValue)
            {
                if (includeTax.Value)
                    return priceIncludesTax ? priceAndTax.Item1 : priceAndTax.Item1 + priceAndTax.Item2;
                return priceIncludesTax ? priceAndTax.Item1 - priceAndTax.Item2 : priceAndTax.Item2;
            }
            return priceAndTax.Item1;
        }

        public static string GetProductsPriceString(this tbl_Basket table, bool hideTax = false)
        {
            Tuple<decimal, decimal> priceAndTax = GetBasketPriceAndTax(table, true);
            return PriceManager.FormatPrice(priceAndTax.Item1, priceAndTax.Item2, table.B_DomainID, hideTax);
        }

        public static decimal GetTaxAmount(this tbl_Basket table)
        {
            Tuple<decimal, decimal> priceAndTax = GetBasketPriceAndTax(table);
            return priceAndTax.Item2;
        }

        public static string GetTaxAmountString(this tbl_Basket table)
        {
            Tuple<decimal, decimal> priceAndTax = GetBasketPriceAndTax(table);
            return String.Format("{0:C}", priceAndTax.Item2);
        }

        public static decimal GetDeliveryAmount(this tbl_Basket table, bool? includeTax = null)
        {
            if (table.tbl_Postage != null && table.tbl_BasketContent.Count > 0)
            {
                IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();
                var priceIncludesTax = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, table.B_DomainID);

                decimal maxTax = table.tbl_BasketContent.Max(bc => bc.GetTaxValue());
                Tuple<decimal, decimal> postagePriceAndTax = PriceManager.GetPostagePriceAndTax(table.tbl_Postage, maxTax, table.B_DomainID);

                if (includeTax.HasValue)
                {
                    if (includeTax.Value)
                        return priceIncludesTax ? postagePriceAndTax.Item1 : postagePriceAndTax.Item1 + postagePriceAndTax.Item2;
                    return priceIncludesTax ? postagePriceAndTax.Item1 - postagePriceAndTax.Item2 : postagePriceAndTax.Item1;
                }

                return postagePriceAndTax.Item1;
            }
            return 0;
        }

        public static string GetDeliveryAmountString(this tbl_Basket table, bool hideTax = false)
        {
            if (table.tbl_Postage != null && table.tbl_BasketContent.Count > 0)
            {
                decimal maxTax = table.tbl_BasketContent.Max(bc => bc.GetTaxValue());
                Tuple<decimal, decimal> postagePriceAndTax = PriceManager.GetPostagePriceAndTax(table.tbl_Postage, maxTax, table.B_DomainID);
                return PriceManager.FormatPrice(postagePriceAndTax.Item1, postagePriceAndTax.Item2, table.B_DomainID, hideTax);
            }
            return String.Empty;
        }

        public static decimal GetDeliveryTaxAmount(this tbl_Basket table)
        {
            if (table.tbl_Postage != null && table.tbl_BasketContent.Count > 0)
            {
                decimal maxTax = table.tbl_BasketContent.Max(bc => bc.GetTaxValue());
                Tuple<decimal, decimal> postagePriceAndTax = PriceManager.GetPostagePriceAndTax(table.tbl_Postage, maxTax, table.B_DomainID);
                return postagePriceAndTax.Item2;
            }
            return 0;
        }

        public static string GetDeliveryTaxAmountString(this tbl_Basket table, bool hideTax = false)
        {
            if (table.tbl_Postage != null && table.tbl_BasketContent.Count > 0)
            {
                decimal maxTax = table.tbl_BasketContent.Max(bc => bc.GetTaxValue());
                Tuple<decimal, decimal> postagePriceAndTax = PriceManager.GetPostagePriceAndTax(table.tbl_Postage, maxTax, table.B_DomainID);
                return String.Format("{0:C}", postagePriceAndTax.Item2);
            }
            return String.Empty;
        }

        public static decimal GetDiscountAmount(this tbl_Basket table)
        {
            if (table.tbl_Discount != null)
            {
                decimal totalPrice = 0, totalTaxAmount = 0;
                foreach (var content in table.tbl_BasketContent)
                {
                    Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(content.tbl_ProductPrice, content.BC_Quantity);
                    totalPrice += priceAndTax.Item1;
                    totalTaxAmount += priceAndTax.Item2;
                }
                return PriceManager.GetDiscountAmount(table.tbl_Discount, totalPrice, totalTaxAmount, table.B_DomainID);
            }
            return 0;
        }

        public static string GetDiscountAmountString(this tbl_Basket table)
        {
            return String.Format("{0:C}", table.GetDiscountAmount());
        }

        public static decimal GetWeight(this tbl_Basket table)
        {
            return table.tbl_BasketContent.Sum(bc => bc.tbl_ProductPrice.PR_Weight.GetValueOrDefault(0));
        }

        private static Tuple<decimal, decimal> GetBasketPriceAndTax(tbl_Basket table, bool onlyProductsPrice = false)
        {
            if (table == null || table.tbl_BasketContent.Count == 0)
                return new Tuple<decimal, decimal>(0, 0);

            decimal totalPrice = 0, totalTaxAmount = 0;
            foreach (var content in table.tbl_BasketContent)
            {
                Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(content.tbl_ProductPrice, content.BC_Quantity);
                totalPrice += priceAndTax.Item1;
                totalTaxAmount += priceAndTax.Item2;
            }

            if (onlyProductsPrice)
                return new Tuple<decimal, decimal>(totalPrice, totalTaxAmount);

            if (table.tbl_Discount != null)
            {
                Tuple<decimal, decimal> priceAndTax = PriceManager.AddDiscountToPrice(table.tbl_Discount, totalPrice, totalTaxAmount, table.B_DomainID);
                totalPrice = priceAndTax.Item1;
                totalTaxAmount = priceAndTax.Item2;
            }

            if (table.tbl_Postage != null)
            {
                decimal maxTax = table.tbl_BasketContent.Max(bc => bc.GetTaxValue());
                Tuple<decimal, decimal> postagePriceAndTax = PriceManager.GetPostagePriceAndTax(table.tbl_Postage, maxTax, table.B_DomainID);
                totalPrice += postagePriceAndTax.Item1;
                totalTaxAmount += postagePriceAndTax.Item2;
            }

            return new Tuple<decimal, decimal>(totalPrice, totalTaxAmount);
        }

        #endregion


        #region OrderContent tbl

        public static string GetPriceString(this tbl_OrderContent table, bool hideTax = false)
        {
            var price = PriceManager.GetPrice(table.OC_Price.GetValueOrDefault(), table.OC_Tax.GetValueOrDefault(), (int)table.OC_Quantity.GetValueOrDefault(), table.tbl_Orders.O_DomainID);
            return PriceManager.FormatPrice(price.Item1, price.Item2, table.tbl_Orders.O_DomainID, hideTax);
        }

        public static string GetItemPriceString(this tbl_OrderContent table, bool hideTax = false)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPrice(table.OC_Price.GetValueOrDefault(), table.OC_Tax.GetValueOrDefault(), 1, table.tbl_Orders.O_DomainID);
            return PriceManager.FormatPrice(priceAndTax.Item1, priceAndTax.Item2, table.tbl_Orders.O_DomainID, hideTax);
        }

        public static string GetTaxValueString(this tbl_OrderContent table)
        {
            return String.Format(percentageFormat, PriceManager.GetTaxPercentage(table.tbl_ProductPrice));
        }

        public static string GetTaxAmountString(this tbl_OrderContent table)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, (int)table.OC_Quantity.GetValueOrDefault(0));
            return String.Format("{0:C}", priceAndTax.Item2);
        }

        public static decimal GetPrice(this tbl_OrderContent table)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, (int)table.OC_Quantity.GetValueOrDefault(0));
            return priceAndTax.Item1;
        }

        public static decimal GetTaxValue(this tbl_OrderContent table)
        {
            return PriceManager.GetTaxPercentage(table.tbl_ProductPrice);
        }

        public static decimal GetTaxAmount(this tbl_OrderContent table)
        {
            Tuple<decimal, decimal> priceAndTax = PriceManager.GetPriceAndTaxAmounts(table.tbl_ProductPrice, (int)table.OC_Quantity.GetValueOrDefault(0));
            return priceAndTax.Item2;
        }

        #endregion


        #region Orders tbl

        public static decimal GetPrice(this tbl_Orders table)
        {
            Tuple<decimal, decimal> priceAndTax = GetOrderPriceAndTax(table);
            return table.O_IsCustomAmount ? table.TotalAmount : priceAndTax.Item1;
        }

        public static string GetPriceString(this tbl_Orders table, bool hideTax = false)
        {
            return PriceManager.FormatPrice(table.O_IsCustomAmount ? table.TotalAmount : table.Amount, table.TotalTaxAmount, table.O_DomainID, hideTax);
        }

        public static decimal GetTaxAmount(this tbl_Orders table)
        {
            Tuple<decimal, decimal> priceAndTax = GetOrderPriceAndTax(table);
            return priceAndTax.Item2;
        }

        public static string GetTaxAmountString(this tbl_Orders table)
        {
            Tuple<decimal, decimal> priceAndTax = GetOrderPriceAndTax(table);
            return String.Format("{0:C}", priceAndTax.Item2);
        }

        public static decimal GetDeliveryAmount(this tbl_Orders table)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();

            bool useTax = domainService.GetSettingsValueAsBool(SettingsKey.useTax, table.O_DomainID);
            bool priceIncludesVat = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, table.O_DomainID);

            return (useTax && priceIncludesVat) ?
                table.TotalDeliveryAmount : 
                table.DeliveryCharge.GetValueOrDefault(0);
        }

        public static string GetDeliveryAmountString(this tbl_Orders table, bool hideTax = false)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();

            bool useTax = domainService.GetSettingsValueAsBool(SettingsKey.useTax, table.O_DomainID);
            bool priceIncludesVat = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, table.O_DomainID);

            return PriceManager.FormatPrice((useTax && priceIncludesVat) ? table.TotalDeliveryAmount : table.DeliveryCharge.GetValueOrDefault(0), table.DeliveryTax.GetValueOrDefault(0), table.O_DomainID, hideTax);
        }

        public static decimal GetDiscountAmount(this tbl_Orders table)
        {
            return table.DiscountAmount;
        }

        public static string GetDiscountAmountString(this tbl_Orders table)
        {
            return String.Format("{0:C}", table.GetDiscountAmount());
        }

        public static decimal GetWeight(this tbl_Orders table)
        {
            return table.tbl_OrderContent.Sum(bc => bc.tbl_ProductPrice.PR_Weight.GetValueOrDefault(0));
        }

        private static Tuple<decimal, decimal> GetOrderPriceAndTax(tbl_Orders table)
        {
            if (table == null || table.tbl_OrderContent.Count == 0)
                return new Tuple<decimal, decimal>(0, 0);

            decimal totalPrice = 0, totalTaxAmount = 0;
            foreach (var content in table.tbl_OrderContent)
            {
                Tuple<decimal, decimal> priceAndTax = PriceManager.GetPrice(content.OC_Price.GetValueOrDefault(), content.OC_Tax.GetValueOrDefault(0), (int)content.OC_Quantity.GetValueOrDefault(0), table.O_DomainID);
                totalPrice += priceAndTax.Item1;
                totalTaxAmount += priceAndTax.Item2;
            }

            if (table.tbl_Discount != null)
            {
                Tuple<decimal, decimal> priceAndTax = PriceManager.AddDiscountToPrice(table.tbl_Discount, totalPrice, totalTaxAmount, table.O_DomainID);
                totalPrice = priceAndTax.Item1;
                totalTaxAmount = priceAndTax.Item2;
            }

            if (table.tbl_Postage != null)
            {
                decimal maxTax = table.tbl_OrderContent.Max(bc => bc.GetTaxValue());
                Tuple<decimal, decimal> postagePriceAndTax = PriceManager.GetPostagePriceAndTax(table.tbl_Postage, maxTax, table.O_DomainID);
                totalPrice += postagePriceAndTax.Item1;
                totalTaxAmount += postagePriceAndTax.Item2;
            }

            return new Tuple<decimal, decimal>(totalPrice, totalTaxAmount);
        }

        #endregion



    }
}

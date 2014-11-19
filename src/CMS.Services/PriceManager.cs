using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.Services.Extensions;
using System.Web.Mvc;
using CMS.BL;
using System.Data.Objects.DataClasses;

namespace CMS.Services
{
    public static class PriceManager
    {
        public static string FormatPrice(decimal price, decimal tax, int domainID, bool hideTax = false)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();

            string priceString = String.Format("{0:C}", price);
            if (!hideTax && domainService.GetSettingsValueAsBool(SettingsKey.useTax, domainID))
            {
                string taxFormat = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, domainID) ? " ({0:C} VAT)" : " + {0:C} VAT";
                priceString += String.Format(taxFormat, tax);
            }

            return priceString;
        }

        public static Tuple<decimal, decimal> GetPriceAndTaxAmounts(tbl_ProductPrice table, int amount)
        {
            decimal tax = PriceManager.GetTaxPercentage(table);
            decimal basePrice = PriceManager.GetBasePrice(table);

            return PriceManager.GetPrice(basePrice, tax, amount, table.tbl_Products.tbl_SiteMap.SM_DomainID);
        }

        public static Tuple<decimal, decimal> AddDiscountToPrice(tbl_Discount discount, decimal price, decimal tax, int domainID)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();

            bool useTax = domainService.GetSettingsValueAsBool(SettingsKey.useTax, domainID);
            bool priceIncludesVat = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, domainID);

            decimal totalPrice = useTax ? priceIncludesVat ? price : price + tax : price;

            if (discount.D_IsPercentage)
                totalPrice -= totalPrice * (discount.D_Value / 100);
            else
                totalPrice -= discount.D_Value;

            if (totalPrice < 0)
                totalPrice = 0;

            if (!useTax)
                return new Tuple<decimal, decimal>(totalPrice, 0);

            decimal taxPercentage = price != 0 ? priceIncludesVat ? (100 * tax) / price : (100 * tax) / (price + tax) : 0;

            price = priceIncludesVat ? totalPrice : totalPrice - totalPrice * (taxPercentage / 100);
            tax = totalPrice * (taxPercentage / 100);

            if (price < 0)
                price = tax = 0;

            return new Tuple<decimal,decimal>(price, tax);
        }

        public static decimal GetDiscountAmount(tbl_Discount discount, decimal price, decimal tax, int domainID)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();

            bool useTax = domainService.GetSettingsValueAsBool(SettingsKey.useTax, domainID);
            bool priceIncludesVat = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, domainID);

            decimal totalPrice = useTax ? priceIncludesVat ? price : price + tax : price;
            return discount.D_IsPercentage ? totalPrice * (discount.D_Value / 100) : discount.D_Value;
        }

        public static Tuple<decimal, decimal> GetPostagePriceAndTax(tbl_Postage postage, decimal taxValue, int domainID)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();

            bool useTax = domainService.GetSettingsValueAsBool(SettingsKey.useTax, domainID);
            bool priceIncludesVat = domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, domainID);

            decimal postageAmount = postage.PST_Amount.GetValueOrDefault(0);

            decimal tax = useTax ? postageAmount * (taxValue / 100) : 0;
            decimal price = useTax && priceIncludesVat ? postageAmount + tax : postageAmount;

            return new Tuple<decimal, decimal>(price, tax);
        }

        public static decimal GetTaxPercentage(tbl_ProductPrice table)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();
            if (!domainService.GetSettingsValueAsBool(SettingsKey.useTax, table.tbl_Products.tbl_SiteMap.SM_DomainID))
                return 0;

            decimal tax = table.tbl_Products.P_TaxID.HasValue && table.tbl_Products.P_TaxID.Value != 0 ? table.tbl_Products.tbl_Tax.TA_Percentage.GetValueOrDefault(-1) : -1;

            if (tax == -1 && table.tbl_Products.tbl_ProdCategories.PC_TaxID.HasValue && table.tbl_Products.tbl_ProdCategories.PC_TaxID.Value != 0)
                tax = table.tbl_Products.tbl_ProdCategories.tbl_Tax.TA_Percentage.GetValueOrDefault(-1);

            if (tax == -1)
            {
                var parentCategory = table.tbl_Products.tbl_ProdCategories.tbl_ProdCategories2;
                while (parentCategory != null)
                {
                    if (parentCategory.PC_TaxID.HasValue && parentCategory.PC_TaxID.Value != 0)
                    {
                        tax = parentCategory.tbl_Tax.TA_Percentage.GetValueOrDefault(-1);
                        break;
                    }
                    parentCategory = parentCategory.tbl_ProdCategories2;
                }
            }

            if (tax == -1)
                tax = domainService.GetSettingsValueAsDecimal(SettingsKey.taxPercentage, table.tbl_Products.tbl_SiteMap.SM_DomainID);

            return tax != -1 ? tax : 0;
        }

        public static Tuple<decimal, decimal> GetPrice(decimal basePrice, decimal tax, int amount, int domainID)
        {
            IDomain domainService = (IDomain)DependencyResolver.Current.GetService<IDomain>();
            decimal price = 0, taxAmount = 0;

            if (!domainService.GetSettingsValueAsBool(SettingsKey.useTax, domainID))
                return new Tuple<decimal, decimal>(amount * basePrice, 0);

            if (domainService.GetSettingsValueAsBool(SettingsKey.priceSaveIncludesVAT, domainID))
                price = amount * (basePrice - (basePrice * tax / (100 + tax)));
            else
                price = amount * basePrice;

            if (price < 0)
                price = 0;

            taxAmount = price * (tax / 100);

            if (domainService.GetSettingsValueAsBool(SettingsKey.priceDisplayIncludesVAT, domainID))
                price = price + taxAmount;

            return new Tuple<decimal, decimal>(price, taxAmount);
        }

        public static decimal GetBasePrice(tbl_ProductPrice table)
        {
            return table.PR_OnSale ? table.PR_SalePrice.GetValueOrDefault(0) : 
                table.tbl_Products.tbl_ProductTypes.PT_Name == ProductType.Event.ToString() ?
                     table.GetCurrentPriceForEvent() : table.PR_Price;
        }
    }
}

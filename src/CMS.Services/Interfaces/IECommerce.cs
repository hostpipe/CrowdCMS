using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.Utils.Extension;
using System.Web.Mvc;
using CMS.BL;
using System.Data.Objects.DataClasses;

namespace CMS.Services
{
    public interface IECommerce
    {
        tbl_Address SaveAddress(int customerID, int countryID, string county, string firstName, string surName, string title,
            string addLine1, string addLine2, string addLine3, string postcode, string phone, string town, int addressID);
        tbl_Address GetAddressByID(int ID);
        List<tbl_Address> GetAllAddresses(int customerID);
        bool DeleteAddress(int addressID);
        List<SelectListItem> GetAllAddressesAsSelectList(int customerID, int countryID = 0);

        tbl_Basket AddContentToBasket(int basketID, int productID, int amount, int[] attrs, string selectedDate);
        tbl_Basket AddDiscountToBasket(int basketID, string code, int domainID);
        bool DeleteBasket(int basketID);
        bool DeleteUnusedBaskets();
        bool DeleteBasketContent(int basketContentID);
        tbl_Basket GetBasketByID(int basketID);
        tbl_BasketContent GetBasketContentByID(int basketContentID);
        tbl_Basket RemoveDiscountFromBasket(int basketID);
        tbl_Basket SaveBasket(int? customerID, int? discountID, string sessionID, int domainID, int basketID);
        tbl_Basket SaveBasketWithContent(int? customerID, int? discountID, string sessionID, int domainID, int basketID, int productID, int amount, int[] attrs, string selectedDate);
        tbl_Basket UpdateBasket(string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, int billingCountryID, string billingTitle, string billingFirstnames,
            string billingPhone, string billingPostCode, string billingState, string billingSurname, string deliveryAddress1, string deliveryAddress2, string deliveryAddress3, string deliveryCity,
            string deliveryCountry, int deliveryCountryID, string deliveryTitle, string deliveryFirstnames, string deliveryPhone, string deliveryPostCode, string deliveryState, string deliverySurname,
            string deliveryNotes, string customerEMail, int? customerID, bool billingEqDelivery, bool subscription, bool detailsFor3rdParties, int basketID);
        tbl_Basket UpdateBasketCustomerID(int customerID, int basketID);
        tbl_Basket UpdateBasketDeliveryCountry(int countryID, int basketID);
        tbl_Basket UpdateBasketPostage(int? postageID, int basketID);
        tbl_Basket UpdateBasketDeliveryNotes(string notes, int basketID);
        tbl_BasketContent UpdateProductQuantity(int bContentID, int amount);

        bool SaveEUVATForBasket(int basketID, string EUVAT);

        bool DeleteCountry(int countryID);
        tbl_Country GetCountry(int countryID);
        tbl_Country GetCountry(int domainID, string countryName);
        tbl_Country GetDefaultCountry(int domainID);
        string GetCountryCode(int domainID, string countryName);
        SelectList GetAllCountriesAsSelectList(int domainID, int selectedCountryID = 0);
        tbl_Country SaveCountry(bool isDefault, string code, string countryName, int domainID, int? postageZoneID, int countryID);
        bool SaveCountriesOrder(int[] orderedCountryIDs);
        int ImportCountries(int sourceDomainID, int destDomainID);
        List<SelectListItem> GetPostageCountriesAsSelectList(int domainID, int selectedCountryID = 0);

        tbl_DonationInfo GetDonationInfoByID(int donationInfoID);
        bool DeleteDonationInfo(int donationInfoID);
        List<tbl_DonationInfo> GetAllDonationsInfoForDomain(int domainID, bool includeNotLive);
        List<tbl_DonationInfo> GetAllDonationsInfoForDomainByType(int domainID, DonationType type);
        tbl_DonationInfo SaveDonationInfo(tbl_DonationInfo donationInfo);
        tbl_DonationInfo UpdateDonationInfoImagePath(string path, int donationInfoID);

        List<tbl_DonationType> GetAllDonationType();

        bool DeleteProdAttribute(int prodAttributeID);
        List<tbl_ProdAttributes> GetAllProdAttributes();
        SelectList GetAllProdAttributesList();
        tbl_ProdAttributes GetProdAttributeByID(int prodAttributeID);
        tbl_ProdAttributes SaveProdAttribute(string title, int attributeID);

        bool DeleteProdAttValue(int prodAttValueID);
        List<tbl_ProdAttValue> GetAllProdAttValues();
        tbl_ProdAttValue GetByID(int prodAttValueID);
        tbl_ProdAttValue SaveProdAttValue(string value, int attributeValueID, int attributeID = 0);

        bool DeleteProdCategory(int prodCategoryID);
        List<tbl_ProdCategories> GetAllProdCategories();
        SelectList GetAllProdCategoriesAsSelectList();
        List<tbl_ProdCategories> GetProdCategoriesForDomain(int domainID, int parentID, bool includeDeleted, ProductType type);
        List<ExtendedSelectListItem> GetProdCategoriesForDomainAsSelectList(int domainID, int selectedCategoryID, ProductType type);
        tbl_ProdCategories GetProdCategoryByID(int prodCategoryID);
        List<tbl_ProdCategories> GetFeaturedProdCategories();
        tbl_ProdCategories SaveProdCategory(string title, bool live, int parentID, int? taxID, int categoryID, ProductType type, bool featured = false);
        tbl_ProdCategories SaveProdCategoryImage(int categoryID, int? imageID);
        bool SaveCategoriesOrder(int[] orderedCategoriesIDs);

        bool DeleteDiscount(int discountID);
        List<tbl_Discount> GetAllDiscounts(int domainID = 0);
        tbl_Discount GetDiscountByID(int discountID);
        tbl_Discount GetDiscountByCode(string code, int domainID);
        tbl_Discount SaveDiscount(decimal value, bool isPercentage, string code, string desc, string title, string start, string expire, int domainID, int discountID);

        bool DeletePostage(int postageID);
        List<tbl_Postage> GetAvailablePostage(decimal amount, decimal weight, int domainID, string postageSetting, int zoneID);
        SelectList GetAvailablePostageAsSelectList(decimal amount, decimal weight, int domainID, string postageSetting, int zoneID);
        tbl_Postage GetPostageByID(int postageID);
        tbl_Postage SavePostage(string description, decimal? amount, int PST_DomainID, int? PST_PostageBandID, int? PST_PostageWeightID, int PST_PostageZoneID, int postageID);

        bool DeletePostageBand(int postageBandID);
        IEnumerable<tbl_PostageBands> GetAllPostageBands(int domainID);
        IEnumerable<SelectListItem> GetAllPostageBandsAsSelectList(int domainID);
        tbl_PostageBands SavePostageBand(tbl_PostageBands band);

        bool DeletePostageWeight(int postageWeightID);
        IEnumerable<tbl_PostageWeights> GetAllPostageWeights(int domainID);
        IEnumerable<SelectListItem> GetAllPostageWeightsAsSelectList(int domainID);
        tbl_PostageWeights SavePostageWeight(tbl_PostageWeights postage);

        bool DeletePostageZone(int postageZoneID);
        tbl_PostageZones GetDefaultPostageZone(int domainID);
        int? GetPostageZoneID(int countryID);
        IEnumerable<tbl_PostageZones> GetAllPostageZones(int domainID);
        SelectList GetAllPostageZonesAsSelectList(int domainID);
        tbl_PostageZones SavePostageZone(tbl_PostageZones zone);

        bool AddProductAttrbiute(int productID, int attributeID);
        bool CreateStockUnitsMatrix(int productID, int[] prodAttValueIDs);
        bool DeleteAllProductStockUnits(int productID);
        bool DeleteProduct(int productID);
        bool DeleteProductAssociation(int productID, int asssociatedProductID);
        bool DeleteProductPrice(int priceID);
        bool DeleteProductPrice(int[] priceID);
        bool DeleteProductPriceTimeWindow(int priceTimeWindowID);
        List<tbl_Products> GetAllProducts();
        List<tbl_Products> GetAllProductsByType(ProductType type);
        List<tbl_Products> GetFeaturedProductsByType(ProductType type);
        tbl_Products GetProductByID(int productID);
        tbl_ProductPrice GetProductPrice(int productID, int[] attrs, string selectedDate);
        tbl_ProductPrice GetProductPriceByID(int productPriceID);
        tbl_ProductPriceTimeWindow GetProductPriceTimeWindowByID(int productPriceTimeWindowID);
        bool RemoveProductAttrbiute(int productID, int attributeID);
        List<tbl_Products> SearchProducts(string search, ProductType type);
        tbl_Products SaveAssociatedProduct(int productID, int associatedProductID);
        tbl_Products SaveProduct(decimal? basePrice, int? categoryID, string desc, DateTime lastModifiedDate, int? minQuantity,
            bool? offer, string productCode, int? groupID, string shortDesc, int? taxID, string title, bool live, bool stockControl,
            ProductType type, int? eventTypeID, bool deliverable, bool purchasable, bool featured, string affiliate, int productID);
        bool SaveProductOrder(int[] orderedProductsIDs);
        tbl_ProductPrice SaveProductStockUnit(int productID, int priceID, string barcode, string delivery, bool onSale, string sPrice,
            string sRRP, string sSalePrice, string SKU, int stock, string sWeight, string endDate, string startDate, string sPriceForRegular, int[] attrValueIDs = null);
        tbl_ProductPriceTimeWindow SaveProductPriceTimeWindow(string price, string startDate, string endDate, int productPriceID,  int productPriceTimeWindowID);
        tbl_Products SaveProductVisibility(int productID);

        bool DeleteProductImage(int imageID);
        tbl_ProductImages GetProductImageByID(int imageID);
        tbl_ProductImages SaveProductImage(int productID, string decsription, string name, bool primary, string view);

        tbl_ProdImageVerNames GetProductImageVersionByName(string name);

        tbl_Orders CancelOrder(int orderID);
        List<tbl_Orders> GetAllOrdersOfCustomer(int customerID);
        tbl_Orders GetOrderByID(int orderID);
        tbl_Orders GetOrderByVendorCode(string vendorTxCode, int domainID);
        tbl_Orders GetOrderBySecurityKey(string securityKey, int domainID);
        SelectList GetDespatchMethodes();
        tbl_DespatchMethod SaveNewDespathMethod(string method);
        tbl_Orders SaveCustomTotalAmount(decimal price, int orderID);
        tbl_Orders SaveOrder(int orderID, int? paymentTypeID, int? cashPayment, int basketID, int? adminID = null, int? addressID = null);
        tbl_Orders SaveOrderForDonation(int orderID, int domainID, string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, int billingCountryID,
                    string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string customerEMail, int? customerID, decimal amount,
                    bool giftAid, int paymentDomainID, DonationType type, int? parentOrderID);
        tbl_Orders UpdateOrderTracking(int despathMethodID, string deliveryDate, string trackingUrl, string trackingRef, int orderID);
        List<tbl_Orders> SearchOrders(string search, string startDate, string endDate, int? domainID, int? statusID, int? categoryID, int? productID, ProductType type = ProductType.AllProducts);
        tbl_Orders UpdateOrderStatus(int orderID, int statusID);
        tbl_Orders UpdateOrderPaymentStatus(int orderID, PaymentStatus status, string currencyCode = null);
        tbl_Orders UpdateOrderSecurityKey(string securityKey, int orderID);
        tbl_Orders UpdateOrderPayment(string vendorTxCode, string addressResult, string addressStatus, string avscv2, string cavv, string cv2Result, bool? giftAid, string postCodeResult,
            string last4Digits, string payerStatus, string securityKey, string status, long txAuthNo, string vpstxId, string threeDSecureStatus, string txType, string currencyCode, int orderID);
        tbl_Orders UpdateOrderVendorTxCodeAndStatus(int orderID, string vendorTxCode, string status = null);
        bool DeleteReferencedBasket(int orderID);
        bool IsEnoughOnStock(EntityCollection<tbl_BasketContent> entityCollection);

        bool CanAddTax(string name, decimal percentage, int taxID = 0);
        bool DeleteTax(int taxID);
        List<tbl_Tax> GetAllTaxes();
        List<SelectListItem> GetAllTaxesAsSelectList(int selectTaxID);
        tbl_Tax GetTaxByID(int taxID);
        tbl_Tax GetTaxByName(string name);
        tbl_Tax SaveTax(string title, decimal percentage, int taxID);

        string GetPaymentDomainLogo(int paymentID);
        tbl_PaymentDomain GetPaymentDomainByID(int paymentDomainID);
        bool DeletePaymentLogoImage(int paymentDomainID);
        int GetPaymentDomainIDByCode(int domainID, PaymentType code);
        List<tbl_PaymentDomain> GetAllPaymentsDomain(int domainID);
        List<SelectListItem> GetAllPaymentDomainAsSelectList(int domainID, bool? isLive, int selectPaymentTypeID = 0);
        string SavePaymentLogoImage(int paymentDomainID, string fileName);

        bool DeleteEventType(int eventTypeID);
        List<tbl_EventTypes> GetAllEventTypes();
        SelectList GetAllEventTypesAsSelectList(int selectedEventTypeID);
        tbl_EventTypes GetEventTypeByID(int eventTypeID);
        tbl_EventTypes GetEventTypeByName(string name);
        tbl_EventTypes SaveEventType(string title, string desc, int eventTypeID);
        tbl_EventTypes UpdateEventTypeImagePath(string path, int eventTypeID);


    }
}

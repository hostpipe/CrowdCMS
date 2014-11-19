using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;
using CMS.Utils.Extension;
using CMS.BL;
using CMS.Services.Extensions;
using System.Globalization;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;
using System.Web;
using System.Data.Objects.DataClasses;
using CMS.Utils.Diagnostics;
using System.Linq.Expressions;

namespace CMS.Services
{
    public class ECommerce : ServiceBase, IECommerce
    {
        private IAddressRepository AddressRepository { get; set; }
        private ICountryRepository CountryRepository { get; set; }
        private IContentTypeRepository ContentTypeRepository { get; set; }
        private IDonationInfoRepository DonationInfoRepository { get; set; }
        private IDonationTypeRepository DonationTypeRepository { get; set; }
        private IPostageBandsRepository PostageBandsRepository { get; set; }
        private IPostageRepository PostageRepository { get; set; }
        private IPostageWeightsRepository PostageWeightsRepository { get; set; }
        private IPostageZonesRepository PostageZonesRepository { get; set; }
        private IProdAttributesRepository ProdAttributesRepository { get; set; }
        private IProdAttValueRepository ProdAttValueRepository { get; set; }
        private IProdCategoriesRepository ProdCategoriesRepository { get; set; }
        private IProductImagesRepository ProductImagesRepository { get; set; }
        private IProdImageVerNamesRepository ProdImageVerNamesRepository { get; set; }
        private IProductPriceRepository ProductPriceRepository { get; set; }
        private IProductsRepository ProductsRepository { get; set; }
        private IDiscountRepository DiscountRepository { get; set; }
        private IBasketRepository BasketRepository { get; set; }
        private IBasketContentRepository BasketContentRepository { get; set; }
        private IOrdersRepository OrdersRepository { get; set; }
        private IOrderContentRepository OrderContentRepository { get; set; }
        private ITaxRepository TaxRepository { get; set; }
        private IPaymentDomainRepository PaymentDomainRepository { get; set; }
        private IEventTypesRepository EventTypesRepository { get; set; }
        private ICustomerRepository CustomerRepository { get; set; }
        private ISitemapRepository SitemapRepository { get; set; }

        public ECommerce()
            : base()
        {
            this.AddressRepository = new AddressRepository(this.Context);
            this.CountryRepository = new CountryRepository(this.Context);
            this.ContentTypeRepository = new ContentTypeRepository(this.Context);
            this.DonationInfoRepository = new DonationInfoRepository(this.Context);
            this.DonationTypeRepository = new DonationTypeRepository(this.Context);
            this.PostageBandsRepository = new PostageBandsRepository(this.Context);
            this.PostageRepository = new PostageRepository(this.Context);
            this.PostageWeightsRepository = new PostageWeightsRepository(this.Context);
            this.PostageZonesRepository = new PostageZonesRepository(this.Context);
            this.ProdAttributesRepository = new ProdAttributesRepository(this.Context);
            this.ProdAttValueRepository = new ProdAttValueRepository(this.Context);
            this.ProdCategoriesRepository = new ProdCategoriesRepository(this.Context);
            this.ProductImagesRepository = new ProductImagesRepository(this.Context);
            this.ProdImageVerNamesRepository = new ProdImageVerNamesRepository(this.Context);
            this.ProductPriceRepository = new ProductPriceRepository(this.Context);
            this.DiscountRepository = new DiscountRepository(this.Context);
            this.ProductsRepository = new ProductsRepository(this.Context);
            this.BasketRepository = new BasketRepository(this.Context);
            this.BasketContentRepository = new BasketContentRepository(this.Context);
            this.OrdersRepository = new OrdersRepository(this.Context);
            this.OrderContentRepository = new OrderContentRepository(this.Context);
            this.TaxRepository = new TaxRepository(this.Context);
            this.PaymentDomainRepository = new PaymentDomainRepository(this.Context);
            this.EventTypesRepository = new EventTypesRepository(this.Context);
            this.CustomerRepository = new CustomerRepository(this.Context);
            this.SitemapRepository = new SitemapRepository(this.Context);
        }

        #region Addresses repo

        public tbl_Address GetAddressByID(int ID)
        {
            return AddressRepository.GetAll().FirstOrDefault(m => m.AddressID == ID);
        }

        public List<tbl_Address> GetAllAddresses(int customerID)
        {
            return AddressRepository.GetAll().Where(m => m.A_CustomerID == customerID).ToList();
        }

        public List<SelectListItem> GetAllAddressesAsSelectList(int customerID, int countryID)
        {
            var addresses =
                (countryID == 0) ?
                    AddressRepository.GetAll().Where(m => m.A_CustomerID == customerID) :
                    AddressRepository.GetAll().Where(m => m.A_CustomerID == customerID && m.A_CountryID == countryID);
            return addresses.AsEnumerable().Select(n => new SelectListItem()
                {
                    Text = String.Format("{0} {1} {2}", n.A_Title, n.A_FirstName, n.A_Surname),
                    Value = n.AddressID.ToString()
                }).ToList();
        }

        public tbl_Address SaveAddress(int customerID, int countryID, string county, string firstName, string surName, string title,
            string addLine1, string addLine2, string addLine3, string postcode, string phone, string town, int addressID)
        {
            var country = CountryRepository.GetByID(countryID);
            if (country == null)
                return null;

            return AddressRepository.SaveAddress(customerID, country.C_Country, countryID, county, firstName, surName, title, addLine1, addLine2, addLine3, postcode, phone, town, addressID);
        }

        public bool DeleteAddress(int addressID)
        {
            return AddressRepository.DeleteAddress(addressID);
        }

        #endregion


        #region Basket

        public tbl_Basket AddContentToBasket(int basketID, int productID, int quantity, int[] attrs, string selectedDate)
        {
            var basket = BasketRepository.GetByID(basketID);
            if (basket == null)
                return null;

            var price = ProductsRepository.GetProductPrice(productID, attrs, selectedDate);
            if (price == null)
                return null;

            if (price.tbl_Products.P_ProductTypeID == (int)ProductType.Event &&
                (price.PR_EventEndDate.HasValue ? price.PR_EventEndDate.Value <= DateTime.Now : price.PR_EventStartDate.GetValueOrDefault() <= DateTime.Now))
                return null;

            if (price.PR_Stock.GetValueOrDefault(0) < quantity && price.tbl_Products.P_StockControl)
                return null;

            tbl_BasketContent content = null;
            var productBasket = basket.tbl_BasketContent.Where(m => m.tbl_ProductPrice.PR_ProductID == productID);
            if (productBasket != null)
            {
                var product = productBasket.FirstOrDefault(m => m.tbl_ProductPrice == price);
                if (product != null)
                {
                    quantity += product.BC_Quantity;
                    content = UpdateProductQuantity(product.BaskContentID, quantity);
                    return content != null ? content.tbl_Basket : null;
                }
            }

            content = BasketContentRepository.SaveBasketContent(basket.BasketID, false, PriceManager.GetBasePrice(price), price.PriceID, quantity,
                price.PR_SKU, price.tbl_Products.P_Title, price.tbl_Products.P_ShortDesc, price.tbl_Products.P_Description, 0);

            return content != null ? content.tbl_Basket : null;
        }

        public tbl_Basket AddDiscountToBasket(int basketID, string code, int domainID)
        {
            if (basketID == 0 || String.IsNullOrEmpty(code))
                return null;

            var discount = DiscountRepository.GetByCode(code, domainID);
            if (discount != null)
                return BasketRepository.UpdateDiscount(basketID, discount.DiscountID);

            return null;
        }

        public bool DeleteBasket(int basketID)
        {
            return BasketRepository.DeleteBasket(basketID);
        }

        public bool DeleteUnusedBaskets()
        {
            return BasketRepository.DeleteUnUsedBaskets();
        }

        public bool DeleteBasketContent(int basketContentID)
        {
            var basketContent = GetBasketContentByID(basketContentID);
            if (BasketContentRepository.DeleteBasketContent(basketContentID))
            {
                // reset postage after change in products
                UpdateBasketPostage(null, basketContent.BC_BasketID); 
                return true;
            }
            return false;
        }

        public tbl_Basket GetBasketByID(int basketID)
        {
            return BasketRepository.GetByID(basketID);
        }

        public tbl_BasketContent GetBasketContentByID(int basketContentID)
        {
            return BasketContentRepository.GetByID(basketContentID);
        }

        public tbl_Basket RemoveDiscountFromBasket(int basketID)
        {
            if (basketID == 0)
                return null;

            return BasketRepository.UpdateDiscount(basketID, null);
        }

        public tbl_Basket SaveBasket(int? customerID, int? discountID, string sessionID, int domainID, int basketID)
        {
            return BasketRepository.SaveBasket(customerID, discountID, sessionID, 1, domainID, basketID);
        }

        public tbl_Basket SaveBasketWithContent(int? customerID, int? discountID, string sessionID, int domainID, int basketID, int productID, int quantity, int[] attrs, string selectedDate)
        {
            var price = ProductsRepository.GetProductPrice(productID, attrs, selectedDate);
            if (price != null && (!price.tbl_Products.P_StockControl || price.PR_Stock.GetValueOrDefault(0) >= quantity))
            {
                var basket = BasketRepository.SaveBasket(customerID, discountID, sessionID, 1, domainID, basketID);
                if (basket != null)
                    AddContentToBasket(basket.BasketID, productID, quantity, attrs, selectedDate);

                return basket;
            }
            return null;
        }

        public bool SaveEUVATForBasket(int basketID, string EUVAT)
        {
            return BasketRepository.SaveEUVAT(basketID, EUVAT);
        }

        public tbl_Basket UpdateBasket(string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, int billingCountryID, string billingTitle, string billingFirstnames,
            string billingPhone, string billingPostCode, string billingState, string billingSurname, string deliveryAddress1, string deliveryAddress2, string deliveryAddress3, string deliveryCity,
            string deliveryCountry, int deliveryCountryID, string deliveryTitle, string deliveryFirstnames, string deliveryPhone, string deliveryPostCode, string deliveryState, string deliverySurname,
            string deliveryNotes, string customerEMail, int? customerID, bool billingEqDelivery, bool subscription, bool detailsFor3rdParties, int basketID)
        {
            var bCountry = CountryRepository.GetByID(billingCountryID);
            var dCountry = CountryRepository.GetByID(deliveryCountryID);

            return BasketRepository.UpdateDeliveryDetails(billingAddress1, billingAddress2, billingAddress3, billingCity, bCountry != null ? bCountry.C_Code : String.Empty, billingCountryID,
                billingTitle, billingFirstnames, billingPhone, billingPostCode, billingState, billingSurname, deliveryAddress1, deliveryAddress2, deliveryAddress3, deliveryCity,
                dCountry != null ? dCountry.C_Code : deliveryCountry, deliveryCountryID, deliveryTitle, deliveryFirstnames, deliveryPhone, deliveryPostCode, deliveryState, 
                deliverySurname, deliveryNotes, customerEMail, customerID, billingEqDelivery,subscription, detailsFor3rdParties, basketID);
        }

        public tbl_Basket UpdateBasketCustomerID(int customerID, int basketID)
        {
            return BasketRepository.UpdateCustomerID(customerID, basketID);
        }

        public tbl_Basket UpdateBasketDeliveryCountry(int countryID, int basketID)
        {
            var country = CountryRepository.GetByID(countryID);
            return (country != null) ?
                BasketRepository.UpdateDeliveryCountry(country.CountryID, country.C_Code, basketID) :
                BasketRepository.UpdateDeliveryCountry(null, String.Empty, basketID);
        }

        public tbl_Basket UpdateBasketPostage(int? postageID, int basketID)
        {
            if (postageID.HasValue && postageID.Value == 0)
                postageID = null;

            return BasketRepository.UpdateBasketPostage(postageID, basketID);
        }

        public tbl_Basket UpdateBasketDeliveryNotes(string notes, int basketID)
        {
            return BasketRepository.UpdateDeliveryNotes(notes, basketID);
        }

        public tbl_BasketContent UpdateProductQuantity(int bContentID, int quantity)
        {
            var basketContent = BasketContentRepository.GetByID(bContentID);
            if (basketContent == null)
                return null;
            if (basketContent.tbl_ProductPrice.tbl_Products.P_StockControl)
            {
                var maxQuantity = basketContent.tbl_ProductPrice.PR_Stock;
                if (maxQuantity.GetValueOrDefault(0) < quantity)
                    return null;
            }

            basketContent = BasketContentRepository.SaveBasketContent(basketContent.BC_BasketID, basketContent.BC_NotAvailable, basketContent.BC_Price, basketContent.BC_ProdPriceID, quantity, basketContent.BC_SKU, basketContent.BC_Title, basketContent.BC_Descripiton, string.Empty, basketContent.BaskContentID);
            return basketContent;
        }

        #endregion


        #region Country

        public bool DeleteCountry(int countryID)
        {
            return CountryRepository.Delete(countryID);
        }

        public SelectList GetAllCountriesAsSelectList(int domainID, int selectedCountryID = 0)
        {
            return new SelectList(CountryRepository.GetAllForDomain(domainID).OrderBy(c => c.C_Order), "CountryID", "C_Country", selectedCountryID);
        }

        public List<SelectListItem> GetPostageCountriesAsSelectList(int domainID, int selectedCountryID = 0)
        {
            return CountryRepository.GetAllForDomainWithPostage(domainID).OrderBy(c => c.C_Order).AsEnumerable().Select(n => new SelectListItem()
            {
                Text = n.C_Country,
                Value = n.CountryID.ToString(),
                Selected = (selectedCountryID != 0 && n.CountryID == selectedCountryID) || (selectedCountryID == 0 && n.C_IsDefault) ? true : false
            }).ToList();
        }

        public tbl_Country GetCountry(int countryID)
        {
            return CountryRepository.GetByID(countryID);
        }

        public tbl_Country GetCountry(int domainID, string countryName)
        {
            return CountryRepository.GetCountry(domainID, countryName);
        }

        public string GetCountryCode(int domainID, string countryName)
        {
            return CountryRepository.GetCountryCode(domainID, countryName);
        }

        public tbl_Country GetDefaultCountry(int domainID)
        {
            return CountryRepository.GetDefaultForDomain(domainID);
        }

        public tbl_Country SaveCountry(bool isDefault, string code, string countryName, int domainID, int? postageZoneID, int countryID)
        {
            if (domainID == 0 || (postageZoneID.HasValue && postageZoneID == 0))
                return null;

            return CountryRepository.Save(isDefault, code, countryName, domainID, postageZoneID, countryID);
        }

        public bool SaveCountriesOrder(int[] orderedCountryIDs)
        {
            if (orderedCountryIDs == null)
                return false;

            return CountryRepository.SaveOrder(orderedCountryIDs);
        }

        public int ImportCountries(int sourceDomainID, int destDomainID)
        {

            return (sourceDomainID != 0) ?
                CountryRepository.ImportCountries(sourceDomainID, destDomainID) :
                CountryRepository.ImportCountriesFromBase(destDomainID);
        }

        #endregion


        #region Discount

        public bool DeleteDiscount(int discountID)
        {
            return DiscountRepository.DeleteDiscount(discountID);
        }

        public List<tbl_Discount> GetAllDiscounts(int domainID = 0)
        {
            return DiscountRepository.GetAll().Where(d => domainID == 0 || d.D_DomainID == domainID).ToList();
        }

        public tbl_Discount GetDiscountByID(int discountID)
        {
            return DiscountRepository.GetByID(discountID);
        }

        public tbl_Discount GetDiscountByCode(string code, int domainID)
        {
            return DiscountRepository.GetByCode(code, domainID);
        }

        public tbl_Discount SaveDiscount(decimal value, bool isPercentage, string code, string desc, string title, string start, string expire, int domainID, int discountID)
        {
            if (domainID == 0 || String.IsNullOrEmpty(code) || string.IsNullOrEmpty(title))
                return null;

            DateTime dExpire, dStart;
            bool eParsed = expire.TryParseDate(out dExpire);
            bool sParsed = start.TryParseDate(out dStart);

            return DiscountRepository.SaveDiscount(value, isPercentage, code, desc, title, sParsed ? dStart : (DateTime?)null, eParsed ? dExpire : (DateTime?)null, domainID, discountID);
        }

        #endregion


        #region DonationInfo

        public tbl_DonationInfo GetDonationInfoByID(int donationInfoID)
        {
            return DonationInfoRepository.GetByID(donationInfoID);
        }

        public bool DeleteDonationInfo(int donationInfoID)
        {
            return DonationInfoRepository.Delete(donationInfoID);
        }

        public List<tbl_DonationInfo> GetAllDonationsInfoForDomain(int domainID, bool includeNotLive)
        {
            if (includeNotLive == false)
                return DonationInfoRepository.GetAllForDomain(domainID).Where(m => m.DI_Live == true).ToList();
            return DonationInfoRepository.GetAllForDomain(domainID).ToList();
        }

        public List<tbl_DonationInfo> GetAllDonationsInfoForDomainByType(int domainID, DonationType type)
        {
            string sType = type.ToString();
            return DonationInfoRepository.GetAllForDomain(domainID).Where(m => m.DI_Live == true && m.tbl_DonationType.DT_Name.Equals(sType)).ToList();
        }

        public tbl_DonationInfo SaveDonationInfo(tbl_DonationInfo donationInfo)
        {
            return DonationInfoRepository.Save(donationInfo);
        }

        public List<tbl_DonationType> GetAllDonationType()
        {
            return DonationTypeRepository.GetAll().ToList();
        }

        public tbl_DonationInfo UpdateDonationInfoImagePath(string path, int donationInfo)
        {
            if (donationInfo == 0)
                return null;

            return DonationInfoRepository.SaveImagePath(path, donationInfo);
        }


        #endregion DonationInfo


        #region Postage

        public bool DeletePostage(int postageID)
        {
            return PostageRepository.Delete(postageID);
        }

        public tbl_PostageZones GetDefaultPostageZone(int domainID)
        {
            return PostageZonesRepository.GetAll().FirstOrDefault(m => m.PZ_DomainID == domainID && m.PZ_IsDefault == true);
        }

        public List<tbl_Postage> GetAvailablePostage(decimal amount, decimal weight, int domainID, string postageSetting, int zoneID)
        {
            List<tbl_Postage> postage = null;
            if (postageSetting == PostageType.Weight.ToString())
                postage = PostageRepository.GetAll().Where(m => m.PST_PostageZoneID == zoneID && m.PST_DomainID == domainID &&
                    (m.PST_PostageWeightID != null &&
                    m.tbl_PostageWeights.PW_Lower <= weight &&
                    m.tbl_PostageWeights.PW_Upper >= weight) ||
                    (m.PST_PostageWeightID == null && m.PST_PostageBandID == null)).OrderBy(m => m.PST_Amount).ToList();
            else if (postageSetting == PostageType.Band.ToString())
                postage = PostageRepository.GetAll().Where(m => m.PST_PostageZoneID == zoneID && m.PST_DomainID == domainID &&
                    ((m.PST_PostageBandID != null &&
                    m.tbl_PostageBands.PB_Lower <= amount &&
                    m.tbl_PostageBands.PB_Upper >= amount) ||
                    (m.PST_PostageWeightID == null && m.PST_PostageBandID == null))).OrderBy(m => m.PST_Amount).ToList();
            return postage;
        }

        public SelectList GetAvailablePostageAsSelectList(decimal amount, decimal weight, int domainID, string postageSetting, int zoneID)
        {
            var postages = GetAvailablePostage(amount, weight, domainID, postageSetting, zoneID).Select(p => new SelectListItem()
            {
                Text = String.Format("{0} {1:C}", p.PST_Description, p.PST_Amount),
                Value = p.PostageID.ToString()
            });
            return new SelectList(postages, "Value", "Text");
        }

        public tbl_Postage GetPostageByID(int postageID)
        {
            return PostageRepository.GetByID(postageID);
        }

        public tbl_Postage SavePostage(string description, decimal? amount, int PST_DomainID, int? PST_PostageBandID, int? PST_PostageWeightID, int PST_PostageZoneID, int postageID)
        {
            return PostageRepository.Save(description, amount, PST_DomainID, PST_PostageBandID, PST_PostageWeightID, PST_PostageZoneID, postageID);
        }

        public bool DeletePostageBand(int postageBandID)
        {
            return PostageBandsRepository.Delete(postageBandID);
        }

        public IEnumerable<tbl_PostageBands> GetAllPostageBands(int domainID = 0)
        {
            return PostageBandsRepository.GetAll().Where(m => m.PB_DomainID == domainID).OrderBy(b => b.PB_Lower).ThenBy(b => b.PB_Upper).ToList();
        }

        public IEnumerable<SelectListItem> GetAllPostageBandsAsSelectList(int domainID = 0)
        {
            return PostageBandsRepository.GetAll()
                .Where(m => m.PB_DomainID == domainID).OrderBy(b => b.PB_Lower).ThenBy(b => b.PB_Upper).AsEnumerable()
                .Select(m => new SelectListItem()
                {
                    Text = String.Format("{0} - {1}", m.PB_Lower, m.PB_Upper),
                    Value = m.PostageBandID.ToString()
                });
        }

        public tbl_PostageBands SavePostageBand(tbl_PostageBands postageBand)
        {
            return PostageBandsRepository.Save(postageBand);
        }

        public bool DeletePostageWeight(int postageWeightID)
        {
            return PostageWeightsRepository.Delete(postageWeightID);
        }

        public IEnumerable<tbl_PostageWeights> GetAllPostageWeights(int domainID = 0)
        {
            return PostageWeightsRepository.GetAll().Where(m => m.PW_DomainID == domainID).OrderBy(w => w.PW_Lower).ThenBy(w => w.PW_Upper).ToList();
        }

        public IEnumerable<SelectListItem> GetAllPostageWeightsAsSelectList(int domainID = 0)
        {
            return PostageWeightsRepository.GetAll()
                .Where(m => m.PW_DomainID == domainID).OrderBy(w => w.PW_Lower).ThenBy(w => w.PW_Upper).AsEnumerable()
                .Select(m => new SelectListItem()
                {
                    Text = String.Format("{0} - {1}", m.PW_Lower, m.PW_Upper),
                    Value = m.PostageWeightID.ToString()
                });
        }

        public tbl_PostageWeights SavePostageWeight(tbl_PostageWeights postageWeight)
        {
            return PostageWeightsRepository.Save(postageWeight);
        }

        public bool DeletePostageZone(int postageZoneID)
        {
            return PostageZonesRepository.Delete(postageZoneID);
        }

        public int? GetPostageZoneID(int countryID)
        {
            var zone = CountryRepository.GetByID(countryID);
            return zone != null ? zone.C_PostageZoneID : 0;
        }


        public IEnumerable<tbl_PostageZones> GetAllPostageZones(int domainID = 0)
        {
            return PostageZonesRepository.GetAll().Where(m => m.PZ_DomainID == domainID).OrderBy(z => z.PZ_Name).ToList();
        }

        public SelectList GetAllPostageZonesAsSelectList(int domainID = 0)
        {
            return new SelectList(PostageZonesRepository.GetAll().Where(m => m.PZ_DomainID == domainID).OrderBy(z => z.PZ_Name), "PostageZoneID", "PZ_Name");
        }

        public tbl_PostageZones SavePostageZone(tbl_PostageZones postageZone)
        {
            return PostageZonesRepository.Save(postageZone);
        }

        #endregion Postage


        #region Product Attributes

        public bool DeleteProdAttribute(int prodAttributeID)
        {
            return ProdAttributesRepository.DeleteProdAttribute(prodAttributeID);
        }

        public List<tbl_ProdAttributes> GetAllProdAttributes()
        {
            return ProdAttributesRepository.GetAll().ToList();
        }

        public SelectList GetAllProdAttributesList()
        {
            return new SelectList(ProdAttributesRepository.GetAll().OrderBy(pa => pa.A_Title), "AttributeID", "A_Title");
        }

        public tbl_ProdAttributes GetProdAttributeByID(int prodAttributeID)
        {
            return ProdAttributesRepository.GetByID(prodAttributeID);
        }

        public tbl_ProdAttributes SaveProdAttribute(string title, int attributeID)
        {
            if (String.IsNullOrEmpty(title))
                return null;

            return ProdAttributesRepository.SaveProdAttribute(title, attributeID);
        }

        #endregion


        #region Product Attribute Values

        public bool DeleteProdAttValue(int prodAttValueID)
        {
            return ProdAttValueRepository.DeleteProdAttValue(prodAttValueID);
        }

        public List<tbl_ProdAttValue> GetAllProdAttValues()
        {
            return ProdAttValueRepository.GetAll().ToList();
        }

        public tbl_ProdAttValue GetByID(int prodAttValueID)
        {
            return ProdAttValueRepository.GetByID(prodAttValueID);
        }

        public tbl_ProdAttValue SaveProdAttValue(string value, int attributeValueID, int attributeID = 0)
        {
            return ProdAttValueRepository.SaveProdAttValue(value, 0, attributeValueID, attributeID);
        }

        #endregion


        #region Product Categories

        public bool DeleteProdCategory(int prodCategoryID)
        {
            return ProdCategoriesRepository.DeleteProdCategory(prodCategoryID);
        }

        public List<tbl_ProdCategories> GetAllProdCategories()
        {
            return ProdCategoriesRepository.GetAll().Where(c => !c.PC_Deleted).ToList();
        }

        public SelectList GetAllProdCategoriesAsSelectList()
        {
            return new SelectList(ProdCategoriesRepository.GetAll().Where(c => !c.PC_Deleted).ToList(), "CategoryID", "PC_Title");
        }

        public List<tbl_ProdCategories> GetProdCategoriesForDomain(int domainID, int parentID, bool includeDeleted, ProductType type = ProductType.AllProducts)
        {
            var query = ProdCategoriesRepository.GetAll().Where(pc => pc.tbl_SiteMap.SM_DomainID == domainID && pc.PC_Live &&
                                 pc.tbl_SiteMap.tbl_Content.Any(c => c.C_Approved && !c.C_Deleted));
            if (!includeDeleted)
                query = query.Where(pc => pc.PC_Deleted == false);
            return (type != ProductType.AllProducts) ?
               query.OrderBy(pc => pc.PC_Order).ToList()
                   .Where(pc => pc.PC_ParentID.GetValueOrDefault(0) == parentID && pc.tbl_ProductTypes.PT_Name == type.ToString()).ToList() :
               query.OrderBy(pc => pc.PC_Order).ToList()
                   .Where(pc => pc.PC_ParentID.GetValueOrDefault(0) == parentID).ToList();
        }

        public List<ExtendedSelectListItem> GetProdCategoriesForDomainAsSelectList(int domainID, int selectedCategoryID, ProductType type = ProductType.Item)
        {
            List<ExtendedSelectListItem> categories = new List<ExtendedSelectListItem>();
            var query = ProdCategoriesRepository.GetAll().Where(m => !m.PC_Deleted && (type == ProductType.AllProducts || m.PC_ProductTypeID == (int)type) && (domainID == 0 || m.tbl_SiteMap.SM_DomainID == domainID)).OrderBy(c => c.PC_Order);

            categories.AddRange(CategoriesChildItems(query.AsEnumerable(), null, selectedCategoryID, 1));
            return categories;
        }

        public tbl_ProdCategories GetProdCategoryByID(int prodCategoryID)
        {
            return ProdCategoriesRepository.GetByID(prodCategoryID);
        }

        public List<tbl_ProdCategories> GetFeaturedProdCategories()
        {
            return ProdCategoriesRepository.GetAll().Where(pc => pc.PC_Live && pc.PC_Featured && !pc.PC_Deleted).ToList();
        }

        public tbl_ProdCategories SaveProdCategory(string title, bool live, int parentID, int? taxID, int categoryID, ProductType type, bool featured = false)
        {
            return ProdCategoriesRepository.SaveProdCategory(title, live, parentID != 0 ? parentID : (int?)null, taxID, categoryID, type, featured);
        }

        public tbl_ProdCategories SaveProdCategoryImage(int categoryID, int? imageID)
        {
            return ProdCategoriesRepository.SaveProdCategoryImage(categoryID, imageID);
        }

        public bool SaveCategoriesOrder(int[] orderedCategoriesIDs)
        {
            return ProdCategoriesRepository.SaveOrder(orderedCategoriesIDs);
        }

        #endregion


        #region Product

        public bool AddProductAttrbiute(int productID, int attributeID)
        {
            return ProductsRepository.AddAttrbiute(productID, attributeID);
        }

        public bool CreateStockUnitsMatrix(int productID, int[] prodAttValueIDs)
        {
            if (prodAttValueIDs == null || prodAttValueIDs.Count() == 0)
                return false;

            var product = ProductsRepository.GetByID(productID);
            if (product == null)
                return false;

            var attributeValues = product.tbl_ProdAttLink.SelectMany(pal => pal.tbl_ProdAttributes.tbl_ProdAttValue.Where(pav => prodAttValueIDs.Contains(pav.AttributeValueID))).ToList();
            CreateStockUnit(productID, attributeValues, new List<int>());
            return true;
        }

        public bool DeleteAllProductStockUnits(int productID)
        {
            if (productID == 0)
                return false;

            return ProductPriceRepository.DeleteAllStockUnits(productID);
        }

        public bool DeleteProduct(int productID)
        {
            return ProductsRepository.DeleteProduct(productID);
        }

        public bool DeleteProductAssociation(int productID, int asssociatedProductID)
        {
            return ProductsRepository.DeleteProductAssociation(productID, asssociatedProductID);
        }

        public bool DeleteProductPrice(int priceID)
        {
            return ProductPriceRepository.DeletePrice(priceID);
        }

        public bool DeleteProductPrice(int[] priceID)
        {
            return ProductPriceRepository.DeletePrice(priceID);
        }

        public bool DeleteProductPriceTimeWindow(int priceTimeWindowID)
        {
            return ProductPriceRepository.DeletePriceTimeWindow(priceTimeWindowID);
        }

        public List<tbl_Products> GetAllProducts()
        {
            return ProductsRepository.GetAll().ToList();
        }

        public List<tbl_Products> GetAllProductsByType(ProductType type)
        {
            return ProductsRepository.GetAllByType(type).ToList();
        }

        public List<tbl_Products> GetFeaturedProductsByType(ProductType type)
        {
            return ProductsRepository.GetAllByType(type).Where(p => p.P_Featured == true).ToList();
        }

        public tbl_Products GetProductByID(int productID)
        {
            return ProductsRepository.GetByID(productID);
        }

        public tbl_ProductPrice GetProductPrice(int productID, int[] attrs, string selectedDate)
        {
            if (productID == 0)
                return null;

            return ProductsRepository.GetProductPrice(productID, attrs, selectedDate);
        }

        public tbl_ProductPrice GetProductPriceByID(int productPriceID)
        {
            return ProductPriceRepository.GetByID(productPriceID);
        }

        public tbl_ProductPriceTimeWindow GetProductPriceTimeWindowByID(int productPriceTimeWindowID)
        {
            return ProductPriceRepository.GetTimeWindowByID(productPriceTimeWindowID);
        }

        public bool RemoveProductAttrbiute(int productID, int attributeID)
        {
            return ProductsRepository.RemoveAttrbiute(productID, attributeID);
        }

        public List<tbl_Products> SearchProducts(string search, ProductType type)
        {
            var products = ProductsRepository.GetAllByType(type);

            if (string.IsNullOrEmpty(search))
                return products.ToList();

            return products.Where(p => p.P_Title.Contains(search) || p.P_ProductCode.Contains(search)).ToList();
        }

        public tbl_Products SaveAssociatedProduct(int productID, int associatedProductID)
        {
            return ProductsRepository.SaveAssociatedProduct(productID, associatedProductID);
        }

        public tbl_Products SaveProduct(decimal? basePrice, int? categoryID, string desc, DateTime lastModifiedDate, int? minQuantity,
            bool? offer, string productCode, int? groupID, string shortDesc, int? taxID, string title, bool live, bool stockControl,
            ProductType type, int? eventTypeID, bool deliverable, bool purchasable, bool featured, string affiliate, int productID)
        {
            if (String.IsNullOrEmpty(title) || String.IsNullOrEmpty(productCode) || categoryID == 0)
                return null;

            return ProductsRepository.SaveProduct(basePrice, categoryID, desc, lastModifiedDate, minQuantity, offer, productCode, groupID, shortDesc, taxID, title, live, stockControl, type, eventTypeID, deliverable, purchasable, featured, affiliate, productID);
        }

        public bool SaveProductOrder(int[] orderedProductsIDs)
        {
            return ProductsRepository.SaveOrder(orderedProductsIDs);
        }

        public tbl_ProductPrice SaveProductStockUnit(int productID, int priceID, string barcode, string delivery, bool onSale, string sPrice,
            string sRRP, string sSalePrice, string SKU, int stock, string sWeight, string endDate, string startDate, string spriceForRegular, int[] attrValueIDs = null)
        {
            if (productID == 0)
                return null;

            decimal price = 0, RRP = 0, salePrice = 0, weight = 0, priceForRegular = 0;
            decimal.TryParse(sPrice.ChangeDecimalSeparator(), out price);
            decimal.TryParse(sRRP.ChangeDecimalSeparator(), out RRP);
            decimal.TryParse(sSalePrice.ChangeDecimalSeparator(), out salePrice);
            decimal.TryParse(sWeight.ChangeDecimalSeparator(), out weight);
            decimal.TryParse(sPrice.ChangeDecimalSeparator(), out priceForRegular);

            DateTime eDate, sDate;
            bool eParsed = endDate.TryParseDateTime(out eDate);
            bool sParsed = startDate.TryParseDateTime(out sDate);

            var dbPrice = ProductPriceRepository.SaveStockUnit(productID, barcode, delivery, onSale, price, RRP, salePrice, SKU, stock, weight, eParsed ? eDate : (DateTime?)null, sParsed ? sDate : (DateTime?)null, priceID, priceForRegular);

            if (attrValueIDs != null)
                ProductPriceRepository.SaveAttrValueForStockUnit(dbPrice.PriceID, attrValueIDs);

            return dbPrice;
        }

        public tbl_ProductPriceTimeWindow SaveProductPriceTimeWindow(string sPrice, string startDate, string endDate, int productPriceID, int productPriceTimeWindowID)
        {
            if (productPriceID == 0)
                return null;

            decimal price = 0;
            decimal.TryParse(sPrice.ChangeDecimalSeparator(), out price);

            DateTime eDate, sDate;
            bool eParsed = endDate.TryParseDateTime(out eDate);
            bool sParsed = startDate.TryParseDateTime(out sDate);

            return ProductPriceRepository.SavePriceForTimeWindow(price, sDate, eParsed ? eDate : (DateTime?)null, productPriceID, productPriceTimeWindowID);
        }

        public tbl_Products SaveProductVisibility(int productID)
        {
            return ProductsRepository.SaveVisibility(productID);
        }

        public bool IsEnoughOnStock(EntityCollection<tbl_BasketContent> basketContent)
        {
            return ProductPriceRepository.IsEnoughOnStock(basketContent);
        }

        #endregion


        #region Product Images

        public bool DeleteProductImage(int imageID)
        {
            return this.ProductImagesRepository.DeleteImage(imageID);
        }

        public tbl_ProductImages GetProductImageByID(int imageID)
        {
            return this.ProductImagesRepository.GetByID(imageID);
        }

        public tbl_ProductImages SaveProductImage(int productID, string decsription, string name, bool primary, string view)
        {
            return this.ProductImagesRepository.SaveImage(productID, decsription, name, primary, view);
        }

        #endregion


        #region Product Image Version Names

        public tbl_ProdImageVerNames GetProductImageVersionByName(string name)
        {
            return ProdImageVerNamesRepository.GetVersionByName(name);
        }

        #endregion


        #region Orders

        public tbl_Orders CancelOrder(int orderID)
        {
            var order = OrdersRepository.GetByID(orderID);
            if (order == null)
                return null;

            if (order.CurrentOrderStatus == OrderStatus.Canceled)
                return null;

            if (order.CurrentOrderStatus != OrderStatus.Commited && order.CurrentOrderStatus != OrderStatus.PaymentFailed)
                IncreaseStock(order);

            return OrdersRepository.CancelOrder(orderID);
        }

        public bool DeleteReferencedBasket(int orderID)
        {
            var basket = OrdersRepository.GetReferencedBasket(orderID);
            return basket != null ? BasketRepository.DeleteBasket(basket.BasketID) : false;
        }

        public List<tbl_Orders> GetAllOrdersOfCustomer(int customerID)
        {
            return OrdersRepository.GetAll().Where(o => o.CustomerID == customerID).ToList().Where(o => !o.Canceled).ToList();
        }

        public tbl_Orders GetOrderByID(int orderID)
        {
            return OrdersRepository.GetByID(orderID);
        }

        public tbl_Orders GetOrderByVendorCode(string vendorTxCode, int domainID)
        {
            return OrdersRepository.GetByVendorCode(vendorTxCode, domainID);
        }

        public tbl_Orders GetOrderBySecurityKey(string securityKey, int domainID)
        {
            return OrdersRepository.GetBySecurityKey(securityKey, domainID);
        }

        public SelectList GetDespatchMethodes()
        {
            return new SelectList(OrdersRepository.GetDespatchMethodes(), "DespatchMethodID", "DM_Name");
        }

        public tbl_DespatchMethod SaveNewDespathMethod(string method)
        {
            if (String.IsNullOrEmpty(method))
                return null;

            return OrdersRepository.SaveNewDespathMethod(method);
        }

        public tbl_Orders SaveCustomTotalAmount(decimal price, int orderID)
        {
            return OrdersRepository.SaveCustomTotalAmount(price, orderID);
        }

        public tbl_Orders SaveOrder(int orderID, int? paymentTypeID, int? cashPayment, int basketID, int? adminID = null, int? addressID = null)
        {
            tbl_Basket basket = BasketRepository.GetByID(basketID);
            tbl_Orders order = null;

            int customerID = 0;
            if (basket.B_CustomerID != null)
                customerID = (int)basket.B_CustomerID;

            if (basket != null)
            {
                if (!String.IsNullOrEmpty(basket.B_CustomerEMail))
                {
                    tbl_Customer customer = CustomerRepository.SaveCustomer(basket.B_CustomerEMail, basket.B_BillingFirstnames, basket.B_BillingSurname, basket.B_BillingPhone, 
                        basket.B_BillingTitle, basket.B_DomainID, customerID, false, basket.B_DetailsFor3rdParties, "");

                    if (customer == null)
                        basket.B_CustomerID = null;
                    else
                    {
                        basket.B_CustomerID = customer.CustomerID;

                        if (basket.B_Subscription)
                            CustomerRepository.SetSubscription(customer.CustomerID, true);

                        if (addressID.HasValue)
                        {
                            SaveAddress(customer.CustomerID, basket.B_BillingCountryID.GetValueOrDefault(0), basket.B_BillingState, basket.B_BillingFirstnames, basket.B_BillingSurname, basket.B_BillingTitle, 
                                basket.B_BillingAddress1, basket.B_BillingAddress2, basket.B_BillingAddress3, basket.B_BillingPostCode, basket.B_BillingPhone, basket.B_BillingCity, addressID.Value);
                        }
                    }
                }

                decimal totalTaxAmount = Math.Round(basket.GetTaxAmount(), 2);
                decimal totalDeliveryAmount = Math.Round(basket.GetDeliveryAmount(true), 2);
                decimal deliveryTaxAmount = Math.Round(basket.GetDeliveryTaxAmount(), 2);
                decimal totalDiscount = Math.Round(basket.GetDiscountAmount(), 2);
                decimal totalAmount = Math.Round(basket.GetPrice(true), 2);

                order = basket.B_BillingEqDelivery && basket.IsDeliverable ?
                    OrdersRepository.SaveOrder(orderID, basket.B_DomainID, Math.Round(basket.GetPrice(), 2), basket.B_DeliveryAddress1, basket.B_DeliveryAddress2, basket.B_DeliveryAddress3,
                        basket.B_DeliveryCity, basket.B_DeliveryCountry, basket.B_DeliveryFirstnames, basket.B_DeliveryPhone, basket.B_DeliveryPostCode, basket.B_DeliveryState, basket.B_DeliverySurname,
                        basket.B_CustomerEMail, basket.B_CustomerID, basket.B_DeliveryAddress1, basket.B_DeliveryAddress2, basket.B_DeliveryAddress3, basket.B_DeliveryCity, basket.B_DeliveryCountry, 
                        basket.B_DeliveryFirstnames, basket.B_DeliveryPhone, basket.B_DeliveryPostCode, basket.B_DeliveryState, basket.B_DeliverySurname, basket.B_DiscountID, basket.B_DeliveryNotes,
                        paymentTypeID, cashPayment, totalTaxAmount, totalDeliveryAmount, Math.Round(deliveryTaxAmount, 2), Math.Round(basket.GetDeliveryAmount(false), 2), totalDiscount, totalAmount, 
                        basket.B_VATNumber, adminID) :
                    OrdersRepository.SaveOrder(orderID, basket.B_DomainID, Math.Round(basket.GetPrice(), 2), basket.B_BillingAddress1, basket.B_BillingAddress2, basket.B_BillingAddress3,
                        basket.B_BillingCity, basket.B_BillingCountry, basket.B_BillingFirstnames, basket.B_BillingPhone, basket.B_BillingPostCode, basket.B_BillingState, basket.B_BillingSurname,
                        basket.B_CustomerEMail, basket.B_CustomerID, basket.B_DeliveryAddress1, basket.B_DeliveryAddress2, basket.B_DeliveryAddress3, basket.B_DeliveryCity, basket.B_DeliveryCountry, 
                        basket.B_DeliveryFirstnames, basket.B_DeliveryPhone, basket.B_DeliveryPostCode, basket.B_DeliveryState, basket.B_DeliverySurname, basket.B_DiscountID, basket.B_DeliveryNotes,
                        paymentTypeID, cashPayment, totalTaxAmount, totalDeliveryAmount, Math.Round(deliveryTaxAmount, 2), Math.Round(basket.GetDeliveryAmount(false), 2), totalDiscount, totalAmount, 
                        basket.B_VATNumber, adminID);

                if (order != null)
                {
                    order = OrdersRepository.UpdateOrderStatus(order.OrderID, (int)OrderStatus.Commited);
                    tbl_OrderContent lastOrderContent = null;
                    foreach (var content in basket.tbl_BasketContent)
                    {
                        string seed = string.Empty;
                        string attributes = content.tbl_ProductPrice.tbl_ProdPriceAttributes
                            .Select(ppa => new
                            {
                                name = ppa.tbl_ProdAttValue.tbl_ProdAttributes.A_Title,
                                value = ppa.tbl_ProdAttValue.AV_Value
                            }).Aggregate(seed, (total, item) => (total += String.Format("{0}:{1};", item.name, item.value)));
                        if (content.tbl_ProductPrice.tbl_Products.tbl_ProductTypes.PT_Name == ProductType.Event.ToString())
                        {
                            attributes += (content.tbl_ProductPrice.PR_EventEndDate.HasValue) ?
                                String.Format("{0}-{1};", content.tbl_ProductPrice.PR_EventStartDate, content.tbl_ProductPrice.PR_EventEndDate) :
                                String.Format("{0};", content.tbl_ProductPrice.PR_EventStartDate);
                        }
                        lastOrderContent = OrderContentRepository.SaveOrderContent(order.OrderID, 0, content.BC_Description, Math.Round(content.BC_Price, 2), content.BC_ProdPriceID, 
                            (short?)content.BC_Quantity, content.BC_SKU, Math.Round(content.GetTaxValue(), 2), content.BC_Title, 0, Math.Round(content.GetPrice(true), 2), 
                            content.tbl_ProductPrice.PR_ProductID, attributes);
                    }
                    BasketRepository.SaveOrderReference(basketID, order.OrderID);
                    DecreaseStock(lastOrderContent.tbl_Orders);
                }
            }

            return order;
        }

        public tbl_Orders SaveOrderForDonation(int orderID, int domainID, string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, int billingCountryID, 
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string customerEMail, int? customerID, decimal amount,
            bool giftAid, int paymentDomainID, DonationType type, int? parentOrderID)
        {
            decimal totalTaxAmount = Math.Round((decimal)0, 2);
            //decimal totalDeliveryAmount = Math.Round((decimal)0, 2);
            //decimal deliveryTaxAmount = Math.Round((decimal)0, 2);
            //decimal totalDiscount = Math.Round((decimal)0, 2);
            decimal totalAmount = Math.Round(amount, 2);

            var country = CountryRepository.GetByID(billingCountryID);

            var orderDonation = OrdersRepository.SaveOrderAsDonation(orderID, domainID, totalAmount, billingAddress1, billingAddress2, billingAddress3, billingCity, 
                country != null ? country.C_Code : String.Empty, billingFirstnames, billingPhone, billingPostCode, billingState, billingSurname, customerEMail, 
                customerID, paymentDomainID, giftAid, totalTaxAmount, totalAmount, type, parentOrderID);

            if (orderDonation != null)
                orderDonation = OrdersRepository.UpdateOrderStatus(orderDonation.OrderID, (int)OrderStatus.Commited);

            return orderDonation;
        }

        public tbl_Orders UpdateOrderTracking(int despathMethodID, string deliveryDate, string trackingUrl, string trackingRef, int orderID)
        {
            if (orderID == 0)
                return null;

            return OrdersRepository.UpdateOrderTracking(despathMethodID, deliveryDate, trackingUrl, trackingRef, orderID);
        }

        public tbl_Orders UpdateOrderVendorTxCodeAndStatus(int orderID, string vendorTxCode, string status = null)
        {
            if (orderID == 0)
                return null;

            return OrdersRepository.UpdateVendorTxCodeAndStatus(orderID, vendorTxCode, status);
        }

        public List<tbl_Orders> SearchOrders(string search, string startDate, string endDate, int? domainID, int? statusID, int? categoryID, int? productID, ProductType type = ProductType.AllProducts)
        {
            DateTime sDate, eDate;
            bool sDateParsed = startDate.TryParseDate(out sDate);
            bool eDateParsed = endDate.TryParseDate(out eDate);
            if (eDateParsed)
            {
                eDate.AddDays(1);
            }

            IQueryable<tbl_Orders> orders = null;
            search = string.IsNullOrEmpty(search) ? null : search.ToLowerInvariant();

            Expression<Func<tbl_Orders, bool>> ordersExpressionHelper = o =>
                    (!domainID.HasValue || domainID.Value == 0 || o.O_DomainID == domainID.Value) &&
                    (!categoryID.HasValue || categoryID.Value == 0 || o.tbl_OrderContent.Any(oc => oc.OC_ProductID.HasValue && oc.OC_ProductID != 0 && oc.tbl_Products.P_CategoryID == categoryID.Value)) &&
                    (!productID.HasValue || productID.Value == 0 || o.tbl_OrderContent.Any(oc => oc.OC_ProductID == productID)) &&
                    (!sDateParsed || sDate <= o.O_Timestamp) &&
                    (!eDateParsed || eDate >= o.O_Timestamp) &&
                    (string.IsNullOrEmpty(search) ||
                            (SqlFunctions.StringConvert((double)o.OrderID).Equals(search)) ||
                            (o.tbl_Customer != null && (o.tbl_Customer.CU_Title.Contains(search) || o.tbl_Customer.CU_FirstName.Contains(search) || o.tbl_Customer.CU_Surname.Contains(search))) ||
                            (!string.IsNullOrEmpty(o.CustomerEMail) && o.CustomerEMail.Contains(search)) ||
                            (!string.IsNullOrEmpty(o.VendorTxCode) && o.VendorTxCode.Contains(search)) ||
                            ((!string.IsNullOrEmpty(o.BillingFirstnames) || !string.IsNullOrEmpty(o.BillingSurname)) && (o.BillingFirstnames.Contains(search) || o.BillingSurname.Contains(search))) ||
                            (!string.IsNullOrEmpty(o.BillingAddress1) && o.BillingAddress1.Contains(search)) ||
                            (!string.IsNullOrEmpty(o.BillingCity) && o.BillingCity.Contains(search)) ||
                            (!string.IsNullOrEmpty(o.BillingPostCode) && o.BillingPostCode.Contains(search)) ||
                            ((!string.IsNullOrEmpty(o.DeliveryFirstnames) || !string.IsNullOrEmpty(o.DeliverySurname)) && (o.DeliveryFirstnames.Contains(search) || o.DeliverySurname.Contains(search))) ||
                            (!string.IsNullOrEmpty(o.DeliveryPostCode) && o.DeliveryPostCode.Contains(search)) ||
                            (!string.IsNullOrEmpty(o.DeliveryAddress1) && o.DeliveryAddress1.Contains(search)) ||
                            (!string.IsNullOrEmpty(o.DeliveryCity) && o.DeliveryCity.Contains(search)) ||
                            o.tbl_OrderContent.Any(oc =>
                                oc.OC_Title.Contains(search) ||
                                (oc.tbl_Products != null && (oc.tbl_Products.P_ProductCode.Contains(search) || oc.tbl_Products.tbl_ProdCategories.PC_Title.Contains(search)))));

            if (type == ProductType.Donation)
                orders = OrdersRepository.GetAll().Where(m => m.O_ProductTypeID.HasValue && m.O_ProductTypeID == (int)ProductType.Donation).Where(ordersExpressionHelper);
            else if (type == ProductType.Event)
                orders = OrdersRepository.GetAll().Where(m => m.O_ProductTypeID.HasValue && m.O_ProductTypeID == (int)ProductType.Event).Where(ordersExpressionHelper);
            else if (type == ProductType.Item)
                orders = OrdersRepository.GetAll().Where(m => m.O_ProductTypeID.HasValue && m.O_ProductTypeID == (int)ProductType.Item).Where(ordersExpressionHelper);
            else 
                orders = OrdersRepository.GetAll().Where(m => !m.O_ProductTypeID.HasValue || m.O_ProductTypeID != (int)ProductType.Donation).Where(ordersExpressionHelper);
                    
                    return orders.ToList().Where(o =>
                        (!statusID.HasValue || statusID.Value == 0 || (int)o.CurrentOrderStatus == statusID.Value)).OrderByDescending(o => o.O_Timestamp).ToList();
        }

        public tbl_Orders UpdateOrderStatus(int orderID, int statusID)
        {
            if (orderID == 0 || statusID == 0)
                return null;

            return OrdersRepository.UpdateOrderStatus(orderID, statusID);
        }

        public tbl_Orders UpdateOrderPaymentStatus(int orderID, PaymentStatus status, string currency = null)
        {
            if (orderID == 0)  // || status == PaymentStatus.Initialized) <-- why not saving Status.Initialized?
                return null;

            var order = OrdersRepository.UpdateOrderPaymentStatus(orderID, status, currency);
            if (order == null)
                return null;

            if (order.DependentOrders.Count > 0)
            {
                foreach (var dOrder in order.DependentOrders)
                {
                    OrdersRepository.UpdateOrderPaymentStatus(dOrder.OrderID, status, currency);
                }
            }
                
            if (status == PaymentStatus.Paid && order != null)
            {
                DeleteReferencedBasket(order.OrderID);
            }
            return order;
        }

        public tbl_Orders UpdateOrderSecurityKey(string securityKey, int orderID)
        {
            return OrdersRepository.UpdateOrderSecurityKey(securityKey, orderID);
        }

        public tbl_Orders UpdateOrderPayment(string vendorTxCode, string addressResult, string addressStatus, string avscv2, string cavv, string cv2Result, bool? giftAid, string postCodeResult,
            string last4Digits, string payerStatus, string securityKey, string status, long txAuthNo, string vpstxId, string threeDSecureStatus, string txType, string currencyCode, int orderID)
        {
            if (orderID == 0)
                return null;

            return OrdersRepository.UpdateOrderPayment(vendorTxCode, addressResult, addressStatus, avscv2, cavv, cv2Result, giftAid, postCodeResult, last4Digits, payerStatus,
                securityKey, status, txAuthNo, vpstxId, threeDSecureStatus, txType, currencyCode, orderID);
        }

        #endregion


        #region Tax

        public bool CanAddTax(string name, decimal percentage, int taxID = 0)
        {
            return TaxRepository.CanAddTax(name, percentage, taxID);
        }

        public bool DeleteTax(int taxID)
        {
            return TaxRepository.DeleteTax(taxID);
        }

        public List<tbl_Tax> GetAllTaxes()
        {
            return TaxRepository.GetAll().ToList();
        }

        public List<SelectListItem> GetAllTaxesAsSelectList(int selectTaxID)
        {
            var taxes = TaxRepository.GetAll().ToList().Select(t => new SelectListItem()
            {
                Text = String.Format("{0} ({1})", t.TA_Title, t.TA_Percentage.GetValueOrDefault(0).ToString("0")),
                Value = t.TaxID.ToString(),
                Selected = t.TaxID == selectTaxID
            }).ToList();
            taxes.Insert(0, new SelectListItem() { Text = "Select Tax Rate", Value = "0" });
            return taxes;
        }

        public tbl_Tax GetTaxByID(int taxID)
        {
            return TaxRepository.GetByID(taxID);
        }

        public tbl_Tax GetTaxByName(string name)
        {
            return TaxRepository.GetByName(name);
        }

        public tbl_Tax SaveTax(string title, decimal percentage, int taxID)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            return TaxRepository.SaveTax(title, percentage, taxID);
        }

        #endregion


        #region PaymentDomain

        public List<tbl_PaymentDomain> GetAllPaymentsDomain(int domainID)
        {
            return PaymentDomainRepository.GetAllByDomainID(domainID, null).ToList();
        }

        public List<SelectListItem> GetAllPaymentDomainAsSelectList(int domainID, bool? isLive, int selectPaymentTypeID = 0)
        {
            var paymentTypes = PaymentDomainRepository.GetAllByDomainID(domainID, isLive).ToList().Select(pd => new SelectListItem()
            {
                Text = pd.tbl_PaymentType.PT_Name,
                Value = pd.PaymentDomainID.ToString(),
                Selected = pd.PD_PaymentTypeID == selectPaymentTypeID
            }).ToList();

            if (paymentTypes.Count > 1)
            {
                paymentTypes.Insert(0, new SelectListItem()
                {
                    Text = "Select Payment Option",
                    Value = "0",
                    Selected = selectPaymentTypeID == 0
                });
            }
            return paymentTypes;
        }

        public tbl_PaymentDomain GetPaymentDomainByID(int paymentDomainID)
        {
            return PaymentDomainRepository.GetByID(paymentDomainID);
        }

        public int GetPaymentDomainIDByCode(int domainID, PaymentType code)
        {
            return PaymentDomainRepository.GetIDByCode(domainID, code);
        }

        public string GetPaymentDomainLogo(int paymentID)
        {
            return PaymentDomainRepository.GetPaymentLogoByDomainID(paymentID);
        }

        public string SavePaymentLogoImage(int paymentDomainID, string fileName)
        {
            return PaymentDomainRepository.SaveImage(paymentDomainID, fileName);
        }

        public bool DeletePaymentLogoImage(int paymentDomainID)
        {
            return PaymentDomainRepository.DeleteImage(paymentDomainID);
        }

        #endregion PaymentDomain


        #region EventTypes

        public bool DeleteEventType(int eventTypeID)
        {
            if (eventTypeID == 0)
                return false;

            return EventTypesRepository.DeleteEventType(eventTypeID);
        }

        public List<tbl_EventTypes> GetAllEventTypes()
        {
            return EventTypesRepository.GetAll().ToList();
        }

        public SelectList GetAllEventTypesAsSelectList(int selectedEventTypeID)
        {
            return new SelectList(EventTypesRepository.GetAll(), "EventTypeID", "ET_Title", selectedEventTypeID);
        }

        public tbl_EventTypes GetEventTypeByID(int eventTypeID)
        {
            if (eventTypeID == 0)
                return null;

            return EventTypesRepository.GetByID(eventTypeID);
        }

        public tbl_EventTypes GetEventTypeByName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;

            return EventTypesRepository.GetByName(name);
        }

        public tbl_EventTypes SaveEventType(string title, string desc, int eventTypeID)
        {
            if (String.IsNullOrEmpty(title))
                return null;

            return EventTypesRepository.SaveEventType(title, desc, eventTypeID);
        }

        public tbl_EventTypes UpdateEventTypeImagePath(string path, int eventTypeID)
        {
            if (eventTypeID == 0)
                return null;

            return EventTypesRepository.UpdatePath(path, eventTypeID);
        }

        #endregion


        #region Private Methodes

        private void CreateStockUnit(int productID, List<tbl_ProdAttValue> prodAttValues, List<int> selectedValues)
        {
            if (prodAttValues.Count == 0)
            {
                if (!ProductPriceRepository.CheckIfStockUnitExists(productID, selectedValues))
                {
                    var price = ProductPriceRepository.SaveStockUnit(productID, String.Empty, String.Empty, false, 0, 0, 0, String.Empty, 0, 0, null, null, 0,0);
                    if (price == null)
                        return;

                    ProductPriceRepository.SaveAttrValueForStockUnit(price.PriceID, selectedValues.ToArray());
                    return;
                }
                return;
            }

            var group = prodAttValues.GroupBy(pav => pav.AV_AttributeID).FirstOrDefault();
            var restProdAttValues = prodAttValues.Where(pav => pav.AV_AttributeID != group.Key).ToList();

            foreach (var value in group)
            {
                var list = new List<int>() { value.AttributeValueID };
                list.AddRange(selectedValues);
                CreateStockUnit(productID, restProdAttValues, list);
            }
        }

        private List<ExtendedSelectListItem> CategoriesChildItems(IEnumerable<tbl_ProdCategories> categories, int? parentID, int selectedCategoryID, int level)
        {
            List<ExtendedSelectListItem> list = new List<ExtendedSelectListItem>();
            foreach (var category in categories.Where(c => c.PC_ParentID == parentID))
            {
                var item = new ExtendedSelectListItem
                {
                    Text = category.PC_Title,
                    Value = category.CategoryID.ToString(),
                    Selected = category.CategoryID == selectedCategoryID,
                    Level = level
                };

                list.Add(item);
                list.AddRange(CategoriesChildItems(categories, category.CategoryID, selectedCategoryID, level + 1));
            }
            return list;
        }

        private bool DecreaseStock(tbl_Orders order)
        {
            return ProductPriceRepository.DecreaseStockUnits(order.tbl_OrderContent);
        }

        private bool IncreaseStock(tbl_Orders order)
        {
            return ProductPriceRepository.IncreaseStockUnits(order.tbl_OrderContent);
        }

        #endregion

    }
}

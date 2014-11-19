using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IBasketRepository
    {
        bool DeleteBasket(int basketID);
        bool DeleteUnUsedBaskets();
        tbl_Basket GetByID(int basketID);
        tbl_Basket SaveBasket(int? customerID, int? discountID, string sessionID, byte type, int domainID, int basketID);
        tbl_Basket UpdateCustomerID(int customerID, int basketID);
        tbl_Basket UpdateDeliveryDetails(string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, string billingCountry, int billingCountryID, string billingTitle,
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string deliveryAddress1, string deliveryAddress2, string deliveryAddress3,
            string deliveryCity, string deliveryCountry, int deliveryCountryID, string deliveryTitle, string deliveryFirstnames, string deliveryPhone, string deliveryPostCode, string deliveryState,
            string deliverySurname, string deliveryNotes, string customerEMail, int? customerID, bool billingEqDelivery, bool subscription, bool detailsFor3rdParties, int basketID);
        tbl_Basket UpdateDate(int basketID);
        tbl_Basket UpdateDiscount(int basketID, int? discountID);
        tbl_Basket UpdateBasketPostage(int? postageID, int basketID);
        tbl_Basket UpdateDeliveryNotes(string note, int basketID);
        tbl_Basket UpdateDeliveryCountry(int? countryID, string country, int basketID);
        bool SaveEUVAT(int basketID, string EUVAT);

        bool SaveOrderReference(int basketID, int orderID);
    }

    public class BasketRepository : Repository<tbl_Basket>, IBasketRepository
    {
        public BasketRepository(IDALContext context) : base(context) { }

        public bool DeleteBasket(int basketID)
        {
            tbl_Basket basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return false;

            foreach (var content in basket.tbl_BasketContent.ToList())
            {
                this.Context.Set<tbl_BasketContent>().Remove(content);
            }

            this.Delete(basket);
            this.Context.SaveChanges();
            return true;
        }

        public bool DeleteUnUsedBaskets()
        {
            var date = DateTime.UtcNow.AddDays(-30);
            var baskets = this.DbSet.Where(b => b.B_CustomerID == 0 && b.B_LastAccessed < date);

            foreach (var basket in baskets.ToList())
            {
                foreach (var content in basket.tbl_BasketContent.ToList())
                {
                    this.Context.Set<tbl_BasketContent>().Remove(content);
                }
                this.Delete(basket);
            }

            this.Context.SaveChanges();
            return true;
        }

        public tbl_Basket GetByID(int basketID)
        {
            return this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
        }

        public tbl_Basket UpdateCustomerID(int customerID, int basketID)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
            {
                basket = new tbl_Basket();
                this.Create(basket);
            }

            basket.B_LastAccessed = DateTime.UtcNow;
            basket.B_CustomerID = customerID;

            this.Context.SaveChanges();
            return basket;
        }

        public tbl_Basket UpdateDeliveryDetails(string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, string billingCountry, int billingCountryID, string billingTitle,
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string deliveryAddress1, string deliveryAddress2, string deliveryAddress3,
            string deliveryCity, string deliveryCountry, int deliveryCountryID, string deliveryTitle, string deliveryFirstnames, string deliveryPhone, string deliveryPostCode, string deliveryState,
            string deliverySurname, string deliveryNotes, string customerEMail, int? customerID, bool billingEqDelivery, bool subscription, bool detailsFor3rdParties, int basketID)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
            {
                basket = new tbl_Basket();
                this.Create(basket);
            }

            basket.B_LastAccessed = DateTime.UtcNow;

            basket.B_BillingAddress1 = billingAddress1;
            basket.B_BillingAddress2 = billingAddress2;
            basket.B_BillingAddress3 = billingAddress3;
            basket.B_BillingCity = billingCity;
            basket.B_BillingCountry = billingCountry;
            basket.B_BillingCountryID = billingCountryID;
            basket.B_BillingFirstnames = billingFirstnames;
            basket.B_BillingPhone = billingPhone;
            basket.B_BillingPostCode = billingPostCode;
            basket.B_BillingState = billingState;
            basket.B_BillingSurname = billingSurname;
            basket.B_BillingTitle = billingTitle;

            basket.B_DeliveryAddress1 = deliveryAddress1;
            basket.B_DeliveryAddress2 = deliveryAddress2;
            basket.B_DeliveryAddress3 = deliveryAddress3;
            basket.B_DeliveryCity = deliveryCity;
            basket.B_DeliveryCountry = deliveryCountry;
            basket.B_DeliveryCountryID = deliveryCountryID;
            basket.B_DeliveryFirstnames = deliveryFirstnames;
            basket.B_DeliveryPhone = deliveryPhone;
            basket.B_DeliveryPostCode = deliveryPostCode;
            basket.B_DeliveryState = deliveryState;
            basket.B_DeliverySurname = deliverySurname;
            basket.B_DeliveryTitle = deliveryTitle;

            basket.B_CustomerEMail = customerEMail;
            basket.B_CustomerID = customerID;
            basket.B_DetailsFor3rdParties = detailsFor3rdParties;
            basket.B_Subscription = subscription;
            basket.B_DeliveryNotes = deliveryNotes;
            basket.B_BillingEqDelivery = billingEqDelivery;


            this.Context.SaveChanges();
            return basket;
        }

        public tbl_Basket UpdateDiscount(int basketID, int? discountID)
        {
            tbl_Basket basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return null;

            basket.B_DiscountID = discountID;

            this.Context.SaveChanges();
            return basket;
        }

        public tbl_Basket SaveBasket(int? customerID, int? discountID, string sessionID, byte type, int domainID, int basketID)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID && b.B_DomainID == domainID);
            if (basket == null)
            {
                basket = new tbl_Basket();
                basket.B_DomainID = domainID;
                this.Create(basket);
            }

            basket.B_LastAccessed = DateTime.UtcNow;

            basket.B_CustomerID = customerID;
            basket.B_DiscountID = discountID;
            basket.B_SessionID = sessionID;
            basket.B_Type = type;

            this.Context.SaveChanges();
            return basket;
        }

        public bool SaveOrderReference(int basketID, int orderID)
        {
            var basket = this.DbSet.FirstOrDefault(m => m.BasketID == basketID);
            if (basket == null)
                return false;
            basket.B_OrderID = orderID;
            this.Context.SaveChanges();
            return true;

        }

        public bool SaveEUVAT(int basketID, string EUVAT)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return false;
            basket.B_VATNumber = EUVAT;
            this.Context.SaveChanges();
            return true;
        }

        public tbl_Basket UpdateBasketPostage(int? postageID, int basketID)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return null;

            basket.B_PostageID = postageID;
            this.Context.SaveChanges();
            return basket;
        }

        public tbl_Basket UpdateDeliveryNotes(string note, int basketID)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return null;

            basket.B_DeliveryNotes = note;
            this.Context.SaveChanges();
            return basket;
        }

        public tbl_Basket UpdateDeliveryCountry(int? countryID, string country, int basketID)
        {
            var basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return null;

            basket.B_DeliveryCountryID = countryID;
            basket.B_DeliveryCountry = country;
            this.Context.SaveChanges();
            return basket;
        }

        public tbl_Basket UpdateDate(int basketID)
        {
            tbl_Basket basket = this.DbSet.FirstOrDefault(b => b.BasketID == basketID);
            if (basket == null)
                return null;

            basket.B_LastAccessed = DateTime.UtcNow;
            this.Context.SaveChanges();
            return basket;
        }
    }
}

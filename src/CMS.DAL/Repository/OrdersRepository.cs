using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using CMS.BL;

namespace CMS.DAL.Repository
{
    public interface IOrdersRepository
    {
        tbl_Orders CancelOrder(int orderID);
        IQueryable<tbl_Orders> GetAll();
        tbl_Orders GetByID(int orderID);
        tbl_Orders GetByVendorCode(string vendorTxCode, int domainID);
        tbl_Orders GetBySecurityKey(string securityKey, int domainID);
        IQueryable<tbl_DespatchMethod> GetDespatchMethodes();
        tbl_DespatchMethod SaveNewDespathMethod(string method);

        tbl_Orders SaveCustomTotalAmount(decimal price, int orderID);
        tbl_Orders SaveOrder(int orderID, int domainID, decimal amount, string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, string billingCountry,
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string customerEMail, int? customerID, string deliveryAddress1,
            string deliveryAddress2, string deliveryAddress3, string deliveryCity, string deliveryCountry, string deliveryFirstnames, string deliveryPhone, string deliveryPostCode, string deliveryState,
            string deliverySurname, int? discountID, string deliveryNotes, int? paymentDomainID, int? cashPayment, decimal totalTaxAmount, decimal totalDeliveryAmount, decimal deliveryTaxAmount,
            decimal deliveryAmount, decimal discountAmount, decimal totalAmount, string vatNumber, int? adminID = null);
        tbl_Orders SaveOrderAsDonation(int orderID, int domainID, decimal amount, string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, string billingCountry,
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string customerEMail, int? customerID, int paymentDomainID, bool? giftAid,
            decimal totalTaxAmount, decimal totalAmount, DonationType donationType, int? parentOrderID);

        tbl_Orders UpdateOrderTracking(int despathMethodID, string deliveryDate, string trackingUrl, string trackingRef, int orderID);
        tbl_Orders UpdateOrderStatus(int orderID, int statusID);
        tbl_Orders UpdateOrderPaymentStatus(int orderID, PaymentStatus status, string currencyCode = null);
        tbl_Orders UpdateOrderSecurityKey(string securityKey, int orderID);
        tbl_Orders UpdateOrderPayment(string vendorTxCode, string addressResult, string addressStatus, string avscv2, string cavv, string cv2Result, bool? giftAid, string postCodeResult,
            string last4Digits, string payerStatus, string securityKey, string status, long txAuthNo, string vpstxId, string threeDSecureStatus, string txType, string currencyCode, int orderID);

        tbl_Orders UpdateVendorTxCodeAndStatus(int orderID, string vendorTxCode, string status = null);
        tbl_Basket GetReferencedBasket(int orderID);
    }

    public class OrdersRepository : Repository<tbl_Orders>, IOrdersRepository
    {
        public OrdersRepository(IDALContext context) : base(context) { }

        public tbl_Orders CancelOrder(int orderID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.tbl_CustOrdStatus.Add(new tbl_CustOrdStatus()
            {
                CS_OrderID = order.OrderID,
                CS_StatusID = (int)OrderStatus.Canceled,
                CS_TimeStamp = DateTime.UtcNow
            });
            

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Basket GetReferencedBasket(int orderID)
        {
            var order = this.DbSet.FirstOrDefault(m => m.OrderID == orderID);
            return order != null ? order.tbl_Basket.FirstOrDefault(m => m.B_OrderID == order.OrderID) : null;
        }

        public IQueryable<tbl_Orders> GetAll()
        {
            return this.DbSet.AsQueryable();
        }

        public tbl_Orders GetByID(int orderID)
        {
            return this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
        }

        public tbl_Orders GetByVendorCode(string vendorTxCode, int domainID)
        {
            return this.DbSet.FirstOrDefault(o => o.VendorTxCode.Equals(vendorTxCode) && o.O_DomainID == domainID);
        }

        public tbl_Orders GetBySecurityKey(string securityKey, int domainID)
        {
            return this.DbSet.Where(o => o.SecurityKey == securityKey && o.O_DomainID == domainID).FirstOrDefault();
        }

        public IQueryable<tbl_DespatchMethod> GetDespatchMethodes()
        {
            return this.Context.Set<tbl_DespatchMethod>().AsQueryable();
        }

        public tbl_DespatchMethod SaveNewDespathMethod(string method)
        {
            var dbMethod = this.Context.Set<tbl_DespatchMethod>().FirstOrDefault(dm => dm.DM_Name.Equals(method));
            if (dbMethod != null)
                return dbMethod;

            dbMethod = new tbl_DespatchMethod() { DM_Name = method };
            dbMethod = this.Context.Set<tbl_DespatchMethod>().Add(dbMethod);
            this.Context.SaveChanges();
            return dbMethod;
        }

        public tbl_Orders SaveCustomTotalAmount(decimal price, int orderID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.TotalAmount = price;
            order.O_IsCustomAmount = true;

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders SaveOrder(int orderID, int domainID, decimal amount, string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, string billingCountry, 
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string customerEMail, int? customerID, string deliveryAddress1, 
            string deliveryAddress2, string deliveryAddress3, string deliveryCity, string deliveryCountry, string deliveryFirstnames, string deliveryPhone, string deliveryPostCode, string deliveryState, 
            string deliverySurname, int? discountID, string deliveryNotes, int? paymentDomainID, int? cashPayment, decimal totalTaxAmount, decimal totalDeliveryAmount, decimal deliveryTaxAmount, 
            decimal deliveryAmount, decimal discountAmount, decimal totalAmount, string vatNumber, int? adminID = null)
        {
            // TODO: Save product type
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID && o.O_DomainID == domainID);
            if (order == null)
            {
                order = new tbl_Orders()
                {
                    O_Timestamp = DateTime.UtcNow,
                    O_DomainID = domainID,
                    VendorTxCode = Guid.NewGuid().ToString(),
                    TxType = "PAYMENT",
                    Currency = "GBP",
                };
                this.Create(order);
            }

            order.Amount = amount;

            order.BillingAddress1 = billingAddress1;
            order.BillingAddress2 = billingAddress2;
            order.BillingAddress3 = billingAddress3;
            order.BillingCity = billingCity;
            order.BillingCountry = billingCountry;
            order.BillingFirstnames = billingFirstnames;
            order.BillingPhone = billingPhone;
            order.BillingPostCode = billingPostCode;
            order.BillingState = billingState;
            order.BillingSurname = billingSurname;

            order.CustomerEMail = customerEMail;
            order.CustomerID = customerID;

            order.DeliveryAddress1 = deliveryAddress1;
            order.DeliveryAddress2 = deliveryAddress2;
            order.DeliveryAddress3 = deliveryAddress3;
            order.DeliveryCity = deliveryCity;
            order.DeliveryCountry = deliveryCountry;
            order.DeliveryFirstnames = deliveryFirstnames;
            order.DeliveryPhone = deliveryPhone;
            order.DeliveryPostCode = deliveryPostCode;
            order.DeliveryState = deliveryState;
            order.DeliverySurname = deliverySurname;

            order.O_DeliveryNotes = deliveryNotes;
            order.O_DiscountID = discountID;
            order.O_PaymentDomainID = paymentDomainID;
            order.O_IsCashSale = !paymentDomainID.HasValue;
            order.O_CashPayment = !paymentDomainID.HasValue ? cashPayment : (int?)null;
            order.B_VATNumber = vatNumber;

            order.TotalDeliveryAmount = totalDeliveryAmount;
            order.DeliveryCharge = deliveryAmount;
            order.DeliveryTax = deliveryTaxAmount;
            order.TotalTaxAmount = totalTaxAmount;
            order.DiscountAmount = discountAmount;
            order.TotalAmount = totalAmount;

            order.O_AdminID = adminID.HasValue && adminID.Value > 0 ? adminID : (int?)null;

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders SaveOrderAsDonation(int orderID, int domainID, decimal amount, string billingAddress1, string billingAddress2, string billingAddress3, string billingCity, string billingCountry,
            string billingFirstnames, string billingPhone, string billingPostCode, string billingState, string billingSurname, string customerEMail, int? customerID,  int paymentDomainID, bool? giftAid,
            decimal totalTaxAmount, decimal totalAmount, DonationType donationType, int? parentOrderID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID && o.O_DomainID == domainID);
            if (order == null)
            {
                order = new tbl_Orders()
                {
                    O_Timestamp = DateTime.UtcNow,
                    O_DomainID = domainID,
                    Currency = "GBP",
                    O_ProductTypeID = (int)ProductType.Donation,
                    VendorTxCode = "",
                    TxType = "PAYMENT",
                };
                this.Create(order);
            }

            order.Amount = amount;

            order.BillingAddress1 = billingAddress1;
            order.BillingAddress2 = billingAddress2;
            order.BillingAddress3 = billingAddress3;
            order.BillingCity = billingCity;
            order.BillingCountry = billingCountry;
            order.BillingFirstnames = billingFirstnames;
            order.BillingPhone = billingPhone;
            order.BillingPostCode = billingPostCode;
            order.BillingState = billingState;
            order.BillingSurname = billingSurname;

            order.DeliveryAddress1 = billingAddress1;
            order.DeliveryAddress2 = billingAddress2;
            order.DeliveryAddress3 = billingAddress3;
            order.DeliveryCity = billingCity;
            order.DeliveryCountry = billingCountry;
            order.DeliveryFirstnames = billingFirstnames;
            order.DeliveryPhone = billingPhone;
            order.DeliveryPostCode = billingPostCode;
            order.DeliveryState = billingState;
            order.DeliverySurname = billingSurname;

            order.CustomerEMail = customerEMail;
            order.CustomerID = customerID;
            order.O_PaymentDomainID = paymentDomainID;
            order.GiftAid = giftAid;
            order.O_DonationTypeID = (int)donationType;
            order.O_ProductTypeID = (int)ProductType.Donation;

            order.TotalTaxAmount = totalTaxAmount;
            order.TotalAmount = totalAmount;


            order.O_ParentOrderID = parentOrderID;

            //if (parentOrderID.HasValue)
            //{
            //    var parentOrder = this.DbSet.FirstOrDefault(m => m.OrderID == parentOrderID.Value);
            //    if (parentOrder == null)
            //        return null;
            //    parentOrder.TotalTaxAmount += totalTaxAmount;
            //    parentOrder.TotalAmount += totalAmount;
            //}

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders UpdateOrderTracking(int despathMethodID, string deliveryDate, string trackingUrl, string trackingRef, int orderID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.O_DespatchMethodID = despathMethodID;
            order.O_TrackingRef = trackingRef;
            order.O_TrackingURL = trackingUrl;
            order.O_DeliveryDate = deliveryDate;

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders UpdateOrderStatus(int orderID, int statusID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            var status = this.Context.Set<tbl_OrderStatus>().FirstOrDefault(s => s.OrderStatusID == statusID);
            if (status == null)
                return null;

            order.tbl_CustOrdStatus.Add(new tbl_CustOrdStatus()
            {
                CS_OrderID = order.OrderID,
                CS_StatusID = status.OrderStatusID,
                CS_TimeStamp = DateTime.UtcNow
            });

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders UpdateOrderPaymentStatus(int orderID, PaymentStatus status, string currencyCode = null)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.O_PaymentStatusID = (int)status;
            if (currencyCode != null)
                order.Currency = currencyCode;

            if (status == PaymentStatus.Paid)
            {
                order.tbl_CustOrdStatus.Add(new tbl_CustOrdStatus()
                {
                    CS_OrderID = order.OrderID,
                    CS_StatusID = (int)OrderStatus.Paid,
                    CS_TimeStamp = DateTime.UtcNow
                });
            }
            else if (status == PaymentStatus.PayPal_DoExpressCheckout_Failure || 
                status == PaymentStatus.PayPal_DoExpressCheckout_Unknown || 
                status == PaymentStatus.PayPal_LandingPage_PayerUnknown || 
                status == PaymentStatus.PayPal_SetExpressCheckout_Failure || 
                status == PaymentStatus.SagePay_Aborted || 
                status == PaymentStatus.SagePay_Error ||
                status == PaymentStatus.SagePay_Invalid || 
                status == PaymentStatus.SagePay_Malformed || 
                status == PaymentStatus.SagePay_NotAuthed || 
                status == PaymentStatus.SagePay_Rejected || 
                status == PaymentStatus.SagePay_Unknown || 
                status == PaymentStatus.SecureTrading_Declined || 
                status == PaymentStatus.SecureTrading_Error)
            {
                order.tbl_CustOrdStatus.Add(new tbl_CustOrdStatus()
                {
                    CS_OrderID = order.OrderID,
                    CS_StatusID = (int)OrderStatus.PaymentFailed,
                    CS_TimeStamp = DateTime.UtcNow
                });
            }

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders UpdateOrderSecurityKey(string securityKey, int orderID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.SecurityKey = securityKey;

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders UpdateVendorTxCodeAndStatus(int orderID, string vendorTxCode, string status = null)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.VendorTxCode = vendorTxCode;
            if (status != null)
                order.Status = status;

            this.Context.SaveChanges();
            return order;
        }

        public tbl_Orders UpdateOrderPayment(string vendorTxCode, string addressResult, string addressStatus, string avscv2, string cavv, string cv2Result, bool? giftAid, string postCodeResult,
            string last4Digits, string payerStatus, string securityKey, string status, long txAuthNo, string vpstxId, string threeDSecureStatus, string txType, string currencyCode, int orderID)
        {
            var order = this.DbSet.FirstOrDefault(o => o.OrderID == orderID);
            if (order == null)
                return null;

            order.AddressResult = addressResult;
            order.AddressStatus = addressStatus;
            order.AVSCV2 = avscv2;
            order.CAVV = cavv;
            order.CV2Result = cv2Result;
            order.GiftAid = giftAid;
            order.Last4Digits = last4Digits;
            order.PayerStatus = payerStatus;
            order.Status = status;
            order.PostCodeResult = postCodeResult;
            order.SecurityKey = securityKey;
            order.TxAuthNo = txAuthNo;
            order.TxType = txType;
            order.VendorTxCode = vendorTxCode;
            order.VPSTxId = vpstxId;
            order.Currency = currencyCode;
            order.ThreeDSecureStatus = threeDSecureStatus;

            this.Context.SaveChanges();
            return order;
        }
    }
}

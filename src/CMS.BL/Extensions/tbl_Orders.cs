using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Text;

namespace CMS.BL.Entity
{
    public partial class tbl_Orders
    {
        public EntityCollection<tbl_Orders> DependentOrders
        {
            get
            {
                return this.tbl_Orders1;
            }
        }

        public tbl_Orders ParentOrder
        {
            get
            {
                return this.tbl_Orders2;
            }
        }

        public decimal TotalAmountToPay
        {
            get
            {
                return this.DependentOrders.Any() ? this.TotalAmount + this.DependentOrders.Sum(o => o.TotalAmount) : this.TotalAmount;
            }
        }

        public OrderStatus CurrentOrderStatus
        {
            get
            {
                var status = this.tbl_CustOrdStatus.OrderByDescending(o => o.CS_TimeStamp).Select(o => o.CS_StatusID).FirstOrDefault();
                return (OrderStatus)Enum.Parse(typeof(OrderStatus), status.ToString());
            }
        }

        public bool Canceled
        {
            get
            {
                return this.CurrentOrderStatus == OrderStatus.Canceled;
            }
        }

        public string BillingFullName
        {
            get
            {
                return String.Format("{0} {1}", this.BillingFirstnames, this.BillingSurname);
            }
        }

        public string DeliveryFullName
        {
            get
            {
                return String.Format("{0} {1}", this.DeliveryFirstnames, this.DeliverySurname);
            }
        }

        public bool IsDeliverable
        {
            get
            {
                return this.tbl_OrderContent.Any(oc => oc.tbl_Products != null && oc.tbl_Products.P_Deliverable);
            }
        }
    }
}

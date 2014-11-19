using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.UI.Common.Validation;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using CMS.BL;

namespace CMS.UI.Models
{
    public class NewOrderModel
    {
        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Title")]
        public string DeliveryTitle { get; set; }

        [Required(ErrorMessage = "Firstname is required")]
        [Display(Name = "Firstname")]
        public string DeliveryFirstName { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [Display(Name = "Surname")]
        public string DeliverySurname { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Display(Name = "Phone")]
        public string DeliveryPhone { get; set; }

        [Required(ErrorMessage = "Address 1 is required")]
        [Display(Name = "Address 1")]
        public string DeliveryAddress1 { get; set; }

        [Display(Name = "Address 2")]
        public string DeliveryAddress2 { get; set; }

        [Display(Name = "Address 3")]
        public string DeliveryAddress3 { get; set; }

        [Required(ErrorMessage = "Town/City is required")]
        [Display(Name = "Town/City")]
        public string DeliveryCity { get; set; }

        [Required(ErrorMessage = "Postcode is required")]
        [Display(Name = "Postcode")]
        public string DeliveryPostcode { get; set; }

        [Required(ErrorMessage = "County is required")]
        [Display(Name = "County")]
        public string DeliveryState { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int DeliveryCountryID { get; set; }

        //[Required(ErrorMessage = "Delivery is required")]
        //[Display(Name = "Delivery option")]
        //public int PostageID { get; set; }

        //[Required(ErrorMessage = "Customer is required")]
        //[Range(1, Int16.MaxValue, ErrorMessage = "Customer is required")]
        [Display(Name = "Search For Customer")]
        public int? CustomerID { get; set; }

        [Display(Name = "Customer address")]
        public int? AddressID { get; set; }

        [Email(ErrorMessage = "Email should have correct format.")]
        //[Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email Address")]
        public string CustomerEmail { get; set; }

        [Display(Name = "The Same As Delivery")]
        public bool BillingAddressTheSame { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Title")]
        public string BillingTitle { get; set; }

        [Required(ErrorMessage = "Firstname is required")]
        [Display(Name = "Firstname")]
        public string BillingFirstName { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [Display(Name = "Surname")]
        public string BillingSurname { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Display(Name = "Phone")]
        public string BillingPhone { get; set; }

        [Required(ErrorMessage = "Address 1 is required")]
        [Display(Name = "Address 1")]
        public string BillingAddress1 { get; set; }

        [Display(Name = "Address 2")]
        public string BillingAddress2 { get; set; }

        [Display(Name = "Address 3")]
        public string BillingAddress3 { get; set; }

        [Required(ErrorMessage = "Town/City is required")]
        [Display(Name = "Town/City")]
        public string BillingCity { get; set; }

        [Required(ErrorMessage = "Postcode is required")]
        [Display(Name = "Postcode")]
        public string BillingPostcode { get; set; }

        [Required(ErrorMessage = "County is required")]
        [Display(Name = "County")]
        public string BillingState { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int BillingCountryID { get; set; }

        public string Instructions { get; set; }

        [Display(Name = "EU VAT Number")]
        public string VatNumber { get; set; }

        [Display(Name = "Promotional Code")]
        public string DiscountCode { get; set; }

        [Display(Name = "Online Payment")]
        public bool IsPayment { get; set; }

        [Display(Name = "Cash Payment")]
        public CashPayment CashPayment { get; set; }

        [Display(Name = "Payment gateway")]
        [RequiredIf(DependentProperty = "IsPayment", TargetValue = "true", ErrorMessage = "Payment gateway is required.")]
        public int PaymentDomainID { get; set; }

        [Display(Name = "Admin Controlled Price")]
        public bool IsCustomPrice { get; set; }
        [Display(Name = "Custom price")]
        public string CustomPrice { get; set; }
    }
}
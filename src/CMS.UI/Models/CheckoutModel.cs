using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using CMS.UI.Common.Validation;
using System.Web.Mvc;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class CheckoutModel
    {
        public CheckoutModel()
        {
        }

        public CheckoutModel(tbl_Basket basket)
        {
            this.IsCheckoutStep = true;
            this.CopyValuesFromBasket(basket);
        }

        public void CopyValuesFromBasket(tbl_Basket basket)
        {
            if (basket != null)
            {
                this.Basket = basket;
                this.BasketID = basket.BasketID;
                this.IsDeliverable = basket.IsDeliverable;

                Email = basket.B_CustomerEMail;
                BillingAddressTheSame = basket.B_BillingEqDelivery;
                Instructions = basket.B_DeliveryNotes;

                DeliveryTitle = basket.B_DeliveryTitle;
                DeliveryFirstName = basket.B_DeliveryFirstnames;
                DeliverySurname = basket.B_DeliverySurname;
                DeliveryPhone = basket.B_DeliveryPhone;
                DeliveryAddress1 = basket.B_DeliveryAddress1;
                DeliveryAddress2 = basket.B_DeliveryAddress2;
                DeliveryAddress3 = basket.B_DeliveryAddress3;
                DeliveryCity = basket.B_DeliveryCity;
                DeliveryPostcode = basket.B_DeliveryPostCode;
                DeliveryState = basket.B_DeliveryState;
                DeliveryCountry = basket.B_DeliveryCountry;
                DeliveryCountryID = basket.B_DeliveryCountryID.GetValueOrDefault(0);

                BillingTitle = basket.B_BillingTitle;
                BillingFirstName = basket.B_BillingFirstnames;
                BillingSurname = basket.B_BillingSurname;
                BillingPhone = basket.B_BillingPhone;
                BillingAddress1 = basket.B_BillingAddress1;
                BillingAddress2 = basket.B_BillingAddress2;
                BillingAddress3 = basket.B_BillingAddress3;
                BillingCity = basket.B_BillingCity;
                BillingPostcode = basket.B_BillingPostCode;
                BillingState = basket.B_BillingState;
                BillingCountry = basket.B_BillingCountry;
                BillingCountryID = basket.B_BillingCountryID;
            }
        }

        public tbl_Basket Basket { get; private set; }

        public int BasketID { get; set; }

        public bool IsDeliverable { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Title is required")]
        [Display(Name = "Title *")]
        public string DeliveryTitle { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Firstname is required")]
        [Display(Name = "Firstname *")]
        public string DeliveryFirstName { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Surname is required")]
        [Display(Name = "Surname *")]
        public string DeliverySurname { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Phone is required")]
        [Display(Name = "Phone *")]
        public string DeliveryPhone { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Address 1 is required")]
        [Display(Name = "Address 1 *")]
        public string DeliveryAddress1 { get; set; }
        [Display(Name = "Address 2")]
        public string DeliveryAddress2 { get; set; }
        [Display(Name = "Address 3")]
        public string DeliveryAddress3 { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Town/City is required")]
        [Display(Name = "Town/City *")]
        public string DeliveryCity { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "County is required")]
        [Display(Name = "County *")]
        public string DeliveryState { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public string DeliveryCountry { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int DeliveryCountryID { get; set; }
        [RequiredIf(DependentProperty = "IsDeliverable", TargetValue = true, ErrorMessage = "Postcode is required")]
        [Display(Name = "Postcode *")]
        public string DeliveryPostcode { get; set; }

        [Email(ErrorMessage = "Email should have correct format.")]
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email Address *")]
        public string Email { get; set; }

        [Display(Name = "Register In Newsletter")]
        public bool Subscription { get; set; }
        [Display(Name = "Permission To Pass Details To 3rd Parties")]
        public bool Permission { get; set; }

        [Display(Name = "The Same As Delivery")]
        public bool BillingAddressTheSame { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Title is required")]
        [Display(Name = "Title *")]
        public string BillingTitle { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Firstname is required")]
        [Display(Name = "Firstname *")]
        public string BillingFirstName { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Surname is required")]
        [Display(Name = "Surname *")]
        public string BillingSurname { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Phone is required")]
        [Display(Name = "Phone *")]
        public string BillingPhone { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Address 1 is required")]
        [Display(Name = "Address 1 *")]
        public string BillingAddress1 { get; set; }
        [Display(Name = "Address 2")]
        public string BillingAddress2 { get; set; }
        [Display(Name = "Address 3")]
        public string BillingAddress3 { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Town/City is required")]
        [Display(Name = "Town/City *")]
        public string BillingCity { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Postcode is required")]
        [Display(Name = "Postcode *")]
        public string BillingPostcode { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "County is required")]
        [Display(Name = "County *")]
        public string BillingState { get; set; }
        public string BillingCountry { get; set; }
        [RequiredIf(DependentProperty = "BillingAddressTheSame", TargetValue = false, ErrorMessage = "Country is required")]
        [Display(Name = "Country *")]
        public int? BillingCountryID { get; set; }

        public string Instructions { get; set; }

        public bool IsCheckoutStep { get; set; }

        [RequiredIf(DependentProperty = "TermsAndConditionsConfirmationExpected", TargetValue = true, ErrorMessage = "Please agree to Terms and Conditions")]
        public bool TermsAndConditionsConfirmed { get; set; }
        public bool TermsAndConditionsConfirmationExpected { get { return IsCheckoutStep ? false : true; } }

        public bool GiftAid { get; set; }
        [RegularExpression("^\\d*[\\.,]?\\d*$", ErrorMessage = "Donation amount has to be a number.")]
        public string DonationAmount { get; set; }

        public int PaymentDomainID { get; set; }
    }
}
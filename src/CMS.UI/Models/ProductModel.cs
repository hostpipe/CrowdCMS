using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CMS.BL;
using CMS.UI.Common.Validation;

namespace CMS.UI.Models
{
    public class ProductModel
    {
        public ProductModel()
        {
            Deliverable = true; // true by default
            Purchasable = true; // true by default
        }

        public int SitemapID { get; set; }
        public int ContentID { get; set; }
        public int ProductID { get; set; }
        [Display(Name = "Domain")]
        public int DomainID { get; set; }
        [Required(ErrorMessage = "Product Title is required.")]
        [Display(Name = "Title")]
        public string ProductTitle { get; set; }
        [Required(ErrorMessage = "Product Code is required.")]
        [Display(Name = "Code")]
        public string ProductCode { get; set; }
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Category is required.")]
        [Range(1, Int32.MaxValue, ErrorMessage = "You have to select a category.")]
        public int CategoryID { get; set; }
        [Display(Name = "Is Live")]
        public bool Live { get; set; }
        [Display(Name = "Special Offer")]
        public bool Offer { get; set; }
        [Display(Name = "Is Stock Controlled")]
        public bool StockControl { get; set; }
        [Display(Name = "Tax Rate")]
        public int TaxID { get; set; }
        [AllowHtml]
        public string Content { get; set; }
        public ProductType ProductType { get; set; }
        [Display(Name = "Event Type")]
        [RequiredIf(DependentProperty = "ProductType", TargetValue = ProductType.Event, ErrorMessage = "You have to choose an event type.")]
        [Range(1, Int32.MaxValue, ErrorMessage = "You have to choose an event type.")]
        public int? EventTypeID { get; set; }
        [Display(Name = "Is Deliverable")]
        public bool Deliverable { get; set; }
        [Display(Name = "Can Be Purchased")]
        public bool Purchasable { get; set; }
        public bool Featured { get; set; }

        public bool IsRegular { get; set; }
        public int Interval { get; set; }



        [Display(Name = "Affiliate Link")]
        public string AffiliateLink { get; set; }

        public virtual List<SelectListItem> Categories { get; set; }
        public virtual SelectList EventTypes { get; set; }
    }
}
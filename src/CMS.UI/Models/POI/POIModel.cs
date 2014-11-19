using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class POIModel
    {
        public POIModel()
        {
            this.TagsIDs = new int[0];
        }

        public int POIID { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public string Description { get; set; }
        [Display(Name = "Telephone Number")]
        public string Phone { get; set; }
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Category is required")]
        [Range(1, Int16.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Latitude is required")]
        public string Latitude { get; set; }
        [Required(ErrorMessage = "Longitude is required")]
        public string Longitude { get; set; }
        [Display(Name = "Page Linked With POI")]
        public int? SitemapID { get; set; }
        [Display(Name = "Domain")]
        public int DomainID { get; set; }

        public int AddressID { get; set; }
        [Display(Name="Address 1")]
        [Required(ErrorMessage = "Address 1 is required")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "Address 3")]
        public string Address3 { get; set; }
        [Required(ErrorMessage = "Town is required")]
        public string Town { get; set; }
        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; }
        public string County { get; set; }
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }

        public int[] TagsIDs { get; set; }
    }
}
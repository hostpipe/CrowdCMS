using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class EditCustomerAdminModel : EditCustomerModel
    {
        public EditCustomerAdminModel() { }
        public EditCustomerAdminModel(tbl_Customer customer)
            : base(customer)
        {
            CustomerID = customer.CustomerID;
            DomainID = customer.CU_DomainID;
            Registered = customer.CU_Registered;

            IsDormant = customer.CU_IsDormant;
            adminNote = customer.CU_AdminNote;
        }

        [Required(ErrorMessage = "Domain is required.")]
        [Range(1, Int16.MaxValue, ErrorMessage = "Domain is required.")]
        [Display(Name = "Domain")]
        public int DomainID { get; set; }
        public int CustomerID { get; set; }
        public bool Registered { get; set; }

        public bool IsDormant { get; set; }
        [MaxLength(350)]
        public string adminNote { get; set; }

        public AddressModel Address { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects.DataClasses;

namespace CMS.BL.Entity
{
    [MetadataType(typeof(tbl_ProdCategoriesData))]
    public partial class tbl_ProdCategories
    {
    }

    public class tbl_ProdCategoriesData
    {
        [Required(ErrorMessage = "Category Title is required.")]
        public object PC_Title;

        [Display(Name = "Tax Rate")]
        public object PC_TaxID;
    }
}
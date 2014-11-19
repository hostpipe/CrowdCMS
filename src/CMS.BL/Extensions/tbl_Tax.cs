using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CMS.BL.Entity
{
    [MetadataType(typeof(tbl_TaxData))]
    public partial class tbl_Tax
    {
    }

    public class tbl_TaxData
    {
        [Required(ErrorMessage = "Tax Title is required.")]
        [Display(Name = "Tax Title")]
        public object TA_Title;

        [Required(ErrorMessage = "Percentage is required.")]
        [Display(Name = "Percentage")]
        [Range(0, 100, ErrorMessage = "Percentage can have value between 0 and 100.")]
        public object TA_Percentage;
    }
}

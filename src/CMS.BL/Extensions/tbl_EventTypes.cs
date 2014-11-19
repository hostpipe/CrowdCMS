using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CMS.BL.Entity
{
    [MetadataType(typeof(tbl_EventTypesData))]
    public partial class tbl_EventTypes
    {
    }

    public class tbl_EventTypesData
    {
        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Name")]
        public object ET_Title;

        [Display(Name = "Description")]
        public object ET_Description;
    }
}

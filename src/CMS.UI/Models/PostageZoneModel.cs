using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class PostageZoneModel
    {
        public int PostageZoneID { get; set; }
        public int PZ_DomainID { get; set; }
        public string PZ_Name { get; set; }
        public bool PZ_IsDefault { get; set; }
    }
}
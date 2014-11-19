using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class PostageWeightModel
    {
        public int PostageWeightID { get; set; }
        public int PW_DomainID { get; set; }
        public decimal PW_Lower { get; set; }
        public decimal PW_Upper { get; set; }
    }
}
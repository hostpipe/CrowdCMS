using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class PaymentLogoModel
    {
        public int PaymentDomainID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string FilePath { get; set; }
        public int DomainID { get; set; }
    }
}
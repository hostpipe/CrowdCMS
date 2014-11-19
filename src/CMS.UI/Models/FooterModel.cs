using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class FooterModel
    {
        public List<WebsiteMenuModel> WebsiteMenu { get; set; }
        public List<PaymentLogoModel> PaymentMethods { get; set; }
        public bool UseTemplate { get; set; }
        public string FooterContent { get; set; }
    }
}
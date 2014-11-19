using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class WebsiteMenuModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public decimal Priority { get; set; }
        public string Target { get; set; }
        public DateTime ModificationDate { get; set; }
        public List<WebsiteMenuModel> SubMenuItems { get; set; }
    }
}
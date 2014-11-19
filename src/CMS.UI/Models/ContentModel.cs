using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class ContentModel
    {
        public tbl_Content Content { get; set; }
        public string CSS { get; set; }
        public string Html { get; set; }

        // object: string or FormModel types allowed
        public List<object> HtmlElements { get; set; }
    }
}
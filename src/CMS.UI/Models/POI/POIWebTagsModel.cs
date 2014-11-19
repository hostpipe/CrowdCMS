using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class POIWebFilterModel
    {
        public bool IsColumn1 { get; set; }
        public bool IsColumn2 { get; set; }

        public POIWebTagGroupModel Column1 { get; set; }
        public POIWebTagGroupModel Column2 { get; set; }
    }

    public class POIWebTagGroupModel
    {
        public string Label { get; set; }
        public SelectList Tags { get; set; }
    }
}
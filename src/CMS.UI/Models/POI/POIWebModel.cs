using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class POIWebModel
    {
        public int POIID { get; set; }
        public string Title { get; set; }
        public string CategoryTitle { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Phone { get; set; }
    }
}
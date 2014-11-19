using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class SocialModel
    {
        public int SocialID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public string IconClass { get; set; }
        public string DefaultForeColour { get; set; }
        public string DefaultBackColour { get; set; }
        public string ForeColour { get; set; }
        public string BackColour { get; set; }
        public bool Live { get; set; }
        public int DomainID { get; set; }
        public double? BorderRadius { get; set; }
        public double? DefaultBorderRadius { get; set; }
    }
}

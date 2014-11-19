using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.UI.Models
{
    public class SettingsValueModel
    {
        public int SettingsValueID { get; set; }
        public int SV_DomainID { get; set; }
        [AllowHtml]
        public string SV_Value { get; set; }
        public int SV_SettingsID { get; set; }
        public SettingsModel Settings { get; set; }
    }
}
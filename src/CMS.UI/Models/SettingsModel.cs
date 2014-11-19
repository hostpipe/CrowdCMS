using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class SettingsModel
    {
        public int SE_Type { get; set; }
        public int SE_Category { get; set; }
        public string SE_Variable { get; set; }
        public string SE_VariableLabel { get; set; }
        public string SE_Description { get; set; }
    }
}

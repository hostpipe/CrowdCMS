using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CMS.Utils.Extension
{
    public class ExtendedSelectListItem : SelectListItem
    {
        public int Level { get; set; }
        public int ID { get; set; }
    }
}

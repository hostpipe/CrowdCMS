using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace CMS.BL.Entity
{
    public partial class tbl_Image
    {
        public tbl_SiteMap SitemapLink
        {
            get
            {
                return this.tbl_SiteMap;
            }
        }

        public bool HasSitemapLink
        {
            get
            {
                return this.SitemapLink != null;
            }
        }
    }
}


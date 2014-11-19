using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace CMS.BL.Entity
{
    public partial class tbl_SiteMap
    {
        public bool IsDirectlyInMenu
        {
            get
            {
                return this.SM_MenuDisplayTypeID == (int)MenuDisplayType.Directly || this.SM_MenuDisplayTypeID == (int)MenuDisplayType.Both;
            }
        }

        public bool IsUnderParentInMenu
        {
            get
            {
                return this.SM_MenuDisplayTypeID == (int)MenuDisplayType.UnderParent || this.SM_MenuDisplayTypeID == (int)MenuDisplayType.Both;
            }
        }

        public bool IsType(ContentType type)
        {
            return this.tbl_ContentType.CTP_Value.Equals(type.ToString());
        }

        public EntityCollection<tbl_Image> ImageLinks
        {
            get
            {
                return this.tbl_Image;
            }
        }

        public EntityCollection<tbl_Image> SitemapImages
        {
            get
            {
                return this.tbl_Image1;
            }
        }

        public bool HasImages
        {
            get
            {
                return this.SitemapImages != null;
            }
        }
    }
}

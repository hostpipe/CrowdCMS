using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.BL.Entity
{
    public partial class tbl_GalleryImage
    {
        public string Reference
        {
            get
            {
                return String.Format("{0}-{1}", this._GI_GalleryID.ToString("D3"), this._GI_ImageID.ToString("D3"));
            }
        }

        public string AltTag
        {
            get
            {
                if (String.IsNullOrEmpty(this.tbl_Image.I_Description))
                {
                    return String.Format("{0} {1}", this.tbl_Gallery.G_Title, this.Reference);
                }
                return this.tbl_Image.I_Description;
            }
        }
    }
}

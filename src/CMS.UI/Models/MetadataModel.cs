using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class MetadataModel
    {
        public MetadataModel(tbl_Content content)
        {
            if (content == null)
                return;

            this.Keywords = content.C_Keywords;
            this.Description = content.C_Description;
            this.MetaTags = content.C_MetaData;
        }

        public MetadataModel(string metatags = "", string keywords = "", string description = "")
        {
            this.Keywords = keywords;
            this.Description = description;
            this.MetaTags = metatags;
        }

        public MetadataModel() { }

        public string Keywords { get; set; }
        public string Description { get; set; }
        public string MetaTags { get; set; }
    }
}
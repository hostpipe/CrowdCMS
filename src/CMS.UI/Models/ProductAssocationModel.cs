using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;
using CMS.BL.Entity;
using CMS.BL;

namespace CMS.UI.Models
{
    public class ProductAssocationModel
    {
        public List<tbl_ProdAss> ProdAss { get; set; }
        public List<tbl_ProdAss> ProdAss1 { get; set; }
        public int ProductID { get; set; }
        public ProductType Type { get; set; }
        public string Url { get; set; }
    }
}
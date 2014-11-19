using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.BL.Entity
{
    public partial class tbl_Basket
    {
        public bool IsDeliverable
        {
            get
            {
                return this.tbl_BasketContent.Any(bc => bc.tbl_ProductPrice != null && bc.tbl_ProductPrice.tbl_Products.P_Deliverable);
            }
        }
    }
}

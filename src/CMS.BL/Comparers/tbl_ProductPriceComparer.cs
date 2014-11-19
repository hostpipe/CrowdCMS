using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;

namespace CMS.BL.Comparers
{
    public class ProductPriceComparer : IEqualityComparer<tbl_ProductPrice>
    {
        public bool Equals(tbl_ProductPrice x, tbl_ProductPrice y)
        {
            return x.PR_EventEndDate == y.PR_EventEndDate && x.PR_EventStartDate == y.PR_EventStartDate;
        }

        public int GetHashCode(tbl_ProductPrice obj)
        {
            return (obj.PR_EventStartDate.GetValueOrDefault().Ticks + obj.PR_EventEndDate.GetValueOrDefault().Ticks).GetHashCode();
        }
    }
}

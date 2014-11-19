using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.BL.Entity
{
    public partial class tbl_Address
    {
        public string FullText
        {
            get
            {
                return this.A_Line1 + 
                    (!String.IsNullOrEmpty(this.A_Line2) ? " " + this.A_Line2 : String.Empty) + 
                    (!String.IsNullOrEmpty(this.A_Line3) ? " " + this.A_Line3 : String.Empty) + 
                    (!String.IsNullOrEmpty(this.A_Postcode) ? ", " + this.A_Postcode : String.Empty) + 
                    (!String.IsNullOrEmpty(this.A_Town) ? ", " + this.A_Town : String.Empty);
            }
        }
    }
}

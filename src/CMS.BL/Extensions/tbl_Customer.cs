using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.BL.Entity
{
    public partial class tbl_Customer
    {
        public string FullName
        {
            get
            {
                return String.Format("{0} {1} {2}", this.CU_Title, this.CU_FirstName, this.CU_Surname);
            }
        }

        public string FullText
        {
            get
            {
                return this.FullName + (!String.IsNullOrEmpty(this.CU_Email) ? " " + this.CU_Email : String.Empty) + (!String.IsNullOrEmpty(this.CU_Telephone) ? ", " + this.CU_Telephone : String.Empty);
            }
        }
    }
}

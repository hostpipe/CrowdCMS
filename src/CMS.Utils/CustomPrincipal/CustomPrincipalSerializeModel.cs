using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Utils
{
    public class CustomPrincipalSerializeModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int UserID { get; set; }
        public bool IsAdmn { get; set; }
    }
}

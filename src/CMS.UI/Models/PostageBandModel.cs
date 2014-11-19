using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class PostageBandModel
    {
        public int PostageBandID { get; set; }
        public int PB_DomainID { get; set; }
        public decimal PB_Lower { get; set; }
        public decimal PB_Upper { get; set; }
    }
}
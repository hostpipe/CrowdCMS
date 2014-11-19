using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace CMS.DAL
{
    public class DB : DbContext
    {
        public DB() : base("CMSEntities") { }
    }
}

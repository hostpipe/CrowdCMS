using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL;

namespace CMS.Services
{
    public class ServiceBase
    {
        protected DALContext Context { get; set; }

        public ServiceBase()
        {
            this.Context = new DALContext();
        }
    }
}

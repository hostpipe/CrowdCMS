using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;

namespace CMS.DAL.Interface
{
    public interface ICommonService
    {

        tbl_AdminUsers GetUserByUsername(string username);

    }
}

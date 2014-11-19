using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;

namespace CMS.Services
{
    public interface ITemplate
    {
        bool DeleteTempate(int templateID);
        List<tbl_Templates> GetAll();
        tbl_Templates GetByID(int templateID);
        tbl_Templates GetLive();
        tbl_Templates SaveTemplate(int templateID, string name, string header, bool useHeader, string footer, bool useFooter, bool live);
    }
}

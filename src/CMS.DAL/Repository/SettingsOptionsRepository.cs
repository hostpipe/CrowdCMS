using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ISettingsOptionsRepository
    {
        List<tbl_SettingsOptions> GetAllForSetting(int settingID);
    }

    public class SettingsOptionsRepository : Repository<tbl_SettingsOptions>, ISettingsOptionsRepository
    {
        public SettingsOptionsRepository(IDALContext context) : base(context) { }

        public List<tbl_SettingsOptions> GetAllForSetting(int settingID)
        {
            return this.DbSet.Where(m => m.SO_SettingID == settingID).ToList();
        }
    }
}

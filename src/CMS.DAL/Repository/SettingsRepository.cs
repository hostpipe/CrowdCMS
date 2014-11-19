using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ISettingsRepository
    {
        IQueryable<tbl_Settings> GetAll();
        tbl_Settings GetSettingsByID(int settingsID);
        tbl_Settings GetValueByKey(SettingsKey key);
        tbl_Settings GetSettingsByValue(string value);
        bool SaveSettings(Dictionary<string, string> settings);
    }

    public class SettingsRepository : Repository<tbl_Settings>, ISettingsRepository
    {
        public SettingsRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_Settings> GetAll()
        {
            return this.All();
        }

        public tbl_Settings GetSettingsByID(int settingsID)
        {
            return this.DbSet.FirstOrDefault(s => s.SettingID == settingsID);
        }

        public tbl_Settings GetValueByKey(SettingsKey key)
        {
            string skey = key.ToString();
            return this.DbSet.FirstOrDefault(s => s.SE_Variable.Equals(skey));
        }

        public tbl_Settings GetSettingsByValue(string value)
        {
            return this.DbSet.FirstOrDefault(s => s.SE_Value.Equals(value));
        }

        public bool SaveSettings(Dictionary<string, string> settings)
        {
            foreach (var key in settings.Keys)
            {
                var item = this.DbSet.FirstOrDefault(s => s.SE_Variable.Equals(key));
                if (item != null)
                    item.SE_Value = settings[key];
            }

            this.Context.SaveChanges();
            return true;
        }
    }
}

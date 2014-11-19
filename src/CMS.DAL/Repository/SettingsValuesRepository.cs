using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ISettingsValuesRepository
    {
        IQueryable<tbl_SettingsValue> GetAll(int domainID);
        tbl_SettingsValue GetByID(int ID);
        bool Save(List<tbl_SettingsValue> model, int domainID);

        tbl_SettingsValue GetValueByKey(SettingsKey key, int domainID);
    }

    public class SettingsValuesRepository : Repository<tbl_SettingsValue>, ISettingsValuesRepository
    {
        public SettingsValuesRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_SettingsValue> GetAll(int domainID = 0)
        {
            if(domainID == 0)
                return this.All();
            return this.DbSet.Where(m => m.SV_DomainID == domainID);
        }

        public tbl_SettingsValue GetByID(int ID)
        {
            return this.DbSet.FirstOrDefault(m=>m.SettingsValueID == ID);
        }

        public tbl_SettingsValue GetValueByKey(SettingsKey key, int domainID)
        {
            string skey = key.ToString();
            return this.DbSet.FirstOrDefault(s => s.SV_DomainID == domainID && s.tbl_Settings.SE_Variable.Equals(skey));
        }


        public bool Save(List<tbl_SettingsValue> model, int domainID)
        {
            if (model == null)
                return false;

            foreach (var item in model)
            {
                var it = this.DbSet.FirstOrDefault(m => (m.SettingsValueID == item.SettingsValueID || m.SV_SettingsID == item.SV_SettingsID) && m.SV_DomainID == domainID);
                if (it != null)
                {
                    it.SV_Value = item.SV_Value;
                }
                else
                {
                    item.SV_DomainID = domainID;
                    this.DbSet.Add(item);
                }
            }
            this.Context.SaveChanges();
            return true;
        }
    }
}

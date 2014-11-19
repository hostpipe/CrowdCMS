using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IFormRepository : IRepository<tbl_Form>
    {
        tbl_Form GetByID(int id);
        bool DeleteByID(int id);
        IQueryable<tbl_Form> GetByDomainID(int domainID);
        IQueryable<tbl_Form> GetAll();
        tbl_Form SaveForm(string name, string description, int domainID, int formID, bool captcha = true, int? sitemapID = null, string tracking = null);
    }

    public class FormRepository : Repository<tbl_Form>, IFormRepository
    {
        public FormRepository(IDALContext context) : base(context) { }


        public IQueryable<tbl_Form> GetByDomainID(int domainID)
        {
            return this.DbSet.Where(f => !f.F_Deleted && f.F_DomainID == domainID);
        }

        public tbl_Form GetByID(int id)
        {
            return this.DbSet.FirstOrDefault(f => !f.F_Deleted && f.FormID == id);
        }

        public bool DeleteByID(int id)
        {
            var formItem = this.DbSet.FirstOrDefault(i => i.FormID == id);
            if (formItem == null)
                return false;

            formItem.F_Deleted = true;
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Form> GetAll()
        {
            return this.DbSet.Where(f => !f.F_Deleted);
        }

        public tbl_Form SaveForm(string name, string description, int domainID, int formID, bool captcha = true, int? sitemapID = null, string tracking = null)
        {
            var form = this.DbSet.FirstOrDefault(f => f.FormID == formID);
            if (form == null)
            {
                form = new tbl_Form()
                {
                    F_Deleted = false
                };
                this.Create(form);
            }


            form.F_Name = name;
            form.F_Description = description;
            form.F_DomainID = domainID;
            form.F_DestinationSitemapID = sitemapID;
            form.F_Tracking = tracking;
            form.F_Captcha = captcha;

            this.Context.SaveChanges();
            return form;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IFormSubmissionRepository : IRepository<tbl_FormSubmission>
    {
        tbl_FormSubmission GetByID(int id);
        bool DeleteByID(int id);
        IQueryable<tbl_FormSubmission> GetAll();
        IQueryable<tbl_FormSubmission> GetByFormID(int formID);
        tbl_FormSubmission GetNewest();
        tbl_FormSubmission MarkAsRead(int formSubmissionID);
        tbl_FormSubmission SaveFormSubmission(string value, string recipients, DateTime date, int formID, int id = 0);
    }

    public class FormSubmissionRepository : Repository<tbl_FormSubmission>, IFormSubmissionRepository
    {
        public FormSubmissionRepository(IDALContext context) : base(context) { }

        public tbl_FormSubmission GetByID(int id)
        {
            return this.DbSet.FirstOrDefault(fs => fs.FormSubmissionID == id);
        }

        public bool DeleteByID(int id)
        {
            var formSumission = this.DbSet.FirstOrDefault(fs => fs.FormSubmissionID == id);
            if (formSumission == null)
                return false;

            this.Delete(formSumission);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_FormSubmission> GetAll()
        {
            return this.All();
        }

        public IQueryable<tbl_FormSubmission> GetByFormID(int formID)
        {
            return this.DbSet.Where(fs => fs.FS_FormID == formID);
        }

        public tbl_FormSubmission GetNewest()
        {
            return this.DbSet.OrderByDescending(fs => fs.FS_Date).FirstOrDefault();
        }

        public tbl_FormSubmission MarkAsRead(int formSubmissionID)
        {
            var formSubmission = this.DbSet.FirstOrDefault(fs => fs.FormSubmissionID == formSubmissionID);
            if (formSubmission == null)
            {
                return null;
            }

            formSubmission.FS_Read = true;
            this.Context.SaveChanges();
            return formSubmission;
        }

        public tbl_FormSubmission SaveFormSubmission(string value, string recipients, DateTime date, int formID, int id = 0)
        {
            var formSubmission = this.DbSet.FirstOrDefault(fs => fs.FormSubmissionID == id);
            if (formSubmission == null)
            {
                formSubmission = new tbl_FormSubmission();
                formSubmission.FS_Read = false;

                this.Create(formSubmission);
            }

            formSubmission.FS_Date = date;
            formSubmission.FS_Value = value;
            formSubmission.FS_Email = recipients;
            formSubmission.FS_FormID = formID;

            this.Context.SaveChanges();
            return formSubmission;
        }
    }
}

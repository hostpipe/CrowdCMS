using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{

    public interface IFormItemValuesRepository: IRepository<tbl_FormItemValues>
    {
        tbl_FormItemValues GetByID(int id);
        bool DeleteByID(int id);
        IQueryable<tbl_FormItemValues> GetAll();        
        tbl_FormItemValues SaveFormItemValue(int value, string text, bool selected, int order, int formItemID, int formItemValueID);
    }

    public class FormItemValuesRepository : Repository<tbl_FormItemValues>, IFormItemValuesRepository
    {
        public FormItemValuesRepository(IDALContext context) : base(context) { }

        public tbl_FormItemValues GetByID(int id)
        {
            return this.DbSet.FirstOrDefault(fiv => fiv.FormItemValueID == id);
        }

        public bool DeleteByID(int id)
        {
            var value = this.DbSet.FirstOrDefault(fiv => fiv.FormItemValueID == id);
            if (value == null)
                return false;

            this.Delete(value);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_FormItemValues> GetAll()
        {
            return this.All();
        }

        public tbl_FormItemValues SaveFormItemValue(int value, string text, bool selected, int order, int formItemID, int formItemValueID)
        {
            var formItemValue = this.DbSet.FirstOrDefault(fiv => fiv.FormItemValueID == formItemValueID);
            if (formItemValue == null)
            {
                formItemValue = new tbl_FormItemValues()
                {
                    FIV_FormItemID = formItemID
                };
                this.Create(formItemValue);
            }

            formItemValue.FIV_Order = order;
            formItemValue.FIV_Selected = selected;
            formItemValue.FIV_Text = text;
            formItemValue.FIV_Value = value;

            this.Context.SaveChanges();
            return formItemValue;
        }
    }
}

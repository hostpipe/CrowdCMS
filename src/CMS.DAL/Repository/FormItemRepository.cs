using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IFormItemRepository : IRepository<tbl_FormItem>
    {
        tbl_FormItem GetByID(int id);
        bool DeleteByID(int id);
        IQueryable<tbl_FormItem> GetAll();
        IQueryable<tbl_FormItem> GetAllLiveByFormID(int formID);
        IQueryable<tbl_FormItemType> GetAllFormItemTypes();
        tbl_FormItemType GetFormItemTypeByName(string name);

        tbl_FormItem SaveFormItem(string name, string text, int itemTypeID, bool required, int formID, int formItemID, string placeholder);
        tbl_FormItem SaveVisibility(int formItemID);
        bool SaveOrder(int[] orderedContactItemIDs);
    }

    public class FormItemRepository : Repository<tbl_FormItem>, IFormItemRepository
    {
        public FormItemRepository(IDALContext context) : base(context) { }


        public tbl_FormItem GetByID(int id)
        {
            return this.DbSet.FirstOrDefault(i => i.FormItemID == id);
        }

        public bool DeleteByID(int id)
        {
            var formItem = this.DbSet.FirstOrDefault(i => i.FormItemID == id);
            if (formItem == null)
                return false;
            
            while (formItem.tbl_FormItemValues.Count > 0)
            {
                Context.Set<tbl_FormItemValues>().Remove(formItem.tbl_FormItemValues.First());
            }
            this.Delete(formItem);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_FormItem> GetAll()
        {
            return this.All();
        }

        public IQueryable<tbl_FormItem> GetAllLiveByFormID(int formID)
        {
            return this.DbSet.Where(fi => fi.FI_FormID == formID && fi.FI_Live).OrderBy(fi => fi.FI_Order);
        }

        public IQueryable<tbl_FormItemType> GetAllFormItemTypes()
        {
            return this.Context.Set<tbl_FormItemType>().AsQueryable();
        }

        public tbl_FormItemType GetFormItemTypeByName(string name)
        {
            return this.Context.Set<tbl_FormItemType>().FirstOrDefault(fit => fit.FIT_Name.Equals(name));
        }

        public tbl_FormItem SaveFormItem(string name, string text, int itemTypeID, bool required, int formID, int formItemID, string placeholder)
        {
            var dbFormItem = this.DbSet.FirstOrDefault(i => i.FormItemID == formItemID);
            if (dbFormItem == null)
            {
                dbFormItem = new tbl_FormItem()
                {
                    FI_Live = false,
                    FI_Order = this.DbSet.Any() ? this.DbSet.Max(i => i.FI_Order) + 1 : 1
                };
                this.Create(dbFormItem);
            }
            
            dbFormItem.FI_FormID = formID;
            dbFormItem.FI_ItemTypeID = itemTypeID;
            dbFormItem.FI_Name = name;
            dbFormItem.FI_Text = text;
            dbFormItem.FI_Required = required;
            dbFormItem.FI_Placeholder = placeholder;

            this.Context.SaveChanges();
            return dbFormItem;
        }

        public tbl_FormItem SaveVisibility(int formItemID)
        {
            var formItem = DbSet.FirstOrDefault(i => i.FormItemID == formItemID);
            if (formItem == null)
                return null;

            formItem.FI_Live = !formItem.FI_Live;
            this.Context.SaveChanges();
            return formItem;
        }

        public bool SaveOrder(int[] orderedContactItemIDs)
        {
            if (orderedContactItemIDs == null) return false;

            for (int i = 0; i < orderedContactItemIDs.Count(); i++)
            {
                var formItemID = orderedContactItemIDs[i];
                var formItem = this.DbSet.FirstOrDefault(fi => fi.FormItemID == formItemID);
                if (formItem != null) formItem.FI_Order = i + 1;
            }

            this.Context.SaveChanges();
            return true;
        }
    }
}

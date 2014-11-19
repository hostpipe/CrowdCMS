using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using CMS.BL;

namespace CMS.DAL.Repository
{
    public interface IProdCategoriesRepository
    {
        bool DeleteProdCategory(int prodCategoryID);
        IQueryable<tbl_ProdCategories> GetAll();
        tbl_ProdCategories GetByID(int prodCategoryID);
        tbl_ProdCategories SaveProdCategory(string title, bool live, int? parentID, int? taxID, int categoryID, ProductType type, bool featured = false);
        tbl_ProdCategories SaveProdCategoryImage(int categoryID, int? imageID);
        bool SaveOrder(int[] orderedCaegoriesIDs);
    }

    public class ProdCategoriesRepository : Repository<tbl_ProdCategories>, IProdCategoriesRepository
    {
        public ProdCategoriesRepository(IDALContext context) : base(context) { }

        public bool DeleteProdCategory(int prodCategoryID)
        {
            var prodCategory = this.DbSet.FirstOrDefault(pc => pc.CategoryID == prodCategoryID);
            if (prodCategory == null)
                return false;
            if (prodCategory.tbl_Products.Where(m => m.P_Deleted == false).Count() > 0 | prodCategory.tbl_ProdCategories1.Where(m=>m.PC_Deleted == false).Count() > 0)
                return false;
            prodCategory.PC_Deleted = true;

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_ProdCategories> GetAll()
        {
            return this.All();
        }

        public tbl_ProdCategories GetByID(int prodCategoryID)
        {
            return this.DbSet.FirstOrDefault(pc => pc.CategoryID == prodCategoryID);
        }

        public tbl_ProdCategories SaveProdCategory(string title, bool live, int? parentID, int? taxID, int categoryID, ProductType type, bool featured = false)
        {
            var category = this.DbSet.FirstOrDefault(c => c.CategoryID == categoryID);
            var order = this.DbSet.OrderByDescending(c => c.PC_Order).Select(c => c.PC_Order).FirstOrDefault();
            if (category == null)
            {
                category = new tbl_ProdCategories()
                {
                    CategoryID = categoryID,
                    PC_Order = (short)(1 + order)
                };
                this.Create(category);
            }

            category.PC_Live = live;
            category.PC_ParentID = parentID;
            category.PC_Title = title;
            category.PC_TaxID = taxID;
            category.PC_ProductTypeID = (int)type;
            category.PC_Featured = featured;

            this.Context.SaveChanges();
            return category;
        }

        public tbl_ProdCategories SaveProdCategoryImage(int categoryID, int? imageID)
        {
            var category = this.DbSet.FirstOrDefault(c => c.CategoryID == categoryID);
            if (category == null)
                return null;

            category.PC_ImageID = imageID;
            this.Context.SaveChanges();
            return category;
        }

        public bool SaveOrder(int[] orderedCaegoriesIDs)
        {
            if (orderedCaegoriesIDs == null)
                return false;

            for (int i = 0; i < orderedCaegoriesIDs.Count(); i++)
            {
                var categoryID = orderedCaegoriesIDs[i];
                var category = this.DbSet.FirstOrDefault(c => c.CategoryID == categoryID);
                category.PC_Order = (short)(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }
    }
}

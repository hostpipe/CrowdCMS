using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPOICategoriesRepository
    {
        bool Delete(int poiCategoryID);
        IQueryable<tbl_POICategories> GetAll();
        tbl_POICategories GetByTitle(string title);
        tbl_POICategories GetByID(int poiCategoryID);
        tbl_POICategories Save(string title, bool isLive, int poiCategoryID);
    }

    public class POICategoriesRepository : Repository<tbl_POICategories>, IPOICategoriesRepository
    {
        public POICategoriesRepository(IDALContext context) : base(context) { }

        public bool Delete(int poiCategoryID)
        {
            var category = this.DbSet.FirstOrDefault(c => c.POICategoryID == poiCategoryID);
            if (category == null)
                return false;

            category.POIC_Delete = true;

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_POICategories> GetAll()
        {
            return this.DbSet.Where(c => !c.POIC_Delete).AsQueryable();
        }

        public tbl_POICategories GetByID(int poiCategoryID)
        {
            return this.DbSet.FirstOrDefault(c => c.POICategoryID == poiCategoryID);
        }

        public tbl_POICategories GetByTitle(string title)
        {
            return this.DbSet.FirstOrDefault(c => c.POIC_Title.Equals(title));
        }

        public tbl_POICategories Save(string title, bool isLive, int poiCategoryID)
        {
            var category = this.DbSet.FirstOrDefault(c => c.POICategoryID == poiCategoryID);
            if (category == null)
            {
                category = new tbl_POICategories();
                this.Create(category);
            }

            category.POIC_Title = title;
            category.POIC_IsLive = isLive;
            
            this.Context.SaveChanges();
            return category;
        }
    }
}

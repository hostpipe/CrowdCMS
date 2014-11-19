using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ICategoriesRepository
    {
        bool DeleteCategory(int categoryID);
        IQueryable<tbl_Categories> GetAll();
        tbl_Categories GetByID(int categoryID);
        tbl_Categories GetByURL(string url);
        tbl_Categories SaveCategory(string title, string url, int categoryID);
    }

    public class CategoriesRepository : Repository<tbl_Categories>, ICategoriesRepository
    {
        public CategoriesRepository(IDALContext context) : base(context) { }

        public bool DeleteCategory(int categoryID)
        {
            var category = this.DbSet.FirstOrDefault(c => c.CategoryID == categoryID);
            if (category == null)
                return false;

            if (category.tbl_NewsCategories.Where(n => !n.tbl_SiteMap.SM_Deleted).Count() > 0)
                return false;

            foreach (var newsCategory in category.tbl_NewsCategories.ToList())
            {
                this.Context.Set<tbl_NewsCategories>().Remove(newsCategory);
            }

            this.Delete(category);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Categories> GetAll()
        {
            return this.All();
        }

        public tbl_Categories GetByID(int categoryID)
        {
            return this.DbSet.FirstOrDefault(c => c.CategoryID == categoryID);
        }

        public tbl_Categories GetByURL(string url)
        {
            return this.DbSet.FirstOrDefault(t => t.CA_URL.Equals(url));
        }

        public tbl_Categories SaveCategory(string title, string url, int categoryID)
        {
            var category = this.DbSet.FirstOrDefault(c => c.CA_Title.Equals(title));
            if (category != null)
                return category;

            category = this.DbSet.FirstOrDefault(c => c.CategoryID == categoryID);
            if (category == null) {
                category = new tbl_Categories();
                this.Create(category);
            }

            category.CA_Title = title;
            category.CA_URL = url;

            this.Context.SaveChanges();
            return category;
        }
    }
}

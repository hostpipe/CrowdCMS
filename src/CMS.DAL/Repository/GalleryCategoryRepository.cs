using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IGalleryCategoryRepository
    {
        bool DeleteGalleryCategory(int categoryID);
        IQueryable<tbl_GalleryCategory> GetAll();
        tbl_GalleryCategory GetByID(int categoryID);
        tbl_GalleryCategory SaveGalleryCategory(string catTitle, int categoryID);
    }

    public class GalleryCategoryRepository : Repository<tbl_GalleryCategory>, IGalleryCategoryRepository
    {
        public GalleryCategoryRepository(IDALContext context) : base(context) { }

        public bool DeleteGalleryCategory(int categoryID)
        {
            tbl_GalleryCategory galleryCategory = this.DbSet.FirstOrDefault(gi => gi.GalleryCategoryID == categoryID);
            if (galleryCategory == null)
                return false;

            this.Delete(galleryCategory);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_GalleryCategory> GetAll()
        {
            return this.All();
        }

        public tbl_GalleryCategory GetByID(int categoryID)
        {
            return this.DbSet.FirstOrDefault(gi => gi.GalleryCategoryID == categoryID);
        }

        public tbl_GalleryCategory SaveGalleryCategory(string catTitle, int categoryID)
        {
            tbl_GalleryCategory galleryCategory = this.DbSet.FirstOrDefault(gi => gi.GalleryCategoryID == categoryID);
            if (galleryCategory == null)
            {
                galleryCategory = new tbl_GalleryCategory();
                this.Create(galleryCategory);
            }

            galleryCategory.GC_Title = catTitle;

            this.Context.SaveChanges();
            return galleryCategory;
        }


    }
}

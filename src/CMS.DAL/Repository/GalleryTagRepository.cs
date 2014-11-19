using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IGalleryTagRepository
    {
        tbl_GalleryTags SaveTag(string title, string cls, bool isImageTag, int tagID);
        IQueryable<tbl_GalleryTags> GetAll();
        IQueryable<tbl_GalleryTags> GetAllGalleryTags();
        IQueryable<tbl_GalleryTags> GetAllImagesTags();
        tbl_GalleryTags GetByID(int tagID);
        bool DeleteTag(int tagID);
    }

    public class GalleryTagRepository : Repository<tbl_GalleryTags>, IGalleryTagRepository
    {
        public GalleryTagRepository(IDALContext context) : base(context) { }

        public tbl_GalleryTags SaveTag(string title, string cls, bool isImageTag, int tagID)
        {
            var galleryTag = this.DbSet.FirstOrDefault(gt => gt.GT_Title.Equals(title) && gt.GT_IsImageTag == isImageTag);
            if (galleryTag != null)
                return galleryTag;

            galleryTag = this.DbSet.FirstOrDefault(gt => gt.GalleryTagID == tagID);
            if (galleryTag == null)
            {
                galleryTag = new tbl_GalleryTags();
                this.Create(galleryTag);
            }

            galleryTag.GT_Title = title;
            galleryTag.GT_Class = cls;
            galleryTag.GT_IsImageTag = isImageTag;

            this.Context.SaveChanges();
            return galleryTag;
        }

        public IQueryable<tbl_GalleryTags> GetAll()
        {
            return this.All();
        }

        public IQueryable<tbl_GalleryTags> GetAllGalleryTags()
        {
            return this.DbSet.Where(t => !t.GT_IsImageTag);
        }

        public IQueryable<tbl_GalleryTags> GetAllImagesTags()
        {
            return this.DbSet.Where(t => t.GT_IsImageTag);
        }

        public tbl_GalleryTags GetByID(int tagID)
        {
            return this.DbSet.FirstOrDefault(gt => gt.GalleryTagID == tagID);
        }

        public bool DeleteTag(int tagID)
        {
            var galleryTag = this.DbSet.FirstOrDefault(gt => gt.GalleryTagID == tagID);
            if (galleryTag == null)
                return false;

            foreach (var tag in galleryTag.tbl_GalleryTagLink.ToList())
            {
                this.Context.Set<tbl_GalleryTagLink>().Remove(tag);
            }
            galleryTag.tbl_Image.Clear();

            this.Delete(galleryTag);
            this.Context.SaveChanges();
            return true;
        }
    }
}
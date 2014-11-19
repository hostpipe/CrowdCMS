using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IGalleryImageRepository
    {
        bool DeleteGalleryImage(int galleryImageID);
        bool DeleteGalleryImages(int galleryID);
        IQueryable<tbl_GalleryImage> GetAll();
        IQueryable<tbl_GalleryImage> GetAll(int galleryID);
        tbl_GalleryImage GetByID(int galleryImageID);
        tbl_GalleryImage SaveGalleryImage(int galleryID, int imageID, int galleryImageID);
        bool SaveOrder(int[] orderedGalleryImageIDs);
    }

    public class GalleryImageRepository : Repository<tbl_GalleryImage>, IGalleryImageRepository
    {
        public GalleryImageRepository(IDALContext context) : base(context) { }

        public bool DeleteGalleryImage(int galleryImageID)
        {
            var galleryImage = this.DbSet.FirstOrDefault(gi => gi.GalleryImageID == galleryImageID);
            if (galleryImage == null)
                return false;

            this.Delete(galleryImage);
            this.Context.SaveChanges();
            return true;
        }

        public bool DeleteGalleryImages(int galleryID)
        {
            IQueryable<tbl_GalleryImage> galleryImages = this.DbSet.Where(gi => gi.GI_GalleryID == galleryID);
            if (galleryImages == null)
                return false;

            foreach (tbl_GalleryImage galleryImage in galleryImages)
            {
                this.Delete(galleryImage);
            }

            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_GalleryImage> GetAll()
        {
            return this.All().OrderBy(gi => gi.GI_Order);
        }

        public IQueryable<tbl_GalleryImage> GetAll(int galleryID)
        {
            return this.All().Where(gi => gi.GI_GalleryID == galleryID).OrderBy(gi => gi.GI_Order);
        }

        public tbl_GalleryImage GetByID(int galleryImageID)
        {
            return this.DbSet.FirstOrDefault(gi => gi.GalleryImageID == galleryImageID);
        }

        public tbl_GalleryImage SaveGalleryImage(int galleryID, int imageID, int galleryImageID)
        {
            tbl_GalleryImage galleryImage = this.DbSet.FirstOrDefault(gi => gi.GalleryImageID == galleryImageID);
            var order = this.DbSet.Where(gi => gi.GI_GalleryID == galleryID).OrderByDescending(gi => gi.GI_Order).Select(gi => gi.GI_Order).FirstOrDefault();
            if (galleryImage == null)
            {
                galleryImage = new tbl_GalleryImage()
                {
                    GI_Order = (short)(1 + order)
                };
                this.Create(galleryImage);
            }

            galleryImage.GI_GalleryID = galleryID;
            galleryImage.GI_ImageID = imageID;

            this.Context.SaveChanges();
            return galleryImage;
        }

        public bool SaveOrder(int[] orderedGalleryImageIDs)
        {
            if (orderedGalleryImageIDs == null)
                return false;

            for (int i = 0; i < orderedGalleryImageIDs.Count(); i++)
            {
                int galleryImageID = orderedGalleryImageIDs[i];
                tbl_GalleryImage galleryImage = this.DbSet.FirstOrDefault(gi => gi.GalleryImageID == galleryImageID);
                galleryImage.GI_Order = (short)(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }

    }
}

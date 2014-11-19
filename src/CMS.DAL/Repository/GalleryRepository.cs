using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IGalleryRepository
    {
        bool DeleteGallery(int galleryID);
        IQueryable<tbl_Gallery> GetAll();
        tbl_Gallery GetByID(int galleryID);
        tbl_Gallery SaveGallery(string title, bool live, int customerID, int galleryCategoryID, int galleryID);
        bool SaveOrder(int[] orderedGalleryIDs);
        tbl_Gallery SaveVisibility(int galleryID);
        bool AddTag(int galleryID, int tagID);
        bool SaveTags(int galleryID, int[] tagIDs);
    }

    public class GalleryRepository : Repository<tbl_Gallery>, IGalleryRepository
    {
        public GalleryRepository(IDALContext context) : base(context) { }

        public bool DeleteGallery(int galleryID)
        {
            var gallery = this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
            if (gallery == null)
                return false;

            this.Delete(gallery);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Gallery> GetAll()
        {
            return this.All().OrderBy(g => g.G_Order);
        }

        public tbl_Gallery GetByID(int galleryID)
        {
            return this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
        }

        public tbl_Gallery SaveGallery(string title, bool live,int customerID, int galleryCategoryID, int galleryID)
        {
            tbl_Gallery gallery = this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
            var order = this.DbSet.OrderByDescending(g => g.G_Order).Select(g => g.G_Order).FirstOrDefault();
            if (gallery == null)
            {
                gallery = new tbl_Gallery()
                {
                    GalleryID = galleryID, //mapped from to SitemapID
                    G_Order = (short)(1 + order)
                };
                this.Create(gallery);
            }

            gallery.G_Live = live;
            gallery.G_Title = title;
            gallery.G_CustomerID = customerID;
            gallery.G_GalleryCategoryID = galleryCategoryID;

            this.Context.SaveChanges();
            return gallery;
        }

        public bool SaveOrder(int[] orderedGalleryIDs)
        {
            if (orderedGalleryIDs == null)
                return false;

            for (int i = 0; i < orderedGalleryIDs.Count(); i++)
            {
                int galleryID = orderedGalleryIDs[i];
                tbl_Gallery gallery = this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
                gallery.G_Order = (short)(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }

        public tbl_Gallery SaveVisibility(int galleryID)
        {
            tbl_Gallery gallery = this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
            if (gallery == null)
                return null;

            gallery.G_Live = !gallery.G_Live;
            this.Context.SaveChanges();
            return gallery;
        }

        public bool AddTag(int galleryID, int tagID)
        {
            var gallery = this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
            if (gallery == null)
                return false;

            if (!gallery.tbl_GalleryTagLink.Select(gt => gt.GT_TagID).Contains(tagID))
            {
                gallery.tbl_GalleryTagLink.Add(new tbl_GalleryTagLink { GT_GalleryID = galleryID, GT_TagID = tagID });
                this.Context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool SaveTags(int galleryID, int[] tagIDs)
        {
            var gallery = this.DbSet.FirstOrDefault(g => g.GalleryID == galleryID);
            if (gallery == null)
                return false;

            foreach (var tag in gallery.tbl_GalleryTagLink)
            {
                this.Context.Set<tbl_GalleryTagLink>().Remove(tag);
            }
            this.Context.SaveChanges();

            if (tagIDs != null)
            {
                foreach (var tagID in tagIDs)
                {
                    gallery.tbl_GalleryTagLink.Add(new tbl_GalleryTagLink { GT_GalleryID = galleryID, GT_TagID = tagID });

                }
            }

            this.Context.SaveChanges();
            return true;
        }
    }
}

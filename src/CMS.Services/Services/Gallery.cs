using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;
using CMS.Utils;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;

namespace CMS.Services
{
    public class Gallery : ServiceBase, IGallery
    {
        private IGalleryRepository GalleryRepository { get; set; }
        private IGalleryImageRepository GalleryImageRepository { get; set; }
        private IImageRepository ImageRepository { get; set; }
        private IGalleryCategoryRepository GalleryCategoryRepository { get; set; }
        private IGalleryTagRepository GalleryTagRepository { get; set; }

         public Gallery()
            : base()
        {
            this.GalleryRepository = new GalleryRepository(this.Context);
            this.GalleryImageRepository = new GalleryImageRepository(this.Context);
            this.ImageRepository = new ImageRepository(this.Context);
            this.GalleryCategoryRepository = new GalleryCategoryRepository(this.Context);
            this.GalleryTagRepository = new GalleryTagRepository(this.Context);
        }

        #region Gallery

        public bool DeleteGallery(int galleryID) //Delete portfolio Images, image link and portfolio
        {
            var galleryImages = this.GalleryImageRepository.GetAll(galleryID);
            if (galleryImages != null)
            {
                List<int> ImageIDs = galleryImages.Select(gi => gi.GI_ImageID).ToList();
                this.GalleryImageRepository.DeleteGalleryImages(galleryID);
                foreach (int ImageID in ImageIDs)
                {
                    this.ImageRepository.DeleteImage(ImageID);
                }
            }
            return this.GalleryRepository.DeleteGallery(galleryID);
        }


        public List<tbl_Gallery> GetAll()
        {
            return this.GalleryRepository.GetAll().ToList();
        }

        public List<tbl_Gallery> GetAllPublic()
        {
            return this.GalleryRepository.GetAll().Where(g => g.G_CustomerID == 0 && g.G_Live).OrderBy(g => g.G_Order).ToList();
        }

        public List<tbl_Gallery> GetCustomerGalleries(int customerID)
        {
            return this.GalleryRepository.GetAll().Where(g => g.G_CustomerID == customerID).ToList();
        }

        public tbl_Gallery GetByID(int galleryID)
        {
            return this.GalleryRepository.GetByID(galleryID);
        }

        public tbl_Gallery GetLive()
        {
            return this.GalleryRepository.GetAll().FirstOrDefault(g => g.G_Live);
        }

        public tbl_Gallery SaveGallery(string title, bool live,int customerID, int galleryCategoryID, int galleryID)
        {
            return this.GalleryRepository.SaveGallery(title, live,customerID, galleryCategoryID, galleryID);
        }

        public bool SaveGalleryOrder(int[] orderedIDs)
        {
            return this.GalleryRepository.SaveOrder(orderedIDs);
        }

        public tbl_Gallery SaveGalleryVisibility(int galleryID)
        {
            return this.GalleryRepository.SaveVisibility(galleryID);
        }

        public bool DeleteGalleryImage(int galleryImageID) //Delete gallery images and image links
        {
            var galleryImage = this.GalleryImageRepository.GetByID(galleryImageID);
            if (galleryImage == null)
                return false;

            return this.GalleryImageRepository.DeleteGalleryImage(galleryImageID) && this.ImageRepository.DeleteImage(galleryImage.GI_ImageID);
        }

        #endregion

        #region Gallery Image

        public List<tbl_GalleryImage> GetAllGalleryImages()
        {
            return GalleryImageRepository.GetAll().ToList();
        }
        public List<tbl_GalleryImage> GetAllGalleryImages(int galleryID)
        {
            return GalleryImageRepository.GetAll().Where(gi => gi.GI_GalleryID==galleryID).ToList();
        }

        public tbl_GalleryImage GetGalleryImageByID(int imageID)
        {
            return GalleryImageRepository.GetByID(imageID);
        }

        public tbl_GalleryImage SaveGalleryImage(int galleryID, int imageID, int galleryImageID)
        {
            return GalleryImageRepository.SaveGalleryImage(galleryID, imageID, galleryImageID);
        }

        public bool SaveGalleryImageOrder(int[] orderedGalleryImageIDs)
        {
            return this.GalleryImageRepository.SaveOrder(orderedGalleryImageIDs);
        }

        #endregion

        #region Gallery Category

        public List<tbl_GalleryCategory> GetAllGalleryCategoriesOrdered()
        {
            return GalleryCategoryRepository.GetAll().OrderBy(gc => gc.GC_Title).ToList();
        }

        public SelectList GetAllGalleryCategoriesAsSelectList(int selectedgalleryCategoryID)
        {
            return new SelectList(this.GetAllGalleryCategoriesOrdered(), "GalleryCategoryID", "GC_Title", selectedgalleryCategoryID);
        }

        public tbl_GalleryCategory GetGalleryCategory(int categoryID)
        {
            return GalleryCategoryRepository.GetByID(categoryID);
        }

        public tbl_GalleryCategory SaveGalleryCategory(string catTitle, int categoryID)
        {
            return GalleryCategoryRepository.SaveGalleryCategory(catTitle, categoryID);
        }

        public bool DeleteGalleryCategory(int categoryID)
        {
            return GalleryCategoryRepository.DeleteGalleryCategory(categoryID);
        }

        #endregion

        #region Gallery Tags

        public List<tbl_GalleryTags> GetAllTags()
        {
            return GalleryTagRepository.GetAll().ToList();
        }

        public List<tbl_GalleryTags> GetAllGalleryTags()
        {
            return GalleryTagRepository.GetAllGalleryTags().ToList();
        }

        public List<tbl_GalleryTags> GetAllImagesTags()
        {
            return GalleryTagRepository.GetAllImagesTags().ToList();
        }

        public List<SelectListItem> GetAllTagsByGalleryID(int galleryID)
        {
            return GalleryTagRepository.GetAllGalleryTags().Select(gt =>
                new SelectListItem
                {
                    Text = gt.GT_Title,
                    Value = SqlFunctions.StringConvert((double)gt.GalleryTagID).Trim(),
                    Selected = gt.tbl_GalleryTagLink.Any(gtl => gtl.GT_GalleryID == galleryID)
                }).ToList();
        }

        public List<SelectListItem> GetAllTagsByImageID(int imageID)
        {
            return GalleryTagRepository.GetAllImagesTags().Select(gt =>
                new SelectListItem
                {
                    Text = gt.GT_Title,
                    Value = SqlFunctions.StringConvert((double)gt.GalleryTagID).Trim(),
                    Selected = gt.tbl_Image.Any(i => i.ImageID == imageID)
                }).ToList();
        }

        public tbl_GalleryTags SaveTag(string title, bool isImageTag, int tagID)
        {
            if (String.IsNullOrEmpty(title))
                return null;

            return this.GalleryTagRepository.SaveTag(title, FriendlyUrl.CreateFriendlyUrl(title).Replace("/",""), isImageTag, tagID);
        }

        public bool DeleteTag(int tagID)
        {
            var galleryTag = this.GalleryTagRepository.GetByID(tagID);
            if (galleryTag == null)
                return false;

            return this.GalleryTagRepository.DeleteTag(tagID);
        }

        public bool SaveTagsForGallery(int galleryID, int[] tagIDs)
        {
            return (galleryID == 0) ?
                false :
                this.GalleryRepository.SaveTags(galleryID, tagIDs);
        }

        public bool AddTagToGallery(int galleryID, int tagID)
        {
            if (galleryID == 0 || tagID == 0)
                return false;

            return GalleryRepository.AddTag(galleryID, tagID);
        }

        #endregion

        #region Image

        public bool DeleteImage(int imageID)
        {
            return ImageRepository.DeleteImage(imageID);
        }

        public tbl_Image GetImageByID(int imageID)
        {
            return ImageRepository.GetByID(imageID);
        }

        public tbl_Image SaveImage(int categoryID, string heading, string desc, int galleryID, short height, bool isCMS, int? order, string path, int? sitemapID, string name, short width, int? linkID = null)
        {
            return ImageRepository.SaveImage(categoryID, heading, desc, galleryID, height, isCMS, order, path, sitemapID, name, width, linkID);
        }

        public tbl_Image UpdateImageDescription(int imageID, string heading, string desc, int? linkID = null)
        {
            return ImageRepository.UpdateImageDescription(imageID, heading, desc, linkID);
        }

        public tbl_Image UpdateImageTags(int imageID, int[] tagsIDs)
        {
            return ImageRepository.UpdateImageTags(imageID, tagsIDs, true);
        }

        public tbl_Image AddImageTags(int imageID, int[] tagsIDs)
        {
            return ImageRepository.UpdateImageTags(imageID, tagsIDs, false);
        }

        #endregion
    }
}

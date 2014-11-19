using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.Services
{
    public interface IGallery
    {
        bool DeleteGallery(int galleryID);
        List<tbl_Gallery> GetAll();
        List<tbl_Gallery> GetAllPublic();
        List<tbl_Gallery> GetCustomerGalleries(int customerID);
        tbl_Gallery GetByID(int galleryID);
        tbl_Gallery SaveGallery(string title, bool live, int customerID, int galleryCategoryID, int galleryID);
        bool SaveGalleryOrder(int[] orderedGalleryIDs);
        tbl_Gallery SaveGalleryVisibility(int galleryID);

        bool DeleteGalleryImage(int galleryImageID);
        List<tbl_GalleryImage> GetAllGalleryImages();
        List<tbl_GalleryImage> GetAllGalleryImages(int galleryID);
        tbl_GalleryImage GetGalleryImageByID(int galleryImageID);
        tbl_GalleryImage SaveGalleryImage(int galleryID, int imageID, int galleryImageID);
        bool SaveGalleryImageOrder(int[] orderedGalleryImageIDs);

        bool DeleteGalleryCategory(int categoryID);
        List<tbl_GalleryCategory> GetAllGalleryCategoriesOrdered();
        SelectList GetAllGalleryCategoriesAsSelectList(int selectedgalleryCategoryID);
        tbl_GalleryCategory GetGalleryCategory(int categoryID);
        tbl_GalleryCategory SaveGalleryCategory(string catTitle, int categoryID);

        List<tbl_GalleryTags> GetAllTags();
        List<tbl_GalleryTags> GetAllGalleryTags();
        List<tbl_GalleryTags> GetAllImagesTags();
        List<SelectListItem> GetAllTagsByGalleryID(int galleryID);
        List<SelectListItem> GetAllTagsByImageID(int imageID);
        tbl_GalleryTags SaveTag(string title, bool isImageTag, int tagID);
        bool DeleteTag(int tagID);
        bool SaveTagsForGallery(int galleryID, int[] tagIDs);
        bool AddTagToGallery(int galleryID, int tagID);

        bool DeleteImage(int imageID);
        tbl_Image GetImageByID(int imageID);
        tbl_Image SaveImage(int categoryID, string heading, string desc, int galleryID, short height, bool isCMS, int? order, string path, int? sitemapID, string name, short width, int? linkID = null);
        tbl_Image UpdateImageDescription(int imageID, string heading, string desc, int? linkID = null);
        tbl_Image UpdateImageTags(int imageID, int[] tagsIDs);
        tbl_Image AddImageTags(int imageID, int[] tagsIDs);
    }
}

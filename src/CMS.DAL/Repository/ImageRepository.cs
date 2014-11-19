using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using CMS.Utils;
using System.IO;

namespace CMS.DAL.Repository
{
    public interface IImageRepository
    {
        bool DeleteImage(int imageID);
        tbl_Image GetByID(int imageID);
        tbl_Image SaveImage(int categoryID, string heading, string desc, int galleryID, short height, bool isCMS, int? order, string path, int? siteMapID, string name, short width, int? linkID);
        tbl_Image UpdateImageDescription(int imageID, string heading, string desc, int? linkID);
        tbl_Image UpdateImageTags(int imageID, int[] tagsIDs, bool deletePrevious);
    }

    public class ImageRepository : Repository<tbl_Image>, IImageRepository
    {
        public ImageRepository(IDALContext context) : base(context) { }

        public bool DeleteImage(int imageID)
        {
            tbl_Image image = this.DbSet.FirstOrDefault(i => i.ImageID == imageID);
            if (image == null)
                return false;

            image.tbl_GalleryTags.Clear();

            this.Delete(image);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_Image GetByID(int imageID)
        {
            return this.DbSet.FirstOrDefault(i => i.ImageID == imageID);
        }

        public tbl_Image SaveImage(int categoryID, string heading, string desc, int galleryID, short height, bool isCMS, int? order, string path, int? sitemapID, string name, short width, int? linkID = null)
        {
            var image = new tbl_Image()
            {
                I_CatID = categoryID,
                I_InCMS = isCMS,
                I_SitemapID = sitemapID,
                I_Gallery = galleryID,
                I_Description = desc,
                I_Heading = heading,
                I_Height = height,
                I_Order = order,
                I_Path = path,
                I_Width = width,
                I_LinkID = linkID
            };
            this.Create(image);
            this.Context.SaveChanges();

            //image.I_Thumb = String.Format("{0}{1}.{2}", FriendlyUrl.CreateFriendlyUrl(name.Split('.')[0]), image.ImageID, name.Split('.')[1]);
            image.I_Thumb = String.Format("{0}{1}.{2}", FriendlyUrl.CreateFriendlyUrl(Path.GetFileNameWithoutExtension(name)), image.ImageID, Path.GetExtension(name).Replace(".",""));

            this.Context.SaveChanges();
            return image;
        }

        public tbl_Image UpdateImageDescription(int imageID, string heading, string desc, int? linkID = null)
        {
            var image = this.DbSet.FirstOrDefault(i => i.ImageID == imageID);
            if (image == null)
                return null;

            image.I_Description = desc;
            image.I_Heading = heading;
            image.I_LinkID = linkID;

            this.Context.SaveChanges();
            return image;
        }

        public tbl_Image UpdateImageTags(int imageID, int[] tagsIDs, bool deletePrevious)
        {
            var image = this.DbSet.FirstOrDefault(i => i.ImageID == imageID);
            if (image == null)
                return null;

            if (deletePrevious)
            {
                image.tbl_GalleryTags.Clear();
                this.Context.SaveChanges();
            }

            foreach (var tagID in tagsIDs)
            {
                var tag = Context.Set<tbl_GalleryTags>().FirstOrDefault(gt => gt.GalleryTagID == tagID);
                if (tag != null)
                    image.tbl_GalleryTags.Add(tag);
            }

            this.Context.SaveChanges();
            return image;
        }
    }
}

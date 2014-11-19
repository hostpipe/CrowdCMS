using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;
using System.IO;
using CMS.Utils;

namespace CMS.DAL.Repository
{
    public interface IProductImagesRepository
    {
        bool DeleteImage(int imageID);
        tbl_ProductImages GetByID(int imageID);
        tbl_ProductImages SaveImage(int productID, string decsription, string name, bool primary, string view);
    }

    public class ProductImagesRepository : Repository<tbl_ProductImages>, IProductImagesRepository
    {
        public ProductImagesRepository(IDALContext context) : base(context) { }

        public bool DeleteImage(int imageID)
        {
            tbl_ProductImages image = this.DbSet.FirstOrDefault(i => i.ImageID == imageID);
            if (image == null)
                return false;

            foreach (var link in image.tbl_ProductImageLink.ToList())
            {
                this.Context.Set<tbl_ProductImageLink>().Remove(link);
            }

            this.Delete(image);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_ProductImages GetByID(int imageID)
        {
            return this.DbSet.FirstOrDefault(i => i.ImageID == imageID);
        }

        public tbl_ProductImages SaveImage(int productID, string description, string name, bool primary, string view)
        {
            var image = new tbl_ProductImages()
            {
                I_Description = description,
                I_Primary = primary,
                I_View = view
            };
            this.Create(image);
            this.Context.SaveChanges();

            image.I_Name = String.Format("{0}_{1}{2}", FriendlyUrl.CreateFriendlyUrl(Path.GetFileNameWithoutExtension(name)), image.ImageID, Path.GetExtension(name));

            var product = this.Context.Set<tbl_Products>().FirstOrDefault(p => p.ProductID == productID);
            if (product != null)
                image.tbl_ProductImageLink.Add(new tbl_ProductImageLink { PI_ProductID = product.ProductID, PI_ImageID = image.ImageID });

            this.Context.SaveChanges();
            return image;
        }
    }
}

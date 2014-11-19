using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPOIFilesRepository
    {
        tbl_POIFiles GetByID(int fileID);
        tbl_POIFiles SaveFile(int poiID, byte[] content, string extension, string fileName, int fileID);
        bool DeleteFile(int fileID);
    }

    public class POIFilesRepository : Repository<tbl_POIFiles>, IPOIFilesRepository
    {
        public POIFilesRepository(IDALContext context) : base(context) { }

        public tbl_POIFiles GetByID(int fileID)
        {
            return this.DbSet.FirstOrDefault(f => f.POIFileID == fileID);
        }

        public tbl_POIFiles SaveFile(int poiID, byte[] content, string extension, string fileName, int fileID)
        {
            var poi = this.Context.Set<tbl_POI>().FirstOrDefault(p => p.POIID == poiID);
            if (poi == null)
                return null;

            var file = this.DbSet.FirstOrDefault(f => f.POIFileID == fileID);
            if (file == null)
            {
                file = new tbl_POIFiles();
                this.Create(file);
            };

            file.POIF_Content = content;
            file.POIF_Extension = extension;
            file.POIF_Name = fileName;
            file.POIF_POIID = poiID;

            this.Context.SaveChanges();
            return file;
        }

        public bool DeleteFile(int fileID)
        {
            var file = this.DbSet.FirstOrDefault(f => f.POIFileID == fileID);
            if (file == null)
                return false;

            this.DbSet.Remove(file);
            this.Context.SaveChanges();
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;
using System.Web.Mvc;
using System.IO;

namespace CMS.Services.Services
{
    public class POI : ServiceBase, IPOI
    {
        private IPOIRepository POIRepository { get; set; }
        private IPOICategoriesRepository POICategoriesRepository { get; set; }
        private IPOITagsGroupsRepository POITagsGroupsRepository { get; set; }
        private IPOITagsRepository POITagsRepoistory { get; set; }
        private IPOIFilesRepository POIFilesRepository { get; set; }

        private IAddressRepository AddressRepository { get; set; }

        public POI()
            : base()
        {
            this.POIRepository = new POIRepository(this.Context);
            this.POICategoriesRepository = new POICategoriesRepository(this.Context);
            this.POITagsGroupsRepository = new POITagsGroupsRepository(this.Context);
            this.POITagsRepoistory = new POITagsRepository(this.Context);
            this.POIFilesRepository = new POIFilesRepository(this.Context);

            this.AddressRepository = new AddressRepository(this.Context);
        }

        #region POI

        public bool DeletePOI(int poiID)
        {
            return POIRepository.Delete(poiID);
        }

        public List<tbl_POI> GetAllPOIs()
        {
            return POIRepository.GetAll().ToList();
        }

        public tbl_POI GetPOIByID(int poiID)
        {
            return POIRepository.GetByID(poiID);
        }

        public tbl_POI SavePOI(string title, string description, int categoryID, string latitude, string longitude, string phone, int addressID, 
            string address1, string address2, string address3, string town, string postcode, string county, string country, int[] tagsIDs, int? sitemapID, int poiID)
        {
            if (String.IsNullOrEmpty(title) || categoryID < 1 || String.IsNullOrEmpty(latitude) || String.IsNullOrEmpty(longitude) || tagsIDs == null ||
                String.IsNullOrEmpty(address1) || String.IsNullOrEmpty(town) || String.IsNullOrEmpty(postcode) || String.IsNullOrEmpty(country) )
                return null;

            var address = AddressRepository.SaveAddress((int?)null, country, (int?)null, county, String.Empty, String.Empty, String.Empty, address1, address2, address3, postcode, String.Empty, town, addressID);
            if (address == null)
                return null;

            var poi = POIRepository.Save(title, description, categoryID, address.AddressID, latitude.Replace(',', '.'), longitude.Replace(',', '.'), phone, sitemapID, poiID);
            if (poi == null)
                return null;

            poi = POIRepository.SaveTags(tagsIDs, poi.POIID);
            return poi;
        }

        public tbl_POI SaveTagsForPOI(int[] tagsIDs, int poiID)
        {
            if (poiID < 1 || tagsIDs == null)
                return null;

            return POIRepository.SaveTags(tagsIDs, poiID);
        }

        public List<tbl_POI> SearchPOIs(string search, int? categoryID, int[] tagIDs)
        {
            if (!String.IsNullOrEmpty(search))
                search = search.ToLower();
            var pois = POIRepository.GetAll().ToList().Where(p =>
                (String.IsNullOrEmpty(search) || (p.POI_Title ?? String.Empty).ToLower().Contains(search) || (p.tbl_Address.A_County ?? String.Empty).ToLower().Contains(search) || (p.tbl_Address.A_Town ?? String.Empty).ToLower().Contains(search)) &&
                (!categoryID.HasValue || categoryID.Value == 0 || p.POI_CategoryID == categoryID.Value) &&
                (tagIDs == null || tagIDs.Count() == 0 || p.tbl_POITags.Any(t => tagIDs.Contains(t.POITagID)))
            ).ToList();

            return pois;
        }

        #endregion

        #region POI Categories

        public bool DeletePOICategory(int poiCategoryID)
        {
            return POICategoriesRepository.Delete(poiCategoryID);
        }

        public List<tbl_POICategories> GetAllPOICategories()
        {
            return POICategoriesRepository.GetAll().ToList();
        }

        public SelectList GetAllPOICategoriesAsSelectList()
        {
            var categories = POICategoriesRepository.GetAll().ToList();
            categories.Insert(0, new tbl_POICategories { POIC_Title = "Select category", POICategoryID = 0 });
            return new SelectList(categories, "POICategoryID", "POIC_Title");
        }

        public List<tbl_POICategories> GetAllPOICategoriesLive()
        {
            return POICategoriesRepository.GetAll().Where(c => c.POIC_IsLive).ToList();
        }

        public SelectList GetAllPOICategoriesLiveAsSelectList()
        {
            var categories = POICategoriesRepository.GetAll().Where(c => c.POIC_IsLive).ToList();
            categories.Insert(0, new tbl_POICategories { POIC_Title = "Select category", POICategoryID = 0 });
            return new SelectList(categories, "POICategoryID", "POIC_Title");
        }

        public tbl_POICategories GetPOICategoryByID(int poiCategoryID)
        {
            return POICategoriesRepository.GetByID(poiCategoryID);
        }

        public tbl_POICategories GetPOICategoryByTitle(string title)
        {
            return POICategoriesRepository.GetByTitle(title);
        }

        public tbl_POICategories SavePOICategory(string title, bool isLive, int poiCategoryID)
        {
            if (String.IsNullOrEmpty(title))
                return null;
                    
            return POICategoriesRepository.Save(title, isLive, poiCategoryID);
        }

        #endregion

        #region POI Tags Groups

        public bool DeletePOITagGroup(int poiTagsGroupID)
        {
            return POITagsGroupsRepository.Delete(poiTagsGroupID);
        }

        public List<tbl_POITagsGroups> GetAllPOITagsGroups()
        {
            return POITagsGroupsRepository.GetAll().ToList();
        }

        public SelectList GetAllPOITagsGroupsAsSelectList()
        {
            var groups = POITagsGroupsRepository.GetAll().ToList();
            groups.Insert(0, new tbl_POITagsGroups { POITagsGroupID = 0, POITG_Title = "Select group" });
            return new SelectList(groups, "POITagsGroupID", "POITG_Title");
        }

        public tbl_POITagsGroups GetPOITagsGroupByID(int poiTagsGroupID)
        {
            return POITagsGroupsRepository.GetByID(poiTagsGroupID);
        }

        public tbl_POITagsGroups SavePOITagsGroup(string title, int poiTagsGroupID)
        {
            if (String.IsNullOrEmpty(title))
                return null;

            return POITagsGroupsRepository.Save(title, poiTagsGroupID);
        }

        #endregion

        #region POI Tags

        public bool DeletePOITag(int poiTagID)
        {
            return POITagsRepoistory.Delete(poiTagID);
        }

        public List<tbl_POITags> GetAllPOITags()
        {
            return POITagsRepoistory.GetAll().ToList();
        }

        public List<SelectListItem> GetAllPOITags(int poiID)
        {
            return POITagsRepoistory.GetAll().OrderBy(t => t.POIT_GroupID).ToList().Select(t => new SelectListItem {
                Text = t.POIT_Title,
                Value = t.POITagID.ToString(),
                Selected = t.tbl_POI.Any(p => p.POIID == poiID)
            }).ToList();
        }

        public tbl_POITags GetPOITagByID(int poiTagID)
        {
            return POITagsRepoistory.GetByID(poiTagID);
        }

        public tbl_POITags SavePOITag(string title, int poiTagsGroupID, int poiTagID)
        {
            if (String.IsNullOrEmpty(title) || poiTagsGroupID < 1)
                return null;

            return POITagsRepoistory.Save(title, poiTagsGroupID, poiTagID);
        }

        #endregion

        #region POI Files

        public tbl_POIFiles GetPOIFileByID(int fileID)
        {
            return POIFilesRepository.GetByID(fileID);
        }

        public tbl_POIFiles SavePOIFile(int poiID, Stream content, string extension, string fileName, int fileID)
        {
            if (poiID < 1 || content == null || String.IsNullOrEmpty(extension) || string.IsNullOrEmpty(fileName))
                return null;

            MemoryStream ms = new MemoryStream();
            content.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return POIFilesRepository.SaveFile(poiID, ms.ToArray(), extension, fileName, fileID);
        }

        public bool DeletePOIFile(int fileID)
        {
            return POIFilesRepository.DeleteFile(fileID);
        }

        #endregion
    }
}

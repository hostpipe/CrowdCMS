using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using System.Web.Mvc;
using System.IO;

namespace CMS.Services
{
    public interface IPOI
    {
        bool DeletePOI(int poiID);
        List<tbl_POI> GetAllPOIs();
        tbl_POI GetPOIByID(int poiID);
        tbl_POI SavePOI(string title, string description, int categoryID, string latitude, string longitude, string phone, int addressID,
            string address1, string address2, string address3, string town, string postcode, string county, string country, int[] tagsIDs, int? sitemapID, int poiID);
        tbl_POI SaveTagsForPOI(int[] tagsIDs, int poiID);
        List<tbl_POI> SearchPOIs(string search, int? categoryID, int[] tagIDs);

        bool DeletePOICategory(int poiCategoryID);
        List<tbl_POICategories> GetAllPOICategories();
        SelectList GetAllPOICategoriesAsSelectList();
        List<tbl_POICategories> GetAllPOICategoriesLive();
        SelectList GetAllPOICategoriesLiveAsSelectList();
        tbl_POICategories GetPOICategoryByID(int poiCategoryID);
        tbl_POICategories GetPOICategoryByTitle(string title);
        tbl_POICategories SavePOICategory(string title, bool isLive, int poiCategoryID);

        bool DeletePOITagGroup(int poiTagsGroupID);
        List<tbl_POITagsGroups> GetAllPOITagsGroups();
        SelectList GetAllPOITagsGroupsAsSelectList();
        tbl_POITagsGroups GetPOITagsGroupByID(int poiTagsGroupID);
        tbl_POITagsGroups SavePOITagsGroup(string title, int poiTagsGroupID);

        bool DeletePOITag(int poiTagID);
        List<tbl_POITags> GetAllPOITags();
        List<SelectListItem> GetAllPOITags(int poiID);
        tbl_POITags GetPOITagByID(int poiTagID);
        tbl_POITags SavePOITag(string title, int poiTagsGroupID, int poiTagID);

        tbl_POIFiles GetPOIFileByID(int fileID);
        tbl_POIFiles SavePOIFile(int poiID, Stream content, string extension, string fileName, int fileID);
        bool DeletePOIFile(int fileID);
    }
}

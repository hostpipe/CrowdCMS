using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.BL
{
    public class Permission
    {
        public static readonly string AddContent = "Add Content";
        public static readonly string ApproveContent = "Approve Content";
        public static readonly string DeleteContent = "Delete Content";
        public static readonly string EditContent = "Edit Content";

        public static readonly string AddNews = "Add News";
        public static readonly string DeleteNews = "Delete News";
        public static readonly string EditNews = "Edit News";

        public static readonly string AddUser = "Add Users";
        public static readonly string DeleteUser = "Delete Users";
        public static readonly string EditUser = "Edit Users";
    }

    public class LeftMenuUrl
    {
        public const string AdminMenu = "/Admn/AdminMenu";
        public const string ContactQuestion = "/Admn/ContactQuestion";
        public const string Countries = "/Admn/Countries";
        public const string Forms = "/Admn/Forms";
        public const string FormSubmission = "/Admn/FormSubmission";
        public const string Discount = "/Admn/Discount";
        public const string Domains = "/Admn/Domains";
        public const string Donations = "/Admn/Donations";
        public const string DonationInfo = "/Admn/DonationInfo";
        public const string News = "/Admn/News";
        public const string Postage = "/Admn/Postage";
        public const string ProdAttributes = "/Admn/ProdAttributes";
        public const string ProdCategories = "/Admn/ProdCategories";
        public const string Products = "/Admn/Products";
        public const string Sections = "/Admn/Sections";
        public const string Testimonials = "/Admn/Testimonials";
        public const string Users = "/Admn/Users";
        public const string UserGroups = "/Admn/UserGroups";
        public const string Tax = "/Admn/TaxRates";
        public const string EventTypes = "/Admn/EventTypes";
        public const string EventCategories = "/Admn/EventCategories";
        public const string POICategories = "/Admn/POICategories";
        public const string POITags = "/Admn/POITags";
        public const string POITagsGroups = "/Admn/POITagsGroups";
        public const string POIs = "/Admn/POIs";
        public const string Templates = "/Admn/Templates";
        public const string Portfolio = "/Admn/Portfolio";
        public const string Gallery = "/Admn/Gallery";
    }

    public class ContactItemTypeName
    {
        public const string Textbox = "Textbox";
        public const string Textarea = "Textarea";
        public const string Subscribe = "Subscribe checkbox";
        public const string Datebox = "Datebox";
        public const string Divider = "Divider";
    }
}

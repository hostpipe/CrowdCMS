using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.UI.Models;
using CMS.BL.Entity;
using CMS.Utils;
using CMS.BL;
using CMS.Services.Extensions;
using System.Globalization;
using System.Data.Objects.DataClasses;


namespace CMS.UI
{
    public class LeftMenuModelMapper
    {
        public static List<CMSMenuModel> MapDomains(List<tbl_Domains> domains)
        {
            return new List<CMSMenuModel> { 
                new CMSMenuModel
                {
                    Title = "Domains",
                    MenuItems = domains.Select(domain => new CMSMenuItem
                        {
                            MenuItemID = domain.DomainID,
                            Title = domain.DO_Domain,

                            IsManage = true,

                            EditText = "Edit " + domain.DO_Domain,
                            DeleteText = "Delete " + domain.DO_Domain,
                            IsPaymentLogosConf = true
                        }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapCountries(List<tbl_Domains> domains)
        {
            return domains.Select(domain => new CMSMenuModel
                {
                    Title = "Countries for " + domain.DO_CompanyName,
                    MenuItems = domain.tbl_Country.OrderBy(m => m.C_Order).Select(item =>
                    {
                        var menuItem = new CMSMenuItem
                        {
                            MenuItemID = item.CountryID,
                            IsMove = true,
                        };
                        if (item.C_IsDefault)
                            menuItem.BoldedTitle = item.C_Country;
                        else
                            menuItem.Title = item.C_Country;
                        return menuItem;
                    }).ToList()
                }).ToList();
        }

        public static List<CMSMenuModel> MapUsers(List<tbl_AdminUsers> users, CustomPrincipal currentUser)
        {
            return new List<CMSMenuModel> 
            { 
                new CMSMenuModel
                {
                    Title = "Users",
                    MenuItems = users.Select(user => new CMSMenuItem
                        {
                            MenuItemID = user.AdminUserID,
                            Title = String.Format("<b>{0}: {1}</b> ({2})", user.US_UserName, user.US_Email, user.tbl_UserGroups.UG_Type),
                            IsDelete = currentUser.HasPermission(Permission.DeleteUser),
                            IsEdit = currentUser.HasPermission(Permission.EditUser)
                        }).ToList()
                    }
            };
        }

        public static List<CMSMenuModel> MapUserGroups(List<tbl_UserGroups> groups)
        {
            return new List<CMSMenuModel>
            {
                new CMSMenuModel
                {
                    Title = "User Groups",
                    MenuItems = groups.Select(group => new CMSMenuItem
                        {
                            MenuItemID = group.UserGroupID,
                            Title = group.UG_Type
                        }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapAdminMenu(List<tbl_AdminMenu> menuItems)
        {
            return new List<CMSMenuModel>
            { 
                new CMSMenuModel 
                { 
                    Title = "Modules", 
                    MenuItems = CreateAdminSubMenu(menuItems, 0)
                } 
            };
        }

        private static List<CMSMenuItem> CreateAdminSubMenu(List<tbl_AdminMenu> menuItems, int parentID)
        {
            return menuItems.OrderBy(mi => mi.AM_Order).Where(mi => mi.AM_ParentID == parentID)
                .Select(menuItem => new CMSMenuItem
                {
                    MenuItemID = menuItem.AdminMenuID,
                    SubMenuItems = CreateAdminSubMenu(menuItems, menuItem.AdminMenuID),
                    Title = menuItem.AM_MenuText,
                    Visible = menuItem.AM_Live,

                    IsMove = true,
                    IsVisibility = true,
                    VisibilityText = "Turn menu item on/off"
                }).ToList();
        }

        public static List<CMSMenuModel> MapSections(List<tbl_Domains> domains, CustomPrincipal currentUser)
        {
            return domains.Select(domain =>
            {
                var sections = domain.tbl_SiteMap.Where(sm => !sm.SM_Deleted &&
                    (sm.IsType(ContentType.Content) || ((sm.IsType(ContentType.Category) || sm.IsType(ContentType.Gallery)) && sm.IsDirectlyInMenu)));
                return new CMSMenuModel
                {
                    Title = domain.DO_CompanyName,
                    MenuItems = CreateSectionsSubMenu(sections, 0, currentUser)
                };
            }).ToList();
        }

        private static List<CMSMenuItem> CreateSectionsSubMenu(IEnumerable<tbl_SiteMap> sections, int parentID, CustomPrincipal currentUser)
        {
            return sections.Where(sm => parentID == 0 ?
                    (sm.SM_ParentID == 0 || sm.IsDirectlyInMenu) :
                    (sm.SM_ParentID == parentID && sm.IsUnderParentInMenu))
                .OrderBy(sm => sm.SM_OrderBy).Select(section =>
                 {
                     var item = new CMSMenuItem
                     {
                         MenuItemID = section.SiteMapID,
                         SubMenuItems = CreateSectionsSubMenu(sections, section.SiteMapID, currentUser),
                         CssClasses = section.SM_IsPredefined ? "predefinedSection" : String.Empty,

                         IsDelete = !section.tbl_ContentType.CTP_Value.Equals(ContentType.Category.ToString()),
                         IsEdit = !section.tbl_ContentType.CTP_Value.Equals(ContentType.Category.ToString()),
                         IsMove = currentUser.HasPermission(Permission.EditContent),
                         ApproveText = String.Format("{0} Content Requires Approval", section.SM_Name),
                         DeleteText = String.Format("Delete {0} ({1})", section.SM_Name, section.SiteMapID),
                         EditText = String.Format("Edit {0} ({1})", section.SM_Name, section.SiteMapID),
                         EditImagesText = String.Format("Images for {0} ({1})", section.SM_Name, section.SiteMapID),
                         ExpandText = String.Format("Show Sub Pages of {0}", section.SM_Name),

                         PreviewText = String.Format("Preview {0}", section.SM_Name),
                         PreviewUrl = String.Format("/Admn/Preview?sectionID={0}", section.SiteMapID),

                         Title = section.SM_Name,
                     };
                     if (section.IsType(ContentType.Category))
                     {
                         item.Title += " (Product Category)";
                     }
                     else if (section.IsType(ContentType.Gallery) && section.tbl_Gallery.G_CustomerID==0)
                     {
                         item.Title += " (Gallery)";
                     }
                     else
                     {
                         item.IsPreview = true;
                         item.IsApprove = section.tbl_Content.Where(c => !c.C_Deleted).All(c => !c.C_Approved) &&
                             currentUser.HasPermission(Permission.ApproveContent);
                         item.IsEdit = currentUser.HasPermission(Permission.EditContent);
                         item.IsExpand = sections.Any(c => c.SM_ParentID == section.SiteMapID);
                         item.IsEditImages = currentUser.HasPermission(Permission.EditContent);
                         if (!section.SM_IsPredefined)
                         {
                             item.IsDelete = currentUser.HasPermission(Permission.DeleteContent);
                         }
                     }
                     return item;
                 }).ToList();
        }

        public static List<CMSMenuModel> MapNews(List<tbl_Domains> domains, CustomPrincipal currentUser)
        {
            return domains.Select(domain => new CMSMenuModel
                {
                    Title = "Blog Articles for " + domain.DO_CompanyName,
                    MenuItems = domain.tbl_SiteMap
                    .Where(sm => !sm.SM_Deleted && sm.IsType(ContentType.Blog)).OrderByDescending(b => b.SM_Live).ThenByDescending(b => b.SM_Date)
                    .Select(b => new CMSMenuItem
                        {
                            MenuItemID = b.SiteMapID,
                            Date = b.SM_Date,
                            IsFuturePublish = b.SM_PublishDate == null ? false : (b.SM_PublishDate >= DateTime.Now),
                            PublishDateText = b.SM_PublishDate != null ? ("Publish date: " + b.SM_PublishDate.ToString()) : String.Empty,
                            Title = b.SM_Name,
                            IsDelete = currentUser.HasPermission(Permission.DeleteNews),
                            DeleteText = String.Format("Delete {0} ({1})", b.SM_Name, b.SiteMapID),
                            IsEdit = currentUser.HasPermission(Permission.EditNews),
                            EditText = String.Format("Edit \"{0}\"", b.SM_Name),
                            IsEditImages = currentUser.HasPermission(Permission.EditNews),
                            EditImagesText = String.Format("Edit images for \"{0}\"", b.SM_Name),
                            IsComment = true,
                            CommentsText = String.Format("View comments for \"{0}\"", b.SM_Name),
                            UnauthorizedCommentExists = b.tbl_Comments.Any(c => !c.CO_Authorised),
                            AuthorizedCommentExists = b.tbl_Comments.Any(c => c.CO_Authorised),
                            IsVisibility = true,
                            VisibilityText = "Turn news article on / off",
                            Visible = b.SM_Live
                        }).ToList()
                }).ToList();
        }

        public static List<CMSMenuModel> MapTestimonials(List<tbl_Testimonials> testimonials)
        {
            return new List<CMSMenuModel>
            {
                new CMSMenuModel
                {
                    Title = "Testimonials",
                    MenuItems = testimonials.Select(t => new CMSMenuItem
                    {
                        MenuItemID = t.TestimonialID,
                        Date = t.TE_Date,
                        Title = String.Format("{1} ({0})", t.TE_Company, t.TE_Client),
                        IsDelete = true,
                        DeleteText = String.Format("Delete {1} ({0})", t.TE_Company, t.TE_Client),
                        IsEdit = true,
                        EditText = String.Format("Edit: {1} ({0})", t.TE_Company, t.TE_Client),
                        IsVisibility = true,
                        VisibilityText = "Turn testimonial on / off",
                        Visible = t.TE_Live
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapForms(List<tbl_Domains> domains, List<tbl_Form> forms, CustomPrincipal currentUser)
        {
            return domains.Select(domain => new CMSMenuModel
                {
                    Title = "Forms for " + domain.DO_CompanyName,
                    MenuItems = forms.Where(f => f.F_DomainID == domain.DomainID).Select(f => new CMSMenuItem
                    {
                        MenuItemID = f.FormID,
                        Title = f.F_Name,
                        IsEdit = false,
                        IsDelete = false,
                        IsComment = true,
                        CommentsText = String.Format("View form submissions for \"{0}\"", f.F_Name),
                        UnauthorizedCommentExists = f.tbl_FormSubmission.Any(fs => !fs.FS_Read),
                        AuthorizedCommentExists = f.tbl_FormSubmission.Any(fs => fs.FS_Read)
                    }).ToList()
                }).ToList();
        }

        public static List<CMSMenuModel> MapFormItems(List<tbl_Domains> domains, List<tbl_Form> forms, List<tbl_FormItem> formsItems, CustomPrincipal currentUser)
        {
            return domains.Select(domain => new CMSMenuModel
                {
                    Title = "Forms for " + domain.DO_CompanyName,
                    MenuItems = forms.Where(f => f.F_DomainID == domain.DomainID).OrderBy(f => f.F_Name)
                    .Select(form => new CMSMenuItem
                    {
                        MenuItemID = form.FormID,
                        Title = form.F_Name,
                        IsDelete = true,
                        IsEdit = true,
                        SubMenuItems = CreateFormItemsSubMenu(formsItems.Where(fi => fi.FI_FormID == form.FormID).ToList(), currentUser)
                    }).ToList()
                }).ToList();
        }

        private static List<CMSMenuItem> CreateFormItemsSubMenu(List<tbl_FormItem> formsItems, CustomPrincipal currentUser)
        {
            return formsItems.OrderBy(fi => fi.FI_Order).Select(item => new CMSMenuItem
                {
                    MenuItemID = item.FormItemID,
                    Title = item.tbl_FormItemType.FIT_Name == ContactItemTypeName.Divider ? String.Format("- {0} -", item.FI_Name) : item.FI_Name,
                    IsDelete = true,
                    DeleteText = String.Format("Delete {0}", item.FI_Name),
                    IsEdit = true,
                    EditText = String.Format("Edit: {0}", item.FI_Name),
                    IsVisibility = true,
                    VisibilityText = "Turn item on / off",
                    Visible = item.FI_Live,
                    IsMove = true
                }).ToList();
        }

        public static List<CMSMenuModel> MapEventCategories(List<tbl_Domains> domains, CustomPrincipal currentUser)
        {
            return domains.Select(domain =>
            {
                var categories = domain.tbl_SiteMap.Where(sm => !sm.SM_Deleted && sm.IsType(ContentType.Category) &&
                        sm.tbl_ProdCategories.PC_ProductTypeID == (int)ProductType.Event
                    ).OrderBy(c => c.tbl_ProdCategories.PC_Order).ToList();
                return new CMSMenuModel
                    {
                        Title = "Event Categories for " + domain.DO_CompanyName,
                        MenuItems = CreateProdSubCategories(categories, 0, currentUser)
                    };
            }).ToList();
        }

        public static List<CMSMenuModel> MapProdCategories(List<tbl_Domains> domains, CustomPrincipal currentUser)
        {
            return domains.Select(domain =>
            {
                var categories = domain.tbl_SiteMap.Where(sm => !sm.SM_Deleted && sm.IsType(ContentType.Category) &&
                    sm.tbl_ProdCategories.PC_ProductTypeID == (int)ProductType.Item).OrderBy(c => c.tbl_ProdCategories.PC_Order).ToList();

                return new CMSMenuModel
                    {
                        Title = "Product Categories for " + domain.DO_CompanyName,
                        MenuItems = CreateProdSubCategories(categories, 0, currentUser)
                    };
            }).ToList();
        }

        private static List<CMSMenuItem> CreateProdSubCategories(List<tbl_SiteMap> categories, int parentID, CustomPrincipal currentUser)
        {
            return categories.Where(c => c.tbl_ProdCategories.PC_ParentID.GetValueOrDefault(0) == parentID)
                .Select(category => new CMSMenuItem
                {
                    MenuItemID = category.tbl_ProdCategories.CategoryID,
                    Title = category.tbl_ProdCategories.PC_Title,
                    SubMenuItems = CreateProdSubCategories(categories, category.SiteMapID, currentUser),
                    DeleteText = String.Format("Delete {0} ({1})", category.tbl_ProdCategories.PC_Title, category.tbl_ProdCategories.CategoryID),
                    EditText = String.Format("Edit {0}", category.tbl_ProdCategories.PC_Title),
                    IsEditImages = true,
                    EditImagesText = "Image",
                    IsMove = true,
                    IsApprove = category.tbl_Content.Where(c => !c.C_Deleted).All(c => !c.C_Approved) && currentUser.HasPermission(Permission.ApproveContent),
                    ApproveText = String.Format("{0} Category Requires Approval", category.tbl_ProdCategories.PC_Title),
                }).ToList();
        }

        public static List<CMSMenuModel> MapProdAttributes(List<tbl_ProdAttributes> attributes)
        {
            return new List<CMSMenuModel> 
            {
                new CMSMenuModel
                {
                    Title = "Product Attributes",
                    MenuItems = attributes.Select(attr =>    new CMSMenuItem
                    {
                        MenuItemID = attr.AttributeID,
                        Title = attr.A_Title,
                        DeleteText = String.Format("Delete {0} ({1})", attr.A_Title, attr.AttributeID),
                        EditText = String.Format("Edit {0}", attr.A_Title)
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapDiscounts(List<tbl_Discount> discounts)
        {
            return discounts.GroupBy(c => c.tbl_Domains).Select(group => new CMSMenuModel
                {
                    Title = "Discounts for " + group.Key.DO_CompanyName,
                    MenuItems = group.Select(discount => new CMSMenuItem
                    {
                        MenuItemID = discount.DiscountID,
                        Title = String.Format("{0} ({1})", discount.D_Title, discount.D_Code),
                        EditText = "Edit " + discount.D_Code,
                        DeleteText = "Delete " + discount.D_Code
                    }).ToList()
                }).ToList();
        }

        public static List<CMSMenuModel> MapProducts(List<tbl_Products> products, CustomPrincipal currentUser)
        {
            var categories = products.Select(p => p.tbl_ProdCategories).Distinct();
            return new List<CMSMenuModel> 
            {
                new CMSMenuModel
                {
                    Title = products.Select(p => p.tbl_ProductTypes.PT_Name).FirstOrDefault() == ProductType.Event.ToString() ? "Events" : "Products",
                    MenuItems = categories.Select(category =>   new CMSMenuItem
                    {
                        MenuItemID = category.CategoryID,
                        BoldedTitle = category.PC_Title,
                        SubMenuItems = MapProductsForCategory(products.Where(p => p.P_CategoryID == category.CategoryID).ToList(), currentUser),
                        IsDelete = false,
                        IsEdit = false
                    }).ToList()
                }
            };
        }

        private static List<CMSMenuItem> MapProductsForCategory(List<tbl_Products> products, CustomPrincipal currentUser)
        {
            return products.OrderBy(p => p.P_Order).Select(product => new CMSMenuItem
            {
                MenuItemID = product.ProductID,
                Title = String.Format("{0} ({1})", product.P_Title, product.P_ProductCode),
                DeleteText = String.Format("Delete {0} ({1})", product.P_Title, product.ProductID),
                EditText = String.Format("Edit {0}", product.P_Title),
                IsEditImages = true,
                EditImagesText = "Images",
                IsAssociation = true,
                IsMove = true,
                IsApprove = product.tbl_SiteMap.tbl_Content.Where(c => !c.C_Deleted).All(c => !c.C_Approved) &&
                        currentUser.HasPermission(Permission.ApproveContent),
                ApproveText = "Content Requires Approval",
                IsStock = true,
                StockText = "Stock"
            }).ToList();
        }

        public static List<CMSMenuModel> MapCustomers(List<tbl_Customer> customers)
        {
            return customers.GroupBy(c => c.tbl_Domains).Select(group => new CMSMenuModel
            {
                Title = "Customers for " + group.Key.DO_CompanyName,
                MenuItems = group.Select(customer => new CMSMenuItem
                {
                    MenuItemID = customer.CustomerID,
                    BoldedTitle = customer.CU_FirstName + ' ' + customer.CU_Surname,
                    Title = customer.CU_Email,

                    IsDelete = false,
                    EditText = "View"
                }).ToList()
            }).ToList();
        }

        public static List<CMSMenuModel> MapOrders(List<tbl_Orders> orders)
        {
            return orders.GroupBy(o => o.tbl_Domains).Select(group =>
                new CMSMenuModel
                {
                    Title = "Orders for " + group.Key.DO_CompanyName,
                    MenuItems = group.Select(order => new CMSMenuItem
                    {
                        MenuItemID = order.OrderID,
                        Title = String.Format("({0}), {1} <br /> {4} {2} {3}", order.GetPriceString(), order.CurrentOrderStatus.ToString(), order.BillingFirstnames, order.BillingSurname,
                        (order.O_ProductTypeID.HasValue ? (order.O_ProductTypeID == (int)ProductType.Donation ? "Donation from" : "Order for") : "Order for")),
                        Date = order.O_Timestamp.GetValueOrDefault().ToLocalTime(),
                        IsDelete = !order.Canceled,
                        DeleteText = "Cancel Order",
                        CssClasses = order.CurrentOrderStatus == OrderStatus.Commited ? "orderStatusCommited" :
                                     order.CurrentOrderStatus == OrderStatus.Canceled ? "orderStatusCanceled" :
                                     order.CurrentOrderStatus == OrderStatus.Despatched ? "orderStatusDispatched" :
                                     order.CurrentOrderStatus == OrderStatus.Paid ? "orderStatusPaid" :
                                     order.CurrentOrderStatus == OrderStatus.PaymentFailed ? "orderStatusPaymentFailed" :
                                     order.CurrentOrderStatus == OrderStatus.Processing ? "orderStatusProcessing" :
                                     order.CurrentOrderStatus == OrderStatus.Refunded ? "orderStatusRefunded" : String.Empty,
                        IsInfo = !String.IsNullOrEmpty(order.O_DeliveryNotes),
                        InfoText = String.IsNullOrEmpty(order.O_DeliveryNotes) ? String.Empty :
                                   order.O_DeliveryNotes.Length <= 30 ? order.O_DeliveryNotes : order.O_DeliveryNotes.Substring(0, 27) + "..."
                    }).ToList()
                }).ToList();
        }

        public static List<CMSMenuModel> MapTaxes(List<tbl_Tax> taxes)
        {
            return new List<CMSMenuModel> 
            {
                new CMSMenuModel
                {
                    Title = "Tax Rates",
                    MenuItems = taxes.Select(tax => new CMSMenuItem
                    {
                        MenuItemID = tax.TaxID,
                        Title = String.Format("{0} ({1}%)", tax.TA_Title, tax.TA_Percentage.GetValueOrDefault(0).ToString("0.###"))
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapPostage(List<tbl_Domains> domains, CustomPrincipal customPrincipal)
        {
            return domains.Select(domain => new CMSMenuModel
            {
                Title = "Postages for " + domain.DO_CompanyName,
                MenuItems = domain.tbl_Postage.Select(item => new CMSMenuItem
                    {
                        MenuItemID = item.PostageID,
                        Title = String.Format("{0} {1:C} ( {2} {3} {4} )", item.PST_Description, item.PST_Amount.GetValueOrDefault(0),
                            item.tbl_PostageBands != null ? String.Format("{0:C}-{1:C}", item.tbl_PostageBands.PB_Lower, item.tbl_PostageBands.PB_Upper) : String.Empty,
                            item.tbl_PostageWeights != null ? String.Format("{0}-{1}", item.tbl_PostageWeights.PW_Lower, item.tbl_PostageWeights.PW_Upper) : String.Empty,
                            item.tbl_PostageZones != null ? item.tbl_PostageZones.PZ_Name : String.Empty)
                    }).ToList()
            }).ToList();

        }

        public static List<CMSMenuModel> MapEventTypes(List<tbl_EventTypes> eventTypes)
        {
            return new List<CMSMenuModel> 
            {
                new CMSMenuModel
                {
                    Title = "Event Types",
                    MenuItems = eventTypes.Select(type => new CMSMenuItem
                    {
                        MenuItemID = type.EventTypeID,
                        Title = type.ET_Title,
                        IsEditImages = true
                    }).ToList()

                }
            };
        }

        public static List<CMSMenuModel> MapDonations(List<tbl_Orders> donations)
        {
            return donations.GroupBy(o => o.tbl_Domains).Select(group =>
                new CMSMenuModel
                {
                    Title = "Donations for " + group.Key.DO_CompanyName,
                    MenuItems = group.Select(donation => new CMSMenuItem
                    {
                        MenuItemID = donation.OrderID,
                        Title = String.Format("({0}), {1} <br /> Ordered for {2} {3}", donation.GetPriceString(), donation.CurrentOrderStatus.ToString(), donation.BillingFirstnames, donation.BillingSurname),
                        Date = donation.O_Timestamp.GetValueOrDefault().ToLocalTime(),
                        IsDelete = !donation.Canceled,
                        DeleteText = "Cancel Donation",
                        CssClasses = donation.CurrentOrderStatus == OrderStatus.Commited ? "orderStatusCommited" :
                                     donation.CurrentOrderStatus == OrderStatus.Canceled ? "orderStatusCanceled" :
                                     donation.CurrentOrderStatus == OrderStatus.Despatched ? "orderStatusDispatched" :
                                     donation.CurrentOrderStatus == OrderStatus.Paid ? "orderStatusPaid" :
                                     donation.CurrentOrderStatus == OrderStatus.PaymentFailed ? "orderStatusPaymentFailed" :
                                     donation.CurrentOrderStatus == OrderStatus.Processing ? "orderStatusProcessing" : String.Empty
                    }).ToList()
                }).ToList();
        }

        public static List<CMSMenuModel> MapDonationInfo(List<tbl_Domains> domains, CustomPrincipal principal)
        {
            var menuModel = new CMSMenuModel { Title = "Donations Info", MenuItems = new List<CMSMenuItem>() };
            var categoriesModels = new List<CMSMenuModel>();

            foreach (var domain in domains)
            {
                var categoryModel = new CMSMenuModel { Title = "Donations Info for " + domain.DO_CompanyName,
                    MenuItems = new List<CMSMenuItem>() };

                foreach (var item in domain.tbl_DonationInfo)
                {
                    categoryModel.MenuItems.Add(new CMSMenuItem()
                    {
                        MenuItemID = item.DonationInfoID,
                        Title = String.Format("{0} {1:C}", item.DI_Title, item.DI_Amount),
                        IsEdit = true,
                        IsDelete = true,
                        EditText = "Edit Donation Info",
                        DeleteText = "Delete Donation Info",
                        IsEditImages = true,
                        EditImagesText = "Manage DonationInfo Image"
                    });
                }
                categoriesModels.Add(categoryModel);
            }

            return categoriesModels;
        }

        public static List<CMSMenuModel> MapPOICategories(List<tbl_POICategories> poiCategories)
        {
            return new List<CMSMenuModel> 
            {
                new CMSMenuModel
                {
                    Title = "POI Categories",
                    MenuItems = poiCategories.Select(category =>  new CMSMenuItem
                    {
                        MenuItemID = category.POICategoryID,
                        Title = category.POIC_Title
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapPOITags(List<tbl_POITagsGroups> poiTagsGroups)
        {
            return new List<CMSMenuModel>
            { 
                new CMSMenuModel
                {
                    Title = "POI Tags",
                    MenuItems = poiTagsGroups.Select(tag =>   new CMSMenuItem
                    {
                        MenuItemID = tag.POITagsGroupID,
                        BoldedTitle = tag.POITG_Title,
                        IsEdit = false,
                        IsDelete = false,
                        SubMenuItems = CreatePOITagsSubmenu(tag.tbl_POITags)
                    }).ToList()
                }
            };
        }

        private static List<CMSMenuItem> CreatePOITagsSubmenu(EntityCollection<tbl_POITags> tags)
        {
            return tags.Select(tag => new CMSMenuItem
             {
                 MenuItemID = tag.POITagID,
                 Title = tag.POIT_Title,
             }).ToList();
        }

        public static List<CMSMenuModel> MapPOITagsGroups(List<tbl_POITagsGroups> poiTagsGroups)
        {
            return new List<CMSMenuModel> 
            {
                new CMSMenuModel
                {
                    Title = "POI Tag Groups",
                    MenuItems = poiTagsGroups.Select(group => new CMSMenuItem
                    {
                        MenuItemID = group.POITagsGroupID,
                        Title = group.POITG_Title
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapPOIs(List<tbl_POI> pois)
        {
            return new List<CMSMenuModel>
            {
                new CMSMenuModel
                {
                    Title = "Points Of Interest",
                    MenuItems = pois.Select(poi => new CMSMenuItem
                    {
                        MenuItemID = poi.POIID,
                        Title = poi.POI_Title,
                        IsEditImages = true,
                        EditImagesText = "Edit point of interest files"
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapTemplates(List<tbl_Templates> templates)
        {
            return new List<CMSMenuModel>
            {
                new CMSMenuModel
                { 
                    Title = "Templates",
                    MenuItems = templates.Select(template => new CMSMenuItem
                    {
                        MenuItemID = template.TemplateID,
                        Title = template.T_Name
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapPortfolioItems(List<tbl_Portfolio> portfolio)
        {
            return new List<CMSMenuModel>
            {
                new CMSMenuModel
                { 
                    Title = "Portfolio Items",
                    MenuItems = portfolio.Select(p => new CMSMenuItem
                    {
                        MenuItemID = p.PortfolioID,
                        Title = p.PO_Title,
                        IsVisibility = true,
                        VisibilityText = "Enable/Disable Portfolio Item",
                        Visible = p.PO_Live,
                        IsMove = true,
                        IsEditImages = true,
                        EditImagesText = "Edit portfolio images"
                    }).ToList()
                }
            };
        }

        public static List<CMSMenuModel> MapGalleryItems(List<tbl_Gallery> galleries)
        {
            return new List<CMSMenuModel>
            {
                new CMSMenuModel
                { 
                    Title = "Galleries",
                    MenuItems = galleries.Select(g => new CMSMenuItem
                    {
                        MenuItemID = g.GalleryID,
                        Title = g.G_Title,
                        IsVisibility = true,
                        VisibilityText = "Enable/Disable Gallery Item",
                        Visible = g.G_Live,
                        IsMove = true,
                        IsEditImages = true,
                        EditImagesText = "Edit gallery images"
                    }).ToList()
                }
            };
        }
    }
}
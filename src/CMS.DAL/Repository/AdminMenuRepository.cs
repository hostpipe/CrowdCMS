using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IAdminMenuRepository
    {
        bool DeleteMenuItem(int menuItemID);
        IQueryable<tbl_AdminMenu> GetAll();
        tbl_AdminMenu GetByID(int menuItemID);
        tbl_AdminMenu SaveMenuItem(string menuText, int parentID, string url, int menuItemID);
        bool SaveOrder(int[] orderedMenuItemIDs);
        tbl_AdminMenu SaveVisibility(int menuItemID);
    }

    public class AdminMenuRepository : Repository<tbl_AdminMenu>, IAdminMenuRepository
    {
        public AdminMenuRepository(IDALContext context) : base(context) { }

        public bool DeleteMenuItem(int menuItemID)
        {
            var menuItem = DbSet.FirstOrDefault(mi => mi.AdminMenuID == menuItemID);
            if (menuItem == null)
                return false;

            foreach (var item in menuItem.tbl_AccessRights.ToList())
            {
                this.Context.Set<tbl_AccessRights>().Remove(item);
            }

            this.Delete(menuItem);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_AdminMenu> GetAll()
        {
            return this.All();
        }

        public tbl_AdminMenu GetByID(int menuItemID)
        {
            return this.DbSet.FirstOrDefault(mi => mi.AdminMenuID == menuItemID);
        }

        public tbl_AdminMenu SaveMenuItem(string menuText, int parentID, string url, int menuItemID)
        {
            var menuItem = DbSet.FirstOrDefault(mi => mi.AdminMenuID == menuItemID);
            var previousMenuItem = DbSet.Where(mi => mi.AM_ParentID == parentID).OrderByDescending(mi => mi.AM_Order).FirstOrDefault();
            if (menuItem == null)
            {
                menuItem = new tbl_AdminMenu()
                {
                    AM_Live = false,
                    AM_Order = previousMenuItem != null ? Convert.ToInt16(previousMenuItem.AM_Order + 1) : Convert.ToInt16(1)
                };
                this.Create(menuItem);
            }

            menuItem.AM_MenuText = menuText;
            menuItem.AM_ParentID = parentID;
            menuItem.AM_URL = url;

            this.Context.SaveChanges();
            return menuItem;
        }

        public bool SaveOrder(int[] orderedMenuItemIDs)
        {
            if (orderedMenuItemIDs == null) return false;

            for (int i = 0; i < orderedMenuItemIDs.Count(); i++)
            {
                var menuItemID = orderedMenuItemIDs[i];
                var menuItem = this.DbSet.FirstOrDefault(mi => mi.AdminMenuID == menuItemID);
                if (menuItem != null) menuItem.AM_Order = Convert.ToInt16(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }

        public tbl_AdminMenu SaveVisibility(int menuItemID)
        {
            var menuItem = DbSet.FirstOrDefault(ug => ug.AdminMenuID == menuItemID);
            if (menuItem == null)
                return null;

            menuItem.AM_Live = !menuItem.AM_Live;
            this.Context.SaveChanges();
            return menuItem;
        }
    }
}

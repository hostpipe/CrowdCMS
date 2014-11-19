using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IUserGroupsRepository
    {
        IQueryable<tbl_UserGroups> GetAll();
        tbl_UserGroups GetByID(int userGroupID);
        bool DeleteUserGroup(int userGroupID);
        tbl_UserGroups SaveUserGroup(string type, int[] menuItems, int[] permissions, int userGroupID);
    }

    public class UserGroupsRepository : Repository<tbl_UserGroups>, IUserGroupsRepository
    {
        public UserGroupsRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_UserGroups> GetAll()
        {
            return this.All();
        }

        public tbl_UserGroups GetByID(int userGroupID)
        {
            return this.DbSet.FirstOrDefault(ug => ug.UserGroupID == userGroupID);
        }

        public bool DeleteUserGroup(int userGroupID)
        {
            var userGroup = DbSet.FirstOrDefault(ug => ug.UserGroupID == userGroupID);
            if (userGroup == null)
                return false;

            foreach (var item in userGroup.tbl_AdminUsers.ToList())
            {
                this.Context.Set<tbl_AdminUsers>().Remove(item);
            }

            foreach (var item in userGroup.tbl_AccessRights.ToList())
            {
                this.Context.Set<tbl_AccessRights>().Remove(item);
            }

            this.Delete(userGroup);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_UserGroups SaveUserGroup(string type, int[] menuItems, int[] permissions, int userGroupID)
        {
            var userGroup = DbSet.FirstOrDefault(ug => ug.UserGroupID == userGroupID);
            var orderConter = All().Max(ug => ug.UG_Order);
            if (userGroup == null)
            {
                userGroup = new tbl_UserGroups()
                {
                    UG_Order = orderConter.HasValue ? orderConter.Value + 10 : 10
                };
                this.Create(userGroup);
            }

            userGroup.UG_Type = type;
            
            foreach (var item in userGroup.tbl_AccessRights.ToList())
	        {
                this.Context.Set<tbl_AccessRights>().Remove(item);
	        }
            this.Context.SaveChanges();

            if (menuItems != null)
                foreach (var id in menuItems)
	            {
                    userGroup.tbl_AccessRights.Add(new tbl_AccessRights()
                    {
                        AR_AdminMenuID = id,
                        AR_UserGroupID = userGroupID
                    });
	            }

            if (permissions != null)
                foreach (var id in permissions)
                {
                    userGroup.tbl_AccessRights.Add(new tbl_AccessRights()
                    {
                        AR_PermissionsID = id,
                        AR_UserGroupID = userGroupID
                    });
                }

            this.Context.SaveChanges();
            return userGroup;
        }
    }
}

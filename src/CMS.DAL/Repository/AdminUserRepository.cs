using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IAdminUserRepository
    {
        bool DeleteUser(int userID);
        IQueryable<tbl_AdminUsers> GetAll();
        tbl_AdminUsers GetByEmail(string email);
        tbl_AdminUsers GetByEmailAndPassword(string email, string password);
        tbl_AdminUsers GetByID(int userID);
        List<string> GetPermissionsByID(int userID);
        tbl_AdminUsers SaveUser(string email, string userName, string password, int groupID, int userID = 0);
    }

    public class AdminUserRepository : Repository<tbl_AdminUsers>, IAdminUserRepository
    {
        public AdminUserRepository(IDALContext context) : base(context) { }

        public bool DeleteUser(int userID)
        {
            var user = this.DbSet.FirstOrDefault(au => au.AdminUserID == userID);
            if (user == null)
                return false;

            this.Delete(user);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_AdminUsers> GetAll()
        {
            return this.All();
        }

        public tbl_AdminUsers GetByEmail(string email)
        {
            return this.DbSet.FirstOrDefault(au => au.US_Email.Equals(email));
        }

        public tbl_AdminUsers GetByEmailAndPassword(string email, string password)
        {
            return this.DbSet.FirstOrDefault(au => au.US_Email.Equals(email) && au.US_Password.Equals(password));
        }

        public tbl_AdminUsers GetByID(int userID)
        {
            return this.DbSet.FirstOrDefault(au => au.AdminUserID == userID);
        }

        public List<string> GetPermissionsByID(int userID)
        {
            var user = this.DbSet.FirstOrDefault(au => au.AdminUserID == userID);
            if (user == null)
                return null;

            return user.tbl_UserGroups.tbl_AccessRights.Where(ar => ar.AR_PermissionsID.HasValue).Select(ar => ar.tbl_Permissions.P_Name).ToList();
        }

        public tbl_AdminUsers SaveUser(string email, string userName, string password, int groupID, int userID)
        {
            var user = this.DbSet.FirstOrDefault(au => au.AdminUserID == userID);
            if (user == null)
            {
                user = new tbl_AdminUsers();
                Create(user);
            }

            user.US_Email = email;
            user.US_UserName = userName;
            user.US_UserGroupID = groupID;
            if (!String.IsNullOrEmpty(password))
                user.US_Password = password;

            Context.SaveChanges();
            return user;
        }
    }
}

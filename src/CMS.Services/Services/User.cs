using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;
using CMS.Utils.Cryptography;
using CMS.Utils.Diagnostics;
using CMS.BL;
using System.Web.Mvc;

namespace CMS.Services
{
    public class User : ServiceBase, IUser
    {
        private IAdminUserRepository AdminUserRepository { get; set; }
        private IUserGroupsRepository UserGroupRepository { get; set; }
        private ICustomerRepository CustomerRepository { get; set; }
        private IDomainsRepository DomainsRepository { get; set; }

        public User()
            : base()
        {
            this.AdminUserRepository = new AdminUserRepository(this.Context);
            this.UserGroupRepository = new UserGroupsRepository(this.Context);
            this.CustomerRepository = new CustomerRepository(this.Context);
            this.DomainsRepository = new DomainsRepository(this.Context);
        }

        #region Customer

        public bool CanUseCustomerEmail(string email, int domainID, int currentCustomerID = 0)
        {
            var customer = this.GetCustomerByEmail(email, domainID);
            return customer != null && customer.CU_Registered && customer.CustomerID != currentCustomerID ? false : true;
        }


        public bool DeleteCustomer(int customerID)
        {
            return CustomerRepository.DeleteCustomer(customerID);
        }

        public List<tbl_Customer> GetAllCustomers()
        {
            return CustomerRepository.GetAll().Where(c => !c.CU_IsDormant).ToList();
        }

        public List<tbl_Customer> GetAllCustomers(int domainID)
        {
            return CustomerRepository.GetAll().Where(c => c.CU_DomainID == domainID && !c.CU_IsDormant).ToList();
        }

        public SelectList GetAllCustomersAsSelectList()
        {
            return new SelectList(CustomerRepository.GetAll().Where(c => !c.CU_IsDormant), "CustomerID", "FullName");
        }

        public List<tbl_Customer> GetAllRegisteredCustomers()
        {
            return CustomerRepository.GetAll().Where(c => c.CU_Registered && !c.CU_IsDormant).ToList();
        }

        public SelectList GetAllRegisteredCustomersAsSelectList()
        {
            return new SelectList(CustomerRepository.GetAll().Where(c => c.CU_Registered && !c.CU_IsDormant).AsEnumerable(), "CustomerID", "FullName");
        }

        public List<tbl_Customer> GetAllUnRegisteredCustomers()
        {
            return CustomerRepository.GetAll().Where(c => !c.CU_Registered && !c.CU_IsDormant).ToList();
        }

        public SelectList GetAllUnRegisteredCustomersAsSelectList()
        {
            return new SelectList(CustomerRepository.GetAll().Where(c => !c.CU_Registered && !c.CU_IsDormant).AsEnumerable(), "CustomerID", "FullName");
        }

        public SelectList GetCustomersByDomainAsSelectList(int domainID)
        {
            return new SelectList(CustomerRepository.GetAll().Where(c => c.CU_DomainID == domainID && !c.CU_IsDormant).ToList(), "CustomerID", "FullName");
        }

        public SelectList GetCustomersExtendedByDomainAsSelectList(int domainID)
        {
            return new SelectList(CustomerRepository.GetAll()
                .Where(c => c.CU_DomainID == domainID && !c.CU_IsDormant)
                .ToList()
                .Select(c => new { Value = c.CustomerID, Text = c.FullText }), "Value", "Text");
        }

        public tbl_Customer GetCustomerByEmail(string email, int domainID)
        {
            return CustomerRepository.GetByEmail(email, domainID);
        }

        public tbl_Customer GetCustomerByEmailAndPassword(string email, string password, int domainID)
        {
            return CustomerRepository.GetByEmailAndPassword(email, Sha512.GetSHA512Hash(password), domainID);
        }

        public tbl_Customer GetCustomerByID(int customerID)
        {
            return (customerID != 0) ?
                CustomerRepository.GetByID(customerID) :
                CustomerRepository.GetAll().OrderBy(c => c.CU_Surname).ThenByDescending(c => c.CU_RegisterDate).FirstOrDefault();
        }

        public tbl_Customer SaveCustomer(string email, string firstName, string surname, string telephone, string title, string password, int domainID, int customerID, bool registered, bool detailsFor3rdParties, string adminNote)
        {
            if (registered && customerID == 0 && String.IsNullOrEmpty(password))
                return null;

            if (String.IsNullOrEmpty(email))
                return null;

            var customer = CustomerRepository.SaveCustomer(email, firstName, surname, telephone, title, domainID, customerID, registered, detailsFor3rdParties, adminNote);
            if (customer == null)
                return null;

            if (!String.IsNullOrEmpty(password))
                CustomerRepository.SavePassword(customer.CustomerID, Sha512.GetSHA512Hash(password));

            return customer;
        }

        public tbl_Customer SaveDormantFlag(int customerID, bool isDormant)
        {
            return CustomerRepository.SaveDormantFlag(customerID, isDormant);
        }

        public List<tbl_Customer> SearchCustomers(string search, string character, int domainID, bool isRegistered, bool isUnRegistered, bool isDormant = false)
        {
            var customers = CustomerRepository.GetAll();

            search = string.IsNullOrEmpty(search) ? null : search.ToLowerInvariant();
            character = string.IsNullOrEmpty(character) ? null : character.ToLowerInvariant();

            return customers.Where(c => (domainID == 0 || c.CU_DomainID == domainID) && c.CU_IsDormant == isDormant &&
                    (string.IsNullOrEmpty(character) || c.CU_Surname.StartsWith(character)) &&
                    (string.IsNullOrEmpty(search) || c.CU_Email.Contains(search) || c.CU_Title.Contains(search) || c.CU_FirstName.Contains(search) || c.CU_Surname.Contains(search)) &&
                    (!isRegistered || c.CU_Registered) && (!isUnRegistered || !c.CU_Registered))
                    .OrderBy(c => c.CU_Surname).ThenByDescending(c => c.CU_RegisterDate).ToList();
        }

        public bool GetSubscriptionStatus(string email, int domainID)
        {
            tbl_Domains domain = DomainsRepository.GetByID(domainID);
            if (domain == null || !domain.IsAnyCRMEnabled)
                return false;

            if (domain.DO_EnableMailChimp)
            {
                var mailchimp = (MailChimp)DependencyResolver.Current.GetService(typeof(MailChimp));
                if (mailchimp.GetSubscriptionStatus(email, domainID))
                    return true;
            }

            if (domain.DO_EnableCommuniGator)
            {
                var communiGator = (CommuniGator)DependencyResolver.Current.GetService(typeof(CommuniGator));
                if (communiGator.GetSubscriptionStatus(email, domainID))
                    return true;
            }

            return false;
        }

        public bool SubscribeNewsletter(string email, bool subscribe, int domainID)
        {
            tbl_Domains domain = DomainsRepository.GetByID(domainID);
            if (domain == null || !domain.IsAnyCRMEnabled)
                return false;

            bool success = true;

            if (domain.DO_EnableMailChimp)
            {
                var mailchimp = (MailChimp)DependencyResolver.Current.GetService(typeof(MailChimp));
                success &= mailchimp.Subscribe(email, domainID);
            }

            if (domain.DO_EnableCommuniGator)
            {
                var communiGator = (CommuniGator)DependencyResolver.Current.GetService(typeof(CommuniGator));
                success &= communiGator.Subscribe(email, domainID);
            }

            var customer = CustomerRepository.GetByEmail(email, domainID);
            if (customer != null && success)
                CustomerRepository.SetSubscription(customer.CustomerID, true);

            return success;
        }

        #endregion


        #region AdminUsers

        public bool CanUseUserEmail(string email, int currentUserID = 0)
        {
            var user = this.GetUserByEmail(email);
            return (user != null && user.AdminUserID != currentUserID) ? false : true;
        }

        public List<tbl_AdminUsers> GetAllUsers()
        {
            return AdminUserRepository.GetAll().ToList();
        }

        public List<string> GetPermissionsByUserID(int userID)
        {
            return AdminUserRepository.GetPermissionsByID(userID);
        }

        public tbl_AdminUsers GetUserByEmail(string email)
        {
            return AdminUserRepository.GetByEmail(email);
        }

        public tbl_AdminUsers GetUserByEmailAndPassword(string email, string password)
        {
            return AdminUserRepository.GetByEmailAndPassword(email, Sha512.GetSHA512Hash(password));
        }

        public tbl_AdminUsers GetUserByID(int userID)
        {
            return AdminUserRepository.GetByID(userID);
        }

        public int GetUserGroupIDByUserID(int userID)
        {
            tbl_AdminUsers user = AdminUserRepository.GetByID(userID);
            return (user != null) ? user.US_UserGroupID : 0;
        }

        public bool DeleteUser(int userID)
        {
            return AdminUserRepository.DeleteUser(userID);
        }

        public tbl_AdminUsers SaveUser(string email, string userName, string password, int groupID, int userID)
        {
            if (userID == 0 && String.IsNullOrEmpty(password))
                return null;

            password = String.IsNullOrEmpty(password) ? String.Empty : Sha512.GetSHA512Hash(password);
            return AdminUserRepository.SaveUser(email, userName, password, groupID, userID);
        }

        #endregion


        #region User Groups

        public List<tbl_UserGroups> GetAllUserGroups()
        {
            return UserGroupRepository.GetAll().ToList();
        }

        public List<tbl_UserGroups> GetAllUserGroupsOrdered()
        {
            return UserGroupRepository.GetAll().OrderBy(ug => ug.UG_Order).ToList();
        }

        public tbl_UserGroups GetUserGroupByID(int groupID)
        {
            return UserGroupRepository.GetByID(groupID);
        }

        public int GetUsersAmountForUserGroup(int groupID)
        {
            var group = UserGroupRepository.GetByID(groupID);
            return (group != null) ? 
                group.tbl_AdminUsers.Count : 
                0;
        }

        public bool DeleteUserGroup(int groupID)
        {
            return UserGroupRepository.DeleteUserGroup(groupID);
        }

        public tbl_UserGroups SaveUserGroup(string groupName, int[] menuItems, int[] permissions, int groupID)
        {
            return (String.IsNullOrEmpty(groupName)) ?
                null :
                UserGroupRepository.SaveUserGroup(groupName, menuItems, permissions, groupID);
        }

        #endregion
    }
}

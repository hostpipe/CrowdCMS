using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.Services
{
    public interface IUser
    {
        bool CanUseCustomerEmail(string email, int domainID, int currentCustomerID = 0);
        bool DeleteCustomer(int customerID);
        List<tbl_Customer> GetAllCustomers();
        List<tbl_Customer> GetAllCustomers(int domainID);
        SelectList GetAllCustomersAsSelectList();
        SelectList GetCustomersByDomainAsSelectList(int domainID);
        SelectList GetCustomersExtendedByDomainAsSelectList(int domainID);
        tbl_Customer GetCustomerByEmail(string email, int domainID);
        tbl_Customer GetCustomerByEmailAndPassword(string email, string password, int domainID);
        tbl_Customer GetCustomerByID(int customerID);
        tbl_Customer SaveCustomer(string email, string firstName, string surname, string telephone, string title, string password, int domainID, int customerID, bool registered, bool detailsFor3rdParties, string adminNote);
        tbl_Customer SaveDormantFlag(int customerID, bool isDormant);
        List<tbl_Customer> SearchCustomers(string search, string character, int domainID, bool isRegistered, bool isUnRegistered, bool isDormant = false);

        bool CanUseUserEmail(string email, int currentUserID = 0);
        List<tbl_AdminUsers> GetAllUsers();
        List<string> GetPermissionsByUserID(int userID);
        tbl_AdminUsers GetUserByEmail(string email);
        tbl_AdminUsers GetUserByEmailAndPassword(string email, string password);
        tbl_AdminUsers GetUserByID(int userID);
        int GetUserGroupIDByUserID(int userID);
        bool DeleteUser(int userID);
        tbl_AdminUsers SaveUser(string email, string userName, string password, int groupID, int userID);

        List<tbl_UserGroups> GetAllUserGroups();
        List<tbl_UserGroups> GetAllUserGroupsOrdered();
        tbl_UserGroups GetUserGroupByID(int groupID);
        int GetUsersAmountForUserGroup(int groupID);
        bool DeleteUserGroup(int groupID);
        tbl_UserGroups SaveUserGroup(string groupName, int[] menuItems, int[] permissions, int groupID);

        bool SubscribeNewsletter(string email, bool subscribe, int domainID);
        bool GetSubscriptionStatus(string email, int domainID);
    }
}

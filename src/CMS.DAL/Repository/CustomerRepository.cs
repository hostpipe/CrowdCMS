using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ICustomerRepository
    {
        bool DeleteCustomer(int customerID);
        IQueryable<tbl_Customer> GetAll();
        tbl_Customer GetByEmail(string email, int domainID);
        tbl_Customer GetByEmailAndPassword(string email, string password, int domainID);
        tbl_Customer GetByID(int customerID);
        tbl_Customer SaveDormantFlag(int customerID, bool isDormant);
        tbl_Customer SaveCustomer(string email, string firstName, string surname, string telephone, string title, int domainID, int customerID, bool registered, bool detailsFor3rdParties, string adminNote);
        bool SavePassword(int customerID, string password);

        bool SetSubscription(int customerID, bool set);
    }

    public class CustomerRepository : Repository<tbl_Customer>, ICustomerRepository
    {
        public CustomerRepository(IDALContext context) : base(context) { }

        public bool DeleteCustomer(int customerID)
        {
            var customer = this.DbSet.FirstOrDefault(c => c.CustomerID == customerID);
            if (customer == null)
                return false;

            this.Delete(customer);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Customer> GetAll()
        {
            return this.All();
        }

        public tbl_Customer GetByEmail(string email, int domainID)
        {
            return this.DbSet.FirstOrDefault(c => c.CU_Email.Equals(email) && c.CU_DomainID == domainID && c.CU_Registered);
        }

        public tbl_Customer GetByEmailAndPassword(string email, string password, int domainID)
        {
            return this.DbSet.FirstOrDefault(c => c.CU_Email.Equals(email) && c.CU_Password.Equals(password) && c.CU_DomainID == domainID && c.CU_Registered);
        }

        public tbl_Customer GetByID(int customerID)
        {
            return this.DbSet.FirstOrDefault(c => c.CustomerID == customerID);
        }

        public bool SetSubscription(int customerID, bool set)
        {
            var customer = this.DbSet.FirstOrDefault(c => c.CustomerID == customerID);
            if (customer == null)
                return false;
            customer.CU_Subscription = set;
            this.Context.SaveChanges();
            return true;
        }

        public tbl_Customer SaveCustomer(string email, string firstName, string surname, string telephone, string title, int domainID, int customerID, bool registered, bool detailsFor3rdParties, string adminNote)
        {
            var customer = this.DbSet.FirstOrDefault(c => c.CustomerID == customerID && c.CU_DomainID == domainID);
            if (customer == null)
            {
                customer = new tbl_Customer()
                {
                    CU_RegisterDate = DateTime.UtcNow,
                    CU_DomainID = domainID
                };
                Create(customer);
            }

            customer.CU_Email = email;
            customer.CU_FirstName = firstName;
            customer.CU_Surname = surname;
            customer.CU_Telephone = telephone;
            customer.CU_Title = title;
            customer.CU_Registered = registered;
            customer.CU_DetailsFor3rdParties = detailsFor3rdParties;
            customer.CU_AdminNote = adminNote;

            Context.SaveChanges();
            return customer;
        }

        public tbl_Customer SaveDormantFlag(int customerID, bool isDormant)
        {
            var customer = this.DbSet.FirstOrDefault(c => c.CustomerID == customerID);
            if (customer == null)
                return null;

            customer.CU_IsDormant = isDormant;

            this.Context.SaveChanges();
            return customer;
        }

        public bool SavePassword(int customerID, string password)
        {
            var customer = this.DbSet.FirstOrDefault(c => c.CustomerID == customerID);
            if (customer == null)
                return false;

            customer.CU_Password = password;

            this.Context.SaveChanges();
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IAddressRepository
    {
        IQueryable<tbl_Address> GetAll();
        tbl_Address SaveAddress(int? customerId, string country, int? countryId, string county, string firstName, string surName, string title,
            string addLine1, string addLine2, string addLine3, string postcode, string phone, string town, int addressId);
        bool DeleteAddress(int addressID);
    }

    public class AddressRepository : Repository<tbl_Address>, IAddressRepository
    {
        public AddressRepository(IDALContext context) : base(context) { }

        public IQueryable<tbl_Address> GetAll()
        {
            return this.DbSet.AsQueryable();
        }

        public bool DeleteAddress(int addressID)
        {
            var address = this.DbSet.FirstOrDefault(m=>m.AddressID == addressID);
            if(address == null)
                return false;
            this.Delete(address);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_Address SaveAddress(int? customerId, string country, int? countryId, string county, string firstName, string surName, string title, 
            string addLine1, string addLine2, string addLine3, string postcode, string phone, string town, int addressId)
        {
            var address = this.DbSet.FirstOrDefault(ad => ad.AddressID == addressId);
            if (address == null)
            {
                address = new tbl_Address();
                this.Create(address);
            }
            address.A_CustomerID = customerId;
            address.A_County = county;
            address.A_Country = country;
            address.A_CountryID = countryId;
            address.A_FirstName = firstName;
            address.A_Surname = surName;
            address.A_Title = title;
            address.A_Line1 = addLine1;
            address.A_Line2 = addLine2;
            address.A_Line3 = addLine3;
            address.A_Phone = phone;
            address.A_Postcode = postcode;
            address.A_Town = town;

            this.Context.SaveChanges();
            return address;
        }
    }
}

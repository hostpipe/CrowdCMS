using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;

namespace CMS.DAL.Repository
{
    public interface ICountryRepository
    {
        IQueryable<tbl_Country> GetAllForDomain(int domainID);
        IQueryable<tbl_Country> GetAllForDomainWithPostage(int domainID);
        string GetCountryCode(int domainID, string countryName);
        tbl_Country GetDefaultForDomain(int domainID);
        tbl_Country GetByID(int countryID);
        tbl_Country GetCountry(int domainID, string countryName);
        int ImportCountries(int sourceDomainID, int destDomainID);
        int ImportCountriesFromBase(int destDomainID);
        bool Delete(int countryID);
        tbl_Country Save(bool isDefault, string code, string countryName, int domainID, int? postageZoneID, int countryID);
        bool SaveOrder(int[] orderedCountryIDs);
    }

    public class CountryRepository : Repository<tbl_Country>, ICountryRepository
    {
        public CountryRepository(DALContext context) : base(context) { }

        public bool Delete(int countryID)
        {
            var country = this.DbSet.FirstOrDefault(m => m.CountryID == countryID);
            if (country == null || country.C_IsDefault || country.tbl_Address.Count > 0)
                return false;

            this.DbSet.Remove(country);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Country> GetAllForDomain(int domainID)
        {
            return this.DbSet.Where<tbl_Country>(c => c.C_DomainID == domainID).AsQueryable();
        }

        public IQueryable<tbl_Country> GetAllForDomainWithPostage(int domainID)
        {
            return this.DbSet.Where<tbl_Country>(c => c.C_DomainID == domainID && c.C_PostageZoneID.HasValue).AsQueryable();
        }

        public string GetCountryCode(int domainID, string countryName)
        {
            return this.DbSet.Where(m => m.C_DomainID == domainID && m.C_Country == countryName).Select(m => m.C_Code).FirstOrDefault();
        }

        public tbl_Country GetByID(int countryID)
        {
            return this.DbSet.FirstOrDefault(m => m.CountryID == countryID);
        }

        public tbl_Country GetCountry(int domainID, string countryName)
        {
            return this.DbSet.Where(m => m.C_DomainID == domainID && m.C_Country == countryName).FirstOrDefault();
        }

        public tbl_Country GetDefaultForDomain(int domainID)
        {
            return this.DbSet.FirstOrDefault(m => m.C_DomainID == domainID && m.C_IsDefault);
        }

        public int ImportCountries(int sourceDomainID, int destDomainID)
        {
            var destCodes = this.DbSet.Where(c => c.C_DomainID == destDomainID).Select(x => x.C_Code).ToArray();
            var countriesToImport = this.DbSet.Where(c => !destCodes.Contains(c.C_Code) && c.C_DomainID == sourceDomainID).ToList();

            foreach (var c in countriesToImport)
            {
                var newCountry = new tbl_Country
                 {
                     C_Code = c.C_Code,
                     C_Country = c.C_Country,
                     C_DomainID = destDomainID,
                     C_IsDefault = c.C_IsDefault,
                     C_Order = c.C_Order,
                     C_PostageZoneID = null
                 };
                this.Create(newCountry);
            }
            this.Context.SaveChanges();

            return countriesToImport.Count();
        }

        public int ImportCountriesFromBase(int destDomainID)
        {
            var destCodes = this.DbSet.Where(c => c.C_DomainID == destDomainID).Select(x => x.C_Code).ToArray();
            var countriesToImport = this.Context.Set<tbl_CountryBase>().Where(c => !destCodes.Contains(c.C_Code)).ToList();

            for (int i = 0; i < countriesToImport.Count(); i++)
            {
                 var newCountry = new tbl_Country
                {
                    C_Code = countriesToImport[i].C_Code,
                    C_Country = countriesToImport[i].C_Country,
                    C_DomainID = destDomainID,
                    C_Order = Convert.ToInt16(i)
                };
                this.Create(newCountry);
            }
            this.Context.SaveChanges();

            return countriesToImport.Count();
        }


        public tbl_Country Save(bool isDefault, string code, string countryName, int domainID, int? postageZoneID, int countryID)
        {
            var country = this.DbSet.FirstOrDefault(c => c.CountryID == countryID);
            if (country == null)
            {
                short order = (short)(this.DbSet.Max(c => c.C_Order).GetValueOrDefault(0) + 1);
                country = new tbl_Country()
                {
                    C_Order = order
                };
                this.Create(country);
            }

            if (isDefault)
            {
                var defCountry = this.DbSet.FirstOrDefault(m => m.C_DomainID == domainID && m.C_IsDefault);
                if (defCountry != null)
                    defCountry.C_IsDefault = false;

                country.C_IsDefault = isDefault;
            }

            country.C_Code = code;
            country.C_Country = countryName;
            country.C_DomainID = domainID;
            country.C_PostageZoneID = postageZoneID;

            this.Context.SaveChanges();
            return country;
        }

        public bool SaveOrder(int[] orderedCountryIDs)
        {
            if (orderedCountryIDs == null) return false;

            for (int i = 0; i < orderedCountryIDs.Count(); i++)
            {
                var countryID = orderedCountryIDs[i];
                var country = this.DbSet.FirstOrDefault(c => c.CountryID == countryID);
                if (country != null) country.C_Order = Convert.ToInt16(i + 1);
            }

            this.Context.SaveChanges();
            return true;
        }
    }
}

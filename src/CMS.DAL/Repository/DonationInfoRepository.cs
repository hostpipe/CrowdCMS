using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;

namespace CMS.DAL.Repository
{
    public interface IDonationInfoRepository
    {
        IQueryable<tbl_DonationInfo> GetAll();
        IQueryable<tbl_DonationInfo> GetAllForDomain(int domainID);
        tbl_DonationInfo GetByID(int donationInfoID);
        bool Delete(int donationInfoID);
        tbl_DonationInfo Save(tbl_DonationInfo donationInfo);
        tbl_DonationInfo SaveImagePath(string path, int donationInfoID);
    }

    public class DonationInfoRepository : Repository<tbl_DonationInfo>, IDonationInfoRepository
    {
        public DonationInfoRepository(DALContext context) : base(context) {}

        public bool Delete(int donationInfoID)
        {
            var donationInfo = this.DbSet.FirstOrDefault(m => m.DonationInfoID == donationInfoID);
            if(donationInfo == null)
                return false;
            this.DbSet.Remove(donationInfo);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_DonationInfo GetByID(int donationInfoID)
        {
            return this.DbSet.FirstOrDefault(m => m.DonationInfoID == donationInfoID);
        }

        public IQueryable<tbl_DonationInfo> GetAllForDomain(int domainID)
        {
            return this.All().Where(m=> m.DI_DomainID == domainID);
        }

        public IQueryable<tbl_DonationInfo> GetAll()
        {
            return this.All();
        }

        public tbl_DonationInfo Save(tbl_DonationInfo donationInfo)
        {
            var donation = this.DbSet.FirstOrDefault(m => m.DonationInfoID == donationInfo.DonationInfoID);
            if (donation != null)
            {
                donation.DI_Amount = donationInfo.DI_Amount;
                donation.DI_Description = donationInfo.DI_Description;
                donation.DI_DomainID = donationInfo.DI_DomainID;
                donation.DI_DonationTypeID = donationInfo.DI_DonationTypeID;
                donation.DI_Title = donationInfo.DI_Title;
                donation.DI_Live = donationInfo.DI_Live;
            }
            else
            {
                donation = this.Create(donationInfo);
            }
            this.Context.SaveChanges();
            return donation;
        }

        public tbl_DonationInfo SaveImagePath(string url, int donationInfoID)
        {
            var donation = this.DbSet.FirstOrDefault(m => m.DonationInfoID == donationInfoID);
            if (donation == null)
                return null;
            donation.DI_ImagePath = url;
            this.Context.SaveChanges();
            return donation;
        }
    }
}

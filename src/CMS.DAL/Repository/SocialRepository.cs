using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ISocialRepository : IRepository<tbl_Social>
    {
        tbl_Social GetByID(int id);
        bool DeleteByID(int id);
        IQueryable<tbl_Social> GetByDomainID(int domainID);
        IQueryable<tbl_Social> GetAll();
        tbl_Social SaveSocial(string title, string url, int domainID, int socialID, string foreColour = null, string backColour = null);
        bool SaveMultipleSocial(List<tbl_Social> model);
        bool SwapStatus(int socialID);
    }

    public class SocialRepository : Repository<tbl_Social>, ISocialRepository
    {
        public SocialRepository(IDALContext context) : base(context) { }


        public IQueryable<tbl_Social> GetByDomainID(int domainID)
        {
            return this.DbSet.Where(s => s.S_DomainID == domainID);
        }

        public IQueryable<tbl_Social> GetAll()
        {
            return this.DbSet.AsQueryable();
        }

        public tbl_Social GetByID(int id)
        {
            return this.DbSet.FirstOrDefault(s => s.SocialID == id);
        }

        public bool DeleteByID(int id)
        {
            var social = this.DbSet.FirstOrDefault(s => s.SocialID == id);
            if (social == null)
                return false;

            this.Delete(social);
            this.Context.SaveChanges();
            return true;
        }

        public tbl_Social SaveSocial(string title, string url, int domainID, int socialID, string foreColour = null, string backColour = null)
        {
            var social = this.DbSet.FirstOrDefault(s => s.SocialID == socialID);
            if (social == null)
                return null;

            social.S_Title = title;
            social.S_URL = url;
            social.S_DomainID = domainID;
            social.S_ForeColour = foreColour;
            social.S_BackColour = backColour;
            
            this.Context.SaveChanges();
            return social;
        }

        public bool SaveMultipleSocial(List<tbl_Social> model)
        {
            if (model == null || model.Count() <= 0)
                return false;

            foreach (var item in model)
            {
                var social = this.DbSet.FirstOrDefault(s => s.SocialID == item.SocialID);
                if (social == null)
                {
                    this.DbSet.Add(item);
                }
                else
                {
                    social.S_URL = item.S_URL;
                    social.S_ForeColour = item.S_ForeColour;
                    social.S_BackColour = item.S_BackColour;
                    social.S_Live = item.S_Live;
                    social.S_DefaultBackColour = item.S_DefaultBackColour;
                    social.S_DefaultBorderRadius = item.S_DefaultBorderRadius;
                    social.S_DefaultForeColour = item.S_DefaultForeColour;
                    social.S_BorderRadius = item.S_BorderRadius;
                    social.S_IconClass = item.S_IconClass;
                    social.S_Title = item.S_Title;
                }
            }

            this.Context.SaveChanges();
            return true;
        }

        public bool SwapStatus(int socialID)
        {
            var social = this.DbSet.FirstOrDefault(s => s.SocialID == socialID);
            if (social == null)
                return false;

            social.S_Live = !social.S_Live;

            this.Context.SaveChanges();
            return true;
        }
    }
}

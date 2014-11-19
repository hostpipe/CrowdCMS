using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface IPostageWeightsRepository
    {
        IQueryable<tbl_PostageWeights> GetAll();

        bool Delete(int postageWeightID);

        tbl_PostageWeights Save(tbl_PostageWeights postageWeight);
    }
    public class PostageWeightsRepository : Repository<tbl_PostageWeights>, IPostageWeightsRepository
    {   
        public PostageWeightsRepository(IDALContext context) : base(context) { }

        public bool Delete(int postageWeightID)
        {
            var weight = this.DbSet.FirstOrDefault(m => m.PostageWeightID == postageWeightID);
            if (weight == null)
                return false;
            this.DbSet.Remove(weight);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_PostageWeights> GetAll()
        {
            return this.All();
        }

        public tbl_PostageWeights Save(tbl_PostageWeights postageWeight)
        {
            var post = this.CreateOrUpdate(postageWeight, postageWeight.PostageWeightID);
            this.Context.SaveChanges();
            return post;
        }
    }
}

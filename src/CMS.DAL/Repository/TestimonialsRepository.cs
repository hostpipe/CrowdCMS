using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{

    public interface ITestimonialsRepository
    {
        bool DeleteTestimonial(int testimonialID);
        IQueryable<tbl_Testimonials> GetAll();
        tbl_Testimonials GetByID(int testimonialID);
        tbl_Testimonials SaveTestimonial(int testimonialID, DateTime testimonialDate, string client, string company, string content);
        tbl_Testimonials SaveVisibility(int testimonialID);
    }

    public class TestimonialsRepository : Repository<tbl_Testimonials>, ITestimonialsRepository
    {
        public TestimonialsRepository(IDALContext context) : base(context) { }

        public bool DeleteTestimonial(int testimonialID)
        {
            var testimonial = this.DbSet.FirstOrDefault(t => t.TestimonialID == testimonialID);
            if (testimonial == null)
                return false;

            this.Delete(testimonial);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Testimonials> GetAll()
        {
            return this.All();
        }

        public tbl_Testimonials GetByID(int testimonialID)
        {
            return this.DbSet.FirstOrDefault(t => t.TestimonialID == testimonialID); 
        }

        public tbl_Testimonials SaveVisibility(int testimonialID)
        {
            var testimonial = DbSet.FirstOrDefault(t => t.TestimonialID == testimonialID);
            if (testimonial == null)
                return null;

            testimonial.TE_Live = !testimonial.TE_Live;
            this.Context.SaveChanges();
            return testimonial;
        }

        public tbl_Testimonials SaveTestimonial(int testimonialID, DateTime testimonialDate, string client, string company, string content)
        {
            var testimonial = this.DbSet.FirstOrDefault(t => t.TestimonialID == testimonialID);
            if (testimonial == null)
            {
                testimonial = new tbl_Testimonials();
                this.Create(testimonial);
            }

            testimonial.TE_Date = testimonialDate;
            testimonial.TE_Client = client;
            testimonial.TE_Company = company;
            testimonial.TE_Content = content;

            this.Context.SaveChanges();
            return testimonial;
        }
    }
}

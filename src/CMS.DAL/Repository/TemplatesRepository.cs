using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using CMS.DAL.Interface;

namespace CMS.DAL.Repository
{
    public interface ITemplatesRepository
    {
        bool DeleteTemplate(int templateID);
        IQueryable<tbl_Templates> GetAll();
        tbl_Templates GetByID(int templateID);
        tbl_Templates SaveTemplate(int templateID, string name, string header, bool useHeader, string footer, bool useFooter, bool live);
    }

    public class TemplatesRepository : Repository<tbl_Templates>, ITemplatesRepository
    {
        public TemplatesRepository(IDALContext context) : base(context) { }

        public bool DeleteTemplate(int templateID)
        {
            var Template = this.DbSet.FirstOrDefault(t => t.TemplateID == templateID);
            if (Template == null)
                return false;

            this.Delete(Template);
            this.Context.SaveChanges();
            return true;
        }

        public IQueryable<tbl_Templates> GetAll()
        {
            return this.All();
        }

        public tbl_Templates GetByID(int templateID)
        {
            return this.DbSet.FirstOrDefault(t => t.TemplateID == templateID);
        }

        public tbl_Templates SaveTemplate(int templateID, string name, string header, bool useHeader, string footer, bool useFooter, bool live)
        {
            var template = this.DbSet.FirstOrDefault(t => t.TemplateID == templateID);
            if (template == null)
            {
                template = new tbl_Templates();
                this.Create(template);
            }

            if (live)
            {
                foreach (var temp in this.GetAll())
                    temp.T_Live = false;
            }

            template.T_Name = name;
            template.T_Live = live;
            template.T_Footer = footer;
            template.T_UseFooter = useFooter;
            template.T_Header = header;
            template.T_UseHeader = useHeader;

            this.Context.SaveChanges();
            return template;
        }
    }
}

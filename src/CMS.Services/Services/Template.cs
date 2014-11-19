using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;

namespace CMS.Services
{
    public class Template: ServiceBase, ITemplate
    {
        private ITemplatesRepository TempateRepository { get; set; }

        public Template()
            : base()
        {
            this.TempateRepository = new TemplatesRepository(this.Context);
        }

        #region Templates

        public bool DeleteTempate(int templateID)
        {
           return this.TempateRepository.DeleteTemplate(templateID);
        }

        public List<tbl_Templates> GetAll()
        {
            return this.TempateRepository.GetAll().ToList();
        }

        public tbl_Templates GetByID(int templateID)
        {
            return this.TempateRepository.GetByID(templateID);
        }

        public tbl_Templates GetLive()
        {
            return this.TempateRepository.GetAll().FirstOrDefault(t => t.T_Live);
        }

        public tbl_Templates SaveTemplate(int templateID, string name, string header, bool useHeader, string footer, bool useFooter, bool live)
        {
            return this.TempateRepository.SaveTemplate(templateID, name, header, useHeader, footer, useFooter, live);
        }

        #endregion
    }
}

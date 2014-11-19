using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.BL.Entity;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class TemplatesModel
    {
        public TemplatesModel()
        {
            this.Footer = String.Empty;
            this.Header = String.Empty;
        }

        public TemplatesModel(tbl_Templates template)
        {
            this.TemplateID = template.TemplateID;
            this.Name = template.T_Name;
            this.Footer = template.T_Footer ?? String.Empty;
            this.UseFooter = template.T_UseFooter;
            this.Header = template.T_Header ?? String.Empty;
            this.UseHeader = template.T_UseHeader;
            this.Live = template.T_Live;
        }

        public int TemplateID { get; set; }
        [Display(Name = "Template Name")]
        public string Name { get; set; }
        public string Footer { get; set; }
        [Display(Name = "Use Footer")]
        public bool UseFooter { get; set; }
        public string Header { get; set; }
        [Display(Name = "Use Header")]
        public bool UseHeader { get; set; }
        [Display(Name = "Is Live")]
        public bool Live { get; set; }
    }
}
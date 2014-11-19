using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class SEOFieldsModel
    {
        [Display(Name = "Page Title")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }
        [Display(Name = "Page Description")]
        public string Desc { get; set; }
        [Display(Name = "Page Keywords")]
        public string Keywords { get; set; }
        [Display(Name = "301 Redirect")]
        public string R301 { get; set; }
        [Display(Name = "Meta Data")]
        [RegularExpression("(<meta\\s*(?:(?:\\b(\\w|-)+\\b\\s*(?:=\\s*(?:\"[^\"]*\"|'[^']*'|[^\"'<> ]+)\\s*)?)*)/?\\s*>\\s*)*", ErrorMessage = "Meta Data should have correct format, i.g.: &lt;meta name=\"name\" content=\"content\" /&gt;.")]
        public string MetaData { get; set; }
        [Display(Name = "Show In Sitemap")]
        public bool SiteMap { get; set; }
        [Display(Name = "XML Sitemap Priority")]
        public string Priority { get; set; }
        public bool IsPageContent { get; set; }
    }
}
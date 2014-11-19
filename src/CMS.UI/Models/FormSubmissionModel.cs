using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMS.UI.Models
{
    public class FormSubmissionModel
    {
        public int FormSubmissionId { get; set; }
        public int FormId { get; set; }
        public DateTime Received { get; set; }
        public string Email { get; set; }
        public bool Read { get; set; }
        public string Message { get; set; }
        
        public int Column { get; set; }
        public List<KeyValuePair<string, string>> MessageRows
        {
            get
            {
                return Message.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(cr => new KeyValuePair<string, string>(cr.Substring(0, cr.IndexOf(':')), cr.Substring(cr.IndexOf(':') + 1)))
                              .ToList<KeyValuePair<string, string>>();
            }
        }
    }
}
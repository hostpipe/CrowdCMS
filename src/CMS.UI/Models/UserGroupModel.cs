using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CMS.UI.Models
{
    public class UserGroupModel
    {
        public int UserGroupID { get; set; }
        [Required(ErrorMessage = "Group name is required.")]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; }
    }
}
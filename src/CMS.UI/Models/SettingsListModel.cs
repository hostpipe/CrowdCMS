using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using CMS.BL.Entity;

namespace CMS.UI.Models
{
    public class SettingsListModel
    {
        public List<SettingsValueModel> list;
        public int startCountAt = 0;
    }
}
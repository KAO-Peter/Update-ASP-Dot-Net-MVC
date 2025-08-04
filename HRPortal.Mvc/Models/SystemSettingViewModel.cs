using HRPortal.MultiLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class SystemSettingViewModel
    {
        public string DisplayName { get; set; }

        public string SettingKey { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "SettingValueRequiredError")]
        public string SettingValue { get; set; }
    }
}

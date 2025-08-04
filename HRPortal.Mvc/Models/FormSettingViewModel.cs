using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class FormSettingViewModel
    {
        public Guid FormID { get; set; }
        public String SettingKey { get; set; }
        public String SettingValue { get; set; }
        public String CompanyCode { get; set; }
    }
}

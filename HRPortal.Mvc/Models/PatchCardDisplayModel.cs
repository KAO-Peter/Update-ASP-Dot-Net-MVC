using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class PatchCardDisplayModel
    {
        public string SenderEmployeeEnglishName { get; set; }
        public string getLanguageCookie { get; set; }
        public string FormNo { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public string ReasonType { get; set; }
        public string Reason { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedTime { get; set; }

        //20190521 Daniel 增加部門英文名稱
        public string DepartmentEnglishName { get; set; }
    }
}

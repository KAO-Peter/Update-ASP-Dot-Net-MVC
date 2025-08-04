using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class LeaveDisplayModel
    {
        public string AbsentNameEn { get; set; }
        public string AgentEmployeeEnglishName { get; set; }
        public string SenderEmployeeEnglishName { get; set; }
        public string getLanguageCookie { get; set; }
        public string FormNo { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime CreatedTime { get; set; }
        public string AbsentType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Amount { get; set; }
        public string Unit { get; set; }
        public string LeaveReason { get; set; }
        public string AgentName { get; set; }

        public string FilePath { get; set; }
        public string FileName { get; set; }
        public bool IsAbroad { get; set; }
        public bool IsHRM { get; set; }
        public bool IsViewFile { get; set; }

        //天時分
        public string DHM { get; set; }

        /// <summary>
        /// 請假單申請人員工編號
        /// </summary>
        public string EmpID { get; set; }

        //20190517 Daniel 增加部門英文名稱
        public string DepartmentEnglishName { get; set; }

    }
}

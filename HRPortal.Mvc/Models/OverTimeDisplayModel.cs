using System;

namespace HRPortal.Mvc.Models
{
    public class OverTimeDisplayModel
    {
        
        public string SenderEmployeeEnglishName { get; set; }
        public string getLanguageCookie { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Amount { get; set; }
        public string Compensation { get; set; }
        public bool HaveDining { get; set; }
        public string OverTimeTypeName { get; set; }
        public string OverTimeReason { get; set; }
        public int CutTime { get; set; }
        public bool EnableSettingEatingTime { get; set; }
        public bool IsHRM { get; set; }

        public string FormNo { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public bool IsViewFile { get; set; }

        //20190521 Daniel 增加部門英文名稱
        public string DepartmentEnglishName { get; set; }
    }
}

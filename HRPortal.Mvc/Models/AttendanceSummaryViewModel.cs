using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class AttendanceSummaryViewModel
    {
        public IEnumerable<MutiSelectListItem> DepartmentListData { get; set; }
        public string SelectedDepartment { get; set; }

        public string Role { get; set; }

        public IEnumerable<MutiSelectListItem> EmployeeListData { get; set; }
        public string SelectedEmployee { get; set; }
        public string EmpID { get; set; }

        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public IEnumerable<SelectListItem> StatuslistDataData { get; set; }
        public string SelectedStatuslistData { get; set; }
        public List<PersonalAttendanceSummary> PersonalDetailDatas { get; set; }
        /// <summary> 排除臨時工 </summary>
        public bool ChkHireId { get; set; }
    }
    public class PersonalAttendanceSummary
    {
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentNo { get; set; }
        public List<DutySummaryDetail> SummaryDetail { get; set; }
    }

    public class DutySummaryDetail
    {
        public string ExcuteDate { get; set; }
        public string Weekday { get; set; }
        public string ClassType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string WorkHours { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string DutyTime { get; set; }
        public string DutyAmount { get; set; }
        public string LateTime { get; set; }
        public string EarlyLeaveTime { get; set; }
        public string AbsentName { get; set; }
        public string AbsentRange { get; set; }
        public string AbsentAmount { get; set; }
    }
}

using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class DeptStaffLeaveStatisticsViewModel
    {
        public IEnumerable<MutiSelectListItem> DepartmentListData { get; set; }
        public string SelectedDepartment { get; set; }

        public string DeptID { get; set; }
        public string EmpID { get; set; }

        public string Role { get; set; }

        public IEnumerable<MutiSelectListItem> EmployeeListData { get; set; }
        public string SelectedEmployee { get; set; }

        public IEnumerable<SelectListItem> YearListData { get; set; }
        public string SelectedYear { get; set; }

        public IEnumerable<SelectListItem> StatuslistDataData { get; set; }
        public string SelectedStatuslistData { get; set; }
        /// <summary>
        /// 假別資訊 SelectListItem
        /// </summary>
        public IEnumerable<SelectListItem> AbsentCodes { get; set; }

        /// <summary>
        /// 要查詢的假別代碼。
        /// </summary>
        public string QAbsentCodes { get; set; }

        public List<DeptSummary> DeptDetailDatas { get; set; }
    }

    public class DeptStaffLeaveSummaryViewModel
    {
        public string Year { get; set; }
        public List<DeptStaffLeaveDeptSummary> DeptDetailDatas { get; set; }
    }

    public class DeptStaffLeaveDeptSummary
    {
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public List<DeptStaffLeaveEmpSummary> EmpDetailDatas { get; set; }
    }


    public class DeptStaffLeaveEmpSummary
    {
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public List<double> ALEachMonth { get; set; }
        public List<decimal> EachMonth { get; set; }
        public List<decimal> DefMonth { get; set; }
        public decimal RemainingLeaveHours { get; set; }
        public decimal RemainingDeferredLeaveHours { get; set; }
        public List<decimal> LeaMonth { get; set; }
        public List<DeptSummary> DeptDetailDatas { get; set; }

    }

    public class DeptSummary
    {
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public List<DeptPersonalSummary> PersonalDetailDatas { get; set; }
    }

    public class DeptPersonalSummary
    {
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public List<HolidaySummaryDetailDeptStaff> SummaryDetail { get; set; }
    }

    public class HolidaySummaryDetailDeptStaff
    {
        public string TypeName { get; set; }
        public string AbsentCode { get; set; }
        public List<decimal> EachMonth { get; set; }
        public string ApprovedHours { get; set; }
        public string AnnualLeaveHours { get; set; }
        public string RemainderLeaveHours { get; set; }
        public string OverdueHours { get; set; }
        public string BalancedCount { get; set; } //平衡時數
        public string DefarredAmount { get; set; } //遞延時數
    }

    public class SortingData
    {
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string AbsentCode { get; set; }
        public string AbsentName { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string AnnualLeaveHours { get; set; }
        public List<decimal> EachMonth { get; set; }
        public string Year { get; set; }
    }
}
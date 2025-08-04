using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
     public class HolidaySummaryViewModel
    {
        public IEnumerable<SelectListItem> DepartmentListData { get; set; }
        public string SelectedDepartment { get; set; }
        public string Role { get; set; }
        public IEnumerable<SelectListItem> EmployeeListData { get; set; }
        public string SelectedEmployee { get; set; }
        public bool ChkHireId { get; set; }

        public IEnumerable<SelectListItem> YearListData { get; set; }
        public string SelectedYear { get; set; }
        public IEnumerable<SelectListItem> StatuslistDataData { get; set; }
        public string SelectedStatuslistData { get; set; }
        
        public List<HolidaySummaryDetail> DetailDatas { get; set; }
        public List<PersonalSummary> PersonalDetailDatas { get; set; }

        //2018/10/30 Neo 增加 假別欄位
        public string LeaveAmountListData { get; set; }

        /// <summary>
        /// 假別資訊 SelectListItem
        /// </summary>
        public IEnumerable<SelectListItem> AbsentCodes { get; set; }

        /// <summary>
        /// 要查詢的假別代碼。
        /// </summary>
        public string QAbsentCodes { get; set; }
        public List<DeptSummary> DeptDetailDatas { get; set; }
        public string EmpID { get; set; }

    }

    public class HolidaySummaryDetail
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

    public class PersonalSummary
    {
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }

        public List<HolidaySummaryDetail> SummaryDetail { get; set; }
    }
}

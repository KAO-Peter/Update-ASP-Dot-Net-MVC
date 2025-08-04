using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class AnnualLeaveScheduleReportViewModel
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
    }

    public class AnnualLeaveScheduleModel
    {
        public IEnumerable<SelectListItem> YearListData { get; set; }
        public string SelectedYear { get; set; }

        public decimal AnnualLeaveHours { get; set; }
        public decimal LeaveHours { get; set; }

        public double S01 { get; set; }
        public double S02 { get; set; }
        public double S03 { get; set; }
        public double S04 { get; set; }
        public double S05 { get; set; }
        public double S06 { get; set; }
        public double S07 { get; set; }
        public double S08 { get; set; }
        public double S09 { get; set; }
        public double S10 { get; set; }
        public double S11 { get; set; }
        public double S12 { get; set; }

        public decimal AnnualDeferredLeaveHours { get; set; }
        public decimal DeferredLeaveHours { get; set; }

        public double D01 { get; set; }
        public double D02 { get; set; }
        public double D03 { get; set; }
        public double D04 { get; set; }
        public double D05 { get; set; }
        public double D06 { get; set; }
        public double D07 { get; set; }
        public double D08 { get; set; }
        public double D09 { get; set; }
        public double D10 { get; set; }
        public double D11 { get; set; }
        public double D12 { get; set; }
    }

    public class AnnualLeaveSummaryViewModel
    {
        public string Year { get; set; }
        public List<AnnualLeaveDeptSummary> DeptDetailDatas { get; set; }
    }

    public class AnnualLeaveDeptSummary
    {
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public List<AnnualLeaveEmpSummary> EmpDetailDatas { get; set; }
    }


    public class AnnualLeaveEmpSummary
    {
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public List<double> ALEachMonth { get; set; }
        public List<decimal> EachMonth { get; set; }
        public List<decimal> DefMonth { get; set; }
        public decimal RemainingLeaveHours { get; set; }
        public decimal RemainingDeferredLeaveHours { get; set; }
        public List<decimal> LeaMonth { get; set; }

    }
}
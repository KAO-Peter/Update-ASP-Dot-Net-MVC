using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class FormQuerySummaryViewModel
    {
        public IEnumerable<SelectListItem> DepartmentListData { get; set; }
        public string SelectedDepartment { get; set; }

        public string DeptID { get; set; }
        public string EmpID { get; set; }

        public string Role { get; set; }

        public IEnumerable<SelectListItem> EmployeeListData { get; set; }
        public string SelectedEmployee { get; set; }

        public string BeginDate { get; set; }
        public string EndDate { get; set; }

        public string StatusData { get; set; }
        public string AbsentCode { get; set; }
        public string course_category { get; set; }

        public int page { get; set; }
    }
}
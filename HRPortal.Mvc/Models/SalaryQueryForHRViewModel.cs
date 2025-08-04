using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
    public class SalaryQueryForHRViewModel
    {
        //public string SalaryYM { get; set; }
        //public string FormNo { get; set; }

        public List<SalaryQueryViewModel> SalaryFormListData { get; set; }

        public IEnumerable<SelectListItem> DepartmentListData { get; set; }
        public string SelectedDepartment { get; set; }
        public string Role { get; set; }
        public IEnumerable<SelectListItem> EmployeeListData { get; set; }
        public string SelectedEmployee { get; set; }

        public string SelectedEmployeeName { get; set; }

        public bool flagShowSalaryData { get; set; }
        //public bool ChkHireId { get; set; }

        //public IEnumerable<SelectListItem> YearListData { get; set; }
        public string SelectedYear { get; set; }
        public IEnumerable<SelectListItem> StatuslistDataData { get; set; }
        public string SelectedStatuslistData { get; set; }

        public string SelectedEmployeeEnglishName { get; set; }
        //public List<HolidaySummaryDetail> DetailDatas { get; set; }
        //public List<PersonalSummary> PersonalDetailDatas { get; set; }
    }
}

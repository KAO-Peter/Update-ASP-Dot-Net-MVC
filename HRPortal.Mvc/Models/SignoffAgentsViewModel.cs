using HRPortal.DBEntities;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
     public class SignoffAgentsViewModel
    {
        public IEnumerable<SelectListItem> DepartmentListData { get; set; }
        public IEnumerable<SelectListItem> DepartmentListData1 { get; set; }
        public IEnumerable<SelectListItem> DepartmentListData2 { get; set; }
        public IEnumerable<SelectListItem> DepartmentListData3 { get; set; }
        public string SelectedDepartment { get; set; }
        public string SelectedDepartment1 { get; set; }
        public string SelectedDepartment2 { get; set; }
        public string SelectedDepartment3 { get; set; }
        public string Role { get; set; }
        public IEnumerable<SelectListItem> EmployeeListData { get; set; }
        public IEnumerable<SelectListItem> EmployeeListData1 { get; set; }
        public IEnumerable<SelectListItem> EmployeeListData2 { get; set; }
        public IEnumerable<SelectListItem> EmployeeListData3 { get; set; }
        public string SelectedEmployee { get; set; }
        public string SelectedEmployee1 { get; set; }
        public string SelectedEmployee2 { get; set; }
        public string SelectedEmployee3 { get; set; }
        public SignoffAgents FormData { get; set; }

        public string SourceType { get; set; }
    }
}

using HRPortal.DBEntities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Models
{
   public class EmployeeInfoManageViewModel
    {
        public IEnumerable<SelectListItem> signDepartmentLists { get; set; }
        public IEnumerable<SelectListItem> companyLists { get; set; }
        public IEnumerable<SelectListItem> roleLists { get; set; }
        public IEnumerable<SelectListItem> _employeeList { get; set; }
      
        public Task<List<SelectListItem>> EmergencyRelation { get; set; }

        public Employee employeeData { get; set; }

        public string updatePasswordHash { get; set; }
        public string DesignatedPerson { get; set; }
        public IEnumerable<SelectListItem> signDepartmentLists1 { get; set; }

        public System.Guid EffectiveDateID { get; set; }
        public System.Guid SignDepartmentID1 { get; set; }
        public System.DateTime EffectiveDate { get; set; }
    }

    public class ShowEmployeeIndex
    {
       
        public System.Guid ID { get; set; }


        public string EmployeeNO { get; set; }

      
        public string EmployeeName { get; set; }
       
        public System.Guid CompanyID { get; set; }
        public string CompanyName { get; set; }
   
        public System.Guid DepartmentID { get; set; }
        public string DepartmentName { get; set; }
       
        public System.Guid SignDepartmentID { get; set; }
        public string SignDepartmentName { get; set; }


        public System.DateTime CreatedTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.Models
{
    public class SignFlowAssignDeptViewModel
    {
        public Guid ID { get; set; }
        public string FormType { get; set; }
        public string FormTypeName { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string DeptID { get; set; }
        public string DeptName { get; set; }
        public string DesignID { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
    }
}

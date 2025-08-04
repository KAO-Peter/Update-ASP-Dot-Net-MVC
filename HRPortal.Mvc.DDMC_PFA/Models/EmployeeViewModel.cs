using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class EmployeeViewModel
    {
        public IList<EmpData> Data { get; set; }
        public int PageCount { get; set; }
        public int DataCount { get; set; }
    }

    public class EmpData
    {
        public System.Guid EmployeeID { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public DateTime? LeaveDate { get; set; }
    }
}

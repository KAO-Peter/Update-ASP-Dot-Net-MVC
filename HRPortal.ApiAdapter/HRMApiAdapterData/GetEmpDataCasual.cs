using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class GetEmpDataCasual
    {
        public IList<EmpDataCasual> EmployeeData { get; set; }
        public int PageCount { get; set; }
        public int DataCount { get; set; }
    }

    public class EmpDataCasual
    {
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string IDNumber { get; set; }
        public DateTime? LeaveDate { get; set; }        
    }
}

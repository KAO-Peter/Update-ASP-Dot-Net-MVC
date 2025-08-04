using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualAttendPayrollChecklist
    {
        public List<CasualAttendPayrollChecklistData> Data { get; set; }
        public string CompanyFullName { get; set; }
        public string CompanyNameEn { get; set; }
    }

    public class CasualAttendPayrollChecklistData
    {
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string SalaryStartTime { get; set; }
        public string SalaryEndTime { get; set; }
        public string ActualStartTime { get; set; }
        public string ActualEndTime { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmployeeBasicData
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public DateTime AssumeDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public string LeaveReason { get; set; }
        public string JobTitleCode { get; set; }
        public string JobTitleName { get; set; }
        public string CostCenterStd_ID { get; set; }
        public string EmpChangeDate { get; set; }
        public string Grade { get; set; }
    }
}

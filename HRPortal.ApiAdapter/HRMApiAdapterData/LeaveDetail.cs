using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class LeaveDetail
    {
        public string DeptName { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string AbsentName { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal AbsentAmount { get; set; }
        public string Unit { get; set; }
    }
}

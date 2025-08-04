using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class PostLeaveData
    {
        public string FormNo { get; set; }
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string LeaveCode { get; set; }
        public string AbsentReason { get; set; }
        public string AgentID { get; set; }
    }
}

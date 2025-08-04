using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class PortalFormDetailData
    {
        public string LeaveForm_ID { get; set; }
        public string FormNo { get; set; }
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string AbsentUnit { get; set; }
        public double WorkHours { get; set; }
        public double TotalAmount { get; set; }
        public List<EmpAbsentCheckDetail> EmpAbsentCheckDetailList { get; set; }
        public string CreateEmpID { get; set; }
    }
}

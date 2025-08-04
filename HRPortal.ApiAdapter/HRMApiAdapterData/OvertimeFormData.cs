using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class OvertimeFormData
    {
        public int ID { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Summary { get; set; }
        public string FormNo { get; set; }
        public string AbsentCode { get; set; }
        public DateTime CreateTime { get; set; }
        public float OvertimeAmount { get; set; }
        public string HaveDinning { get; set; }
        public bool Compensation { get; set; }
        public string OverTimeReasonCode { get; set; }
        public string OverTimeReason { get; set; }
        public int CutTime { get; set; }
        public string DeptNameEN { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class AbsentDataForBambooHRQueryObj
    {
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public List<string> AbsentCodeList { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AbsentDataForBambooHR
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public int? EmpAbsent_ID { get; set; }
        public int? EmpWorkAdjust_ID { get; set; }
        public bool isAdjust { get; set; } //是否為追補假單
        public string FormNo { get; set; }
        public int Absent_ID { get; set; }
        public string AbsentCode { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public double AbsentAmount { get; set; }
        public string AbsentReason { get; set; }
        public string AgentEmpID { get; set; }
    }
}

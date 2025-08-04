using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class PostOverTimeData
    {
        public string FormNo { get; set; }
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool CheckFlag { get; set; }
        public bool ToRest { get; set; }
        public string HaveDinning { get; set; }
        public bool EatFee { get; set; }
        public string Reason { get; set; }
        public string Overtime_ID { get; set; }
        public int CutTime { get; set; }
        public bool isCheckDutyCard { get; set; }
        public bool ToPay { get; set; }
    }
}

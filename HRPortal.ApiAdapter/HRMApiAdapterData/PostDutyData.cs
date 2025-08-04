using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class PostDutyData
    {
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public DateTime ClockInTime { get; set; }
        public string ClockInWay { get; set; }
        public string ClockInReason { get; set; }
        public string Reason { get; set; }
        public string FormNo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualCycleResult
    {
        public int CasualCycle_ID { get; set; }
        public string CasualFormNo { get; set; }
        public DateTime CasualPayDate { get; set; }
        public DateTime CountBeginDate { get; set; }
        public DateTime CountEndDate { get; set; }
        public string LockFlag { get; set; }
        public int CompanyID { get; set; }
    }
}

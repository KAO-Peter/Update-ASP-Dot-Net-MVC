using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CheckOverTimeResponse : RequestResult
    {
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal OvertimeHours { get; set; }
    }
}

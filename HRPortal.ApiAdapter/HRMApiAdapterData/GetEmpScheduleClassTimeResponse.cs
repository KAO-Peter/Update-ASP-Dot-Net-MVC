using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class GetEmpScheduleClassTimeResponse : RequestResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

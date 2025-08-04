using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmpScheduleTimeK9Data
    {
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public DateTime ExcuteDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Double WorkHours { get; set; }
    }
}

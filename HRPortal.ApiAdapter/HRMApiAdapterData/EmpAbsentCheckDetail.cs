using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmpAbsentCheckDetail
    {
        public int Absent_ID { get; set; }
        public string Absent_Code { get; set; }
        public System.DateTime BeginTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public double AbsentAmount { get; set; }
        public Nullable<double> WorkHours { get; set; }
    }
}

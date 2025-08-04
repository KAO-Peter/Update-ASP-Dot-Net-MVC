using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmpDutyCardK9Result : RequestResult
    {
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public DateTime ExcuteDate { get; set; }
        public string CardNo { get; set; }
        public bool CardKeep { get; set; }
    }
}

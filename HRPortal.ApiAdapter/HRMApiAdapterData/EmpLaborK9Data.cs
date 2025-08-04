using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmpLaborK9Data
    {
        public string CompanyCode { get; set; }
        public string EmpID { get; set; }
        public DateTime ExcuteDate { get; set; }
        public double SumSalary { get; set; }
        public string UserID { get; set; }
        public Nullable<System.Guid> Portal_CasualFormID { get; set; }
        public string Status { get; set; }
    }
}

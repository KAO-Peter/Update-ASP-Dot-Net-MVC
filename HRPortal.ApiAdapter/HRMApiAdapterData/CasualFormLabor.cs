using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualFormLabor
    {
        public List<EmpCasualFormLabor> EmpLaborData { get; set; }
    }

    public class EmpCasualFormLabor
    {
        public Guid? Portal_CasualFormID { get; set; }
        public string EmpID { get; set; }
        public Nullable<double> PersonalLabor { get; set; }
        public Nullable<double> CompanyLabor { get; set; }
    }
}

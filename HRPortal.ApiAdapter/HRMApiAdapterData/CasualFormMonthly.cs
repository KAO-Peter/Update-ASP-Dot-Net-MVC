using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualFormMonthly
    {
        public List<CasualFormMonthlyData> CasualFormMonthlyData { get; set; }
    }

    public class CasualFormMonthlyData
    {
        public string Name { get; set; }
        public double Cross { get; set; }
        public double Hours { get; set; }
        public double EmployeerPremium { get; set; }
        public double EmployeePremium { get; set; }
        public double NET { get; set; }
    }
}

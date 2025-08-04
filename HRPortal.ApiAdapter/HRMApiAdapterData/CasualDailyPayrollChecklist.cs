using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualDailyPayrollChecklist
    {
        public List<CasualDailyPayrollChecklistDetailData> DetailData { get; set; }
        public List<CasualDailyPayrollChecklistTotalData> TotalData { get; set; }
        public string ReportType { get; set; }
        public string CompanyFullName { get; set; }
        public string CompanyNameEn { get; set; }
    }

    public class CasualDailyPayrollChecklistDetailData
    {
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string IDNumber { get; set; }
        public string dept { get; set; }
        public string Birthday { get; set; }
        public string ExcuteDate { get; set; }
        public string SalaryUnit { get; set; }
        public double Wage { get; set; }
        public double WorkHours { get; set; }
        public double Allowance { get; set; }
        public double Salary { get; set; }
        public double CompanyLabor { get; set; }
        public double PersonalLabor { get; set; }
        public double CompanyRetirement { get; set; }
        public string Note { get; set; }
    }

    public class CasualDailyPayrollChecklistTotalData
    {
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string IDNumber { get; set; }
        public string Birthday { get; set; }
        public double WorkHours { get; set; }
        public double Allowance { get; set; }
        public double Salary { get; set; }
        public double CompanyLabor { get; set; }
        public double PersonalLabor { get; set; }
        public double CompanyRetirement { get; set; }
    }
}

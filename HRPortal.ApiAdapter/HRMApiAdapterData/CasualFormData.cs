using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CasualFormData
    {
        public string CompanyCode { get; set; }
        public int EmpData_ID { get; set; }
        public int Dept_ID { get; set; }
        public int Cost_ID { get; set; }
        public DateTime ExcuteDate { get; set; }
        public Nullable<int> Class_ID { get; set; }
        public string DefaultStartTime { get; set; }
        public string DefaultEndTime { get; set; }
        public double DefaultWorkHours { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Nullable<double> DiningHours { get; set; }
        public Nullable<double> WorkHours { get; set; }
        public string SalaryUnit { get; set; }
        public Nullable<double> Salary { get; set; }
        public Nullable<double> Allowance { get; set; }
        public string CardNo { get; set; }
        public Nullable<bool> CardKeep { get; set; }
        public Nullable<bool> CashFlag { get; set; }
        public Nullable<double> DaySalary { get; set; }
        public Nullable<System.Guid> Portal_CasualFormID { get; set; }
        public string UserID { get; set; } 
    }
}

using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.SignFlow.Model
{
    public class LeaveFormData : FormData
    {
        //public string AbsentCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal LeaveAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public bool IsAbroad { get; set; }
        public string AgentNo { get; set; }
        public decimal WorkHours { get; set; }
        public string AbsentUnit { get; set; }

        public LeaveFormData(LeaveForm form)
        {
            this.FormNumber = form.FormNo;
            this.FormType = SignLists.FormType.Leave.ToString();
            this.EmployeeNo = form.Employee.EmployeeNO;
            this.DeptCode = form.Department.DepartmentCode;
            this.CompanyCode = form.Company.CompanyCode;
            this.AbsentCode = form.AbsentCode;
            this.StartTime = form.StartTime;
            this.EndTime = form.EndTime;
            this.LeaveAmount = form.LeaveAmount;
            this.AfterAmount = form.AfterAmount;
            this.IsAbroad = form.IsAbroad;
            if (form.Agent != null)
            {
                this.AgentNo = form.Agent.EmployeeNO;
            }
            this.WorkHours = form.WorkHours.HasValue ? form.WorkHours.Value : Convert.ToDecimal(0.0);
            //20180815 Daniel 增加假別單位欄位
            this.AbsentUnit = form.AbsentUnit;
        }

        public LeaveFormData()
        {
 
        }
    }
}

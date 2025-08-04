using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.SignFlow.Model
{
    public class OverTimeFormData : FormData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal OverTimeAmount { get; set; }
        public string OverTimeReasonCode { get; set; }
        public short CompensationWay { get; set; }
        public bool HaveDinning { get; set; }

        public OverTimeFormData(OverTimeForm form)
        {
            this.FormNumber = form.FormNo;
            this.FormType = SignLists.FormType.OverTime.ToString();
            this.EmployeeNo = form.Employee.EmployeeNO;
            this.DeptCode = form.Department.DepartmentCode;
            this.CompanyCode = form.Company.CompanyCode;
            this.StartTime = form.StartTime;
            this.EndTime = form.EndTime;
            this.OverTimeAmount = form.OverTimeAmount;
            this.OverTimeReasonCode = form.OverTimeReasonCode;
            this.CompensationWay = form.CompensationWay;
            this.HaveDinning = form.HaveDinning;
        }
    }
}

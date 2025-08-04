using HRPortal.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.SignFlow.Model
{
    public class PatchCardFormData : FormData
    {
        public DateTime Time { get; set; }
        public int Type { get; set; }
        public int ReasonType { get; set; }

        public PatchCardFormData(PatchCardForm form)
        {
            this.FormNumber = form.FormNo;
            this.FormType = SignLists.FormType.PatchCard.ToString();
            this.EmployeeNo = form.Employee.EmployeeNO;
            this.DeptCode = form.Department.DepartmentCode;
            this.CompanyCode = form.Company.CompanyCode;
            this.Time = form.PatchCardTime;
            this.Type = form.Type;
            this.ReasonType = form.ReasonType;
        }
    }
}

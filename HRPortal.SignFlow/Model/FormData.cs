using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.Model
{
    public class FormData : IFormData
    {
        public string FormNumber { get; set; }
        public string FormType { get; set; }
        public string CompanyCode { get; set; }
        public string DeptCode { get; set; }
        public string EmployeeNo { get; set; }
        public string AbsentCode { get; set; }

        public string CompanyID { get { return CompanyCode; } }
        public string DeptID { get { return DeptCode; } }
        //public string Negotiator { get; set; }
        public string CUser { get { return EmployeeNo; } }

    }
}

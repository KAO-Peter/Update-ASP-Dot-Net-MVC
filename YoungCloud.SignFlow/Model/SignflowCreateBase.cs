using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoungCloud.SignFlow.Model
{
    public class SignflowCreateBase : YoungCloud.SignFlow.Model.ISignflowCreateBase
    {
        public string FormNumber { get; set; }
        public string  FormType { get; set; }
        public string CompanyID { get; set; }
        public string DeptID { get; set; }
        public string Negotiator { get; set; }
        public string CUser { get; set; }
    }
}

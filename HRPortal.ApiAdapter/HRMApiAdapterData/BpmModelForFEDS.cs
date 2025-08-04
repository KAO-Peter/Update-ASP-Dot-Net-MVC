using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class BpmModelForFEDS
    {
        public string EmployeeNo { get; set; }
        public string CompanyCode { get; set; }
        public string SignDepartmentCode { get; set; }
        public string SignDepartmentName { get; set; }
        public string ParentDepartmentList { get; set; }
    }
}

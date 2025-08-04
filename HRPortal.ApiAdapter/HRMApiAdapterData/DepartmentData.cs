using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class DepartmentData
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DeptNameEN { get; set; }
        public string ManagerEmpID { get; set; }
        public string ParentDeptCode { get; set; }
        public DateTime EndDate { get; set; }
    }
}

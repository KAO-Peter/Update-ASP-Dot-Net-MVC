using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class DepartmentLeaveSummaryPostData
    {
        public string DeptCodee { get; set; }
        public string CompanyCode { get; set; }
        public List<string> DeptCode { get; set; }
        public string empId { get; set; }
        public DateTime beginDate { get; set; }
        public DateTime endDate { get; set; }
        public List<string> empIds { get; set; }
    }
}

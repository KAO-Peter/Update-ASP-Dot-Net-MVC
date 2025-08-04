using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class DepartmentSalarySummaryPostData
    {
        public string CompanyCode { get; set; }
        public List<string> DeptCode { get; set; }
        public string FormNo { get; set; }
    }
}

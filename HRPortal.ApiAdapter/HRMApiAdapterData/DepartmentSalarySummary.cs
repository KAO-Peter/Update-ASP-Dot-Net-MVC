using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class DepartmentSalarySummary
    {
        public List<DepartmentSalarySummaryItem> DeptSum { get; set; }
    }

    public class DepartmentSalarySummaryItem
    {
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public List<DepartmentMemberForSalarySummary> DeptMember { get;set;}
    }

    public class DepartmentMemberForSalarySummary
    {
        public string Name { get; set; }
        public string EmpID { get; set; }
        public string JobTitle { get; set; }
        public string Grade { get; set; }
        public decimal TaxableSum { get; set; }
        public decimal NoTaxSum { get; set; }
        public decimal Withholding { get; set; }
        public decimal OtherWithholding { get; set; }
        public decimal StandardAmount { get; set; }
        public decimal RealAmount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class EmployeeSalaryInfoFEDS
    {
        public string Name { get; set; }
        public string EmpID { get; set; }
        public string Sex { get; set; }
        public bool Married { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string JobFunctionName { get; set; }
        public string JobTitle { get; set; }
        public string GradeName { get; set; }
        public string PositionName { get; set; }
        public double SalaryBasis { get; set; }
        public int BringNumber { get; set; }
        public string BankCode { get; set; }
        public string BankAccount { get; set; }
        public decimal RetirementAllocate { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal StandardAmount { get; set; }
        public decimal RealAmount { get; set; }
        public string BankBranchFISC { get; set; } //銀行分支機構
        public string BankName { get; set; } //銀行名稱
        public string SalaryFormNo { get; set; }  //薪資批號
        public string SalaryYM { get; set; } //薪資年月
        public int HealthNumber { get; set; } //健保眷口
        public double WithholdingItemsAmount { get; set; } //代扣合計
    }
}

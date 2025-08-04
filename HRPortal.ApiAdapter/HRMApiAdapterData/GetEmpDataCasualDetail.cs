using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class GetEmpDataCasualDetail
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "部門")]
        public string DeptCode { get; set; }
        public string DeptName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        [Display(Name = "員工編號")]
        public string EmpID { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        [Display(Name = "員工姓名")]
        public string EmpName { get; set; }

        [Required]
        [StringLength(12, ErrorMessage = "欄位長度不得大於 12 個字元")]
        [Display(Name = "身份證號")]
        public string IDNumber { get; set; }

        [Required]
        [Display(Name = "性別")]
        public string Sex { get; set; }

        [Required]
        [Display(Name = "費用別")]
        public string CostCode { get; set; }
        public string CostName { get; set; }

        [Required]
        [Display(Name = "到職日期")]
        public DateTime AssumeDate { get; set; }
        public DateTime? LeaveDate { get; set; }

        [Required]
        [Display(Name = "出生日期")]
        public DateTime Birthday { get; set; }

        [Required]
        [Display(Name = "是否己婚")]
        public bool Married { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "欄位長度不得大於 150 個字元")]
        [Display(Name = "戶籍地址")]
        public string RegisterAddress { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "欄位長度不得大於 150 個字元")]
        [Display(Name = "連絡地址")]
        public string Address { get; set; }

        [StringLength(20, ErrorMessage = "欄位長度不得大於 20 個字元")]
        [Display(Name = "連絡電話")]
        public string Tel { get; set; }

        [StringLength(20, ErrorMessage = "欄位長度不得大於 20 個字元")]
        [Display(Name = "行動電話")]
        public string Mobile { get; set; }

        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        [Display(Name = "緊急聯絡人")]
        public string EmergencyName { get; set; }

        [StringLength(25, ErrorMessage = "欄位長度不得大於 25 個字元")]
        [Display(Name = "緊急聯絡人電話")]
        public string EmergencyPhone { get; set; }

        [Display(Name = "同上")]
        public bool AsAbove { get; set; }

        [StringLength(3, ErrorMessage = "欄位長度不得大於 3 個字元")]
        [Display(Name = "銀行分行碼")]
        public string BranchNo { get; set; }

        [StringLength(20, ErrorMessage = "欄位長度不得大於 20 個字元")]
        [Display(Name = "銀行帳號")]
        public string Account { get; set; }

        //20170507 Start 幫Dsinfo補上註解
        [Display(Name = "身份別")]
        public bool NationType { get; set; }

        [Display(Name = "國籍")]
        public string NationCode { get; set; }

        [Display(Name = "外籍身分註記")]
        public string ForeignType { get; set; }
        //20170507 End
    }
}

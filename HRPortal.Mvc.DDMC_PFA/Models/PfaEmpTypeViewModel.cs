using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaEmpTypeViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public Guid CompanyID { get; set; }

        /// <summary>
        /// 部門代碼
        /// </summary>
        public string PfaEmpTypeCode { get; set; }

        /// <summary>
        /// 部門名稱
        /// </summary>
        public string PfaEmpTypeName { get; set; }

        /// <summary>
        /// 適用職稱
        /// </summary>
        public string JobTitleName { get; set; }

        /// <summary>
        /// 適用職稱List
        /// </summary>
        public List<Guid> JobTitleIDList { get; set; }
    }

    public class CreatePfaEmpType
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入績效考核組織ID")]
        public Guid ID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        [Required(ErrorMessage = "請輸入公司別")]
        public System.Guid CompanyID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入身分類別代碼")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string PfaEmpTypeCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入身分類別名稱")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string PfaEmpTypeName { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }
    }

    public class SelJobTitleFunction
    {
        [Required(ErrorMessage = "請輸入身分類別ID")]
        public Guid PfaEmpTypeID { get; set; }
        public string PfaEmpTypeCode { get; set; }
        public string PfaEmpTypeName { get; set; }
        public List<JobTitleItem> JobTitleItems { get; set; }
    }

    public class JobTitleItem
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Chk { get; set; }
        public bool Setucompleted { get; set; }
    }
}
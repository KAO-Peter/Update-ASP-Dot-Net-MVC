using System;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核員工工作績效
    /// </summary>
    public class PfaEmpIndicatorViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// 績效考核員工ID
        /// </summary>
        public Guid? PfaCycleEmpID { get; set; }

        /// <summary>
        /// 指標代碼
        /// </summary>
        public string PfaIndicatorCode { get; set; }

        /// <summary>
        /// 指標名稱
        /// </summary>
        public string PfaIndicatorName { get; set; }

        /// <summary>
        /// 定義
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 權重
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 工作績效程度
        /// </summary>
        public string PfaIndicatorDesc { get; set; }

        /// <summary>
        /// 自評工作績效分數
        /// </summary>
        public double? SelfIndicator { get; set; }

        /// <summary>
        /// 主管工作績效分數
        /// </summary>
        public double? ManagerIndicator { get; set; }
    } 
}

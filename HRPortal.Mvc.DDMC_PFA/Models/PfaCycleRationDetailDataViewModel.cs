using System;
using System.Collections.Generic;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核人數配比
    /// </summary>
    public class PfaCycleRationDetailDataViewModel
    {
        /// <summary>
        /// 績效考核批號ID
        /// </summary>
        public Guid? PfaCycleID { get; set; }

        /// <summary>
        /// 部門組織ID
        /// </summary>
        public Guid? PfaOrgID { get; set; }

        /// <summary>
        /// 考核部門ID
        /// </summary>
        public Guid? PfaDeptID { get; set; }

        /// <summary>
        /// 考核部門名稱
        /// </summary>
        public string PfaDeptName{ get; set; }

        /// <summary>
        /// 行類型
        /// </summary>
        public string RowType { get; set; }

        /// <summary>
        /// 行類型
        /// </summary>
        public string RowTypeName { get; set; }

        /// <summary>
        /// 組織應配比人數
        /// </summary>
        public int? OrgTotal { get; set; }

        /// <summary>
        /// 人數
        /// </summary>
        public List<RationNumberViewModel> Number { get; set; }

        /// <summary>
        /// 是否配比正確
        /// </summary>
        public string IsRation { get; set; }
    }

    /// <summary>
    /// 人數
    /// </summary>
    public class RationNumberViewModel 
    {
        /// <summary>
        /// 績效等第ID
        /// </summary>
        public Guid? PfaPerformanceID { get; set; }

        /// <summary>
        /// 人數
        /// </summary>
        public int Number { get; set; }
    }
}

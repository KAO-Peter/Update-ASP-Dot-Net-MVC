using System;
using System.Collections.Generic;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核能力行為指標
    /// </summary>
    public class PfaEmpAbilityViewModel
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
        /// 勝任能力代碼
        /// </summary>
        public string PfaAbilityCode { get; set; }

        /// <summary>
        /// 勝任能力名稱
        /// </summary>
        public string PfaAbilityName { get; set; }

        /// <summary>
        /// 定義
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 總分
        /// </summary>
        public int? TotalScore { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 主管勝任能力分數
        /// </summary>
        public double? ManagerAbility { get; set; }

        /// <summary>
        /// 能力行為指標
        /// </summary>
        public List<PfaEmpAbilityDetailViewModel> PfaEmpAbilityDetail { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 能力行為指標
    /// </summary>
    public class PfaEmpAbilityDetailViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// 績效考核員工勝任能力ID
        /// </summary>
        public Guid? PfaEmpAbilityID { get; set; }

        /// <summary>
        /// 關鍵行為指標
        /// </summary>
        public string PfaAbilityKey { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        public int? UpperLimit { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        public int? LowerLimit { get; set; }

        /// <summary>
        /// 分數區間(依上下限)
        /// </summary>
        public List<int> ScoreInterval { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 勝任能力程度
        /// </summary>
        public double? AbilityScore { get; set; }
    }
}

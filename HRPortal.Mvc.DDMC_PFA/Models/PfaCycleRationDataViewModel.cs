using System.Collections.Generic;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 評等人數配比資料
    /// </summary>
    public class PfaCycleRationDataViewModel
    {
        /// <summary>
        /// 分數評等區間
        /// </summary>
        public List<PfaPerformanceViewModel> PfaPerformance { get; set; }

        /// <summary>
        /// 初核部門筆數
        /// </summary>
        public int FirstDept { get; set; }

        /// <summary>
        /// 評等人數配比
        /// </summary>
        public List<PfaCycleRationDetailDataViewModel> PfaCycleRationDetail  { get; set; }
    }
}

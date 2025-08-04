namespace HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData
{
    public class PfaPerformanceData
    {
        /// <summary>
        /// 公司代碼
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 評等區間代碼
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 評等區間名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Performance
        /// </summary>
        public string Performance { get; set; }

        /// <summary>
        /// band
        /// </summary>
        public string band { get; set; }

        /// <summary>
        /// 佔比(%)
        /// </summary>
        public int? Rates { get; set; }

        /// <summary>
        /// Multiplier
        /// </summary>
        public decimal? Multiplier { get; set; }

        /// <summary>
        /// 分數起
        /// </summary>
        public decimal? ScoresStart { get; set; }

        /// <summary>
        /// 分數迄
        /// </summary>
        public decimal? ScoresEnd { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsUsed { get; set; }
    }
}

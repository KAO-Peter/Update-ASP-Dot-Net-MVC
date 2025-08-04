using System;

namespace HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData
{
    public class GetPfaCycleResponse
    {
        /// <summary>
        /// 公司別代碼
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 考績年度
        /// </summary>
        public string PfaYear { get; set; }

        /// <summary>
        /// 在職基礎日期
        /// </summary>
        public DateTime? ServeBasedate { get; set; }

        /// <summary>
        /// 出勤日期起
        /// </summary>
        public DateTime? DutyBeginDate { get; set; }

        /// <summary>
        /// 出勤日期迄
        /// </summary>
        public DateTime? DutyEndDate { get; set; }

        /// <summary>
        /// 到職基礎日期
        /// </summary>
        public DateTime? AssumeBasedate { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        public string Desription { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }
    }
}

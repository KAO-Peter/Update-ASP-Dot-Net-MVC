using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核員工簽核
    /// </summary>
    public class PfaCycleEmpSignViewModel : PfaCycleEmpViewModel
    {
        /// <summary>
        /// 簽核ID
        /// </summary>
        public Guid? PfaSignProcessID { get; set; }

        /// <summary>
        /// 績效考核員工ID
        /// </summary>
        public Guid? PfaCycleEmpID { get; set; }

        /// <summary>
        /// 績效考核員工
        /// </summary>
        public string PfaCycleEmpName { get; set; }

        /// <summary>
        /// 簽核狀態
        /// </summary>
        public string SignStatus { get; set; }

        /// <summary>
        /// 簽核狀態名稱
        /// </summary>
        public string StrSignStatus { get; set; }

        /// <summary>
        /// 簽核意見
        /// </summary>
        public string Assessment { get; set; }

        /// <summary>
        /// 可編輯初核
        /// </summary>
        public bool EditFirstEvaluation { get; set; }

        /// <summary>
        /// 績效考核員工工作績效
        /// </summary>
        public List<PfaEmpIndicatorViewModel> PfaEmpIndicator { get; set; }

        /// <summary>
        /// 績效考核員工訓練紀錄
        /// </summary>
        public List<PfaEmpTrainingViewModel> PfaEmpTraining { get; set; }

        /// <summary>
        /// 附件筆數
        /// </summary>
        public int SignUploadCount { get; set; }

        /// <summary>
        /// 績效考核簽核附件資料
        /// </summary>
        public PfaSignUploadDataViewModel PfaSignUploadData { get; set; }

        /// <summary>
        /// 績效考核能力行為指標
        /// </summary>
        public List<PfaEmpAbilityViewModel> PfaEmpAbility { get; set; }

        /// <summary>
        /// 與過去表現比較
        /// </summary>
        public List<SelectListItem> PastPerformanceData { get; set; }

        /// <summary>
        /// 評價目前工作
        /// </summary>
        public List<SelectListItem> NowPerformanceData { get; set; }

        /// <summary>
        /// 未來發展評斷
        /// </summary>
        public List<SelectListItem> DevelopmentData { get; set; }

        /// <summary>
        /// 評等人數配比資料
        /// </summary>
        public PfaCycleRationDataViewModel PfaCycleRationData { get; set; }

        /// <summary>
        /// 績效考核簽核流程
        /// </summary>
        public List<PfaSignRecordViewModel> PfaSignRecord { get; set; }
    }

    public enum SortOption
    {
        /// <summary>
        /// 初核分數降冪
        /// </summary>
        FirstH,
        /// <summary>
        /// 初核分數升冪
        /// </summary>
        FirstL,
        /// <summary>
        /// 複核分數降冪
        /// </summary>
        SecondH,
        /// <summary>
        /// 複核分數升冪
        /// </summary>
        SecondL,
        /// <summary>
        /// 核決分數降冪
        /// </summary>
        ThirdH,
        /// <summary>
        /// 核決分數升冪
        /// </summary>
        ThirdL
    }
}

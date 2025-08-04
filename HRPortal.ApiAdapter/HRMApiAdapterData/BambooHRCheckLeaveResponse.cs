using System;
using System.Collections.Generic;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class BambooHRCheckLeaveResponse : RequestResult
    {
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime BeginTime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 請假時數(小時)
        /// </summary>
        public double InputHours { get; set; }
        /// <summary>
        /// 備註資訊
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 假別名稱
        /// </summary>
        public string AbsentName { get; set; }
        /// <summary>
        /// 工作時數
        /// </summary>
        public double WorkHours { get; set; }
        /// <summary>
        /// 假別單位(BambooHR輸入的)
        /// </summary>
        public string InputUnit { get; set; }
        /// <summary>
        /// 假別單位(HRM系統設定的)
        /// </summary>
        public string SystemUnit { get; set; }
        /// <summary>
        /// 檢查結果狀態
        /// </summary>
        public BambooHRCheckLeaveStatus CheckStatus { get; set; }
        /// <summary>
        /// 系統算出的請假時數(小時)
        /// </summary>
        public double SystemAbsentHours { get; set; }
        /// <summary>
        /// 假別剩餘時數(小時)
        /// </summary>
        public double RemainingHours { get; set; }
        /// <summary>
        /// 假別最小時數
        /// </summary>
        public double MinNumber { get; set; }
        /// <summary>
        /// 請假檢核拆單明細
        /// </summary>
        public List<EmpAbsentCheckDetail> EmpAbsentCheckDetailList { get; set; }
        /// <summary>
        /// 總核假數量(依據假別單位，單位是日，這邊就是幾日)
        /// </summary>
        public double CanUseCount { get; set; }
        /// <summary>
        /// 假別已用數量(依據假別單位，單位是日，這邊就是幾日)
        /// </summary>
        public double UsedCount { get; set; }
    }

    public enum BambooHRCheckLeaveStatus
    {
        CheckPassed, //檢查時數沒問題
        QuotaExceeded, //超過可用時數，剩餘時數不足
        LessThanMinNumber, //不到最小時數
        AmountNotMatched, //輸入時數與系統計算時數不符
        OtherError //其他錯誤
    }

}

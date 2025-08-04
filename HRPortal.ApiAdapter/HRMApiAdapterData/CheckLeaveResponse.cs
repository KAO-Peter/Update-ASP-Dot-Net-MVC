using System;
using System.Collections.Generic;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class CheckLeaveResponse : RequestResult
    {
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal AbsentAmount { get; set; }
        public string Note { get; set; }
        public string AbsentName { get; set; }
        public double WorkHours { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string AbsentEngName { get; set; } //假別英文名稱
        public decimal AbsentQuota { get; set; } //總核假時數(單位已經轉換過，不一定是小時)
    }

    //20170627 Daniel 增加請假檢核明細回傳結果
    public class CheckLeaveDetailResponse : CheckLeaveResponse
    {
        /*
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal AbsentAmount { get; set; }
        public string Note { get; set; }
        public string AbsentName { get; set; }
        public double WorkHours { get; set; }
        public string Type { get; set; }
        */
        public List<EmpAbsentCheckDetail> EmpAbsentCheckDetailList { get; set; }
    }
}

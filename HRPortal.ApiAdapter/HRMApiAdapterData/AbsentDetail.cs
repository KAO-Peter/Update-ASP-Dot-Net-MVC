using System;
using System.Collections.Generic;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class AbsentDetail
    {
        public string getLanguageCookie { get; set; }
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string AbsentNameEn { get; set; }
        public decimal AnnualLeaveHours { get; set; }
        public decimal ApprovedHours { get; set; }
        public decimal LeaveHours { get; set; }
        public decimal UseAmount { get; set; }
        public decimal AllLeaveHours { get; set; }
        public decimal CanUseCount { get; set; }
        public decimal OverdueHours { get; set; }
        //20170614 Daniel，增加未來生效的假別時數欄位
        public decimal FutureTotal { get; set; }
        public decimal FutureUsed { get; set; }
        //單位 H/D
        public string Unit { get; set; }
        public bool CanUse { get; set; }
        public string Remark { get; set; }
        public bool CanOverdraft { get; set; }
        public bool YearCalWayFlag { get; set; }
        public DateTime? BeginDateMin { get; set; }
        public DateTime? EndDateMax { get; set; }
    }

    public class AbsentDetailAll
    {
        public List<AbsentDetail> AbsentDetail_Overdue { get; set; }
        public List<AbsentDetail> AbsentDetail_Now { get; set; }
        public List<AbsentDetail> AbsentDetail_Future { get; set; }

    }
}

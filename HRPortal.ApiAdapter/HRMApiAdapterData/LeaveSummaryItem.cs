using System.Collections.Generic;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class LeaveSummaryItem
    {
        public string Name { get; set; }
        public string AbsentCode { get; set; }
        public List<decimal> EachMonth { get; set; }
        public double BalancedCount { get; set; } //平衡時數

        //20180808 Daniel 增加核假統計狀況，包括假別的一些屬性
        /// <summary>
        /// 目前核假統計狀況
        /// </summary>
        public EmpHolidayUsedData HolidayData { get; set; }
    }

    public class EmpLeaveSummaryItem
    {
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public string EmpName { get; set; }
        public string EmpNo { get; set; }
        public List<LeaveSummaryItem> SummaryDetail { get; set; }
    }

    public class DeptEmpLeaveSummaryItem
    {
        public List<LeaveSummaryItem> DetailDatas { get; set; }
        public List<EmpLeaveSummaryItem> PersonalDetailDatas { get; set; }
    }

    public class AbsentSummaryQueryRes
    {
        public string CompanyCode { get; set; }
        public List<string> EmpIDList { get; set; }
        public int Year { get; set; }
        public string StatusData { get; set; }
        //2018/10/30 Neo 增加假別欄位
        //public string AbsentCode { get; set; }
        public List<string> AbsentCodeList { get; set; }
        //20190523 Daniel 增加傳入語系欄位
        public string LanguageCookie { get; set; }
        public List<string> AbsentCode { get; set; }

    }

    public class EmpHolidayUsedData
    {
        public double AbsentUsedCount { get; set; } //請假時數
        public double AdjustUsedCount { get; set; } //追補扣時數
        public double PendingCount { get; set; } //簽核中時數
        public double BalancedCount { get; set; } //平衡時數
        public double TotalUsedCount { get; set; } //總已用時數
        public double TotalCanUseCount { get; set; } //總核假時數
        public double TotalRemainingCount { get; set; } //總剩餘時數
        public double CanUseCountInRange { get; set; } //日期區間內的核假時數 (總核假時數-日期區間以外已用掉的時數-平衡時數)
        public double UsedCountInRange { get; set; } //日期區間內的已用時數 (總已用時數-日期區間以外已用掉的時數-平衡時數)
        public double RemainingCountInRange { get; set; } //日期區間內的剩餘時數 (日期區間內的核假時數-日期區間內的已用時數)
        public string DefarredAmount { get; set; } //遞延時數

    }
}

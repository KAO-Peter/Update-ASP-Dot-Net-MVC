using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{

    public class EmpHolidayTimeSpanQuotaData
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string EmpNameEN { get; set; }
        public int EmpHoliday_ID { get; set; }
        public int Absent_ID { get; set; }
        public string AbsentCode { get; set; }
        public string AbsentName { get; set; }
        public string AbsentNameEN { get; set; }
        public string AbsentUnit { get; set; }
        public double CanUseCount { get; set; } //原核假時數，注意這是依照假別單位給的
        public double CanUseCountHours { get; set; }
        public double BeginningBalance { get; set; } //查詢開始日期當時的可用時數
        public double EndingBalance { get; set; } //查詢區間後的剩餘可用時數
        public double HoursUsed { get; set; }
        public double HoursScheduled { get; set; }
        public double HoursPending { get; set; }
        public double HoursBefore { get; set; } //查詢區間前的總已用時數
        public DateTime HolidayBeginDate { get; set; }
        public DateTime HolidayEndDate { get; set; }
        public DateTime QueryBeginDate { get; set; } 
        public DateTime QueryEndDate { get; set; }
        public List<GeneralAbsentData> InRangeUsedList { get; set; } //查詢區間內的已使用假單資料(當日以前的)
        public List<GeneralAbsentData> InRangeScheduledList { get; set; } //查詢區間內的尚未發生假單資料(當日或當日之後的)
        public List<GeneralAbsentData> BeforeRangeList { get; set; } //查詢區間之前的假單資料
        public string InRangeUsedString { get; set; } 
        public string InRangeScheduledString { get; set; } 
        public string BeforeRangeString { get; set; } 
        // public List<GeneralAbsentData> PendingList { get; set; }
        public string PendingString { get; set; }
    }

    public class GeneralAbsentData
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public int? EmpAbsent_ID { get; set; }
        public int? EmpWorkAdjust_ID { get; set; }
        public bool isAdjust { get; set; } //是否為追補假單
        public string FormNo { get; set; }
        public int Absent_ID { get; set; }
        public string AbsentCode { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public double AbsentAmount { get; set; }
        public string AbsentReason { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class QueryEmpHolidayTimeSpanQuotaObj
    {
        public string CompanyCode { get; set; }
        public string DepartmentCode { get; set; }
        public DateTime QueryBeginDate { get; set; }
        public DateTime QueryEndDate { get; set; }
        public List<string> EmpIDList { get; set; }
        public List<string> AbsentCodeList { get; set; }
        public bool IncludingPendingData { get; set; }
    }

    public class EmpHolidayTimeSpanQuotaViewModel
    {
        public List<EmpHolidayTimeSpanQuotaData> QuotaData { get; set; }
        public QueryEmpHolidayTimeSpanQuotaObj QueryCondition { get; set; }
    }

}

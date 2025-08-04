using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class AbsentItem
    {
        public string Name { get; set; }
        public decimal ThisMonth { get; set; }
        public decimal ThisYear { get; set; }
    }

    public class EmployeeAbsentItems
    {
        public string AttendanceFirstDays { get; set; }
        public string AttendanceLastDays { get; set; }
        public List<AbsentItem> Leaves { get; set; }
        public List<AbsentItem> Absents { get; set; }
        public List<AbsentItem> OverTimes { get; set; }
    }

    ///<summary>
    ///員工假別資料
    ///</summary>
    public class EmployeeAbsentItemsDDMC
    {
        /// <summary>
        ///假別陣列
        ///</summary>
        public LeavesDataFEDS Leaves { get; set; }
        /// <summary>
        ///出缺勤陣列
        ///</summary>
        public AbsentsDataFEDS Absents { get; set; }
        /// <summary>
        ///加班陣列
        ///</summary>
        public OverTimesDataFEDS OverTimes { get; set; }
        /// <summary>
        ///年度特休陣列
        ///</summary>
        public AnnualLeavesDataFEDS AnnualLeaves { get; set; }

        //考勤區間(開始)
        public DateTime AttendanceFirstDays { get; set; }

        //考勤區間(結束)
        public DateTime AttendanceLastDays { get; set; }
    }

    ///<summary>
    ///員工假別名稱、當月、年累物件
    ///</summary>
    public class AbsentNameYearAndMonthDDMC
    {
        /// <summary>
        /// 假別名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 當月
        /// </summary>
        public double ThisMonth { get; set; }
        /// <summary>
        /// 年累
        /// </summary>
        public double ThisYear { get; set; }
    }

    ///<summary>
    ///出缺勤項目
    ///</summary>
    public class AbsentsDataDDMC
    {
        /// <summary>
        /// 遲到次數
        /// </summary>
        public AbsentNameYearAndMonthDDMC LateCount { get; set; }
        /// <summary>
        /// 早退次數
        /// </summary>
        public AbsentNameYearAndMonthDDMC LeaveEarlyCount { get; set; }
        /// <summary>
        /// 曠職次數
        /// </summary>
        public AbsentNameYearAndMonthDDMC AbsenteeismCount { get; set; }
    }

    ///<summary>
    ///假別項目
    ///</summary>
    public class LeavesDataDDMC
    {
        ///<summary>
        ///特休
        ///</summary>
        public AbsentNameYearAndMonthDDMC AnnualLeaveHours { get; set; }
        ///<summary>
        ///補休
        ///</summary>
        public AbsentNameYearAndMonthDDMC CompensatedDayOffHours { get; set; }
        ///<summary>
        ///補休未休
        ///</summary>
        public AbsentNameYearAndMonthDDMC CompensatedDayOffNoUseHours { get; set; }
        ///<summary>
        ///事假
        ///</summary>
        public AbsentNameYearAndMonthDDMC PersonalLeaveHours { get; set; }
        ///<summary>
        ///半薪病假
        ///</summary>
        public AbsentNameYearAndMonthDDMC HalfPaySickSickLeaveHours { get; set; }
        ///<summary>
        ///無薪病假
        ///</summary>
        public AbsentNameYearAndMonthDDMC NoPaySickSickLeaveHours { get; set; }
        ///<summary>
        ///產假
        ///</summary>
        public AbsentNameYearAndMonthDDMC MaternityLeaveHours { get; set; }
        ///<summary>
        ///陪產假
        ///</summary>
        public AbsentNameYearAndMonthDDMC PaternityLeaveHours { get; set; }
        ///<summary>
        ///其它假
        ///</summary>
        public AbsentNameYearAndMonthDDMC OtherLeaveHours { get; set; }
        ///<summary>
        ///其它假(無薪)
        ///</summary>
        public AbsentNameYearAndMonthDDMC OtherLeaveHoursNoPay { get; set; }
    }

    ///<summary>
    ///加班項目
    ///</summary>
    public class OverTimesDataDDMC
    {
        ///<summary>
        ///申請加班費時數
        ///</summary>
        public AbsentNameYearAndMonthDDMC OverTimePayHours { get; set; }
        ///<summary>
        ///申請加班補休時數
        ///</summary>
        public AbsentNameYearAndMonthDDMC OverTimeRestHours { get; set; }
        ///<summary>
        ///加班時數合計
        ///</summary>
        public double OverTimeTotalHours { get; set; }
        ///<summary>
        ///平日(上班日)加班時數合計
        ///</summary>
        public double OverTimeRestHours_D { get; set; }
        ///<summary>
        ///例假日加班時數合計
        ///</summary>
        public double OverTimeRestHours_H { get; set; }
        ///<summary>
        ///休息日加班時數合計
        ///</summary>
        public double OverTimeRestHours_O { get; set; }
        ///<summary>
        ///國定假日加班時數合計
        ///</summary>
        public double OverTimeRestHours_P { get; set; }
        ///<summary>
        ///免出勤日加班時數合計
        ///</summary>
        public double OverTimeRestHours_N { get; set; }
    }

    /// <summary>
    ///年度特休項目
    ///</summary>
    public class AnnualLeavesDataDDMC
    {
        /// <summary>
        ///前年度展延剩餘特休時數
        ///</summary>
        public double BeforeLastYearRemainderAnnualLeaveHours { get; set; }
        /// <summary>
        ///本年度新增可休特休時數
        ///</summary>
        public double CurrentYearAnnualLeaveHours { get; set; }
        /// <summary>
        ///本年度已休特休時數
        ///</summary>
        public double CurrentYearUsedAnnualLeaveHours { get; set; }
        /// <summary>
        ///累計未特休時數
        ///</summary>
        public double CurrentYearRemainderAnnualLeaveHours { get; set; }
        /// <summary>
        ///可休期限
        ///</summary>
        public string AnnualLeaveEndDate { get; set; }
        /// <summary>
        ///本年度預支特休累積時數
        ///</summary>
        public double CurrentYearAdvanceAnnualLeaveHours { get; set; }
    }
}

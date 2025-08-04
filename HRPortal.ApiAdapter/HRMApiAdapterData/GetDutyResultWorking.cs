using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class GetDutyResultWorking
    {
        public IList<DutyResultWorking> Data { get; set; }
        public int PageCount { get; set; }
        public int DataCount { get; set; }
    }

    public class DutyResultWorking
    {
        public int? EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string EmpData_Name { get; set; }
        public string ExcuteDate { get; set; }
        public string WeekDay { get; set; }
        public string Class { get; set; }
        public string InTimeCard { get; set; }
        public string OutTimeCard { get; set; }
        public string CardHour { get; set; }
        public string ClassStartTime { get; set; }
        public string ClassEndTime { get; set; }
        public string WorkHours { get; set; }
        public string ResultType { get; set; }
        public string ResultTypeName { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string DutyAmount { get; set; }
        public string Overtime { get; set; }
        public string Compensatory { get; set; }
        public string AdvanceCompensatory { get; set; }
        public string CanUseCompensatory { get; set; }
        public string UseAdvanceCompensatory { get; set; }
        public bool OvertimeCheckFlag { get; set; }
        public bool AbsentCheckFlag { get; set; }
    }

    public class GetDutyResultWorking2
    {
        public IList<DutyResultWorking2> Data { get; set; }
        public int PageCount { get; set; }
        public int DataCount { get; set; }
    }

    public class DutyResultWorking2
    {
        public int? EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string EmpData_Name { get; set; }
        public string ExcuteDate { get; set; }
        public string WeekDay { get; set; }
        public string Class { get; set; }
        public string InTimeCard { get; set; }
        public string OutTimeCard { get; set; }
        public string CardHour { get; set; }
        public string ClassStartTime { get; set; }
        public string ClassEndTime { get; set; }
        public string WorkHours { get; set; }
        public string ResultType { get; set; }
        public string ResultTypeName { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string DutyAmount { get; set; }
        public string Overtime { get; set; }
        public string Compensatory { get; set; }
        public string AdvanceCompensatory { get; set; }
        public string CanUseCompensatory { get; set; }
        public string UseAdvanceCompensatory { get; set; }
        public bool OvertimeCheckFlag { get; set; }
        public bool AbsentCheckFlag { get; set; }

        //201707 Daniel 增加四段用餐時間、彈性時間與加班補休已請時段
        //四段用餐時間
        public string DiningBegin1 { get; set; }
        public string DiningEnd1 { get; set; }
        public string DiningBegin2 { get; set; }
        public string DiningEnd2 { get; set; }
        public string DiningBegin3 { get; set; }
        public string DiningEnd3 { get; set; }
        public string DiningBegin4 { get; set; }
        public string DiningEnd4 { get; set; }

        //彈性時間
        public double? FlexibleTime { get; set; }

        //已請加班補休時段
        public DateTime? OverTimeBeginTime { get; set; }
        public DateTime? OverTimeEndTime { get; set; }
        public DateTime? CompensatoryBeginTime { get; set; }
        public DateTime? CompensatoryEndTime { get; set; }
        public DateTime? AdvanceCompensatoryBeginTime { get; set; }
        public DateTime? AdvanceCompensatoryEndTime { get; set; }
    }
}


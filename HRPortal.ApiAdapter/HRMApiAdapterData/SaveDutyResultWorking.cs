using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class SaveDutyResultWorking
    {
        public string CompanyCode { get; set; }

        //20170712 Daniel 加上執行動作的人的EmpID
        public string EmpID { get; set; }
        
        public DutyResultWork[] Data { get; set; }
    }

    public class DutyResultWork
    {
        public string EmpID { get; set; }
        public string ExcuteDate { get; set; }
        public string ClassStartTime { get; set; }
        public string ClassEndTime { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public double? Overtime { get; set; }
        public double? Compensatory { get; set; }
        public double? AdvanceCompensatory { get; set; }
    }

    public class SaveDutyResultWorking2
    {
        public string CompanyCode { get; set; }

        //20170712 Daniel 加上執行動作的人的EmpID
        public string EmpID { get; set; }

        public DutyResultWork2[] Data { get; set; }
    }

    public class DutyResultWork2
    {
        public string EmpID { get; set; }
        public string ExcuteDate { get; set; }
        public string ClassStartTime { get; set; }
        public string ClassEndTime { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public double? Overtime { get; set; }
        public double? Compensatory { get; set; }
        public double? AdvanceCompensatory { get; set; }

        //201707 Daniel 增加四段用餐時間、彈性時間
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
    }
}

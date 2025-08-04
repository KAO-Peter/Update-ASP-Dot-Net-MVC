using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    public class AbsentCountAll
    {
        //假別時數單位
        public string Unit;

        //逾期
        public AbsentCountData Absent_Overdue;

        //目前
        public AbsentCountData Absent_Now;

        //未來
        public AbsentCountData Absent_Future;
    }

    public class AbsentCountData //請假資訊(含核假及已休)
    {
        public AbsentCountQuota Quota;
        public AbsentCountUsed Used;
        public DateTime? BeginDateMax;
        public DateTime? EndDateMax;
    }

    public class AbsentCountQuota //核假資訊
    {
        public double Sum; //核假總時數=以下三個欄位加總
        public double EmpHoliday; //核假檔核假時數
        public double EmpOvertime_To_Rest; //未結薪加班轉補休時數 (非補休假此欄位為0)
        public double EmpWorkAdjust_OT_To_Rest; //追補扣未結薪加班轉補休時數 (非補休假此欄位為0)
    }

    public class AbsentCountUsed //已休資訊
    {
        public double Sum; //已休總時數=以下五個欄位加總
        public double EmpHoliday; //核假檔已休已月結時數
        public double Signing; //Portal簽核中請假時數
        public double EmpAbsent; //未結薪請假檔請假時數
        public double EmpWorkAdjust; //追補扣請假時數
        public double UsedPrevious; //前期若已休>核假，表示有部分請假時數需算進現在的已休
    }
}

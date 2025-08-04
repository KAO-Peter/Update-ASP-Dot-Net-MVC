using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{

    public class DutyScheduleSummary
    {
        /// <summary>
        /// 公司代碼
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 部門代碼
        /// </summary>
        public string DeptCode { get; set; }
        /// <summary>
        /// 部門名稱
        /// </summary>
        public string DeptName { get; set; }
        /// <summary>
        /// 部門人員陣列
        /// </summary>
        public IList<DeptMemberList> DeptMember { get; set; }
    }
    public class DeptMemberList
    {
        /// <summary>
        /// 員工編號
        /// </summary>
        public string EmpID { get; set; }
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmpName { get; set; }
        /// <summary>
        /// 員工出勤人員陣列
        /// </summary>
        public IList<DutySetGetList> DutySet { get; set; }

    }
    public class DutySetGetList
    {
        /// <summary>
        /// 排班日期
        /// </summary>
        public string ExcuteDate { get; set; }
        /// <summary>
        /// 開始時間
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 工作時數
        /// </summary>
        public double WorkHours { get; set; }
        /// <summary>
        /// 刷進時間
        /// </summary>
        public string InTime { get; set; }
        /// <summary>
        /// 刷出時間
        /// </summary>
        public string OutTime { get; set; }

        /// <summary>
        /// 缺勤時數
        /// </summary>
        public double DutyAmount { get; set; }
        /// <summary>
        /// 遲到分鐘
        /// </summary>
        public int LateTime { get; set; }
        /// <summary>
        /// 早退分鐘
        /// </summary>
        public int EarlyLeaveTime { get; set; }

        /// <summary>
        /// 星期
        /// </summary>
        public string Weekday { get; set; }
        /// <summary>
        /// 出勤時數
        /// </summary>
        public string DutyTime { get; set; }


        ///// <summary>
        ///// 請假時數
        ///// </summary>
        //public double AbsentAmount { get; set; }
        ///// <summary>
        ///// 假別名稱
        ///// </summary>
        //public string AbsentName { get; set; }
        ///// <summary>
        ///// 假別開始時間
        ///// </summary>
        //public string AbsentBeginTime { get; set; }
        ///// <summary>
        ///// 假別結束時間
        ///// </summary>
        //public string AbsentEndTime { get; set; }


        /// <summary>
        /// 請假加班資料
        /// </summary>
        public IList<AbsentOvertimeInfo> AbsentOvertimeInfo { get; set; }
    }



    public class AbsentOvertimeInfo
    {
        /// <summary>
        /// 時數
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// 請假加班名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime BeginTime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime EndTime { get; set; }
    }

}

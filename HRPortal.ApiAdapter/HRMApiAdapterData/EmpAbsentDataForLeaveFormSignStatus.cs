using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{

    public class QueryLeaveFormSignStatusObj
    {
        public string CompanyCode { get; set; }
        public string DepartmentCode { get; set; }
        public DateTime QueryBeginDate { get; set; }
        public DateTime QueryEndDate { get; set; }
        public List<string> EmpIDList { get; set; }
        public List<string> AbsentCodeList { get; set; }
        public bool isUsed { get; set; } //true:已發生的假單， false:尚未發生的假單
    }

    public class QueryAbsentDataForLeaveFormSignStatusObj
    {
        public List<string> ExcludeFormNoList { get; set; }
        public QueryLeaveFormSignStatusObj QueryCondition { get; set; }
    }

    public class LeaveFormSignStatusData
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string EmpNameEN { get; set; }
        public string FormNo { get; set; }
        public string AbsentCode { get; set; }
        public AbsentType Absent { get; set; }
        public string AbsentUnit { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ApproveTime { get; set; }
        public double LeaveHours { get; set; }
        public string Reason { get; set; }
    }

    public class LeaveFormSignStatusBackendData
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string EmpName { get; set; }
        public string EmpNameEN { get; set; }
        public string FormNo { get; set; }
        public bool isAdjust { get; set; }
        public string AbsentCode { get; set; }
        public string AbsentName { get; set; }
        public string AbsentNameEN { get; set; }
        public string AbsentUnit { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public double AbsentAmount { get; set; }
        public string AbsentReason { get; set; }
    }

    public class QueryLeaveFormSignStatusViewModel
    {
        public List<LeaveFormSignStatusData> DetailData { get; set; }
        public QueryLeaveFormSignStatusObj QueryCondition { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.Services;
using HRPortal.DBEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using System.Globalization;
using HRPortal.Services.Models;

namespace HRPortal.Services.Models.BambooHR
{
    public class LeaveFormGeneral //請假單通用物件，Portal與後台的假單都需先轉成此物件
    {
        public string FormNo { get; set; }              //表單編號
        public Guid? PortalLeaveFormID { get; set; }    //Portal假單ID
        public int? EmpData_ID { get; set; }            //後台的EmpData_ID，正常應該一定有資料
        public string EmpID { get; set; }               //員工編號
        public Guid? PortalEmployeeID { get; set; }     //Portal的EmployeeID
        public DateTime StartTime { get; set; }         //請假開始時間
        public DateTime EndTime { get; set; }           //請假結束時間
        public string AbsentCode { get; set; }          //假別代碼
        public decimal LeaveAmount { get; set; }        //請假時數，後台是AbsentAmount，但因Portal假單的AbsentAmount意義不同，所以統一用Portal的LeaveAmount
        public string AmountUnit { get; set; }          //請假時數的單位，注意這邊是註記上面的LeaveAmount的實際單位，不是看假別的單位
        public TimeOffRequestStatus LeaveFormStatus { get; set; }    //假單狀態，原則上只需要簽核中(requested)，與已核准(approved)
        public string LeaveReason { get; set; }              //請假原因/說明
        public bool forHistory { get; set; }            //是否為歷史假單，新增假單會需要判斷這個欄位
        public int? EmpAbsent_ID { get; set; }          //後台假單ID，歷史假單才會有這資料
        public int? EmpWorkAdjust_ID { get; set; }      //後台追補假單ID，歷史假單才會有這資料
        public string AgentEmpID { get; set; }          //代理人工號
        public string CompanyCode { get; set; }         //公司代碼
    }

    public class ImportHistoryResult
    {
        public string Result { get; set; }
        public List<LeaveFormGeneral> HistoryFormList { get; set; }
    }

    public class TimeOffRequest
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string BambooEmployeeID { get; set; }
        public TimeOffRequestStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string AbsentCode { get; set; }
        public decimal LeaveAmount { get; set; }
        public List<TimeOffRequest_Note> Notes { get; set; }
    }

    public class TimeOffRequest_RequestBody
    {
        public string status { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string timeOffTypeId { get; set; }
        public string amount { get; set; }
        public List<TimeOffRequest_Note> notes { get; set; }
    }

    public class TimeOffRequest_Note
    {
        public string from { get; set; }
        public string note { get; set; }
    }

    public class ChangeTimeOffStatus
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string BambooTimeOffRequestID { get; set; }
        public TimeOffRequestStatus Status { get; set; }
        public string Note { get; set; }
    }

    public class ChangeTimeOffStatus_RequestBody
    {
        public string status { get; set; }
        public string note { get; set; }
    }

    public class TimeOffRequestQuery
    {
        public int EmpData_ID { get; set; }
        public string EmpID { get; set; }
        public string BambooEmployeeID { get; set; }
        public DateTime BeginDate { get; set; } //BambooHR只接受查詢日期
        public DateTime EndDate { get; set; }
        public string AbsentCode { get; set; }
        public string TimeOffType { get; set; }
        public TimeOffRequestStatus? Status { get; set; }
        //public string BambooTimeOffTypeID { get; set; }
    }

    public class TimeOffRequestQueryResult
    {
        public string id { get; set; }
        public string employeeId { get; set; }
        public TimeOffRequestQueryResult_Status status { get; set; }
        public string name { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string created { get; set; }
        public TimeOffRequestQueryResult_Type type { get; set; }
        public TimeOffRequestQueryResult_Amount amount { get; set; }
        public TimeOffRequestQueryResult_Actions actions { get; set; }

        [JsonConverter(typeof(JsonNetFix.IgnoreUnexpectedArraysConverter<TimeOffRequestQueryResult_Notes>))]
        public TimeOffRequestQueryResult_Notes notes { get; set; }

    }

    public class TimeOffRequestMappedInfo
    {
        public TimeOffRequestQueryResult QueryResult { get; set; }
        public Employee Employee { get; set; }
        public BambooHREmployeeMapping EmployeeMapping { get; set; }
        public BambooHRTimeOffType TimeOffTypeMapping { get; set; }
        public BambooHRLeaveFormRecord BambooHRLeaveFormRecord { get; set; }
    }

    public class TimeOffCheckResult
    {
        public bool Success { get; set; }
        public DateTime ConfirmedStartTime { get; set; }
        public DateTime ConfirmedEndTime { get; set; }
        public TimeOffRequestQueryResult checkItem { get; set; }
        public TimeOffRequestMappedInfo MappedInfo { get; set; }
        public string CheckResult { get; set; }
        public BambooHRCheckLeaveResponse HRMCheckResponse { get; set; }

    }

    public class TimeOffRequestQueryResult_Status
    {
        public string lastChanged { get; set; }
        public string lastChangedByUserId { get; set; }
        public string status { get; set; }
    }

    public class TimeOffRequestQueryResult_Type
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
    }

    public class TimeOffRequestQueryResult_Amount
    {
        public string unit { get; set; }
        public string amount { get; set; }

    }

    public class TimeOffRequestQueryResult_Actions
    {
        public string view { get; set; }
        public string edit { get; set; }
        public string cancel { get; set; }
        public string approve { get; set; }
        public string deny { get; set; }
        public string bypass { get; set; }

    }

    public class TimeOffRequestQueryResult_Notes
    {
        public string employee { get; set; }
        public string manager { get; set; }

    }

    public class EmployeeDirectoryResult
    {
        public List<EmployeeDirectoryResult_Field> fields { get; set; }
        public List<EmployeeDirectoryResult_Employee> employees { get; set; }

    }

    public class EmployeeDirectoryResult_Field
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
    }

    public class EmployeeDirectoryResult_Employee
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string preferredName { get; set; }
        public string gender { get; set; }
        public string jobTitle { get; set; }
        public string workPhone { get; set; }
        public string mobilePhone { get; set; }
        public string workEmail { get; set; }
        public string department { get; set; }
        public string location { get; set; }
        public string division { get; set; }
        public string linkedIn { get; set; }
        public string workPhoneExtension { get; set; }
        public string photoUploaded { get; set; }
        public string photoUrl { get; set; }
        public string canUploadPhoto { get; set; }

    }
    
    /*
    public class QueryUsersResult
    {
        public Dictionary<string, QueryUsersResult_User> Users { get; set; }
    }
    */

    public class QueryUsersResult_User
    {
        public string id { get; set; }
        public string employeeId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string lastLogin { get; set; }
    }


    public class QueryEmployeeResult
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string employeeNumber { get; set; }
        public string workEmail { get; set; }
    }

    public class AdjustTimeOffBalance
    {
        public int EmpData_ID { get; set; } 
        public string EmpID { get; set; }
        public string BambooEmployeeID { get; set; }
        public DateTime BeginDate { get; set; }
        public string AbsentCode { get; set; }
        public decimal LeaveAmount { get; set; }
        public string Note { get; set; }
    }

    public class AdjustTimeOffBalance_RequestBody
    {
        public string date { get; set; }
        public string timeOffTypeId { get; set; }
        public decimal amount { get; set; }
        public string note { get; set; }
    }

    public class InputTimeinNote
    {
        public bool IsFormatCorrect { get; set; }
        public string InputNote { get; set; }
        public string ExtractedTimeStr { get; set; }
        public DateTime ExtractedStartTime { get; set; }
        public DateTime ExtractedEndTime { get; set; }
    }

    public class TimePeriod
    {
        public TimePair StartTime { get; set; }
        public TimePair EndTime { get; set; }
        public string ExtractedStartTimeString { get; set; }
        public string ExtractedEndTimeString { get; set; }
    }

    public class TimePair
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
    }

    public class LogInfo
    {
        public Guid? UserID { get; set; }
        public string UserIP { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }

    public class BackgroundServiceResult
    {
        public bool Success { get; set; }
        public string Result { get; set; }
        public string Remark { get; set; } //目前拿來放檢查所有假單的回傳結果
        //public Exception Exception { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum TimeOffRequestStatus
    {
        requested,
        approved,
        denied,
        canceled,
        superceded,
        pending //這個狀態BambooHR API內沒有，是系統這邊自己用的，中間主管簽核後會是此狀態

    }

    public enum BambooHRIntegrationStatus
    {
        Normal,
        Absent_QuotaExceeded,
        Absent_LessThanMinNumber,
        Absent_AmountNotMatched,
        Absent_InvalidTimeFormat,
        Absent_EndTimeEarlierThanStartTime,
        Absent_LeaveFormOverlapped,
        Absent_EmployeeNotFound,
        Absent_OtherError,
        SyncStatus_Unexpected
    }

    public enum BambooHRMailNotificationCategory
    {
        Applicant, //申請人
        Manager, //簽核主管
        Agent, //代理人
        BambooHR_Admin, //BambooHR管理者
        HR_Admin, //HR管理者
        Tester //測試人員
    }

    public enum BambooHRMailRcptType //收件人類型
    {
        NoRcpt = 0, //沒有收件人，不須寄Mail
        Applicant = 1, //收件人為申請人
        Manager = 2, //收件人為主管(目前是指下一關主管)
        Custom = 3, //自訂收件人
        Agent = 4 //代理人
    }

}

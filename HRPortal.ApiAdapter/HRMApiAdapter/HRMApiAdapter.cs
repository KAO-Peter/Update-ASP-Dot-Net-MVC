using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        private static string _hostUri
        {
            get
            {
                if(GetHostUri != null)
                {
                    return GetHostUri();
                }
                else
                {
                    throw new Exception("No delegate function for get host uri.");
                }
            }
        }
        //20161125增加HireNo判斷員工類別
        private static string _getDutyScheduleSummaryUri = "api/EmpSchedule/GetSchedule?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&StatusData={StatusData}&BeginTime={BeginTime}&EndTime={EndTime}&HireNo={HireNo}";
        private static string _getRelativesUri = "api/employee/GetEmergencyRelationList?CompanyCode={CompanyCode}";
        private static string _getCompanyUri = "api/Employee/GetCompanyList";
        
        //20190524 Daniel 取得單一公司資訊
        private static string _getCompanyByCodeUri = "api/Employee/GetCompanyByCode?CompanyCode={CompanyCode}";
        
        private static string _getDepartmentUri = "api/Employee/GetDeptList?CompanyCode={CompanyCode}";
        private static string _getEmplyeeUri = "api/Employee/Get?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}";
        //2019/03/05 Neo 增加取得員工基本資料
        private static string _getEmplyeeBasicDataUri = "api/Employee/GetBasicData?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}";
        private static string _getCasualListUri = "api/EmpDataCasual/GetCasualList?CompanyCode={CompanyCode}&EmpID={EmpID}&IDNumber={IDNumber}&Page={Page}&PageSize={PageSize}";
        private static string _getEmpDataCasualUri = "api/EmpDataCasual/GetEmpDataCasual?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&EmpName={EmpName}&IDNumber={IDNumber}&Page={Page}&PageSize={PageSize}";
        private static string _getEmpDataCasualDetailUri = "api/EmpDataCasual/GetEmpDataCasualDetail?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpNo={EmpNo}";
        private static string _getOverTimeReasonUri = "api/OverTime/GetReason?CompanyCode={CompanyCode}";
        private static string _getLeaveListUri = "api/Leave/GetLeaveList?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&BeginTime={BeginTime}&EndTime={EndTime}";
        private static string _getAbsentUri = "api/Leave/GetAbsent?CompanyCode={CompanyCode}&EmpID={EmpID}&ExcuteDate={ExcuteDate}&type={type}";
        private static string _getAbsent2Uri = "api/Leave/GetAbsent2?CompanyCode={CompanyCode}&EmpID={EmpID}&ExcuteDate={ExcuteDate}&type={type}";

        private static string _getAllAbsentUri = "api/Leave/GetAllAbsent?CompanyCode={CompanyCode}";
        private static string _getAbsentFormDataUri = "api/Leave/GetAbsentFormData?CompanyCode={CompanyCode}&EmpID={EmpID}&DeptCode={DeptCode}&StatusData={StatusData}&StartTime={StartTime}&EndTime={EndTime}&LanguageCookie={LanguageCookie}";
        private static string _getOverTimeFormDataUri = "api/OverTime/GetOverTimeFormData?CompanyCode={CompanyCode}&EmpID={EmpID}&DeptCode={DeptCode}&StatusData={StatusData}&StartTime={StartTime}&EndTime={EndTime}&LanguageCookie={LanguageCookie}";
        private static string _getAbsentFormDetailUri = "api/Leave/GetSpecificAbsentFormData?CompanyCode={CompanyCode}&FormNo={FormNo}";
        private static string _getOverTimeFormDetailUri = "api/OverTime/GetSpecificOvertimeFormData?CompanyCode={CompanyCode}&FormNo={FormNo}";
        private static string _getEmpHolidayDataUri = "api/Leave/GetEmpHolidayData?CompanyCode={CompanyCode}&EmpID={EmpID}&AbsentID={AbsentID}";
        private static string _getUnLockRestLeaveInfoUri = "api/OverTime/GetUnLockRestLeaveInfo?CompanyCode={CompanyCode}&EmpID={EmpID}";
        private static string _getOverTimeTotalUri = "api/OverTime/GetEmpOverTimeTotal?EmpID={EmpID}&StartTime={StartTime}&EndTime={EndTime}";//

        //20201124 Daniel 增加查詢特定工號假別特定區間內的假單，BambooHR歷史資料匯入用
        private static string _getAbsentDataForBambooHR = "api/Leave/GetAbsentDataForBambooHR";

        //20210513 Daniel 增加查詢特定區間內核假可用與已使用的假別時數，主要是BambooHR比對資料用
        private static string _getEmpHolidayTimeSpanQuota = "api/Leave/GetEmpHolidayTimeSpanQuota";

        //20210525 Daniel 增加查詢後台假單與追補假單資料的API，主要是提供BambooHR報表使用
        private static string _getEmpAbsentDataForLeaveFormSignStatus = "api/Leave/GetEmpAbsentDataForLeaveFormSignStatus";

        private static string _getSalaryFormNo = "api/Salary/GetEmpSalaryFormNo?CompanyCode={CompanyCode}&EmpID={EmpID}&SalaryBeginMonth={SalaryBeginMonth}&SalaryEndMonth={SalaryEndMonth}";
        //20170614 Daniel，遠百增加薪資批號只能於發薪日前一天上午9:00之後才發放查詢
        private static string _getSalaryFormNoFEDS = "api/Salary/GetEmpSalaryFormNoFEDS?CompanyCode={CompanyCode}&EmpID={EmpID}&SalaryBeginMonth={SalaryBeginMonth}&SalaryEndMonth={SalaryEndMonth}";
        
        //20180313 Daniel 增加查詢所有薪資批號功能，包括計薪中的
        private static string _getSalaryFormNoAllStatus = "api/Salary/GetEmpSalaryFormNoAllStatus?CompanyCode={CompanyCode}&EmpID={EmpID}&SalaryBeginMonth={SalaryBeginMonth}&SalaryEndMonth={SalaryEndMonth}";
    
        private static string _getDeptSalaryFormNo = "api/Salary/GetDeptSalaryFormNo?CompanyCode={CompanyCode}&SalaryBeginMonth={SalaryBeginMonth}&SalaryEndMonth={SalaryEndMonth}";
        private static string _getEmployeeSalaryInfo = "api/Salary/GetEmployeeItems?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
        private static string _getEmployeeSalaryDetail = "api/Salary/GetSalaryItems?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
        private static string _getEmployeeAbsentDetail = "api/Salary/GetAbsentItems?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
        private static string _getDepartmentSalarySummary = "api/Salary/GetDeptSumItem";
        //20161121增加HireID判斷員工類別
        private static string _getLeaveSummary = "api/Leave/GetDeptEmpLeaveSumItem?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&StatusData={StatusData}&HireId={HireId}&Year={Year}";
        //private static string _getLeaveSummary2 = "api/Leave/GetDeptEmpLeaveSumItem2?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&StatusData={StatusData}&HireId={HireId}&Year={Year}";
        private static string _getLeaveSummary2 = "api/Leave/GetDeptEmpLeaveSumItem2";
        private static string _getClassUri = "api/Class/GetClassList?CompanyCode={CompanyCode}";
        private static string _getClassByIDUri = "api/Class/GetClassByID?CompanyCode={CompanyCode}&ClassID={ClassID}";
        private static string _getEmpDutyCardUri = "api/EmpDutyCard/GetEmpDutyCard?CompanyCode={CompanyCode}&EmpID={EmpID}&ExcuteDate={ExcuteDate}";
        private static string _getAllEmpDutyCardUri = "api/EmpDutyCard/GetAllEmpDutyCard?CompanyCode={CompanyCode}&ExcuteDate={ExcuteDate}";
        private static string _getDutyResultWorkingUri = "api/DutyResult/GetDutyResultWorking?CompanyCode={CompanyCode}&DeptCode={DeptCode}&DateStart={DateStart}&DateEnd={DateEnd}&EmpID={EmpID}&EmpName={EmpName}&Page={Page}&PageSize={PageSize}";

        private static string _getCasualFormLaborUri = "api/CasualForm/GetCasualFormLabor?CompanyCode={CompanyCode}&DeptCode={DeptCode}&ExcuteDate={ExcuteDate}&TimeType={TimeType}&EmpID={EmpID}&EmpName={EmpName}";
        private static string _getEmpScheduleClassTimeUri = "api/EmpSchedule/GetEmpScheduleClassTime?CompanyCode={CompanyCode}&EmpID={EmpID}&ExcuteDate={ExcuteDate}"; //20160822 加入查詢班表時間
        private static string _getEmpScheduleClassTimeByStartEndTimeUrl = "api/EmpSchedule/GetEmpScheduleClassTimeByStartEndTime?CompanyCode={CompanyCode}&EmpID={EmpID}&StartTime={StartTime}&EndTime={EndTime}"; //20161114 加入查詢班表時間API 2

        //20170430-20170507 Start 幫Dsinfo補上註解
        private static string _getCasualFormMonthlyUri = "api/CasualForm/GetCasualFormMonthly?CompanyCode={CompanyCode}&DeptCode={DeptCode}&Grouping={Grouping}&Date1={Date1}&Date2={Date2}&MonthlyOrCash={MonthlyOrCash}";
        private static string _getCasualDailyPayrollChecklistUri = "api/CasualForm/GetCasualDailyPayrollChecklist?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&Date1={Date1}&Date2={Date2}&MonthlyOrCash={MonthlyOrCash}&ReportType={ReportType}";
        private static string _getCasualAttendPayrollChecklistUri = "api/CasualForm/GetCasualAttendPayrollChecklist?CompanyCode={CompanyCode}&DeptCode={DeptCode}&EmpID={EmpID}&EmpName={EmpName}&Date1={Date1}&Date2={Date2}&Status={Status}";
        private static string _getNationUri = "api/EmpDataCasual/GetAllNation?CompanyCode={CompanyCode}";
        //20170430-20170507 End

        /*遠百客製*/
        //薪資明細表
        private static string _getEmployeeSalaryInfoFEDS = "api/Salary/GetEmployeeItems_FEDS?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
        private static string _getEmployeeSalaryDetailFEDS = "api/Salary/GetSalaryItems_FEDS?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
        private static string _getEmployeeAbsentDetailFEDS = "api/Salary/GetAbsentItems_FEDS?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
        //BPM
        private static string _postEmpSignDeptImportUri = "api/EmpSignDept/Import";

        //勞健保繳費證明
        private static string _getEmployeeInsyreInfoUri = "api/EmpInsure/GetEmployeeItems?CompanyCode={CompanyCode}&EmpID={EmpID}&InsureYear={InsureYear}";
        private static string _getInsurePersonalMoneyInfoUri = "api/EmpInsure/GetInsurePersonalMoney?CompanyCode={CompanyCode}&EmpID={EmpID}&InsureYear={InsureYear}";

        //各類所得扣繳暨免扣繳憑單
        private static string _getIncomeTaxTransListUri = "api/IncomeTax/GetIncomeTaxTransList?CompanyCode={CompanyCode}&EmpID={EmpID}&IncomeTaxYear={IncomeTaxYear}";
        private static string _getIncomeTaxCycleInfoListUri = "api/IncomeTax/GetIncomeTaxCycleInfoList?CompanyCode={CompanyCode}&BeginYear={BeginYear}&EndYear={EndYear}";

        /*遠百客製 API (End)*/
        private static string _checkScheduleResultUri = "api/EmpSchedule/EmpScheduleCheck?CompanyCode={CompanyCode}&DepartmentCode={DepartmentCode}&&EmpID={EmpID}&LeaveCode={LeaveCode}&AgentID={AgentID}&AbsentReason={AbsentReason}&NowTime={NowTime}&BeginTime={BeginTime}&EndTime={EndTime}&isCheck={isCheck}";
        private static string _checkLeaveUri = "api/Leave/LeaveCheck?CompanyCode={CompanyCode}&EmpID={EmpID}&BeginTime={BeginTime}&EndTime={EndTime}&LeaveCode={LeaveCode}&AgentID={AgentID}&AbsentReason={AbsentReason}&isCheck={isCheck}";

        //20170627 Daniel 增加請假檢核回傳明細的API網址
        private static string _checkLeaveDetailUri = "api/Leave/LeaveCheckWithDetail?CompanyCode={CompanyCode}&EmpID={EmpID}&BeginTime={BeginTime}&EndTime={EndTime}&LeaveCode={LeaveCode}&AgentID={AgentID}&AbsentReason={AbsentReason}&isCheck={isCheck}";

        //20180322 小榜 增加請假檢核計算時數API網址
        private static string _checkAbsentAmountUri = "api/Leave/CheckAbsentAmount?CompanyCode={CompanyCode}&EmpID={EmpID}&BeginTime={BeginTime}&EndTime={EndTime}&LeaveCode={LeaveCode}&isCheck={isCheck}&LanguageCookie={LanguageCookie}"; //20190530 Daniel 增加傳入語系

        //20201216 Daniel 配合BambooHR，修改原檢查時數API
        private static string _bambooHRCheckAbsentAmountUri = "api/Leave/BambooHRCheckAbsentAmount?CompanyCode={CompanyCode}&EmpID={EmpID}&EmpData_ID={EmpData_ID}&BeginTime={BeginTime}&EndTime={EndTime}&AbsentCode={AbsentCode}&InputAmount={InputAmount}&InputUnit={InputUnit}"; 

        //20170629 Daniel 增加儲存、刪除及更新請假單檢核明細的網址(HTTP POST)
        private static string _SavePortalFormDetailUri = "api/PortalForm/CreatePortalFormDetail";
        private static string _DeletePortalFormDetailUri = "api/PortalForm/DeletePortalFormDetail?FormNo={FormNo}";
        private static string _UpdatePortalFormDetailUri = "api/PortalForm/UpdatePortalFormDetail?FormNo={FormNo}&Status={Status}&UpdateEmpID={UpdateEmpID}";

        private static string _checkDutyUri = "api/Duty/DutyCardCheck?CompanyCode={CompanyCode}&EmpID={EmpID}&ClockInTime={ClockInTime}&ClockInWay={ClockInWay}";
        private static string _checkOverTimeUri = "api/OverTime/OvertimeCheck?CompanyCode={CompanyCode}&EmpID={EmpID}&BeginTime={BeginTime}&EndTime={EndTime}&CheckFlag={CheckFlag}&ToRest={ToRest}&HaveDinning={HaveDinning}&EatFee={EatFee}&CutTime={CutTime}&isCheckDutyCard={isCheckDutyCard}&InProcessingAmt={InProcessingAmt}&LanguageCookie={LanguageCookie}";
        private static string _checkDeleteLeaveUri = "api/Leave/DeleteLeaveCheck?FormNo={FormNo}&CompanyCode={CompanyCode}&EmpID={EmpID}";
        private static string _checkDeletePatchCard = "api/Duty/DeletePatchCardCheck?FormNo={FormNo}&CompanyCode={CompanyCode}&EmpID={EmpID}";
        private static string _checkDeleteOverTimeUri = "api/OverTime/DeleteOvertimeCheck?FormNo={FormNo}&CompanyCode={CompanyCode}&EmpID={EmpID}";
        //20180424 小榜 增加讀取假單/加班單 薪資批號
        private static string _getLeaveSalaryFormNo = "api/Leave/GetLeaveSalaryFormNoData?CompanyCode={CompanyCode}&FormNo={FormNo}&StatusData={StatusData}";

        //20230201 小榜 增加查詢所有薪給調整通知單功能(考績)
        private static string _getSalaryChangeNoteAllStatus = "api/Salary/GetEmpSalaryChangeNoteAllStatus?CompanyCode={CompanyCode}&EmpID={EmpID}";
        private static string _getSalaryChangeNoteInfo = "api/Salary/SalaryChangeNoteDataList?CompanyCode={CompanyCode}&EmpID={EmpID}&ChangeDate={ChangeDate}";

        private static string _postGeneralLeaveListUri = "api/Leave/GeneralGetToDayLeaveList";
        private static string _postLeaveListUri = "api/Leave/GetToDayLeaveList";
        private static string _postLeaveUri = "api/Leave/SetLeave";
        private static string _postDutyUri = "api/Duty/SetDutyCard";
        private static string _postDutyUri_FEDS = "api/Duty/SetDutyCard_FEDS";
        private static string _postDutyAddFormNoUri = "api/Duty/SetDutyCardAddFormNo";
        private static string _postOverTimeUri = "api/OverTime/SetOvertime";
        private static string _postDeleteLeaveUri = "api/Leave/DeleteLeave";
        private static string _postDeletePatchCardUri = "api/Duty/DeletePatchCard";
        private static string _postDeleteOverTimeUri = "api/OverTime/DeleteOverTime";
        private static string _PostModifyEmpDataUri = "api/employee/ModifyEmpData";
        private static string _saveEmpDataCasualUri = "api/EmpDataCasual/SaveEmpDataCasual";
        private static string _saveDutyResultWorkingUri = "api/DutyResult/SaveDutyResultWorking";
        private static string _saveDutyResultWorking2Uri = "api/DutyResult/SaveDutyResultWorking2";

        private static string _createEmpChangeK9Uri = "api/EmpChange/CreateEmpChangeK9";
        private static string _createEmpScheduleK9Uri = "api/EmpSchedule/CreateEmpScheduleK9";
        private static string _createEmpDutyCardK9Uri = "api/EmpDutyCard/CreateEmpDutyCardK9";
        private static string _createEmpLaborK9Uri = "api/EmpLabor/CreateEmpLaborK9";
        private static string _createCasualFormUri = "api/CasualForm/Create";
        private static string _createEmpScheduleTimeK9Uri = "api/EmpSchedule/CreateEmpScheduleTimeK9";
        private static string _updateCasualCycleUri = "api/CasualCycle/UpdateCasualCycle";
        private static string _updateCasualFormUri = "api/CasualForm/Update";
        private static string _updateEmpScheduleTimeK9Uri = "api/EmpSchedule/UpdateEmpScheduleTimeK9";
        private static string _deleteCasualFormUri = "api/CasualForm/Del";
        private static string _deleteEmpScheduleTimeK9Uri = "api/EmpSchedule/DelEmpScheduleTimeK9";

        //20181122 Daniel 增加Warm Up HRMApi
        private static string _warmUpAPIUri = "api/WarmUp/WarmUp";

        //20180314 Daniel 增加DDMC專用薪資領條查詢請假資訊的API網址
        private static string _getEmployeeAbsentDetailDDMC = "api/Salary/GetAbsentItems_DDMC?CompanyCode={CompanyCode}&EmpID={EmpID}&FormNo={FormNo}";
  
        //20180328 Daniel 增加DDMC專用查詢部門人員薪資，所有可查詢到薪資的工號清單
        private static string _getGetDeptSalaryEmpIDList = "api/DeptSalary/GetDeptSalaryQueryMapping?EmpID={EmpID}&IsHrOrAdmin={IsHrOrAdmin}";

        //20170913 Daniel 增加查詢臨時工最低時薪參數的API網址
        private static string _getCasualMinSalaryPerHour = "api/CasualForm/GetCasualMinSalaryPerHour?CompanyCode={CompanyCode}";

        //20180502 小榜 增加DDMC專用查詢部門可選代理人部門 
        private static string _getGetAgentDeptList = "api/Agent/GetDeptAgentQueryMapping?DeptCode={DeptCode}&EmpID={EmpID}";

        private static string _getEmpCanUseCount = "api/Leave/CanUseCount?CompanyCode={CompanyCode}&EmpID={EmpID}&begin={begin}&end={end}&AbsentCode={AbsentCode}";

        private static string _getAnnualLeaveSummary = "api/Leave/GetAnnualLeaveSumItem?Year={Year}&CompanyCode={CompanyCode}&EmpID={EmpID}";
        //private static string _getAnnualLeaveCodeUri = "api/Leave/GetAnnualLeaveCode?CompanyCode={CompanyCode}";
        private static string _getAnnualLeaveCodeUri = "api/Leave/GetAnnualLeaveCodeList?CompanyCode={CompanyCode}";

        private static string _checkFormNoUri = "api/Salary/GetCheckFormNo?CompanyCode={CompanyCode}&FormNo={FormNo}";

        public static Func<string> GetHostUri;

        private static async Task<string> SendRequest(HttpMethod method, string uri, object data = null)
        {
            string _responseBody = string.Empty;

            using (HttpClient _client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpResponseMessage _response;

                try
                {
                    if (method == HttpMethod.Get)
                    {
                        _response = await _client.GetAsync(uri);
                    }
                    else if (method == HttpMethod.Post)
                    {
                        _response = await _client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    _response.EnsureSuccessStatusCode();
                    _responseBody = await _response.Content.ReadAsStringAsync();
                }
                catch (Exception ex) // SSL certificate error
                {
                    throw ex;
                }
            }

            return _responseBody;
        }
    }
}

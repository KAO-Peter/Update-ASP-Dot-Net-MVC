using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using HRPortal.DBEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HRPortal.Services.Models.BambooHR;
using System.Data.Entity;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using System.Globalization;
using HRPortal.Services.Models;
using System.Text.RegularExpressions;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;
using HRPortal.MultiLanguage;
using HRPortal.Mvc.Models;

namespace HRPortal.BambooHRIntegration
{
    public class BambooHRIntegrationService
    {
        private const int WORKHOURS_PER_DAY = 8; //預設一天工時為8小時
        private const string DEFAULT_COMPANY_CODE = "ddmc"; //先預設公司代碼為ddmc，因目前不會有其他公司，可減少一些查資料庫的時機
        private const string DEFAULT_ADMIN_EMPLOYEENO = "admin"; //預設admin的帳號，需要系統身分時會用到
        private const string DEFAULT_LOG_IP = "127.0.0.1";
        private const int DEFAULT_START_HOUR = 9;
        private const int DEFAULT_END_HOUR = 18;

        public static string urlAddTimeOffRequest = "/v1/employees/{employeeId}/time_off/request";
        public static string urlQueryTimeOffRequests = "/v1/time_off/requests";
        public static string urlChangeTimeOffRequestStatus = "/v1/time_off/requests/{requestId}/status";
        public static string urlQueryEmployeeDirectory = "/v1/employees/directory";
        public static string urlQueryEmployee = "/v1/employees/{id}";
        public static string urlAdjustTimeOffBalance = "/v1/employees/{employeeId}/time_off/balance_adjustment";
        public static string urlQueryUsers = "/v1/meta/users";

        protected HRPortal_Services _services;
        private SystemSettingService _systemSettingService;
        private BambooHRSetting _setting;
        private LogInfo _logInfo;
        private Employee _empAdmin; //admin的Employee物件
        private List<BambooHRMailNotificationSetting> _mailSettings;
        private string _lastCheckTimeOffResponse = ""; //紀錄上次檢查所有假單的回傳結果，用來判斷是否要存Log(與上次不同才存)

        public BambooHRIntegrationService(LogInfo LogInfo = null, string LastCheckTimeOffResponse = "")
        {
            this._lastCheckTimeOffResponse = LastCheckTimeOffResponse;

            this._services = new HRPortal_Services();
            this._systemSettingService = this._services.GetService<SystemSettingService>();

            string BaseURL = this._systemSettingService.GetSettingValue("BambooHR_BaseURL");
            string APIKey = this._systemSettingService.GetSettingValue("BambooHR_APIKey");
            string apiOwnerUsertID = this._systemSettingService.GetSettingValue("BambooHR_APIOwnerUserID");
            string bambooHRAdminUsertID = this._systemSettingService.GetSettingValue("BambooHR_AdminUserID");

            //移除末尾的斜線
            BaseURL = BaseURL.EndsWith("/") ? BaseURL.Substring(0, BaseURL.Length - 1) : BaseURL;
            APIKey = APIKey.EndsWith("/") ? APIKey.Substring(0, APIKey.Length - 1) : APIKey;

            this._setting = new BambooHRSetting() { BaseURL = BaseURL, APIKey = APIKey, APIOwnerUserID = apiOwnerUsertID, AdminUserID = bambooHRAdminUsertID };

            this._empAdmin = this._services.GetService<EmployeeService>().GetEmployeeByEmpNo(DEFAULT_ADMIN_EMPLOYEENO);

            //設定Log資訊
            if (LogInfo != null)
            {
                //IP沒輸入就預設為127.0.0.1
                LogInfo.UserIP = string.IsNullOrWhiteSpace(LogInfo.UserIP) ? DEFAULT_LOG_IP : LogInfo.UserIP;

                if (LogInfo.UserID == null) //沒有傳入UserID資訊就先用admin的
                {
                    if (this._empAdmin != null)
                    {
                        LogInfo.UserID = this._empAdmin.ID;
                    }
                    else //如果連admin都找不到，就不紀錄Log，將logInfo設定為null
                    {
                        LogInfo = null;
                    }
                }
            }

            this._logInfo = LogInfo;
            //this._services.GetService<SystemLogService>().WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, "建立IntegrationService");
            //取得異常狀況Mail通知設定
            List<BambooHRMailNotificationSetting> mSettings = this._services.GetService<BambooHRMailNotificationSettingService>().Where(x => x.Enabled).ToList();
            this._mailSettings = mSettings;
        }

        public IRestResponse Send(RequestItem rItem, bool NeedCheckResponse = false, string CheckContent = null)
        {
            IRestResponse response = null;
            Exception exForLog = null;
            string url = "";
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                url = this._setting.BaseURL + rItem.URL;
                var client = new RestClient(url);

                string password = "xxx"; //任意字串都可以
                client.Authenticator = new HttpBasicAuthenticator(this._setting.APIKey, password);

                response = client.Execute(rItem.RestRequest);
            }
            catch (Exception ex)
            {
                exForLog = ex;
            }
            finally
            {
                Log(url, rItem.RestRequest, response, exForLog, NeedCheckResponse, CheckContent);
            }

            return response;

        }

        /// <summary>
        /// 依據BambooHR EmployeeID找出BambooHR儲存的HRM工號
        /// </summary>
        /// <param name="BambooHREmployeeID"></param>
        /// <returns></returns>
        public string GetBamboooEmployeeNumberByEmployeeID(string BambooHREmployeeID)
        {
            string empID = "";
            QueryEmployeeResult query = API_GetEmployee(BambooHREmployeeID);
            empID = query.employeeNumber;
            return empID;
        }

        public QueryEmployeeResult API_GetEmployee(string BambooHREmployeeID)
        {
            string url = urlQueryEmployee;
            url = url.Replace("{id}", BambooHREmployeeID);

            var request = new RestRequest();
            request.AddHeader("Accept", "application/json");
            request.Method = Method.GET;

            request.AddParameter("fields", "displayName,employeeNumber,workEmail");

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            QueryEmployeeResult result = JsonConvert.DeserializeObject<QueryEmployeeResult>(response.Content);

            return result;
        }

        public Dictionary<string, QueryUsersResult_User> API_GetAllUsers()
        {
            string url = urlQueryUsers;

            var request = new RestRequest();
            request.AddHeader("Accept", "application/json");
            request.Method = Method.GET;

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            Dictionary<string, QueryUsersResult_User> result = JsonConvert.DeserializeObject<Dictionary<string, QueryUsersResult_User>>(response.Content);

            return result;
        }

        public EmployeeDirectoryResult API_GetAllEmployees()
        {
            string url = urlQueryEmployeeDirectory;

            var request = new RestRequest();
            request.AddHeader("Accept", "application/json");
            request.Method = Method.GET;

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            EmployeeDirectoryResult result = JsonConvert.DeserializeObject<EmployeeDirectoryResult>(response.Content);

            return result;
        }

        //匯入歷史假單到BambooHR(特定員工)
        public async Task<ImportHistoryResult> ImportHistoryLeaveForm(String EmpID, string CompanyCode, DateTime BeginDate, DateTime EndDate, bool includeSigning = false, bool onlyForViewing = false)
        {
            StringBuilder sbError = new StringBuilder();
            List<LeaveFormGeneral> generalList = new List<LeaveFormGeneral>();
            ImportHistoryResult result = new ImportHistoryResult();

            string BambooHRAbsentCodes = this._systemSettingService.GetSettingValue("BambooHR_AbsentCodeList");
            if (!string.IsNullOrWhiteSpace(BambooHRAbsentCodes))
            {
                List<string> bbAbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();

                //由API取得後台資料
                List<AbsentDataForBambooHR> absentDataList = await HRMApiAdapter.GetAbsentDataForBambooHR(CompanyCode, EmpID, bbAbsentCodeList, BeginDate, EndDate);

                //排除歷史假單已經送過的
                BambooHRHistoryLeaveFormService historyService = this._services.GetService<BambooHRHistoryLeaveFormService>();
                List<string> historyFormNoList = historyService.Where(x => x.EmpID == EmpID).Select(y => y.FormNo).ToList();
                absentDataList = absentDataList.Where(x => !historyFormNoList.Contains(x.FormNo)).ToList();

                //取得前台假單資料，先只找已核准的
                DateTime endTime = EndDate.AddDays(1).AddSeconds(-1);
                List<LeaveForm> leaveFormList = this._services.GetService<LeaveFormService>().Where(x => x.Employee.EmployeeNO == EmpID && x.Status == 3
                                                        && ((x.StartTime >= BeginDate && x.StartTime <= endTime) || (x.EndTime >= BeginDate && x.EndTime <= endTime)))
                                                        .Include("Agent").AsNoTracking().ToList();

                //整理成通用物件LeaveFormGeneral
                //由後台資料分出前台與後台的假單
                //前台的，要用前台資料傳送
                List<AbsentDataForBambooHR> portalAbsentDataList = absentDataList.Where(x => x.FormNo.StartsWith("P")).ToList();
                List<string> portalFormNoList = portalAbsentDataList.Select(x => x.FormNo).Distinct().ToList();
                if (portalFormNoList.Count > 0)
                {
                    //用前台資料整理成LeaveFormGeneral，先不處理前後台假單不一致的狀況(包括只有前台有或後台有，以及時數不符等等)
                    List<LeaveFormGeneral> pList = leaveFormList.Where(x => portalFormNoList.Contains(x.FormNo)).Select(y => PortalFormToLeaveFormGeneral(y, true)).ToList();

                    if (pList.Count > 0)
                    {
                        generalList.AddRange(pList);
                    }
                }

                //後台的要用後台資料組假單傳送
                List<AbsentDataForBambooHR> hrmAbsentDataList = absentDataList.Where(x => !x.FormNo.StartsWith("P")).ToList();
                if (hrmAbsentDataList.Count > 0)
                {
                    //用後台資料整理成LeaveFormGeneral
                    List<LeaveFormGeneral> hList = hrmAbsentDataList.Select(x => HRMFormToLeaveFormGeneral(x)).ToList();
                    generalList.AddRange(hList);
                }

                //如果傳入參數要包含簽核中假單
                if (includeSigning)
                {
                    //取得簽核中假單，這邊會單獨處理是因為不一定需要匯入簽核中的(簽核中假單匯入後，可能會造成其他問題得需要人工才能處理)
                    List<LeaveForm> signingFormList = this._services.GetService<LeaveFormService>().Where(x => x.Employee.EmployeeNO == EmpID && x.Status == 1
                                                        && ((x.StartTime >= BeginDate && x.StartTime <= endTime) || (x.EndTime >= BeginDate && x.EndTime <= endTime)))
                                                        .Include("Agent").AsNoTracking().ToList();

                    //整理成LeaveFormGeneral，這邊仍用歷史假單方式送出，但狀態不會錯，因為是簽核中的
                    if (signingFormList.Count > 0)
                    {
                        List<LeaveFormGeneral> sList = signingFormList.Select(x => PortalFormToLeaveFormGeneral(x, true)).ToList();
                        generalList.AddRange(sList);
                    }
                }

                //有歷史資料或是簽核中資料要傳至BambooHR
                if (generalList.Count > 0)
                {
                    generalList = generalList.OrderBy(x => x.StartTime).ThenBy(y => y.EndTime).ToList();

                    foreach (LeaveFormGeneral lfg in generalList)
                    {
                        //儲存歷史資料記錄到BambooHRHistoryLeaveForm
                        BambooHRHistoryLeaveForm historyForm = new BambooHRHistoryLeaveForm()
                        {
                            EmpData_ID = lfg.EmpData_ID ?? 0,
                            EmpID = lfg.EmpID,
                            FormNo = lfg.FormNo,
                            PortalLeaveFormID = lfg.PortalLeaveFormID,
                            EmpAbsent_ID = lfg.EmpAbsent_ID,
                            EmpWorkAdjust_ID = lfg.EmpWorkAdjust_ID,
                            AbsentCode = lfg.AbsentCode,
                            BeginTime = lfg.StartTime,
                            EndTime = lfg.EndTime,
                            AbsentAmount = lfg.LeaveAmount,
                            AbsentUnit = lfg.AmountUnit,
                            FormStatus = lfg.LeaveFormStatus == TimeOffRequestStatus.approved ? 3 : 1, //匯入歷史假單核准的就是3，其它都算是1
                            CreateTime = DateTime.Now
                        };
                        try
                        {
                            if (!onlyForViewing)
                            {
                                historyService.Create(historyForm);

                                //傳至BambooHR，建立假單
                                CreateBambooHRLeaveForm(lfg);
                            }
                        }
                        catch (Exception ex)
                        {
                            sbError.Append("工號=" + lfg.EmpID + "，FormNo=" + lfg.FormNo + "，Error=" + ex.Message + Environment.NewLine);
                        }
                    }
                }
            }

            //設定傳出結果
            result.Result = sbError.ToString();
            result.HistoryFormList = generalList;

            return result;
        }

        //同步BambooHR人員資料到Portal(存入對應ID)
        public string SyncBambooHREmployee()
        {
            EmployeeDirectoryResult apiResult = API_GetAllEmployees();
            string result = UpdateMappingFromBambooHREmployeeDirectory(apiResult.employees);
            return result;
        }

        /// <summary>
        /// 根據BambooHR所有的員工資料，更新對應表格
        /// </summary>
        /// <param name="Employees">BambooHR取回的所有員工資料</param>
        /// <returns></returns>
        public string UpdateMappingFromBambooHREmployeeDirectory(List<EmployeeDirectoryResult_Employee> Employees)
        {
            StringBuilder errMessage = new StringBuilder();

            if (Employees.Count > 0)
            {
                try
                {
                    //依據BambooHR的Employee ID來找對應表格的資料
                    List<string> bambooIDList = Employees.Select(x => x.id).ToList();

                    BambooHREmployeeMappingService mappingService = this._services.GetService<BambooHREmployeeMappingService>();

                    List<BambooHREmployeeMapping> mappingAll = mappingService.GetAll().Where(x => bambooIDList.Contains(x.BambooHREmployeeID)).ToList();
                    List<Employee> portalEmployees = this._services.GetService<EmployeeService>().GetAll().AsNoTracking().ToList();

                    foreach (EmployeeDirectoryResult_Employee bbEmp in Employees)
                    {
                        Guid? portalEmployeeID = null;
                        int? empData_ID = null;
                        //string empID = null;
                        string empName = null;

                        //先取得BambooHR儲存的工號
                        string empID = GetBamboooEmployeeNumberByEmployeeID(bbEmp.id);

                        //用BambooHR的Employee ID當Key來檢查是否已經有對應表格的資料
                        //有相同ID的，檢查BamBooHREmail與HRPortal Employee資料是否相同，不相同就要更新
                        //沒有ID的，就新增一筆BambooHREmployeeMapping紀錄
                        //因為不刪除資料，所以可能會有一些歷史資料存留
                        BambooHREmployeeMapping mapping = mappingAll.Where(x => x.BambooHREmployeeID == bbEmp.id).OrderByDescending(y => y.UpdateTime).FirstOrDefault();

                        //Portal資料用工號找，先不看公司別(不會有其他公司)
                        Employee portalEmp = portalEmployees.Where(x => x.EmployeeNO == empID).OrderByDescending(y => y.CreatedTime).FirstOrDefault();
                        if (portalEmp != null)
                        {
                            portalEmployeeID = portalEmp.ID;
                            empData_ID = portalEmp.Employee_ID;
                            empName = portalEmp.EmployeeName;
                        }

                        if (mapping != null) //對應表格已經有資料，要更新
                        {
                            if (bbEmp.workEmail != mapping.BambooHREmail || bbEmp.displayName != mapping.BambooHREmployeeName
                                || mapping.PortalEmployeeID != portalEmployeeID || mapping.EmpData_ID != empData_ID || mapping.EmpID != empID || mapping.EmpName != empName)
                            {
                                mapping.BambooHREmail = bbEmp.workEmail ?? "";
                                mapping.BambooHREmployeeName = bbEmp.displayName;
                                mapping.PortalEmployeeID = portalEmployeeID;
                                mapping.EmpData_ID = empData_ID;
                                mapping.EmpID = empID;
                                mapping.EmpName = empName;
                                mapping.UpdateTime = DateTime.Now;

                                mappingService.Update(mapping);
                            }

                        }
                        else //沒有舊紀錄，要新增一筆
                        {
                            mapping = new BambooHREmployeeMapping()
                            {
                                BambooHREmployeeID = bbEmp.id,
                                BambooHREmail = bbEmp.workEmail ?? "",
                                BambooHREmployeeName = bbEmp.displayName,
                                PortalEmployeeID = portalEmployeeID,
                                EmpData_ID = empData_ID,
                                EmpID = empID,
                                EmpName = empName,
                                CreateTime = DateTime.Now
                            };

                            mappingService.Create(mapping);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errMessage.AppendLine(ex.Message);
                }
            }

            return errMessage.ToString();
        }

        //更新BambooHR的User，對應到Employee
        public string SyncBambooHRUser()
        {
            Dictionary<string, QueryUsersResult_User> apiResult = API_GetAllUsers();
            string result = UpdateBambooHRUserMapping(apiResult);

            return result;

        }

        public string UpdateBambooHRUserMapping(Dictionary<string, QueryUsersResult_User> allUsers)
        {
            StringBuilder errMessage = new StringBuilder();

            if (allUsers!=null && allUsers.Count > 0)
            {
                try
                {
                    //取得BambooHREmployeeMapping所有資料
                    BambooHREmployeeMappingService mappingService = this._services.GetService<BambooHREmployeeMappingService>();
                    List<BambooHREmployeeMapping> mappingAll = mappingService.GetAll().ToList();

                    //逐筆更新
                    List<int> mappingFoundIDList=new List<int>();
                    foreach (KeyValuePair<string,QueryUsersResult_User> item in allUsers)
                    {
                        BambooHREmployeeMapping mapping = mappingAll.Where(x => x.BambooHREmployeeID == item.Value.employeeId).FirstOrDefault();
                        if (mapping != null)
                        {
                            mappingFoundIDList.Add(mapping.ID);
                            mapping.BambooHRUserID = item.Value.id;
                            mapping.BambooHRUserFirstName = item.Value.firstName;
                            mapping.BambooHRUserLastName = item.Value.lastName;
                            mapping.BambooHRUserEmail = item.Value.email;
                            mapping.BambooHRUserStatus = item.Value.status;
                            mapping.BambooHRUserLastLogin = item.Value.lastLogin;
                            
                        }
 
                    }

                    //清空沒有對應到的User資訊(有可能之後刪掉User)
                    List<BambooHREmployeeMapping> mappingNotFoundList = mappingAll.Where(x => !mappingFoundIDList.Contains(x.ID)).ToList();
                    foreach (BambooHREmployeeMapping noMapping in mappingNotFoundList)
                    {
                        noMapping.BambooHRUserID = null;
                        noMapping.BambooHRUserFirstName = null;
                        noMapping.BambooHRUserLastName = null;
                        noMapping.BambooHRUserEmail = null;
                        noMapping.BambooHRUserStatus = null;
                        noMapping.BambooHRUserLastLogin = null;    
                    }

                    mappingService.Db.SaveChanges();
                   
                }
                catch (Exception ex)
                {
                    errMessage.AppendLine(ex.Message);
                }
            }

            return errMessage.ToString();
        }

        public string CreateBambooHRLeaveForm(Guid LeaveFormID, bool forHistory = false)
        {
            //取得假單
            LeaveForm lf = this._services.GetService<LeaveFormService>().GetLeaveFormByIDWithEmployee(LeaveFormID);

            return CreateBambooHRLeaveForm(lf, forHistory);
        }

        public string CreateBambooHRLeaveForm(LeaveForm LF, bool forHistory = false)
        {
            LeaveFormGeneral lfg = PortalFormToLeaveFormGeneral(LF, forHistory);

            return CreateBambooHRLeaveForm(lfg);
        }

        private LeaveFormGeneral PortalFormToLeaveFormGeneral(LeaveForm LF, bool forHistory = false)
        {
            return new LeaveFormGeneral()
            {
                FormNo = LF.FormNo,
                PortalLeaveFormID = LF.ID,
                EmpData_ID = LF.Employee.Employee_ID,
                EmpID = LF.Employee.EmployeeNO,
                PortalEmployeeID = LF.EmployeeID,
                StartTime = LF.StartTime,
                EndTime = LF.EndTime,
                AbsentCode = LF.AbsentCode,
                LeaveAmount = LF.LeaveAmount,
                AmountUnit = LF.AbsentUnit,
                LeaveFormStatus = LF.Status == 3 ? TimeOffRequestStatus.approved : TimeOffRequestStatus.requested,
                LeaveReason = LF.LeaveReason,
                forHistory = forHistory,
                EmpAbsent_ID = null,
                EmpWorkAdjust_ID = null,
                AgentEmpID = LF.Agent != null ? LF.Agent.EmployeeNO : "",
                CompanyCode = DEFAULT_COMPANY_CODE
            };
        }

        private LeaveFormGeneral HRMFormToLeaveFormGeneral(AbsentDataForBambooHR Item)
        {
            return new LeaveFormGeneral()
            {
                FormNo = Item.FormNo,
                PortalLeaveFormID = null,
                EmpData_ID = Item.EmpData_ID,
                EmpID = Item.EmpID,
                PortalEmployeeID = null,
                StartTime = Item.BeginTime,
                EndTime = Item.EndTime,
                AbsentCode = Item.AbsentCode,
                LeaveAmount = (decimal)Item.AbsentAmount,
                AmountUnit = "h", //後台的假單時數單位固定為小時
                LeaveFormStatus = TimeOffRequestStatus.approved, //後台假單一定是核准的
                LeaveReason = Item.AbsentReason,
                forHistory = true, //後台都算是歷史假單
                EmpAbsent_ID = Item.EmpAbsent_ID,
                EmpWorkAdjust_ID = Item.EmpWorkAdjust_ID,
                AgentEmpID = Item.AgentEmpID,
                CompanyCode = DEFAULT_COMPANY_CODE
            };
        }

        /// <summary>
        /// 新增BambooHR假單
        /// </summary>
        /// <param name="LeaveFormID">請假單ID(Portal)</param>
        /// <returns></returns>
        public string CreateBambooHRLeaveForm(LeaveFormGeneral LFG)
        {
            string result = "";
            IRestResponse response = new RestResponse();

            if (LFG != null)
            {
                //string empID = LFG.EmpID;
                string bambooHREmployeeID = this._services.GetService<BambooHREmployeeMappingService>().GetBambooHREmployeeIDByEmpID(LFG.EmpID);

                //請假時數要轉換，目前BambooHR假別單位都是hours，但Portal假單LeaveAmount是依據單位存的，跟後台不同
                decimal amount = LFG.AmountUnit == "d" ? LFG.LeaveAmount * WORKHOURS_PER_DAY : LFG.LeaveAmount;

                //整理請假理由，補上請假實際時間(之後會以此判定是哪張假單)，後來有改為直接在建立假單時取回Time Off Request ID
                List<TimeOffRequest_Note> notes = new List<TimeOffRequest_Note>();
                string fromStartToEndTime = GetFromStartToEndTime(LFG.StartTime, LFG.EndTime);
                string note = LFG.LeaveReason + Environment.NewLine + fromStartToEndTime;
                notes.Add(new TimeOffRequest_Note() { from = "employee", note = note });

                string managerNote = "";
                if (LFG.forHistory && LFG.LeaveFormStatus == TimeOffRequestStatus.approved) //歷史假單且是核准的，要補上系統匯入歷史假單說明
                {
                    managerNote = "系統匯入歷史假單"; //目前直接新增核准假單一定是匯入歷史假單
                    notes.Add(new TimeOffRequest_Note() { from = "manager", note = managerNote });
                }

                TimeOffRequest sendObj = new TimeOffRequest()
                {
                    EmpData_ID = LFG.EmpData_ID ?? 0,
                    EmpID = LFG.EmpID,
                    BambooEmployeeID = bambooHREmployeeID,
                    StartTime = LFG.StartTime,
                    EndTime = LFG.EndTime,
                    AbsentCode = LFG.AbsentCode,
                    LeaveAmount = amount,
                    Notes = notes,
                    Status = LFG.forHistory ? LFG.LeaveFormStatus : TimeOffRequestStatus.requested //歷史假單要看假單本身的狀態，非歷史假單固定是新申請(requested)
                };

                try
                {
                    response = API_CreateTimeOffRequest(sendObj);
                    result = string.Format("{0} {1}", ((int)response.StatusCode).ToString(), response.StatusCode.ToString());
                    //檢查回傳代碼是否為201 Created
                    int SuccessCode = 201; //201 Created
                    if ((int)response.StatusCode == SuccessCode)
                    {
                        //取得Header Location內的TimeOffRequestID
                        var objLocation = response.Headers.Where(x => x.Name.ToLower() == "location").Select(y => y.Value).FirstOrDefault();
                        string bbTimeOffRequestID = "";
                        if (objLocation != null)
                        {
                            string location = objLocation.ToString();
                            bbTimeOffRequestID = location.Substring(location.LastIndexOf(@"/") + 1);
                        }

                        //存入發送紀錄到BambooHRLeaveFormRecord表格
                        int formStatus = 0;
                        switch (sendObj.Status)
                        {
                            case TimeOffRequestStatus.requested:
                                formStatus = (int)FormStatus.Signing;
                                break;
                            case TimeOffRequestStatus.approved:
                                formStatus = (int)FormStatus.Send; //Portal核准狀態是3，名稱是Send......
                                break;
                        }

                        BambooHRLeaveFormRecord record = new BambooHRLeaveFormRecord()
                        {
                            PortalLeaveFormID = LFG.PortalLeaveFormID,
                            BambooHRTimeOffID = bbTimeOffRequestID,
                            EmpData_ID = LFG.EmpData_ID,
                            EmpID = LFG.EmpID,
                            BambooHREmployeeID = bambooHREmployeeID,
                            PortalEmployeeID = LFG.PortalEmployeeID,
                            FormNo = LFG.FormNo,
                            BambooHRStatus = TimeOffRequestStatus.requested.ToString(),
                            FormStatus = formStatus,
                            PortalAbsentCode = LFG.AbsentCode,
                            PortalStartTime = LFG.StartTime,
                            PortalEndTime = LFG.EndTime,
                            PortalLeaveAmount = LFG.LeaveAmount,
                            PortalAbsentUnit = LFG.AmountUnit,
                            PortalLeaveReason = LFG.LeaveReason,
                            AgentEmpID = LFG.AgentEmpID,
                            BambooHRStartDate = sendObj.StartTime.Date,
                            BambooHREndDate = sendObj.EndTime.Date,
                            BambooHRLeaveAmount = sendObj.LeaveAmount,
                            BambooHRLeaveReason = note,
                            BambooHRManagerNote = managerNote,
                            CompanyCode = LFG.CompanyCode,
                            UpdateTime = null,
                            UpdateFrom = null,
                            CreateTime = DateTime.Now,
                            CreateFrom = "HRPortal"
                        };
                        BambooHRLeaveFormRecordService recordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                        recordService.LogInfo = this._logInfo;
                        recordService.Create(record);

                    }
                    else //20210226 Daniel 發生錯誤也紀錄到BambooHRLeaveFormRecord
                    {
                        int formStatus = -1;
                        BambooHRLeaveFormRecord record = new BambooHRLeaveFormRecord()
                        {
                            PortalLeaveFormID = LFG.PortalLeaveFormID,
                            BambooHRTimeOffID = "",
                            EmpData_ID = LFG.EmpData_ID,
                            EmpID = LFG.EmpID,
                            BambooHREmployeeID = bambooHREmployeeID,
                            PortalEmployeeID = LFG.PortalEmployeeID,
                            FormNo = LFG.FormNo,
                            BambooHRStatus = TimeOffRequestStatus.requested.ToString(),
                            FormStatus = formStatus,
                            PortalAbsentCode = LFG.AbsentCode,
                            PortalStartTime = LFG.StartTime,
                            PortalEndTime = LFG.EndTime,
                            PortalLeaveAmount = LFG.LeaveAmount,
                            PortalAbsentUnit = LFG.AmountUnit,
                            PortalLeaveReason = LFG.LeaveReason,
                            AgentEmpID = LFG.AgentEmpID,
                            BambooHRStartDate = sendObj.StartTime.Date,
                            BambooHREndDate = sendObj.EndTime.Date,
                            BambooHRLeaveAmount = sendObj.LeaveAmount,
                            BambooHRLeaveReason = note,
                            BambooHRManagerNote = managerNote,
                            CompanyCode = LFG.CompanyCode,
                            IsMailSent = false,
                            Remark = "API_CreateTimeOffRequest回傳異常，StatusCode=" + ((int)response.StatusCode).ToString() + " " + response.StatusCode.ToString(),
                            UpdateTime = null,
                            UpdateFrom = null,
                            CreateTime = DateTime.Now,
                            CreateFrom = "HRPortal"
                        };
                        BambooHRLeaveFormRecordService recordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                        recordService.LogInfo = this._logInfo;
                        recordService.Create(record);
                    }

                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }

            return result;
        }

        public IRestResponse API_CreateTimeOffRequest(TimeOffRequest item)
        {
            string url = urlAddTimeOffRequest;
            url = url.Replace("{employeeId}", item.BambooEmployeeID);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.PUT;
            request.RequestFormat = DataFormat.Json;

            //List<TimeOffRequest_Note> notes = new List<TimeOffRequest_Note>();
            //string fromStartToEndTime = getFromStartToEndTime(item.StartTime, item.EndTime);
            //string note = item.Note + Environment.NewLine + fromStartToEndTime;
            //notes.Add(new TimeOffRequest_Note() { from = "employee", note = note });

            TimeOffRequest_RequestBody body = new TimeOffRequest_RequestBody()
            {
                status = item.Status.ToString(),
                start = item.StartTime.ToString("yyyy-MM-dd"),
                end = item.EndTime.ToString("yyyy-MM-dd"),
                timeOffTypeId = GetBambooHRTimeOffTypeID(item.AbsentCode),
                amount = item.LeaveAmount.ToString(),
                //notes = notes
                notes = item.Notes
            };

            request.AddJsonBody(body);

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            return response;
        }

        public string ChangeLeaveFormStatus(Guid LeaveFormID, TimeOffRequestStatus Status, string SignInstruction, TimeOffRequestStatus StatusBefore = TimeOffRequestStatus.requested, string Source = "HRPortal", bool isRepeatedApproval = false)
        {
            //取得假單
            LeaveForm lf = this._services.GetService<LeaveFormService>().GetLeaveFormByIDWithEmployee(LeaveFormID);

            return ChangeLeaveFormStatus(lf, Status, SignInstruction, StatusBefore, Source, isRepeatedApproval);
        }

        /// <summary>
        /// 更新請假單在BambooHR的狀態
        /// </summary>
        /// <param name="LeaveFormID">請假單ID(Portal)</param>
        /// <param name="Status">新狀態</param>
        /// <param name="SignInstruction">核准或退回的主管意見</param>
        /// <returns></returns>
        public string ChangeLeaveFormStatus(LeaveForm LF, TimeOffRequestStatus Status, string SignInstruction, TimeOffRequestStatus StatusBefore = TimeOffRequestStatus.requested, string Source = "HRPortal", bool isRepeatedApproval = false)
        {
            string result = "";
            IRestResponse response = new RestResponse();

            if (LF != null)
            {
                //取得BambooHR Time Off Request ID，要看StatusBefore看要查之前哪種狀態的假單(一般是requested，銷假是查approved)
                //不能直接看LF狀態，因為LF狀態在到這邊之前可能已經異動了
                //因後來增加發送紀錄，所以改為先由發送記錄內找Time Off ID，找不到才用假單屬性找
                int formStatus = 0;
                string requestID = "";
                BambooHRLeaveFormRecord record = GetBambooHRLeaveFormRecordByFormNo(LF.FormNo);
                if (record != null)
                {
                    formStatus = record.FormStatus;
                    requestID = record.BambooHRTimeOffID;
                    if (string.IsNullOrWhiteSpace(requestID))
                    {
                        requestID = GetBambooHRTimeOffRequestID(LF, StatusBefore);
                    }
                }

                //更新狀態
                if (!string.IsNullOrWhiteSpace(requestID))
                {
                    ChangeTimeOffStatus sendObj = new ChangeTimeOffStatus()
                    {
                        EmpData_ID = LF.Employee.Employee_ID ?? 0,
                        EmpID = LF.Employee.EmployeeNO,
                        BambooTimeOffRequestID = requestID,
                        Status = Status,
                        Note = SignInstruction
                    };

                    string approvedStr = "";
                    if (Status == TimeOffRequestStatus.approved)
                    {
                        approvedStr = "(第" + (isRepeatedApproval ? "2" : "1") + "次)";
                    }

                    try
                    {
                        response = API_ChangeTimeOffRequestStatus(sendObj);
                        result = string.Format("{0} {1}", ((int)response.StatusCode).ToString(), response.StatusCode.ToString());

                        if ((int)response.StatusCode == 200) //成功才更新BambooHRLeaveFormRecord的FormStatus
                        {
                            int updatedFormStatus = (int)FormStatus.TempOrEmpty;
                            switch (Status) //依據Status更新BambooHRLeaveFormRecord狀態 //目前只會傳入approved、denied、canceled
                            {
                                case TimeOffRequestStatus.approved:
                                    updatedFormStatus = (int)FormStatus.Send;
                                    break;
                                case TimeOffRequestStatus.denied:
                                    updatedFormStatus = (int)FormStatus.Draft;
                                    break;
                                case TimeOffRequestStatus.canceled:
                                    updatedFormStatus = (int)FormStatus.Draft;
                                    break;

                            }

                            //20210305 Daniel 因核准要發送兩次，一律改為每次都要更新，不需看狀態是否相同，另外核准時調整備註欄位
                            //if (record.FormStatus != updatedFormStatus) //狀態不同才需要更新狀態
                            //{
                            if (Status == TimeOffRequestStatus.approved)
                            {
                                record.Remark = "更新BambooHR狀態為核准" + approvedStr;
                            }
                            record.BambooHRStatus = Status.ToString(); 
                            record.FormStatus = updatedFormStatus;
                            record.UpdateFrom = "HRPortal";
                            record.UpdateTime = DateTime.Now;

                            BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                            lfRecordService.LogInfo = this._logInfo;
                            lfRecordService.Update(record);
                            //}
                        }
                        else //如果更新狀態回傳HTTP狀態代碼不是200，要更新Remark，FormStatus先不調整，將代碼記錄到UpdateFrom，之後有這錯誤訊息就可以查SystemLog，如果被雙向清掉要查BambooHRLeaveFormRecordLog
                        {
                            record.Remark = "更新BambooHR狀態異常，本次需更新為" + Status.ToString() + "狀態" + approvedStr + "，但回傳HTTP代碼" + ((int)response.StatusCode).ToString() + " " + response.StatusCode.ToString();
                            record.UpdateFrom = "HRPortal";
                            record.UpdateTime = DateTime.Now;
                            BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                            lfRecordService.LogInfo = this._logInfo;
                            lfRecordService.Update(record);
                        }
                        
                        //更新LeaveFormRecord狀態還是要看狀況，不確定如何處理
                        //TimeOffRequestStatus newStatus;
                        
                        
                        /*
                        switch (formStatus)
                        {
                            case 1:
                                newStatus = TimeOffRequestStatus.requested; //新增假單
                                break;
                            case 3:
                                newStatus = TimeOffRequestStatus.approved; //假單核決(最後關卡)
                                break;
                            case 5:
                                newStatus = TimeOffRequestStatus.denied; //假單退回(拒絕)
                                break;
                            case 7:
                                newStatus = TimeOffRequestStatus.pending; //假單核准(中間主管)，此狀態只能透過email判定，目前不會有這個狀態
                                break;
                            case 9:
                                newStatus = TimeOffRequestStatus.canceled; //假單取消
                                break;


                        }
                        */
                        /*
                        if (Status != newStatus) //狀態不同才需要更新，先不管來源是誰
                        {

                        }
                         * */

                    }
                    catch (Exception ex)
                    {
                        result = ex.Message;
                    }
                }
            }

            return result;

        }

        public IRestResponse API_ChangeTimeOffRequestStatus(ChangeTimeOffStatus item)
        {
            string url = urlChangeTimeOffRequestStatus;
            url = url.Replace("{requestId}", item.BambooTimeOffRequestID);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.PUT;
            request.RequestFormat = DataFormat.Json;

            ChangeTimeOffStatus_RequestBody body = new ChangeTimeOffStatus_RequestBody()
            {
                status = item.Status.ToString(),
                note = item.Note
            };

            request.AddJsonBody(body);

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            return response;

        }

        public List<TimeOffRequestQueryResult> API_QueryTimeOffRequests(TimeOffRequestQuery item, BackgroundServiceResult FinalResult = null)
        {
            string url = urlQueryTimeOffRequests;

            var request = new RestRequest();
            request.AddHeader("Accept", "application/json");
            request.Method = Method.GET;

            if (!string.IsNullOrWhiteSpace(item.BambooEmployeeID))
            {
                request.AddParameter("employeeId", item.BambooEmployeeID);
            }

            if (!string.IsNullOrWhiteSpace(item.AbsentCode))
            {
                string typeID = this._services.GetService<BambooHRTimeOffTypeService>().GetTimeOffTypeIDByAbsentCode(item.AbsentCode);
                request.AddParameter("type", typeID);
            }

            request.AddParameter("start", item.BeginDate.ToString("yyyy-MM-dd"));
            request.AddParameter("end", item.EndDate.ToString("yyyy-MM-dd"));

            if (!string.IsNullOrWhiteSpace(item.TimeOffType))
            {
                request.AddParameter("type", item.TimeOffType);
            }

            if (item.Status.HasValue)
            {
                request.AddParameter("status", item.Status.ToString());
            }

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            bool needCheckResponse = true;
            string checkContent = this._lastCheckTimeOffResponse;
            IRestResponse response = Send(rItem, needCheckResponse, checkContent);

            if (FinalResult != null)
            {
                FinalResult.Remark = response.Content; //將回傳結果設定給Remark，讓背景服務可以記錄上次回傳結果
            }

            List<TimeOffRequestQueryResult> result = new List<TimeOffRequestQueryResult>();
            try
            {
                result = JsonConvert.DeserializeObject<List<TimeOffRequestQueryResult>>(response.Content);

            }
            catch (Exception ex) //回傳結果不是Json格式，就拋出錯誤
            {
                throw ex;
            }

            return result;
        }

        public string GetBambooHRTimeOffTypeID(string AbsentCode)
        {
            return this._services.GetService<BambooHRTimeOffTypeService>().GetTimeOffTypeIDByAbsentCode(AbsentCode);
        }

        /// <summary>
        ///  /// <summary>
        /// 依據假單資料(人員、假別、時間與狀態等)找BambooHR Time Off Request ID
        /// </summary>
        /// <param name="LF"></param>
        /// <param name="StatusBefore"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="LeaveFormID"></param>
        /// <param name="StatusBefore"></param>
        /// <returns></returns>
        public string GetBambooHRTimeOffRequestID(Guid LeaveFormID, TimeOffRequestStatus StatusBefore = TimeOffRequestStatus.requested)
        {
            LeaveForm lf = this._services.GetService<LeaveFormService>().GetLeaveFormByIDWithEmployee(LeaveFormID);
            return GetBambooHRTimeOffRequestID(lf, StatusBefore);
        }

        /// <summary>
        /// 依據假單資料(人員、假別、時間與狀態等)找BambooHR Time Off Request ID
        /// </summary>
        /// <param name="LF"></param>
        /// <param name="StatusBefore"></param>
        /// <returns></returns>
        public string GetBambooHRTimeOffRequestID(LeaveForm LF, TimeOffRequestStatus StatusBefore = TimeOffRequestStatus.requested)
        {
            string requestID = "";

            if (LF != null)
            {
                string fromStartToEndTime = GetFromStartToEndTime(LF.StartTime, LF.EndTime);
                string empID = LF.Employee.EmployeeNO;
                string bambooHREmployeeID = this._services.GetService<BambooHREmployeeMappingService>().GetBambooHREmployeeIDByEmpID(empID);

                //查詢之前假單在BambooHR的Time Off Request ID，status要看傳入的之前狀態(一般請假會是requested，銷假單則是要查approved)
                TimeOffRequestQuery query = new TimeOffRequestQuery()
                {
                    BambooEmployeeID = bambooHREmployeeID,
                    AbsentCode = LF.AbsentCode,
                    BeginDate = LF.StartTime.Date,
                    EndDate = LF.EndTime.Date,
                    Status = StatusBefore
                };

                //撈回符合條件的所有假單
                List<TimeOffRequestQueryResult> result = API_QueryTimeOffRequests(query);

                TimeOffRequestQueryResult timeOff = result.Where(x => x.notes != null && x.notes.employee.Contains(fromStartToEndTime)).OrderBy(y => int.Parse(y.id)).FirstOrDefault();

                if (timeOff != null)
                {
                    requestID = timeOff.id;
                }
            }

            return requestID;

        }

        public BambooHRLeaveFormRecord GetBambooHRLeaveFormRecordByFormNo(string FormNo)
        {
            return this._services.GetService<BambooHRLeaveFormRecordService>().GetRecordByFormNo(FormNo);
        }

        /// <summary>
        /// 從發送記錄內找新增假單時所記錄的BambooHR Time Off Request ID
        /// </summary>
        /// <param name="FormNo"></param>
        /// <returns></returns>
        public string GetBambooHRTimeOffRequestIDFromRecord(string FormNo)
        {
            return this._services.GetService<BambooHRLeaveFormRecordService>().GetTimeOffIDByFormNo(FormNo);
        }

        public string CancelLeaveForm(LeaveForm LF)
        {
            string result = "";

            string note = "系統回補銷假時數";

            //依據原假單的日期，決定處理方式
            string prefix = "";
            if (LF.StartTime >= DateTime.Now.Date) //假單開始時間超過當天，還可以修改Time Off狀態
            {
                prefix = "(更新狀態)";
                result = ChangeLeaveFormStatus(LF, TimeOffRequestStatus.canceled, note, TimeOffRequestStatus.approved);
            }
            else //假單開始時間在當天之前，已無法修改，需調整Balance
            {
                prefix = "(調整Balance)";
                result = AdjustBambooHRBalance(LF, note);
            }

            return prefix + " " + result;
        }

        /// <summary>
        /// 調整BambooHR Time Off 可用時數，用在銷假回補，銷假多少時數就回補多少
        /// </summary>
        /// <param name="LF">原請假單</param>
        /// <returns></returns>
        public string AdjustBambooHRBalance(LeaveForm LF, string Note)
        {
            string result = "";
            IRestResponse response = new RestResponse();

            if (LF != null)
            {
                string empID = LF.Employee.EmployeeNO;
                string bambooHREmployeeID = this._services.GetService<BambooHREmployeeMappingService>().GetBambooHREmployeeIDByEmpID(empID);

                ////回補銷假時數要轉換，目前BambooHR假別單位都是hours，但Portal假單LeaveAmount是依據單位存的，跟後台不同
                decimal amount = LF.AbsentUnit == "d" ? LF.LeaveAmount * WORKHOURS_PER_DAY : LF.LeaveAmount;

                AdjustTimeOffBalance sendObj = new AdjustTimeOffBalance()
                {
                    EmpData_ID = LF.Employee.Employee_ID ?? 0,
                    EmpID = LF.Employee.EmployeeNO,
                    BambooEmployeeID = bambooHREmployeeID,
                    BeginDate = LF.StartTime,
                    AbsentCode = LF.AbsentCode,
                    LeaveAmount = amount,
                    Note = Note
                };

                //調整Time Off Balance
                try
                {
                    response = API_AdjustTimeOffBalance(sendObj);
                    result = string.Format("{0} {1}", ((int)response.StatusCode).ToString(), response.StatusCode.ToString());
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }

            }

            return result;
        }

        public IRestResponse API_AdjustTimeOffBalance(AdjustTimeOffBalance item)
        {
            string url = urlAdjustTimeOffBalance;
            url = url.Replace("{employeeId}", item.BambooEmployeeID);

            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.Method = Method.PUT;
            request.RequestFormat = DataFormat.Json;

            AdjustTimeOffBalance_RequestBody body = new AdjustTimeOffBalance_RequestBody()
            {
                date = item.BeginDate.ToString("yyyy-MM-dd"),
                timeOffTypeId = GetBambooHRTimeOffTypeID(item.AbsentCode),
                amount = item.LeaveAmount,
                note = item.Note
            };

            request.AddJsonBody(body);

            RequestItem rItem = new RequestItem()
            {
                URL = url,
                RestRequest = request
            };

            IRestResponse response = Send(rItem);
            return response;

        }

        /// <summary>
        /// BambooHR整合通用Log函式
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="ex"></param>
        public void Log(string URL, IRestRequest request, IRestResponse response, Exception ex, bool NeedCheckResponse = false, string CheckContent = null)
        {
            //有logInfo才存log
            if (this._logInfo != null && this._logInfo.UserID.HasValue)
            {
                string method = "";
                List<RequestParameter> parameters = new List<RequestParameter>();
                object requestBody = null;
                string responseCode = "";
                string responseCodeDescription = "";
                object responseContent = null;

                if (request != null)
                {
                    method = request.Method.ToString();

                    foreach (Parameter p in request.Parameters.Where(x => x.Type == ParameterType.GetOrPost))
                    {
                        parameters.Add(new RequestParameter() { key = p.Name, value = p.Value.ToString() });
                    }

                    if (request.RequestFormat == DataFormat.Json)
                    {
                        Parameter body = request.Parameters.Where(x => x.Type == ParameterType.RequestBody).FirstOrDefault();
                        if (body != null)
                        {
                            try
                            {
                                string strBody = body.Value.ToString();
                                if (strBody.StartsWith("["))
                                {
                                    requestBody = JArray.Parse(strBody); //陣列要用JArray
                                }
                                else
                                {
                                    requestBody = JObject.Parse(strBody);
                                }
                            }
                            catch (Exception reqEX)
                            {
                                requestBody = new { ErrorMsg = reqEX.Message };
                            }
                        }
                    }
                }

                if (response != null)
                {
                    responseCode = ((int)response.StatusCode).ToString();
                    responseCodeDescription = response.StatusCode.ToString();
                    if (!string.IsNullOrWhiteSpace(response.Content) && response.ContentType == "application/json")
                    {
                        try
                        {
                            string strContent = response.Content;
                            if (strContent.StartsWith("["))
                            {
                                responseContent = JArray.Parse(strContent); //陣列要用JArray
                            }
                            else
                            {
                                responseContent = JObject.Parse(strContent);
                            }
                        }
                        catch (Exception resEX)
                        {
                            responseContent = new { ErrorMsg = resEX.Message };
                        }
                    }
                }

                APILog logData = new APILog()
                {
                    URL = URL,
                    HTTP_Method = method,
                    RequestParameters = parameters,
                    RequestBody = requestBody,
                    ResponseCode = responseCode,
                    ResponseCodeDescription = responseCodeDescription,
                    ResponseContent = responseContent,
                    ErrorMessage = ex == null ? "" : ex.Message
                };

                var logService = this._services.GetService<SystemLogService>();
                string strLog = "";
                if (NeedCheckResponse && response.Content == CheckContent) //為減少Log儲存空間，如果要檢查且結果跟檢查字串一致，就存與上次資料相同
                {
                    strLog = "回傳結果與上次相同(" + logData.URL + ")";
                }
                else //不需要檢查或是檢查結果不一致才要記錄Log
                {
                    //如果有傳入Exception物件，表示API傳送接收有異常，增加額外說明方便之後撈取Log
                    string preFix = ex == null ? "" : "API_Exception：";

                    strLog = preFix + JsonConvert.SerializeObject(logData);
                }

                logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, strLog);

            }
        }

        /// <summary>
        /// 撈取BambooHR假單並與之前發送紀錄進行比對，BambooHR上請假簽核同步到Portal最主要的邏輯起點都在此處
        /// </summary>
        /// <returns></returns>
        public BackgroundServiceResult BambooHRCheckTimeOffStatusAndSync(string BambooHRTimeOffIDForCheck = "")
        {
            BackgroundServiceResult finalResult = new BackgroundServiceResult() { Success = false };
            string result = "";
            var logService = this._services.GetService<SystemLogService>();
            try
            {
                //撈取上個月一號之後到明年年底的所有假單
                //20201215 因BambooHR開放可補之前假單，所以不能只撈上月一號之後，改用參數處理
                string checkMonthLimit = this._systemSettingService.GetSettingValue("BambooHR_TimeOffCheckMonth");
                int checkMonth;
                if (!int.TryParse(checkMonthLimit, out checkMonth))
                {
                    checkMonth = 1;
                }
                DateTime lastMonthDate = DateTime.Now.AddMonths(-1 * checkMonth);
                int nextYear = DateTime.Now.Year + 1;
                DateTime QueryStartDate = new DateTime(lastMonthDate.Year, lastMonthDate.Month, 1);
                DateTime QueryEndDate = new DateTime(nextYear, 12, 31);

                //取得BambooHRTimeOffType紀錄
                List<BambooHRTimeOffType> timeOffTypeList = this._services.GetService<BambooHRTimeOffTypeService>().GetAll().AsNoTracking().ToList();

                //組合假別查詢資料
                string timeOffTypeQueryStr = string.Join(",", timeOffTypeList.Select(x => x.BambooHR_TimeOffTypeID).ToArray());

                TimeOffRequestQuery query = new TimeOffRequestQuery()
                {
                    BeginDate = QueryStartDate,
                    EndDate = QueryEndDate,
                    TimeOffType = timeOffTypeQueryStr
                };

                List<TimeOffRequestQueryResult> allTimeOffList = API_QueryTimeOffRequests(query, finalResult);

                //若有傳入Bamboo Time Off Request ID，就只檢查該張假單
                if (!string.IsNullOrWhiteSpace(BambooHRTimeOffIDForCheck))
                {
                    allTimeOffList = allTimeOffList.Where(x => x.id == BambooHRTimeOffIDForCheck).ToList();
                }

                allTimeOffList = allTimeOffList.OrderBy(x => x.id).ToList();
                if (this._logInfo != null && this._logInfo.UserID.HasValue)
                {
                    string strLog = "本次有" + allTimeOffList.Count.ToString() + "筆BambooHR假單需比對";
                    logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, strLog);
                }

                //底下幾個表格因為筆數不會太多，一次撈出來減少DB存取次數
                //取得BambooHRLeaveFormRecord紀錄
                List<BambooHRLeaveFormRecord> leaveFormRecordList = this._services.GetService<BambooHRLeaveFormRecordService>().GetAll().Include("LeaveForm").ToList();

                //取得BambooHREmployeeMapping紀錄
                List<BambooHREmployeeMapping> employeeMappingList = this._services.GetService<BambooHREmployeeMappingService>().GetAll().Include("Employee").Include("Employee.Company").Include("Employee.SignDepartment").AsNoTracking().ToList();

                //canceled與superceded狀態要先處理，接著是denied，最後才是requested與approved，避免取消重請會檢核到重覆假單
                List<string[]> checkOrder = new List<string[]>() 
                {
                    new string[] {TimeOffRequestStatus.canceled.ToString(), TimeOffRequestStatus.superceded.ToString() },
                    new string[] {TimeOffRequestStatus.denied.ToString()},
                    new string[] {TimeOffRequestStatus.requested.ToString(),TimeOffRequestStatus.approved.ToString()}
                };

#if DEBUG 
            List<string> resultList = new List<string>(); //DEBUG模式再回傳結果
#endif

                //依照順序處理各種狀態
                foreach (string[] statusArray in checkOrder)
                {
                    List<TimeOffRequestQueryResult> checkList = allTimeOffList.Where(x => statusArray.Contains(x.status.status)).ToList();
                    string pResult = CompareStatusAndProcess(checkList, leaveFormRecordList, employeeMappingList, timeOffTypeList);
#if DEBUG
                resultList.Add(pResult);
#endif
                }

#if DEBUG 
            result = string.Join(Environment.NewLine, resultList);
#endif

                finalResult.Success = true;
                finalResult.Result = result; //debug訊息存到Result
                //finalResult.Exception = Exception;
                finalResult.ErrorMessage = "";

            }
            catch (Exception ex)
            {
                finalResult.Success = false;
                finalResult.Result = result;
                //finalResult.Exception = Exception;
                finalResult.ErrorMessage = ex.Message;

            }

            return finalResult;

        }

        private string CompareStatusAndProcess(List<TimeOffRequestQueryResult> CheckList, List<BambooHRLeaveFormRecord> LeaveFormRecordList, List<BambooHREmployeeMapping> EmployeeMappingList, List<BambooHRTimeOffType> TimeOffTypeList)
        {
            string finalResult = "";

            StringBuilder sb = new StringBuilder();
            sb.Append("本次比對共" + CheckList.Count.ToString() + "筆" + Environment.NewLine);

            //逐筆進行比對
            foreach (TimeOffRequestQueryResult timeOff in CheckList)
            {
                //檢查是否Record已經存在(用Time Off ID比對)
                var record = LeaveFormRecordList.Where(x => x.BambooHRTimeOffID == timeOff.id).FirstOrDefault();

                if (record == null) //HR Portal沒有紀錄-->檢查狀態
                {
                    //由BambooHR假單整理出所需資料，同時設定BambooHRLeaveFormRecord部分資料-->由CheckTimeOffData裡面移到此處先處理
                    //目前因為改為所有Time Off都會存入BambooHRLeaveFormRecord，所以要先做對應
                    TimeOffRequestMappedInfo mappedInfo = GetMappedInfo(timeOff, EmployeeMappingList, TimeOffTypeList);

                    //BambooHR是requested、approved、denied狀態都要建立新假單
                    if (timeOff.status.status == TimeOffRequestStatus.requested.ToString()
                        || timeOff.status.status == TimeOffRequestStatus.approved.ToString()
                        || timeOff.status.status == TimeOffRequestStatus.denied.ToString())
                    {

                        //需檢核格式、時數、正確才建立假單
                        TimeOffCheckResult timeOffCheckResult = CheckTimeOffData(timeOff, mappedInfo, EmployeeMappingList, TimeOffTypeList);
                        if (timeOffCheckResult.Success) //檢查都正確，要存BambooHRLeaveFormRecord跟建立假單
                        {
                            //BambooHREmployeeMapping empMapping = EmployeeMappingList.Where(x => x.BambooHREmployeeID == timeOff.employeeId).FirstOrDefault();
                            BambooHREmployeeMapping empMapping = mappedInfo.EmployeeMapping;

                            if (empMapping != null)
                            {
                                BambooHRLeaveFormRecord leaveFormRecord = mappedInfo.BambooHRLeaveFormRecord;

                                leaveFormRecord.PortalLeaveFormID = null;
                                leaveFormRecord.FormNo = "";
                                leaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，先給-1
                                leaveFormRecord.CreateFrom = "BambooHR";
                                leaveFormRecord.CreateTime = DateTime.Now;

                                //Portal新增假單，比照原請假單新增程序
                                string createResult = CreatePortalLeaveForm(leaveFormRecord, timeOffCheckResult.HRMCheckResponse, timeOffCheckResult.MappedInfo);

                                if (string.IsNullOrWhiteSpace(createResult))
                                {
                                    //取得最後更新狀態主管，approved是做核決的主管，denied是做退回的主管
                                    string lastManagerBambooHRUserID = timeOff.status.lastChangedByUserId;

                                    Employee lastManager;
                                    if (lastManagerBambooHRUserID == this._setting.APIOwnerUserID)
                                    {
                                        lastManager = this._empAdmin; //如果是BambooHR API Owner退件，就視為Portal Admin退的
                                    }
                                    else
                                    {
                                        lastManager = this._services.GetService<BambooHREmployeeMappingService>().GetEmployeeByBambooHRUserID(lastManagerBambooHRUserID);
                                        lastManager = lastManager != null ? lastManager : this._empAdmin; //目前改為找不到退件主管又不是API_Owner，都視為Portal Admin退的
                                    }

                                    //目前核准不論是否找得到核准者都沒關係，因為Portal是用各關卡主管去做核准
                                    if (timeOff.status.status == TimeOffRequestStatus.approved.ToString()) //如果BambooHR是approved狀態，還要繼續將流程所有關卡都簽核完成
                                    {
                                        string approveResult = ApprovePortalSignFlow(timeOff, leaveFormRecord, lastManager);
                                    }
                                    else if (lastManager != null && timeOff.status.status == TimeOffRequestStatus.denied.ToString()) //如果BambooHR是denied狀態，還要繼續做退回與刪單
                                    {
                                        string rejectResult = RejectPortalSignFlow(timeOff, leaveFormRecord, lastManager);
                                    }

                                }
                                else
                                {
                                    //存入BambooHRLeaveFormRecord
                                    leaveFormRecord.PortalLeaveFormID = null;
                                    leaveFormRecord.FormNo = "";
                                    leaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，給-1
                                    leaveFormRecord.CreateFrom = "BambooHR"; //雖然這邊之前應該已經新增過，但還是更新建立欄位，因實際上這段是無之前紀錄才會進來
                                    leaveFormRecord.CreateTime = DateTime.Now;
                                    leaveFormRecord.IsMailSent = true;

                                    //BambooHRStatua狀態直接用timeOff的狀態
                                    leaveFormRecord.BambooHRStatus = timeOff.status.status;

                                    //加上紀錄狀況到BambooHRLeaveFormRecord
                                    string subject = "新增假單檢核失敗(" + timeOff.id + ")";
                                    string body = createResult;
                                    leaveFormRecord.Remark = subject + " " + body;

                                    //建立假單有異常狀況
                                    SendBambooHRNotificationMail(empMapping == null ? null : empMapping.Employee, "", "新增假單檢核失敗", createResult, leaveFormRecord, EmployeeMappingList);
                                    
                                    BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                                    lfRecordService.LogInfo = this._logInfo;
                                    if (leaveFormRecord.ID > 0)
                                    {
                                        lfRecordService.Update(leaveFormRecord);
                                    }
                                    else
                                    {
                                        lfRecordService.Create(leaveFormRecord);
                                    }
                                }
                            }
                        }
                        else //格式或時數有錯誤，還是要新增BambooHRLeaveFormRecord，要發通知
                        {
                            //存入BambooHRLeaveFormRecord
                            mappedInfo.BambooHRLeaveFormRecord.PortalLeaveFormID = null;
                            mappedInfo.BambooHRLeaveFormRecord.FormNo = "";
                            mappedInfo.BambooHRLeaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，給-1
                            mappedInfo.BambooHRLeaveFormRecord.CreateFrom = "BambooHR";
                            mappedInfo.BambooHRLeaveFormRecord.CreateTime = DateTime.Now;
                            mappedInfo.BambooHRLeaveFormRecord.IsMailSent = true;

                            //BambooHRStatua狀態直接用timeOff的狀態
                            mappedInfo.BambooHRLeaveFormRecord.BambooHRStatus = timeOff.status.status;

                            //加上紀錄狀況到BambooHRLeaveFormRecord
                            string subject = "輸入的時間格式或時數錯誤(" + timeOff.id + ")";
                            string body = timeOffCheckResult.CheckResult;
                            mappedInfo.BambooHRLeaveFormRecord.Remark = subject + " " + body;
                            SendBambooHRNotificationMail(mappedInfo.EmployeeMapping == null ? null : mappedInfo.EmployeeMapping.Employee, "", subject, body, mappedInfo.BambooHRLeaveFormRecord, EmployeeMappingList);

                            BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                            lfRecordService.LogInfo = this._logInfo;
                            if (mappedInfo.BambooHRLeaveFormRecord.ID > 0)
                            {
                                lfRecordService.Update(mappedInfo.BambooHRLeaveFormRecord);
                            }
                            else
                            {
                                lfRecordService.Create(mappedInfo.BambooHRLeaveFormRecord);
                            }
                        }
                    }

                    else if (timeOff.status.status == TimeOffRequestStatus.canceled.ToString()
                        || timeOff.status.status == TimeOffRequestStatus.superceded.ToString()) //抓到新紀錄就已經是取消或轉單的
                    {
                        //建立BambooHRLeaveFormRecord空單，FormStatus是-1(TempOrEmpty)，此處其實跟之前有錯誤所存的資訊類似，只是沒有寄信
                        mappedInfo.BambooHRLeaveFormRecord.PortalLeaveFormID = null;
                        mappedInfo.BambooHRLeaveFormRecord.FormNo = "";
                        mappedInfo.BambooHRLeaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，給-1
                        mappedInfo.BambooHRLeaveFormRecord.CreateFrom = "BambooHR";
                        mappedInfo.BambooHRLeaveFormRecord.CreateTime = DateTime.Now;
                        mappedInfo.BambooHRLeaveFormRecord.IsMailSent = false; //取消的請假單不寄通知

                        BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                        lfRecordService.LogInfo = this._logInfo;
                        lfRecordService.Create(mappedInfo.BambooHRLeaveFormRecord);
                    }
                    /*
                    else if (timeOff.status.status == TimeOffRequestStatus.superceded.ToString()) //抓到新紀錄就已經轉單了
                    {
                        //不處理
                    }
                    */

                }
                else //之前已經有紀錄，需比對兩邊狀態
                {
                    //如果之前發送過異常通知信件，這筆就跳過，因為之後應該是人工接手，沒寄過通知信才繼續處理
                    if (!(record.IsMailSent.HasValue && record.IsMailSent.Value))
                    {
                        bool isStatusValid = true; //雙方狀態是否合理的註記

                        if (timeOff.status.status == TimeOffRequestStatus.requested.ToString()) //BambooHR還是requested狀態，表示沒有變過
                        {
                            if (record.FormStatus == (int)FormStatus.Signing) //BambooHR簽核中，HR Portal簽核中，資料應該相同，不需處理
                            {

                            }
                            else if (record.FormStatus == (int)FormStatus.Send) ////BambooHR簽核中，HR Portal已核決，應該不會有此狀況
                            {
                                isStatusValid = false;
                            }
                            else //其他狀態都不合理
                            {
                                isStatusValid = false;
                            }

                        }
                        else if (timeOff.status.status == TimeOffRequestStatus.approved.ToString()) //BambooHR已核決
                        {
                            if (record.FormStatus == (int)FormStatus.Signing) //BambooHR已核決，HR Portal簽核中，要將流程所有關卡都簽核完成
                            {
                                //取得核准主管
                                string approverBambooHRID = timeOff.status.lastChangedByUserId;
                                Employee approver = this._services.GetService<BambooHREmployeeMappingService>().GetEmployeeByBambooHREmployeeID(approverBambooHRID);
                                string approveResult = ApprovePortalSignFlow(timeOff, record, approver);
                            }
                            else if (record.FormStatus == (int)FormStatus.Send) //BambooHR已核決，HR Portal已核決，不用處理
                            {
                            }
                            else //出現不應該出現的狀態
                            {
                                isStatusValid = false;
                            }

                        }
                        else if (timeOff.status.status == TimeOffRequestStatus.denied.ToString())
                        {
                            if (record.FormStatus == (int)FormStatus.Signing) //BambooHR已退回，HR Portal簽核中，要做退件跟刪單
                            {
                                //取得最後更新狀態主管，找出做退回動作的主管
                                string lastManagerBambooHRUserID = timeOff.status.lastChangedByUserId;
                                Employee lastManager;
                                if (lastManagerBambooHRUserID == this._setting.APIOwnerUserID)
                                {
                                    lastManager = this._empAdmin; //如果是BambooHR API Owner退件，就視為Portal Admin退的
                                }
                                else
                                {
                                    lastManager = this._services.GetService<BambooHREmployeeMappingService>().GetEmployeeByBambooHRUserID(lastManagerBambooHRUserID);
                                    lastManager = lastManager != null ? lastManager : this._empAdmin; //目前改為非API_Owner，又找不到退件主管，就視為Portal Admin退的
                                }

                                string rejectResult = RejectPortalSignFlow(timeOff, record, lastManager);

                                /*
                                if (lastManager != null) //有找到退件人才做事情
                                {
                                    string rejectResult = RejectPortalSignFlow(timeOff, record, lastManager);
                                }
                                */
                            }
                            //else if (record.FormStatus == (int)FormStatus.Draft && record.LeaveForm != null && record.LeaveForm.IsDeleted) 
                            else if (record.FormStatus == (int)FormStatus.Draft) //BambooHR已退回，HR Portal也是未送出，不需處理，目前先不檢查刪單狀態
                            {

                            }
                            else //其他狀態都不合理
                            {
                                isStatusValid = false;
                            }

                        }
                        else if (timeOff.status.status == TimeOffRequestStatus.canceled.ToString())
                        {
                            //if (record.FormStatus == (int)FormStatus.Draft && record.LeaveForm != null && record.LeaveForm.IsDeleted)
                            if (record.FormStatus == (int)FormStatus.Draft) //BambooHR已取消，HR Portal已退回/拉回且已刪單，不處理，目前先不檢查刪單狀態
                            {
                            }
                            else if (record.FormStatus == (int)FormStatus.TempOrEmpty) //BambooHR已取消，HR Portal是空單，表示是之前無紀錄時撈取就是canceled狀態，目前不需處理
                            {
                            }
                            else if (record.FormStatus == (int)FormStatus.Signing) //BambooHR已取消，HR Portal簽核中，一律由admin退件跟刪單
                            {
                                Employee lastManager = this._empAdmin;
                                bool isBambooHRCanceled = true;
                                string rejectResult = RejectPortalSignFlow(timeOff, record, lastManager, isBambooHRCanceled);
                            }
                            else if (record.FormStatus == (int)FormStatus.Send) //BambooHR已取消，HR Portal已核決(核准)，要做銷假流程，由系統Admin建立銷假單並核准
                            {
                                Employee lastManager = this._empAdmin;
                                string cancelResult = CancelApprovePortalLeaveForm(timeOff, record, lastManager);
                            }
                            else
                            {
                                isStatusValid = false;
                            }
                        }
                        else if (timeOff.status.status == TimeOffRequestStatus.superceded.ToString()) //先以與canceled狀態相同邏輯處理
                        {
                            //if (record.FormStatus == (int)FormStatus.Draft && record.LeaveForm != null && record.LeaveForm.IsDeleted) 
                            if (record.FormStatus == (int)FormStatus.Draft)//BambooHR已取消，HR Portal已退回/拉回，不處理，目前先不檢查刪單狀態
                            {
                            }
                            else if (record.FormStatus == (int)FormStatus.TempOrEmpty) //BambooHR已取消，HR Portal是空單，表示是之前無紀錄時撈取就是canceled狀態，目前不需處理
                            {
                            }
                            else if (record.FormStatus == (int)FormStatus.Signing) //BambooHR已取消，HR Portal簽核中，一律由admin退件跟刪單
                            {
                                Employee lastManager = this._empAdmin;
                                bool isBambooHRCanceled = true;
                                string rejectResult = RejectPortalSignFlow(timeOff, record, lastManager, isBambooHRCanceled);
                            }
                            else if (record.FormStatus == (int)FormStatus.Send) //BambooHR已取消(轉單)，HR Portal已核決(核准)，要做銷假流程，由系統Admin建立銷假單並核准
                            {
                                Employee lastManager = this._empAdmin;
                                string cancelResult = CancelApprovePortalLeaveForm(timeOff, record, lastManager);
                            }
                            else
                            {
                                isStatusValid = false;
                            }
                        }

                        //雙方狀態異常，要寄通知
                        if (!isStatusValid)
                        {

                            BambooHREmployeeMapping empMapping = EmployeeMappingList.Where(x => x.BambooHREmployeeID == timeOff.employeeId).FirstOrDefault();
                            string msgStatus = GetStatusDescription(timeOff.status.status, (FormStatus)record.FormStatus);

                            SendBambooHRNotificationMail(empMapping == null ? null : empMapping.Employee, "", "BambooHR狀態異常", msgStatus, record, EmployeeMappingList);
                            record.IsMailSent = true;

                            BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                            lfRecordService.LogInfo = this._logInfo;
                            lfRecordService.Update(record);

                        }
                    }
                    //如果之前有通知過(表示是異常)，但這次假單最後狀態修改人員是BambooHR管理員，且假單狀態是approved，要特別處理，因為有可能管理員是去改日期、時數或是備註，但這時候不會變成轉單
                    else if (timeOff.status.lastChangedByUserId == this._setting.AdminUserID && timeOff.status.status == TimeOffRequestStatus.approved.ToString())
                    {
                        //比對這次資料與BambooHRLeaveFormRecord內的請假起訖日期、時數與備註資料是否不一致，不一致就要重新跑核准動作
                        //假別
                        if (record.BambooHRStartDate == null || (record.BambooHRStartDate.HasValue && record.BambooHRStartDate.Value.ToString("yyyy-MM-dd") != timeOff.start)
                            || record.BambooHREndDate == null || (record.BambooHREndDate.HasValue && record.BambooHREndDate.Value.ToString("yyyy-MM-dd") != timeOff.end)
                            || record.BambooHRLeaveAmount == null || (record.BambooHRLeaveAmount.HasValue && record.BambooHRLeaveAmount.Value != decimal.Parse(timeOff.amount.amount))
                            || (record.BambooHRLeaveReason ?? "") != (timeOff.notes == null ? "" : (timeOff.notes.employee ?? ""))
                            )
                        {
                            //比照直接API撈到資料就是核准的假單處理方式，但BambooHRLeaveFormRecord已經有資料，所以只需更新不需新增record
                            TimeOffRequestMappedInfo mappedInfo = GetMappedInfo(timeOff, EmployeeMappingList, TimeOffTypeList, record);

                            //需檢核格式、時數、正確才建立假單
                            TimeOffCheckResult timeOffCheckResult = CheckTimeOffData(timeOff, mappedInfo, EmployeeMappingList, TimeOffTypeList);
                            if (timeOffCheckResult.Success) //檢查都正確，要存BambooHRLeaveFormRecord跟建立假單
                            {
                                //BambooHREmployeeMapping empMapping = EmployeeMappingList.Where(x => x.BambooHREmployeeID == timeOff.employeeId).FirstOrDefault();
                                BambooHREmployeeMapping empMapping = mappedInfo.EmployeeMapping;

                                if (empMapping != null)
                                {
                                    BambooHRLeaveFormRecord leaveFormRecord = mappedInfo.BambooHRLeaveFormRecord;

                                    leaveFormRecord.PortalLeaveFormID = null;
                                    leaveFormRecord.FormNo = "";
                                    leaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，先給-1
                                    leaveFormRecord.UpdateFrom = "BambooHR"; //此處是更新
                                    leaveFormRecord.UpdateTime = DateTime.Now; //此處是更新
                  
                                    //Portal新增假單，比照原請假單新增程序
                                    string createResult = CreatePortalLeaveForm(leaveFormRecord, timeOffCheckResult.HRMCheckResponse, timeOffCheckResult.MappedInfo);

                                    if (string.IsNullOrWhiteSpace(createResult))
                                    {
                                        //取得最後更新狀態主管，approved是做核決的主管，denied是做退回的主管
                                        string lastManagerBambooHRUserID = timeOff.status.lastChangedByUserId;

                                        Employee lastManager;
                                        if (lastManagerBambooHRUserID == this._setting.APIOwnerUserID || lastManagerBambooHRUserID == this._setting.AdminUserID)
                                        {
                                            lastManager = this._empAdmin; //如果是BambooHR API Owner退件，就視為Portal Admin退的
                                        }
                                        else
                                        {
                                            lastManager = this._services.GetService<BambooHREmployeeMappingService>().GetEmployeeByBambooHRUserID(lastManagerBambooHRUserID);
                                            lastManager = lastManager != null ? lastManager : this._empAdmin; //目前改為找不到退件主管又不是API_Owner，都視為Portal Admin退的
                                        }

                                        //目前核准不論是否找得到核准者都沒關係，因為Portal是用各關卡主管去做核准
                                        if (timeOff.status.status == TimeOffRequestStatus.approved.ToString()) //如果BambooHR是approved狀態，還要繼續將流程所有關卡都簽核完成
                                        {
                                            string approveResult = ApprovePortalSignFlow(timeOff, leaveFormRecord, lastManager);
                                        }
                                        else if (lastManager != null && timeOff.status.status == TimeOffRequestStatus.denied.ToString()) //如果BambooHR是denied狀態，還要繼續做退回與刪單
                                        {
                                            string rejectResult = RejectPortalSignFlow(timeOff, leaveFormRecord, lastManager);
                                        }

                                    }
                                    else
                                    {

                                        //存入BambooHRLeaveFormRecord
                                        leaveFormRecord.PortalLeaveFormID = null;
                                        leaveFormRecord.FormNo = "";
                                        leaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，給-1
                                        leaveFormRecord.UpdateFrom = "BambooHR";
                                        leaveFormRecord.UpdateTime = DateTime.Now;
                                        leaveFormRecord.IsMailSent = true;

                                        //BambooHRStatua狀態直接用timeOff的狀態
                                        leaveFormRecord.BambooHRStatus = timeOff.status.status;

                                        //加上紀錄狀況到BambooHRLeaveFormRecord
                                        string subject = "新增假單檢核失敗(" + timeOff.id + ")";
                                        string body = createResult;
                                        leaveFormRecord.Remark = subject + " " + body;

                                        //建立假單有異常狀況，先不新增BambooHRLeaveFormRecord記錄了，因為建假單第一個動作就是新增Record，沒建立的機率很小
                                        SendBambooHRNotificationMail(empMapping == null ? null : empMapping.Employee, "", "新增假單檢核失敗", createResult, leaveFormRecord, EmployeeMappingList);
                                        
                                        BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                                        lfRecordService.LogInfo = this._logInfo;
                                        if (leaveFormRecord.ID > 0)
                                        {
                                            lfRecordService.Update(leaveFormRecord);
                                        }
                                        else
                                        {
                                            lfRecordService.Create(leaveFormRecord);
                                        }
                                    }
                                }
                            }
                            else //格式或時數有錯誤，還是要新增BambooHRLeaveFormRecord，要發通知
                            {
                                //存入BambooHRLeaveFormRecord
                                mappedInfo.BambooHRLeaveFormRecord.PortalLeaveFormID = null;
                                mappedInfo.BambooHRLeaveFormRecord.FormNo = "";
                                mappedInfo.BambooHRLeaveFormRecord.FormStatus = (int)FormStatus.TempOrEmpty; //此時狀態因為Portal還未建單，給-1
                                mappedInfo.BambooHRLeaveFormRecord.UpdateFrom = "BambooHR"; //此處是更新
                                mappedInfo.BambooHRLeaveFormRecord.UpdateTime = DateTime.Now; //此處是更新
                                mappedInfo.BambooHRLeaveFormRecord.IsMailSent = true;

                                //BambooHRStatua狀態直接用timeOff的狀態
                                mappedInfo.BambooHRLeaveFormRecord.BambooHRStatus = timeOff.status.status;

                                //加上紀錄狀況到BambooHRLeaveFormRecord
                                string subject = "輸入的時間格式或時數錯誤(" + timeOff.id + ")";
                                string body = timeOffCheckResult.CheckResult;
                                mappedInfo.BambooHRLeaveFormRecord.Remark = subject + " " + body;
                                SendBambooHRNotificationMail(mappedInfo.EmployeeMapping == null ? null : mappedInfo.EmployeeMapping.Employee, "", subject, body, mappedInfo.BambooHRLeaveFormRecord, EmployeeMappingList);

                                BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                                lfRecordService.LogInfo = this._logInfo;
                                if (mappedInfo.BambooHRLeaveFormRecord.ID > 0)
                                {
                                    lfRecordService.Update(mappedInfo.BambooHRLeaveFormRecord);
                                }
                                else
                                {
                                    lfRecordService.Create(mappedInfo.BambooHRLeaveFormRecord);
                                }
                            }
                        }

                    }
                }

                sb.Append(timeOff.id + "-" + timeOff.status.status + "-" + (record == null ? "null" : ((FormStatus)record.FormStatus).ToString()) + Environment.NewLine);

            }

            finalResult = sb.ToString();
            return finalResult;
        }

        private string GetStatusDescription(string bambooHRStatus, FormStatus portalStatus)
        {
            string result = "";
            string msgBambooHR = "";
            string msgPortal = "";
            if (bambooHRStatus == TimeOffRequestStatus.requested.ToString())
            {
                msgBambooHR = "簽核中";
            }
            else if (bambooHRStatus == TimeOffRequestStatus.approved.ToString())
            {
                msgBambooHR = "已核准";
            }
            else if (bambooHRStatus == TimeOffRequestStatus.denied.ToString())
            {
                msgBambooHR = "已退回";
            }
            else if (bambooHRStatus == TimeOffRequestStatus.canceled.ToString())
            {
                msgBambooHR = "已取消";
            }
            else if (bambooHRStatus == TimeOffRequestStatus.canceled.ToString())
            {
                msgBambooHR = "已轉單";
            }
            else
            {
                msgBambooHR = "狀態未定義";
            }

            msgBambooHR += "(" + bambooHRStatus + ")";

            switch (portalStatus)
            {
                case FormStatus.Draft:
                    msgPortal = "已退回/拉回";
                    break;
                case FormStatus.Signing:
                    msgPortal = "簽核中";
                    break;
                case FormStatus.Approved:
                    msgPortal = "目前無此狀態";
                    break;
                case FormStatus.Send:
                    msgPortal = "已核准";
                    break;
                case FormStatus.TempOrEmpty:
                    msgPortal = "無之前紀錄";
                    break;

            }

            result = "狀態異常：BambooHR" + msgBambooHR + "，HR Portal" + msgPortal;

            return result;
        }

        /// <summary>
        /// 核准Portal簽核流程，目前是將所有關卡找出來並依序用各關主管身分做簽核同意動作。
        /// </summary>
        /// <param name="LFRecord"></param>
        /// <param name="Approver"></param>
        /// <returns></returns>
        private string ApprovePortalSignFlow(TimeOffRequestQueryResult TimeOffItem, BambooHRLeaveFormRecord LFRecord, Employee Approver)
        {
            string result = "";

            //取得Portal原假單
            if (LFRecord.PortalLeaveFormID.HasValue)
            {
                LeaveForm form = this._services.GetService<LeaveFormService>().GetLeaveFormByIDWithEmployee(LFRecord.PortalLeaveFormID.Value);
                if (form.Status == (int)FormStatus.Signing)
                {
                    //假單檢核，原先是放在簽核流程內每關都要跑一次，但現在是一次完成簽核，所以放到外面只跑一次
                    RequestResult _checkResult = null;
                    string agentNo = LFRecord.AgentEmpID;
                    _checkResult = Task.Run(() => HRMApiAdapter.CheckLeave(LFRecord.CompanyCode, LFRecord.PortalAbsentCode, LFRecord.EmpID,
                            agentNo, form.StartTime, form.EndTime)).Result;

                    if (_checkResult.Status)
                    {
                        //取得該假單所有簽核流程
                        var _signFlowRecQueryHelper = new SignFlowRecQueryHelper(new SignFlowRecRepository());
                        
                        //取得全部流程中還是W狀態的，W表示尚未簽核，要逐筆進行簽核同意動作
                        IList<SignFlowRecModel> _signFlowList = _signFlowRecQueryHelper.GetSignFlowByFormNumber(form.FormNo);
                        List<SignFlowRecModel> waitingFlowList = _signFlowList.Where(x => x.SignStatus == "W").OrderBy(y => y.GroupID).ThenBy(z => z.SignOrder).ToList();
                        if (waitingFlowList.Count > 0)
                        {
                            SignFlowRecModel lastItem = _signFlowList.Last();

                            foreach (SignFlowRecModel signFlowRec in waitingFlowList) 
                            {
                                bool isLastLevel = (signFlowRec.Equals(lastItem));
                                string acceptResult = AcceptFlow_Leave(TimeOffItem, signFlowRec, isLastLevel, LFRecord, form, agentNo);
                            }
                        }
                    }

                }


            }

            return result;
        }

        /// <summary>
        /// 更新流程狀態，參考原Flow Accept處理內容修改
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="isLastLevel"></param>
        /// <param name="Record"></param>
        /// <param name="Form"></param>
        /// <param name="AgentNo"></param>
        /// <returns></returns>
        private string AcceptFlow_Leave(TimeOffRequestQueryResult TimeOffItem, SignFlowRecModel SignFlowRec, bool isLastLevel, BambooHRLeaveFormRecord LFRecord, LeaveForm Form, string AgentNo)
        {
            string result = "";
            RequestResult _checkResult = new RequestResult() { Status = true };

            if (isLastLevel) //核決關卡，新增假單(最後一筆)
            {
                _checkResult = Task.Run(() => HRMApiAdapter.PostLeave(Form.FormNo, LFRecord.CompanyCode, LFRecord.EmpID,
                       Form.StartTime, Form.EndTime, Form.AbsentCode, Form.LeaveReason, AgentNo)).Result;
                if (_checkResult.Status)
                {
                    //更新LeaveForm狀態
                    Form.Status = (int)FormStatus.Send;
                    this._services.GetService<LeaveFormService>().Update(Form);

                    //更新BambooHRLeaveFormRecord狀態
                    LFRecord.BambooHRStatus = TimeOffRequestStatus.approved.ToString();
                    LFRecord.FormStatus = (int)FormStatus.Send;
                    LFRecord.BambooHRManagerNote = TimeOffItem.notes == null ? "" : TimeOffItem.notes.manager;
                    LFRecord.UpdateTime = DateTime.Now;
                    LFRecord.UpdateFrom = "BambooHR"; //由BambooHR觸發的更新

                    BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                    lfRecordService.LogInfo = this._logInfo;
                    lfRecordService.Update(LFRecord);

                    //更新SmartSheet移到此處
                    //SmartSheet檢查部門
                    string SmartSheetDepartments = this._systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                    if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                    {
                        List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                        if (ssDeptList.Contains(Form.Department.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                        {
                            var smartSheetService = new SmartSheetrIntegrationService();
                            string retValue = smartSheetService.Update(Form.FormNo, Form.ID, 3); //核准狀態為3
                            string smartSheetLog = "假單核准更新SmartSheet紀錄：" + retValue;

                            var logService = this._services.GetService<SystemLogService>();
                            logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, smartSheetLog);

                        }
                    }
                }

            }

            if (_checkResult.Status)
            {
                //更新流程狀態為同意
                LeaveSignList _signList = new LeaveSignList();
                //主管意見加上From BambooHR提醒文字
                string managerNote = string.IsNullOrWhiteSpace((TimeOffItem.notes == null ? "" : TimeOffItem.notes.manager)) ? "" : "From BambooHR：" + Environment.NewLine + (TimeOffItem.notes == null ? "" : TimeOffItem.notes.manager);
                _signList.Accept(Form.FormNo, SignFlowRec.ID, SignFlowRec.SignerID, managerNote); //直接用該關主管簽核
            }

            //更新ePortal簽核箱，先跳過

            

            return result;
        }

        /// <summary>
        /// 退回Portal簽核流程，需用退回執行者身分來做退回動作。
        /// </summary>
        /// <param name="LFRecord"></param>
        /// <param name="Rejector"></param>
        /// <returns></returns>
        private string RejectPortalSignFlow(TimeOffRequestQueryResult TimeOffItem, BambooHRLeaveFormRecord LFRecord, Employee Rejector, bool IsBambooHRCanceled = false)
        {
            string result = "";

            //取得Portal原假單
            if (LFRecord.PortalLeaveFormID.HasValue)
            {
                string agentNo = LFRecord.AgentEmpID;

                LeaveFormService leaveFormService = this._services.GetService<LeaveFormService>();
                LeaveForm form = leaveFormService.GetLeaveFormByIDWithEmployee(LFRecord.PortalLeaveFormID.Value);
                if (form.Status == (int)FormStatus.Signing)
                {
                    bool isPullBack = false; //此處是退回不是拉回
                    LeaveSignList _signList = new LeaveSignList();

                    //取得該假單所有簽核流程
                    var _signFlowRecQueryHelper = new SignFlowRecQueryHelper(new SignFlowRecRepository());
                    IList<SignFlowRecModel> _signFlowList = _signFlowRecQueryHelper.GetSignFlowByFormNumber(form.FormNo);

                    string managerNote = TimeOffItem.notes == null ? "" : TimeOffItem.notes.manager;
                    if (IsBambooHRCanceled)
                    {
                        managerNote = "From BambooHR：" + Environment.NewLine + "Time Off Request canceled.";
                    }
                    else
                    {
                        managerNote = string.IsNullOrWhiteSpace(managerNote) ? null : "From BambooHR：" + Environment.NewLine + managerNote;
                    }

                    //取得全部流程中還是W狀態的，W表示尚未簽核，要逐筆與傳入的Rejector比對，不同的要進行核准，相同的進行簽核退回動作，並跳出迴圈
                    List<SignFlowRecModel> waitingFlowList = _signFlowList.Where(x => x.SignStatus == "W").OrderBy(y => y.GroupID).ThenBy(z => z.SignOrder).ToList();
                    if (waitingFlowList.Count > 0)
                    {
                        if (waitingFlowList.Any(x => x.SignerID == Rejector.EmployeeNO) && !IsBambooHRCanceled) //如果BambooHR是取消狀態，不跑這段
                        {
                            foreach (SignFlowRecModel signFlowRec in waitingFlowList)
                            {
                                //如果是BambooHR更新狀態的主管就要進行退回
                                if (signFlowRec.SignerID == Rejector.EmployeeNO)
                                {
                                    _signList.Reject(LFRecord.FormNo, signFlowRec.ID, Rejector.EmployeeNO, managerNote, isPullBack);
                                    break; //跳出迴圈

                                }
                                else //非BambooHR更新狀態的主管，要先核准，因退回可能是第二關主管退的，這樣表示第一關是同意的
                                {
                                    //核准，但不是核決
                                    string acceptResult = AcceptFlow_Leave(TimeOffItem, signFlowRec, false, LFRecord, form, agentNo);
                                }
                            }
                        }
                        else //流程裡面找不到退件主管，目前都直接於第一關退回，身分是admin或是BambooHR退件者
                        {
                            SignFlowRecModel signFlowRec = waitingFlowList.First();
                            if (Rejector.EmployeeNO == DEFAULT_ADMIN_EMPLOYEENO || IsBambooHRCanceled) //如果是API_Owner退的就視為admin於第一關退回，另外如果傳入的BambooHR是取消狀態也跑這段
                            {
                                _signList.Reject(LFRecord.FormNo, signFlowRec.ID, Rejector.EmployeeNO, managerNote, isPullBack);
                            }
                            else //目前如果不是API_Owner退件，又不是流程內主管，還是存入BambooHR退件主管做退件動作
                            {
                                _signList.Reject(LFRecord.FormNo, signFlowRec.ID, Rejector.EmployeeNO, managerNote, isPullBack);
                                //return "退回者非流程主管";
                            }
                        }
                    }
                    else //沒有流程，應該不會有這狀況
                    {
                        return "找不到原申請流程";
                    }

                    //更新ePortal簽核箱，先跳過

                    //更新SmartSheet
                    //SmartSheet檢查部門
                    string SmartSheetDepartments = this._systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                    if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                    {
                        List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                        if (ssDeptList.Contains(form.Department.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                        {
                            var smartSheetService = new SmartSheetrIntegrationService();
                            string retValue = smartSheetService.Delete(form.FormNo, form.ID);
                            var logService = this._services.GetService<SystemLogService>();
                            string smartSheetLog = "假單" + (isPullBack ? "拉回" : "退回") + "刪除SmartSheet紀錄：" + retValue;
                            logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, smartSheetLog);

                        }
                    }

                    //更新請假單狀態
                    form.Status = (int)FormStatus.Draft;
                    leaveFormService.Update(form);

                    //接著要比照原leaveForm刪單動作進行刪單，紀錄更新者為admin，沒有admin就記錄為發動退回的主管的Portal Employee ID
                    Guid rejectorEmployeeID = this._empAdmin != null ? this._empAdmin.ID : Rejector.ID;
                    int _result = leaveFormService.Delete(form, form, rejectorEmployeeID, true);

                    //更新流程的isUsed為N，重新取得一次流程(因為前面的動作可能會造成變化)
                    //不能直接用_signFlowRecQueryHelper的方法，因為會讀出更新前的資料(可能是EF的Cache機制)，改用LoadSigningFlow
                    IList<SignFlowRecModel> _signFlow = _signList.LoadSigningFlow(form.FormNo, DefaultEnum.IsUsed.Y);
                    if (_signFlow != null || _signFlow.Count != 0)
                    {
                        foreach (var flowItem in _signFlow)
                        {
                            flowItem.IsUsed = DefaultEnum.IsUsed.N.ToString();
                            flowItem.DataState = DefaultEnum.SignFlowDataStatus.Modify.ToString();
                        }
                        _signList.SaveSigningFlow(_signFlow, null);
                    }

                    //更新BambooHRLeaveFormRecord狀態
                    LFRecord.BambooHRStatus = IsBambooHRCanceled ? TimeOffRequestStatus.canceled.ToString() : TimeOffRequestStatus.denied.ToString();
                    LFRecord.FormStatus = (int)FormStatus.Draft;
                    LFRecord.BambooHRManagerNote = TimeOffItem.notes == null ? "" : TimeOffItem.notes.manager;
                    LFRecord.UpdateTime = DateTime.Now;
                    LFRecord.UpdateFrom = "BambooHR"; //由BambooHR觸發的更新

                    BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                    lfRecordService.LogInfo = this._logInfo;
                    lfRecordService.Update(LFRecord);

                    //刪除PortalFormDetail明細
                    var taskResult = Task.Run<RequestResult>(async () => await HRMApiAdapter.DeletePortalFormDetail(form.FormNo)).Result;

                }


            }


            return result;
        }

        /// <summary>
        /// BambooHR已經取消，但Portal該假單是核准狀態，要用Admin做銷假流程
        /// </summary>
        /// <returns></returns>
        private string CancelApprovePortalLeaveForm(TimeOffRequestQueryResult TimeOffItem, BambooHRLeaveFormRecord LFRecord, Employee Canceler)
        {
            string result = "";
            string Sender = DEFAULT_ADMIN_EMPLOYEENO; //銷假單送件人都用admin
            Guid SenderGUID = this._empAdmin.ID;

            //取得Portal原假單
            if (LFRecord.PortalLeaveFormID.HasValue)
            {
                string agentNo = LFRecord.AgentEmpID;

                LeaveFormService leaveFormService = this._services.GetService<LeaveFormService>();
                LeaveForm leaveForm = leaveFormService.GetLeaveFormByIDWithEmployee(LFRecord.PortalLeaveFormID.Value);
                if (leaveForm.Status == (int)FormStatus.Send) //確定是核准的才做銷假，其實前面已經檢查過了
                {
                    //新增LeaveCancel銷假單
                    LeaveCancel cancelForm = new LeaveCancel()
                    {
                        ID = Guid.NewGuid(),
                        FormNo = GetFormNo(),
                        Status = (int)FormStatus.Draft,
                        LeaveFormID = leaveForm.ID,
                        CancelReason = "因BambooHR假單已取消，HR Portal由系統Admin進行銷假",
                        Createdby = SenderGUID,
                        CreatedTime = DateTime.Now
                    };

                    LeaveCancelService leaveCancelService = this._services.GetService<LeaveCancelService>();
                    int cResult = leaveCancelService.Create(cancelForm);

                    //比照Portal銷假單，複製原假單流程
                    LeaveCancelSignList _signList = new LeaveCancelSignList();
                    IList<SignFlowRecModel> _signFlow = _signList.CopyFlow(cancelForm.FormNo, leaveForm.FormNo, Sender);
                    _signList.SaveSigningFlow(_signFlow, null);
                    string _senderFlowId = _signFlow[0].ID;

                    _signList.Accept(cancelForm.FormNo, _senderFlowId, Sender, string.Empty);

                    //更新銷假單狀態為簽核中
                    cancelForm.Status = (int)FormStatus.Signing;
                    leaveCancelService.Update(cancelForm);

                    //建立銷假單後，要將後面流程都核准
                    //取得銷假單所有流程，用之前取得的就可以，除了第一筆送件人的不用更新外，其他全部都要用Admin核准
                    //先檢查能否刪後台的假單
                    DeleteResponse _deleteResponse = Task.Run(() => HRMApiAdapter.CheckDeleteLeave(LFRecord.CompanyCode, LFRecord.EmpID, LFRecord.FormNo)).Result;
                    if (_deleteResponse.isLocked) 
                    {
                        //要銷假已經考勤鎖定，目前BambooHR那邊都是銷未來的假單，應該不會有這狀況

                    }
                    else if (_deleteResponse.Status) //檢核成功
                    {
                        for (int i = 1; i < _signFlow.Count; i++)
                        {
                            bool isLastLevel = (i == _signFlow.Count - 1);
                            string aResult = AcceptFlow_CancelLeave(TimeOffItem, _signFlow[i], isLastLevel, LFRecord, cancelForm);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 核准銷假單流程，參考原Flow Accept處理內容修改
        /// </summary>
        /// <returns></returns>
        private string AcceptFlow_CancelLeave(TimeOffRequestQueryResult TimeOffItem, SignFlowRecModel SignFlowRec, bool isLastLevel, BambooHRLeaveFormRecord LFRecord, LeaveCancel cancelForm)
        {
            string result = "";
            string Signer = DEFAULT_ADMIN_EMPLOYEENO; //銷假單目前都是由Admin核准
            RequestResult _checkResult = new RequestResult() { Status = true };

            if (isLastLevel) //核決關卡，刪除後台假單(最後一筆)
            {
                //刪除後台假單
                DeleteResponse _deleteResponse = Task.Run(() => HRMApiAdapter.DeleteLeave(LFRecord.CompanyCode, LFRecord.EmpID, LFRecord.FormNo)).Result;

                //更新SmartSheet
                //檢查部門
                SystemSettingService systemSettingService = this._services.GetService<SystemSettingService>();
                string SmartSheetDepartments = systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                {
                    LeaveForm leaveForm = LFRecord.LeaveForm;
                    List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                    if (ssDeptList.Contains(leaveForm.Department.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                    {
                        var smartSheetService = new SmartSheetrIntegrationService();
                        string retValue = smartSheetService.Delete(leaveForm.FormNo, leaveForm.ID);
                        var logService = this._services.GetService<SystemLogService>();
                        string smartSheetLog = "銷假刪除SmartSheet紀錄：" + retValue;
                        logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, smartSheetLog);
                    }
                }

                //更新銷假單狀態
                cancelForm.Status = (int)FormStatus.Send;
                this._services.GetService<LeaveCancelService>().Update(cancelForm);

                //更新BambooHRLeaveFormRecord狀態
                LFRecord.BambooHRStatus = TimeOffRequestStatus.canceled.ToString();
                LFRecord.FormStatus = (int)FormStatus.Draft; //雖然銷假不會改原請假單的狀態，但BambooHRLeaveFormRecord這邊還是要改，之後就不會再處理這張
                //LFRecord.BambooHRManagerNote = TimeOffItem.notes == null ? "" : TimeOffItem.notes.manager;
                LFRecord.UpdateTime = DateTime.Now;
                LFRecord.UpdateFrom = "BambooHR"; //由BambooHR觸發的更新

                BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
                lfRecordService.LogInfo = this._logInfo;
                lfRecordService.Update(LFRecord);
            }

            //不論是否為最後一關，都要更新SignFlowRec簽核流程
            LeaveCancelSignList _signList = new LeaveCancelSignList();
            string managerNote = "因BambooHR假單已取消，HR Portal由系統Admin進行銷假";
            _signList.Accept(cancelForm.FormNo, SignFlowRec.ID, Signer, managerNote); //直接用該關主管簽核

            return result;

        }

           

        //檢查Time Off Request資料是否合理
        //MappedInfo改外部傳入，因為即使錯誤因為要存紀錄也要取得一次，就直接在外面先處理
        private TimeOffCheckResult CheckTimeOffData(TimeOffRequestQueryResult item, TimeOffRequestMappedInfo MappedInfo, List<BambooHREmployeeMapping> EmployeeMappingList = null, List<BambooHRTimeOffType> TimeOffTypeList = null)
        {
            TimeOffCheckResult result = new TimeOffCheckResult() { checkItem = item };
            //LeaveFormRecord = new BambooHRLeaveFormRecord();

            //檢查時數
            double leaveAmount;
            if (!double.TryParse(item.amount.amount, out leaveAmount))
            {
                result.Success = false;
                result.CheckResult = "請假時數非數字"; //應該不會有這個狀況
            }

            string inputAmount = item.amount.amount;
            string inputUnit = item.amount.unit;

            //暫定BambooHR假別單位都是小時
            DateTime bambooHR_StartDate = MappedInfo.BambooHRLeaveFormRecord.BambooHRStartDate.Value;
            DateTime bambooHR_EndDate = MappedInfo.BambooHRLeaveFormRecord.BambooHREndDate.Value;
            DateTime bambooHR_StartTime, bambooHR_EndTime;
            if (leaveAmount % WORKHOURS_PER_DAY == 0) //如果時數剛好是整天，先補上時間，後面再做時數檢查
            {
                bambooHR_StartTime = bambooHR_StartDate.AddHours(DEFAULT_START_HOUR);
                bambooHR_EndTime = bambooHR_EndDate.AddHours(DEFAULT_END_HOUR);
            }
            else //非整天，要檢查註解裡面是否有輸入正確時間格式
            {
                string note = item.notes == null ? "" : item.notes.employee;
                InputTimeinNote timeResult = ExtractTimeFromNote(note, bambooHR_StartDate, bambooHR_EndDate);
                if (!timeResult.IsFormatCorrect)
                {
                    result.Success = false;
                    result.CheckResult = "輸入時間格式錯誤";
                    return result;
                }
                else
                {
                    //更新時間為整理後結果
                    bambooHR_StartTime = timeResult.ExtractedStartTime;
                    bambooHR_EndTime = timeResult.ExtractedEndTime;
                }

            }

            //檢查起訖時間順序
            if (bambooHR_EndTime <= bambooHR_StartTime)
            {
                result.Success = false;
                result.CheckResult = "請假結束時間必須大於開始時間";
                return result;
            }

            //檢查輸入時數是否與請假時間區間相符
            //由BambooHR假單整理出所需資料，同時設定BambooHRLeaveFormRecord部分資料-->改到外部先處理完再傳入
            //TimeOffRequestMappedInfo mappedInfo = GetMappedInfo(item, EmployeeMappingList, TimeOffTypeList);

            //檢查是否Portal已經有同時段假單，這時候才有Portal Employee資料
            if (MappedInfo.Employee != null && this._services.GetService<LeaveFormService>().CheckLeaveFormExist(bambooHR_StartTime, bambooHR_EndTime, MappedInfo.Employee.ID))
            {
                result.Success = false;
                result.CheckResult = "已有此區間假單";
                return result;
            }

            result.MappedInfo = MappedInfo;

            //LeaveFormRecord = MappedInfo.BambooHRLeaveFormRecord;
            //MappedInfo.BambooHRLeaveFormRecord.BambooHRStartDate = bambooHR_StartDate; //移到GetMappedInfo內設定
            //MappedInfo.BambooHRLeaveFormRecord.BambooHREndDate = bambooHR_EndDate; //移到GetMappedInfo內設定
            MappedInfo.BambooHRLeaveFormRecord.BambooHRConfirmedStartTime = bambooHR_StartTime;
            MappedInfo.BambooHRLeaveFormRecord.BambooHRConfirmedEndTime = bambooHR_EndTime;

            if (MappedInfo.Employee != null)
            {
                //計算時數
                BambooHRCheckLeaveResponse hrmResult = Task.Run(() => HRMApiAdapter.BambooHRCheckAbsentAmount(MappedInfo.Employee.Company.CompanyCode, MappedInfo.TimeOffTypeMapping.HRM_AbsentCode, MappedInfo.Employee.EmployeeNO, MappedInfo.Employee.Employee_ID.Value, bambooHR_StartTime, bambooHR_EndTime, inputAmount, inputUnit)).Result;
                if (hrmResult.Status)
                {
                    result.Success = true;
                    result.CheckResult = "";
                    result.ConfirmedStartTime = bambooHR_StartTime;
                    result.ConfirmedEndTime = bambooHR_EndTime;
                    result.HRMCheckResponse = hrmResult;
                }
                else
                {
                    result.Success = false;
                    //要根據hrmResult的CheckStatus來決定，先用Note
                    //string description = "";
                    result.CheckResult = hrmResult.Note;

                }

            }
            else
            {
                result.Success = false;
                result.CheckResult = "查無此員工";
            }

            return result;
        }

        private InputTimeinNote ExtractTimeFromNote(string Note, DateTime InputStartDate, DateTime InputEndDate)
        {
            InputTimeinNote extractResult = new InputTimeinNote()
            {
                IsFormatCorrect = false,
                InputNote = Note,
                ExtractedTimeStr = ""

            };

            //時間分隔符號預設-與~都可以
            char[] delimiters = new char[] { '-', '~' };
            TimePeriod timeResult;

            if (!string.IsNullOrWhiteSpace(Note))
            {
                if (CheckTimeFormat(Note, delimiters, out timeResult))
                {
                    //組合出正確時間
                    DateTime extractedStartTime = new DateTime(InputStartDate.Year, InputStartDate.Month, InputStartDate.Day, timeResult.StartTime.Hour, timeResult.StartTime.Minute, 0);
                    DateTime extractedEndTime = new DateTime(InputEndDate.Year, InputEndDate.Month, InputEndDate.Day, timeResult.EndTime.Hour, timeResult.EndTime.Minute, 0);

                    extractResult.IsFormatCorrect = true;
                    extractResult.ExtractedTimeStr = timeResult.ExtractedStartTimeString + " " + timeResult.ExtractedEndTimeString;
                    extractResult.ExtractedStartTime = extractedStartTime;
                    extractResult.ExtractedEndTime = extractedEndTime;
                }
            }

            return extractResult;

        }

        //檢查時間格式
        //am、pm檢核時直接忽略，所以1am與1pm結果是一樣的
        //第一種格式9:00-12:00、13:00-15:00，固定用24小時制，但小時的地方若為上午則允許只有一碼，下午固定要輸入兩碼
        //第二種格式9-12，8-11，2-4，3-6，13-15，只有小時，1~7 一律視為下午時間，8~11一律為上午時間，12一律為中午十二點，0視為錯誤，超過12就是24小時制

        private bool CheckTimeFormat(string Note, char[] Delimiters, out TimePeriod TimeResult)
        {
            bool result = false;

            TimeResult = new TimePeriod()
            {
                StartTime = new TimePair(),
                EndTime = new TimePair(),
                ExtractedStartTimeString = "",
                ExtractedEndTimeString = ""
            };

            //先去掉am pm等字串
            string removePattern="[aApP][mM]";
            Note = Regex.Replace(Note, removePattern, "");

            //string pattern1 = @"\d{4}$";
            //string pattern2 = @"^\d{4}";
         

            foreach (char item in Delimiters)
            {
                string[] s = Note.Split(item).Select(x => x.Trim()).ToArray();
                if (s.Length > 1)
                {
                   
                    string s1 = s[0].Substring(Math.Max(0, s[0].Length - 5)); //取開始時間的字串末尾5碼
                    string s2 = s[1].Substring(0, Math.Min(s[1].Length, 5)); //取結束時間的字串最前面5碼

                    int hour1, minute1, hour2, minute2;
                    hour1 = hour2 = minute1 = minute2 = 0; //先全部設定為0

                    if (s1 != s2)
                    {
                        //先驗證第一種格式
                        string pattern1 = @"\d{1,2}:\d{2}$";
                        string pattern2 = @"^\d{1,2}:\d{2}";
                        Regex r1 = new Regex(pattern1);
                        Regex r2 = new Regex(pattern2);

                        Match m1 = r1.Match(s1);
                        Match m2 = r2.Match(s2);

                        bool isPassed = false;
                        if (m1.Success && m2.Success)
                        {
                            string[] a1 = m1.Value.Split(':');
                            string[] a2 = m2.Value.Split(':');
                            hour1 = int.Parse(a1[0]);
                            minute1 = int.Parse(a1[1]);
                            hour2 = int.Parse(a2[0]);
                            minute2 = int.Parse(a2[1]);

                            //20210312 Daniel 格式一的小時部分，也比照格式二做轉換
                            hour1 = TransformHour(hour1);
                            hour2 = TransformHour(hour2);
                            
                            if (hour1 != 0 && hour2 != 0)
                            {
                                isPassed = true;
                            }

                            //isPassed = true;

                        }
                        else //驗證另一種格式 8-12、1-5
                        {
                           
                            pattern1 = @"[\d]{1,2}$";
                            pattern2 = @"^[\d]{1,2}";
                            r1 = new Regex(pattern1);
                            r2 = new Regex(pattern2);

                            m1 = r1.Match(s1);
                            m2 = r2.Match(s2);

                            if (m1.Success && m2.Success)
                            {
                                hour1 = TransformHour(int.Parse(m1.Value));
                                hour2 = TransformHour(int.Parse(m2.Value));

                                //此格式分鐘部分一律是0
                                minute1 = 0;
                                minute2 = 0;

                                if (hour1 != 0 && hour2 != 0)
                                {
                                    isPassed = true;
                                }
                            }
                        }

                        if (isPassed && hour1 < 24 && minute1 < 60 && hour2 < 24 && minute2 < 60 && (hour1 != hour2 || minute1 != minute2))
                        {
                            
                            //整理回傳結果固定格式為HHmm
                            TimeResult.StartTime.Hour = hour1;
                            TimeResult.StartTime.Minute = minute1;
                            TimeResult.EndTime.Hour = hour2;
                            TimeResult.EndTime.Minute = minute2;
                            TimeResult.ExtractedStartTimeString = s1;
                            TimeResult.ExtractedEndTimeString = s2;

                            result = true;
                            break; //有一種分隔符號驗證正確就跳出迴圈

                        }
                    }
                }
            }

            return result;
        }

        private int TransformHour(int hour)
        {
            int result = 0;
            if (hour >= 1 && hour <= 7) //1~7一律視為下午時間
            {
                result = hour + 12;
            }
            else if (hour >= 8 && hour <= 23) //8~11一律視為上午時間，12一律為中午十二點，超過12就是24小時制
            {
                result = hour;
            }

            //0與24以上的數字都是錯的，直接回0

            return result;
        }

        private TimeOffRequestMappedInfo GetMappedInfo(TimeOffRequestQueryResult item, List<BambooHREmployeeMapping> EmployeeMappinglist, List<BambooHRTimeOffType> TimeOffTypeList, BambooHRLeaveFormRecord Record = null)
        {
            BambooHREmployeeMapping mapping = this._services.GetService<BambooHREmployeeMappingService>().GetMappingByBambooHREmployeeID(item.employeeId, EmployeeMappinglist);
            Employee employee = mapping != null ? mapping.Employee : null;

            BambooHRTimeOffType timeOffTypeMapping = TimeOffTypeList.Where(x => x.BambooHR_TimeOffTypeID == item.type.id).FirstOrDefault();

            CultureInfo culture = CultureInfo.InvariantCulture;
            DateTime bambooHRStartDate=DateTime.ParseExact(item.start, "yyyy-MM-dd", culture);
            DateTime bambooHREndDate=DateTime.ParseExact(item.end, "yyyy-MM-dd", culture);
            decimal bambooHRLeaveAmount=decimal.Parse(item.amount.amount);
            string bambooHRLeaveReason=item.notes == null ? "" : item.notes.employee;

            if (Record == null)
            {   
                Record = new BambooHRLeaveFormRecord()
                {
                    BambooHRTimeOffID = item.id,
                    EmpData_ID = employee == null ? null : employee.Employee_ID,
                    EmpID = employee == null ? "" : employee.EmployeeNO, //EmpID在Table裡面不能存null，改為空字串
                    BambooHREmployeeID = item.employeeId,
                    BambooHRStatus = item.status.status,
                    PortalEmployeeID = employee == null ? (Guid?)null : employee.ID,
                    PortalAbsentCode = timeOffTypeMapping == null ? "" : timeOffTypeMapping.HRM_AbsentCode,
                    PortalAbsentUnit = timeOffTypeMapping == null ? "" : timeOffTypeMapping.HRM_AbsentUnit,
                    AgentEmpID = mapping == null ? null : mapping.DefaultAgentEmpID,
                    BambooHRStartDate = bambooHRStartDate,
                    BambooHREndDate = bambooHREndDate,
                    BambooHRLeaveAmount = bambooHRLeaveAmount,
                    BambooHRLeaveReason = bambooHRLeaveReason,
                    CompanyCode = employee == null ? null : employee.Company.CompanyCode,
                    IsMailSent = false
                };
            }
            else //有傳入之前的紀錄，表示只要更新(BambooHR管理者改之前已經核准假單資料時會用到)，預設不會改假別
            {
                Record.BambooHRStartDate = bambooHRStartDate;
                Record.BambooHREndDate = bambooHREndDate;
                Record.BambooHRLeaveAmount = bambooHRLeaveAmount;
                Record.BambooHRLeaveReason = bambooHRLeaveReason;
                Record.IsMailSent = false;
                Record.Remark = "";
            }

            return new TimeOffRequestMappedInfo()
            {
                QueryResult = item,
                Employee = employee,
                EmployeeMapping = mapping,
                TimeOffTypeMapping = timeOffTypeMapping,
                BambooHRLeaveFormRecord = Record
            };
        }

        /// <summary>
        /// 依據原來作業方式建立一筆Portal LeaveForm，BambooHRLeaveFormRecord也會一並建立或更新
        /// </summary>
        /// <param name="LeaveFormRecord"></param>
        /// <param name="EmpAbsentCheckDetailList"></param>
        /// <returns></returns>
        private string CreatePortalLeaveForm(BambooHRLeaveFormRecord LeaveFormRecord, BambooHRCheckLeaveResponse HRMResult, TimeOffRequestMappedInfo MappedInfo)
        {
            string result = "";

            //先存入一筆BambooHRLeaveFormRecord，之後Portal完成新增再更新PortalLeaveFormID
            BambooHRLeaveFormRecordService lfRecordService = this._services.GetService<BambooHRLeaveFormRecordService>();
            lfRecordService.LogInfo = this._logInfo;

            if (LeaveFormRecord.ID > 0) //之前已經有資料的(這是BambooHR管理者修改已核准假單時才會有這狀況)
            {
                lfRecordService.Update(LeaveFormRecord);
            }
            else
            {
                lfRecordService.Create(LeaveFormRecord);
            }

            //新增LeaveForm
            //先換算請假時數單位
            double leaveAmount = HRMResult.SystemUnit == "d" ? HRMResult.SystemAbsentHours / (double)WORKHOURS_PER_DAY : HRMResult.SystemAbsentHours;
            DateTime now = DateTime.Now;
            decimal workhours = (decimal)((LeaveFormRecord.BambooHRConfirmedEndTime.Value.Date - LeaveFormRecord.BambooHRConfirmedStartTime.Value.Date).TotalDays * WORKHOURS_PER_DAY); //Portal原來的算法就是請假區間每天的標準工時加總，但不排除假日
            workhours = workhours < WORKHOURS_PER_DAY ? WORKHOURS_PER_DAY : workhours;

            //找代理人
            LeaveForm lf = new LeaveForm()
            {
                ID = Guid.NewGuid(),
                FormNo = GetFormNo(),
                Status = (int)FormStatus.Draft,
                EmployeeID = LeaveFormRecord.PortalEmployeeID.Value, //如果BambooHR對不到Portal員工，前面就會擋掉了，不會進到這邊
                CompanyID = MappedInfo.Employee.CompanyID,
                DepartmentID = MappedInfo.Employee.SignDepartmentID,
                AbsentCode = LeaveFormRecord.PortalAbsentCode,
                AbsentAmount = (decimal)HRMResult.CanUseCount,
                AbsentUnit = LeaveFormRecord.PortalAbsentUnit,
                StartTime = LeaveFormRecord.BambooHRConfirmedStartTime.Value,
                EndTime = LeaveFormRecord.BambooHRConfirmedEndTime.Value,
                LeaveAmount = (decimal)leaveAmount,
                AfterAmount = (decimal)(HRMResult.UsedCount - leaveAmount),
                LeaveReason = LeaveFormRecord.BambooHRLeaveReason,
                AgentID = MappedInfo.EmployeeMapping == null ? null : MappedInfo.EmployeeMapping.DefaultAgentPortalEmployeeID,
                FilePath = null,
                FileName = null,
                FileFormat = null,
                IsAbroad = false, //是否出國，目前先存false，因為沒有來源
                Createdby = LeaveFormRecord.PortalEmployeeID.Value,//建立人(用申請人)
                CreatedTime = now,
                Modifiedby = LeaveFormRecord.PortalEmployeeID.Value,
                ModifiedTime = now,
                IsDeleted = false,
                Deletedby = null,
                DeletedTime = null,
                WorkHours = workhours //Portal原來的算法就是請假區間每天的標準工時加總，但不排除假日

            };

            LeaveFormService leaveFormService = this._services.GetService<LeaveFormService>();
            int isSuccess = leaveFormService.Create(lf);
            //讀取一次Navigation Properties
            LeaveForm lfNow = leaveFormService.Where(x => x.ID == lf.ID).Include("Employee").Include("Company").Include("Department").FirstOrDefault();
            
            //存入檢核明細到後台的PortalFormDetail表格
            if (isSuccess == 1)
            {
                try
                {
                    RequestResult _portalFormSaveResult = Task.Run(() => HRMApiAdapter.SavePortalFormDetail(
                                                                                lfNow.ID.ToString(),
                                                                                lfNow.FormNo,
                                                                                LeaveFormRecord.EmpData_ID.Value,
                                                                                LeaveFormRecord.EmpID,
                                                                                lfNow.StartTime,
                                                                                lfNow.EndTime,
                                                                                lfNow.AbsentUnit,
                                                                                WORKHOURS_PER_DAY, //WorkHours傳進去基本上不會用到
                                                                                Convert.ToDouble(lfNow.LeaveAmount),
                                                                                HRMResult.EmpAbsentCheckDetailList,
                                                                                LeaveFormRecord.EmpID
                                                                                )).Result;
                    isSuccess = _portalFormSaveResult.Status ? 1 : 0;
                }
                catch (Exception ex) //儲存PortalFormDetail錯誤，原先是直接中斷
                {
                    isSuccess = 0;
                    result = "儲存PortalFormDetail發生錯誤，錯誤訊息：" + ex.Message;
                }

            }
           
            if (isSuccess == 1) //處理簽核流程
            {
                //取得假別中英文名稱，直接從對照檔取得，因為對照檔是人工建立，資料不應該有錯
                BambooHRTimeOffType timeOffType = this._services.GetService<BambooHRTimeOffTypeService>().Where(x => x.HRM_AbsentCode == lfNow.AbsentCode).FirstOrDefault();
                string absentName = "";
                string absentNameEN = "";
                if (timeOffType != null)
                {
                    absentName = timeOffType.HRM_AbsentName;
                    absentNameEN = timeOffType.HRM_AbsentNameEN;
                }

                LeaveFormData _formData = new LeaveFormData(lf);
                LeaveSignList _signList = new LeaveSignList();
                SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();
                string _senderFlowId;
                IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(lfNow.FormNo);

                if (_signFlow == null || _signFlow.Count == 0) //沒建立過流程，取得流程並儲存於SignFlowRec
                {
                    try
                    {
                        _signFlow = _signList.GetDefaultSignList(_formData, true);
                        _signList.SaveSigningFlow(_signFlow, null);
                        _senderFlowId = _signFlow[0].ID;

                        SendMailToAgent(lfNow, _formData.AgentNo, _formData.CUser, MappedInfo.Employee, absentName, absentNameEN);
                    }
                    catch (Exception ex)
                    {
                        result = ex.Message;
                        return result;
                    }
                }
                else
                {
                    _senderFlowId = _signFlow.Last().ID;
                    string _lastOrder = _signFlow.Last().SignOrder;
                    decimal _groupId = _signFlow.Last().GroupID;
                    foreach (SignFlowRecModel _flowItem in _signList.GetDefaultSignList(_formData))
                    {
                        _flowItem.GroupID = _groupId;
                        _signFlow.Add(_flowItem);
                    }
                    _signList.SaveSigningFlow(_signFlow, _lastOrder);

                    SendMailToAgent(lfNow, _formData.AgentNo, _formData.CUser, MappedInfo.Employee, absentName, absentNameEN);
                }

                //送出假單更新簽核流程
                _signList.Accept(_formData.FormNumber, _senderFlowId, MappedInfo.Employee.EmployeeNO, string.Empty);

                //指定收件人待確認是否要做

                //ePortal待確認是否要做，先不做

                //SmartSheet整合            
                //先檢查部門
                string SmartSheetDepartments = this._systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                {
                    List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                    if (ssDeptList.Contains(MappedInfo.Employee.SignDepartment.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                    {
                        var smartSheetService = new SmartSheetrIntegrationService();
                        string smartSheetResult = smartSheetService.Create(lfNow, absentName, absentNameEN, (decimal)HRMResult.CanUseCount);
                        string smartSheetLog = "假單申請新增SmartSheet紀錄：" + smartSheetResult;
                        
                        var logService = this._services.GetService<SystemLogService>();
                        logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, smartSheetLog);

                    }
                }

                //上述步驟完成之後。更新LeaveForm與BambooHRLeaveFormRecord
                lfNow.Status = (int)FormStatus.Signing;
                leaveFormService.Update(lfNow);

                LeaveFormRecord.PortalLeaveFormID = lfNow.ID;
                LeaveFormRecord.FormNo = lfNow.FormNo;
                LeaveFormRecord.FormStatus = (int)FormStatus.Signing;
                LeaveFormRecord.PortalStartTime = lfNow.StartTime;
                LeaveFormRecord.PortalEndTime = lfNow.EndTime;
                LeaveFormRecord.PortalLeaveAmount = lfNow.LeaveAmount;
                LeaveFormRecord.PortalLeaveReason = lfNow.LeaveReason;

                lfRecordService.Update(LeaveFormRecord);
            }

            return result;
        }

        /// <summary>
        /// 比照LeaveFormController，修改寄送代理人信件函式
        /// </summary>
        public void SendMailToAgent(LeaveForm LF, string agentNo, string cUser, Employee Emp, string AbsentName, string AbsentNameEN)
        {
            //特定人員，不需要代理人，直接跳過 ???
            if (Emp.EmployeeNO == "00630" || Emp.EmployeeNO == "00733" || Emp.EmployeeNO == "01189" || Emp.EmployeeNO == "00242" || Emp.EmployeeNO == "01034" || Emp.EmployeeNO == "01098")
            {
            }
            else
            {
                //抓取假單資料
                //LeaveForm _form = this._services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);

                //抓取假別資料，修改原先寫法，原先是撈取所有假單再找假別??
                //改為由外部傳入

                //抓取代理人編號
                Employee _agent = null;
                _agent = this._services.GetService<EmployeeService>().GetEmployeeByEmpNo(Emp.CompanyID, agentNo);

                if (_agent != null)
                {
                    //代理人姓名
                    //var _agentName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == agentNo).Select(x => x.EmployeeName).FirstOrDefault();
                    string _agentName = _agent == null ? "" : _agent.EmployeeName;
                    string _agentNameEN = _agent == null ? "" : _agent.EmployeeEnglishName;

                    //申請人姓名
                    string userName = Emp.EmployeeName;
                    string userNameEN = Emp.EmployeeEnglishName;

                    //中英文併列ma
                    string _body = (userName + "於" + "(" + AbsentName + "(" + LF.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + LF.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + LF.LeaveAmount.ToString("0.#") + " " + (LF.AbsentUnit == "h" ? Resource.ResourceManager.GetString("Hour", CultureInfo.GetCultureInfo("zh-TW")) : Resource.ResourceManager.GetString("Day", CultureInfo.GetCultureInfo("zh-TW"))) + ")" + ")" + "間因請假，已設定" + _agent.EmployeeName + "為代理人，特此通知。");
                    string _bodyEN = "<br/>　<br/>" + (userNameEN + " (" + AbsentNameEN + "(" + LF.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + LF.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + LF.LeaveAmount.ToString("0.#") + " " + (LF.AbsentUnit == "h" ? Resource.ResourceManager.GetString("Hour", CultureInfo.GetCultureInfo("en-US")) : Resource.ResourceManager.GetString("Day", CultureInfo.GetCultureInfo("en-US"))) + ")" + ")" + " is on leave, substitute would be " + _agent.EmployeeEnglishName + ".");

                    List<string> _rcpt = new List<string>();
                    _rcpt.Add(_agent.Email);
                    string _subject = userName + "指定" + _agentName + "為代理人通知";
                    string _subjectEN = " Notice of Assigned designee (from " + userNameEN + ")";
                    SendMail(_rcpt.ToArray(), null, null, _subject + _subjectEN, _body + _bodyEN, true);

                }
            }

            return;
        }

        public void SendMail(string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            string _fromMail = this._systemSettingService.GetSettingValue("BambooHR_NoticeEmailAddress");
            this._services.GetService<MailMessageService>().CreateMail(_fromMail, rcpt, cc, bcc, subject, body, isHtml);
        }

        //寄送BambooHR處理過程中間發生異常狀況的通知信件
        private void SendBambooHRNotificationMail(Employee Emp, string ManagerEmpID, string Subject, string Body, BambooHRLeaveFormRecord LFRecord, List<BambooHREmployeeMapping> EmployeeMappingList)
        {
            if (Emp != null)
            {
                //依照寄件人設定內的類別寄信，BambooHRMailNotificationSetting表格有設定幾筆就會寄幾次
                foreach (BambooHRMailNotificationSetting item in this._mailSettings)
                {
                    string[] address = GetTargetMailAddresses(item, Emp, ManagerEmpID, LFRecord, EmployeeMappingList); //回傳陣列第一個是收件人，第二個是副本
                    if (!string.IsNullOrWhiteSpace(address[0])) //必須要有收件人才會寄信
                    {
                        string[] rcpt = address[0].Split(';');
                        string[] cc = address[1].Split(';');
                        SendMail(rcpt, cc, null, Subject, Body, item.IsHTML);
                    }
                }
            }
        }

        private string[] GetTargetMailAddresses(BambooHRMailNotificationSetting item, Employee Emp, string ManagerEmpID, BambooHRLeaveFormRecord LFRecord, List<BambooHREmployeeMapping> EmployeeMappingList)
        {
            string[] result = { "", "" }; //陣列第一個是收件人，第二個是副本

            Employee manager = null;
            Employee agent = null;
            EmployeeService empService = this._services.GetService<EmployeeService>();

            string address = "";

            //依照寄件人設定內的類別寄信
            for (int i = 0; i < result.Length; i++)
            {
                //依據RcptType與CCType來找Address
                int type = i == 0 ? item.RcptType : item.CCType;
                switch ((BambooHRMailRcptType)type)
                {
                    case BambooHRMailRcptType.NoRcpt:

                        address = "";

                        break;
                    case BambooHRMailRcptType.Applicant: //申請人

                        address = Emp.Email;

                        break;
                    case BambooHRMailRcptType.Manager: //主管

                        if (manager == null)
                        {
                            if (string.IsNullOrWhiteSpace(ManagerEmpID)) //沒輸入主管工號就要去找原始流程
                            {
                                LeaveSignList _signList = new LeaveSignList();
                                //因LeaveForm可能根本沒建立(因各種問題)，所以這邊為了要找預設流程可先暫時用Dummy資料
                                var _formData = new LeaveFormData()
                                {
                                    FormNumber = "***", //虛擬表單編號
                                    FormType = FormType.Leave.ToString(),
                                    EmployeeNo = Emp.EmployeeNO,
                                    DeptCode = Emp.SignDepartment.DepartmentCode,
                                    CompanyCode = Emp.Company.CompanyCode,
                                    AbsentCode = LFRecord.PortalAbsentCode,
                                    StartTime = LFRecord.BambooHRConfirmedStartTime.HasValue ? LFRecord.BambooHRConfirmedStartTime.Value : DateTime.Now.Date.AddHours(DEFAULT_START_HOUR),
                                    EndTime = LFRecord.BambooHRConfirmedEndTime.HasValue ? LFRecord.BambooHRConfirmedEndTime.Value : DateTime.Now.Date.AddHours(DEFAULT_END_HOUR),
                                    LeaveAmount = LFRecord.PortalLeaveAmount.HasValue ? LFRecord.PortalLeaveAmount.Value : WORKHOURS_PER_DAY,
                                    AfterAmount = 0,
                                    IsAbroad = false, //目前BambooHR無法輸入出國與否
                                    AgentNo = LFRecord.AgentEmpID,
                                    WorkHours = WORKHOURS_PER_DAY,
                                    AbsentUnit = LFRecord.PortalAbsentUnit
                                };

                                _signList._tempFormData = _formData;
                                try
                                {
                                    IList<SignFlowRecModel> _signFlow = _signList.GetDefaultSignListWithoutCondition(_formData, true);
                                    if (_signFlow.Count > 1) //第一筆是申請人
                                    {
                                        ManagerEmpID = _signFlow[1].SignerID; //取第一關主管
                                    }
                                }
                                catch (Exception ex) //如找不到流程之類的錯誤，暫時不需要處理，只是會少寄Mail
                                {
 
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(ManagerEmpID)) //還是要再檢查一次有沒有主管工號
                            {
                                var managerMapping = EmployeeMappingList.Where(x => x.EmpID == ManagerEmpID).FirstOrDefault();

                                if (managerMapping != null && managerMapping.Employee != null)
                                {
                                    manager = managerMapping.Employee;
                                }
                                else
                                {
                                    manager = empService.GetEmployeeByEmpNo(ManagerEmpID);
                                }
                            }
                        }

                        if (manager != null)
                        {
                            address = manager.Email;
                        }

                        break;
                    case BambooHRMailRcptType.Custom: //自訂收件人
                        address = i == 0 ? item.CustomRcpt : item.CustomCC;

                        break;
                    case BambooHRMailRcptType.Agent: //代理人
                        if (agent == null)
                        {
                            agent = getEmpMappingAgent(Emp.EmployeeNO, EmployeeMappingList, empService);
                        }

                        if (agent != null)
                        {
                            address = agent.Email;
                        }
                        break;
                }

                result[i] = address;
            }

            return result;
        }

        private Employee getEmpMappingAgent(string EmployeeNO, List<BambooHREmployeeMapping> EmployeeMappingList, EmployeeService EmpService)
        {
            Employee agent = null;
            var empMapping = EmployeeMappingList.Where(x => x.EmpID == EmployeeNO).FirstOrDefault();
            if (empMapping != null)
            {
                if (empMapping.DefaultAgentPortalEmployeeID.HasValue)
                {
                    agent = EmpService.Where(x => x.ID == empMapping.DefaultAgentPortalEmployeeID.Value).FirstOrDefault();
                }
            }
            return agent;
        }
        

        /// <summary>
        /// 為避免互相參照，比照HRPortal.Helper.FormNoGenerator產生序號函式，表單編號直接由此處產生
        /// </summary>
        /// <returns></returns>
        private string GetFormNo()
        {
            return "P" + DateTime.Now.ToString("yyyyMMdd") + this._services.GetService<SerialControlService>().GetSerialNumber("FormNo").ToString(new string('0', 6));
        }

        /*
        public void UpdateTest()
        {
            var s = this._services.GetService<BambooHREmployeeMappingService>();
            var item = s.Where(x => x.EmpData_ID.HasValue).First();
            item.EmpName = "AAA";
            item.BambooHREmployeeName = "BBB";
            item.BambooHREmail = "www@test.com";
            item.EmpData_ID = 11111;
            int result = s.Update(item);
        }
        */

        public string GetFromStartToEndTime(DateTime StartTime, DateTime EndTime)
        {
            //配合新規範，修改Portal建立假單傳到BambooHR的備註時間格式
            //return "(" + StartTime.ToString("yyyy/MM/dd HH:mm") + " ~ " + EndTime.ToString("yyyy/MM/dd HH:mm") + ")";
            return "(" + StartTime.ToString("HH:mm") + " - " + EndTime.ToString("HH:mm") + ")";
        }

        public class RequestItem
        {
            public string URL { get; set; }
            public RestRequest RestRequest { get; set; }
        }

        private class BambooHRSetting
        {
            public string BaseURL { get; set; }
            public string APIKey { get; set; }
            public string APIOwnerUserID { get; set; }
            public string AdminUserID { get; set; }
        }

        public class RequestParameter
        {
            public string key { get; set; }
            public string value { get; set; }
        }

        public class APILog
        {
            public string URL { get; set; }
            public string HTTP_Method { get; set; }
            public List<RequestParameter> RequestParameters { get; set; }
            public object RequestBody { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseCodeDescription { get; set; }
            public object ResponseContent { get; set; }
            public string ErrorMessage { get; set; }
        }
    }

}


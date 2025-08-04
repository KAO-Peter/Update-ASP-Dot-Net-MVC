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

namespace HRPortal.Services
{
    public class BambooHRIntegrationService_Bak
    {
        private const int WORKHOURS_PER_DAY = 8; //預設一天工時為8小時

        public static string urlAddTimeOffRequest = "/v1/employees/{employeeId}/time_off/request";
        public static string urlQueryTimeOffRequests = "/v1/time_off/requests";
        public static string urlChangeTimeOffRequestStatus = "/v1/time_off/requests/{requestId}/status";
        public static string urlQueryEmployeeDirectory = "/v1/employees/directory";
        public static string urlQueryEmployee = "/v1/employees/{id}";
        public static string urlAdjustTimeOffBalance = "/v1/employees/{employeeId}/time_off/balance_adjustment";

        protected HRPortal_Services _services;
        private SystemSettingService _systemSettingService;
        private BambooHRSetting _setting;
        private LogInfo _logInfo;

        public BambooHRIntegrationService_Bak(LogInfo LogInfo = null)
        {
            this._services = new HRPortal_Services();
            this._systemSettingService=this._services.GetService<SystemSettingService>();
            string BaseURL = this._systemSettingService.GetSettingValue("BambooHR_BaseURL");
            string APIKey = this._systemSettingService.GetSettingValue("BambooHR_APIKey");

            //移除末尾的斜線
            BaseURL = BaseURL.EndsWith("/") ? BaseURL.Substring(0, BaseURL.Length - 1) : BaseURL;
            APIKey = APIKey.EndsWith("/") ? APIKey.Substring(0, APIKey.Length - 1) : APIKey;

            this._setting = new BambooHRSetting() { BaseURL = BaseURL, APIKey = APIKey };

            if (LogInfo != null)
            {
                string defaultIP = "127.0.0.1";

                //IP沒輸入就預設為127.0.0.1
                LogInfo.UserIP = string.IsNullOrWhiteSpace(LogInfo.UserIP) ? defaultIP : LogInfo.UserIP;

                if (LogInfo.UserID == null) //沒有傳入UserID資訊就先用admin的
                {
                    string defaultUser = "admin";
                    
                    Employee e = this._services.GetService<EmployeeService>().GetEmployeeByEmpNo(defaultUser);
                    if (e != null)
                    {
                        LogInfo.UserID = e.ID;
                    }
                    else //如果連admin都找不到，就不紀錄Log，將logInfo設定為null
                    {
                        LogInfo = null;
                    }
                }
            }

            this._logInfo = LogInfo;
        }

        public IRestResponse Send(RequestItem rItem)
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
                Log(url, rItem.RestRequest, response, exForLog);
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
            EmployeeDirectoryResult resultAPI = API_GetAllEmployees();
            string result = UpdateMappingFromBambooHREmployeeDirectory(resultAPI.employees);
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
                AgentEmpID = LF.Agent != null ? LF.Agent.EmployeeNO : ""
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
                AgentEmpID = Item.AgentEmpID
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
                string fromStartToEndTime = getFromStartToEndTime(LFG.StartTime, LFG.EndTime);
                string note = LFG.LeaveReason + Environment.NewLine + fromStartToEndTime;
                notes.Add(new TimeOffRequest_Note() { from = "employee", note = note });

                string managerNote = "系統匯入歷史假單"; //目前直接新增核准假單一定是匯入歷史假單
                if (LFG.forHistory && LFG.LeaveFormStatus == TimeOffRequestStatus.approved) //歷史假單且是核准的，要補上系統匯入歷史假單說明
                {
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
                                formStatus = 1;
                                break;
                            case TimeOffRequestStatus.approved:
                                formStatus = 3;
                                break;
                        }

                        BambooHRLeaveFormRecord record = new BambooHRLeaveFormRecord()
                        {
                            PortalLeaveFormID = LFG.PortalLeaveFormID,
                            BambooHRTimeOffID = bbTimeOffRequestID,
                            EmpData_ID = LFG.EmpData_ID,
                            EmpID = LFG.EmpID,
                            BambooHREmployeeID = bambooHREmployeeID,
                            FormNo = LFG.FormNo,
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

        public string ChangeLeaveFormStatus(Guid LeaveFormID, TimeOffRequestStatus Status, string SignInstruction, TimeOffRequestStatus StatusBefore = TimeOffRequestStatus.requested)
        {
            //取得假單
            LeaveForm lf = this._services.GetService<LeaveFormService>().GetLeaveFormByIDWithEmployee(LeaveFormID);

            return ChangeLeaveFormStatus(lf, Status, SignInstruction, StatusBefore);
        }

        /// <summary>
        /// 更新請假單在BambooHR的狀態
        /// </summary>
        /// <param name="LeaveFormID">請假單ID(Portal)</param>
        /// <param name="Status">新狀態</param>
        /// <param name="SignInstruction">核准或退回的主管意見</param>
        /// <returns></returns>
        public string ChangeLeaveFormStatus(LeaveForm LF, TimeOffRequestStatus Status, string SignInstruction, TimeOffRequestStatus StatusBefore = TimeOffRequestStatus.requested,string Source="HRPortal")
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
                    try
                    {
                        response = API_ChangeTimeOffRequestStatus(sendObj);
                        result = string.Format("{0} {1}", ((int)response.StatusCode).ToString(), response.StatusCode.ToString());

                        TimeOffRequestStatus newStatus;
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

        public List<TimeOffRequestQueryResult> API_QueryTimeOffRequests(TimeOffRequestQuery item)
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

            IRestResponse response = Send(rItem);
            List<TimeOffRequestQueryResult> result = JsonConvert.DeserializeObject<List<TimeOffRequestQueryResult>>(response.Content);

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
                string fromStartToEndTime = getFromStartToEndTime(LF.StartTime, LF.EndTime);
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

                TimeOffRequestQueryResult timeOff = result.Where(x => x.notes.employee.Contains(fromStartToEndTime)).OrderBy(y => int.Parse(y.id)).FirstOrDefault();

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

            string note="系統回補銷假時數";
            
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
        public void Log(string URL, IRestRequest request, IRestResponse response, Exception ex)
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
                logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, JsonConvert.SerializeObject(logData));
            }
        }

        /// <summary>
        /// 撈取BambooHR假單並與之前發送紀錄進行比對，BambooHR上請假簽核最主要的邏輯起點都在此處
        /// </summary>
        /// <returns></returns>
        public string CheckBambooHRTimeOffRequest()
        {
            string result = "";
            var logService = this._services.GetService<SystemLogService>();

            //撈取上個月一號之後到明年年底的所有假單
            //20201215 因BambooHR開放可補之前假單，所以不能只撈上月一號之後，改用參數處理
            string checkMonthLimit = this._services.GetService<SystemSettingService>().GetSettingValue("BambooHR_TimeOffCheckMonth");
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

            List<TimeOffRequestQueryResult> allTimeOffList = API_QueryTimeOffRequests(query);

            allTimeOffList = allTimeOffList.OrderBy(x => x.id).ToList();
            if (this._logInfo != null && this._logInfo.UserID.HasValue)
            {
                string strLog = "本次有" + allTimeOffList.Count.ToString() + "筆BambooHR假單需比對";
                logService.WriteLog(this._logInfo.UserID.Value, this._logInfo.UserIP, this._logInfo.Controller, this._logInfo.Action, strLog);
            }

            //底下幾個表格因筆數不會太多，一次撈出來減少DB存取次數
            //取得BambooHRLeaveFormRecord紀錄
            List<BambooHRLeaveFormRecord> portalRecordList = this._services.GetService<BambooHRLeaveFormRecordService>().GetAll().ToList();

            //取得BambooHREmployeeMapping紀錄
            List<BambooHREmployeeMapping> employeeMappingList = this._services.GetService<BambooHREmployeeMappingService>().GetAll().Include("Employee").AsNoTracking().ToList();


            //逐筆進行比對
            foreach (TimeOffRequestQueryResult timeOff in allTimeOffList)
            {
                //檢查是否Record已經存在(用Time Off ID比對)
                var record = portalRecordList.Where(x => x.BambooHRTimeOffID == timeOff.id).FirstOrDefault();

                if (record == null) //HR Portal沒有紀錄-->檢查狀態
                {
                    if (timeOff.status.status == TimeOffRequestStatus.requested.ToString()) //BambooHR是requested狀態表示要建立新假單
                    {
                        //需檢核格式、時數、正確才建立假單
                        BambooHRLeaveFormRecord leaveFormRecord;
                        TimeOffCheckResult timeOffCheckResult = CheckTimeOffData(timeOff, out leaveFormRecord, employeeMappingList, timeOffTypeList);
                        if (timeOffCheckResult.Success) //檢查都正確，要存BambooHRLeaveFormRecord跟建立假單
                        {
                            BambooHREmployeeMapping empMapping = employeeMappingList.Where(x => x.BambooHREmployeeID == timeOff.employeeId).FirstOrDefault();
                            if (empMapping != null)
                            {
                                leaveFormRecord.PortalLeaveFormID = null;
                                leaveFormRecord.FormNo = "";
                                leaveFormRecord.FormStatus = -1; //此時狀態因為Portal還未建單，先給-1
                                leaveFormRecord.CreateFrom = "BambooHR";
                                leaveFormRecord.CreateTime = DateTime.Now;

                                //Portal新增假單，比照原請假單新增程序
                                string createResult = CreatePortalLeaveForm(leaveFormRecord, timeOffCheckResult.HRMCheckResponse, timeOffCheckResult.MappedInfo);



                            }
                        }

                    }
                    else if (timeOff.status.status == TimeOffRequestStatus.approved.ToString()) //抓到BambooHR新紀錄就已經是approved狀態
                    {

                    }
                    else if (timeOff.status.status == TimeOffRequestStatus.canceled.ToString()) //抓到新紀錄就已經是取消的-->要檢查這筆假單之前是否請過，請過就要做銷假流程
                    {
                       
                    }


                }
                else
                {

                }

            }

            return result;

        }

        private TimeOffCheckResult CheckTimeOffData(TimeOffRequestQueryResult item, out BambooHRLeaveFormRecord LeaveFormRecord, List<BambooHREmployeeMapping> EmployeeMappingList = null, List<BambooHRTimeOffType> TimeOffTypeList = null)
        {
            const int DEFAULT_START_HOUR = 9;
            const int DEFAULT_END_HOUR = 18;
            TimeOffCheckResult result = new TimeOffCheckResult() { checkItem = item };
            LeaveFormRecord = new BambooHRLeaveFormRecord();

            CultureInfo culture = CultureInfo.InvariantCulture;
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
            DateTime bambooHR_StartDate = DateTime.ParseExact(item.start, "yyyy-MM-dd", culture);
            DateTime bambooHR_EndDate = DateTime.ParseExact(item.end, "yyyy-MM-dd", culture);
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
            //由BambooHR假單整理出所需資料，同時設定BambooHRLeaveFormRecord部分資料
            TimeOffRequestMappedInfo mappedInfo = GetMappedInfo(item, EmployeeMappingList, TimeOffTypeList);
            result.MappedInfo = mappedInfo;

            LeaveFormRecord = mappedInfo.BambooHRLeaveFormRecord;
            LeaveFormRecord.BambooHRStartDate = bambooHR_StartDate;
            LeaveFormRecord.BambooHREndDate = bambooHR_EndDate;
            LeaveFormRecord.BambooHRConfirmedStartTime = bambooHR_StartTime;
            LeaveFormRecord.BambooHRConfirmedEndTime = bambooHR_EndTime;

            if (mappedInfo.Employee != null)
            {
                //計算時數
                BambooHRCheckLeaveResponse hrmResult = Task.Run(() => HRMApiAdapter.BambooHRCheckAbsentAmount(mappedInfo.Employee.Company.CompanyCode, mappedInfo.TimeOffTypeMapping.HRM_AbsentCode, mappedInfo.Employee.EmployeeNO, mappedInfo.Employee.Employee_ID.Value, bambooHR_StartTime, bambooHR_EndTime, inputAmount, inputUnit)).Result;
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
                InputNote=Note,
                ExtractedTimeStr =""

            };

            //預設格式為HHmm~HHmm 或是HHmm-HHmm
            char[] delimiters=new char[] {'-','~'};
            string[] times = new string[] { "", "" };
            
            if (!string.IsNullOrWhiteSpace(Note))
            {
                if (CheckTimeFormat(Note, delimiters, ref times))
                {
                    //組合出正確時間
                    DateTime extractedStartTime = new DateTime(InputStartDate.Year, InputStartDate.Month, InputStartDate.Day, int.Parse(times[0].Substring(0, 2)), int.Parse(times[0].Substring(2, 2)), 0);
                    DateTime extractedEndTime = new DateTime(InputEndDate.Year, InputEndDate.Month, InputEndDate.Day, int.Parse(times[1].Substring(0, 2)), int.Parse(times[1].Substring(2, 2)), 0);
                   
                    extractResult.IsFormatCorrect = true;
                    extractResult.ExtractedTimeStr = times[0] + " " + times[1];
                    extractResult.ExtractedStartTime = extractedStartTime;
                    extractResult.ExtractedEndTime = extractedEndTime;
                }
            }
            
            return extractResult;

        }

        //檢查格式，只要字串內有 HHmm - HHmm 類似的格式就可以
        private bool CheckTimeFormat(string Note, char[] Delimiters, ref string[] TimeResult)
        {
            bool result = false;
            string pattern1 = @"\d{4}$";
            string pattern2 = @"^\d{4}";

            foreach (char item in Delimiters)
            {
                string[] s = Note.Split(item).Select(x => x.Trim()).ToArray();
                if (s.Length > 1)
                {
                    Regex r1 = new Regex(pattern1);
                    Regex r2 = new Regex(pattern2);
                    string s1 = s[0].Substring(Math.Max(0, s[0].Length - 4));
                    string s2 = s[1].Substring(0, 4);
                    if (s1 != s2 && r1.IsMatch(s1) && r2.IsMatch(s2)
                        && int.Parse(s1.Substring(0, 2)) < 24 && int.Parse(s1.Substring(2, 2)) < 60
                        && int.Parse(s2.Substring(0, 2)) < 24 && int.Parse(s2.Substring(2, 2)) < 60)
                    {
                        result = true;
                        TimeResult[0] = s1;
                        TimeResult[1] = s2;
                        break; //有一個正確就跳出迴圈
                    }
                }
            }

            return result;
        }

        private TimeOffRequestMappedInfo GetMappedInfo(TimeOffRequestQueryResult item, List<BambooHREmployeeMapping> EmployeeMappinglist, List<BambooHRTimeOffType> TimeOffTypeList)
        {
            Employee employee = this._services.GetService<BambooHREmployeeMappingService>().GetEmployeeByBambooHREmployeeID(item.employeeId, EmployeeMappinglist);

            BambooHRTimeOffType timeOffTypeMapping = TimeOffTypeList.Where(x => x.BambooHR_TimeOffTypeID == item.type.id).FirstOrDefault();

            BambooHRLeaveFormRecord leaveFormRecord = new BambooHRLeaveFormRecord()
            {
                BambooHRTimeOffID = item.id,
                EmpData_ID = employee == null ? null : employee.Employee_ID,
                EmpID = employee == null ? null : employee.EmployeeNO,
                BambooHREmployeeID = item.employeeId,
                PortalEmployeeID = employee == null ? (Guid?)null : employee.ID,
                PortalAbsentCode = timeOffTypeMapping == null ? "" : timeOffTypeMapping.HRM_AbsentCode,
                PortalAbsentUnit = timeOffTypeMapping == null ? "" : timeOffTypeMapping.HRM_AbsentUnit,
                BambooHRLeaveAmount = decimal.Parse(item.amount.amount),
                BambooHRLeaveReason = item.notes == null ? "" : item.notes.employee,
            };

            return new TimeOffRequestMappedInfo()
            {
                QueryResult = item,
                Employee = employee,
                TimeOffTypeMapping = timeOffTypeMapping,
                BambooHRLeaveFormRecord = leaveFormRecord
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
            lfRecordService.Create(LeaveFormRecord);

            //新增LeaveForm
            //先換算請假時數單位
            double leaveAmount=HRMResult.SystemUnit=="d"? HRMResult.SystemAbsentHours/(double)WORKHOURS_PER_DAY : HRMResult.SystemAbsentHours;
            DateTime now = DateTime.Now;
            LeaveForm lf = new LeaveForm()
            {
                FormNo = GetFormNo(),
                Status = (int)FormSignStatus.Draft,
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
                AgentID = null, //之後要做Mapping
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
                WorkHours = (decimal)((LeaveFormRecord.BambooHRConfirmedEndTime.Value.Date - LeaveFormRecord.BambooHRConfirmedStartTime.Value.Date).TotalDays * WORKHOURS_PER_DAY) //Portal原來的算法是請假區間每天的標準工時加總，但不排除假日

            };

            int isSuccess = this._services.GetService<LeaveFormService>().Create(lf);

            //存入檢核明細到後台的PortalFormDetail表格
            if (isSuccess == 1)
            {
                try
                {
                    RequestResult _portalFormSaveResult = Task.Run(() => HRMApiAdapter.SavePortalFormDetail(
                                                                                lf.ID.ToString(),
                                                                                lf.FormNo,
                                                                                LeaveFormRecord.EmpData_ID.Value,
                                                                                LeaveFormRecord.EmpID,
                                                                                lf.StartTime,
                                                                                lf.EndTime,
                                                                                lf.AbsentUnit,
                                                                                WORKHOURS_PER_DAY, //WorkHours傳進去基本上不會用到
                                                                                Convert.ToDouble(lf.LeaveAmount),
                                                                                HRMResult.EmpAbsentCheckDetailList,
                                                                                LeaveFormRecord.EmpID
                                                                                )).Result;
                    isSuccess = _portalFormSaveResult.Status ? 1 : 0;
                }
                catch (Exception ex) //儲存PortalFormDetail錯誤，原先是直接中斷
                {
                    return "";
                }

            }

            if (isSuccess == 1) //處理簽核流程
            {
                /*
                LeaveSignList _signList = new LeaveSignList();
                SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();
                string _senderFlowId;
                IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(model.FormData.FormNo);
                if (_signFlow == null || _signFlow.Count == 0)
                {
                    _signFlow = _signList.GetDefaultSignList(_formData, true);
                    _signList.SaveSigningFlow(_signFlow, null);
                    _senderFlowId = _signFlow[0].ID;

                    //判斷簽核流程無代理人時也會寄信給代理人Irving 2016/03/15
                    var flag = 0;
                    foreach (var item in _signFlow)
                    {
                        if (item.SignerID == _formData.AgentNo)
                        {
                            flag = 1;
                        }
                    }

                    //20190507 小榜 發送代理人通知E-Mail拉出去共用，修改假單時也要重新發送
                    await sendMailToAgent(model.FormData.FormNo, _formData.AgentNo, _formData.CUser);

                    #region 20190507 小榜 註解原發送代理人通知E-Mail
                    
                    #endregion
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
                    await sendMailToAgent(model.FormData.FormNo, _formData.AgentNo, _formData.CUser);
                }
                */
            }

            return result;
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

        public string getFromStartToEndTime(DateTime StartTime, DateTime EndTime)
        {
            return "(" + StartTime.ToString("yyyy/MM/dd HH:mm") + " ~ " + EndTime.ToString("yyyy/MM/dd HH:mm") + ")";
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
        }

        public class RequestParameter
        {
            public string key { get; set; }
            public string value{get;set;}
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



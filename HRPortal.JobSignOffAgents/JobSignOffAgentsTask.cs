using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.JobSignOffAgents
{
    public class JobSignOffAgentsTask
    {
        public Action<string> OnMessage;
        public HRPortal_Services Services = new HRPortal_Services();

        private NewHRPortalEntities _db;
        private SignFlowEntities _sdb;
        public NewHRPortalEntities DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new NewHRPortalEntities();
                }
                return _db;
            }
        }

        public SignFlowEntities SDB
        {
            get
            {
                if (_sdb == null)
                {
                    _sdb = new SignFlowEntities();
                }
                return _sdb;
            }
        }
        public JobSignOffAgentsTask()
        {
        }

        public JobSignOffAgentsTask(NewHRPortalEntities db)
        {
            this._db = db;
        }

        public void WriteMessage(string message)
        {
            if (OnMessage != null)
            {
                OnMessage(message);
            }
        }
        public async Task Run()
        {
            //List<SignoffAgents> _signoffAgentsList = DB.SignoffAgents.ToList();
            List<Employee> _employeeList = DB.Employees.ToList();
            List<LeaveForm> _leaveFormList = DB.LeaveForms.ToList();
            List<PatchCardForm> _patchCardFormList = DB.PatchCardForms.ToList();
            List<LeaveDetail> _result;
            List<SignFlowRec> _signFlowRecList = SDB.SignFlowRec.ToList();
            List<Company> _CompanyData = DB.Companys.ToList();
            int _AbsentDate = int.Parse(GetAbsentDate());
            int _VacationHours = int.Parse(GetVacationHours());
            string _PortalUrl = GetPortalUrl();
            DateTime startTime = DateTime.Today;
            DateTime endTime = DateTime.Today.AddDays(_AbsentDate);
            SignFlowRecQueryHelper _SignFlowRecQuert = new SignFlowRecQueryHelper();
            List<SignFlowRec> _signFlowList;
            List<SignFlowRec> _signFlowListP;
            SignFlowRec _signFlowData = new SignFlowRec();
            SignFlowRec _signFlowDataP = new SignFlowRec();
            var _Agents = new List<Tuple<string, string>>();
            String tempSignerID;
            #region 取出Leave需要簽核的資料
            //取出需要簽核的資料
            _signFlowList = _signFlowRecList.Where(x => x.SignType == "P" && x.SignStatus == "W" && x.IsUsed == "Y" && x.FormType == "Leave" ).ToList();

            //20171011 Daniel 補上<=，原先只有<
            for (int i = 0; i <= _signFlowList.Count - 1; i++)
            {
                _signFlowData = _signFlowList[i];
                //檢查是否有休假
                var deptCode = _employeeList.Where(x => x.EmployeeNO == _signFlowData.OrgSignerID).FirstOrDefault().Department.DepartmentCode;

                //讀取主管假單 
                _result = await HRMApiAdapter.GetDeptLeaveEmployee(_signFlowData.SignCompanyID + "", deptCode + "", startTime, endTime, _signFlowList[i].OrgSignerID + "");

                //未休假執行下一位主管
                if (_result.Count == 0)
                {
                    //未休假，將原本該簽核的單子取回來
                    if (_signFlowData.OrgSignerID != _signFlowData.SignerID)
                    {
                        GetRetrieve(_signFlowData, _signFlowData.OrgSignerID);
                    }
                    continue;
                }//判斷休假時數是否大於設定時數，小於設定時數，將原本該簽核的單子取回來
                else if (_VacationHours != 0)
                {
                    for (int n = 0; n <= _result.Count - 1; n++)
                    {
                        //主管休假時數小於設定時數，假單不轉移至代理人
                        //20171012 Daniel <改成<=，因為要大於設定時數才轉代理人，例如設定8小時，就是8.5或9小時之後才會轉
                        if (_result[n].AbsentAmount <= _VacationHours)
                        {
                            WriteMessage(string.Format("{0}休假時數未大於設定時數，簽核單重新取回", _signFlowData.OrgSignerID));
                            if (_signFlowData.OrgSignerID != _signFlowData.SignerID)
                            {
                                GetRetrieve(_signFlowData, _signFlowData.OrgSignerID);
                                _signFlowData = null;
                                continue;
                            }
                        }
                    }
                }

                if (_signFlowData == null)
                {
                    continue;
                }
                Guid companyID = _CompanyData.FirstOrDefault(x => x.CompanyCode == _signFlowData.SignCompanyID).ID;
                Guid employeeID = _employeeList.FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeNO == _signFlowData.OrgSignerID).ID;

                List<LeaveDetail> _resultChcek;
                string starDate = startTime.ToShortDateString();
                string endDate = endTime.ToShortDateString();
                //抓取假單資料(主管)
                var queryform = _leaveFormList.OrderByDescending(e => e.FormNo);
                //20171012 Daniel 簽核代理人調整邏輯，避免不在今天範圍的假單被抓出來
                //LeaveForm _form = queryform.FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeID == employeeID && x.IsDeleted != true && ((x.StartTime >= DateTime.Parse(starDate) && x.EndTime >= DateTime.Parse(endDate)) || (x.StartTime <= DateTime.Parse(starDate) && x.EndTime >= DateTime.Parse(endDate)) || (x.StartTime < DateTime.Parse(endDate) && x.EndTime >= DateTime.Parse(starDate))));
                LeaveForm _form = queryform.FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeID == employeeID && x.IsDeleted != true && ((x.StartTime >= DateTime.Parse(starDate) && x.StartTime < DateTime.Parse(endDate)) || (x.StartTime <= DateTime.Parse(starDate) && x.EndTime >= DateTime.Parse(endDate)) || (x.EndTime >= DateTime.Parse(starDate) && x.EndTime < DateTime.Parse(endDate))));


                if (_form != null)
                {
                    //抓取假單資料
                    LeaveForm _formU = _leaveFormList.FirstOrDefault(x => x.FormNo == _signFlowList[i].FormNumber);
                    //抓取假別資料(待簽核人員)
                    List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(_signFlowData.SignCompanyID + "", _signFlowData.SenderID + "", DateTime.Now);

                    //20171011 Start Daniel 原表單沒有填代理人就直接跳過
                    //String agentNo = _employeeList.FirstOrDefault(x => x.CompanyID == _form.CompanyID && x.ID == _form.AgentID).EmployeeNO;
                    string agentNo = "";
                    Employee emp = _employeeList.FirstOrDefault(x => x.CompanyID == _form.CompanyID && x.ID == _form.AgentID);
                    if (emp == null)
                    {
                        continue;
                    }
                    else
                    {
                        agentNo = emp.EmployeeNO;
                    }
                    //20171011 End

                    tempSignerID = _signFlowData.SignerID;

                    deptCode = _employeeList.Where(x => x.EmployeeNO == agentNo).FirstOrDefault().Department.DepartmentCode;
                    _resultChcek = await HRMApiAdapter.GetDeptLeaveEmployee(_signFlowData.SignCompanyID + "", deptCode + "", startTime, endTime, agentNo + "");

                    if (_resultChcek.Count == 0)
                    {
                        //送件人員不是主管代理人才變更簽核代理人
                        if (_signFlowData.SenderID != agentNo)
                        {
                            GetSignFlow(_signFlowData, agentNo);
                            //簽核人員就是代理人時，不寄送通知
                            if (tempSignerID != agentNo + "")
                            {
                                SendEmail(data, _employeeList, _formU, _signFlowData.OrgSignerID, agentNo + "", _PortalUrl);
                            }
                        }
                    }
                    else if (_VacationHours == 0)
                    {
                        //送件人員不是主管代理人才變更簽核代理人
                        if (_signFlowData.SenderID != agentNo)
                        {
                            GetSignFlow(_signFlowData, agentNo);
                            //簽核人員就是代理人時，不寄送通知
                            if (tempSignerID != agentNo + "")
                            {
                                SendEmail(data, _employeeList, _formU, _signFlowData.OrgSignerID, agentNo + "", _PortalUrl);
                            }
                        }
                    }
                    else if (_VacationHours != 0)
                    {
                        for (int n = 0; n <= _resultChcek.Count - 1; n++)
                        {
                            //代理人休假時數大於設定時數，假單不轉移至代理人
                            if (_resultChcek[n].AbsentAmount >= _VacationHours)
                            {
                                WriteMessage(string.Format("{0}代理人在此區間已休假.", agentNo));
                                GetRetrieve(_signFlowData, _signFlowData.OrgSignerID);
                                continue;
                            }
                            else
                            {
                                //送件人員不是主管代理人才變更簽核代理人
                                if (_signFlowData.SenderID != agentNo)
                                {
                                    GetSignFlow(_signFlowData, agentNo);

                                    //簽核人員就是代理人時，不寄送通知
                                    if (tempSignerID != agentNo + "")
                                    {
                                        SendEmail(data, _employeeList, _formU, _signFlowData.OrgSignerID, agentNo + "", _PortalUrl);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        WriteMessage(string.Format("{0}代理人在此區間已休假.", agentNo));
                        continue;
                    }
                }
            }
            #endregion
            #region 取出PatchCard需要簽核的資料
            //取出需要簽核的資料
            _signFlowListP = _signFlowRecList.Where(x => x.SignType == "P" && x.SignStatus == "W" && x.IsUsed == "Y" && x.FormType == "PatchCard").ToList();

            for (int i = 0; i <= _signFlowListP.Count - 1; i++)
            {
                _signFlowDataP = _signFlowListP[i];
                //檢查是否有休假
                var deptCode = _employeeList.Where(x => x.EmployeeNO == _signFlowDataP.OrgSignerID).FirstOrDefault().Department.DepartmentCode;
                String SignCompanyID = _employeeList.FirstOrDefault(x => x.EmployeeNO == _signFlowDataP.OrgSignerID).Company.CompanyCode;
                //讀取主管假單 
                _result = await HRMApiAdapter.GetDeptLeaveEmployee(SignCompanyID + "", deptCode + "", startTime, endTime, _signFlowListP[i].OrgSignerID + "");

                //未休假執行下一位主管
                if (_result.Count == 0)
                {
                    //未休假，將原本該簽核的單子取回來
                    if (_signFlowDataP.OrgSignerID != _signFlowDataP.SignerID)
                    {
                        GetRetrieve(_signFlowDataP, _signFlowDataP.OrgSignerID);
                    }
                    continue;
                }//判斷休假時數是否大於設定時數，小於設定時數，將原本該簽核的單子取回來
                else if (_VacationHours != 0)
                {
                    for (int n = 0; n <= _result.Count - 1; n++)
                    {
                        //主管休假時數小於設定時數，假單不轉移至代理人
                        //20171012 Daniel <改成<=，因為要大於設定時數才轉代理人，例如設定8小時，就是8.5或9小時之後才會轉
                        if (_result[n].AbsentAmount <= _VacationHours)
                        {
                            WriteMessage(string.Format("{0}休假時數小於設定時數，簽核單重新取回", _signFlowDataP.OrgSignerID));
                            if (_signFlowDataP.OrgSignerID != _signFlowDataP.SignerID)
                            {
                                GetRetrieve(_signFlowDataP, _signFlowDataP.OrgSignerID);
                                _signFlowDataP = null;
                                continue;
                            }
                        }
                    }
                }

                if (_signFlowDataP == null)
                {
                    continue;
                }
                Guid companyID = _CompanyData.FirstOrDefault(x => x.CompanyCode == SignCompanyID).ID;
                Guid employeeID = _employeeList.FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeNO == _signFlowDataP.OrgSignerID).ID;

                List<LeaveDetail> _resultChcek;
                string starDate = startTime.ToShortDateString();
                string endDate = endTime.ToShortDateString();
                //抓取假單資料(主管)
                var queryformP = _leaveFormList.OrderByDescending(e => e.FormNo);
                //20171012 Daniel 簽核代理人調整邏輯，避免不在今天範圍的假單被抓出來
                //LeaveForm _form_P = queryformP.FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeID == employeeID && x.IsDeleted != true && ((x.StartTime >= DateTime.Parse(starDate) && x.EndTime >= DateTime.Parse(endDate)) || (x.StartTime <= DateTime.Parse(starDate) && x.EndTime >= DateTime.Parse(endDate)) || (x.StartTime < DateTime.Parse(endDate) && x.EndTime >= DateTime.Parse(starDate))));
                LeaveForm _form_P = queryformP.FirstOrDefault(x => x.CompanyID == companyID && x.EmployeeID == employeeID && x.IsDeleted != true && ((x.StartTime >= DateTime.Parse(starDate) && x.StartTime < DateTime.Parse(endDate)) || (x.StartTime <= DateTime.Parse(starDate) && x.EndTime >= DateTime.Parse(endDate)) || (x.EndTime >= DateTime.Parse(starDate) && x.EndTime < DateTime.Parse(endDate))));

                if (_form_P != null)
                {
                    //抓取假單資料
                    PatchCardForm _formP = _patchCardFormList.FirstOrDefault(x => x.FormNo == _signFlowListP[i].FormNumber);
                    //抓取假別資料(待簽核人員)
                    List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(SignCompanyID + "", _signFlowDataP.SenderID + "", DateTime.Now);

                    //20171011 Start Daniel 原表單沒有填代理人就直接跳過
                    //String agentNo = _employeeList.FirstOrDefault(x => x.CompanyID == _form_P.CompanyID && x.ID == _form_P.AgentID).EmployeeNO;
                    string agentNo = "";
                    Employee emp = _employeeList.FirstOrDefault(x => x.CompanyID == _form_P.CompanyID && x.ID == _form_P.AgentID);
                    if (emp == null)
                    {
                        continue;
                    }
                    else
                    {
                        agentNo = emp.EmployeeNO;
                    }
                    //20171011 End
                    
                    tempSignerID = _signFlowDataP.SignerID;

                    deptCode = _employeeList.Where(x => x.EmployeeNO == agentNo).FirstOrDefault().Department.DepartmentCode;
                    _resultChcek = await HRMApiAdapter.GetDeptLeaveEmployee(SignCompanyID + "", deptCode + "", startTime, endTime, agentNo + "");

                    if (_resultChcek.Count == 0)
                    {
                        //送件人員不是主管代理人才變更簽核代理人
                        if (_signFlowDataP.SenderID != agentNo)
                        {
                            GetSignFlow(_signFlowDataP, agentNo);
                            //簽核人員就是代理人時，不寄送通知
                            if (tempSignerID != agentNo + "")
                            {
                                PatchCardSendEmail(data, _employeeList, _formP, _signFlowDataP.OrgSignerID, agentNo + "", _PortalUrl);
                            }
                        }
                    }
                    else if (_VacationHours == 0)
                    {
                        //送件人員不是主管代理人才變更簽核代理人
                        if (_signFlowDataP.SenderID != agentNo)
                        {
                            GetSignFlow(_signFlowDataP, agentNo);
                            //簽核人員就是代理人時，不寄送通知
                            if (tempSignerID != agentNo + "")
                            {
                                PatchCardSendEmail(data, _employeeList, _formP, _signFlowDataP.OrgSignerID, agentNo + "", _PortalUrl);
                            }
                        }
                    }
                    else if (_VacationHours != 0)
                    {
                        for (int n = 0; n <= _resultChcek.Count - 1; n++)
                        {
                            //代理人休假時數大於設定時數，假單不轉移至代理人
                            if (_resultChcek[n].AbsentAmount >= _VacationHours)
                            {
                                WriteMessage(string.Format("{0}代理人在此區間已休假.", agentNo));
                                GetRetrieve(_signFlowDataP, _signFlowDataP.OrgSignerID);
                                continue;
                            }
                            else
                            {
                                //送件人員不是主管代理人才變更簽核代理人
                                if (_signFlowDataP.SenderID != agentNo)
                                {
                                    GetSignFlow(_signFlowDataP, agentNo);

                                    //簽核人員就是代理人時，不寄送通知
                                    if (tempSignerID != agentNo + "")
                                    {
                                        PatchCardSendEmail(data, _employeeList, _formP, _signFlowDataP.OrgSignerID, agentNo + "", _PortalUrl);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        WriteMessage(string.Format("{0}代理人在此區間已休假.", agentNo));
                        continue;
                    }
                }
            }
            #endregion
        }

        /// <summary> 取回待簽核假單 </summary>
        public void GetRetrieve(SignFlowRec _signFlowData, string SignerID)
        {
            //將原本該簽核的單子取回來
            if (_signFlowData != null)
            {
                var updSignFlow = _sdb.SignFlowRec.Find(_signFlowData.ID);
                updSignFlow.SignerID = SignerID;
                _sdb.SaveChanges();
            }
        }

        /// <summary> 變更簽核人員 </summary>
        public void GetSignFlow(SignFlowRec _signFlowData, string agentNo)
        {
            //變更簽核人員
            if (_signFlowData != null)
            {
                var updSignFlow = _sdb.SignFlowRec.Find(_signFlowData.ID);
                updSignFlow.SignerID = agentNo;
                _sdb.SaveChanges();
            }
        }

        #region 寄送E-Mail通知
        /// <summary> 寄送E-Mail通知 </summary>
        public void SendEmail(List<AbsentDetail> data, List<Employee> _employeeList, LeaveForm _form, string OrgSignerID, string AgentNo, string PortalUrl)
        {
            //原簽核主管
            Employee currentUser = _employeeList.FirstOrDefault(x => x.EmployeeNO == OrgSignerID);

            //假別資料
            string AbsentCodee = null;
            foreach (var i in data)
            {
                if (i.Code == _form.AbsentCode)
                {
                    AbsentCodee = i.Name;//抓取假別名稱
                }
            }
            //內容資料
            string _body = (_form.Employee.EmployeeName + "的請假單" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "正在等待您簽核<br/><a href=" + PortalUrl + ">系統網站 </a>");

            //抓取代理人編號
            Employee _agent = null;
            _agent = _employeeList.FirstOrDefault(x => x.CompanyID == currentUser.CompanyID && x.EmployeeNO == AgentNo);
            if (_agent != null)
            {
                List<string> _rcpt = new List<string>();
                _rcpt.Add(_agent.Email);
                string _subject = _form.Employee.EmployeeName + "請假單簽核通知";

                string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
                Services.GetService<MailMessageService>().CreateMail(_fromMail, _rcpt.ToArray(), null, null, _subject, _body, true);
            }
        }
        #endregion


        #region 寄送補刷卡E-Mail通知
        /// <summary> 寄送補刷卡E-Mail通知 </summary>
        public void PatchCardSendEmail(List<AbsentDetail> data, List<Employee> _employeeList, PatchCardForm _form, string OrgSignerID, string AgentNo, string PortalUrl)
        {
            //原簽核主管
            Employee currentUser = _employeeList.FirstOrDefault(x => x.EmployeeNO == OrgSignerID);

            ////假別資料
            //string AbsentCodee = null;
            //foreach (var i in data)
            //{
            //    if (i.Code == _form.AbsentCode)
            //    {
            //        AbsentCodee = i.Name;//抓取假別名稱
            //    }
            //}
            //內容資料
            string _body = (_form.Employee.EmployeeName + "的補刷卡單正在等待您簽核<br/><a href=" + PortalUrl + ">系統網站 </a>");

            //抓取代理人編號
            Employee _agent = null;
            _agent = _employeeList.FirstOrDefault(x => x.CompanyID == currentUser.CompanyID && x.EmployeeNO == AgentNo);
            if (_agent != null)
            {
                List<string> _rcpt = new List<string>();
                _rcpt.Add(_agent.Email);
                string _subject = _form.Employee.EmployeeName + "補刷卡單簽核通知";

                string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
                Services.GetService<MailMessageService>().CreateMail(_fromMail, _rcpt.ToArray(), null, null, _subject, _body, true);
            }
        }
        #endregion

        #region 設定參數
        /// <summary> 取得公司 </summary>
        /// <returns></returns>
        private async Task<List<CompanyData>> GetCompanyData()
        {
            return await HRMApiAdapter.GetCompany();
        }

        /// <summary> 取得部門 </summary>
        /// <param name="companyCode"></param>
        private async Task<List<DepartmentData>> GetDepartmentData(string companyCode)
        {
            return await HRMApiAdapter.GetDepartment(companyCode);
        }

        /// <summary> 取得休假天數設定 </summary>
        public static string GetAbsentDate()
        {
            return ConfigurationManager.AppSettings["AbsentDate"];
        }

        /// <summary> 取得休假時數設定 </summary>
        public static string GetVacationHours()
        {
            return ConfigurationManager.AppSettings["VacationHours"];
        }

        /// <summary> 取得休假時數設定 </summary>
        public static string GetPortalUrl()
        {
            return ConfigurationManager.AppSettings["PortalUrl"];
        }
        #endregion

        public void Dispose()
        {
        }
    }
}

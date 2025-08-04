using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.BambooHRIntegration;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.MultiLanguage;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Services.Models;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.Areas.M02.Controllers
{
    public class LeaveFormController : BaseController
    {      
        private IFormNoGenerator formNoGenerator;
        String commonAgent = "CommonAgent";//常用代理人部門代碼
        //
        // GET: /M02/LeaveForm/
        public async Task<ActionResult> Index(string formNo)
        {
             
            SetBaseUserInfo();
            //初始化
            this.formNoGenerator = new FormNoGenerator();
            LeaveFormViewModel viewmodel = new LeaveFormViewModel();
            LeaveForm model = null;

            //設定ViewBag.StartTime、ViewBag.minTime、ViewBag.EndTime，初始值，預設為08:00~17:00，一天工作8小時。避免前端引用時因為沒有物件資料造成黃頁。
            ViewBag.StartTime = DateTime.Parse(DateTime.Now.ToShortDateString() + " 08:00");
            ViewBag.minTime = "08:00";
            ViewBag.EndTime = DateTime.Parse(DateTime.Now.ToShortDateString() + " 17:00"); 

            //20180625 小榜 將班表時間先讀取出來
            //預帶班表時間
            List<GetEmpScheduleClassTimeResponse> EmpScheduleClassTime = await GetEmpScheduleClassTime(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, DateTime.Now);
            foreach (var empScheduleTime in EmpScheduleClassTime)
            {
                string StartTime = empScheduleTime.StartTime.ToString("HH:mm"); //這邊轉24小時格式
                string EndTime = empScheduleTime.EndTime.ToString("HH:mm"); //這邊轉24小時格式
              
                if (StartTime != "00:00" && EndTime != "00:00")
                {
                    ViewBag.StartTime = empScheduleTime.StartTime;
                    ViewBag.minTime = StartTime;
                    ViewBag.EndTime = empScheduleTime.EndTime;
                }
            }

            if (formNo != null)
            {
                model = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
                //判斷該筆是否為此登入者的
                if (model.EmployeeID != CurrentUser.EmployeeID)
                {
                    TempData["message"] = "您無此單的編輯權限。";
                    return RedirectToAction("Index");
                }
                ViewBag.LeaveAmountData = model.AbsentCode;
                if (model.Agent != null)
                {
                    ViewBag.DeptID = model.Agent.SignDepartmentID;
                    ViewBag.DeptData = model.Agent.Department.DepartmentCode;
                    ViewBag.AgentData = model.Agent.EmployeeNO;
                    ViewBag.StartTime = model.StartTime.ToString("yyyy/MM/dd hh:mm");
                    ViewBag.EndTime = model.EndTime.ToString("yyyy/MM/dd hh:mm");
                }
                else
                {
                    ViewBag.DeptID = Guid.Empty;
                    ViewBag.DeptData = "";
                    ViewBag.AgentData = "";
                    ViewBag.StartTime = "";
                    ViewBag.EndTime = "";
                }
                ViewBag.StatusEdit = "True";
                Session["Mode"] = FormEditMode.Edit.ToString();
            }
            if (model == null)
            {
                model = new LeaveForm();
                model.StartTime = ViewBag.StartTime;// DateTime.Now;
                model.EndTime = ViewBag.EndTime;// DateTime.Now.AddHours(1);
                model.FormNo = formNoGenerator.GetLeaveFormNo();
                model.IsAbroad = false;
                Session["Mode"] = FormEditMode.Create.ToString();
                ViewBag.LeaveAmountData = "";
                ViewBag.DeptID = Guid.Empty;
                ViewBag.DeptData = "";
                ViewBag.AgentData = "";
                ////預帶班表時間
                //List<GetEmpScheduleClassTimeResponse> EmpScheduleClassTime = await GetEmpScheduleClassTime(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, DateTime.Now);
                //foreach (var empScheduleTime in EmpScheduleClassTime)
                //{
                //    string StartTime = empScheduleTime.StartTime.ToString("hh:mm");
                //    string EndTime = empScheduleTime.EndTime.ToString("hh:mm");
                //    if (StartTime != "00:00" && EndTime != "00:00")
                //    {
                //        model.StartTime = empScheduleTime.StartTime;
                //        model.EndTime = empScheduleTime.EndTime;
                //    }
                //}
            }

            //Irving 預設部門及代理人為使用者部門 2016/03/21
            var deptID = Services.GetService<DepartmentService>().GetAll().Where(x => x.DepartmentCode == CurrentUser.SignDepartmentCode).Where(x=>x.Enabled==true).Select(x => x.ID).FirstOrDefault();
            if (deptID == null)
            {
                deptID = Guid.Empty;
            }
            //檢核待簽核的假單後台是否確實有建立 如果有建立 狀態碼沒變的會做修正 by 20161205 bee
            await checkLeaveFormIsNotBuildingToHRM(CurrentUser.Employee.ID, CurrentUser.Employee.Company.CompanyCode);
            //ViewData["DepartmentList"] = GetDepartmentList(CurrentUser.SignDepartmentCode);
            //取得可選擇的部門所有人員資料
            List<Employee> DeptEmpAgentList = await GetDeptAgentAll();

            ViewData["DepartmentList"] = GetDepartmentList(commonAgent, DeptEmpAgentList);
            //20180516 Ricky 起迄應同View(model)上的時間
            ViewData["AgentList"] = await GetActingPersonList(deptID, commonAgent, ViewBag.AgentData, model.StartTime.ToString("yyyy/MM/dd hh:mm"), model.EndTime.ToString("yyyy/MM/dd hh:mm"), DeptEmpAgentList);
            //ViewData["AgentList"] = await GetActingPersonList(deptID, commonAgent, ViewBag.AgentData, ViewBag.StartTime, ViewBag.EndTime, DeptEmpAgentList);
            //End  Irving
            ViewData["LeaveAmountList"] = await GetLeaveAmountList2(ViewBag.LeaveAmountData);
            viewmodel.FormData = model;
            viewmodel.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
           
            Employee EmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, ViewBag.EmpID);
            ViewBag.EmployeeEnglishName = EmployeeEnglishName.EmployeeEnglishName;
            var DepartmentEnglishName = Services.GetService<DepartmentService>().GetAll().Where(x => x.DepartmentCode == CurrentUser.SignDepartmentCode).Select(x => x.DepartmentEnglishName).FirstOrDefault();
            ViewBag.DepartmentEnglishName=DepartmentEnglishName;
            ViewBag.AgentRequired = Services.GetService<SystemSettingService>().GetSettingValue("LeaveFormAgentRequired").ToLower() ;
            
            //參數設定檔
            List<FormSetting> FormSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode).ToList();

            ViewBag.DeferredAmount = await this.CanUseCount(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, "DD1");
            ViewBag.isCheck = FormSettings.FirstOrDefault(x => x.SettingKey == "isCheck" && x.CompanyCode == CurrentUser.CompanyCode && x.FormType.ToLower() == "leave").SettingValue;

            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LeaveFormViewModel model, string agentNo,
            string AbsentCode, decimal AbsentAmount, string AbsentUnitData, string LeaveAmountData, HttpPostedFileBase UploadFilePath, double WorkHours, string EmpAbsentCheckDetailList,string AbsentName, string AbsentEngName, decimal AbsentQuota)
        {

            model.FormData.AbsentCode = AbsentCode;
            model.FormData.AbsentUnit = AbsentUnitData.Substring(0, 1);
            model.FormData.AbsentAmount = AbsentAmount;
            model.FormData.LeaveAmount = (decimal)Session["LeaveAmount"];
            model.FormData.AfterAmount = model.FormData.AbsentAmount - model.FormData.LeaveAmount;
            model.FormData.WorkHours = Convert.ToDecimal(WorkHours);
            bool ChkLeave = model.ChkLeave;

            //取得傳入的請假檢核明細資訊
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local;
            List<EmpAbsentCheckDetail> empAbsentCheckDetailList = JsonConvert.DeserializeObject<List<EmpAbsentCheckDetail>>(EmpAbsentCheckDetailList, setting);
            //throw new Exception();

            if (UploadFilePath != null && UploadFilePath.ContentLength > 4000000)
            {
                //100mb= 800000000 bits
                //TempData["message"] = "檔案上限為100MB.";
                //return View(model);
                return Json(new AjaxResult() { status = "failed", message = "檔案上限為4MB" });
            }
            else
            {
                int IsSuccess = 1;
                try
                {
                    if ((string)Session["Mode"] == FormEditMode.Create.ToString())
                    {
                        model.FormData.Createdby = CurrentUser.EmployeeID;
                        model.FormData.Status = (int)FormStatus.Draft;
                        model.FormData.ID = Guid.NewGuid();
                        if (!string.IsNullOrEmpty(agentNo))
                        {
                            model.FormData.AgentID = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, agentNo).ID;
                        }
                        model.FormData.CompanyID = CurrentUser.Employee.CompanyID;
                        model.FormData.EmployeeID = CurrentUser.EmployeeID;
                        model.FormData.DepartmentID = CurrentUser.Employee.SignDepartmentID;
                        IsSuccess = Services.GetService<LeaveFormService>().Create(model.FormData, UploadFilePath);
                    }
                    else
                    {
                        LeaveForm _form = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == model.FormData.FormNo);
                        if (_form.Status >= (int)FormStatus.Signing)
                        {
                            IsSuccess = 0;
                        }
                        else
                        {
                            _form.StartTime = model.FormData.StartTime;
                            _form.EndTime = model.FormData.EndTime;
                            _form.IsAbroad = model.FormData.IsAbroad;
                            _form.AbsentCode = model.FormData.AbsentCode;
                            _form.AbsentAmount = model.FormData.AbsentAmount;
                            _form.AbsentUnit = model.FormData.AbsentUnit;
                            _form.LeaveAmount = model.FormData.LeaveAmount;
                            _form.AfterAmount = model.FormData.AfterAmount;
                            _form.WorkHours = Convert.ToDecimal(WorkHours);

                            if (!string.IsNullOrEmpty(agentNo))
                            {
                                _form.AgentID = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, agentNo).ID;
                            }
                            else
                            {
                                _form.AgentID = null;
                            }
                            _form.LeaveReason = model.FormData.LeaveReason;
                            _form.Modifiedby = CurrentUser.EmployeeID;
                            _form.ModifiedTime = DateTime.Now;
                            IsSuccess = Services.GetService<LeaveFormService>().Update(_form, UploadFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new AjaxResult() { status = "failed", message = ex.Message });
                }

                //20180208 判斷是否需要檢核，等於false存入檢核明細
                //if (!ChkLeave)
                //{
                    #region 20170629 Start Daniel 存入檢核明細到後台的PortalFormDetail表格
                    //20170629 Start Daniel 存入檢核明細到後台的PortalFormDetail表格
                    /* //20220905 小榜 移到產生完簽核流程後在記錄檢核明細
                    if (IsSuccess == 1)
                    {
                        try
                        {
                            RequestResult _portalFormSaveResult = await HRMApiAdapter.SavePortalFormDetail(
                                                                                        model.FormData.ID.ToString(),
                                                                                        model.FormData.FormNo,
                                                                                        CurrentUser.Employee.Employee_ID.Value,
                                                                                        CurrentUser.EmployeeNO,
                                                                                        model.FormData.StartTime,
                                                                                        model.FormData.EndTime,
                                                                                        model.FormData.AbsentUnit,
                                                                                        WorkHours,
                                                                                        Convert.ToDouble(model.FormData.LeaveAmount),
                                                                                        empAbsentCheckDetailList,
                                                                                        CurrentUser.EmployeeNO

                                                                                                    );
                            IsSuccess = _portalFormSaveResult.Status ? 1 : 0;
                        }
                        catch (Exception ex)
                        {
                            return Json(new AjaxResult() { status = "failed", message = ex.Message });
                        }

                    }
                    */
                    //20170629 End
                    #endregion
                //}

                if (IsSuccess == 1)
                {
                    //TempData["message"] = "成功";
                    WriteLog("Success:" + model.FormData.ID);

                    model.FormData = Services.GetService<LeaveFormService>().Where(x => x.ID == model.FormData.ID).Include(
                        x => x.Company).Include(x => x.Department).FirstOrDefault();

                    try
                    {
                        //SignFlow
                        LeaveFormData _formData = new LeaveFormData(model.FormData);
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
                            /*
                            if (flag == 0)
                            {
                                //特定人員，不需要代理人，直接跳過
                                if (CurrentUser.EmployeeNO == "00630" || CurrentUser.EmployeeNO == "00733" || CurrentUser.EmployeeNO == "01189" || CurrentUser.EmployeeNO == "00242" || CurrentUser.EmployeeNO == "01034" || CurrentUser.EmployeeNO == "01098")
                                {
                                }
                                else
                                {
                                    //抓取假單資料
                                    LeaveForm _form = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == model.FormData.FormNo);
                                    //抓取假別資料
                                    List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
                                    string AbsentCodee = null;
                                    foreach (var i in data)
                                    {
                                        if (i.Code == _form.AbsentCode)
                                        {
                                            AbsentCodee = i.Name;//抓取假別名稱
                                        }
                                    }

                                    //抓取代理人編號
                                    Employee _agent = null;
                                    _agent = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _formData.AgentNo);
                                    //代理人姓名
                                    var _agentName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _formData.AgentNo).Select(x => x.EmployeeName).FirstOrDefault();
                                    //申請人姓名
                                    var CUserName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _formData.CUser).Select(x => x.EmployeeName).FirstOrDefault();
                                    //內容資料
                                    //string _body = (CurrentUser.Employee.EmployeeName + "的請假單" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "設定" + _agent.EmployeeName + "為代理人");
                                    //string _body = (CurrentUser.Employee.EmployeeName + "於" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "間因請假，已指定" + _agent.EmployeeName + "為考勤簽核代理人，特此通知。");//修改代理人通知內文 Irving 20170727

                                    //20180301 Daniel 修改代理人通知內文，鼎鼎自己沒有使用簽核代理人機制
                                    string _body = (CurrentUser.Employee.EmployeeName + "於" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "間因請假，已設定" + _agent.EmployeeName + "為代理人，特此通知。");
                                    if (_agent != null)
                                    {
                                        List<string> _rcpt = new List<string>();
                                        _rcpt.Add(_agent.Email);
                                        string _subject = CUserName + "指定" + _agentName + "為代理人通知";
                                        SendMail(_rcpt.ToArray(), null, null, _subject, _body, true);
                                    }
                                }
                                //end Irving
                            }
                            */
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

                        #region //送假單時抓取信件代理人,並寄信通知 //Irving20171221
                        var SignerEmpData = Services.GetService<EmployeeService>().FirstOrDefault(x => x.EmployeeNO == _formData.EmployeeNo);
                        if (SignerEmpData != null)
                        {
                            if (SignerEmpData.DesignatedPerson != null)
                            {
                                //抓取假單資料
                                LeaveForm _form = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == model.FormData.FormNo);
                                //抓取假別資料
                                List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
                                string AbsentCodee = null;
                                foreach (var i in data)
                                {
                                    if (i.Code == _form.AbsentCode)
                                    {
                                        AbsentCodee = i.Name;//抓取假別名稱
                                    }
                                }

                                //抓取指定送件人編號
                                Employee DesignatedPerson = null;
                                Guid SignerEmpDataGuid = Guid.Parse(SignerEmpData.DesignatedPerson);
                                DesignatedPerson = Services.GetService<EmployeeService>().GetAll().Where(x => x.ID == SignerEmpDataGuid).FirstOrDefault();
                                //申請人姓名
                                var CUserName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _formData.CUser).Select(x => x.EmployeeName).FirstOrDefault();
                                //內容資料
                                //string _body = (CurrentUser.Employee.EmployeeName + "的請假單" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "設定" + _agent.EmployeeName + "為代理人");
                                string _body = (CurrentUser.Employee.EmployeeName + "於" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "間請假，已指定" + DesignatedPerson.EmployeeName + "為信件代理人，特此通知。");//修改信件代理人通知內文 Irving 20171221
                                if (DesignatedPerson != null)
                                {
                                    List<string> _rcpt = new List<string>();
                                    _rcpt.Add(DesignatedPerson.Email);
                                    string _subject = CUserName + "指定" + DesignatedPerson.EmployeeName + "為送件指定人通知";
                                    SendMail(_rcpt.ToArray(), null, null, _subject, _body, true);
                                }

                            }
                        }
                        #endregion

                        List<AbsentType> listAbsent = (List<AbsentType>)Session["Absents"];
                        //SignMailHelper _mailHelper = new SignMailHelper(await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode));
                        SignMailHelper _mailHelper = new SignMailHelper(listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentName), listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentEnglishName));

                        _signList.OnFlowAccepted += _mailHelper.SendMailOnFlowAccepted;

                        _signList.Accept(_formData.FormNumber, _senderFlowId, CurrentUser.EmployeeNO, string.Empty);


                        //20180316 Start Daniel 請假單送出，更新ePortal簽核箱
                        SignFlowRecRepository signFlowRecRepository = new SignFlowRecRepository();
                        //找出目前表單編號對應的流程中，現在需要簽核的人為何
                        //FormNo在Portal不會重覆
                        var signFlowList = signFlowRecRepository.GetAll().Where(x => x.FormNumber == _formData.FormNumber).ToList();
                        var signerFlow = signFlowList.Where(x => x.SignStatus == "W").OrderBy(y => y.SignOrder).FirstOrDefault(); //基本上一定有資料
                        //還需要找出此單最早是什麼時候申請的
                        var firstFlow = signFlowList.OrderBy(x => x.SignOrder).FirstOrDefault(); //基本上一定有資料
                        DateTime applyDate = firstFlow.CDate;
                        //string applyEmpID = firstFlow.CUser;

                        if (signerFlow != null)
                        {
                            string signerID = signerFlow.SignerID;
                            SendFormType sendType = SendFormType.Added; //送出一定是新增
                            SignFlowToEPortalDetailModel ePortalData = new SignFlowToEPortalDetailModel()
                            {
                                empno = signerID, //待簽核主管工號
                                docno = _formData.FormNumber, //表單編號
                                formdesc = "請假單申請",
                                sendtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), //送簽時間
                                sendname = this.CurrentUser.Employee.EmployeeName, //送簽(申請)人員
                                appdate = applyDate.ToString("yyyy-MM-dd"), //申請日期
                                memo = sendType.ToString(),
                            };

                            var ePortalService = Services.GetService<SignFlowToEPortalService>();
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                            string result = ePortalService.UpdateEPortal(sendType, ePortalData, this.CurrentUser.EmployeeID, this.Request.UserHostAddress, controllerName, actionName);
                            //result先不判斷狀況，更新ePortal失敗不應該擋住Portal流程

                        }
                        //20180316 End

                        SystemSettingService systemSettingService = Services.GetService<SystemSettingService>();
                        //20190506 Daniel 新增一筆請假資料到SmartSheet
                        //先檢查部門
                        string SmartSheetDepartments = systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                        if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                        {
                            List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                            if (ssDeptList.Contains(this.CurrentUser.SignDepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                            {
                                var smartSheetService = new SmartSheetrIntegrationService();
                                string result = smartSheetService.Create(model.FormData, AbsentName, AbsentEngName, AbsentQuota);
                                WriteLog("假單申請新增SmartSheet紀錄：" + result);

                            }
                        }

                        //20201119 Daniel 新增一筆請假資料到BambooHR
                        //先檢查部門與假別
                        string BambooHRDepartments = systemSettingService.GetSettingValue("BambooHR_DepartmentList");
                        string BambooHRAbsentCodes = systemSettingService.GetSettingValue("BambooHR_AbsentCodeList");
                        if (!string.IsNullOrWhiteSpace(BambooHRDepartments) && !string.IsNullOrWhiteSpace(BambooHRAbsentCodes))
                        {
                            List<string> bbDeptList = BambooHRDepartments.Split(';').ToList();
                            List<string> bbAbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();
                            if (bbDeptList.Contains(this.CurrentUser.SignDepartmentCode) && bbAbsentCodeList.Contains(model.FormData.AbsentCode)) //符合設定的部門與假別，要傳資料到BambooHR
                            {
                                var bbService = new BambooHRIntegrationService(this.LogInfo);
                                string result = bbService.CreateBambooHRLeaveForm(model.FormData);
                                WriteLog("假單申請新增BambooHR Time Off Request：" + result);
                            }
                        }

                        model.FormData.Status = (int)FormStatus.Signing;
                        Services.GetService<LeaveFormService>().Update(model.FormData);

                        try
                        {
                            RequestResult _portalFormSaveResult = await HRMApiAdapter.SavePortalFormDetail(
                                                                                        model.FormData.ID.ToString(),
                                                                                        model.FormData.FormNo,
                                                                                        CurrentUser.Employee.Employee_ID.Value,
                                                                                        CurrentUser.EmployeeNO,
                                                                                        model.FormData.StartTime,
                                                                                        model.FormData.EndTime,
                                                                                        model.FormData.AbsentUnit,
                                                                                        WorkHours,
                                                                                        Convert.ToDouble(model.FormData.LeaveAmount),
                                                                                        empAbsentCheckDetailList,
                                                                                        CurrentUser.EmployeeNO

                                                                                                    );
                            //IsSuccess = _portalFormSaveResult.Status ? 1 : 0;
                            if (!_portalFormSaveResult.Status)
                            {
                                return Json(new AjaxResult() { status = "failed", message = _portalFormSaveResult.Message });
                            }
                        }
                        catch (Exception ex)
                        {
                            return Json(new AjaxResult() { status = "failed", message = ex.Message });
                        }
                    }
                    catch (Exception ex)
                    {
                        model.FormData.IsDeleted = true;
                        Services.GetService<LeaveFormService>().Update(model.FormData);
                        return Json(new AjaxResult() { status = "failed", message = ex.Message });
                    }

                    //return RedirectToAction("Index");
                    return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
                }
            }
            //重整
            //ViewData["AgentList"] = await GetDepartmentList(agentNo);
            //ViewData["LeaveAmountList"] = await GetLeaveAmountList(LeaveAmountData);
            //ViewBag.LeaveAmountData = LeaveAmountData;
            //ViewBag.AgentData = agentNo;
            //SetBaseUserInfo();
            //return View(model);
            return Json(new AjaxResult() { status = "failed", message = "送出失敗" });
        }
       
        [HttpPost]
        public async Task<ActionResult> CheckEmployeeLeave(LeaveFormViewModel model, string agentNo, string AbsentCode)
        {
            if (string.IsNullOrWhiteSpace(AbsentCode))
            {
                return Json(new AjaxResult() { status = "failed", message = "請選擇假別" });
            }
            //if (string.IsNullOrWhiteSpace(agentNo))
            //{
            //    return Json(new AjaxResult() { status = "failed", message = "請選擇代理人" });
            //}
            if (DateTime.Compare(model.FormData.StartTime, model.FormData.EndTime) > 0)
            {
                return Json(new AjaxResult() { status = "failed", message = "結束時間大於開始時間" });
            }
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResult() { status = "failed", message = "驗證失敗請檢查頁面資料" });
            }
            if (Services.GetService<LeaveFormService>().CheckLeaveFormExist(model.FormData.StartTime, model.FormData.EndTime, CurrentUser.EmployeeID))
            {
                return Json(new AjaxResult() { status = "failed", message = "已有此區間假單" });
            }
            if (await HRMApiAdapter.CheckIfEmployeeLeave(CurrentUser.Employee.Company.CompanyCode,
                CurrentUser.Employee.Department.DepartmentCode, CurrentUser.Employee.EmployeeNO, model.FormData.StartTime, model.FormData.EndTime))
            {
                return Json(new AjaxResult() { status = "failed", message = "此區間已休假" });
            }
            return Json(new AjaxResult() { status = "success", message = string.Empty });
        }

        #region 判斷代理人是否休假
        [HttpPost]
        public async Task<ActionResult> CheckAgentLeave(LeaveFormViewModel model, string agentNo)
        {
            if (string.IsNullOrEmpty(agentNo))
            {
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }

            Employee _agent = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, agentNo);
            if (Services.GetService<LeaveFormService>().CheckLeaveFormExist(model.FormData.StartTime, model.FormData.EndTime, _agent.ID))
            {
                return Json(new AjaxResult() { status = "failed", message = "代理人在此區間已請假" });
            }
            if (await HRMApiAdapter.CheckIfEmployeeLeave(_agent.Company.CompanyCode,
                _agent.Department.DepartmentCode, _agent.EmployeeNO, model.FormData.StartTime, model.FormData.EndTime))
            {
                return Json(new AjaxResult() { status = "failed", message = "代理人在此區間已休假" });
            }
            return Json(new AjaxResult() { status = "success", message = string.Empty });
        }

        #endregion
        
        [HttpPost]
        public async Task<ActionResult> CheckLeave(LeaveFormViewModel model, string agentNo, string AbsentCode, decimal AbsentAmount, string CanOverdraft)
        {
            model.FormData.AbsentCode = AbsentCode;
            model.FormData.AbsentAmount = AbsentAmount;
            //請假卡控不能先請下個月的假(總公司除外)  Irving 20170612
            //if (CurrentUser.CompanyCode != "1010")
            //{
            //    CheckLeaveResponse _checkScheduleResult = await HRMApiAdapter.CheckScheduleResult(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.Employee.EmployeeNO, model.FormData.AbsentCode, agentNo, DateTime.Now, model.FormData.StartTime, model.FormData.EndTime);
            //    if (_checkScheduleResult.Status.ToString() != null)
            //    {
            //        if (_checkScheduleResult.Status.ToString() == "False")
            //        {
            //            return Json(new CheckLeaveResponse() { Status = false, Message = _checkScheduleResult.Message });

            //        }
            //        if (_checkScheduleResult.Type == "N")
            //        {
            //            return Json(new CheckLeaveResponse() { Status = false, Message = "請假區間內無班表匯入,請匯入新班表!" });

            //        }
            //    }
            //}
            //End  Irving

            //計算扣掉空中飄的時數後的剩餘時數是否為0或是負值
            //20170613 Daniel，配合遠百補申請假別調整，取出所有還有剩餘時數的假，取得假別邏輯與請假單假別下拉欄位的邏輯需一致，要不然會有問題
            //List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
            List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now, "remaining");

            //取得所有目前該員工還在空中飄的所有假別時數(因為這些只有前台有資料) //20181203 Daniel 移除過渡期判斷邏輯，目前已經不需要了
            //Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(CurrentUser.EmployeeID);
            
            AbsentDetail absentData = data.Where(x => x.Code == AbsentCode).FirstOrDefault();
            //該假別有空中飄的需扣掉
            /*
            if (notApprovedAbsentAmount.ContainsKey(absentData.Code))
            {
                absentData.UseAmount -= notApprovedAbsentAmount[absentData.Code];
            }
            */

            decimal FutureHours = absentData.FutureTotal - absentData.FutureUsed;
            //判斷目前剩餘時數扣掉未簽核時數是否小於等於0 如果小於等於0就無法再請了 P.s CanOverdraft= False <-為不可預支假
            //20170613 Daniel，遠百需可補申請假單，剩餘時數判定需再加上逾期未休時數，這邊會有問題，因為逾期未休並未設定檢查區間，同年度都會抓出來
            //20170614 Daniel，加上預先申請未來生效的假時數判斷
            //if (absentData.UseAmount <= 0 && CanOverdraft=="False")
            if ((absentData.UseAmount + absentData.OverdueHours + FutureHours) <= 0 && CanOverdraft == "False")
            {
                return Json(new CheckLeaveResponse() { Status = false, Message = "剩餘可休時數不足!" });
            }

            //FormSetting FormSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode).FirstOrDefault(x => x.FormType == "Leave" && x.SettingKey == "isCheck");
            
            //20170627 Daniel 增加回傳檢核明細
            CheckLeaveDetailResponse _checkResult = await CheckLeaveDataWithDetail(model.FormData, agentNo);
            
            //CheckLeaveResponse _checkResult = await CheckLeaveData(model.FormData, agentNo);
            if (!_checkResult.Status)
            {
                //pop up win 假單不符合的提醒，邏輯不合及太長要斷句 Irving 20161206
                if (_checkResult.Message.Length > 48)
                {
                    string _checkResultString = _checkResult.Message.Substring(33);
                    return Json(new CheckLeaveResponse() { Status = false, Message = _checkResult.Message.Substring(0, 33) + "<br>" + _checkResultString });
                }
                else
                {
                    return Json(new CheckLeaveResponse() { Status = false, Message = _checkResult.Message });
                }
                //End
            }
            else
            {
                Session["LeaveAmount"] = _checkResult.AbsentAmount;
                //判斷目剩餘時數扣掉當下請假時數是否小於0 如果小於0就無法再請了 P.s CanOverdraft= False <-為不可預支假
                //20170613 Daniel，遠百需可補申請假單，剩餘時數判定需再加上逾期未休時數
                //20170614 Daniel，加上預先申請未來生效的假時數判斷
                //if (absentData.UseAmount - _checkResult.AbsentAmount < 0 && CanOverdraft == "False")
                if (absentData.UseAmount + absentData.OverdueHours + FutureHours - _checkResult.AbsentAmount < 0 && CanOverdraft == "False")
                {
                    return Json(new CheckLeaveResponse() { Status = false, Message = "剩餘可休時數不足!" });
                }
                //20170627 Daniel 增加回傳請假明細
                return Json(new CheckLeaveDetailResponse()
                {
                    Status = true,
                    Message = string.Format(
                       "<div class='dl-info dl-horizontal'><label class='col-md-22'>" + MultiLanguage.Resource.StartingDate + "</label><p class='form-control-static'>{0} ~ {1}</p><label class='col-md-22'>" + MultiLanguage.Resource.AppliedLeaveHours + "</label><p class='form-control-static'>{2}{3}</p><label class='col-md-22'>" + MultiLanguage.Resource.CategoryOfLeave + "</label><p class='form-control-static'>{4}</p></div>",
                        _checkResult.BeginTime.ToString("yyyy/MM/dd HH:mm"),
                        _checkResult.EndTime.ToString("yyyy/MM/dd HH:mm"),
                        _checkResult.AbsentAmount,
                        model.AbsentUnitData == AbsentUnit.hour ? MultiLanguage.Resource.Hour : MultiLanguage.Resource.dayy,
                        _checkResult.AbsentName),
                    WorkHours = _checkResult.WorkHours,
                    EmpAbsentCheckDetailList=_checkResult.EmpAbsentCheckDetailList
                }
                );
                /*
                return Json(new CheckLeaveResponse()
                    {
                        Status = true,
                        Message = string.Format(
                           "<div class='dl-info dl-horizontal'><label class='col-md-22'>" + MultiLanguage.Resource.StartingDate + "</label><p class='form-control-static'>{0} ~ {1}</p><label class='col-md-22'>" + MultiLanguage.Resource.AppliedLeaveHours + "</label><p class='form-control-static'>{2}{3}</p><label class='col-md-22'>" + MultiLanguage.Resource.CategoryOfLeave + "</label><p class='form-control-static'>{4}</p></div>",
                            _checkResult.BeginTime.ToString("yyyy/MM/dd HH:mm"),
                            _checkResult.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _checkResult.AbsentAmount,
                            model.AbsentUnitData == AbsentUnit.hour ? MultiLanguage.Resource.Hour : MultiLanguage.Resource.dayy,
                            _checkResult.AbsentName),
                        WorkHours = _checkResult.WorkHours
                    }
                );
                */
            }
        }

        [HttpPost]
        public async Task<ActionResult> CheckLeave2(LeaveFormViewModel model, string agentNo, string AbsentCode, decimal AbsentAmount, string CanOverdraft)
        {
            model.FormData.AbsentCode = AbsentCode;
            model.FormData.AbsentAmount = AbsentAmount;
            //請假卡控不能先請下個月的假(總公司除外)  Irving 20170612
            //if (CurrentUser.CompanyCode != "1010")
            //{
            //    CheckLeaveResponse _checkScheduleResult = await HRMApiAdapter.CheckScheduleResult(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.Employee.EmployeeNO, model.FormData.AbsentCode, agentNo, DateTime.Now, model.FormData.StartTime, model.FormData.EndTime);
            //    if (_checkScheduleResult.Status.ToString() != null)
            //    {
            //        if (_checkScheduleResult.Status.ToString() == "False")
            //        {
            //            return Json(new CheckLeaveResponse() { Status = false, Message = _checkScheduleResult.Message });

            //        }
            //        if (_checkScheduleResult.Type == "N")
            //        {
            //            return Json(new CheckLeaveResponse() { Status = false, Message = "請假區間內無班表匯入,請匯入新班表!" });

            //        }
            //    }
            //}
            //End  Irving

            //計算扣掉空中飄的時數後的剩餘時數是否為0或是負值
            //20170613 Daniel，配合遠百補申請假別調整，取出所有還有剩餘時數的假，取得假別邏輯與請假單假別下拉欄位的邏輯需一致，要不然會有問題
            //List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
            AbsentDetailAll data = await HRMApiAdapter.GetEmployeeAbsent2(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now, "remaining");
            //計算扣掉空中飄的時數後的剩餘時數是否為0或是負值
            List<AbsentDetail> data1 = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
            //取得所有目前該員工還在空中飄的所有假別時數(因為這些只有前台有資料)，過渡期還是需要 //20181203 Daniel 移除過渡期判斷邏輯，目前已經不需要了
            //Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(CurrentUser.EmployeeID);

            //AbsentDetail absentData = data.Where(x => x.Code == AbsentCode).FirstOrDefault();
          
            //取得目前、逾期與未來的剩餘時數(這個剩餘時數已經扣除簽核中時數了)
            //目前剩餘時數
            //20170829 Daniel 剩餘時數如果是負數就歸零
            //20190506 Daniel 增加總核假時數計算與取得假別英文名稱
            string absentEngName = "";

            List<AbsentDetail> absentDetailList_Now = data.AbsentDetail_Now.Where(x => x.Code == AbsentCode).ToList();
            decimal amountRemainNow = absentDetailList_Now.Sum(x => x.UseAmount);
            amountRemainNow = (amountRemainNow >= 0) ? amountRemainNow : 0;
            decimal amountAllNow = absentDetailList_Now.Sum(x => x.AllLeaveHours); //注意請假單位已經都轉換過了
            absentEngName = absentDetailList_Now.Count > 0 ? absentDetailList_Now.First().AbsentNameEn : "";

            //逾期剩餘時數
            List<AbsentDetail> absentDetailList_Overdue = data.AbsentDetail_Overdue.Where(x => x.Code == AbsentCode).ToList();
            decimal amountRemainOverdue = absentDetailList_Overdue.Sum(x => x.UseAmount);
            amountRemainOverdue = (amountRemainOverdue >= 0) ? amountRemainOverdue : 0;
            decimal amountAllOverdue = absentDetailList_Overdue.Sum(x => x.AllLeaveHours); //注意請假單位已經都轉換過了
            if (string.IsNullOrWhiteSpace(absentEngName)) //前面如果沒有此假別資料，才再找一次
            {
                absentEngName = absentDetailList_Overdue.Count > 0 ? absentDetailList_Overdue.First().AbsentNameEn : "";
            }

            //未來剩餘時數
            List<AbsentDetail> absentDetailList_Future = data.AbsentDetail_Future.Where(x => x.Code == AbsentCode).ToList();
            decimal amountRemainFuture = absentDetailList_Future.Sum(x => x.UseAmount);
            amountRemainFuture = (amountRemainFuture >= 0) ? amountRemainFuture : 0;
            decimal amountAllFuture = absentDetailList_Future.Sum(x => x.AllLeaveHours); //注意請假單位已經都轉換過了
            if (string.IsNullOrWhiteSpace(absentEngName)) //前面如果沒有此假別資料，才再找一次
            {
                absentEngName = absentDetailList_Future.Count > 0 ? absentDetailList_Future.First().AbsentNameEn : "";
            }

            //全部剩餘時數
            decimal amountRemainTotal = amountRemainNow + amountRemainOverdue + amountRemainFuture;

            //全部核假時數(只加總目前與未來的)
            decimal absentQuota = amountAllNow + amountAllFuture;
            
            //取得目前、逾期與未來的剩餘但不含簽核中的時數(為了過渡期額外處理的)
            //目前剩餘不含簽核中時數
            //20170829 Daniel 剩餘時數如果是負數就歸零
            /*
            decimal amountRemainNow2 = data.AbsentDetail_Now.Where(x => x.Code == AbsentCode).Sum(y => (y.AllLeaveHours-y.LeaveHours));
            amountRemainNow2 = (amountRemainNow2 >= 0) ? amountRemainNow2 : 0;

            //逾期剩餘不含簽核中時數
            decimal amountRemainOverdue2 = data.AbsentDetail_Overdue.Where(x => x.Code == AbsentCode).Sum(y => (y.AllLeaveHours - y.LeaveHours));
            amountRemainOverdue2 = (amountRemainOverdue2 >= 0) ? amountRemainOverdue2 : 0;

            //未來剩餘不含簽核中時數
            decimal amountRemainFuture2 = data.AbsentDetail_Future.Where(x => x.Code == AbsentCode).Sum(y => (y.AllLeaveHours - y.LeaveHours));
            amountRemainFuture2 = (amountRemainFuture2 >= 0) ? amountRemainFuture2 : 0;

            //全部剩餘不含簽核中時數
            decimal amountRemainTotal2 = amountRemainNow2 + amountRemainOverdue2 + amountRemainFuture2;
            */
            
            //過渡期改抓前台簽核中時數
            //該假別有空中飄的需扣掉

            //decimal amountPortalSigningTotal = (notApprovedAbsentAmount.ContainsKey(AbsentCode)) ? notApprovedAbsentAmount[AbsentCode] : 0;
            //decimal amountPortalSigningTotal = 0; 

            AbsentDetail absentData = data1.Where(x => x.Code == AbsentCode).FirstOrDefault();
            /*
            if (notApprovedAbsentAmount.ContainsKey(absentData.Code))
            {
                absentData.UseAmount -= notApprovedAbsentAmount[absentData.Code];
            }
            */
            //decimal FutureHours = absentData.FutureTotal - absentData.FutureUsed;
            
            //判斷目前剩餘時數扣掉未簽核時數是否小於等於0 如果小於等於0就無法再請了 P.s CanOverdraft= False <-為不可預支假
            //20170613 Daniel，遠百需可補申請假單，剩餘時數判定需再加上逾期未休時數，這邊會有問題，因為逾期未休並未設定檢查區間，同年度都會抓出來
            //20170614 Daniel，加上預先申請未來生效的假時數判斷
            //201707 Daniel 更新計算可休時數邏輯，剩餘時數加總>0，就可以請假
            //if ((absentData.UseAmount + absentData.OverdueHours + FutureHours) <= 0 && CanOverdraft == "False")
            if (amountRemainTotal <= 0 && CanOverdraft.ToLower() == "false")
            {
                return Json(new CheckLeaveResponse() { Status = false, Message = "剩餘可休時數不足!" });
            }

            //FormSetting FormSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode).FirstOrDefault(x => x.FormType == "Leave" && x.SettingKey == "isCheck");

            

            if (model.ChkLeave)
            {

                CheckLeaveResponse _checkResult = await CheckLeaveData(model.FormData, agentNo);
                // 20180208 小榜：當勾選不檢核時依輸入的時數為單位
                decimal dAbsentAmount = model.IsCheckedAmount = (absentData.Unit == "d" ? model.IsCheckedAmount / 8 : model.IsCheckedAmount);

                //判斷不檢核時間是否小於等於0
                if (model.ChkLeave == true && (double)model.FormData.AbsentAmount < 0.5 && absentData.Unit != "d")
                {
                    return Json(new CheckLeaveResponse() { Status = false, Message = "不檢核時間不可小於 0.5" });
                }
                else if (model.ChkLeave == true && dAbsentAmount < 8 && absentData.Unit == "d")
                {
                    return Json(new CheckLeaveResponse() { Status = false, Message = "不檢核時間不可小於 8" });
                }
                Session["LeaveAmount"] = dAbsentAmount;
                //判斷目剩餘時數扣掉當下請假時數是否小於0 如果小於0就無法再請了 P.s CanOverdraft= False <-為不可預知假
                if (absentData.UseAmount - model.FormData.AbsentAmount < 0 && CanOverdraft == "False")
                {
                    return Json(new CheckLeaveResponse() { Status = false, Message = "剩餘可休時數不足!" });
                }
                return Json(new CheckLeaveResponse()
                {
                    Status = true,
                    Message = string.Format(
                        "<dl class='dl-info dl-horizontal'><dt>" + MultiLanguage.Resource.StartingDate + "</dt><dd>{0} ~ {1}</dd><dt>" + MultiLanguage.Resource.AppliedLeaveHours + "</dt><dd>{2}{3}</dd></dl>",
                        model.FormData.StartTime.ToString("yyyy/MM/dd HH:mm"),
                        model.FormData.EndTime.ToString("yyyy/MM/dd HH:mm"),
                        dAbsentAmount,
                        model.AbsentUnitData == AbsentUnit.hour ? MultiLanguage.Resource.Hour : MultiLanguage.Resource.dayy),
                    WorkHours = _checkResult.WorkHours
                }
                );
            }
            else
            {
                #region 要檢核
                //20170627 Daniel 增加回傳檢核明細
                CheckLeaveDetailResponse _checkResult = await CheckLeaveDataWithDetail(model.FormData, agentNo);

                if (!_checkResult.Status)
                {
                    #region 檢核失敗
                    //pop up win 假單不符合的提醒，邏輯不合及太長要斷句 Irving 20161206
                    if (_checkResult.Message.Length > 48)
                    {
                        string _checkResultString = _checkResult.Message.Substring(33);
                        return Json(new CheckLeaveResponse() { Status = false, Message = _checkResult.Message.Substring(0, 33) + "<br>" + _checkResultString });
                    }
                    else
                    {
                        return Json(new CheckLeaveResponse() { Status = false, Message = _checkResult.Message });
                    }
                    //End

                    #endregion
                }
                else
                {
                    #region 檢核成功

                    //判斷目剩餘時數扣掉當下請假時數是否小於0 如果小於0就無法再請了 P.s CanOverdraft= False <-為不可預支假
                    //if (absentData.UseAmount - _checkResult.AbsentAmount < 0 && CanOverdraft == "False")
                    if (amountRemainTotal - _checkResult.AbsentAmount < 0 && CanOverdraft.ToLower() == "false")
                    {
                        return Json(new CheckLeaveResponse() { Status = false, Message = "剩餘可休時數不足!" });
                    }

                    Session["LeaveAmount"] = _checkResult.AbsentAmount;

                    //20170627 Daniel 增加回傳請假明細
                    return Json(new CheckLeaveDetailResponse()
                    {
                        Status = true,
                        Message = string.Format(
                           "<div class='dl-info dl-horizontal'><label class='col-md-22'>" + MultiLanguage.Resource.StartingDate + "</label><p class='form-control-static'>{0} ~ {1}</p><label class='col-md-22'>" + MultiLanguage.Resource.AppliedLeaveHours + "</label><p class='form-control-static'>{2}{3}</p><label class='col-md-22'>" + MultiLanguage.Resource.CategoryOfLeave + "</label><p class='form-control-static'>{4}</p></div>",
                            _checkResult.BeginTime.ToString("yyyy/MM/dd HH:mm"),
                            _checkResult.EndTime.ToString("yyyy/MM/dd HH:mm"),
                            _checkResult.AbsentAmount,
                            model.AbsentUnitData == AbsentUnit.hour ? MultiLanguage.Resource.Hour : MultiLanguage.Resource.dayy,
                            _checkResult.AbsentName),
                        WorkHours = _checkResult.WorkHours,
                        AbsentEngName = absentEngName, //20190506 Daniel 增加假別英文名稱與總核假額度
                        AbsentQuota = absentQuota,
                        EmpAbsentCheckDetailList = _checkResult.EmpAbsentCheckDetailList
                    }
                    );
                    #endregion
                }

                #endregion
            }
        }

        #region 取得部門列表
        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata">被選取的部門</param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata = "", List<Employee> DeptAgentList = null)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            //List<Department> data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
            List<Department> data = new List<Department>();
            //取出所有部門物件
            if (DeptAgentList != null)
            {
                //取出所有部門物件
                data = DeptAgentList.Select(x => x.SignDepartment).Distinct().ToList();
            }
            else
            {
                data = Services.GetService<DepartmentService>().GetParentDepartmentDate(this.CurrentUser.DepartmentID).ToList();
            }
            //增加常用代理人選項
            data.Insert(0, new Department { DepartmentCode = commonAgent, DepartmentName = MultiLanguage.Resource.CommonAgent, DepartmentEnglishName = MultiLanguage.Resource.CommonAgent });

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (getLanguageCookie == "en-US")
            {
                listItem.Add(new SelectListItem { Text = "Please Select", Value = "", Selected = (selecteddata == "" ? true : false) });
            }
            else
            {
                listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            }

            foreach (var item in data)
            {
                if (item.EndDate == null || item.EndDate > DateTime.Now)//Irving 增加判斷部門到期日過期及空值不顯示在選單上 2016/03/04
                {
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                }
            }

            return listItem;
        }

        #endregion
        
        #region 取得代理人列表
        /// <summary>
        /// 取得代理人列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetActingPersonList(Guid deptid, string DeptCode = "", string selecteddata = "", string beginDate = "", string endDate = "", List<Employee> DeptEmpAgentList = null)
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            //listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = "", Selected = (selecteddata == "" ? true : false) });

            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = ""});
            List<Employee> employeelist = new List<Employee>();
            string firstAgent = "";
            if (DeptCode == commonAgent)
            {
                //讀取最近20筆請假單
                var leaveData = Services.GetService<LeaveFormService>().GetAll().Where(x => x.EmployeeID == CurrentUser.EmployeeID).OrderByDescending(x => x.CreatedTime).Take(20).ToList();//.Select(x => x.AgentID).Distinct().Take(5).ToList();

                //排除掉非可選單位的人員
                if (DeptEmpAgentList != null)
                {
                    List<LeaveForm> LeaveFormList = new List<LeaveForm>();
                    foreach (var item in DeptEmpAgentList)
                    {
                        for (int i = 0; i <= leaveData.Count - 1; i++)
                        {
                            if (item.ID == leaveData[i].AgentID)
                            {
                                LeaveFormList.Add(leaveData[i]);
                            }
                        }
                    }
                    leaveData = LeaveFormList;
                }

                //依最近20筆請假單取出5位代理人
                var leaveAgent = leaveData.OrderByDescending(x=>x.CreatedTime).Select(x => x.AgentID).Distinct().Take(5).ToList();
                var agentFirst = Services.GetService<EmployeeService>().GetAll().Where(x => x.ID == leaveAgent.FirstOrDefault());//.Where(x => leaveAgent.Contains(x.ID));
                //抓取最後一張假單的代理人，預設選取
                firstAgent = agentFirst == null || agentFirst.Count() <  1 ? "" : agentFirst.FirstOrDefault().EmployeeNO;
                //employeelist = Services.GetService<EmployeeService>().GetAll().Where(x => leaveAgent.Contains(x.ID)).OrderBy(x => x.EmployeeNO).ToList();
                employeelist = Services.GetService<EmployeeService>().GetAll().Where(x => leaveAgent.Contains(x.ID)).OrderBy(x=>x.EmployeeNO).ToList();

            }
            else
            {
                //listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = "", Selected = (selecteddata == "" ? true : false) });
                employeelist = Services.GetService<EmployeeService>().GetListsBySignDepartment(CurrentUser.CompanyID, deptid).OrderBy(x => x.EmployeeNO).ToList();
            }

            DateTime startTime;
            DateTime endTime;
            foreach (var employeeData in employeelist)
            {
                if (beginDate + "" == "" && endDate + "" == "")
                {
                    beginDate = System.DateTime.Now.ToString();
                    endDate = System.DateTime.Now.ToString();
                }
                startTime = DateTime.Parse(beginDate);
                endTime = DateTime.Parse(endDate);
                //讀取代理人假單 

                Employee _agent = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, employeeData.EmployeeNO);

                bool checkLeave = await HRMApiAdapter.CheckIfEmployeeLeave(_agent.Company.CompanyCode,
_agent.Department.DepartmentCode, _agent.EmployeeNO, startTime, endTime);
                var checkLeave1 = Services.GetService<LeaveFormService>().CheckLeaveFormExist(startTime, endTime, _agent.ID);

                //扣除自己
                if ((employeeData.EmployeeNO != CurrentUser.Employee.EmployeeNO))
                {
                    //Irving 增加判斷代理人不顯示離職人員 2016/03/03
                    //小榜 增加判斷代理人在區間內是否休假，已休假不顯示 2017/11/20
                    //if ((employeeData.LeaveDate == null || employeeData.LeaveDate > DateTime.Now) && (checkLeave == false) && checkLeave1 == false)
                    //{
                    //    selecteddata = selecteddata == "" ? employeeData.EmployeeNO : selecteddata;

                    //    if (getLanguageCookie == "en-US")
                    //    {
                    //        listItem.Add(new SelectListItem { Text = employeeData.EmployeeNO + " " + employeeData.EmployeeEnglishName, Value = employeeData.EmployeeNO, Selected = (selecteddata == employeeData.EmployeeNO ? true : false) });
                    //    }
                    //    else
                    //    {
                    //        listItem.Add(new SelectListItem { Text = employeeData.EmployeeNO + " " + employeeData.EmployeeName, Value = employeeData.EmployeeNO, Selected = (selecteddata == employeeData.EmployeeNO ? true : false) });
                    //    }
                    //}
                    //Ricky 已休假增加顯示休假 2018/05/14
                    if ((employeeData.LeaveDate == null || employeeData.LeaveDate > DateTime.Now))
                    {
                        var strLeave = checkLeave || checkLeave1 ? (getLanguageCookie == "en-US" ? " (on vacation)" : " (休假)") : string.Empty;
                        var strName = getLanguageCookie == "en-US" ? employeeData.EmployeeEnglishName : employeeData.EmployeeName;
                        selecteddata = selecteddata == "" && string.IsNullOrWhiteSpace(strLeave)  ? employeeData.EmployeeNO : selecteddata;

                        //判斷最近一張假單代理人當日是否休假
                        if (firstAgent == employeeData.EmployeeNO && string.IsNullOrWhiteSpace(strLeave))
                        {
                            selecteddata = firstAgent;
                        }
                        listItem.Add(new SelectListItem
                        {
                            Text = employeeData.EmployeeNO + " " + strName + strLeave,
                            Value = employeeData.EmployeeNO,
                            Selected = selecteddata == employeeData.EmployeeNO,
                            Disabled = !string.IsNullOrWhiteSpace(strLeave)
                        });
                    }
                }
            }

            return listItem;
        }

        #endregion

        #region 給下拉式選單讀取代理人列表
        /// <summary>
        /// 給下拉式選單讀取代理人列表
        /// </summary>
        /// <param name="DeptCode"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetActingPerson(string DeptCode, string beginDate = "", string endDate = "")
        {
            var deptID = Services.GetService<DepartmentService>().GetAll().Where(x => x.DepartmentCode == DeptCode).Where(x => x.Enabled == true).Select(x => x.ID).FirstOrDefault();

            if (deptID == null)
            {
                deptID = Guid.Empty;
            }

            //取得可選擇的部門所有人員資料
            List<Employee> DeptEmpAgentList = await GetDeptAgentAll();

            List<SelectListItem> result = await GetActingPersonList(deptID, DeptCode, "", beginDate, endDate, DeptEmpAgentList);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
          #endregion

        #region 計算時數
        /// <summary>
        /// 計算時數
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetCalculatehHours(string leaveCode = "", string beginDate = "", string endDate = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            DateTime StarTime = Convert.ToDateTime(beginDate);
            DateTime EndTime = Convert.ToDateTime(endDate);

            if (string.IsNullOrWhiteSpace(leaveCode))
            {
                return Json(new AjaxResult() { status = "0", message = HRPortal.MultiLanguage.Resource.Text_SelectLeaveType });
            }

            if (DateTime.Compare(StarTime, EndTime) > 0)
            {//結束時間大於開始時間
                return Json(new AjaxResult() { status = "0", message = HRPortal.MultiLanguage.Resource.Text_DatetimeError });
            }
            //計算時數
            CheckLeaveResponse _result = await HRMApiAdapter.CheckAbsentAmount(CurrentUser.Employee.Company.CompanyCode, leaveCode, CurrentUser.Employee.EmployeeNO, StarTime, EndTime, "true", getLanguageCookie);
            
            _result.AbsentAmount = _result.Unit == "d" ? (_result.AbsentAmount * 8) : _result.AbsentAmount;
            if (_result.AbsentAmount < 0)
            {
                _result.AbsentAmount = 0;
            }
            //20180821 小榜：判斷有無錯誤訊息，當有錯時將時數清空，以顯示訊息
            _result.AbsentAmount = string.IsNullOrEmpty(_result.Message) ? _result.AbsentAmount : 0;
            return Json(new AjaxResult() { status = _result.AbsentAmount + "", message = _result.Message });
        }
        #endregion

        #region 假別名稱
        private string getAbsentLang(string Name)
        {
            string rtn = "";
            switch (Name)
            {
                case "事假":
                    rtn = MultiLanguage.Resource.PersonalLeave;
                    break;
                case "病假":
                    rtn = MultiLanguage.Resource.SickLeave;
                    break;
                case "公假":
                    rtn = MultiLanguage.Resource.OfficialLeave;
                    break;
                case "生理假":
                    rtn = MultiLanguage.Resource.MentrualLeave;
                    break;
                case "無薪生理假":
                    rtn = MultiLanguage.Resource.NoPayMenstrualLeave;
                    break;
                case "曠職":
                    rtn = MultiLanguage.Resource.UnauthorizedAbsence;
                    break;
                case "補休":
                    rtn = MultiLanguage.Resource.CompensatedDayOff;
                    break;
                case "特休":
                    rtn = MultiLanguage.Resource.AnnualLeave;
                    break;
                case "補假":
                    rtn = MultiLanguage.Resource.CompensatedLeave;
                    break;
                case "喪假":
                    rtn = MultiLanguage.Resource.CompassionateLeave;
                    break;
                case "婚假":
                    rtn = MultiLanguage.Resource.MarriageLeave;
                    break;
                case "產假":
                    rtn = MultiLanguage.Resource.MaternityLeave;
                    break;
                case "產檢假":
                    rtn = MultiLanguage.Resource.MaternityCheckLeave;
                    break;
                case "陪產假":
                    rtn = MultiLanguage.Resource.PaternityLeave;
                    break;
                case "公傷":
                    rtn = MultiLanguage.Resource.WorkInjuryLeave;
                    break;
                case "延長病假":
                    rtn = MultiLanguage.Resource.ExtendedSickLeave;
                    break;
                case "家庭照顧假":
                    rtn = MultiLanguage.Resource.FamilyCareLeave;
                    break;
                case "預支特休":
                    rtn = MultiLanguage.Resource.AdvancedPaidAnnualLeave;
                    break;
                case "預支補休":
                    rtn = MultiLanguage.Resource.advanceddayoff;
                    break;
                case "公傷30%":
                    rtn = MultiLanguage.Resource.vocationalinjuryleave;
                    break;
                default:
                    rtn = Name;
                    break;
            }
            return rtn;
        }

        #endregion

        #region 取得目前可請假時數
        /// <summary>
        /// 取得目前可請假時數
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetLeaveAmountList(string selecteddata = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            ViewBag.RemarkData = "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = "", Selected = (selecteddata == "" ? true : false) });
            //20170613 Daniel，取得所有還有剩餘時數的假別
            //List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
            List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now, "remaining");

            //20181203 Daniel 移除過渡期判斷邏輯，目前已經不需要了
            //Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(CurrentUser.EmployeeID);
            //data = data.Where(x => x.CanUse == true).ToList();
            foreach (var item in data.Where(x => x.CanUse == true))//取有給核假的假別 Irving 20161206
            {
                AbsentUnit _unit = item.Unit == "h" ? AbsentUnit.hour : AbsentUnit.day;
                /*
                if (notApprovedAbsentAmount.ContainsKey(item.Code))
                {
                    item.ApprovedHours = notApprovedAbsentAmount[item.Code];
                    item.UseAmount -= notApprovedAbsentAmount[item.Code];
                }
                */

                item.LeaveHours = item.AnnualLeaveHours - item.ApprovedHours - item.UseAmount;

                string valueData = item.Code + "_" + item.UseAmount + "_" + item.Remark + "_" + _unit.ToString() + "_" + item.CanOverdraft;
                if (getLanguageCookie == "en-US")
                {
                    listItem.Add(new SelectListItem
                    {
                        Text = item.AbsentNameEn,
                        Value = valueData,
                        Selected = (item.Code == selecteddata ? true : false)
                    });
                }
                else
                {
                    listItem.Add(new SelectListItem
                    {
                        Text = item.Name,
                        Value = valueData,
                        Selected = (item.Code == selecteddata ? true : false)
                    });
                }
                //恢復LeaveAmountData顯示
                if (item.Code == selecteddata)
                {
                    if (!string.IsNullOrWhiteSpace(item.Remark))
                        ViewBag.RemarkData = "※" + item.Remark;
                    ViewBag.LeaveAmountData = valueData;
                }
                item.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
                item.AbsentNameEn = item.AbsentNameEn != null ? item.AbsentNameEn : item.Name;

                //ViewData["LeaveDatas"] = data;
            }
            ViewData["LeaveDatas"] = data;
            return listItem;
        }

        #endregion        

        private async Task<List<SelectListItem>> GetLeaveAmountList2(string selecteddata = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            ViewBag.RemarkData = "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = "", Selected = (selecteddata == "" ? true : false) });
            //20170613 Daniel，取得所有還有剩餘時數的假別
            //List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
            AbsentDetailAll data = await HRMApiAdapter.GetEmployeeAbsent2(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now, "remaining");

            //過渡期後台有可能沒有簽核資料，但是前台有，所以還是要抓前台簽核中的資訊 //20181203 Daniel 移除過渡期判斷邏輯，目前已經不需要了
            //Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(CurrentUser.EmployeeID);
            
            //data = data.Where(x => x.CanUse == true).ToList();

     
            //整理目前生效的資料
            if (data.AbsentDetail_Now != null)
            {
                foreach (var item in data.AbsentDetail_Now.Where(x => x.CanUse == true))//取有給核假的假別 Irving 20161206
                {

                    item.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
                    item.AbsentNameEn = item.AbsentNameEn != null ? item.AbsentNameEn : item.Name;

                    AbsentUnit _unit = item.Unit == "h" ? AbsentUnit.hour : AbsentUnit.day;

                    //檢查後台簽核中是否為0
                    if (item.ApprovedHours == 0) 
                    {
                        //如果後台無簽核中資料，改檢查前台是否有值
                        /*
                        if (notApprovedAbsentAmount.ContainsKey(item.Code))
                        {
                            item.ApprovedHours = notApprovedAbsentAmount[item.Code];
                            item.UseAmount -= notApprovedAbsentAmount[item.Code]; //剩餘可休也要扣除前台簽核中的
                        }
                        */
                    }
                    
                    string valueData = item.Code + "_" + item.UseAmount + "_" + item.Remark + "_" + _unit.ToString() + "_" + item.CanOverdraft;

                    //將假別選項加入假別下拉選單
                    //取得假別名稱
                    string optionText = (getLanguageCookie == "en-US") ? item.AbsentNameEn : item.Name;

                    //判定是否選單重複加入了，重複就使用前一個假別資訊就可以
                    if (!listItem.Exists(x => x.Value.Split('_')[0] == item.Code)) // 沒有重複就需要新增下拉選項
                    {
                        listItem.Add(new SelectListItem
                        {
                            Text = optionText,
                            Value = valueData,
                            Selected = (item.Code == selecteddata ? true : false)
                        });

                        //回填ViewBag假別備註及選項資訊(ViewBag.LeaveAmountData很多地方會用到，這邊應該要切開比較好)
                        if (item.Code == selecteddata)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Remark))
                            {
                                ViewBag.RemarkData = "※" + item.Remark;
                            }
                            ViewBag.LeaveAmountData = valueData;
                        }
                    }
                   
                    /*
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new SelectListItem
                        {
                            Text = item.AbsentNameEn,
                            Value = valueData,
                            Selected = (item.Code == selecteddata ? true : false)
                        });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem
                        {
                            Text = item.Name,
                            Value = valueData,
                            Selected = (item.Code == selecteddata ? true : false)
                        });
                    }

                    //恢復LeaveAmountData顯示
                    if (item.Code == selecteddata)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Remark))
                            ViewBag.RemarkData = "※" + item.Remark;
                        ViewBag.LeaveAmountData = valueData;
                    }
                    */
           
                    //ViewData["LeaveDatas"] = data;
                }

                //2018/11/7 Neo 假別調整排序
                List<AbsentDetail> leaveDatas = data.AbsentDetail_Now;
                List<AbsentDetail> leaveDatas2 = new List<AbsentDetail>();

                //取得假別優先排序
                string sortLeaveStr = Services.GetService<SystemSettingService>().GetSettingValue("SortLeave");
                string[] sortLeavAry = null;

                if (!string.IsNullOrEmpty(sortLeaveStr))
                {
                    sortLeavAry = sortLeaveStr.Split(';');
                    foreach (var sortLeave in sortLeavAry)
                    {
                        var SLeave = leaveDatas.Where(x => x.Name == sortLeave).FirstOrDefault();
                        if (SLeave != null)
                        {
                            leaveDatas2.Add(SLeave);
                        }
                        leaveDatas = leaveDatas.Where(x => x.Name != sortLeave).ToList();//排除假別優先排序
                    }
                }
                leaveDatas = leaveDatas.OrderBy(x => x.Name.Substring(0)).ToList();//其他假別需依假別名稱第一個字的筆劃來排序(遞增)
                if (leaveDatas != null && leaveDatas.Count() > 0)
                {
                    leaveDatas2.AddRange(leaveDatas);
                }

                //ViewData["LeaveDatas"] = data.AbsentDetail_Now;
                ViewData["LeaveDatas"] = leaveDatas2;

                //從目前生效的假別找出年假(特休假)的最晚結束日期
                List<AbsentDetail> NowData = data.AbsentDetail_Now.Where(x => x.YearCalWayFlag == true).ToList();
                if (NowData.Count > 0)
                {
                    ViewData["Now_SpecialAbsent_EndDate"] = NowData.Max(x => x.EndDateMax);
                }
            }

            //整理逾期失效的資料
            if (data.AbsentDetail_Overdue != null)
            {
                foreach (var item in data.AbsentDetail_Overdue.Where(x => x.CanUse == true))
                {

                    item.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
                    item.AbsentNameEn = item.AbsentNameEn != null ? item.AbsentNameEn : item.Name;
                    
                    AbsentUnit _unit = item.Unit == "h" ? AbsentUnit.hour : AbsentUnit.day;

                    string valueData = item.Code + "_" + item.UseAmount + "_" + item.Remark + "_" + _unit.ToString() + "_" + item.CanOverdraft;

                    //將假別選項加入假別下拉選單
                    //取得假別名稱
                    string optionText = (getLanguageCookie == "en-US") ? item.AbsentNameEn : item.Name;

                    //判定是否選單重複加入了，重複就使用前一個假別資訊就可以
                    if (!listItem.Exists(x => x.Value.Split('_')[0] == item.Code)) // 沒有重複就需要新增下拉選項
                    {
                        listItem.Add(new SelectListItem
                        {
                            Text = optionText,
                            Value = valueData,
                            Selected = (item.Code == selecteddata ? true : false)
                        });

                        //回填ViewBag假別備註及選項資訊(ViewBag.LeaveAmountData很多地方會用到，這邊應該要切開比較好)
                        if (item.Code == selecteddata)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Remark))
                            {
                                ViewBag.RemarkData = "※" + item.Remark;
                            }
                            ViewBag.LeaveAmountData = valueData;
                        }
                    }
             

                    //ViewData["LeaveDatas"] = data;
                }

                ViewData["LeaveDatas_Overdue"] = data.AbsentDetail_Overdue;
            }

            //整理未來生效的資料
            if (data.AbsentDetail_Future != null)
            {
                foreach (var item in data.AbsentDetail_Future.Where(x => x.CanUse == true))
                {

                    item.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
                    item.AbsentNameEn = item.AbsentNameEn != null ? item.AbsentNameEn : item.Name;

                    AbsentUnit _unit = item.Unit == "h" ? AbsentUnit.hour : AbsentUnit.day;

                    string valueData = item.Code + "_" + item.UseAmount + "_" + item.Remark + "_" + _unit.ToString() + "_" + item.CanOverdraft;

                    //將假別選項加入假別下拉選單
                    //取得假別名稱
                    string optionText = (getLanguageCookie == "en-US") ? item.AbsentNameEn : item.Name;

                    //判定是否選單重複加入了，重複就使用前一個假別資訊就可以
                    if (!listItem.Exists(x => x.Value.Split('_')[0] == item.Code)) // 沒有重複就需要新增下拉選項
                    {
                        listItem.Add(new SelectListItem
                        {
                            Text = optionText,
                            Value = valueData,
                            Selected = (item.Code == selecteddata ? true : false)
                        });

                        //回填ViewBag假別備註及選項資訊(ViewBag.LeaveAmountData很多地方會用到，這邊應該要切開比較好)
                        if (item.Code == selecteddata)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Remark))
                            {
                                ViewBag.RemarkData = "※" + item.Remark;
                            }
                            ViewBag.LeaveAmountData = valueData;
                        }
                    }
                 

                    //ViewData["LeaveDatas"] = data;
                }

                //未來生效的假別暫定只顯示特休假(年假)
                List<AbsentDetail> FutureData=data.AbsentDetail_Future.Where(x => x.YearCalWayFlag == true).ToList();
                ViewData["LeaveDatas_Future"] = FutureData;
                if (FutureData.Count > 0)
                {
                    //顯示未來年假最早開始日期
                    ViewData["Future_SpecialAbsent_BeginDate"] = FutureData.Min(x => x.BeginDateMin);
                }
               
            }
            /*
            ViewData["LeaveDatas"] = data.AbsentDetail_Now;
            ViewData["LeaveDatas_Overdue"] = data.AbsentDetail_Overdue;
            ViewData["LeaveDatas_Future"] = data.AbsentDetail_Future;
            */

            return listItem;
        }

        #region 檢核
        /// <summary>
        /// 檢核
        /// </summary>
        /// <param name="model"></param>
        /// <param name="AgentData"></param>
        /// <returns></returns>
        private async Task<CheckLeaveResponse> CheckLeaveData(LeaveForm model, string AgentData)
        {
            //HRMApiAdapter.CheckLeave 卡控檢核
            CheckLeaveResponse _result = await HRMApiAdapter.CheckLeave(CurrentUser.Employee.Company.CompanyCode, model.AbsentCode, CurrentUser.Employee.EmployeeNO, AgentData, model.StartTime, model.EndTime);

            return _result;
        }
        #endregion

        #region 增加回傳檢核明細
        //20170627 Daniel 增加回傳檢核明細
        private async Task<CheckLeaveDetailResponse> CheckLeaveDataWithDetail(LeaveForm model, string AgentData)
        {
            //HRMApiAdapter.CheckLeave 卡控檢核
            CheckLeaveDetailResponse _result = await HRMApiAdapter.CheckLeaveWithDetail(CurrentUser.Employee.Company.CompanyCode, model.AbsentCode, CurrentUser.Employee.EmployeeNO, AgentData, model.StartTime, model.EndTime);

            return _result;
        }

        #endregion

        #region 向後台要排班時間
        /// <summary>
        /// 向後台要排班時間
        /// </summary>
        /// <param name="model"></param>
        /// <param name="AgentData"></param>
        /// <returns></returns>
        public async Task<List<GetEmpScheduleClassTimeResponse>> GetEmpScheduleClassTime(string compayCode, string empID, DateTime excuteDate)
        {
            //HRMApiAdapter.GetEmpScheduleClassTime 獲取排班資料
            List<GetEmpScheduleClassTimeResponse> _result = await HRMApiAdapter.GetEmpScheduleClassTime(compayCode, empID, excuteDate);
            return _result;

        }
        #endregion

        #region 刪除假單
        [HttpPost]
        public ActionResult DeletedForm(LeaveFormViewModel model)
        {
            LeaveForm data = Services.GetService<LeaveFormService>().GetLeaveFormByID(model.FormData.ID);
            int _result = Services.GetService<LeaveFormService>().Delete(data, data, CurrentUser.EmployeeID, true);
            //20160122 刪除編輯中的假單時流程中的假單也要同時刪除
            SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();
            IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(model.FormData.FormNo);
            LeaveSignList _signList = new LeaveSignList();

            if (_signFlow != null || _signFlow.Count != 0)
            {
                foreach (var flowItem in _signFlow)
                {
                    flowItem.IsUsed = DefaultEnum.IsUsed.N.ToString();
                    flowItem.DataState = DefaultEnum.SignFlowDataStatus.Modify.ToString();
                }
                _signList.SaveSigningFlow(_signFlow, null);

            }

            if (_result == 1)
            {
                WriteLog("Success:" + model.FormData.ID);
                return Json(new AjaxResult() { status = "success", message = "刪除成功" });
            }
            else
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
        }
        #endregion
       

        //檢查前台的假單狀態碼是否有確實修改 by 20161205 Bee
        //20170510 修改 by Daniel，原來async void會導致無預期的非同步的錯誤，改成async Task就可以了
        //public async void checkLeaveFormIsNotBuildingToHRM(Guid employeeID, string companyCode)
        public async Task checkLeaveFormIsNotBuildingToHRM(Guid employeeID, string companyCode)
        {
            //此處是每次請假時先做檢查，前後台有沒有不同步的狀況，發現不同步(後台建了假單，前台狀態沒更新)，就要更新前台狀態
            var leaveFormNotApprovedAbsent = Services.GetService<LeaveFormService>().GetAll().Where(x => x.EmployeeID == employeeID && x.Status < 3 && !x.IsDeleted).ToList();
            List<AbsentFormData> _formList = new List<AbsentFormData>();
            foreach (var item in leaveFormNotApprovedAbsent)
            {
                _formList = await HRMApiAdapter.GetLeaveFormDetail(companyCode, item.FormNo);
                if (_formList.Count != 0)
                {
                    var FN = item.FormNo;
                    var AbsentItems = item;
                    AbsentItems.Status = 3;
                    Services.GetService<LeaveFormService>().Update(item, AbsentItems, true);

                    //20170704 Daniel 更新PortalFormDetail狀態為核准完成("2")
                    Task<RequestResult> apiResult = Task.Run<RequestResult>(async () => await HRMApiAdapter.UpdatePortalFormDetail(item.FormNo, "2"));
                    var taskResult = apiResult.Result;

                }
            }
        }


        /// <summary>
        /// 取得指定假別，扣除申請中尚未核准後的剩餘可用時數。
        /// </summary>
        /// <param name="CompanyCode"></param>
        /// <param name="EmployeeNO"></param>
        /// <param name="AbsentCode"></param>
        /// <returns></returns>
        public async Task<decimal> CanUseCount(string CompanyCode, string EmployeeNO, string AbsentCode)
        {
            decimal retVal = 0;
            decimal inProcess = 0;

            IQueryable<LeaveForm> q = Services.GetService<LeaveFormService>().GetAll().Where(x =>
                        x.Employee.Company.CompanyCode == CurrentUser.CompanyCode &&
                        x.Employee.EmployeeNO == CurrentUser.EmployeeNO &&
                        x.AbsentCode == AbsentCode &&
                        x.Status == 1 && x.IsDeleted == false);

            if (q.Any()) inProcess = q.Sum(s => s.LeaveAmount);
          
            double hrmAmount= await HRMApiAdapter.GetCanUseCount(CompanyCode, EmployeeNO, DateTime.Now, DateTime.Now, AbsentCode);

            retVal = Convert.ToDecimal(hrmAmount) - inProcess; //剩餘可用減去申請中的時數。
            return retVal;
        }

        /// <summary>
        /// 可選取部門的所有人員清單
        /// </summary>
        /// <returns></returns>
        private async Task<List<Employee>> GetDeptAgentAll()
        {
            List<Employee> result = new List<Employee>();
            //檢查之前有沒有抓過了
            if (TempData["EmployeeListAll"] != null)
            {
                TempData.Keep("EmployeeListAll");
                result = (List<Employee>)TempData["EmployeeListAll"];
            }
            else
            {
                //取得目前使用者可選取部門的所有人員清單
                List<string> deptAgentList = await HRMApiAdapter.GetAgentDeptList(this.CurrentUser.DepartmentCode,this.CurrentUser.EmployeeNO);
                deptAgentList = deptAgentList.Where(x => x != this.CurrentUser.EmployeeNO).ToList();
                TempData["empAgentListList"] = deptAgentList;
                //找出該清單在Portal對應的所有人員物件
                result = Services.GetService<EmployeeService>().GetSalaryEmployeeListByEmpNoList(deptAgentList);
                TempData["EmployeeListAll"] = result;
            }

            return result;

        }

        /// <summary>
        /// 發送代理人通知信件
        /// </summary>
        /// <param name="formNo"></param>
        /// <param name="agentNo"></param>
        /// <param name="cUser"></param>
        public async Task sendMailToAgent(string formNo, string agentNo, string cUser)
        {
                //特定人員，不需要代理人，直接跳過
                if (CurrentUser.EmployeeNO == "00630" || CurrentUser.EmployeeNO == "00733" || CurrentUser.EmployeeNO == "01189" || CurrentUser.EmployeeNO == "00242" || CurrentUser.EmployeeNO == "01034" || CurrentUser.EmployeeNO == "01098")
                {
                }
                else
                {
                    //抓取假單資料
                    LeaveForm _form = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
                    //抓取假別資料
                    List<AbsentDetail> data = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, CurrentUser.Employee.EmployeeNO, DateTime.Now);
                    string AbsentCodee = null;
                    string AbsentNameEN = "";
                    foreach (var i in data)
                    {
                        if (i.Code == _form.AbsentCode)
                        {
                            AbsentCodee = i.Name;//抓取假別名稱
                            AbsentNameEN = i.AbsentNameEn;
                        }
                    }

                    //抓取代理人編號
                    Employee _agent = null;
                    _agent = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, agentNo);
                    //代理人姓名
                    //var _agentName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == agentNo).Select(x => x.EmployeeName).FirstOrDefault();
                    string _agentName = _agent == null ? "" : _agent.EmployeeName;
                    string _agentNameEN = _agent == null ? "" : _agent.EmployeeEnglishName;
                    
                    //申請人姓名
                    Employee user = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == cUser).FirstOrDefault();
                    //var CUserName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == cUser).Select(x => x.EmployeeName).FirstOrDefault();
                    string CUserName = user == null ? "" : user.EmployeeName;
                    string CUserNameEN = user == null ? "" : user.EmployeeEnglishName;
                    
                    //內容資料
                    //string _body = (CurrentUser.Employee.EmployeeName + "的請假單" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "設定" + _agent.EmployeeName + "為代理人");
                    //string _body = (CurrentUser.Employee.EmployeeName + "於" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.Hour : Resource.Day) + ")" + ")" + "間因請假，已指定" + _agent.EmployeeName + "為考勤簽核代理人，特此通知。");//修改代理人通知內文 Irving 20170727

                    //20180301 Daniel 修改代理人通知內文，鼎鼎自己沒有使用簽核代理人機制
                    //20190710 Daniel 因改為中英文併列，所以資源檔的部分要直接找中英文語系的
                    string _body = (CurrentUser.Employee.EmployeeName + "於" + "(" + AbsentCodee + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.ResourceManager.GetString("Hour", CultureInfo.GetCultureInfo("zh-TW")) : Resource.ResourceManager.GetString("Day", CultureInfo.GetCultureInfo("zh-TW"))) + ")" + ")" + "間因請假，已設定" + _agent.EmployeeName + "為代理人，特此通知。");

                    //20190528 Daniel 增加英文說明
                    string _bodyEN = "<br/>　<br/>" + (CurrentUser.Employee.EmployeeEnglishName + " (" + AbsentNameEN + "(" + _form.StartTime.ToString("yyyy/MM/dd HH:mm") + "~" + _form.EndTime.ToString("yyyy/MM/dd HH:mm") + "   " + _form.LeaveAmount.ToString("0.#") + " " + (_form.AbsentUnit == "h" ? Resource.ResourceManager.GetString("Hour", CultureInfo.GetCultureInfo("en-US")) : Resource.ResourceManager.GetString("Day", CultureInfo.GetCultureInfo("en-US"))) + ")" + ")" + " is on leave, substitute would be " + _agent.EmployeeEnglishName + ".");

                    if (_agent != null)
                    {
                        List<string> _rcpt = new List<string>();
                        _rcpt.Add(_agent.Email);
                        string _subject = CUserName + "指定" + _agentName + "為代理人通知";
                        string _subjectEN = " Notice of Assigned designee (from " + CUserNameEN + ")";
                        SendMail(_rcpt.ToArray(), null, null, _subject + _subjectEN, _body + _bodyEN, true);
                    }
                }
                //end Irving
                return;
        }

    }
}
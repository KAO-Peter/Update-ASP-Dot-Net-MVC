using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.BambooHRIntegration;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Services.Models;
using HRPortal.Services.Models.BambooHR;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace HRPortal.Areas.Reports.Controllers
{
    public class SignFormsController : BaseController
    {
        //
        // GET: /Reports/SignForms/
        public async Task<ActionResult> Index(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;

            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }

            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
            Session["AbsentsDataAll"] = await HRMApiAdapter.GetAllAbsentData("");
            return View(Query().ToPagedList(currentPage, currentPageSize));
        }

        public List<HRPotralFormSignStatus> Query()
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = _queryHelper.GetToSignList(CurrentUser.CompanyCode, CurrentUser.SignDepartmentCode, CurrentUser.EmployeeNO).Where(x => x.FormStatus > 0).ToList();


                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"], (List<AbsentDetail>)Session["AbsentsDataAll"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);
                foreach (HRPotralFormSignStatus _item in _result)
                {
                    _summaryBuilder.BuildSummary(_item);
                }

                return _result.OrderByDescending(x => x.FormCreateDate).ThenBy(x => x.FormType).ToList();
            }
        }

        public ActionResult GetLeaveCancelDetail(string formNo)
        {
            LeaveCancelFormViewModel _viewmodel = new LeaveCancelFormViewModel();
            
            //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
            List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];
            
            LeaveCancel _form = this.Services.GetService<LeaveCancelService>().GetLeaveCancelByFormNo(formNo);

            if (_form.Status == (int)FormStatus.Signing)
            {
                ViewBag.CanSign = true;
            }

            Employee AgentEmployeeEnglishName = null;
            if (_form.LeaveForm.Agent != null)
            {
                var EmployeeCompanyID = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _form.LeaveForm.Agent.EmployeeNO);
                foreach (var i in EmployeeCompanyID)
                {
                    AgentEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(i.CompanyID, _form.LeaveForm.Agent.EmployeeNO);
                }
            }
            var SenderCompanyID = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _form.LeaveForm.Employee.EmployeeNO);
            Employee SenderEmployeeEnglishName = null;
            foreach (var a in SenderCompanyID)
            {
                SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(a.CompanyID, _form.LeaveForm.Employee.EmployeeNO);
            }

            AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.LeaveForm.AbsentCode).FirstOrDefault();
            //if (_form != null)
            //{
            LeaveDisplayModel _model = new LeaveDisplayModel()
            {
                AbsentNameEn = thisAbsent == null ? "" : thisAbsent.AbsentEnglishName,
                AgentEmployeeEnglishName = _form.LeaveForm.Agent != null ? AgentEmployeeEnglishName.EmployeeEnglishName : "",
                SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "",
                FormNo = _form.LeaveForm.FormNo,
                EmployeeName = _form.LeaveForm.Employee.EmployeeName,
                DepartmentName = _form.LeaveForm.Department.DepartmentName,
                CreatedTime = _form.LeaveForm.CreatedTime,
                AbsentType = thisAbsent == null ? "" : thisAbsent.AbsentName,
                StartTime = _form.LeaveForm.StartTime,
                EndTime = _form.LeaveForm.EndTime,
                Amount = _form.LeaveForm.LeaveAmount,
                Unit = _form.LeaveForm.AbsentUnit,
                LeaveReason = _form.LeaveForm.LeaveReason,
                AgentName = _form.LeaveForm.Agent != null ? _form.LeaveForm.Agent.EmployeeName : null,
                FileName = _form.LeaveForm.FileName,
                FilePath = _form.LeaveForm.FilePath,
                IsAbroad = _form.LeaveForm.IsAbroad,
                DepartmentEnglishName = _form.LeaveForm.Department.DepartmentEnglishName
            };

            _viewmodel.SourceData = _model;
            _viewmodel.FormData = _form;

            return PartialView("_LeaveCancelContent", _viewmodel);
            //}
        }

        public ActionResult GetLeaveDetail(string formNo)
        {
            //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
            List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];

            LeaveForm _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);

            if (_form.Status == (int)FormStatus.Signing)
            {
                ViewBag.CanSign = true;
            }

            //if (_form != null)
            //{
            Employee AgentEmployeeEnglishName = null;
            if (_form.Agent != null)
            {
                AgentEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(_form.CompanyID, _form.Agent.EmployeeNO);
            }

            Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(_form.CompanyID, _form.Employee.EmployeeNO);

            AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.AbsentCode).FirstOrDefault();

            LeaveDisplayModel _model = new LeaveDisplayModel()
            {
                AbsentNameEn = thisAbsent == null ? "" : thisAbsent.AbsentEnglishName,
                AgentEmployeeEnglishName = _form.Agent != null ? (AgentEmployeeEnglishName != null ? AgentEmployeeEnglishName.EmployeeEnglishName : "") : "",
                SenderEmployeeEnglishName = SenderEmployeeEnglishName != null ? SenderEmployeeEnglishName.EmployeeEnglishName : "",
                getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "",
                FormNo = _form.FormNo,
                EmployeeName = _form.Employee.EmployeeName,
                DepartmentName = _form.Department.DepartmentName,
                CreatedTime = _form.CreatedTime,
                AbsentType = thisAbsent == null ? "" : thisAbsent.AbsentName,
                StartTime = _form.StartTime,
                EndTime = _form.EndTime,
                Amount = _form.LeaveAmount,
                Unit = _form.AbsentUnit,
                LeaveReason = _form.LeaveReason,
                AgentName = _form.Agent != null ? _form.Agent.EmployeeName : null,
                FileName = _form.FileName,
                FilePath = _form.FilePath,
                IsAbroad = _form.IsAbroad,
                DepartmentEnglishName = _form.Department.DepartmentEnglishName
            };

            return PartialView("_LeaveContent", _model);
            //}
        }

        public async Task<ActionResult> GetOverTimeDetail(string formNo)
        {
            OverTimeForm _form = this.Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == formNo);

            string OverTimeTypeName = _form.OverTimeReasonCode;
            var OverTimeReasonList = await HRMApiAdapter.GetOverTimeReasons(_form.Company.CompanyCode);

            foreach (var reason in OverTimeReasonList)
            {
                if (_form.OverTimeReasonCode == reason.Code)
                {
                    OverTimeTypeName = reason.Name;
                    break;
                }
            }

            if (_form.Status == (int)FormStatus.Signing)
            {
                ViewBag.CanSign = true;
            }

            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Employee.EmployeeNO);
                OverTimeDisplayModel _model = new OverTimeDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "",
                    EmployeeName = _form.Employee.EmployeeName,
                    DepartmentName = _form.Department.DepartmentName,
                    CreatedTime = _form.CreatedTime,
                    StartTime = _form.StartTime,
                    EndTime = _form.EndTime,
                    Amount = _form.OverTimeAmount,
                    Compensation = _form.CompensationWay.ToString(),
                    HaveDining = _form.HaveDinning,
                    OverTimeTypeName = OverTimeTypeName,
                    OverTimeReason = _form.OverTimeReason,
                    DepartmentEnglishName = _form.Department.DepartmentEnglishName
                };

                return PartialView("_OverTimeContent", _model);
            //}
        }

        public ActionResult GetOverTimeCancelDetail(string formNo)
        {
            OverTimeCancelFormViewModel _viewmodel = new OverTimeCancelFormViewModel();
            OverTimeCancel _form = this.Services.GetService<OverTimeCancelService>().GetOverTimeCancelByFormNo(formNo);

            if (_form.Status == (int)FormStatus.Signing)
            {
                ViewBag.CanSign = true;
            }

            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.OverTimeForm.Employee.EmployeeNO);
                OverTimeDisplayModel _model = new OverTimeDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "",
                    EmployeeName = _form.OverTimeForm.Employee.EmployeeName,
                    DepartmentName = _form.OverTimeForm.Department.DepartmentName,
                    CreatedTime = _form.OverTimeForm.CreatedTime,
                    StartTime = _form.OverTimeForm.StartTime,
                    EndTime = _form.OverTimeForm.EndTime,
                    Amount = _form.OverTimeForm.OverTimeAmount,
                    Compensation = _form.OverTimeForm.CompensationWay.ToString(),
                    HaveDining = _form.OverTimeForm.HaveDinning,
                    OverTimeTypeName = _form.OverTimeForm.OverTimeReasonCode,
                    OverTimeReason = _form.OverTimeForm.OverTimeReason,
                    DepartmentEnglishName = _form.OverTimeForm.Department.DepartmentEnglishName
                };

                _viewmodel.FormData = _form;
                _viewmodel.SourceData = _model;

                return PartialView("_OverTimeCancelContent", _viewmodel);
            //}
        }

        public ActionResult GetPatchCardDetail(string formNo)
        {
            PatchCardForm _form = this.Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == formNo);

            if (_form.Status == (int)FormStatus.Signing)
            {
                ViewBag.CanSign = true;
            }

            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Employee.EmployeeNO);
                PatchCardDisplayModel _model = new PatchCardDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "",
                    FormNo = _form.FormNo,
                    EmployeeName = _form.Employee.EmployeeName,
                    DepartmentName = _form.Department.DepartmentName,
                    Time = _form.PatchCardTime,
                    Type = _form.Type == 1 ? "上班" : "下班",
                    ReasonType = _form.ReasonType == 0 ? "正常刷卡" : _form.ReasonType == 1 ? "忘記刷卡" : "忘記帶卡",
                    Reason = _form.Reason,
                    FileName = _form.FileName,
                    FilePath = _form.FilePath,
                    DepartmentEnglishName = _form.Department.DepartmentEnglishName
                };

                return PartialView("_PatchCardContent", _model);
            //}
        }

        public ActionResult DownloadLeaveFormFile(string formNo)
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("LeaveFormFiles"));
            LeaveForm _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
            using (FileStream _file = new FileStream(Path.Combine(_path, _form.FilePath), FileMode.Open))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream _ms = new MemoryStream())
                {
                    int _read;
                    while ((_read = _file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _ms.Write(buffer, 0, _read);
                    }
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    return File(_ms.ToArray(), _form.FileFormat, _form.FileName);
                }
            }
        }

        public ActionResult DownloadPatchCardFormFile(string formNo)
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("PatchCardFormFiles"));
            PatchCardForm _form = this.Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == formNo);
            using (FileStream _file = new FileStream(Path.Combine(_path, _form.FilePath), FileMode.Open))
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream _ms = new MemoryStream())
                {
                    int _read;
                    while ((_read = _file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _ms.Write(buffer, 0, _read);
                    }
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    return File(_ms.ToArray(), _form.FileFormat, _form.FileName);
                }
            }
        }

        public ActionResult GetSignFlow(string formNo)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralSignFlowStatus> _signFlow = _queryHelper.GetSignFlowRecord(formNo, CurrentUser.CompanyID);
                int _index = 0;
                while (_index < _signFlow.Count && _signFlow[_index].SignStatus != "W") _index++;

                return PartialView("_SignFlowView", _signFlow);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Sign(SignModel model)
        {
            AjaxResult _result;

            try
            {
                _result = (await Accept(model) as JsonResult).Data as AjaxResult;
            }
            catch (Exception ex)
            {
                _result = new AjaxResult()
                {
                    status = "failed",
                    message = ex.Message,
                };
            }

            return Json(_result);
        }

        [HttpPost]
        public async Task<ActionResult> Accept(SignModel model)
        {
            FormType _formType = (FormType)Enum.Parse(typeof(FormType), model.FormType);
            HRPortalSignList<SignFlowRec> _signList;
            LeaveForm _leaveForm = null;
            OverTimeForm _overTimeForm = null;
            PatchCardForm _patchCardForm = null;
            LeaveCancel _leaveCancel = null;
            OverTimeCancel _overTimeCancel = null;
            RequestResult _checkResult = null;
            DeleteResponse _deleteResponse = null;
            PatchCardCancel _patchCardCancel = null;
            string _successMsg = "簽核完成";

            //20160811 多加檢核參數 by Bee
            List<FormSetting> FormSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode);

            //20180316 Daniel 簽核完成，更新ePortal簽核箱
            SendFormType sendType = SendFormType.SignedEnRoute; //先設定為簽核完成要送下一關的狀態

            string errMessage = "";
            try
            {
                switch (_formType)
                {
                    case FormType.Leave:
                        _signList = new LeaveSignList();
                        _leaveForm = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                        errMessage = CheckFormStatus(_leaveForm.Status, "核准");
                        if (!string.IsNullOrWhiteSpace(errMessage))
                        {
                            throw new Exception(errMessage);
                        }

                        _leaveForm.Status = (int)FormStatus.Signing;
                        Services.GetService<LeaveFormService>().Update(_leaveForm);
                        string _agentEmployeeNo = string.Empty;
                        if (_leaveForm.Agent != null)
                        {
                            _agentEmployeeNo = _leaveForm.Agent.EmployeeNO;
                        }
                        //假單是否檢核參數
                        //string isCheckLeave = FormSettings.FirstOrDefault(x => x.FormType == "Leave" && x.SettingKey == "isCheck").SettingValue;    
                        _checkResult = await HRMApiAdapter.CheckLeave(
                            _leaveForm.Company.CompanyCode, _leaveForm.AbsentCode, _leaveForm.Employee.EmployeeNO,
                            _agentEmployeeNo, _leaveForm.StartTime, _leaveForm.EndTime);
                        break;
                    case FormType.OverTime:
                        bool enableSettingEatingTime = FormSettings.FirstOrDefault(x => x.SettingKey == "enableSettingEatingTime").SettingValue == "true" ? true : false;
                        //加班單是否檢核刷卡資料
                        bool isCheckDutyCard = FormSettings.FirstOrDefault(x => x.FormType == "OverTime" && x.SettingKey == "isCheck").SettingValue == "true" ? true : false;
                        _signList = new OverTimeSignList();
                        _overTimeForm = Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                        errMessage = CheckFormStatus(_overTimeForm.Status, "核准");
                        if (!string.IsNullOrWhiteSpace(errMessage))
                        {
                            throw new Exception(errMessage);
                        }

                        _overTimeForm.Status = (int)FormStatus.Signing;
                        Services.GetService<OverTimeFormService>().Update(_overTimeForm);
                        _checkResult = await HRMApiAdapter.CheckOverTime(
                              _overTimeForm.Company.CompanyCode, _overTimeForm.Employee.EmployeeNO,
                              _overTimeForm.StartTime, _overTimeForm.EndTime, true,
                              _overTimeForm.CompensationWay == 0, _overTimeForm.HaveDinning,
                              _overTimeForm.HaveDinning, enableSettingEatingTime, (_overTimeForm.CutTime.HasValue == true ? _overTimeForm.CutTime.Value : 0), isCheckDutyCard);
                        break;
                    case FormType.PatchCard:
                        _signList = new PatchCardSignList();
                        _patchCardForm = Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                        errMessage = CheckFormStatus(_patchCardForm.Status, "核准");
                        if (!string.IsNullOrWhiteSpace(errMessage))
                        {
                            throw new Exception(errMessage);
                        }

                        _patchCardForm.Status = (int)FormStatus.Signing;
                        Services.GetService<PatchCardFormService>().Update(_patchCardForm);
                        _checkResult = await HRMApiAdapter.CheckDuty(
                           _patchCardForm.Company.CompanyCode, _patchCardForm.Employee.EmployeeNO,
                           _patchCardForm.PatchCardTime, _patchCardForm.Type);
                        break;
                    case FormType.LeaveCancel:
                        _signList = new LeaveCancelSignList();
                        _leaveCancel = Services.GetService<LeaveCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                        errMessage = CheckFormStatus(_leaveCancel.Status, "核准");
                        if (!string.IsNullOrWhiteSpace(errMessage))
                        {
                            throw new Exception(errMessage);
                        }

                        _leaveCancel.Status = (int)FormStatus.Signing;
                        Services.GetService<LeaveCancelService>().Update(_leaveCancel);
                        _deleteResponse = await HRMApiAdapter.CheckDeleteLeave(
                           _leaveCancel.LeaveForm.Company.CompanyCode, _leaveCancel.LeaveForm.Employee.EmployeeNO,
                           _leaveCancel.LeaveForm.FormNo);
                        break;
                    case FormType.OverTimeCancel:
                        _signList = new OverTimeCancelSignList();
                        _overTimeCancel = Services.GetService<OverTimeCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                        errMessage = CheckFormStatus(_overTimeCancel.Status, "核准");
                        if (!string.IsNullOrWhiteSpace(errMessage))
                        {
                            throw new Exception(errMessage);
                        }

                        _overTimeCancel.Status = (int)FormStatus.Signing;
                        Services.GetService<OverTimeCancelService>().Update(_overTimeCancel);
                        _deleteResponse = await HRMApiAdapter.CheckDeleteOverTime(
                           _overTimeCancel.OverTimeForm.Company.CompanyCode, _overTimeCancel.OverTimeForm.Employee.EmployeeNO,
                           _overTimeCancel.OverTimeForm.FormNo);
                        break;
                    case FormType.PatchCardCancel:
                        _signList = new PatchCardCancelSignList();
                        _patchCardCancel = Services.GetService<PatchCardCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                        errMessage = CheckFormStatus(_patchCardCancel.Status, "核准");
                        if (!string.IsNullOrWhiteSpace(errMessage))
                        {
                            throw new Exception(errMessage);
                        }

                        _patchCardCancel.Status = (int)FormStatus.Signing;
                        Services.GetService<PatchCardCancelService>().Update(_patchCardCancel);
                        _deleteResponse = await HRMApiAdapter.DeletePatchCard(
                           _patchCardCancel.PatchCardForm.Company.CompanyCode, _patchCardCancel.PatchCardForm.Employee.EmployeeNO,
                           _patchCardCancel.PatchCardForm.FormNo);
                        break;
                    default:
                        throw new Exception();
                }

                if (_deleteResponse != null)
                {
                    _checkResult = new RequestResult()
                    {
                        Status = _deleteResponse.Status && !_deleteResponse.isLocked,
                        StatusCode = _deleteResponse.StatusCode,
                        Message = _deleteResponse.Message,
                    };
                }

                if (_checkResult.Status && _signList.IsApprovingLevel(model.FormNo, model.SignFlowID))
                {
                    sendType = SendFormType.Approved; //如果是核決關卡，就改為已核準狀態

                    switch (_formType)
                    {
                        case FormType.Leave:
                            string _agentEmployeeNo = string.Empty;
                            if (_leaveForm.Agent != null)
                            {
                                _agentEmployeeNo = _leaveForm.Agent.EmployeeNO;
                            }
                            _checkResult = await HRMApiAdapter.PostLeave(_leaveForm.FormNo, _leaveForm.Company.CompanyCode, _leaveForm.Employee.EmployeeNO,
                                _leaveForm.StartTime, _leaveForm.EndTime, _leaveForm.AbsentCode, _leaveForm.LeaveReason, _agentEmployeeNo);
                            break;
                        case FormType.OverTime:
                            //加班單是否手動填寫用餐時間參數
                            bool enableSettingEatingTime = FormSettings.FirstOrDefault(x => x.FormType == "OverTime" && x.SettingKey == "enableSettingEatingTime").SettingValue == "true" ? true : false;
                            //加班單是否檢核刷卡資料
                            bool isCheckDutyCard = FormSettings.FirstOrDefault(x => x.FormType == "OverTime" && x.SettingKey == "isCheck").SettingValue == "true" ? true : false;
                            _checkResult = await HRMApiAdapter.PostOverTime(_overTimeForm.FormNo, _overTimeForm.Company.CompanyCode, _overTimeForm.Employee.EmployeeNO,
                                _overTimeForm.StartTime, _overTimeForm.EndTime, true, _overTimeForm.CompensationWay == 0, _overTimeForm.HaveDinning, _overTimeForm.HaveDinning, _overTimeForm.OverTimeReason, _overTimeForm.OverTimeReasonCode, enableSettingEatingTime, _overTimeForm.CutTime.HasValue == true ? _overTimeForm.CutTime.Value : 0, isCheckDutyCard);
                            break;
                        case FormType.PatchCard:
                            _checkResult = await HRMApiAdapter.PostDutyAddFormNo(_patchCardForm.FormNo, _patchCardForm.Company.CompanyCode, _patchCardForm.Employee.EmployeeNO, _patchCardForm.PatchCardTime,
                                _patchCardForm.Type, _patchCardForm.ReasonType, _patchCardForm.Reason);
                            break;
                        case FormType.LeaveCancel:
                            //20170809 Daniel 原來底下這段寫到刪除補刷卡去了，應該是刪除假單
                            /*
                            _deleteResponse = await HRMApiAdapter.DeletePatchCard(
                               _leaveCancel.LeaveForm.Company.CompanyCode, _leaveCancel.LeaveForm.Employee.EmployeeNO,
                               _leaveCancel.LeaveForm.FormNo);
                            */
                            _deleteResponse = await HRMApiAdapter.DeleteLeave(
                               _leaveCancel.LeaveForm.Company.CompanyCode, _leaveCancel.LeaveForm.Employee.EmployeeNO,
                               _leaveCancel.LeaveForm.FormNo);

                            break;
                        case FormType.OverTimeCancel:
                            _deleteResponse = await HRMApiAdapter.DeleteOverTime(
                               _overTimeCancel.OverTimeForm.Company.CompanyCode, _overTimeCancel.OverTimeForm.Employee.EmployeeNO,
                               _overTimeCancel.OverTimeForm.FormNo);
                            break;
                        case FormType.PatchCardCancel:
                            _deleteResponse = await HRMApiAdapter.DeletePatchCard(
                               _patchCardCancel.PatchCardForm.Company.CompanyCode, _patchCardCancel.PatchCardForm.Employee.EmployeeNO,
                               _patchCardCancel.PatchCardForm.FormNo);
                            break;
                        default:
                            throw new Exception();
                    }

                    if (_deleteResponse != null)
                    {
                        _checkResult = new RequestResult()
                        {
                            Status = _deleteResponse.Status && !_deleteResponse.isLocked,
                            StatusCode = _deleteResponse.StatusCode,
                            Message = _deleteResponse.Message,
                        };
                    }
                }

                List<AbsentType> listAbsent = (List<AbsentType>)Session["Absents"];
                //SignMailHelper _mailHelper = new SignMailHelper((Dictionary<string, string>)Session["Absents"]);
                SignMailHelper _mailHelper = new SignMailHelper(listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentName), listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentEnglishName));

                _signList.OnFlowAccepted += _mailHelper.SendMailOnFlowAccepted;
                _signList.OnFormApproved += _mailHelper.SendMailOnFormApproved;

                if (_checkResult.Status && _signList.Accept(model.FormNo, model.SignFlowID, CurrentUser.EmployeeNO, model.Instruction))
                {
                    _successMsg = HRPortal.MultiLanguage.Resource.Text_Approved;
                    switch (_formType)
                    {
                        case FormType.Leave:
                            _leaveForm.Status = (int)FormStatus.Send;
                            Services.GetService<LeaveFormService>().Update(_leaveForm);
                            break;
                        case FormType.OverTime:
                            _overTimeForm.Status = (int)FormStatus.Send;
                            Services.GetService<OverTimeFormService>().Update(_overTimeForm);
                            break;
                        case FormType.PatchCard:
                            _patchCardForm.Status = (int)FormStatus.Send;
                            Services.GetService<PatchCardFormService>().Update(_patchCardForm);
                            break;
                        case FormType.LeaveCancel:
                            _leaveCancel.Status = (int)FormStatus.Send;
                            Services.GetService<LeaveCancelService>().Update(_leaveCancel);
                            break;
                        case FormType.OverTimeCancel:
                            _overTimeCancel.Status = (int)FormStatus.Send;
                            Services.GetService<OverTimeCancelService>().Update(_overTimeCancel);
                            break;
                        case FormType.PatchCardCancel:
                            _patchCardCancel.Status = (int)FormStatus.Send;
                            Services.GetService<PatchCardCancelService>().Update(_patchCardCancel);
                            break;
                        default:
                            throw new Exception();
                    }
                }

                //20180316 Start Daniel 簽核完成，更新ePortal簽核箱
                SignFlowRecRepository signFlowRecRepository = new SignFlowRecRepository();
                SignFlowToEPortalDetailModel ePortalData = new SignFlowToEPortalDetailModel();

                if (sendType == SendFormType.Approved) //核准時，需刪除簽核箱內容
                {

                    ePortalData.docno = model.FormNo; //表單編號
                    ePortalData.memo = sendType.ToString();

                }
                else //簽核完成要送下一關時，要找出下一關資訊
                {
                    //找出目前表單編號對應的流程中，現在需要簽核的人為何
                    //FormNo在Portal不會重覆
                    var signFlowList = signFlowRecRepository.GetAll().Where(x => x.FormNumber == model.FormNo).ToList();
                    var signerFlow = signFlowList.Where(x => x.SignStatus == "W").OrderBy(y => y.SignOrder).FirstOrDefault(); //基本上一定有資料
                    //還需要找出此單最早是誰申請的
                    var firstFlow = signFlowList.OrderBy(x => x.SignOrder).FirstOrDefault(); //基本上一定有資料
                    DateTime applyDate = firstFlow.CDate;
                    string applyEmpID = firstFlow.CUser;
                    //找出申請人姓名
                    var employeeService = Services.GetService<EmployeeService>();
                    var employee = employeeService.GetAll().Where(x => x.EmployeeNO == applyEmpID).FirstOrDefault(); //目前先不比對公司別
                    string applyEmpName = (employee != null) ? employee.EmployeeName : "";


                    if (signerFlow != null)
                    {
                        string signerID = signerFlow.SignerID;
                        string formDesc = "";
                        switch (_formType)
                        {
                            case FormType.Leave:
                                formDesc = "請假單";
                                break;
                            case FormType.OverTime:
                                formDesc = "加班單";
                                break;
                            case FormType.PatchCard:
                                formDesc = "補刷卡"; //目前應該沒有補刷卡
                                break;
                            case FormType.LeaveCancel:
                                formDesc = "請假單銷單";
                                break;
                            case FormType.OverTimeCancel:
                                formDesc = "加班單銷單";
                                break;
                            case FormType.PatchCardCancel:
                                formDesc = "補刷卡銷單"; //目前應該沒有補刷卡銷單
                                break;
                        }
                        ePortalData.empno = signerID; //待簽核主管工號
                        ePortalData.docno = model.FormNo; //表單編號
                        ePortalData.formdesc = formDesc + "申請";
                        ePortalData.sendtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //送簽時間
                        ePortalData.sendname = applyEmpName; //送簽(申請)人員
                        ePortalData.appdate = applyDate.ToString("yyyy-MM-dd"); //申請日期
                        ePortalData.memo = sendType.ToString();
                    }



                }
                var ePortalService = Services.GetService<SignFlowToEPortalService>();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                string result = ePortalService.UpdateEPortal(sendType, ePortalData, this.CurrentUser.EmployeeID, this.Request.UserHostAddress, controllerName, actionName);
                //result先不判斷狀況，更新ePortal失敗不應該擋住Portal流程
                //20180316 End

                //20190507 Daniel 假單核准時，要更新SmartSheet狀態欄位，銷假單核准時要刪除SmartSheet紀錄
                //20201119 Daniel 假單核准時，要更新BambooHR該張Time Off狀態為approved，銷假單核准時，要看該張假單時間，如果請假開始時間在當天(含)之後的，變更狀態為Canceled，如果開始時間在當天之前，要送Balance還時數
                //檢查是否為核准狀態
                if (sendType == SendFormType.Approved)
                {
                    if (_formType == FormType.Leave) //請假單
                    {
                        //取得請假單資訊
                        LeaveForm leaveForm = Services.GetService<LeaveFormService>().GetLeaveFormByFormNoWithEmployee(model.FormNo);
                        if (leaveForm != null)
                        {
                            SystemSettingService systemSettingService = Services.GetService<SystemSettingService>();

                            //SmartSheet檢查部門
                            string SmartSheetDepartments = systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                            if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                            {
                                List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                                if (ssDeptList.Contains(leaveForm.Department.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                                {
                                    var smartSheetService = new SmartSheetrIntegrationService();
                                    string retValue = smartSheetService.Update(model.FormNo, leaveForm.ID, 3); //核准狀態為3
                                    WriteLog("HR代簽核假單更新SmartSheet紀錄：" + retValue);

                                }
                            }

                            //BambooHR先檢查部門與假別
                            string BambooHRDepartments = systemSettingService.GetSettingValue("BambooHR_DepartmentList");
                            string BambooHRAbsentCodes = systemSettingService.GetSettingValue("BambooHR_AbsentCodeList");
                            if (!string.IsNullOrWhiteSpace(BambooHRDepartments) && !string.IsNullOrWhiteSpace(BambooHRAbsentCodes))
                            {
                                List<string> bbDeptList = BambooHRDepartments.Split(';').ToList();
                                List<string> bbAbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();
                                if (bbDeptList.Contains(leaveForm.Department.DepartmentCode) && bbAbsentCodeList.Contains(leaveForm.AbsentCode)) //符合設定的部門與假別，要傳資料到BambooHR
                                {
                                    var bbService = new BambooHRIntegrationService(this.LogInfo);
                                    string note = string.IsNullOrWhiteSpace(model.Instruction) ? sendType.ToString() : model.Instruction;
                                    string retValue = bbService.ChangeLeaveFormStatus(leaveForm, TimeOffRequestStatus.approved, note);
                                    WriteLog("HR代簽核假單更新BambooHR Time Off狀態(第1次)：" + retValue);
                                    
                                    //20210305 Daniel 因BambooHR流程如果是兩關，API_Owner核准需發送兩次更新狀態交易才能真的完成
                                    retValue = bbService.ChangeLeaveFormStatus(leaveForm, TimeOffRequestStatus.approved, note, TimeOffRequestStatus.requested, "HRPortal", true);
                                    WriteLog("HR代簽核假單更新BambooHR Time Off狀態(第2次)：" + retValue);
                                    
                                }
                            }
                        }
                    }
                    else if (_formType == FormType.LeaveCancel) //銷假單
                    {
                        //取得銷假單資訊
                        LeaveCancel leaveCancel = Services.GetService<LeaveCancelService>().GetLeaveCancelByFormNo(model.FormNo);
                        if (leaveCancel != null)
                        {
                            //取得請假單資訊
                            Guid leaveFormID = leaveCancel.LeaveFormID;
                            LeaveForm leaveForm = Services.GetService<LeaveFormService>().GetLeaveFormByID(leaveFormID);
                            if (leaveForm != null)
                            {
                                SystemSettingService systemSettingService = Services.GetService<SystemSettingService>();

                                //檢查部門
                                string SmartSheetDepartments = systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                                if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                                {
                                    List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                                    if (ssDeptList.Contains(leaveForm.Department.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                                    {
                                        var smartSheetService = new SmartSheetrIntegrationService();
                                        string retValue = smartSheetService.Delete(leaveForm.FormNo, leaveForm.ID);
                                        WriteLog("HR代簽核銷假刪除SmartSheet紀錄：" + retValue);
                                    }
                                }

                                //BambooHR先檢查部門與假別
                                string BambooHRDepartments = systemSettingService.GetSettingValue("BambooHR_DepartmentList");
                                string BambooHRAbsentCodes = systemSettingService.GetSettingValue("BambooHR_AbsentCodeList");
                                if (!string.IsNullOrWhiteSpace(BambooHRDepartments) && !string.IsNullOrWhiteSpace(BambooHRAbsentCodes))
                                {
                                    List<string> bbDeptList = BambooHRDepartments.Split(';').ToList();
                                    List<string> bbAbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();
                                    if (bbDeptList.Contains(leaveForm.Department.DepartmentCode) && bbAbsentCodeList.Contains(leaveForm.AbsentCode)) //符合設定的部門與假別，要傳資料到BambooHR
                                    {
                                        var bbService = new BambooHRIntegrationService(this.LogInfo);
                                        //string note = string.IsNullOrWhiteSpace(model.Instruction) ? sendType.ToString() : model.Instruction;
                                        string retValue = bbService.CancelLeaveForm(leaveForm);
                                        WriteLog("銷假單核准回補時數或更新BambooHR Time Off狀態：" + retValue);
                                    }
                                }
                            }
                        }

                    }
                }

                var _ajaxResult = new AjaxResult()
                {
                    status = _checkResult.Status.ToString(),
                    message = _checkResult.Message
                };

                if (_checkResult.Status)
                    _ajaxResult.message = _successMsg;

                return Json(_ajaxResult);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult()
                {
                    status = "failed",
                    message = ex.Message,
                });
            }
        }

        //20170509 Start 增加 by Daniel
        //簽核狀況查詢-明細頁面增加退簽功能
        [HttpPost]
        public ActionResult Reject(SignModel model, bool isPullBack = false)
        {
            try
            {
                FormType _formType = (FormType)Enum.Parse(typeof(FormType), model.FormType);
                HRPortalSignList<SignFlowRec> _signList;
                LeaveForm _leaveForm = null;
                OverTimeForm _overTimeForm = null;
                PatchCardForm _patchCardForm = null;
                LeaveCancel _leaveCancel = null;
                OverTimeCancel _overTimeCancel = null;
                PatchCardCancel _patchCardCancel = null;
                switch (_formType)
                {
                    case FormType.Leave:
                        _signList = new LeaveSignList();
                        break;
                    case FormType.OverTime:
                        _signList = new OverTimeSignList();
                        break;
                    case FormType.PatchCard:
                        _signList = new PatchCardSignList();
                        break;
                    case FormType.LeaveCancel:
                        _signList = new LeaveCancelSignList();
                        break;
                    case FormType.OverTimeCancel:
                        _signList = new OverTimeCancelSignList();
                        break;
                    case FormType.PatchCardCancel:
                        _signList = new PatchCardCancelSignList();
                        break;
                    default:
                        throw new Exception();
                        break;
                }

                string _resendUrl = null;

                List<AbsentType> listAbsent = (List<AbsentType>)Session["Absents"];
                //SignMailHelper _mailHelper = new SignMailHelper((Dictionary<string, string>)Session["Absents"]);
                SignMailHelper _mailHelper = new SignMailHelper(listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentName), listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentEnglishName));

                _signList.OnFlowRejected += _mailHelper.SendMailOnFlowRejected;
                _signList.OnFormReturned += _mailHelper.SendMailOnFormReturned;

                if (_signList.Reject(model.FormNo, model.SignFlowID, CurrentUser.EmployeeNO, model.Instruction, isPullBack))
                {

                    //20180316 Start Daniel 拉回或退回送出，更新ePortal簽核箱
                    SendFormType sendType = (isPullBack) ? SendFormType.Retracted : SendFormType.Rejected; //拉回或退回
                    SignFlowToEPortalDetailModel ePortalData = new SignFlowToEPortalDetailModel()
                    {
                        docno = model.FormNo, //表單編號
                        memo = sendType.ToString(),
                    };

                    var ePortalService = Services.GetService<SignFlowToEPortalService>();
                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    string result = ePortalService.UpdateEPortal(sendType, ePortalData, this.CurrentUser.EmployeeID, this.Request.UserHostAddress, controllerName, actionName);
                    //result先不判斷狀況，更新ePortal失敗不應該擋住Portal流程
                    //20180316 End

                    //20190507 Daniel 拉回或退回，更新SmartSheet
                    //20201119 Dnaiel 拉回或退回，更新BambooHR Time Off狀態
                    //檢查是否為請假單，目前只針對請假單
                    if (_formType == FormType.Leave)
                    {
                        //取得請假單資訊
                        LeaveForm leaveForm = Services.GetService<LeaveFormService>().GetLeaveFormByFormNoWithEmployee(model.FormNo);
                        if (leaveForm != null)
                        {
                            SystemSettingService systemSettingService = Services.GetService<SystemSettingService>();

                            //SmartSheet檢查部門
                            string SmartSheetDepartments = systemSettingService.GetSettingValue("SmartSheet_DepartmentList");
                            if (!string.IsNullOrWhiteSpace(SmartSheetDepartments))
                            {
                                List<string> ssDeptList = SmartSheetDepartments.Split(';').ToList();
                                if (ssDeptList.Contains(leaveForm.Department.DepartmentCode)) //符合設定的部門，要傳資料到SmartSheet
                                {
                                    var smartSheetService = new SmartSheetrIntegrationService();
                                    string retValue = smartSheetService.Delete(model.FormNo, leaveForm.ID);
                                    WriteLog("HR代退假單刪除SmartSheet紀錄：" + retValue);
                                }
                            }

                            //BambooHR先檢查部門與假別
                            string BambooHRDepartments = systemSettingService.GetSettingValue("BambooHR_DepartmentList");
                            string BambooHRAbsentCodes = systemSettingService.GetSettingValue("BambooHR_AbsentCodeList");
                            if (!string.IsNullOrWhiteSpace(BambooHRDepartments) && !string.IsNullOrWhiteSpace(BambooHRAbsentCodes))
                            {
                                List<string> bbDeptList = BambooHRDepartments.Split(';').ToList();
                                List<string> bbAbsentCodeList = BambooHRAbsentCodes.Split(';').ToList();
                                if (bbDeptList.Contains(leaveForm.Department.DepartmentCode) && bbAbsentCodeList.Contains(leaveForm.AbsentCode)) //符合設定的部門與假別，要傳資料到BambooHR
                                {
                                    var bbService = new BambooHRIntegrationService(this.LogInfo);
                                    string note = string.IsNullOrWhiteSpace(model.Instruction) ? sendType.ToString() : model.Instruction;
                                    string retValue = bbService.ChangeLeaveFormStatus(leaveForm, TimeOffRequestStatus.denied, note);
                                    WriteLog("HR代退假單更新BambooHR Time Off狀態：" + retValue);
                                }
                            }
                        }
                    }

                    string errMessage = "";
                    switch (_formType)
                    {
                        case FormType.Leave:
                            _leaveForm = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                            //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                            errMessage = CheckFormStatus(_leaveForm.Status, "退回");
                            if (!string.IsNullOrWhiteSpace(errMessage))
                            {
                                throw new Exception(errMessage);
                            }

                            _leaveForm.Status = (int)FormStatus.Draft;
                            Services.GetService<LeaveFormService>().Update(_leaveForm);

                            //20170703 Daniel 刪除PortalFormDetail明細
                            Task<RequestResult> apiResult = Task.Run<RequestResult>(async () => await HRMApiAdapter.DeletePortalFormDetail(model.FormNo));
                            var taskResult = apiResult.Result;

                            _resendUrl = Url.Action("Index", "../M02/LeaveForm") + "?formNo=" + _leaveForm.FormNo;
                            break;
                        case FormType.OverTime:
                            _overTimeForm = Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                            //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                            errMessage = CheckFormStatus(_overTimeForm.Status, "退回");
                            if (!string.IsNullOrWhiteSpace(errMessage))
                            {
                                throw new Exception(errMessage);
                            }

                            _overTimeForm.Status = (int)FormStatus.Draft;
                            Services.GetService<OverTimeFormService>().Update(_overTimeForm);
                            _resendUrl = Url.Action("Index", "../M02/OverTimeForm") + "?formNo=" + _overTimeForm.FormNo;
                            break;
                        case FormType.PatchCard:
                            _patchCardForm = Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                            //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                            errMessage = CheckFormStatus(_patchCardForm.Status, "退回");
                            if (!string.IsNullOrWhiteSpace(errMessage))
                            {
                                throw new Exception(errMessage);
                            }

                            _patchCardForm.Status = (int)FormStatus.Draft;
                            Services.GetService<PatchCardFormService>().Update(_patchCardForm);
                            _resendUrl = Url.Action("Index", "../M02/PatchCardForm") + "?formNo=" + _patchCardForm.FormNo;
                            break;
                        case FormType.LeaveCancel:
                            _leaveCancel = Services.GetService<LeaveCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                            //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                            errMessage = CheckFormStatus(_leaveCancel.Status, "退回");
                            if (!string.IsNullOrWhiteSpace(errMessage))
                            {
                                throw new Exception(errMessage);
                            }

                            _leaveCancel.Status = (int)FormStatus.Draft;
                            Services.GetService<LeaveCancelService>().Update(_leaveCancel);
                            _resendUrl = Url.Action("Index", "../M02/LeaveCancelForm") + "?formNo=" + _leaveCancel.FormNo;
                            break;
                        case FormType.OverTimeCancel:
                            _overTimeCancel = Services.GetService<OverTimeCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                            //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                            errMessage = CheckFormStatus(_overTimeCancel.Status, "退回");
                            if (!string.IsNullOrWhiteSpace(errMessage))
                            {
                                throw new Exception(errMessage);
                            }

                            _overTimeCancel.Status = (int)FormStatus.Draft;
                            Services.GetService<OverTimeCancelService>().Update(_overTimeCancel);
                            _resendUrl = Url.Action("Index", "../M02/OverTimeCancelForm") + "?formNo=" + _overTimeCancel.FormNo;
                            break;
                        case FormType.PatchCardCancel:
                            _patchCardCancel = Services.GetService<PatchCardCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);

                            //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
                            errMessage = CheckFormStatus(_patchCardCancel.Status, "退回");
                            if (!string.IsNullOrWhiteSpace(errMessage))
                            {
                                throw new Exception(errMessage);
                            }

                            _patchCardCancel.Status = (int)FormStatus.Draft;
                            Services.GetService<PatchCardCancelService>().Update(_patchCardCancel);
                            _resendUrl = Url.Action("Index", "../M02/PatchCardCancelForm") + "?formNo=" + _patchCardCancel.FormNo;
                            break;
                        default:
                            throw new Exception();
                    }

                }

                var _ajaxResult = new AjaxResult()
                {
                    status = true.ToString(),
                    message = isPullBack ? _resendUrl : HRPortal.MultiLanguage.Resource.Text_ReturnedToSender,
                };

                return Json(_ajaxResult);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult()
                {
                    status = "failed",
                    message = ex.Message,
                });
            }
        }
        //20170509 End

        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
        private string CheckFormStatus(int Status, string SignActionType)
        {
            string errMessage = "";
            switch (Status)
            {
                case (int)FormStatus.Draft:
                    errMessage = "該申請單之前已經退回或自行撤回，無法再" + SignActionType;
                    break;
                case (int)FormStatus.Send:
                    errMessage = "該申請單之前已經核決，無法再" + SignActionType;
                    break;
            }

            return errMessage;

        }
    }
}
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;
using YoungCloud.SignFlow.Model;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class SignFormsController : BaseController
    {
        private bool isHR = false, isAdmin = false;
        //20181127 Warm up 使用
        /*
        [AllowAnonymous]
        public ActionResult WarmUp()
        {
            return View("Index");
        }
        */

        // GET: /ToDo/SignForms/
        public async Task<ActionResult> Index(int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }
            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
            Session["AbsentsDataAll"] = await HRMApiAdapter.GetAllAbsentData("");
            return View(Query()); //return View(Query().ToPagedList(currentPage, currentPageSize));
        }

        public List<HRPotralFormSignStatus> Query()
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = _queryHelper.GetToSignList(CurrentUser.CompanyCode, CurrentUser.SignDepartmentCode, CurrentUser.EmployeeNO).Where(x => x.FormStatus > 0).ToList();

                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    //抓取申請人英文名字
                    Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _item.SenderEmployeeNo);
                    _item.SenderEmployeeEnglishName = SenderEmployeeEnglishName != null ? SenderEmployeeEnglishName.EmployeeEnglishName : "";
                    //抓取簽核者人英文名字
                    Employee SignerEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _item.SignerEmployeeNo);
                    if (SignerEmployeeEnglishName != null) // 如果是 Null代表有可能是給HR部門簽核
                    {
                        _item.SignerEmployeeEnglishName = SignerEmployeeEnglishName.EmployeeEnglishName;
                    }
                    //抓取語系
                    _item.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW";
                   
                    //20180323 Daniel 修改簽核欄位內容與樣式
                    // _summaryBuilder.BuildSummary(_item);
                    FormSummaryDetailData result = _summaryBuilder.BuildSummaryForSign(_item);
                    _item.FormSummary = result.Description + "|" + result.Hours + "|" + result.Period;

                    //增加附件檔案連結 Irving 20161209
                    //LeaveForm _form = new LeaveForm();
                    if (_item.FormNo.Substring(0, 1) == "P")
                    {
                        switch (_item.FormType)
                        {
                            case FormType.Leave:
                                LeaveForm _form_LeaveForm = new LeaveForm();
                                _form_LeaveForm = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                                _item.BeDate = _form_LeaveForm.StartTime;
                                if (_form_LeaveForm != null)
                                {
                                    _item.FilePath = _form_LeaveForm.FilePath;
                                }
                                else
                                {
                                    _item.FilePath = null;
                                }
                                break;

                            case FormType.OverTime:
                                OverTimeForm _form_OverTimeForm = new OverTimeForm();
                                _form_OverTimeForm = this.Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                                _item.BeDate = _form_OverTimeForm.StartTime;
                                if (_form_OverTimeForm != null)
                                {
                                    _item.FilePath = _form_OverTimeForm.FilePath;
                                }
                                else
                                {
                                    _item.FilePath = null;
                                }
                                break;

                            case FormType.PatchCard:
                                PatchCardForm _form_PatchCardForm = new PatchCardForm();
                                _form_PatchCardForm = this.Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                                _item.BeDate = _form_PatchCardForm.PatchCardTime;
                                break;

                            case FormType.LeaveCancel:
                                LeaveCancel _form_LeaveCancel = new LeaveCancel();
                                _form_LeaveCancel = this.Services.GetService<LeaveCancelService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                                _item.BeDate = _form_LeaveCancel.LeaveForm.StartTime;
                                break;

                            case FormType.OverTimeCancel:
                                OverTimeCancel _form_OverTimeCancel = new OverTimeCancel();
                                _form_OverTimeCancel = this.Services.GetService<OverTimeCancelService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                                _item.BeDate = _form_OverTimeCancel.OverTimeForm.StartTime;
                                break;

                            case FormType.PatchCardCancel:
                                PatchCardCancel _form_PatchCardCancel = new PatchCardCancel();
                                _form_PatchCardCancel = this.Services.GetService<PatchCardCancelService>().FirstOrDefault(x => x.FormNo == _item.FormNo);
                                _item.BeDate = _form_PatchCardCancel.PatchCardForm.PatchCardTime;
                                break;

                            default:
                                break;
                        }
                    }
                    //End
                }

                return _result.OrderBy(x => x.BeDate).ThenBy(x => x.FormType).ToList();
            }
        }

        public async Task<ActionResult> GetLeaveCancelDetail(string formNo)
        {
            LeaveCancelFormViewModel _viewmodel = new LeaveCancelFormViewModel();

            //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
            List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];

            LeaveCancel _form = this.Services.GetService<LeaveCancelService>().GetLeaveCancelByFormNo(formNo);
            if (_form.Status == (int)FormStatus.Draft && _form.LeaveForm.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/LeaveCancelForm") + "?formNo=" + _form.FormNo;
            }
            //if (_form != null)
            {
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
                List<AbsentDetail> data = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
                AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.LeaveForm.AbsentCode).FirstOrDefault();

                foreach (var i in data)
                {
                    if (i.Code == _form.LeaveForm.AbsentCode)
                    {
                        LeaveDisplayModel _model = new LeaveDisplayModel()
                        {
                            AbsentNameEn = i.AbsentNameEn != null ? i.AbsentNameEn : (thisAbsent == null ? "" : thisAbsent.AbsentName),
                            AgentEmployeeEnglishName = _form.LeaveForm.Agent != null ? AgentEmployeeEnglishName.EmployeeEnglishName : "",
                            SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                            getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                            FormNo = _form.LeaveForm.FormNo,
                            EmployeeName = _form.LeaveForm.Employee.EmployeeName,
                            DepartmentName = _form.LeaveForm.Department.DepartmentName,
                            CreatedTime = _form.LeaveForm.CreatedTime,
                            AbsentType = (thisAbsent == null ? "" : thisAbsent.AbsentName),
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
                    }
                }
                return PartialView("_LeaveCancelContent", _viewmodel);
            }
        }

        //增加判斷簽核過程中要能提示該簽核者此假單代理人是否也請假 Irving 2016/03/24
        [HttpPost]
        public async Task<ActionResult> CheckAgentLeave(SignModel model)
        {
            OverTimeForm _overTimeForm = null;
            LeaveForm _leaveForm = null;
            PatchCardForm _patchCardForm = null;
            LeaveCancel _leaveCancel = null;
            OverTimeCancel _overTimeCancel = null;
            //獲取假單資料
            _patchCardForm = Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);
            _leaveCancel = Services.GetService<LeaveCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);
            _overTimeCancel = Services.GetService<OverTimeCancelService>().FirstOrDefault(x => x.FormNo == model.FormNo);
            _leaveForm = Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);
            _overTimeForm = Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);
            if ((_overTimeForm != null) || (_patchCardForm != null) || (_leaveCancel != null) || (_overTimeCancel != null))
            {
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }
            //獲取代理人資料
            if (_leaveForm != null)
            {
                if (_leaveForm.Agent != null)
                {
                    Employee _agent = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(_leaveForm.CompanyID, _leaveForm.Agent.EmployeeNO);
                    if (_agent != null)
                    {
                        if (Services.GetService<LeaveFormService>().CheckLeaveFormExist(_leaveForm.StartTime, _leaveForm.EndTime, _agent.ID))
                        {
                            return Json(new AjaxResult() { status = "failed", message = "代理人在此區間已請假" });
                        }
                        if (await HRMApiAdapter.CheckIfEmployeeLeave(_agent.Company.CompanyCode,
                           _agent.Department.DepartmentCode, _agent.EmployeeNO, _leaveForm.StartTime, _leaveForm.EndTime))
                        {
                            return Json(new AjaxResult() { status = "failed", message = "代理人在此區間已休假" });
                        }
                    }
                }
            }
            return Json(new AjaxResult() { status = "success", message = string.Empty });
        }

        //End Irving

        public async Task<ActionResult> GetLeaveDetail(string formNo, int formNoOrder = 0)
        {
            base.SetBaseUserInfo();
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                LeaveCancelFormViewModel _viewmodel = new LeaveCancelFormViewModel();

                //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
                List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];

                LeaveForm _form = new LeaveForm();

                // Irving 讓代理人簽核時能按鈕顯示為代理人簽核
                List<HRPotralSignFlowStatus> _signFlow = _queryHelper.GetSignFlowRecord(formNo, CurrentUser.CompanyID);
                string SignerEmployeeNo = string.Empty;
                string Status = string.Empty;
                foreach (var i in _signFlow)
                {
                    if (i.SignerEmployeeNo2 == SignerEmployeeNo && Status == "A")
                    {
                        ViewBag.AgentSame = "Y";
                    }
                    else
                    {
                        Status = i.SignStatus;
                        SignerEmployeeNo = i.SignerEmployeeNo2;
                        ViewBag.AgentSame = "N";
                    }
                }

                //End
                if (formNo.Substring(0, 1) == "P")
                {
                    _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
                }
                else
                {
                    //_form = await GetHRMLeaveDetail(CurrentUser.CompanyCode, formNo);
                    return await GetHRMLeaveDetail(CurrentUser.CompanyCode, formNo, formNoOrder);
                }
                if (_form.Status == (int)FormStatus.Draft && _form.EmployeeID == CurrentUser.EmployeeID)
                {
                    ViewBag.CanResend = true;
                    ViewBag.ResendUrl = Url.Action("Index", "../M02/LeaveForm") + "?formNo=" + _form.FormNo;
                }
                //銷假按鈕顯示
                if (_form.Status == (int)FormStatus.Send && _form.EmployeeID == CurrentUser.EmployeeID && !Services.GetService<LeaveCancelService>().Any(x => x.LeaveFormID == _form.ID && !x.IsDeleted))
                {
                    ViewBag.CanCancel = true;
                    ViewBag.CancelURL = Url.Action("Index", "../M02/LeaveCancelForm") + "?LeaveFormID=" + _form.FormNo;
                }
                //if (_form.Status == (int)FormStatus.Send && _form.EmployeeID == CurrentUser.EmployeeID && !this.Services.GetService<LeaveCancelService>().CheckCompleteByLeaveFormNo(formNo, (int)FormStatus.Send))
                //{
                //    ViewBag.FormStatus = _form.Status;
                //    ViewBag.CancelURL = Url.Action("Index", "../M02/LeaveCancelForm") + "?LeaveFormID=" + _form.FormNo;
                //}
                //if (_form != null)
                //{

                AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.AbsentCode).FirstOrDefault();

                Employee AgentEmployeeEnglishName = null;
                if (_form.Agent != null)
                {
                    AgentEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(_form.CompanyID, _form.Agent.EmployeeNO);
                }

                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(_form.CompanyID, _form.Employee.EmployeeNO);
                List<AbsentDetail> data = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

                LeaveFormData _formData = new LeaveFormData(_form);
                bool IsViewFile = getViewFilePermission(_formData);

                foreach (var i in data)
                {
                    if (i.Code == _form.AbsentCode)
                    {
                        LeaveDisplayModel _model = new LeaveDisplayModel()
                        {
                            AbsentNameEn = i.AbsentNameEn != null ? i.AbsentNameEn : (thisAbsent == null ? "" : thisAbsent.AbsentName),
                            AgentEmployeeEnglishName = _form.Agent != null ? (AgentEmployeeEnglishName != null ? AgentEmployeeEnglishName.EmployeeEnglishName : "") : "",
                            SenderEmployeeEnglishName = SenderEmployeeEnglishName != null ? SenderEmployeeEnglishName.EmployeeEnglishName : "",
                            getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                            FormNo = _form.FormNo,
                            EmployeeName = _form.Employee.EmployeeName,
                            DepartmentName = _form.Department.DepartmentName,
                            CreatedTime = _form.CreatedTime,
                            AbsentType = (thisAbsent == null ? "" : thisAbsent.AbsentName),
                            StartTime = _form.StartTime,
                            EndTime = _form.EndTime,
                            Amount = _form.LeaveAmount,
                            Unit = _form.AbsentUnit,
                            LeaveReason = _form.LeaveReason,
                            AgentName = _form.Agent != null ? _form.Agent.EmployeeName : null,
                            FileName = _form.FileName,
                            FilePath = _form.FilePath,
                            IsAbroad = _form.IsAbroad,
                            IsHRM = false,
                            DHM = getAbsentdhm(_form.AbsentUnit, Convert.ToDecimal(_form.LeaveAmount)),
                            EmpID = _form.Employee.EmployeeNO,
                            DepartmentEnglishName = _form.Department.DepartmentEnglishName,
                            IsViewFile = IsViewFile
                        };
                        return PartialView("_LeaveContent", _model);
                    }
                }
                return PartialView("_LeaveContent", _viewmodel);
                //}
            }
        }

        public async Task<ActionResult> GetPLeaveDetail(string formNo, int formNoOrder = 0)
        {
            base.SetBaseUserInfo();
            LeaveCancelFormViewModel _viewmodel = new LeaveCancelFormViewModel();

            //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
            List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];

            LeaveForm _form = new LeaveForm();
            if (formNo.Substring(0, 1) == "P")
            {
                _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
            }
            else
            {
                //_form = await GetHRMLeaveDetail(CurrentUser.CompanyCode, formNo);
                return await GetHRMLeaveDetail(CurrentUser.CompanyCode, formNo, formNoOrder);
            }

            if (_form.Status == (int)FormStatus.Draft && _form.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/LeaveForm") + "?formNo=" + _form.FormNo;
            }
            //銷假按鈕顯示
            if (_form.Status == (int)FormStatus.Send && _form.EmployeeID == CurrentUser.EmployeeID && !Services.GetService<LeaveCancelService>().Any(x => x.LeaveFormID == _form.ID && !x.IsDeleted))
            {
                ViewBag.CanCancel = true;
                ViewBag.CancelURL = Url.Action("Index", "../M02/LeaveCancelForm") + "?LeaveFormID=" + _form.FormNo;
            }
            //if (_form.Status == (int)FormStatus.Send && _form.EmployeeID == CurrentUser.EmployeeID && !this.Services.GetService<LeaveCancelService>().CheckCompleteByLeaveFormNo(formNo, (int)FormStatus.Send))
            //{
            //    ViewBag.FormStatus = _form.Status;
            //    ViewBag.CancelURL = Url.Action("Index", "../M02/LeaveCancelForm") + "?LeaveFormID=" + _form.FormNo;
            //}
            //if (_form != null)
            //{

            AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.AbsentCode).FirstOrDefault();

            Employee AgentEmployeeEnglishName = null;
            if (_form.Agent != null)
            {
                AgentEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Agent.EmployeeNO);
            }

            Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Employee.EmployeeNO);
            List<AbsentDetail> data = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
            foreach (var i in data)
            {
                if (i.Code == _form.AbsentCode)
                {
                    LeaveDisplayModel _model = new LeaveDisplayModel()
                    {
                        AbsentNameEn = i.AbsentNameEn != null ? i.AbsentNameEn : (thisAbsent == null ? "" : thisAbsent.AbsentName),
                        AgentEmployeeEnglishName = _form.Agent != null ? AgentEmployeeEnglishName.EmployeeEnglishName : "",
                        SenderEmployeeEnglishName = SenderEmployeeEnglishName != null ? SenderEmployeeEnglishName.EmployeeEnglishName : "",
                        getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                        FormNo = _form.FormNo,
                        EmployeeName = _form.Employee.EmployeeName,
                        DepartmentName = _form.Department.DepartmentName,
                        CreatedTime = _form.CreatedTime,
                        AbsentType = (thisAbsent == null ? "" : thisAbsent.AbsentName),
                        StartTime = _form.StartTime,
                        EndTime = _form.EndTime,
                        Amount = _form.LeaveAmount,
                        Unit = _form.AbsentUnit,
                        LeaveReason = _form.LeaveReason,
                        AgentName = _form.Agent != null ? _form.Agent.EmployeeName : null,
                        FileName = _form.FileName,
                        FilePath = _form.FilePath,
                        IsAbroad = _form.IsAbroad,
                        IsHRM = false,
                        DHM = getAbsentdhm(_form.AbsentUnit, _form.LeaveAmount),
                        EmpID = _form.Employee.EmployeeNO,
                        DepartmentEnglishName = _form.Department.DepartmentEnglishName
                    };
                    return PartialView("_PLeaveContent", _model);
                }
            }
            return PartialView("_PLeaveContent", _viewmodel);
            //}
        }

        //獲得後臺(HRM)建立假單的詳細資料 by Bee 20160907
        private async Task<ActionResult> GetHRMLeaveDetail(string companyCode, string formNo, int formNoOrder)
        {
            //List<AbsentFormData> _form = await HRMApiAdapter.GetLeaveFormDetail(companyCode, formNo);
            List<AbsentFormData> _formList = await HRMApiAdapter.GetLeaveFormDetail(companyCode, formNo);
            AbsentFormData _form = _formList[formNoOrder];
            LeaveCancelFormViewModel _viewmodel = new LeaveCancelFormViewModel();

            //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
            List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];
            AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.AbsentCode).FirstOrDefault();

            Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.EmpID);
            List<AbsentDetail> data = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

            foreach (var i in data)
            {
                if (i.Code == _form.AbsentCode)
                {
                    LeaveDisplayModel _model = new LeaveDisplayModel()
                    {
                        AbsentNameEn = i.AbsentNameEn != null ? i.AbsentNameEn : (thisAbsent == null ? "" : thisAbsent.AbsentName),
                        AgentEmployeeEnglishName = _form.AgentNameEN != null ? _form.AgentNameEN : "",
                        SenderEmployeeEnglishName = SenderEmployeeEnglishName != null ? SenderEmployeeEnglishName.EmployeeEnglishName : "",
                        getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                        FormNo = _form.FormNo,
                        EmployeeName = _form.EmpName,
                        DepartmentName = _form.DeptName,
                        AbsentType = (thisAbsent == null ? "" : thisAbsent.AbsentName),
                        LeaveReason = _form.LeaveReason,
                        StartTime = _form.BeginTime,
                        EndTime = _form.EndTime,
                        Unit = "h",
                        AgentName = _form.AgentName != null ? _form.AgentName : "",
                        Amount = Convert.ToDecimal(_form.AbsentAmount),
                        IsHRM = true,
                        DHM = getAbsentdhm(_form.AbsentUnit, Convert.ToDecimal(_form.AbsentAmount)),
                        EmpID = _form.EmpID,
                        DepartmentEnglishName = _form.DeptNameEN
                    };
                    return PartialView("_LeaveContent", _model);
                }
            }

            return PartialView("_LeaveContent", _viewmodel);
        }

        //獲得後臺加班單得詳細資料 by Bee 20160907
        private async Task<ActionResult> GetHRMOvertimeDetail(string companyCode, string formNo)
        {
            OverTimeDisplayModel _viewmodel = new OverTimeDisplayModel();
            OvertimeFormData _form = await HRMApiAdapter.GetOverTimeFormDetail(companyCode, formNo);
            string OverTimeTypeName = _form.OverTimeReasonCode;
            var OverTimeReasonList = await HRMApiAdapter.GetOverTimeReasons(companyCode);

            foreach (var reason in OverTimeReasonList)
            {
                if (_form.OverTimeReasonCode == reason.Code)
                {
                    OverTimeTypeName = reason.Name;
                    break;
                }
            }

            //參數設定檔
            List<FormSetting> _formSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode).ToList();
            bool enableSettingEatingTime = _formSettings.FirstOrDefault(x => x.SettingKey == "enableSettingEatingTime").SettingValue == "true" ? true : false;

            Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.EmpID);
            OverTimeDisplayModel _model = new OverTimeDisplayModel()
            {
                SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                EmployeeName = _form.EmpName,
                DepartmentName = _form.DeptName,
                CreatedTime = _form.CreateTime,
                StartTime = _form.BeginTime,
                EndTime = _form.EndTime,
                Amount = Convert.ToDecimal(_form.OvertimeAmount),
                HaveDining = _form.HaveDinning == "n" ? false : true,
                Compensation = _form.Compensation == true ? "0" : "1",
                OverTimeTypeName = OverTimeTypeName,
                OverTimeReason = _form.OverTimeReason,
                CutTime = _form.CutTime,
                EnableSettingEatingTime = enableSettingEatingTime,
                IsHRM = true,
                DepartmentEnglishName = _form.DeptNameEN

            };
            return PartialView("_OverTimeContent", _model);
        }

        public async Task<ActionResult> GetOverTimeDetail(string formNo)
        {
            if (formNo.Substring(0, 1) != "P")
            {
                return await GetHRMOvertimeDetail(CurrentUser.CompanyCode, formNo);
            }
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

            //參數設定檔
            List<FormSetting> _formSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode).ToList();
            bool enableSettingEatingTime = _formSettings.FirstOrDefault(x => x.SettingKey == "enableSettingEatingTime").SettingValue == "true" ? true : false;

            if (_form.Status == (int)FormStatus.Draft && _form.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/OverTimeForm") + "?formNo=" + _form.FormNo;
            }
            if (_form.Status == (int)FormStatus.Send && _form.EmployeeID == CurrentUser.EmployeeID && !Services.GetService<OverTimeCancelService>().Any(x => x.OverTimeFormID == _form.ID && !x.IsDeleted))
            {
                ViewBag.CanCancel = true;
                ViewBag.CancelURL = Url.Action("Index", "../M02/OverTimeCancelForm") + "?OverTimeFormID=" + _form.FormNo;
            }
            //if (_form.Status == (int)FormStatus.Send && _form.EmployeeID == CurrentUser.EmployeeID && !this.Services.GetService<OverTimeCancelService>().CheckCompleteByOverTimeFormNo(formNo, (int)FormStatus.Send))
            //{
            //ViewBag.FormStatus = _form.Status;
            //ViewBag.CancelURL = Url.Action("Index", "../M02/OverTimeCancelForm") + "?OverTimeFormID=" + _form.FormNo;
            //}
            //if (_form != null)
            //{
            OverTimeFormData _formData = new OverTimeFormData(_form);
            bool IsViewFile = getViewFilePermission(_formData);

                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Employee.EmployeeNO);
                OverTimeDisplayModel _model = new OverTimeDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
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
                    CutTime = _form.CutTime.HasValue == true ? _form.CutTime.Value : 0,
                    EnableSettingEatingTime = enableSettingEatingTime,
                    IsHRM = false,
                    FormNo = _form.FormNo,
                    FileName = _form.FileName,
                    FilePath = _form.FilePath,
                    DepartmentEnglishName = _form.Department.DepartmentEnglishName,
                    IsViewFile = IsViewFile
                };
                return PartialView("_OverTimeContent", _model);
            //}
        }

        public ActionResult GetOverTimeCancelDetail(string formNo)
        {
            OverTimeCancelFormViewModel _viewmodel = new OverTimeCancelFormViewModel();
            OverTimeCancel _form = this.Services.GetService<OverTimeCancelService>().GetOverTimeCancelByFormNo(formNo);
            if (_form.Status == (int)FormStatus.Draft && _form.OverTimeForm.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/OverTimeCancelForm") + "?formNo=" + _form.FormNo;
            }

            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.OverTimeForm.Employee.EmployeeNO);
                OverTimeDisplayModel _model = new OverTimeDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
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

        public ActionResult GetPatchCardCancelDetail(string formNo)
        {
            PatchCardCancelFormViewModel _viewmodel = new PatchCardCancelFormViewModel();
            PatchCardCancel _form = this.Services.GetService<PatchCardCancelService>().GetOverTimeCancelByFormNo(formNo);
            if (_form.Status == (int)FormStatus.Draft && _form.PatchCardForm.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/PatchCardCancelForm") + "?formNo=" + _form.FormNo;
            }

            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.PatchCardForm.Employee.EmployeeNO);
                PatchCardDisplayModel _model = new PatchCardDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                    EmployeeName = _form.PatchCardForm.Employee.EmployeeName,
                    DepartmentName = _form.PatchCardForm.Department.DepartmentName,
                    CreatedTime = _form.PatchCardForm.CreatedTime,
                    Reason = _form.PatchCardForm.Reason,
                    Time = _form.PatchCardForm.PatchCardTime,
                    Type = Convert.ToString(_form.PatchCardForm.Type) == "1" ? "上班" : "下班",
                    DepartmentEnglishName = _form.PatchCardForm.Department.DepartmentEnglishName
                };
                _viewmodel.FormData = _form;
                _viewmodel.SourceData = _model;
                return PartialView("_PatchCardCancelContent", _viewmodel);
            //}
        }

        public ActionResult GetPatchCardDetail(string formNo)
        {
            PatchCardForm _form = this.Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == formNo);
            if (_form.Status == (int)FormStatus.Draft && _form.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/PatchCardForm") + "?formNo=" + _form.FormNo;
            }
            
            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Employee.EmployeeNO);
                PatchCardDisplayModel _model = new PatchCardDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
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

        public ActionResult GetSPatchCardDetail(string formNo)
        {
            PatchCardForm _form = this.Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == formNo);
            if (_form.Status == (int)FormStatus.Draft && _form.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/PatchCardForm") + "?formNo=" + _form.FormNo;
            }
            //if (_form != null)
            //{
                Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, _form.Employee.EmployeeNO);
                PatchCardDisplayModel _model = new PatchCardDisplayModel()
                {
                    SenderEmployeeEnglishName = SenderEmployeeEnglishName.EmployeeEnglishName,
                    getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW",
                    FormNo = _form.FormNo,
                    EmployeeName = _form.Employee.EmployeeName,
                    DepartmentName = _form.Department.DepartmentName,
                    Time = _form.PatchCardTime,
                    Type = _form.Type == 1 ? "上班" : "下班",
                    ReasonType = _form.ReasonType == 0 ? "正常刷卡" : _form.ReasonType == 1 ? "忘記刷卡" : "忘記帶卡",
                    Reason = _form.Reason,
                    FileName = _form.FileName,
                    FilePath = _form.FilePath
                };
                return PartialView("_SPatchCardContent", _model);
            //}
        }

        //取附件檔案 Irving 20161209
        public ActionResult DownloadLeaveFormFile(string formNo, string formType = "Leave")
        {
            string _path = System.Web.HttpContext.Current.Server.MapPath(Services.GetService<SystemSettingService>().GetSettingValue("LeaveFormFiles"));

            string FilePath = "";
            string FileFormat = "";
            string FileName = "";
            if (formType == "Leave")
            {
                LeaveForm _form = this.Services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == formNo);
                FilePath = _form.FilePath;
                FileFormat = _form.FileFormat;
                FileName = _form.FileName;
            }
            else
            {
                OverTimeForm _form = this.Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == formNo);
                FilePath = _form.FilePath;
                FileFormat = _form.FileFormat;
                FileName = _form.FileName;
            }

            try
            {
                using (FileStream _file = new FileStream(Path.Combine(_path, FilePath), FileMode.Open))
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
                        return File(_ms.ToArray(), FileFormat, FileName);
                    }
                }
            }
            catch
            {
                Response.Write("<script >alert('此檔案已過期或無此檔案,請回上一頁');</script>");
                return null;
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
                if (_index > 0 && _index < _signFlow.Count && _signFlow[_index - 1].FormLevelID == "0" && (_signFlow[0].SignerEmployeeNo == CurrentUser.EmployeeNO || _signFlow[0].ActSignerEmployeeNo == CurrentUser.EmployeeNO))
                {
                    ViewBag.CanPullBack = true;
                }

                foreach (var i in _signFlow)
                {
                    if (i.ActSignerEmployeeNo != null)
                    {
                        Employee AgentEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, i.ActSignerEmployeeNo);
                        i.ActSignerEmployeeEnglishName = AgentEmployeeEnglishName != null ? AgentEmployeeEnglishName.EmployeeEnglishName : "";
                    }
                    if (i.SignerEmployeeNo != null)
                    {
                        Employee SenderEmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, i.SignerEmployeeNo);

                        i.SignerEmployeeEnglishName = SenderEmployeeEnglishName != null ? SenderEmployeeEnglishName.EmployeeEnglishName : "";
                    }
                    i.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "zh-TW";
                }

                return PartialView("_SignFlowView", _signFlow);
            }
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
            PatchCardCancel _patchCardCancel = null;
            RequestResult _checkResult = null;
            DeleteResponse _deleteResponse = null;
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

                        //已申請尚未核准的當月加班時數。
                        double inProcessingAmt = Services.GetService<OverTimeFormService>().GetInProcessingAmtByMonth(_overTimeForm.EmployeeID, _overTimeForm.StartTime);

                        _checkResult = await HRMApiAdapter.CheckOverTime(
                              _overTimeForm.Company.CompanyCode, _overTimeForm.Employee.EmployeeNO,
                              _overTimeForm.StartTime, _overTimeForm.EndTime, true,
                              _overTimeForm.CompensationWay == 0, _overTimeForm.HaveDinning,
                              _overTimeForm.HaveDinning, enableSettingEatingTime, (_overTimeForm.CutTime.HasValue == true ? _overTimeForm.CutTime.Value : 0), isCheckDutyCard,
                              inProcessingAmt);
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
                        _deleteResponse = await HRMApiAdapter.CheckDeletePatchCard(
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

                //20180316 Daniel Start 簽核完成，更新ePortal簽核箱
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
                    if (_formType==FormType.Leave) //請假單
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
                                    WriteLog("假單核准更新SmartSheet紀錄：" + retValue);

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
                                    WriteLog("假單核准更新BambooHR Time Off狀態(第1次)：" + retValue);

                                    //20210305 Daniel 因BambooHR流程如果是兩關，API_Owner核准需發送兩次更新狀態交易才能真的完成
                                    retValue = bbService.ChangeLeaveFormStatus(leaveForm, TimeOffRequestStatus.approved, note, TimeOffRequestStatus.requested, "HRPortal", true);
                                    WriteLog("假單核准更新BambooHR Time Off狀態(第2次)：" + retValue);
                                }
                            }
                        }
                    }
                    else if (_formType==FormType.LeaveCancel) //銷假單
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
                                        WriteLog("銷假刪除SmartSheet紀錄：" + retValue);

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

        [HttpPost]
        public async Task<ActionResult> BatchAccept(SignModel[] model)
        {
            Dictionary<string, AjaxResult> _result = new Dictionary<string, AjaxResult>();
            foreach (SignModel _form in model.OrderBy(x => x.FormNo))
            {
                AjaxResult _formResult;
                try
                {
                    _formResult = (await Accept(_form) as JsonResult).Data as AjaxResult;
                }
                catch (Exception ex)
                {
                    _formResult = new AjaxResult()
                    {
                        status = "failed",
                        message = ex.Message,
                    };
                }

                _result.Add(_form.FormNo, _formResult);
            }

            return Json(_result);
        }

        [HttpPost]
        public ActionResult PullBack(SignModel model)
        {
            return Reject(model, true);
        }

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
                                    WriteLog("假單" + (isPullBack ? "拉回" : "退回") + "刪除SmartSheet紀錄：" + retValue);
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
                                    string retValue = bbService.ChangeLeaveFormStatus(leaveForm, (isPullBack ? TimeOffRequestStatus.canceled : TimeOffRequestStatus.denied), note);
                                    WriteLog("假單" + (isPullBack ? "拉回" : "退回") + "更新BambooHR Time Off狀態：" + retValue);
                                }
                            }
                        }
                    }


                    string errMessage="";
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

        //20181005 Daniel 增加表單狀態判斷，避免拉回或是已核准的狀態又被更新
        private string CheckFormStatus(int Status,string SignActionType)
        {
            string errMessage="";
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="pUnit"></param>
        /// <param name="pAmount"></param>
        /// <returns></returns>
        private string getAbsentdhm(string pUnit, decimal pAmount)
        {
            string retval = string.Empty;
            int workhours = 8;
            decimal d = 0, h = 0, m = 0;

            if (pUnit.ToLower().Equals("d")) pAmount = pAmount * workhours;

            d = Math.Truncate(pAmount / workhours);
            h = Math.Truncate(pAmount % workhours);
            m = (pAmount - Math.Truncate(pAmount)) * 60;
            retval = string.Format("{0}天{1}時{2}分", d, h, Convert.ToInt16(m));

            return retval;
        }

        private bool getViewFilePermission(IFormData formData)
        {
            GetRole();
            if (isHR || isAdmin)
            {
                return true;
            }

            LeaveSignList _LsignList = new LeaveSignList();
            OverTimeSignList _OsignList = new OverTimeSignList();

            var _signFlow = formData.FormType == "Leave" ? _LsignList.GetAllSignPermission(formData, false) : _OsignList.GetAllSignPermission(formData, false);
            bool Result = false;
            List<string> empList = new List<string>();
            empList = _signFlow.Select(s => s.SignerID).Distinct().ToList();
            empList.Add(_signFlow[0].SenderID);

            foreach (var check in empList)
            {
                if (check == CurrentUser.EmployeeNO)
                {
                    Result = true;
                    break;
                }
            }
          
            return Result;
        }

        /// <summary>
        /// 使用者角色
        /// </summary>
        public bool GetRole()
        {
            bool result = false;

            try
            {
                Role roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();

                if (roleData != null)
                {
                    if (!string.IsNullOrEmpty(roleData.RoleParams))
                    {
                        dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                        isHR = (roleParams.is_hr != null && roleParams.is_hr);
                        isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}
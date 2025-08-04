using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
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
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.Areas.M02.Controllers
{
    public class OverTimeCancelFormController : BaseController
    {

        // GET: M02/OverTimeCancelForm
        private IFormNoGenerator formNoGenerator;
        public ActionResult Index(string FormNo = "", string OverTimeFormID = "")
        {

            ViewBag.LanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            
            //初始化
            this.formNoGenerator = new FormNoGenerator();
            OverTimeCancel _formdata = new OverTimeCancel();
            OverTimeCancelFormViewModel _viewModel = new OverTimeCancelFormViewModel();
            if (string.IsNullOrWhiteSpace(FormNo) && string.IsNullOrWhiteSpace(OverTimeFormID))
            {
                ViewBag.StartTime = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
                ViewBag.EndTime = DateTime.Now.ToString("yyyy/MM/dd");
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(FormNo))
            {
                _formdata = this.Services.GetService<OverTimeCancelService>().GetOverTimeCancelByFormNo(FormNo);
                if (_formdata != null)
                {
                    Session["Mode"] = FormEditMode.Edit.ToString();
                }

            }
            else if (!string.IsNullOrWhiteSpace(OverTimeFormID))
            {
                Session["Mode"] = FormEditMode.Edit.ToString();
                _formdata = this.Services.GetService<OverTimeCancelService>().GetOverTimeCancelByOverTimeFormID(OverTimeFormID);
                if (_formdata == null)
                {
                    OverTimeForm _overtimeformdata = this.Services.GetService<OverTimeFormService>().GetAll().Where(x => x.FormNo == OverTimeFormID).FirstOrDefault();
                    if (_overtimeformdata != null)
                    {
                        _formdata = new OverTimeCancel()
                        {
                            OverTimeFormID = _overtimeformdata.ID,
                            ID = Guid.NewGuid(),
                            OverTimeForm = _overtimeformdata,
                            FormNo = formNoGenerator.GetOverTimeCancelFormNo()
                        };

                        Session["Mode"] = FormEditMode.Create.ToString();
                    }
                    else
                    {
                        TempData["message"] = "查無此單。";
                        return RedirectToAction("Index");
                    }

                }
            }
            SetBaseUserInfo();
            if (Session["Mode"] != null && Session["Mode"].ToString() == FormEditMode.Edit.ToString())
            {
                ViewBag.StatusEdit = "True";
            }
            if (_formdata != null && _formdata.ID != null)
            {
                //判斷該筆是否為此登入者的
                if (_formdata.OverTimeForm.EmployeeID != CurrentUser.EmployeeID)
                {
                    TempData["message"] = "您無此單的編輯權限。";
                    return RedirectToAction("Index");
                }
                _viewModel = SetViewModel(_formdata);

                return View(_viewModel);
            }
            else
            {
                TempData["message"] = "查無此單。";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(OverTimeCancelFormViewModel viewModel)
        {
            int IsSuccess = 1;
            try
            {
                if ((string)Session["Mode"] == FormEditMode.Create.ToString())
                {
                    viewModel.FormData.Createdby = CurrentUser.EmployeeID;
                    viewModel.FormData.Status = (int)FormStatus.Draft;
                    IsSuccess = Services.GetService<OverTimeCancelService>().Create(viewModel.FormData);
                }
                else
                {
                    if (viewModel.FormData.Status >= (int)FormStatus.Signing)
                    {
                        IsSuccess = 0;
                    }
                    else
                    {
                        viewModel.FormData.Modifiedby = CurrentUser.EmployeeID;
                        var olddata = Services.GetService<OverTimeCancelService>().GetOverTimeCancelByID(viewModel.FormData.ID);
                        IsSuccess = Services.GetService<OverTimeCancelService>().Update(olddata, viewModel.FormData, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult() { status = "failed", message = ex.Message });
            }

            OverTimeCancel _form = Services.GetService<OverTimeCancelService>().GetOverTimeCancelByID(viewModel.FormData.ID);
            if (IsSuccess == 1)
            {
                //TempData["message"] = "成功";
                WriteLog("Success:" + viewModel.FormData.ID);

                try
                {
                    //SignFlow
                    OverTimeCancelSignList _signList = new OverTimeCancelSignList();
                    SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();

                    string _senderFlowId;
                    IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(_form.FormNo);
                    if (_signFlow == null || _signFlow.Count == 0)
                    {
                        _signFlow = _signList.CopyFlow(_form.FormNo, _form.OverTimeForm.FormNo, _form.OverTimeForm.Employee.EmployeeNO);
                        _signList.SaveSigningFlow(_signFlow, null);
                        _senderFlowId = _signFlow[0].ID;
                    }
                    else
                    {
                        _senderFlowId = _signFlow.Last().ID;
                        string _lastOrder = _signFlow.Last().SignOrder;
                        decimal _groupId = _signFlow.Last().GroupID;
                        foreach (SignFlowRecModel _flowItem in _signList.CopyFlow(_form.FormNo, _form.OverTimeForm.FormNo, _form.OverTimeForm.Employee.EmployeeNO, false))
                        {
                            _flowItem.GroupID = _groupId;
                            _signFlow.Add(_flowItem);
                        }
                        _signList.SaveSigningFlow(_signFlow, _lastOrder);
                    }

                    SignMailHelper _mailHelper = new SignMailHelper(await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode));
                    _signList.OnFlowAccepted += _mailHelper.SendMailOnFlowAccepted;

                    _signList.Accept(_form.FormNo, _senderFlowId, CurrentUser.EmployeeNO, string.Empty);

                    //20180316 Start Daniel 加班單刪除送出，更新ePortal簽核箱
                    SignFlowRecRepository signFlowRecRepository = new SignFlowRecRepository();
                    //找出目前表單編號對應的流程中，現在需要簽核的人為何
                    //FormNo在Portal不會重覆
                    var signFlowList = signFlowRecRepository.GetAll().Where(x => x.FormNumber == _form.FormNo).ToList();
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
                            docno = _form.FormNo, //表單編號
                            formdesc = "加班單銷單申請",
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

                    _form.Status = (int)FormStatus.Signing;
                    Services.GetService<OverTimeCancelService>().Update(_form);
                }
                catch (Exception ex)
                {
                    _form.IsDeleted = true;
                    Services.GetService<OverTimeCancelService>().Update(_form);
                    return Json(new AjaxResult() { status = "failed", message = ex.Message });
                }

                //return RedirectToAction("Index");
                return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
            }
            return Json(new AjaxResult() { status = "failed", message = Resource.SendFailed });
            //return View();
        }

        private OverTimeCancelFormViewModel SetViewModel(OverTimeCancel _formData)
        {
            OverTimeCancelFormViewModel viewModel = new OverTimeCancelFormViewModel();
            OverTimeForm _form = _formData.OverTimeForm;
            OverTimeDisplayModel _model = new OverTimeDisplayModel()
            {
                EmployeeName = _form.Employee.EmployeeName,
                DepartmentName = _form.Department.DepartmentName,
                CreatedTime = _form.CreatedTime,
                StartTime = _form.StartTime,
                EndTime = _form.EndTime,
                Amount = _form.OverTimeAmount,
                Compensation = _form.CompensationWay.ToString(),
                HaveDining = _form.HaveDinning,
                OverTimeTypeName = _form.OverTimeReasonCode,
                OverTimeReason = _form.OverTimeReason
            };


            viewModel.SourceData = _model;
            viewModel.FormData = _formData;
            return viewModel;
        }

        [HttpPost]
        public async Task<ActionResult> CheckDeleteOverTimeData(OverTimeCancelFormViewModel model)
        {
            string _formNo = Services.GetService<OverTimeFormService>().GetOverTimeFormByID(model.FormData.OverTimeFormID).FormNo;
            var _checkResult = await HRMApiAdapter.CheckDeleteOverTime(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, _formNo);
            if (!_checkResult.Status || _checkResult.isLocked)
            {
                return Json(new AjaxResult() { status = "failed", message = _checkResult.Message });
            }
            else
            {
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }
        }

        [HttpPost]
        public ActionResult DeletedForm(OverTimeCancelFormViewModel model)
        {
            OverTimeCancel data = Services.GetService<OverTimeCancelService>().GetOverTimeCancelByID(model.FormData.ID);
            int _result = Services.GetService<OverTimeCancelService>().Delete(data, data, CurrentUser.EmployeeID, true);

            //20160201 刪除編輯中的假單時流程中的假單也要同時刪除
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

        public async Task<ActionResult> SearchList(int page = 1, string beginDate = "", string endDate = "")
        {
            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);
            int currentPage = page < 1 ? 1 : page;
            ViewBag.StartTime = beginDate;
            ViewBag.EndTime = endDate;
            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }
            Session["AbsentsDataAll"] = await HRMApiAdapter.GetAllAbsentData("");
            var model = Query(DateTime.Parse(beginDate), DateTime.Parse(endDate)).ToPagedList(currentPage, currentPageSize);
            return View(model);
        }

        private List<HRPotralFormSignStatus> Query(DateTime startTime, DateTime endTime)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = new List<HRPotralFormSignStatus>();
                _result = _queryHelper.GetCurrentSignListByEmployee(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, false, startTime, endTime.AddDays(1));
                _result = _result.Where(x => x.FormType == FormType.OverTime && x.FormStatus == (int)FormStatus.Send).ToList();

                List<HRPotralFormSignStatus> removedata = new List<HRPotralFormSignStatus>();

                //移除已送出的銷加班單
                foreach (var item in _result)
                {
                    var tmpdata = Services.GetService<OverTimeCancelService>().GetOverTimeCancelByOverTimeFormID(item.FormNo);
                    if (tmpdata != null)
                        removedata.Add(item);

                }
                //remove
                foreach (var item in removedata)
                {
                    _result.Remove(item);
                }


                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    _summaryBuilder.BuildSummary(_item);
                }

                return _result;
            }
        }



        public async Task<ActionResult> LoadSearchList(int page = 1, string BeginDate = "", string EndDate = "")
        {
            int currentPage = page < 1 ? 1 : page;
            ViewBag.StartTime = BeginDate;
            ViewBag.EndTime = EndDate;
            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }
            return PartialView("SearchList", Query(DateTime.Parse(BeginDate), DateTime.Parse(EndDate)).ToPagedList(currentPage, currentPageSize));
        }
    }
}
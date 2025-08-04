using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.MultiLanguage;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.Areas.M02.Controllers
{
    public class PatchCardCancelFormController : BaseController
    {
        // GET: M02/OverTimeCancelForm
        private IFormNoGenerator formNoGenerator;
        public ActionResult Index(string FormNo = "", string PatchCardFormID = "")
        {
            //初始化
            this.formNoGenerator = new FormNoGenerator();
            PatchCardCancel _formdata = new PatchCardCancel();
            PatchCardCancelFormViewModel _viewModel = new PatchCardCancelFormViewModel();
            if (string.IsNullOrWhiteSpace(FormNo) && string.IsNullOrWhiteSpace(PatchCardFormID))
            {
                ViewBag.StartTime = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
                ViewBag.EndTime = DateTime.Now.ToString("yyyy/MM/dd");
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(FormNo))
            {
                _formdata = this.Services.GetService<PatchCardCancelService>().GetOverTimeCancelByFormNo(FormNo);
                if (_formdata != null)
                {
                    Session["Mode"] = FormEditMode.Edit.ToString();
                }

            }
            else if (!string.IsNullOrWhiteSpace(PatchCardFormID))
            {
                Session["Mode"] = FormEditMode.Edit.ToString();
                _formdata = this.Services.GetService<PatchCardCancelService>().GetOverTimeCancelByOverTimeFormID(PatchCardFormID);
                if (_formdata == null)
                {
                    PatchCardForm _overtimeformdata = this.Services.GetService<PatchCardFormService>().GetAll().Where(x => x.FormNo == PatchCardFormID).FirstOrDefault();
                    if (_overtimeformdata != null)
                    {
                        _formdata = new PatchCardCancel()
                        {
                            PatchCardFormID = _overtimeformdata.ID,
                            ID = Guid.NewGuid(),
                            PatchCardForm = _overtimeformdata,
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
                if (_formdata.PatchCardForm.EmployeeID != CurrentUser.EmployeeID)
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
        public async Task<ActionResult> Index(PatchCardCancelFormViewModel viewModel)
        {
            int IsSuccess = 1;
            try
            {
                if ((string)Session["Mode"] == FormEditMode.Create.ToString())
                {
                    viewModel.FormData.Createdby = CurrentUser.EmployeeID;
                    viewModel.FormData.Status = (int)FormStatus.Draft;
                    IsSuccess = Services.GetService<PatchCardCancelService>().Create(viewModel.FormData);
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
                        var olddata = Services.GetService<PatchCardCancelService>().GetOverTimeCancelByID(viewModel.FormData.ID);
                        IsSuccess = Services.GetService<PatchCardCancelService>().Update(olddata, viewModel.FormData, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult() { status = "failed", message = ex.Message });
            }

            PatchCardCancel _form = Services.GetService<PatchCardCancelService>().GetOverTimeCancelByID(viewModel.FormData.ID);
            if (IsSuccess == 1)
            {
                //TempData["message"] = "成功";
                WriteLog("Success:" + viewModel.FormData.ID);

                try
                {
                    //SignFlow
                    PatchCardCancelSignList _signList = new PatchCardCancelSignList();
                    SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();

                    string _senderFlowId;
                    IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(_form.FormNo);
                    if (_signFlow == null || _signFlow.Count == 0)
                    {
                        _signFlow = _signList.CopyFlow(_form.FormNo, _form.PatchCardForm.FormNo, _form.PatchCardForm.Employee.EmployeeNO);
                        _signList.SaveSigningFlow(_signFlow, null);
                        _senderFlowId = _signFlow[0].ID;
                    }
                    else
                    {
                        _senderFlowId = _signFlow.Last().ID;
                        string _lastOrder = _signFlow.Last().SignOrder;
                        decimal _groupId = _signFlow.Last().GroupID;
                        foreach (SignFlowRecModel _flowItem in _signList.CopyFlow(_form.FormNo, _form.PatchCardForm.FormNo, _form.PatchCardForm.Employee.EmployeeNO, false))
                        {
                            _flowItem.GroupID = _groupId;
                            _signFlow.Add(_flowItem);
                        }
                        _signList.SaveSigningFlow(_signFlow, _lastOrder);
                    }

                    SignMailHelper _mailHelper = new SignMailHelper(await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode));
                    _signList.OnFlowAccepted += _mailHelper.SendMailOnFlowAccepted;

                    _signList.Accept(_form.FormNo, _senderFlowId, CurrentUser.EmployeeNO, string.Empty);

                    _form.Status = (int)FormStatus.Signing;
                    Services.GetService<PatchCardCancelService>().Update(_form);
                }
                catch (Exception ex)
                {
                    _form.IsDeleted = true;
                    Services.GetService<PatchCardCancelService>().Update(_form);
                    return Json(new AjaxResult() { status = "failed", message = ex.Message });
                }

                //return RedirectToAction("Index");
                return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
            }
            return Json(new AjaxResult() { status = "failed", message = Resource.SendFailed });
            //return View();
        }

        private PatchCardCancelFormViewModel SetViewModel(PatchCardCancel _formData)
        {
            PatchCardCancelFormViewModel viewModel = new PatchCardCancelFormViewModel();
            PatchCardForm _form = _formData.PatchCardForm;
            PatchCardDisplayModel _model = new PatchCardDisplayModel()
            {
                EmployeeName = _form.Employee.EmployeeName,
                DepartmentName = _form.Department.DepartmentName,
                CreatedTime = _form.CreatedTime,
                Time = _form.PatchCardTime,
                Type =  Convert.ToString(_form.Type)=="1"?"上班":"下班",
                Reason=_form.Reason,
                
            };


            viewModel.SourceData = _model;
            viewModel.FormData = _formData;
            return viewModel;
        }

        [HttpPost]
        public async Task<ActionResult> CheckDeletePatchCardData(PatchCardCancelFormViewModel model)
        {
            string _formNo = Services.GetService<PatchCardFormService>().GetOverTimeFormByID(model.FormData.PatchCardFormID).FormNo;
            var _checkResult = await HRMApiAdapter.CheckDeletePatchCard(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, _formNo);
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
        public ActionResult DeletedForm(PatchCardCancelFormViewModel model)
        {
            PatchCardCancel data = Services.GetService<PatchCardCancelService>().GetOverTimeCancelByID(model.FormData.ID);
            int _result = Services.GetService<PatchCardCancelService>().Delete(data, data, CurrentUser.EmployeeID, true);

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
            var model = Query(DateTime.Parse(beginDate), DateTime.Parse(endDate)).ToPagedList(currentPage, currentPageSize);
            return View(model);
        }

        private List<HRPotralFormSignStatus> Query(DateTime startTime, DateTime endTime)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = new List<HRPotralFormSignStatus>();
                _result = _queryHelper.GetCurrentSignListByEmployee(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, false, startTime, endTime.AddDays(1));
                _result = _result.Where(x => x.FormType == FormType.PatchCard && x.FormStatus == (int)FormStatus.Send).ToList();

                List<HRPotralFormSignStatus> removedata = new List<HRPotralFormSignStatus>();

                //移除已送出的銷加班單
                foreach (var item in _result)
                {
                    var tmpdata = Services.GetService<PatchCardCancelService>().GetOverTimeCancelByOverTimeFormID(item.FormNo);
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
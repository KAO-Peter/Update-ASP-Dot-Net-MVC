using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.DBEntities;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.ApiAdapter;
using System.Threading.Tasks;
using HRPortal.Helper;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using YoungCloud.SignFlow.Model;
using System.Data.Entity;
using HRPortal.Mvc.Results;
using Newtonsoft.Json;
using YoungCloud.SignFlow.SignLists;
using HRPortal.MultiLanguage;


namespace HRPortal.Areas.M02.Controllers
{
    public class PatchCardFormController : BaseController
    {
        private IFormNoGenerator formNoGenerator;
        //
        // GET: /M02/PatchCardForm/
        public ActionResult Index(string formNo)
        {
            SetBaseUserInfo();
            //初始化
            this.formNoGenerator = new FormNoGenerator();
            PatchCardFormViewModel viewmodel = new PatchCardFormViewModel();
            PatchCardForm model = null;
            if (formNo != null)
            {
                model = Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == formNo);
                //判斷該筆是否為此登入者的
                if (model.EmployeeID != CurrentUser.EmployeeID)
                {
                    TempData["message"] = "您無此單的編輯權限。";
                    return RedirectToAction("Index");
                }
                ViewBag.StatusEdit = "True";
                Session["Mode"] = FormEditMode.Edit.ToString();
            }
            if (model == null)
            {
                model = new PatchCardForm();
                model.PatchCardTime = DateTime.Now;
                model.Type = (int)PatchCardFormType.StartWork;
                model.FormNo = formNoGenerator.GetPatchCardFormNo();
                Session["Mode"] = FormEditMode.Create.ToString();
            }
            viewmodel.FormData = model;
            viewmodel.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            Employee EmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, ViewBag.EmpID);
            ViewBag.EmployeeEnglishName = EmployeeEnglishName.EmployeeEnglishName;
            var DepartmentEnglishName = Services.GetService<DepartmentService>().GetAll().Where(x => x.DepartmentCode == CurrentUser.SignDepartmentCode).Select(x => x.DepartmentEnglishName).FirstOrDefault();
            ViewBag.DepartmentEnglishName = DepartmentEnglishName;
            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(PatchCardFormViewModel model, HttpPostedFileBase UploadFilePath)
        {
            //重整
            SetBaseUserInfo();
            if (UploadFilePath != null && UploadFilePath.ContentLength > 4000000)
            {
                //100mb= 800000000 bits
                //TempData["message"] = "檔案上限為100MB.";
                //return View(model);
                return Json(new AjaxResult() { status = "failed", message = "檔案上限為4MB." });

            }
            //if (!ModelState.IsValid)
            //{
            //    TempData["message"] = "驗證失敗請檢查頁面資料";
            //    return View(model);
            //}
            else
            {
                int IsSuccess = 1;
                try
                {
                    if (Session["Mode"] == FormEditMode.Create.ToString())
                    {
                        model.FormData.Createdby = CurrentUser.Employee.ID;
                        model.FormData.Status = (int)FormStatus.Draft;
                        model.FormData.CreatedTime = DateTime.Now;
                        model.FormData.CompanyID = CurrentUser.Employee.CompanyID;
                        model.FormData.EmployeeID = CurrentUser.Employee.ID;
                        model.FormData.DepartmentID = CurrentUser.Employee.SignDepartmentID;
                        model.FormData.ID = Guid.NewGuid();

                        IsSuccess = Services.GetService<PatchCardFormService>().Create(model.FormData, UploadFilePath);
                    }
                    else
                    {
                        PatchCardForm _form = Services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == model.FormData.FormNo);
                        if (_form.Status >= (int)FormStatus.Signing)
                        {
                            IsSuccess = 0;
                        }
                        else
                        {
                            _form.Type = model.FormData.Type;
                            _form.PatchCardTime = model.FormData.PatchCardTime;
                            _form.ReasonType = model.FormData.ReasonType;
                            _form.Reason = model.FormData.Reason;

                            IsSuccess = Services.GetService<PatchCardFormService>().Update(_form, UploadFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new AjaxResult() { status = "failed", message = ex.Message });
                }

                if (IsSuccess == 1)
                {
                    //TempData["message"] = "送出成功";
                    WriteLog("Success:" + model.FormData.ID);

                    PatchCardForm _form = Services.GetService<PatchCardFormService>().Where(x => x.ID == model.FormData.ID).Include(
                        x => x.Company).Include(x => x.Department).FirstOrDefault();
                    try
                    {
                        //SignFlow
                        PatchCardFormData _formData = new PatchCardFormData(_form);
                        PatchCardSignList _signList = new PatchCardSignList();
                        SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();

                        string _senderFlowId;
                        IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(_form.FormNo);
                        if (_signFlow == null || _signFlow.Count == 0)
                        {
                            _signFlow = _signList.GetDefaultSignList(_formData, true);
                            _signList.SaveSigningFlow(_signFlow, null);
                            _senderFlowId = _signFlow[0].ID;
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
                        }

                        SignMailHelper _mailHelper = new SignMailHelper(await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode));
                        _signList.OnFlowAccepted += _mailHelper.SendMailOnFlowAccepted;

                        _signList.Accept(_formData.FormNumber, _senderFlowId, CurrentUser.EmployeeNO, string.Empty);

                        _form.Status = (int)FormStatus.Signing;
                        Services.GetService<PatchCardFormService>().Update(_form);
                    }
                    catch (Exception ex)
                    {
                        _form.IsDeleted = true;
                        Services.GetService<PatchCardFormService>().Update(_form);
                        return Json(new AjaxResult() { status = "failed", message = ex.Message });
                    }


                    //return RedirectToAction("Index");
                    return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
                }
            }
          
            //TempData["message"] = "Submit error";
            //return View(model);
            return Json(new AjaxResult() { status = "failed", message = Resource.SendFailed });
        }

        [HttpPost]
        public async Task<ActionResult> CheckPatchCardForm(PatchCardFormViewModel model)
        {
            RequestResult _checkResult = await HRMApiAdapter.CheckDuty(CurrentUser.CompanyCode, CurrentUser.EmployeeNO,
                model.FormData.PatchCardTime, model.FormData.Type);
            if (!_checkResult.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = _checkResult.Message });
            }
            else
            {
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletedForm(PatchCardFormViewModel model)
        {
            PatchCardForm data = Services.GetService<PatchCardFormService>().GetPatchCardFormByID(model.FormData.ID);
            int _result = Services.GetService<PatchCardFormService>().Delete(data, data, CurrentUser.EmployeeID, true);

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
    }
}
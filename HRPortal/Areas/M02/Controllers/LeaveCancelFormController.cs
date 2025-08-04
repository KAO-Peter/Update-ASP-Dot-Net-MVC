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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;

namespace HRPortal.Areas.M02.Controllers
{
    public class LeaveCancelFormController : BaseController
    {
        // GET: M02/LeaveCancelForm
        private IFormNoGenerator formNoGenerator;
        public ActionResult Index(string FormNo = "", string LeaveFormID = "")
        {
            //初始化
            this.formNoGenerator = new FormNoGenerator();
            LeaveCancel _formdata = new LeaveCancel();
            LeaveCancelFormViewModel _viewModel = new LeaveCancelFormViewModel();
            if (string.IsNullOrWhiteSpace(FormNo) && string.IsNullOrWhiteSpace(LeaveFormID))
            {
                ViewBag.StartTime = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
                // 20190911 小榜 結束時間預帶2年後，先抓取所有預請的假單
                ViewBag.EndTime = DateTime.Now.AddYears(2).ToString("yyyy/MM/dd");
                DateTime starDate = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day);
                DateTime endDate = DateTime.Now.AddYears(2);
                List<HRPotralFormSignStatus> _tempData = new List<HRPotralFormSignStatus>();
                _tempData = Query(starDate, endDate);

                // 20190911 小榜 當有預請的假單時，將最後一天的結束日期帶入查詢預設值
                if (_tempData.Count > 0)
                {
                    endDate = DateTime.Parse(_tempData.OrderByDescending(o => o.EndTime).FirstOrDefault().EndTime + "");
                    ViewBag.EndTime = endDate.ToString("yyyy/MM/dd");
                }
                else
                {
                    ViewBag.EndTime = DateTime.Now.ToString("yyyy/MM/dd");
                }
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(FormNo))
            {
                _formdata = this.Services.GetService<LeaveCancelService>().GetLeaveCancelByFormNo(FormNo);
                if (_formdata != null)
                {
                    Session["Mode"] = FormEditMode.Edit.ToString();
                }

            }
            else if (!string.IsNullOrWhiteSpace(LeaveFormID))
            {
                Session["Mode"] = FormEditMode.Edit.ToString();
                _formdata = this.Services.GetService<LeaveCancelService>().GetLeaveCancelByLeaveFormID(LeaveFormID);
                if (_formdata == null)
                {
                    LeaveForm _leaveformdata = this.Services.GetService<LeaveFormService>().GetAll().Where(x => x.FormNo == LeaveFormID).FirstOrDefault();
                    if (_leaveformdata != null)
                    {
                        _formdata = new LeaveCancel()
                        {
                            LeaveFormID = _leaveformdata.ID,
                            ID = Guid.NewGuid(),
                            LeaveForm = _leaveformdata,
                            FormNo = formNoGenerator.GetLeaveCancelFormNo()
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

            //20190517 Daniel 增加語系判斷
            ViewBag.LanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            if (Session["Mode"] != null && Session["Mode"].ToString() == FormEditMode.Edit.ToString())
            {
                ViewBag.StatusEdit = "True";
            }
            if (_formdata != null && _formdata.ID != null)
            {
                //判斷該筆是否為此登入者的
                if (_formdata.LeaveForm.EmployeeID != CurrentUser.EmployeeID)
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
        public async Task<ActionResult> Index(LeaveCancelFormViewModel viewModel)
        {
            int IsSuccess = 1;
            try
            {
                if ((string)Session["Mode"] == FormEditMode.Create.ToString())
                {
                    viewModel.FormData.Createdby = CurrentUser.EmployeeID;
                    viewModel.FormData.Status = (int)FormStatus.Draft;
                    IsSuccess = Services.GetService<LeaveCancelService>().Create(viewModel.FormData);
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
                        var olddata = Services.GetService<LeaveCancelService>().GetLeaveCancelByID(viewModel.FormData.ID);
                        IsSuccess = Services.GetService<LeaveCancelService>().Update(olddata, viewModel.FormData, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult() { status = "failed", message = ex.Message });
            }

            LeaveCancel _form = Services.GetService<LeaveCancelService>().GetLeaveCancelByID(viewModel.FormData.ID);
            if (IsSuccess == 1)
            {
                //TempData["message"] = "成功";
                WriteLog("Success:" + viewModel.FormData.ID);

                try
                {
                    //SignFlow
                    LeaveCancelSignList _signList = new LeaveCancelSignList();
                    SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();

                    string _senderFlowId;
                    IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(_form.FormNo);
                    if (_signFlow == null || _signFlow.Count == 0)
                    {
                        _signFlow = _signList.CopyFlow(_form.FormNo, _form.LeaveForm.FormNo, _form.LeaveForm.Employee.EmployeeNO);
                        _signList.SaveSigningFlow(_signFlow, null);
                        _senderFlowId = _signFlow[0].ID;
                    }
                    else
                    {
                        _senderFlowId = _signFlow.Last().ID;
                        string _lastOrder = _signFlow.Last().SignOrder;
                        decimal _groupId = _signFlow.Last().GroupID;
                        foreach (SignFlowRecModel _flowItem in _signList.CopyFlow(_form.FormNo, _form.LeaveForm.FormNo, _form.LeaveForm.Employee.EmployeeNO, false))
                        {
                            _flowItem.GroupID = _groupId;
                            _signFlow.Add(_flowItem);
                        }
                        _signList.SaveSigningFlow(_signFlow, _lastOrder);
                    }

                    List<AbsentType> listAbsent = (List<AbsentType>)Session["Absents"];
                    //SignMailHelper _mailHelper = new SignMailHelper(await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode));
                    SignMailHelper _mailHelper = new SignMailHelper(listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentName), listAbsent.ToDictionary(x => x.AbsentCode, x => x.AbsentEnglishName));

                    _signList.OnFlowAccepted += _mailHelper.SendMailOnFlowAccepted;

                    _signList.Accept(_form.FormNo, _senderFlowId, CurrentUser.EmployeeNO, string.Empty);

                    //20180316 Start Daniel 請假單刪除送出，更新ePortal簽核箱
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
                            formdesc = "請假單銷單申請",
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
                    Services.GetService<LeaveCancelService>().Update(_form);
                }
                catch (Exception ex)
                {
                    _form.IsDeleted = true;
                    Services.GetService<LeaveCancelService>().Update(_form);
                    return Json(new AjaxResult() { status = "failed", message = ex.Message });
                }

                //return RedirectToAction("Index");
                return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
            }
            return Json(new AjaxResult() { status = "failed", message = Resource.SendFailed });
            //return View();
        }

        private LeaveCancelFormViewModel SetViewModel(LeaveCancel _formData)
        {
            LeaveCancelFormViewModel viewModel = new LeaveCancelFormViewModel();

            //Dictionary<string, string> _absent = (Dictionary<string, string>)Session["Absents"];
            List<AbsentType> _absent = (List<AbsentType>)Session["Absents"];

            LeaveForm _form = _formData.LeaveForm;
            if (_form.Status == (int)FormStatus.Draft && _form.EmployeeID == CurrentUser.EmployeeID)
            {
                ViewBag.CanResend = true;
                ViewBag.ResendUrl = Url.Action("Index", "../M02/LeaveForm") + "?formNo=" + _form.FormNo;
            }

            AbsentType thisAbsent = _absent.Where(x => x.AbsentCode == _form.AbsentCode).FirstOrDefault();

            LeaveDisplayModel _model = new LeaveDisplayModel()
            {
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
                AgentEmployeeEnglishName = _form.Agent != null ? _form.Agent.EmployeeEnglishName : null,
                FileName = _form.FileName,
                FilePath = _form.FilePath,
                IsAbroad = _form.IsAbroad,
                DepartmentEnglishName = _form.Department.DepartmentEnglishName, //20190517 Daniel 增加部門英文名稱
                AbsentNameEn = thisAbsent == null ? "" : thisAbsent.AbsentEnglishName //假別英文名稱
            };
            viewModel.SourceData = _model;
            viewModel.FormData = _formData;
            return viewModel;
        }

        [HttpPost]
        public async Task<ActionResult> CheckDeleteLeaveData(LeaveCancelFormViewModel model)
        {
            string _formNo = Services.GetService<LeaveFormService>().GetLeaveFormByID(model.FormData.LeaveFormID).FormNo;
            var _checkResult = await HRMApiAdapter.CheckDeleteLeave(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, _formNo);
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
        public ActionResult DeletedForm(LeaveCancelFormViewModel model)
        {
            LeaveCancel data = Services.GetService<LeaveCancelService>().GetLeaveCancelByID(model.FormData.ID);
            int _result = Services.GetService<LeaveCancelService>().Delete(data, data, CurrentUser.EmployeeID, true);

            //20160202 刪除編輯中的假單時流程中的假單也要同時刪除
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

        public ActionResult DownloadFile(string formNo)
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
                _result = _result.Where(x => x.FormType == FormType.Leave && x.FormStatus == (int)FormStatus.Send).ToList();

                List<HRPotralFormSignStatus> removedata = new List<HRPotralFormSignStatus>();

                //移除已送出的銷假單
                foreach (var item in _result)
                {
                    var tmpdata = Services.GetService<LeaveCancelService>().GetLeaveCancelByLeaveFormID(item.FormNo);
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
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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Model;
using YoungCloud.SignFlow.SignLists;



namespace HRPortal.Areas.M02.Controllers
{
    public class OverTimeFormController : BaseController
    {
        private IFormNoGenerator formNoGenerator;
        //
        // GET: /M02/OverTimeForm/
        public async Task<ActionResult> Index(string formNo)
        {
            SetBaseUserInfo();
            string OverTimeReason = "";
            //初始化
            this.formNoGenerator = new FormNoGenerator();
            OverTimeForm model = null;
            if (formNo != null)
            {
                model =  Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == formNo);
                //判斷該筆是否為此登入者的
                if (model.EmployeeID != CurrentUser.EmployeeID)
                {
                    TempData["message"] = "您無此單的編輯權限。";
                    return RedirectToAction("Index");
                }
                ViewBag.StatusEdit = "True";
                OverTimeReason = model.OverTimeReasonCode;
                Session["Mode"] = FormEditMode.Edit.ToString();
            }
            else
            {
                //20180312 Daniel 加班單預帶時間改為抓 班表結束時間 與 結束時間+1小時
                List<GetEmpScheduleClassTimeResponse> EmpScheduleClassTime = await GetEmpScheduleClassTime(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, DateTime.Now);
                GetEmpScheduleClassTimeResponse LastClassTime = EmpScheduleClassTime.OrderByDescending(x => x.EndTime).FirstOrDefault();
                DateTime sDatetime,eDatetime;
                if (LastClassTime != null)
                {
                    sDatetime = LastClassTime.EndTime;
                    eDatetime = LastClassTime.EndTime.AddHours(1);
                }
                else
                {
                    sDatetime = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                    eDatetime = sDatetime;
                }

                model = new OverTimeForm();

                //20180312 Daniel 加班單預帶時間改為抓 班表結束時間 與 結束時間+1小時
                //model.StartTime = DateTime.Now;
                //model.EndTime = DateTime.Now.AddHours(1);
                model.StartTime = sDatetime;
                model.EndTime = eDatetime;

                model.FormNo = formNoGenerator.GetLeaveFormNo();
                //O補修,1加班費
                model.CompensationWay = 0;
                //model.OverTimeAmount = 4;
                model.CutTime = 0;
                Session["Mode"] = FormEditMode.Create.ToString();
            }
            List<FormSetting> FormSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode);
        
            //新增取加班總時數 API  Irving 20161209
            DateTime DateTimeNow = DateTime.Now;
            DateTime FirstDay = new DateTime(DateTimeNow.Year, DateTimeNow.Month, 1);//本月第一天
            DateTime LastDay = new DateTime(DateTimeNow.AddMonths(1).Year, DateTimeNow.AddMonths(1).Month, 1).AddDays(-1);//本月最後一天
            var EmpOverTimeTotal = await HRMApiAdapter.GetOverTimeFormData(CurrentUser.EmployeeNO, FirstDay, LastDay);
            ViewBag.OverTimeTotal = EmpOverTimeTotal.Total;//加班總時數
            //End

            ViewData["OverTimeReasonList"] = await GetOverTimeReasonList(OverTimeReason);
            ViewBag.OverTimeData = "";
            ViewBag.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            //是否允許設定用餐時間
            ViewBag.enableSettingEatingTime = FormSettings.FirstOrDefault(x => x.SettingKey == "enableSettingEatingTime").SettingValue;
            Employee EmployeeEnglishName = Services.GetService<EmployeeService>().GetEmployeeByEmpNo(CurrentUser.CompanyID, ViewBag.EmpID);
            ViewBag.EmployeeEnglishName = EmployeeEnglishName.EmployeeEnglishName;
            var DepartmentEnglishName = Services.GetService<DepartmentService>().GetAll().Where(x => x.DepartmentCode == CurrentUser.SignDepartmentCode).Select(x => x.DepartmentEnglishName).FirstOrDefault();
            ViewBag.DepartmentEnglishName = DepartmentEnglishName;
            //抓取加班類型參數
            var OvertimeType = this.Services.GetService<SystemSettingService>().GetAll().Where(x => x.SettingKey == "OvertimeType").Select(x => x.SettingValue).FirstOrDefault();
            ViewBag.OvertimeType = OvertimeType;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(OverTimeForm model,
      string OverTimeData, HttpPostedFileBase UploadFilePath)
        {
            //判斷抓取加班當天是否為上班日 Irving 20171116
            DateTime startTime = new DateTime(model.StartTime.Year, model.StartTime.Month,model.StartTime.Day, 0, 0, 0);
            DateTime EndTime = new DateTime(model.EndTime.Year, model.EndTime.Month, model.EndTime.Day, 0, 0, 0);
            //DateTime EndTime = new DateTime(model.EndTime.Year, model.EndTime.Month, model.EndTime.AddDays(1).Day, 0, 0, 0);
            //20200416 小榜 結束日期如果剛好是月底最後一天，會造成抓取不到 TimetableName 造成無法判斷是否為例假日、國定假日
            EndTime = DateTime.Compare(startTime, EndTime) == 0 ? EndTime.AddDays(1) : EndTime;

            List<GetEmpScheduleClassTimeByStartEndTimeResponse> empScheduleTime = await HRMApiAdapter.GetEmpScheduleClassTimeByStartEndTime(CurrentUser.Employee.Company.CompanyCode, CurrentUser.EmployeeNO, startTime, EndTime);
            foreach (var i in empScheduleTime)
            {
                if (i.TimetableName != "上班日" && i.TimetableName != "休息日")
                {
                    return Json(new AjaxResult() { status = "failed", message = HRPortal.MultiLanguage.Resource.ErrorMessage_OvertimeForbidden });
                }
            }
            //End
            model.OverTimeAmount = (decimal)Session["OverTimeAmount"];
            if (!string.IsNullOrWhiteSpace(OverTimeData))
            {
                model.OverTimeReasonCode = OverTimeData;
            }
            if (UploadFilePath != null && UploadFilePath.ContentLength > 4000000)
            {
                //100mb= 800000000 bits
                return Json(new AjaxResult() { status = "failed", message = "檔案上限為4MB." });
            }
            //附件檔案卡控需必填 Irving20170218
            //if (UploadFilePath == null )
            //{
            //    return Json(new AjaxResult() { status = "failed", message = "附件檔案為必填" });
            //}
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResult() { status = "failed", message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                int IsSuccess = 1;
                try
                {
                    if (Session["Mode"] == FormEditMode.Create.ToString())
                    {

                        model.Createdby = CurrentUser.EmployeeID;
                        model.Status = (int)FormStatus.Draft;
                        model.ID = Guid.NewGuid();
                        model.CompanyID = CurrentUser.Employee.CompanyID;
                        model.EmployeeID = CurrentUser.EmployeeID;
                        model.DepartmentID = CurrentUser.Employee.SignDepartmentID;

                        IsSuccess = Services.GetService<OverTimeFormService>().Create(model, UploadFilePath);
                    }
                    else
                    {
                        OverTimeForm _form = Services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == model.FormNo);
                        if (_form.Status >= (int)FormStatus.Signing)
                        {
                            IsSuccess = 0;
                        }
                        else
                        {
                            _form.StartTime = model.StartTime;
                            _form.EndTime = model.EndTime;
                            _form.OverTimeAmount = model.OverTimeAmount;
                            _form.OverTimeReasonCode = model.OverTimeReasonCode;
                            _form.OverTimeReason = model.OverTimeReason;
                            _form.CompensationWay = model.CompensationWay;
                            _form.HaveDinning = model.HaveDinning;
                            _form.Modifiedby = CurrentUser.EmployeeID;
                            _form.ModifiedTime = DateTime.Now;

                            IsSuccess = Services.GetService<OverTimeFormService>().Updates(_form, UploadFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new AjaxResult() { status = "failed", message = ex.Message });
                }

                if (IsSuccess == 1)
                {

                    WriteLog("Success:" + model.ID);

                    model = Services.GetService<OverTimeFormService>().Where(x => x.ID == model.ID).Include(
                        x => x.Company).Include(x => x.Department).FirstOrDefault();

                    try
                    {
                        //SignFlow
                        OverTimeFormData _formData = new OverTimeFormData(model);
                        OverTimeSignList _signList = new OverTimeSignList();
                        SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();

                        string _senderFlowId;
                        IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(model.FormNo);
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

                        //20180316 Start Daniel 加班單送出，更新ePortal簽核箱
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
                                formdesc = "加班單申請",
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

                        model.Status = (int)FormStatus.Signing;
                        Services.GetService<OverTimeFormService>().Update(model);
                    }
                    catch (Exception ex)
                    {
                        model.IsDeleted = true;
                        Services.GetService<OverTimeFormService>().Update(model);
                        return Json(new AjaxResult() { status = "failed", message = ex.Message });
                    }

                    return Json(new AjaxResult() { status = "success", message = Resource.SendSuccess });
                    //return RedirectToAction("Index");
                }
            }
            //重整
            ViewBag.OverTimeData = OverTimeData;
            SetBaseUserInfo();
            ViewData["OverTimeReasonList"] = await GetOverTimeReasonList();
            return Json(new AjaxResult() { status = "failed", message = Resource.SendFailed });
        }

        [HttpPost]
        public ActionResult CheckCrossOverTime(OverTimeForm model)
        {
            if (Services.GetService<OverTimeFormService>().CheckOverTimeFormExist(model.StartTime, model.EndTime, CurrentUser.EmployeeID))
            {
                return Json(new AjaxResult() { status = "failed", message = "已有此區間加班單" });
               
            }
                return Json(new AjaxResult() { status = "success", message = string.Empty });
        }

        [HttpPost]
        public async Task<ActionResult> CheckOverTimeForm(OverTimeForm model)
        {
            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<FormSetting> FormSettings = Services.GetService<FormSettingService>().GetFormParameterByCompamyCode(CurrentUser.Employee.Company.CompanyCode);
            bool enableSettingEatingTime = FormSettings.FirstOrDefault(x => x.SettingKey == "enableSettingEatingTime").SettingValue == "true" ? true : false;
            //加班單是否檢核刷卡資料
            bool isCheckDutyCard = FormSettings.FirstOrDefault(x => x.FormType == "OverTime" && x.SettingKey == "isCheck").SettingValue == "true" ? true : false;

            //已申請尚未核准的當月加班時數。
            double inProcessingAmt = Services.GetService<OverTimeFormService>().GetInProcessingAmtByMonth(CurrentUser.EmployeeID, model.StartTime);

            CheckOverTimeResponse _checkResult = await HRMApiAdapter.CheckOverTime(
                CurrentUser.CompanyCode, CurrentUser.EmployeeNO, model.StartTime, model.EndTime, true,
                model.CompensationWay == 0, model.HaveDinning, model.HaveDinning, enableSettingEatingTime, (model.CutTime.HasValue == true ? model.CutTime.Value : 0), isCheckDutyCard,
                inProcessingAmt, languageCookie);
            if (!_checkResult.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = _checkResult.Message });
                //Session["OverTimeAmount"] = _checkResult.OvertimeHours;
                //return Json(new AjaxResult() { status = "success", message = string.Format("<div class='dl-info dl-horizontal'><label class='col-md-22'>" + MultiLanguage.Resource.StartingDate + "</label><p class='form-control-static'>{0} ~ {1}</p><label class='col-md-22'>加班時數</label><p class='form-control-static'>{2}小時</p></div>", _checkResult.BeginTime.ToString("yyyy/MM/dd HH:mm"), _checkResult.EndTime.ToString("yyyy/MM/dd HH:mm"), _checkResult.OvertimeHours) });
            }
            else
            {
                Session["OverTimeAmount"] = _checkResult.OvertimeHours;
                return Json(new AjaxResult() { status = "success", message = string.Format("<div class='dl-info dl-horizontal'><label class='col-md-22'>" + MultiLanguage.Resource.StartingDate + "</label><p class='form-control-static'>{0} ~ {1}</p><label class='col-md-22'>" + MultiLanguage.Resource.Label_OvertimeAmount + "</label><p class='form-control-static'>{2}" + MultiLanguage.Resource.Hour + "</p></div>", _checkResult.BeginTime.ToString("yyyy/MM/dd HH:mm"), _checkResult.EndTime.ToString("yyyy/MM/dd HH:mm"), _checkResult.OvertimeHours) });
            }
        }

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
        

        /// <summary>
        /// 取得加班原因列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetOverTimeReasonList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            //20190619 Daniel 移除請選擇選擇，直接預設為第一個原因
            //listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in await HRMApiAdapter.GetOverTimeReasons(CurrentUser.Employee.Company.CompanyCode))
            {
                    listItem.Add(new SelectListItem { Text = item.Name, Value = item.Code, Selected = (selecteddata == item.Code ? true : false) });
              
            }
            return listItem;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletedForm(OverTimeForm model)
        {
            OverTimeForm data = Services.GetService<OverTimeFormService>().GetOverTimeFormByID(model.ID);
            int _result = Services.GetService<OverTimeFormService>().Delete(data, data, CurrentUser.EmployeeID, true);

            //20160201 刪除編輯中的假單時流程中的假單也要同時刪除
            SignFlowRecQueryHelper _queryHelper = new SignFlowRecQueryHelper();
            IList<SignFlowRecModel> _signFlow = _queryHelper.GetSignFlowByFormNumber(model.FormNo);
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
                WriteLog("Success:" + model.ID);
                return Json(new AjaxResult() { status = "success", message = "刪除成功" });
            }
            else
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
        }
    }
}
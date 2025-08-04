using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaCycleFirstEvaluationController : BaseController
    {
        #region 查詢
        public ActionResult Index(string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", string txtStatus = "", int page = 1, string cmd = "")
        {
            GetDefaultData(txtDepartmentID, txtEmployeeNo, txtEmployeeName, txtStatus);

            int currentPage = page < 1 ? 1 : page;

            //  m: 未收件 c:待評核 a:已評核 e: 已送出 r:退回修改
            string[] status = { "m", "c", "a", "e", "r" };

            var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                                    .Where(x => x.IsFirstEvaluation == true && x.IsSecondEvaluation == false && x.PfaCycleEmp.Status != "y");

            if (!CurrentUser.IsAdmin)
                queryData = queryData.Where(x => x.PreSignEmpID == CurrentUser.EmployeeID);

            if (!string.IsNullOrEmpty(txtDepartmentID))
            {
                var departmentID = Guid.Parse(txtDepartmentID);
                queryData = queryData.Where(x => x.PfaCycleEmp.PfaDeptID == departmentID);
            }

            if (!string.IsNullOrEmpty(txtEmployeeNo))
                queryData = queryData.Where(x => x.PfaCycleEmp.Employees.EmployeeNO.Contains(txtEmployeeNo));

            if (!string.IsNullOrEmpty(txtEmployeeName))
                queryData = queryData.Where(x => x.PfaCycleEmp.Employees.EmployeeName.Contains(txtEmployeeName));

            if (!string.IsNullOrEmpty(txtStatus))
                queryData = queryData.Where(x => x.Status == txtStatus);

            var pfaOptionList = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignStatus").ToList();
            var viewModel = queryData.Select(x => new PfaCycleEmpSignViewModel()
            {
                PfaSignProcessID = x.ID,
                PfaCycleID = x.PfaCycleEmp.PfaCycleID,
                PfaDeptID = x.PfaCycleEmp.PfaDeptID,
                PfaDeptCode = x.PfaCycleEmp.PfaDept.PfaDeptCode,
                PfaDeptName = x.PfaCycleEmp.PfaDept.PfaDeptName,
                EmployeeID = x.PfaCycleEmpID,
                EmployeeNo = x.PfaCycleEmp.Employees.EmployeeNO,
                EmployeeName = x.PfaCycleEmp.Employees.EmployeeName,
                PfaSelfScore = x.PfaCycleEmp.PfaSelfScore,
                PfaFirstScore = x.PfaCycleEmp.PfaFirstScore,
                FirstPerformance_ID = x.PfaCycleEmp.FirstPerformance_ID,
                SignStatus = x.Status,
            }).OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.EmployeeNo).ToList().ToPagedList(currentPage, currentPageSize);

            var pfaCycleIDs = viewModel.Select(x => x.PfaCycleID).Distinct().ToList();
            var pfaCycleList = Services.GetService<PfaCycleService>().GetAll().Where(x => pfaCycleIDs.Contains(x.ID)).ToList();
            var pfaPerformanceList = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID).ToList();
            foreach (var item in viewModel)
            {
                var pfaCycle = pfaCycleList.FirstOrDefault(y => y.ID == item.PfaCycleID);
                var pfaOption = pfaOptionList.FirstOrDefault(y => y.OptionCode == item.SignStatus);
                item.PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "";
                item.StrSignStatus = pfaOption != null ? pfaOption.OptionName : "";
                if (item.FirstPerformance_ID.HasValue)
                {
                    var pfaPerformance = pfaPerformanceList.FirstOrDefault(x => x.ID == item.FirstPerformance_ID.Value);
                    if (pfaPerformance != null)
                        item.FirstPerformanceName = pfaPerformance.Name;
                }
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtDepartmentID, string txtEmployeeNo, string txtEmployeeName, string txtStatus, string btnQuery, string btnClear)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                return RedirectToAction("Index", new
                {
                    page = 1,
                    txtDepartmentID,
                    txtEmployeeNo,
                    txtEmployeeName,
                    txtStatus,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtDepartmentID, txtEmployeeNo, txtEmployeeName, txtStatus);

            return View();
        }
        #endregion

        #region 編輯
        public ActionResult Edit(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_Edit");

            var pfaSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.ID == Id.Value);
            if (pfaSignProcess == null)
                return PartialView("_Edit");

            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(pfaSignProcess.PfaCycleEmp.PfaCycleID);
            if (pfaCycle == null)
                return PartialView("_Edit");

            var model = GetDetail(pfaSignProcess, pfaCycle);

            return PartialView("_Edit", model);
        }

        [HttpPost]
        public ActionResult Edit(PfaCycleEmpSignViewModel model, string cmd)
        {
            var result = new Result();

            try
            {
                var checkResult = CheckPfaCycleEmpSign(model, cmd);
                if (!checkResult.success)
                    throw new Exception(checkResult.message);

                var pfaSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.ID == model.PfaSignProcessID);
                if (pfaSignProcess == null)
                    throw new Exception("績效考核簽核資料取得失敗");

                var pfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x => x.ID == model.PfaCycleEmpID);
                if (pfaCycleEmp == null)
                    throw new Exception("績效考核員工資料取得失敗");

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(pfaCycleEmp.PfaCycleID);
                if (pfaCycle == null)
                    throw new Exception("績效考核資料取得失敗");

                pfaCycleEmp.ManagerIndicator = model.ManagerIndicator == 0 ? null : model.ManagerIndicator;
                pfaCycleEmp.ManagerAbility = model.ManagerAbility == 0 ? null : model.ManagerAbility;
                pfaCycleEmp.PfaFirstScore = model.PfaFirstScore == 0 ? null : model.PfaFirstScore;
                pfaCycleEmp.FirstPerformance_ID = model.PfaFirstScore == 0 ? null : model.FirstPerformance_ID;
                pfaCycleEmp.FirstAppraisal = model.FirstAppraisal;
                pfaCycleEmp.PastPerformance = model.PastPerformance;
                pfaCycleEmp.NowPerformance = model.NowPerformance;
                pfaCycleEmp.Development = model.Development;
                pfaCycleEmp.ModifiedBy = CurrentUser.EmployeeID;
                pfaCycleEmp.ModifiedTime = DateTime.Now;

                #region 績效考核員工工作績效
                var pfaEmpIndicatorList = model.PfaEmpIndicator.Select(x => new PfaEmpIndicator
                {
                    ID = x.ID.HasValue ? x.ID.Value : Guid.Empty,
                    PfaCycleEmpID = pfaCycleEmp.ID,
                    PfaIndicatorCode = x.PfaIndicatorCode,
                    PfaIndicatorName = x.PfaIndicatorName,
                    Description = x.Description,
                    Scale = x.Scale,
                    Ordering = x.Ordering,
                    PfaIndicatorDesc = x.PfaIndicatorDesc,
                    SelfIndicator = x.SelfIndicator,
                    ManagerIndicator = x.ManagerIndicator,
                    CreatedBy = CurrentUser.EmployeeID,
                    CreatedTime = DateTime.Now,
                    ModifiedBy = CurrentUser.EmployeeID,
                    ModifiedTime = DateTime.Now
                }).ToList();
                #endregion

                #region 績效考核員工勝任能力
                var pfaEmpAbilityList = new List<PfaEmpAbility>();
                var pfaAbilityList = Services.GetService<PfaAbilityService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true && x.PfaEmpTypeID == pfaSignProcess.PfaCycleEmp.PfaEmpTypeID).OrderBy(x => x.Ordering).ToList();
                foreach (var pfaAbility in pfaAbilityList)
                {
                    var tempPfaEmpAbility = model.PfaEmpAbility.FirstOrDefault(x => x.PfaAbilityCode == pfaAbility.PfaAbilityCode);
                    var pfaEmpAbility = new PfaEmpAbility
                    {
                        PfaCycleEmpID = pfaCycleEmp.ID,
                        PfaAbilityCode = pfaAbility.PfaAbilityCode,
                        PfaAbilityName = pfaAbility.PfaAbilityName,
                        Description = pfaAbility.Description,
                        TotalScore = pfaAbility.TotalScore,
                        Ordering = pfaAbility.Ordering,
                        CreatedBy = CurrentUser.EmployeeID,
                        CreatedTime = DateTime.Now,
                        ModifiedBy = CurrentUser.EmployeeID,
                        ModifiedTime = DateTime.Now
                    };

                    if (tempPfaEmpAbility != null)
                    {
                        var sumAbilityScore = tempPfaEmpAbility.PfaEmpAbilityDetail.Sum(x => x.AbilityScore);
                        if (sumAbilityScore.HasValue)
                        {
                            var chkManagerAbility = Math.Round(sumAbilityScore.Value / tempPfaEmpAbility.PfaEmpAbilityDetail.Count(), 1, MidpointRounding.AwayFromZero);
                            pfaEmpAbility.ID = tempPfaEmpAbility.ID.HasValue ? tempPfaEmpAbility.ID.Value : Guid.NewGuid();
                            if (tempPfaEmpAbility.ManagerAbility.HasValue)
                            {
                                if (chkManagerAbility != tempPfaEmpAbility.ManagerAbility.Value)
                                    tempPfaEmpAbility.ManagerAbility = chkManagerAbility;
                                pfaEmpAbility.ManagerAbility = tempPfaEmpAbility.ManagerAbility.Value;
                            }
                        }
                    }
                    else
                        pfaEmpAbility.ID = Guid.NewGuid();

                    var pfaAbilityDetailList = pfaAbility.PfaAbilityDetail.OrderBy(x => x.Ordering).ToList();
                    foreach (var pfaAbilityDetail in pfaAbilityDetailList)
                    {
                        PfaEmpAbilityDetailViewModel tempPfaEmpAbilityDetail = null;
                        if (tempPfaEmpAbility.PfaEmpAbilityDetail.Any())
                            tempPfaEmpAbilityDetail = tempPfaEmpAbility.PfaEmpAbilityDetail.FirstOrDefault(x => x.Ordering == pfaAbilityDetail.Ordering);

                        var pfaEmpAbilityDetail = new PfaEmpAbilityDetail
                        {
                            PfaEmpAbilityID = pfaEmpAbility.ID,
                            PfaAbilityKey = pfaAbilityDetail.PfaAbilityKey,
                            UpperLimit = pfaAbilityDetail.UpperLimit,
                            LowerLimit = pfaAbilityDetail.LowerLimit,
                            Ordering = pfaAbilityDetail.Ordering,
                            CreatedBy = CurrentUser.EmployeeID,
                            CreatedTime = DateTime.Now,
                            ModifiedBy = CurrentUser.EmployeeID,
                            ModifiedTime = DateTime.Now
                        };
                        if (tempPfaEmpAbilityDetail != null)
                        {
                            pfaEmpAbilityDetail.ID = tempPfaEmpAbilityDetail.ID.HasValue ? tempPfaEmpAbilityDetail.ID.Value : Guid.NewGuid();
                            if (tempPfaEmpAbilityDetail.AbilityScore.HasValue)
                                pfaEmpAbilityDetail.AbilityScore = tempPfaEmpAbilityDetail.AbilityScore.Value;
                        }
                        else
                            pfaEmpAbilityDetail.ID = Guid.NewGuid();

                        pfaEmpAbility.PfaEmpAbilityDetail.Add(pfaEmpAbilityDetail);
                    }
                    pfaEmpAbilityList.Add(pfaEmpAbility);
                }
                #endregion

                pfaSignProcess.Status = model.SignStatus;
                pfaSignProcess.Assessment = model.Assessment;
                result = Services.GetService<PfaSignProcessService>().UpdateFirstEvaluationData(pfaSignProcess, pfaCycleEmp, pfaEmpIndicatorList, pfaEmpAbilityList, cmd);
                if (!result.success)
                    throw new Exception(result.message);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = result.success, message = result.message });
        }

        /// <summary>
        /// 檢核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private Result CheckPfaCycleEmpSign(PfaCycleEmpSignViewModel model, string cmd)
        {
            var result = new Result { success = true };

            try
            {
                if (model.PfaSignProcessID == null || model.PfaCycleEmpID == Guid.Empty)
                    throw new Exception("績效考核編輯失敗");

                if (model.PfaEmpIndicator == null)
                    throw new Exception("績效考核查無工作績效評核資料");

                if (!model.PfaEmpIndicator.Any())
                    throw new Exception("績效考核查無工作績效評核資料");

                if (model.PfaEmpAbility == null)
                    throw new Exception("績效考核查無員工勝任能力資料");

                if (!model.PfaEmpAbility.Any())
                    throw new Exception("績效考核查無員工勝任能力資料");

                if (cmd == "btnReject")
                    model.SignStatus = "b"; // b:已退回
                else
                    model.SignStatus = (model.SignStatus == "c" || model.SignStatus == "r") ? "a" : model.SignStatus; // c:待評核 a:已評核

                if (cmd == "btnReject" && string.IsNullOrWhiteSpace(model.Assessment))
                    throw new Exception("請於簽核意見輸入退回原因");

                string pattern = @"^[0-9]+(.[0-9]{1})?$";

                var pfaFirstScore = (double)0;
                #region 績效考核員工工作績效
                foreach (var pfaEmpIndicator in model.PfaEmpIndicator)
                {
                    if (pfaEmpIndicator.ManagerIndicator.HasValue)
                    {
                        if (pfaEmpIndicator.ManagerIndicator < 0 || pfaEmpIndicator.ManagerIndicator > pfaEmpIndicator.Scale)
                            throw new Exception(string.Format("{0}請輸入0-{1}區間的主管評分數，小數點最多1位", pfaEmpIndicator.PfaIndicatorName, pfaEmpIndicator.Scale));
                        if (!Regex.IsMatch(pfaEmpIndicator.ManagerIndicator.ToString(), pattern))
                            throw new Exception(string.Format("{0}請輸入0-{1}區間的主管評分數，小數點最多1位", pfaEmpIndicator.PfaIndicatorName, pfaEmpIndicator.Scale));
                    }
                    else
                    {
                        if (cmd == "btnSave")
                            throw new Exception(string.Format("請輸入{0}主管評分數", pfaEmpIndicator.PfaIndicatorName));
                    }
                }
                model.ManagerIndicator = model.PfaEmpIndicator.Where(x => x.ManagerIndicator.HasValue).Sum(x => x.ManagerIndicator);
                if (model.ManagerIndicator.HasValue)
                    pfaFirstScore = pfaFirstScore + model.ManagerIndicator.Value;
                #endregion

                #region 績效考核員工勝任能力
                foreach (var pfaEmpAbility in model.PfaEmpAbility)
                {
                    if (cmd == "btnSave")
                    {
                        var checkPfaEmpAbility = pfaEmpAbility.PfaEmpAbilityDetail.OrderBy(x => x.Ordering).FirstOrDefault(x => !x.AbilityScore.HasValue);
                        if (checkPfaEmpAbility != null)
                            throw new Exception(string.Format("請選擇{0}-{1}能力符合程度評核", pfaEmpAbility.PfaAbilityName, checkPfaEmpAbility.PfaAbilityKey));
                    }
                }
                model.ManagerAbility = model.PfaEmpAbility.Where(x => x.ManagerAbility.HasValue).Sum(x => x.ManagerAbility);
                if (model.ManagerAbility.HasValue)
                    pfaFirstScore = pfaFirstScore + model.ManagerAbility.Value;
                #endregion

                model.PfaFirstScore = pfaFirstScore;
                var tempPfaFirstScore = (decimal)model.PfaFirstScore;
                var pfaPerformance = Services.GetService<PfaPerformanceService>().GetAll().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed == true &&
                                                                                                               x.ScoresStart <= tempPfaFirstScore && x.ScoresEnd >= tempPfaFirstScore);
                if (pfaPerformance != null)
                    model.FirstPerformance_ID = pfaPerformance.ID;
                else
                {
                    if (cmd == "btnSave")
                        throw new Exception(string.Format("初核總分:{0}，查無績效等第資料", model.PfaFirstScore));
                }

                if (cmd == "btnSave")
                {
                    if (string.IsNullOrWhiteSpace(model.FirstAppraisal))
                        throw new Exception("請輸入主管綜合考評");

                    if (!model.PastPerformance.HasValue)
                        throw new Exception("請選擇與過去表現比較");

                    if (!model.NowPerformance.HasValue)
                        throw new Exception("請選擇評價目前工作");

                    if (!model.Development.HasValue)
                        throw new Exception("請選擇未來發展評斷");
                }
            }
            catch (Exception ex)
            {
                result.success = false;
                result.message = ex.Message;
            }
            return result;
        }
        #endregion

        #region 送出
        public ActionResult SentQuery(Guid? Id = null)
        {
            var ds = new PfaCycleSentDataViewModel
            {
                PfaFormNo = "",
                PfaYear = "",
                Remark = "",
                PfaFormNoList = new List<SelectListItem>(),
                PfaCycleDeptSent = new List<PfaCycleSentViewModel>(),
            };
            var now = DateTime.Now.Date;
            string[] status = { PfaSignProcess_Status.Reviewed
                    , PfaSignProcess_Status.Submitted  }; //  m: 未收件 c:待評核 a:已評核 e: 已送出 r:退回修改

            var pfaCycleList = Services.GetService<PfaCycleService>().GetAll()
                    .Where(x =>
                        x.CompanyID == CurrentUser.CompanyID
                        && x.Status == PfaCycle_Status.InApprovalProcess
                        ).ToList()
                        .OrderByDescending(x => x.PfaYear)
                        .ToList();

            PfaCycle pfaCycle = null;
            var pfaCycleId = "";
            if (Id.HasValue)
            {
                pfaCycleId = Id.Value.ToString();
                pfaCycle = pfaCycleList.FirstOrDefault(x => x.ID == Id.Value);
            }
            else
            {
                pfaCycleId = pfaCycleList.FirstOrDefault().ID.ToString();
                pfaCycle = pfaCycleList.FirstOrDefault();
            }

            try
            {
                if (pfaCycle == null)
                    throw new Exception("無績效考核簽核批號資料");

                var pfaDeptIDs = Services.GetService<PfaSignProcessService>().GetAll()
                    .Where(x => x.PfaCycleEmp.IsRatio 
                        && x.PfaCycleEmp.PfaCycleID == pfaCycle.ID 
                        && x.IsRatio 
                        && x.IsFirstEvaluation == true 
                        && x.IsSecondEvaluation == false 
                        && x.PreSignEmpID == CurrentUser.EmployeeID
                        )
                    .Select(x => x.PfaCycleEmp.PfaDeptID)
                    .ToList();

                var queryPfaDeptData = Services.GetService<PfaDeptService>().GetAll()
                    .Where(x=> pfaDeptIDs.Contains(x.ID) 
                    && x.CompanyID == CurrentUser.CompanyID 
                    && x.BeginDate <= now 
                    && x.EndDate >= now).ToList();

                // 該主管所有可評核部門
                var pfaCycleDeptSentList = queryPfaDeptData.Select(x => new PfaCycleSentViewModel
                {
                    PfaDeptID = x.ID,
                    PfaDeptCode = x.PfaDeptCode,
                    PfaDeptName = x.PfaDeptName,
                }).OrderBy(x => x.PfaDeptCode).ToList();

                // 由部門取得簽核中績效考核員工資料
                var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll()
                    .Where(x => pfaDeptIDs.Contains(x.PfaDeptID) 
                        && x.IsRatio 
                        && x.Status == "a")
                    .ToList();

                if (pfaCycleEmpList.Any())
                {
                    ds.PfaFormNoList = GetPfaFormNoList(pfaCycleList, pfaCycleId);
                    ds.PfaCycleID = pfaCycle.ID;
                    ds.PfaFormNo = pfaCycle.PfaFormNo;
                    ds.PfaYear = pfaCycle.PfaYear;
                    ds.Remark = pfaCycle.Desription;
                    foreach (var pfaCycleDeptSent in pfaCycleDeptSentList)
                    {
                        var pfaSignProcessList = Services.GetService<PfaSignProcessService>().GetAll()
                            .Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycle.ID 
                                && x.PfaCycleEmp.PfaDeptID == pfaCycleDeptSent.PfaDeptID 
                                && x.PreSignEmpID == CurrentUser.EmployeeID 
                                && x.IsRatio 
                                && x.IsFirstEvaluation == true 
                                && x.IsSecondEvaluation == false)
                            .ToList();

                        // 應評核人數
                        pfaCycleDeptSent.FirstAll = pfaSignProcessList.Count();

                        // 已評核人數
                        pfaCycleDeptSent.FirstFinal = pfaSignProcessList
                            .Where(x => status.Contains(x.Status))
                            .Select(x => x.PfaCycleEmpID)
                            .Distinct()
                            .Count();

                        // 狀態
                        if (pfaCycleDeptSent.FirstAll == pfaCycleDeptSent.FirstFinal)
                        {
                            if (pfaSignProcessList.Any(x => x.Status == "a"))
                                pfaCycleDeptSent.SignStatus = pfaCycleDeptSent.SignStatus_N;
                            else
                                pfaCycleDeptSent.SignStatus = pfaCycleDeptSent.SignStatus_Y;
                        }
                        else
                        {
                            pfaCycleDeptSent.SignStatus = pfaCycleDeptSent.SignStatus_N;
                        }
                    }
                    pfaCycleDeptSentList = pfaCycleDeptSentList.Where(x => x.FirstAll > 0).ToList();
                    ds.PfaCycleDeptSent = pfaCycleDeptSentList;
                }
                if (!ds.PfaCycleDeptSent.Any(x => x.SignStatus == "N"))
                    ViewBag.Status = "Y";
                else
                    ViewBag.Status = "N";
            }
            catch (Exception)
            {
                ViewBag.Status = "Y";
                ds = new PfaCycleSentDataViewModel
                {
                    PfaFormNo = "",
                    PfaYear = "",
                    Remark = "",
                    PfaFormNoList = new List<SelectListItem>(),
                    PfaCycleDeptSent = new List<PfaCycleSentViewModel>(),
                };
            }

            if (ds.PfaFormNoList.Any() == false)
            {
                ds = new PfaCycleSentDataViewModel
                {
                    PfaFormNo = "",
                    PfaYear = "",
                    Remark = "",
                    PfaFormNoList = GetPfaFormNoList(pfaCycleList, pfaCycleId),
                    PfaCycleDeptSent = new List<PfaCycleSentViewModel>(),
                };
            }
                

            return View("_Sent", ds);
        }

        [HttpPost]
        public ActionResult SentConfirm(PfaCycleSentDataViewModel model)
        {
            var result = new Result();

            try
            {
                if (model.PfaCycleDeptSent.Count() == 0)
                {
                    WriteLog("請選擇要送出的部門");
                    return Json(new { success = false, message = "請選擇要送出的部門" });
                }

                var isExist = Services.GetService<PfaCycleService>().IsExist(model.PfaCycleID);
                if (!isExist)
                {
                    WriteLog(string.Format("績效考核批號資料不存在,ID:{0}", model.PfaCycleID));
                    return Json(new { success = false, message = "績效考核批號資料不存在" });
                }

                var newModel = model.PfaCycleDeptSent.Select(x => new PfaCycleEmp
                {
                    PfaCycleID = model.PfaCycleID,
                    PfaDeptID = x.PfaDeptID
                }).ToList();

                result = Services.GetService<PfaSignProcessService>().SentFirstEvaluationData(newModel, CurrentUser.EmployeeID);
                if (!result.success)
                    throw new Exception(result.message);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 明細
        public ActionResult Detail(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_Detail");

            var pfaSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.ID == Id.Value);
            if (pfaSignProcess == null)
                return PartialView("_Detail");

            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(pfaSignProcess.PfaCycleEmp.PfaCycleID);
            if (pfaCycle == null)
                return PartialView("_Detail");

            var model = GetDetail(pfaSignProcess, pfaCycle);

            return PartialView("_Detail", model);
        }

        /// <summary>
        /// 取得明細
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycle"></param>
        /// <returns></returns>
        private PfaCycleEmpSignViewModel GetDetail(PfaSignProcess pfaSignProcess, PfaCycle pfaCycle)
        {
            var hire = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Hire").FirstOrDefault(x => x.ID == pfaSignProcess.PfaCycleEmp.HireID);
            var jobTitle = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").FirstOrDefault(x => x.ID == pfaSignProcess.PfaCycleEmp.JobTitleID);
            var jobFunction = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobFunction").FirstOrDefault(x => x.ID == pfaSignProcess.PfaCycleEmp.JobFunctionID);
            var grade = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Grade").FirstOrDefault(x => x.ID == pfaSignProcess.PfaCycleEmp.GradeID);

            var model = new PfaCycleEmpSignViewModel
            {
                PfaSignProcessID = pfaSignProcess.ID,
                PfaCycleID = pfaSignProcess.PfaCycleEmp.PfaCycleID,
                PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "",
                DutyBeginEndDate = pfaCycle != null ? string.Format("統計區間:{0}至{1}止", pfaCycle.DutyBeginDate.HasValue ? pfaCycle.DutyBeginDate.Value.ToString("yyyy/MM/dd") : "", pfaCycle.DutyEndDate.HasValue ? pfaCycle.DutyEndDate.Value.ToString("yyyy/MM/dd") : "") : "",
                PfaCycleEmpID = pfaSignProcess.PfaCycleEmp.ID,
                PfaDeptID = pfaSignProcess.PfaCycleEmp.PfaDeptID,
                PfaDeptCode = pfaSignProcess.PfaCycleEmp.PfaDept.PfaDeptCode,
                PfaDeptName = pfaSignProcess.PfaCycleEmp.PfaDept.PfaDeptName,
                PfaOrgID = pfaSignProcess.PfaCycleEmp.PfaOrgID,
                HireID = pfaSignProcess.PfaCycleEmp.HireID,
                HireCode = hire != null ? hire.OptionCode : "",
                HireName = hire != null ? hire.OptionName : "",
                JobTitleID = pfaSignProcess.PfaCycleEmp.JobTitleID,
                JobTitleCode = jobTitle != null ? jobTitle.OptionCode : "",
                JobTitleName = jobTitle != null ? jobTitle.OptionName : "",
                JobFunctionID = pfaSignProcess.PfaCycleEmp.JobFunctionID,
                JobFunctionCode = jobFunction != null ? jobFunction.OptionCode : "",
                JobFunctionName = jobFunction != null ? jobFunction.OptionName : "",
                GradeID = pfaSignProcess.PfaCycleEmp.GradeID,
                GradeCode = grade != null ? grade.OptionCode : "",
                GradeName = grade != null ? grade.OptionName : "",
                EmployeeID = pfaSignProcess.PfaCycleEmpID,
                EmployeeNo = pfaSignProcess.PfaCycleEmp.Employees.EmployeeNO,
                EmployeeName = pfaSignProcess.PfaCycleEmp.Employees.EmployeeName,
                SignStatus = pfaSignProcess.Status,
                ArriveDate = pfaSignProcess.PfaCycleEmp.Employees.ArriveDate.ToString("yyyy/MM/dd"),
                Education = pfaSignProcess.PfaCycleEmp.Education,
                SchoolName = pfaSignProcess.PfaCycleEmp.SchoolName,
                DeptDescription = pfaSignProcess.PfaCycleEmp.DeptDescription,
                PersonalLeave = pfaSignProcess.PfaCycleEmp.PersonalLeave,
                SickLeave = pfaSignProcess.PfaCycleEmp.SickLeave,
                LateLE = pfaSignProcess.PfaCycleEmp.LateLE,
                AWL = pfaSignProcess.PfaCycleEmp.AWL,
                Salary01 = pfaSignProcess.PfaCycleEmp.Salary01,
                Salary02 = pfaSignProcess.PfaCycleEmp.Salary02,
                Salary03 = pfaSignProcess.PfaCycleEmp.Salary03,
                Salary04 = pfaSignProcess.PfaCycleEmp.Salary04,
                Salary05 = pfaSignProcess.PfaCycleEmp.Salary05,
                Salary06 = pfaSignProcess.PfaCycleEmp.Salary06,
                FullSalary = pfaSignProcess.PfaCycleEmp.FullSalary,
                FirstAppraisal = pfaSignProcess.PfaCycleEmp.FirstAppraisal,
                PastPerformance = pfaSignProcess.PfaCycleEmp.PastPerformance,
                NowPerformance = pfaSignProcess.PfaCycleEmp.NowPerformance,
                Development = pfaSignProcess.PfaCycleEmp.Development,
                LastAppraisal = pfaSignProcess.PfaCycleEmp.LastAppraisal,
                SelfAppraisal = pfaSignProcess.PfaCycleEmp.SelfAppraisal,
                PfaFirstScore = pfaSignProcess.PfaCycleEmp.PfaFirstScore,
                ManagerAbility = pfaSignProcess.PfaCycleEmp.ManagerAbility,
                ManagerIndicator = pfaSignProcess.PfaCycleEmp.ManagerIndicator,
                PfaSelfScore = pfaSignProcess.PfaCycleEmp.PfaSelfScore,
                SelfIndicator = pfaSignProcess.PfaCycleEmp.SelfIndicator,
            };

            if (model.PfaOrgID.HasValue)
                model.PfaOrgName = pfaSignProcess.PfaCycleEmp.PfaOrg.PfaOrgName;

            //if (pfaSignProcess.Status == "r") // r:退回修改
            //{
            //    var pfaBackSignProcess = Services.GetService<PfaSignRecordService>().GetAll().Where(x => x.PfaCycleEmpID == pfaSignProcess.PfaCycleEmpID && x.Status == "b").OrderByDescending(x=> x.CreatedTime).FirstOrDefault();
            //    if (pfaBackSignProcess != null)
            //        model.Assessment = pfaBackSignProcess.Assessment;
            //}

            var performance = new StringBuilder();
            performance.AppendLine(pfaSignProcess.PfaCycleEmp.Performance1);
            performance.AppendLine(pfaSignProcess.PfaCycleEmp.Performance2);
            performance.AppendLine(pfaSignProcess.PfaCycleEmp.Performance3);
            model.StrPerformance = performance.ToString();

            #region 績效考核員工工作績效
            if (pfaSignProcess.PfaCycleEmp.PfaEmpIndicator.Any())
            {
                model.PfaEmpIndicator = pfaSignProcess.PfaCycleEmp.PfaEmpIndicator.Select(x => new PfaEmpIndicatorViewModel
                {
                    ID = x.ID,
                    PfaCycleEmpID = x.PfaCycleEmpID,
                    PfaIndicatorCode = x.PfaIndicatorCode,
                    PfaIndicatorName = x.PfaIndicatorName,
                    Description = x.Description,
                    Scale = x.Scale,
                    Ordering = x.Ordering,
                    PfaIndicatorDesc = x.PfaIndicatorDesc,
                    SelfIndicator = x.SelfIndicator,
                    ManagerIndicator = x.ManagerIndicator,
                }).OrderBy(x => x.Ordering).ToList();
            }
            else
            {
                model.PfaEmpIndicator = new List<PfaEmpIndicatorViewModel>();
                var pfaIndicatorDesc = new StringBuilder();
                var pfaIndicatorList = Services.GetService<PfaIndicatorService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true).OrderBy(x => x.Ordering).ToList();
                foreach (var pfaIndicator in pfaIndicatorList)
                {
                    pfaIndicatorDesc.Length = 0;
                    var pfaIndicatorDetailList = pfaIndicator.PfaIndicatorDetail.OrderBy(x => x.Ordering).ToList();
                    foreach (var pfaIndicatorDetail in pfaIndicatorDetailList)
                        pfaIndicatorDesc.AppendLine(string.Format("{0}.{1} {2}-{3}", pfaIndicatorDetail.Ordering, pfaIndicatorDetail.PfaIndicatorKey, pfaIndicatorDetail.UpperLimit, pfaIndicatorDetail.LowerLimit));
                    model.PfaEmpIndicator.Add(new PfaEmpIndicatorViewModel
                    {
                        PfaCycleEmpID = pfaSignProcess.PfaCycleEmp.ID,
                        PfaIndicatorCode = pfaIndicator.PfaIndicatorCode,
                        PfaIndicatorName = pfaIndicator.PfaIndicatorName,
                        Description = pfaIndicator.Description,
                        Scale = pfaIndicator.Scale,
                        Ordering = pfaIndicator.Ordering,
                        PfaIndicatorDesc = pfaIndicatorDesc.ToString(),
                    });
                }
            }
            #endregion

            #region 績效考核員工訓練紀錄
            model.PfaEmpTraining = new List<PfaEmpTrainingViewModel>();
            if (pfaSignProcess.PfaCycleEmp.PfaEmpTraining.Any())
            {
                foreach (var pfaEmpTraining in pfaSignProcess.PfaCycleEmp.PfaEmpTraining)
                {
                    var tempPfaEmpTraining = new PfaEmpTrainingViewModel
                    {
                        ID = pfaEmpTraining.ID,
                        PfaCycleEmpID = pfaEmpTraining.PfaCycleEmpID,
                        CoursesCode = pfaEmpTraining.CoursesCode,
                        CoursesName = pfaEmpTraining.CoursesName,
                    };
                    if (pfaEmpTraining.TrainingHours.HasValue)
                        tempPfaEmpTraining.TrainingHours = (double)pfaEmpTraining.TrainingHours.Value;
                    model.PfaEmpTraining.Add(tempPfaEmpTraining);
                }
            }
            #endregion

            # region 績效考核簽核附件資料
            model.PfaSignUploadData = new PfaSignUploadDataViewModel
            {
                IsUpload = pfaSignProcess.IsUpload
            };

            var filename = "";
            model.PfaSignUploadData.PfaSignUpload = Services.GetService<PfaSignUploadService>().GetAll()
                                                                             .Where(x => x.PfaCycleEmpID == pfaSignProcess.PfaCycleEmp.ID).OrderBy(x => x.Ordering)
                                                                             .ToList().Select(x =>
                                                                             {
                                                                                 filename = "";
                                                                                 if (!string.IsNullOrWhiteSpace(x.Href))
                                                                                 {
                                                                                     filename = Path.GetFileName(x.Href);
                                                                                     if (!string.IsNullOrWhiteSpace(filename))
                                                                                         filename = filename.Split('.')[0];
                                                                                 }

                                                                                 return new PfaSignUploadViewModel
                                                                                 {
                                                                                     ID = x.ID,
                                                                                     PfaCycleEmpID = x.PfaCycleEmpID,
                                                                                     PfaSignProcessID = x.PfaCycleEmpID,
                                                                                     FileName = filename,
                                                                                     Href = x.Href,
                                                                                     Ordering = x.Ordering,
                                                                                 };
                                                                             }).ToList();
            model.SignUploadCount = model.PfaSignUploadData.PfaSignUpload.Count();
            #endregion

            #region 績效考核員工勝任能力
            model.PfaEmpAbility = new List<PfaEmpAbilityViewModel>(); 
            if (pfaSignProcess.PfaCycleEmp.PfaEmpAbility.Any())
            {
                var pfaEmpAbilityList = pfaSignProcess.PfaCycleEmp.PfaEmpAbility.OrderBy(x=> x.Ordering).ToList();
                foreach (var pfaEmpAbility in pfaEmpAbilityList)
                {
                    var tempPfaEmpAbility = new PfaEmpAbilityViewModel
                    {
                        ID = pfaEmpAbility.ID,
                        PfaCycleEmpID = pfaEmpAbility.PfaCycleEmpID,
                        PfaAbilityCode = pfaEmpAbility.PfaAbilityCode,
                        PfaAbilityName = pfaEmpAbility.PfaAbilityName,
                        Description = pfaEmpAbility.Description,
                        TotalScore = pfaEmpAbility.TotalScore,
                        Ordering = pfaEmpAbility.Ordering,
                        ManagerAbility = pfaEmpAbility.ManagerAbility,
                        PfaEmpAbilityDetail = new List<PfaEmpAbilityDetailViewModel>(),
                    };
                    var pfaEmpAbilityDetailList = pfaEmpAbility.PfaEmpAbilityDetail.OrderBy(x => x.Ordering).ToList();
                    foreach (var pfaEmpAbilityDetail in pfaEmpAbilityDetailList)
                    {
                        var tempPfaEmpAbilityDetail = new PfaEmpAbilityDetailViewModel
                        {
                            ID = pfaEmpAbilityDetail.ID,
                            PfaEmpAbilityID = pfaEmpAbilityDetail.PfaEmpAbilityID,
                            PfaAbilityKey = string.Format("{0}.{1}", pfaEmpAbilityDetail.Ordering, pfaEmpAbilityDetail.PfaAbilityKey),
                            UpperLimit = pfaEmpAbilityDetail.UpperLimit,
                            LowerLimit = pfaEmpAbilityDetail.LowerLimit,
                            Ordering = pfaEmpAbilityDetail.Ordering,
                            AbilityScore = pfaEmpAbilityDetail.AbilityScore.HasValue ? pfaEmpAbilityDetail.AbilityScore.Value : 0,
                            ScoreInterval = new List<int>(),
                        };
                        if (tempPfaEmpAbilityDetail.UpperLimit.HasValue && tempPfaEmpAbilityDetail.LowerLimit.HasValue)
                        {
                            for (int i = tempPfaEmpAbilityDetail.LowerLimit.Value; i <= tempPfaEmpAbilityDetail.UpperLimit.Value; i++)
                                tempPfaEmpAbilityDetail.ScoreInterval.Add(i);
                        }
                        else if (tempPfaEmpAbilityDetail.UpperLimit.HasValue)
                        {
                            for (int i = 0; i <= tempPfaEmpAbilityDetail.UpperLimit.Value; i++)
                                tempPfaEmpAbilityDetail.ScoreInterval.Add(i);
                        }
                        else if (tempPfaEmpAbilityDetail.LowerLimit.HasValue)
                        {
                            if (tempPfaEmpAbility.TotalScore.HasValue && tempPfaEmpAbility.TotalScore.Value >= tempPfaEmpAbilityDetail.LowerLimit.Value)
                            {
                                for (int i = tempPfaEmpAbilityDetail.LowerLimit.Value; i <= tempPfaEmpAbility.TotalScore.Value; i++)
                                    tempPfaEmpAbilityDetail.ScoreInterval.Add(i);
                            }
                            else
                            {
                                tempPfaEmpAbilityDetail.ScoreInterval.Add(tempPfaEmpAbilityDetail.LowerLimit.Value);
                            }
                        }
                        tempPfaEmpAbility.PfaEmpAbilityDetail.Add(tempPfaEmpAbilityDetail);
                    }
                    model.PfaEmpAbility.Add(tempPfaEmpAbility);
                }
            }
            else
            {
                var pfaAbilityList = Services.GetService<PfaAbilityService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true && x.PfaEmpTypeID == pfaSignProcess.PfaCycleEmp.PfaEmpTypeID).OrderBy(x => x.Ordering).ToList();
                foreach (var pfaAbility in pfaAbilityList)
                {
                    var tempPfaEmpAbility = new PfaEmpAbilityViewModel
                    {
                        PfaAbilityCode = pfaAbility.PfaAbilityCode,
                        PfaAbilityName = pfaAbility.PfaAbilityName,
                        Description = pfaAbility.Description,
                        TotalScore = pfaAbility.TotalScore,
                        Ordering = pfaAbility.Ordering,
                        PfaEmpAbilityDetail = new List<PfaEmpAbilityDetailViewModel>(),
                    };
                    var pfaAbilityDetailList = pfaAbility.PfaAbilityDetail.OrderBy(x => x.Ordering).ToList();
                    foreach (var pfaAbilityDetail in pfaAbilityDetailList)
                    {
                        var tempPfaEmpAbilityDetail = new PfaEmpAbilityDetailViewModel
                        {
                            PfaAbilityKey = string.Format("{0}.{1}", pfaAbilityDetail.Ordering, pfaAbilityDetail.PfaAbilityKey),
                            UpperLimit = pfaAbilityDetail.UpperLimit,
                            LowerLimit = pfaAbilityDetail.LowerLimit,
                            Ordering = pfaAbilityDetail.Ordering,
                            AbilityScore = 0,
                            ScoreInterval = new List<int>(),
                        };
                        if (tempPfaEmpAbilityDetail.UpperLimit.HasValue && tempPfaEmpAbilityDetail.LowerLimit.HasValue)
                        {
                            for (int i = tempPfaEmpAbilityDetail.LowerLimit.Value; i <= tempPfaEmpAbilityDetail.UpperLimit.Value; i++)
                                tempPfaEmpAbilityDetail.ScoreInterval.Add(i);
                        }
                        else if (tempPfaEmpAbilityDetail.UpperLimit.HasValue)
                        {
                            for (int i = 0; i <= tempPfaEmpAbilityDetail.UpperLimit.Value; i++)
                                tempPfaEmpAbilityDetail.ScoreInterval.Add(i);
                        }
                        else if (tempPfaEmpAbilityDetail.LowerLimit.HasValue)
                        {
                            if (tempPfaEmpAbility.TotalScore.HasValue && tempPfaEmpAbility.TotalScore.Value >= tempPfaEmpAbilityDetail.LowerLimit.Value)
                            {
                                for (int i = tempPfaEmpAbilityDetail.LowerLimit.Value; i <= tempPfaEmpAbility.TotalScore.Value; i++)
                                    tempPfaEmpAbilityDetail.ScoreInterval.Add(i);
                            }
                            else
                            {
                                tempPfaEmpAbilityDetail.ScoreInterval.Add(tempPfaEmpAbilityDetail.LowerLimit.Value);
                            }
                        }
                        tempPfaEmpAbility.PfaEmpAbilityDetail.Add(tempPfaEmpAbilityDetail);
                    }
                    model.PfaEmpAbility.Add(tempPfaEmpAbility);
                }
            }
            #endregion

            #region 與過去表現比較
            model.PastPerformanceData = GetPfaOptionList("PastPerformance", model.PastPerformance.HasValue ? model.PastPerformance.Value.ToString() : "");
            #endregion

            #region 評價目前工作
            model.NowPerformanceData = GetPfaOptionList("NowPerformance", model.NowPerformance.HasValue ? model.NowPerformance.Value.ToString() : "");
            #endregion

            #region 未來發展評斷
            model.DevelopmentData = GetPfaOptionList("Development", model.Development.HasValue ? model.Development.Value.ToString() : "");
            #endregion

            #region 簽核記錄
            model.PfaSignRecord = HRPortal.Common.PFA.PfaSignRecord.GePfaSignRecord(pfaSignProcess, model);
            #endregion

            return model;
        }
        #endregion

        #region 初始值
        private void GetDefaultData(string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", string txtStatus = "")
        {
            ViewBag.DepartmentList = GetDepartmentList(txtDepartmentID);
            ViewBag.txtDepartmentID = txtDepartmentID;
            ViewBag.txtEmployeeNo = txtEmployeeNo;
            ViewBag.txtEmployeeName = txtEmployeeName;
            ViewBag.StatusList = Common.PFA.PfaOption.GetPfaSignStatusOption(txtStatus, flag: true, exclude: "t");
            ViewBag.txtStatus = txtStatus;
        }
        #endregion

        #region 下拉
        /// <summary>
        ///  取得部門選單
        /// </summary>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.PfaDeptCode, item.PfaDeptName), Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });

            return listItem;
        }

        /// <summary>
        /// 取得群組選單
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetPfaOptionList(string groupCode, string selectedData, string valueFlag = "ID", bool flag = false)
        {
            var listItem = new List<SelectListItem>();
            var data = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == groupCode).OrderBy(x => x.Ordering).ToList();

            if (flag)
                listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            if (valueFlag == "ID")
            {
                //轉為Guid 判斷ID
                Guid SelectedDataID = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(selectedData))
                    SelectedDataID = Guid.Parse(selectedData);

                foreach (var item in data)
                    listItem.Add(new SelectListItem { Text = item.OptionName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            else
            {
                foreach (var item in data)
                    listItem.Add(new SelectListItem { Text = item.OptionName, Value = item.OptionCode, Selected = (selectedData == item.OptionCode ? true : false) });
            }
            return listItem;
        }

        /// <summary>
        /// 取得績效考核批號選單
        /// </summary>
        /// <param name="data"></param>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetPfaFormNoList(List<PfaCycle> datas, string selectedData)
        {
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            var listItem = new List<SelectListItem>();
            foreach (var item in datas.OrderByDescending(r => r.PfaYear).ThenByDescending(r => r.PfaFormNo))
                listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            return listItem;
        }
        #endregion

        /// <summary>
        /// 計算總和
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ManagerIndicatorTotalSum(PfaCycleEmpSignViewModel model)
        {
            var totalSum = (double)0;
            var pfaFirstScore = (double)0;

            try
            {
                if (model.PfaEmpIndicator != null)
                {
                    if (model.PfaEmpIndicator.Any())
                    {
                        foreach (var numberData in model.PfaEmpIndicator)
                        {
                            if (numberData.ManagerIndicator.HasValue)
                                totalSum = totalSum + numberData.ManagerIndicator.Value;
                        }
                        totalSum = Math.Floor(totalSum * 10) / 10;
                        pfaFirstScore = pfaFirstScore + totalSum;
                    }
                }

                if (model.ManagerAbility.HasValue)
                    pfaFirstScore = pfaFirstScore + model.ManagerAbility.Value;

                return Json(new { success = true, message = "", Total = totalSum.ToString(), FirstScore = pfaFirstScore.ToString() });
            }
            catch
            {
                return Json(new { success = true, message = "加總錯誤", Total = "0", FirstScore = "0" });
            }
        }

        /// <summary>
        /// 計算總和
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ManagerAbilityTotalSum(PfaCycleEmpSignViewModel model)
        {
            var totalSum = (decimal)0;
            var pfaFirstScore = (decimal)0;

            try
            {
                if (model.PfaEmpAbility != null)
                {
                    if (model.PfaEmpAbility.Any())
                    {
                        foreach (var numberData in model.PfaEmpAbility)
                        {
                            if (numberData.ManagerAbility.HasValue)
                                totalSum = totalSum + (decimal)numberData.ManagerAbility.Value;
                        }
                        totalSum = Math.Floor(totalSum * 10) / 10;
                        pfaFirstScore = pfaFirstScore + totalSum;
                    }
                }

                if (model.ManagerIndicator.HasValue)
                    pfaFirstScore = pfaFirstScore + (decimal)model.ManagerIndicator.Value;

                return Json(new { success = true, message = "", Total = totalSum.ToString(), FirstScore = pfaFirstScore.ToString() });
            }
            catch
            {
                return Json(new { success = true, message = "加總錯誤", Total = "0", FirstScore = "0" });
            }
        }
    }
}
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using PFA_Common = HRPortal.Services.DDMC_PFA.Common;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaCycleSecondEvaluationController : BaseController
    {
        #region 查詢
        public ActionResult Index(string txtOrgID = "", string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", string txtStatus = "", string txtPerformanceID = "", string txtSort = "", string txtNoRatio = "", string txtDeptSort = "", int page = 1, string cmd = "")
        {
            if (string.IsNullOrWhiteSpace(txtDeptSort))
                txtDeptSort = "Y";
            if (string.IsNullOrWhiteSpace(txtSort))
                txtSort = "ALL";

            GetDefaultData(txtOrgID, txtDepartmentID, txtEmployeeNo, txtEmployeeName, txtStatus, txtPerformanceID, txtSort, txtNoRatio, txtDeptSort);

            int currentPage = page < 1 ? 1 : page;

            //  m: 未收件 c:待評核 a:已評核 e: 已送出 r:退回修改
            string[] status = { "m", "c", "a", "e", "r" };

            var queryData = Services.GetService<PfaSignProcessService>().GetAll().Where(x => x.IsSecondEvaluation == true && x.PfaCycleEmp.Status != "y");

            if (!CurrentUser.IsAdmin)
                queryData = queryData.Where(x => x.PreSignEmpID == CurrentUser.EmployeeID);

            if (!string.IsNullOrEmpty(txtOrgID))
            {
                var orgID = Guid.Parse(txtOrgID);
                queryData = queryData.Where(x => x.PfaCycleEmp.PfaOrgID == orgID);
            }

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

            if (!string.IsNullOrEmpty(txtPerformanceID))
            {
                var performanceID = Guid.Parse(txtPerformanceID);
                queryData = queryData.Where(x => x.PfaCycleEmp.LastPerformance_ID == performanceID);
            }

            if (txtNoRatio == "Y")
                queryData = queryData.Where(x => x.IsRatio == false);

            var pfaOptionList = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignStatus").ToList();
            var tempQueryData = queryData.Select(x => new PfaCycleEmpSignViewModel()
            {
                PfaSignProcessID = x.ID,
                PfaCycleID = x.PfaCycleEmp.PfaCycleID,
                PfaOrgCode = x.PfaCycleEmp.PfaOrg.PfaOrgCode,
                PfaOrgName = x.PfaCycleEmp.PfaOrg.PfaOrgName,
                PfaDeptID = x.PfaCycleEmp.PfaDeptID,
                PfaDeptCode = x.PfaCycleEmp.PfaDept.PfaDeptCode,
                PfaDeptName = x.PfaCycleEmp.PfaDept.PfaDeptName,
                EmployeeID = x.PfaCycleEmpID,
                EmployeeNo = x.PfaCycleEmp.Employees.EmployeeNO,
                EmployeeName = x.PfaCycleEmp.Employees.EmployeeName,
                PfaSelfScore = x.PfaCycleEmp.PfaSelfScore,
                PfaFirstScore = x.PfaCycleEmp.PfaFirstScore,
                PfaLastScore = x.PfaCycleEmp.PfaLastScore,
                LastPerformance_ID = x.PfaCycleEmp.LastPerformance_ID,
                SignStatus = x.Status,
            });

            if (txtDeptSort == "Y" && txtSort != "ALL")
            {
                switch (txtSort)
                {
                    case "FirstH": //初核分數降冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenByDescending(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                        break;
                    case "FirstL": //初核分數升冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                        break;
                    case "SecondH": //複核分數降冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenByDescending(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                        break;
                    case "SecondL": //複核分數升冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                        break;
                }
            }
            else if (txtDeptSort == "Y" && txtSort == "ALL")
                tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.EmployeeNo);
            else if (txtDeptSort == "N" && txtSort != "ALL")
            {
                switch (txtSort)
                {
                    case "FirstH": //初核分數降冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenByDescending(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                        break;
                    case "FirstL": //初核分數升冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                        break;
                    case "SecondH": //複核分數降冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenByDescending(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                        break;
                    case "SecondL": //複核分數升冪
                        tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                        break;
                }
            }
            else
                tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.EmployeeNo);

            var viewModel = tempQueryData.ToList().ToPagedList(currentPage, currentPageSize);

            var pfaCycleIDs = viewModel.Select(x=> x.PfaCycleID).Distinct().ToList();
            var pfaCycleList = Services.GetService<PfaCycleService>().GetAll().Where(x => pfaCycleIDs.Contains(x.ID)).ToList();
            var pfaPerformanceList = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID).ToList();
            foreach (var item in viewModel)
            {
                var pfaCycle = pfaCycleList.FirstOrDefault(y => y.ID == item.PfaCycleID);
                var pfaOption = pfaOptionList.FirstOrDefault(y => y.OptionCode == item.SignStatus);
                var pfaPerformance = pfaPerformanceList.FirstOrDefault(y => y.ID == item.LastPerformance_ID);
                item.PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "";
                item.LastPerformanceName = pfaPerformance != null ? pfaPerformance.Name : "";
                item.StrSignStatus = pfaOption != null ? pfaOption.OptionName : "";
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtOrgID, string txtDepartmentID, string txtEmployeeNo, string txtEmployeeName, string txtStatus, string txtPerformanceID, string txtSort, string txtNoRatio, string txtDeptSort, string btnQuery, string btnClear)
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
                    txtOrgID,
                    txtDepartmentID,
                    txtEmployeeNo,
                    txtEmployeeName,
                    txtStatus,
                    txtPerformanceID,
                    txtSort,
                    txtNoRatio,
                    txtDeptSort,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtOrgID, txtDepartmentID, txtEmployeeNo, txtEmployeeName, txtStatus, txtPerformanceID, txtSort, txtNoRatio, txtDeptSort);

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
                model.EditFirstEvaluation = pfaSignProcess.IsFirstEvaluation;

                var pfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x => x.ID == model.PfaCycleEmpID);
                if (pfaCycleEmp == null)
                    throw new Exception("績效考核員工資料取得失敗");

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(pfaCycleEmp.PfaCycleID);
                if (pfaCycle == null)
                    throw new Exception("績效考核資料取得失敗");

                var pfaEmpIndicatorList = new List<PfaEmpIndicator>();
                var pfaEmpAbilityList = new List<PfaEmpAbility>();

                if (model.EditFirstEvaluation)
                {
                    pfaCycleEmp.ManagerIndicator = model.ManagerIndicator;
                    pfaCycleEmp.ManagerAbility = model.ManagerAbility;
                    pfaCycleEmp.PfaFirstScore = model.PfaFirstScore;
                    pfaCycleEmp.PastPerformance = model.PastPerformance;
                    pfaCycleEmp.NowPerformance = model.NowPerformance;
                    pfaCycleEmp.Development = model.Development;
                    pfaCycleEmp.FirstPerformance_ID = model.FirstPerformance_ID;

                    #region 績效考核員工工作績效
                    pfaEmpIndicatorList = model.PfaEmpIndicator.Select(x => new PfaEmpIndicator
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
                                var chkManagerAbility = Math.Round(sumAbilityScore.Value / tempPfaEmpAbility.PfaEmpAbilityDetail.Count() , 1, MidpointRounding.AwayFromZero);
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
                            if (tempPfaEmpAbility != null 
                                && tempPfaEmpAbility.PfaEmpAbilityDetail.Any())
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
                }

                pfaCycleEmp.PfaFinalScore = model.PfaLastScore;
                pfaCycleEmp.FinalPerformance_ID = model.LastPerformance_ID;

                pfaCycleEmp.PfaLastScore = model.PfaLastScore;
                pfaCycleEmp.LastPerformance_ID = model.LastPerformance_ID;
                pfaCycleEmp.LastAppraisal = model.LastAppraisal;
                pfaCycleEmp.ModifiedBy = CurrentUser.EmployeeID;
                pfaCycleEmp.ModifiedTime = DateTime.Now;

                pfaSignProcess.Status = model.SignStatus;
                pfaSignProcess.Assessment = model.Assessment;

                 if (cmd == "btnSent")
                    pfaSignProcess.SignEmpID = CurrentUser.EmployeeID;

                result = Services.GetService<PfaSignProcessService>().UpdateSecondEvaluationData(pfaSignProcess
                    , pfaCycleEmp
                    , pfaEmpIndicatorList
                    , pfaEmpAbilityList
                    , cmd);

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
                else if (cmd == "btnSent")
                    model.SignStatus = "e"; // e: 已送出
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
                        if (cmd == "btnSent" || model.IsRatio)
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
                    var checkPfaEmpAbility = pfaEmpAbility.PfaEmpAbilityDetail.OrderBy(x => x.Ordering).FirstOrDefault(x => !x.AbilityScore.HasValue);
                    if (checkPfaEmpAbility != null)
                    {
                        if (cmd == "btnSent" || model.IsRatio)
                            throw new Exception(string.Format("請選擇{0}-{1}能力符合程度評核", pfaEmpAbility.PfaAbilityName, checkPfaEmpAbility.PfaAbilityKey));
                    }
                }
                model.ManagerAbility = model.PfaEmpAbility.Where(x => x.ManagerAbility.HasValue).Sum(x => x.ManagerAbility);
                if (model.ManagerAbility.HasValue)
                    pfaFirstScore = pfaFirstScore + model.ManagerAbility.Value;
                #endregion

                model.PfaFirstScore = pfaFirstScore;
                var tempPfaFirstScore = (decimal)model.PfaFirstScore;
                var pfaPerformanceFirst = Services.GetService<PfaPerformanceService>().GetAll().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed == true &&
                                                                                                                    x.ScoresStart <= tempPfaFirstScore && x.ScoresEnd >= tempPfaFirstScore);
                if (pfaPerformanceFirst != null)
                    model.FirstPerformance_ID = pfaPerformanceFirst.ID;
                else
                {
                    if (cmd == "btnSent" || model.IsRatio)
                        throw new Exception(string.Format("初核總分:{0}，查無績效等第資料", model.PfaFirstScore));
                }

                if (!model.PfaLastScore.HasValue)
                {
                    if (cmd == "btnSent" || model.IsRatio)
                        throw new Exception("請輸入複核總分");
                }
                else 
                {
                    if (!Regex.IsMatch(model.PfaLastScore.Value.ToString(), pattern))
                        throw new Exception("複核總分，小數點最多1位");

                    var tempPfaLastScore = (decimal)model.PfaLastScore;
                    var pfaPerformanceLast = Services.GetService<PfaPerformanceService>().GetAll().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed == true &&
                                                                                                                       x.ScoresStart <= tempPfaLastScore && x.ScoresEnd >= tempPfaLastScore);
                    if (pfaPerformanceLast != null)
                        model.LastPerformance_ID = pfaPerformanceLast.ID;
                    else
                    {
                        if (cmd == "btnSent" || model.IsRatio)
                            throw new Exception(string.Format("複核總分:{0}，查無績效等第資料", model.PfaLastScore));
                    }
                }

                if (cmd == "btnSent" || model.IsRatio)
                {
                    if (!model.PastPerformance.HasValue)
                        throw new Exception("請選擇與過去表現比較");

                    if (!model.NowPerformance.HasValue)
                        throw new Exception("請選擇評價目前工作");

                    if (!model.Development.HasValue)
                        throw new Exception("請選擇未來發展評斷");

                    if (string.IsNullOrWhiteSpace(model.FirstAppraisal) && string.IsNullOrWhiteSpace(model.LastAppraisal))
                        throw new Exception("請輸入複核主管綜合考評");
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
                PfaCycleOrgSent = new List<PfaCycleOrgSentViewModel>(),
            };

            var now = DateTime.Now.Date;
            string[] status = { PfaSignProcess_Status.Reviewed
                    , PfaSignProcess_Status.Submitted }; //  m: 未收件 c:待評核 a:已評核 e: 已送出 r:退回修改

            List<PfaCycle> pfaCycleList = Services.GetService<PfaCycleService>().GetAll()
                    .Where(x => x.CompanyID == CurrentUser.CompanyID
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

                var pfaOrgIDs = Services.GetService<PfaSignProcessService>().GetAll()
                    .Where(x => x.PfaCycleEmp.IsRatio 
                        && x.PfaCycleEmp.PfaCycleID == pfaCycle.ID 
                        && x.IsRatio 
                        && x.IsSecondEvaluation == true 
                        && x.PreSignEmpID == CurrentUser.EmployeeID
                    ).Select(x => x.PfaCycleEmp.PfaOrgID)
                    .Distinct()
                    .ToList();

                // 該主管所有可評核績效考核組織
                var queryPfaOrgData = Services.GetService<PfaOrgService>().GetAll()
                    .Where(x => pfaOrgIDs.Contains(x.ID) 
                        && x.CompanyID == CurrentUser.CompanyID
                    ).ToList();

                var pfaCycleOrgSentList = queryPfaOrgData.Select(x => 
                {
                    return new PfaCycleOrgSentViewModel
                    {
                        PfaOrgID = x.ID,
                        PfaOrgCode = x.PfaOrgCode,
                        PfaOrgName = x.PfaOrgName,
                        Ordering = x.Ordering.HasValue ? x.Ordering.Value : 0,
                    };
                }).OrderBy(x => x.Ordering)
                .ToList();

                // 由考核組織取得簽核中績效考核員工資料
                var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll()
                    .Where(x => x.PfaOrgID.HasValue 
                        && pfaOrgIDs.Contains(x.PfaOrgID) 
                        && x.IsRatio 
                        && x.Status == PfaCycleEmp_Status.InApprovalProcess
                        ).ToList();

                if (pfaCycleEmpList.Any())
                {
                    var pfaCycleRationList = Services.GetService<PfaCycleRationService>().GetAll()
                        .Where(x => x.PfaCycleID == pfaCycle.ID 
                            && pfaOrgIDs.Contains(x.PfaOrgID)
                        ).Include(r=>r.PfaCycleRationDetail)
                        .ToList();

                    if (!pfaCycleRationList.Any())
                        throw new Exception("無績效考核人數配比資料");

                    ds.PfaFormNoList = GetPfaFormNoList(pfaCycleList, pfaCycleId);
                    ds.PfaCycleID = pfaCycle.ID;
                    ds.PfaFormNo = pfaCycle.PfaFormNo;
                    ds.PfaYear = pfaCycle.PfaYear;
                    ds.Remark = pfaCycle.Desription;
                    foreach (var pfaCycleOrgSent in pfaCycleOrgSentList)
                    {
                        var pfaCycleRation = pfaCycleRationList.FirstOrDefault(x => x.PfaOrgID == pfaCycleOrgSent.PfaOrgID);
                        if (pfaCycleRation != null)
                            pfaCycleOrgSent.RationAll = pfaCycleRation.OrgTotal.HasValue ? pfaCycleRation.OrgTotal.Value : 0;
                        else
                            continue;

                        var pfaSignProcessList = Services.GetService<PfaSignProcessService>().GetAll()
                            .Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycle.ID 
                                && x.PfaCycleEmp.PfaOrgID == pfaCycleOrgSent.PfaOrgID 
                                && x.PreSignEmpID == CurrentUser.EmployeeID 
                                && x.IsRatio 
                                && x.IsSecondEvaluation == true
                            ).ToList();

                        // 已評核人數
                        pfaCycleOrgSent.SecondFinal = pfaSignProcessList
                            .Where(x => status.Contains(x.Status))
                            .Select(x => x.PfaCycleEmpID)
                            .Distinct()
                            .Count();

                        #region 計算配比
                        if (pfaCycleOrgSent.RationAll == pfaCycleOrgSent.SecondFinal)
                        {
                            if(PFA_Common.CycleRationDetail.CheckSecondEvaluationRatio(pfaCycleRation.OrgTotal
                                , pfaCycleRation.PfaCycleRationDetail.ToList()))
                            {
                                pfaCycleOrgSent.IsRation = "Y";
                            }
                        }
                        #endregion

                        // 狀態
                        if (pfaCycleOrgSent.RationAll == pfaCycleOrgSent.SecondFinal)
                        {
                            if (pfaSignProcessList.Any(x => x.Status == "a"))
                                pfaCycleOrgSent.SignStatus = "N";
                            else
                                pfaCycleOrgSent.SignStatus = "Y";
                        }
                        else
                        {
                            pfaCycleOrgSent.SignStatus = "N";
                        }
                    }
                    pfaCycleOrgSentList = pfaCycleOrgSentList.Where(x => x.RationAll > 0).ToList();
                    ds.PfaCycleOrgSent = pfaCycleOrgSentList;
                }

                if (!ds.PfaCycleOrgSent.Any(x => x.SignStatus == "N"))
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
                    PfaFormNoList = GetPfaFormNoList(pfaCycleList, pfaCycleId),
                    PfaCycleOrgSent = new List<PfaCycleOrgSentViewModel>(),
                };
            }

            if(ds.PfaFormNoList.Any() == false)
            {
                ds = new PfaCycleSentDataViewModel
                {
                    PfaFormNo = "",
                    PfaYear = "",
                    Remark = "",
                    PfaFormNoList = new List<SelectListItem>(),
                    PfaCycleOrgSent = new List<PfaCycleOrgSentViewModel>(),
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
                if (model.PfaCycleOrgSent.Count() == 0)
                {
                    WriteLog("請選擇要送出的部門類別");
                    return Json(new { success = false, message = "請選擇要送出的部門類別" });
                }

                var isExist = Services.GetService<PfaCycleService>().IsExist(model.PfaCycleID);
                if (!isExist)
                {
                    WriteLog(string.Format("績效考核批號資料不存在,ID:{0}", model.PfaCycleID));
                    return Json(new { success = false, message = "績效考核批號資料不存在" });
                }


                #region 計算配比
                var newModel = model.PfaCycleOrgSent.Select(x => new PfaCycleRation
                {
                    PfaCycleID = model.PfaCycleID,
                    PfaOrgID = x.PfaOrgID
                }).ToList();

                var pfaCycleID = model.PfaCycleID;
                var pfaOrgIDs = newModel.Select(x => x.PfaOrgID).ToList();
                var pfaCycleRationList = Services.GetService<PfaCycleRationService>()
                    .Where(x => x.PfaCycleID == pfaCycleID 
                        && pfaOrgIDs.Contains(x.PfaOrgID)
                    ).ToList();

                if (!pfaCycleRationList.Any())
                {
                    WriteLog(string.Format("查無績效考核人數配比資料,PfaCycleID:{0}", model.PfaCycleID));
                    return Json(new { success = false, message = "查無績效考核人數配比資料" });
                }

                decimal totalScore = 0;
                foreach (var data in newModel)
                {
                    totalScore = 0;
                    var pfaCycleRation = pfaCycleRationList.FirstOrDefault(x => x.PfaOrgID == data.PfaOrgID);
                    if (pfaCycleRation == null)
                    {
                        WriteLog(string.Format("查無績效考核人數配比資料,PfaOrgID:{0}", data.PfaOrgID));
                        return Json(new { success = false, message = "查無績效考核人數配比資料" });
                    }

                    if (false == PFA_Common.CycleRationDetail.CheckSecondEvaluationRatio(pfaCycleRation.OrgTotal
                        , pfaCycleRation.PfaCycleRationDetail.ToList()
                        , out totalScore))
                    {
                        WriteLog(string.Format("無績效考核人數配比錯誤,PfaOrgID:{0}", data.PfaOrgID));
                        return Json(new { success = false, message = "無績效考核人數配比錯誤" });
                    }

                    data.FinalRation = totalScore;
                }
                #endregion

                #region 檢查是否有不需配比人員未送出
                result.success = true;
                string checkHasUnRatioNoSubmitMasg = string.Empty;
                foreach (var data in newModel)
                {
                    if (CheckHasUnRatioNoSubmit(data.PfaCycleID, data.PfaOrgID, CurrentUser.EmployeeID, out checkHasUnRatioNoSubmitMasg))
                    {
                        result.success = false;
                        break;
                    }
                }
                if (!result.success)
                    throw new Exception("尚有不需配比人員未送出" + checkHasUnRatioNoSubmitMasg);
                #endregion


                result = Services.GetService<PfaSignProcessService>().SentSecondEvaluationData(newModel, CurrentUser.EmployeeID);
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
        /// 檢查是否有不需配比人員未送出
        /// </summary>
        /// <param name="pfaCycleID"></param>
        /// <param name="pfaOrgID"></param>
        /// <param name="selfEmpId"></param>
        /// <param name="msg">返回未送出人員</param>
        /// <returns></returns>
        private bool CheckHasUnRatioNoSubmit(Guid pfaCycleID, Guid pfaOrgID, Guid selfEmpId, out string msg)
        {
       
            var pfaSignProcessList = Services.GetService<PfaSignProcessService>().GetAll()
                .Where(x=> x.IsSecondEvaluation == true)
                .Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID
                    && x.PfaCycleEmp.PfaOrgID == pfaOrgID
                    && x.PreSignEmpID == selfEmpId
                    && x.IsRatio == false
                    && x.Status != PfaSignProcess_Status.Submitted
                    && x.PfaCycleEmp.EmployeeID != selfEmpId
                );

            bool check = pfaSignProcessList.Any();
            msg = string.Empty;
            if (check)
            {
                // 查看未送出的不需配比人員
                var emps = pfaSignProcessList.Select(r => r.PfaCycleEmp.Employees)
                    .Include(r => r.Department)
                    .ToList();
                foreach (var itemGroup in emps.GroupBy(r=>r.Department.DepartmentName))
                {
                    msg += @"<br>" + itemGroup.Key + ":";
                    msg += @"" + string.Join(",", itemGroup.Select(r => r.EmployeeName));
                }
            }
            

            return check;
        }
        #endregion

        #region 退回
        public ActionResult RejectQuery(Guid? Id = null)
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
            //  m: 未收件 c:待評核 a:已評核 e: 已送出 b:已退回 r:退回修改
            string[] finStatus = { "a", "e" };

            try
            {
                // 該主管目前須簽核資料
                var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                    .Where(x => (x.Status == PfaSignProcess_Status.PendingReview
                    || x.Status == PfaSignProcess_Status.Reviewed
                    || x.Status == PfaSignProcess_Status.ReturnedForModification
                    ) 
                    && x.IsSecondEvaluation == true);
                if (!CurrentUser.IsAdmin)
                    queryData = queryData.Where(x => x.PreSignEmpID == CurrentUser.EmployeeID);
                var signPfaSignProcess = queryData.ToList();

                var pfaCycleList = Services.GetService<PfaCycleService>().GetAll()
                    .Where(x => 
                        x.CompanyID == CurrentUser.CompanyID 
                        && x.Status == "a"
                    ).ToList()
                    .OrderByDescending(x => x.PfaYear)
                    .ThenByDescending(x => x.PfaFormNo)
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

                var pfaDeptIDs = Services.GetService<PfaSignProcessService>().GetAll()
                    .Where(x => x.PfaCycleEmp.IsRatio 
                    && x.PfaCycleEmp.PfaCycleID == pfaCycle.ID 
                    && x.IsRatio 
                    && x.IsSecondEvaluation == true 
                    && x.PreSignEmpID == CurrentUser.EmployeeID
                    ).Select(x => x.PfaCycleEmp.PfaDeptID)
                    .ToList();

                var queryPfaDeptData = Services.GetService<PfaDeptService>().GetAll().Where(x => pfaDeptIDs.Contains(x.ID) && x.CompanyID == CurrentUser.CompanyID && x.BeginDate <= now && x.EndDate >= now).ToList();
                // 該主管所有可評核部門
                var pfaCycleDeptSentList = queryPfaDeptData.Select(x => new PfaCycleSentViewModel
                {
                    PfaDeptID = x.ID,
                    PfaDeptCode = x.PfaDeptCode,
                    PfaDeptName = x.PfaDeptName,
                }).OrderBy(x => x.PfaDeptCode).ToList();

                // 由部門取得簽核中績效考核員工資料
                var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => pfaDeptIDs.Contains(x.PfaDeptID) && x.IsRatio && x.Status == "a").ToList();
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
                            && (   x.Status == PfaSignProcess_Status.PendingReview
                                || x.Status == PfaSignProcess_Status.Reviewed
                                || x.Status == PfaSignProcess_Status.ReturnedForModification
                            ) 
                            && x.IsRatio && x.IsSecondEvaluation == true).ToList();
                        // 應評核人數
                        pfaCycleDeptSent.FirstAll = pfaSignProcessList.Count();
                        // 已評核人數
                        pfaCycleDeptSent.FirstFinal = pfaSignProcessList.Where(x => finStatus.Contains(x.Status)).Select(x => x.PfaCycleEmpID).Distinct().Count();

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
                {
                    if (signPfaSignProcess.Any())
                        ViewBag.Status = "N";
                    else
                        ViewBag.Status = "Y";
                }
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
            return View("_Reject", ds);
        }

        [HttpPost]
        public ActionResult RejectConfirm(PfaCycleSentDataViewModel model)
        {
            var result = new Result();

            try
            {
                if (model.PfaCycleDeptSent.Count() == 0)
                {
                    WriteLog("請選擇要退回的部門");
                    return Json(new { success = false, message = "請選擇要退回的部門" });
                }

                var isExist = Services.GetService<PfaCycleService>().IsExist(model.PfaCycleID);
                if (!isExist)
                {
                    WriteLog(string.Format("績效考核批號資料不存在,ID:{0}", model.PfaCycleID));
                    return Json(new { success = false, message = "績效考核批號資料不存在" });
                }

                if (string.IsNullOrWhiteSpace(model.Assessment))
                {
                    WriteLog("請於簽核意見輸入退回原因");
                    return Json(new { success = false, message = "請於簽核意見輸入退回原因" });
                }

                var newModel = model.PfaCycleDeptSent.Select(x => new PfaCycleEmp
                {
                    PfaCycleID = model.PfaCycleID,
                    PfaDeptID = x.PfaDeptID
                }).ToList();

                result = Services.GetService<PfaSignProcessService>().BackSecondEvaluationData(newModel, model.Assessment, CurrentUser.EmployeeID);
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

        #region 配比查詢
        /// <summary>
        /// 試算
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Calculation(PfaCycleRationDetailDataViewModel model)
        {
            string isRation = "";

            try
            {
                decimal totalScore = 0;

                var pfaCycleRation = Services.GetService<PfaCycleRationService>().GetAll()
                    .FirstOrDefault(x => x.PfaCycleID == model.PfaCycleID 
                        && x.PfaOrgID == model.PfaOrgID);

                if (pfaCycleRation == null)
                    throw new Exception("查無績效考核人數配比資料");


                var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.ToList();

                var number = 0;
                var numberTotal = 0;
                foreach (var pfaCycleRationDetail in pfaCycleRationDetailList)
                {
                    number = 0;
                    var detail = model.Number.FirstOrDefault(x => x.PfaPerformanceID == pfaCycleRationDetail.PfaPerformanceID);

                    number = detail != null ? detail.Number : 0;

                    if (pfaCycleRationDetail.Multiplier.HasValue)
                        totalScore = totalScore + (number * pfaCycleRationDetail.Multiplier.Value);

                    numberTotal = numberTotal + number;
                }
                if (numberTotal >= pfaCycleRation.OrgTotal)
                {
                    if (totalScore <= pfaCycleRation.OrgTotal)
                        isRation = "Y";
                    else
                        isRation = "N";
                }
                else
                {
                    isRation = "";
                }
                return Json(new { success = true, message = "", data = isRation });
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("試算是否配比正確失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "試算是否配比正確失敗", data = "" });
            }
        }

        /// <summary>
        /// 績效等第配比(複核)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult LastPerformanceRation(PfaCycleEmpSignViewModel model)
        {
            var isRation = "";
            try
            {
                #region 績效等第配比
                if (!model.PfaLastScore.HasValue)
                    throw new Exception("請輸入複核總分");

                if (!model.PfaCycleID.HasValue)
                    throw new Exception("查無績效考核批號");

                if (!model.PfaCycleEmpID.HasValue)
                    throw new Exception("查無績效考核員工資料");

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(model.PfaCycleID.Value);
                if (pfaCycle == null)
                    throw new Exception("查無績效考核批號");

                var pfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x=> x.ID == model.PfaCycleEmpID.Value);
                if (pfaCycleEmp == null)
                    throw new Exception("查無績效考核員工資料");

                var tempPfaLastScore = (decimal)model.PfaLastScore;
                var tempPfaPerformance = Services.GetService<PfaPerformanceService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.ScoresStart <= tempPfaLastScore && x.ScoresEnd >= tempPfaLastScore);
                if (tempPfaPerformance != null)
                    model.LastPerformance_ID = tempPfaPerformance.ID;
                else
                    throw new Exception("查無績效等第資料");

                model.PfaCycleRationData = new PfaCycleRationDataViewModel
                {
                    FirstDept = 0,
                    PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>()
                };
                string[] itemAry = { "Ration", "First", "Last", "Calculat" };
                string[] itemNameAry = { "配比", "初核", "複核", "試算" };

                var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaOrgID == pfaCycleEmp.PfaOrgID &&
                                                                                                    x.PfaCycleID == model.PfaCycleID && x.IsRatio).ToList();
                var pfaDeptIDs = pfaCycleEmpList.Select(x => x.PfaDeptID).Distinct().ToList();
                var pfaDeptList = Services.GetService<PfaDeptService>().GetAll().Where(x => pfaDeptIDs.Contains(x.ID)).OrderBy(x => x.PfaDeptCode).ToList();
                model.PfaCycleRationData.FirstDept = pfaDeptList.Count();

                bool pfaCycleRationFlag = false;
                if (pfaCycle.PfaCycleRation.Any())
                {
                    var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == pfaCycleEmp.PfaOrgID);
                    if (pfaCycleRation != null)
                    {
                        pfaCycleRationFlag = true;
                        var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                        model.PfaCycleRationData.PfaPerformance = pfaCycleRationDetailList.Select(x =>
                        {
                            var tempScores = "";
                            if (x.ScoresStart.HasValue)
                                tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                            if (x.ScoresEnd.HasValue)
                                tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                            return new PfaPerformanceViewModel
                            {
                                ID = x.PfaPerformanceID,
                                Code = x.Code,
                                Name = x.Name,
                                Scores = tempScores,
                                Rates = x.Rates,
                            };
                        }).ToList();
                    }
                }
                if (pfaCycleRationFlag == false)
                {
                    model.PfaCycleRationData.PfaPerformance = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed).OrderBy(x => x.Ordering).ToList().Select(x =>
                    {
                        var tempScores = "";
                        if (x.ScoresStart.HasValue)
                            tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                        if (x.ScoresEnd.HasValue)
                            tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                        return new PfaPerformanceViewModel
                        {
                            ID = x.ID,
                            Code = x.Code,
                            Name = x.Name,
                            Scores = tempScores,
                            Multiplier = x.Multiplier,
                        };
                    }).ToList();
                }

                if (model.PfaCycleRationData.PfaPerformance.Any())
                {
                    for (int i = 0; i < itemAry.Length; i++)
                    {
                        if (itemAry[i] == "First")
                        {
                            foreach (var pfaDept in pfaDeptList)
                            {
                                var tempRationDetail = new PfaCycleRationDetailDataViewModel
                                {
                                    RowType = itemAry[i],
                                    RowTypeName = itemNameAry[i],
                                    PfaDeptID = pfaDept.ID,
                                    PfaDeptName = pfaDept.PfaDeptName,
                                    OrgTotal = 0,
                                    IsRation = "",
                                    Number = new List<RationNumberViewModel>()
                                };
                                foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                {
                                    var numberData = new RationNumberViewModel
                                    {
                                        PfaPerformanceID = pfaPerformance.ID,
                                        Number = 0,
                                    };
                                    tempRationDetail.Number.Add(numberData);
                                }
                                model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                            }
                        }
                        else
                        {
                            var tempRationDetail = new PfaCycleRationDetailDataViewModel
                            {
                                RowType = itemAry[i],
                                RowTypeName = itemNameAry[i],
                                OrgTotal = 0,
                                IsRation = "",
                                Number = new List<RationNumberViewModel>()
                            };
                            foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                            {
                                var numberData = new RationNumberViewModel
                                {
                                    PfaPerformanceID = pfaPerformance.ID,
                                    Number = 0,
                                };
                                tempRationDetail.Number.Add(numberData);
                            }
                            model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                        }
                    }
                }

                if (pfaCycle.PfaCycleRation.Any())
                {
                    var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == pfaCycleEmp.PfaOrgID);
                    if (pfaCycleRation != null)
                    {
                        decimal totalScore = 0;
                        int totalNum = 0;
                        var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                        for (int i = 0; i < itemAry.Length; i++)
                        {
                            totalScore = 0;
                            totalNum = 0;
                            var tempPfaCycleRationDetailList = model.PfaCycleRationData.PfaCycleRationDetail.Where(x => x.RowType == itemAry[i]);
                            foreach (var detail in tempPfaCycleRationDetailList)
                            {
                                detail.IsRation = pfaCycleRation.IsRation ? "Y" : "";
                                detail.OrgTotal = pfaCycleRation.OrgTotal;

                                if (detail.PfaDeptID.HasValue)
                                {
                                    var deptTotal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value);
                                    detail.OrgTotal = deptTotal;
                                }

                                if (model.PfaCycleRationData.PfaPerformance.Any())
                                {
                                    foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                    {
                                        var tempPfaCycleRationDetail = pfaCycleRationDetailList.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                        var numberData = detail.Number.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                        if (tempPfaCycleRationDetail != null && numberData != null)
                                        {

                                            switch (detail.RowType)
                                            {
                                                case "Ration":
                                                    if (tempPfaCycleRationDetail.Staffing.HasValue)
                                                    {
                                                        numberData.Number = tempPfaCycleRationDetail.Staffing.Value;
                                                        totalNum = totalNum + numberData.Number;
                                                    }
                                                    break;
                                                case "First":
                                                    if (detail.PfaDeptID.HasValue)
                                                    {
                                                        var firstFinal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value && x.FirstPerformance_ID == pfaPerformance.ID);
                                                        numberData.Number = firstFinal;
                                                        totalNum = totalNum + numberData.Number;
                                                    }
                                                    break;
                                                case "Last":
                                                case "Calculat":
                                                    var lastFinal = pfaCycleEmpList.Where(x => x.ID != model.PfaCycleEmpID)
                                                        .Count(x => x.LastPerformance_ID == pfaPerformance.ID);

                                                    if (model.LastPerformance_ID == pfaPerformance.ID)
                                                        lastFinal = lastFinal + 1;

                                                    numberData.Number = lastFinal;

                                                    if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                        totalScore = totalScore + 
                                                            (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);

                                                    totalNum = totalNum + numberData.Number;
                                                    break;
                                            }
                                        }
                                    }
                                    switch (detail.RowType)
                                    {
                                        case "Ration":
                                            detail.IsRation = "Y";
                                            break;
                                        case "First":
                                            detail.IsRation = "";
                                            break;
                                        case "Last":
                                        case "Calculat":
                                            if (totalNum == detail.OrgTotal)
                                            {
                                                if (totalScore <= detail.OrgTotal)
                                                {
                                                    detail.IsRation = "Y";
                                                }
                                                else
                                                {
                                                    detail.IsRation = "N";
                                                }
                                            }
                                            else
                                            {
                                                detail.IsRation = string.Empty;
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                var dataResult = new StringBuilder();
                dataResult.AppendLine("<table id='tableCheckData' class='table-bordered ui-jqgrid' style='width:100%'>");
                dataResult.AppendLine("<thead>");
                dataResult.AppendLine("<tr class='ui-jqgrid-labels' role='rowheader' style='height:40px;color:#777'>");
                dataResult.AppendLine("<th style='width:8%'><div id='jqgh_grid-table_code' style='text-align:center;'></div></th>");
                dataResult.AppendLine("<th style='width:8%'><div id='jqgh_grid-table_code' style='text-align:center;'>部門</div></th>");
                dataResult.AppendLine("<th style='width:8%'><div id='jqgh_grid-table_code' style='text-align:center;'>應配比人數</div></th>");
                if (model.PfaCycleRationData.PfaPerformance.Count > 0)
                {
                    foreach (var tempTh in model.PfaCycleRationData.PfaPerformance)
                    {
                        dataResult.AppendLine(string.Format("<th><div id='jqgh_grid-table_code' style='text-align:center;color:red;'>{0}<br>{1}<br>{2} %</div></th>", tempTh.Name, tempTh.Scores, tempTh.Rates));
                    }
                }
                dataResult.AppendLine("<th colspan='2' style='width:10%'><div id='jqgh_grid-table_code' style='text-align:center;'>配比正確</div></th>");
                dataResult.AppendLine("</tr>");
                dataResult.AppendLine("</thead>");
                dataResult.AppendLine("<tbody id='tbodyCheckData'>");
                if (model.PfaCycleRationData.PfaCycleRationDetail.Count() > 0)
                {
                    foreach (var detail in model.PfaCycleRationData.PfaCycleRationDetail)
                    {
                        dataResult.AppendLine(string.Format("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' id='{0}'>", detail.RowType));
                        dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>");
                        dataResult.AppendLine(string.Format("<span id='RowTypeName' name='RowTypeName'>{0}</span>", detail.RowTypeName));
                        dataResult.AppendLine("</td>");
                        dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>");
                        dataResult.AppendLine(string.Format("<span id='PfaDeptName' name='PfaDeptName'>{0}</span>", detail.PfaDeptName));
                        dataResult.AppendLine("</td>");
                        dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>");
                        dataResult.AppendLine(string.Format("<span id='OrgTotal' name='OrgTotal'>{0}</span>", detail.OrgTotal));
                        dataResult.AppendLine("</td>");
                        foreach (var number in detail.Number)
                        {
                            dataResult.AppendLine(string.Format("<td role='gridcell' style='white-space:normal;text-align:center' aria-describedby='grid-table_notification_count' id='{0}' name='RationNumber'>", number.PfaPerformanceID));
                            if (detail.RowType == "Calculat")
                                dataResult.AppendLine(string.Format("<input class='form-control calculatRationNumber' id='RationNumber' min='0' name='RationNumber' type='number' value='{0}'>", number.Number));
                            else
                            {
                                if (model.IsRatio == true && detail.RowType == "Last")
                                    dataResult.AppendLine(string.Format("<a href ='/DDMC_PFA/PfaCycleSecondEvaluation?txtPerformanceID={0}&cmd=Query'>{1}</a>", number.PfaPerformanceID, number.Number));
                                else
                                    dataResult.AppendLine(string.Format("<span id='RationNumber' name='RationNumber'>{0}</span>", number.Number));
                            }
                            dataResult.AppendLine("</td>");
                        }
                        if (detail.RowType == "Calculat")
                        {
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;width:5%;' aria-describedby='grid-table_notification_count'>");
                            dataResult.AppendLine("<button type='button' id='btnCalculat' class='btn btn-primary'>試算</button>");
                            dataResult.AppendLine("</td>");
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;width:5%;' aria-describedby='grid-table_notification_count'>");
                            dataResult.AppendLine(string.Format("<span id='CalculatIsRation' name='CalculatIsRation'>{0}</span>", detail.IsRation));
                            dataResult.AppendLine("</td>");
                        }
                        else
                        {
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center' aria-describedby='grid-table_notification_count' colspan='2'>");
                            dataResult.AppendLine(string.Format("<span id='IsRation' name='IsRation'>{0}</span>", detail.IsRation));
                            dataResult.AppendLine("</td>");
                        }
                        dataResult.AppendLine("</tr>");
                    }
                }
                dataResult.AppendLine("</tbody>");
                dataResult.AppendLine("</table>");

                return Json(new { success = true, message = "", data = dataResult.ToString(), IsRation = isRation });
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("試算績效等第配比失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "試算績效等第配比失敗", data = "", IsRation = "" });
            }
        }

        /// <summary>
        /// 批次績效等第配比(複核)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult BathLastPerformanceRation(PfaCycleEmpBatchViewModel data)
        {
            var isRation = "";
            try
            {
                var model = new PfaCycleEmpSignViewModel();

                #region 績效等第配比
                if (!data.PfaCycleEmp.Any())
                    throw new Exception("查無績效考核員工資料");

                model.PfaCycleID = data.PfaCycleID;
                if (!model.PfaCycleID.HasValue)
                    throw new Exception("查無績效考核批號");

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(model.PfaCycleID.Value);
                if (pfaCycle == null)
                    throw new Exception("查無績效考核批號");

                model.PfaOrgID =  data.PfaOrgID;
                if (!model.PfaOrgID.HasValue)
                    throw new Exception("查無組織部門資料");

                foreach (var emp in data.PfaCycleEmp)
                {
                    if (emp.PfaLastScore.HasValue)
                    {
                        var tempPfaLastScore = (decimal)emp.PfaLastScore;
                        var tempPfaPerformance = Services.GetService<PfaPerformanceService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.ScoresStart <= tempPfaLastScore && x.ScoresEnd >= tempPfaLastScore);
                        if (tempPfaPerformance != null)
                            emp.LastPerformance_ID = tempPfaPerformance.ID;
                        else
                            throw new Exception(string.Format("考核員工{0}，查無績效等第資料", emp.PfaCycleEmpName));
                    }
                }

                model.PfaCycleRationData = new PfaCycleRationDataViewModel
                {
                    FirstDept = 0,
                    PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>()
                };
                string[] itemAry = { "Ration", "First", "Last", "Calculat" };
                string[] itemNameAry = { "配比", "初核", "複核", "試算" };

                var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaOrgID == model.PfaOrgID &&
                                                                                                    x.PfaCycleID == model.PfaCycleID && x.IsRatio).ToList();
                var pfaDeptIDs = pfaCycleEmpList.Select(x => x.PfaDeptID).Distinct().ToList();
                var pfaDeptList = Services.GetService<PfaDeptService>().GetAll().Where(x => pfaDeptIDs.Contains(x.ID)).OrderBy(x => x.PfaDeptCode).ToList();
                model.PfaCycleRationData.FirstDept = pfaDeptList.Count();

                bool pfaCycleRationFlag = false;
                if (pfaCycle.PfaCycleRation.Any())
                {
                    var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == model.PfaOrgID);
                    if (pfaCycleRation != null)
                    {
                        pfaCycleRationFlag = true;
                        var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                        model.PfaCycleRationData.PfaPerformance = pfaCycleRationDetailList.Select(x =>
                        {
                            var tempScores = "";
                            if (x.ScoresStart.HasValue)
                                tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                            if (x.ScoresEnd.HasValue)
                                tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                            return new PfaPerformanceViewModel
                            {
                                ID = x.PfaPerformanceID,
                                Code = x.Code,
                                Name = x.Name,
                                Scores = tempScores,
                                Rates = x.Rates,
                            };
                        }).ToList();
                    }
                }
                if (pfaCycleRationFlag == false)
                {
                    model.PfaCycleRationData.PfaPerformance = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed).OrderBy(x => x.Ordering).ToList().Select(x =>
                    {
                        var tempScores = "";
                        if (x.ScoresStart.HasValue)
                            tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                        if (x.ScoresEnd.HasValue)
                            tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                        return new PfaPerformanceViewModel
                        {
                            ID = x.ID,
                            Code = x.Code,
                            Name = x.Name,
                            Scores = tempScores,
                            Multiplier = x.Multiplier,
                        };
                    }).ToList();
                }

                if (model.PfaCycleRationData.PfaPerformance.Any())
                {
                    for (int i = 0; i < itemAry.Length; i++)
                    {
                        if (itemAry[i] == "First")
                        {
                            foreach (var pfaDept in pfaDeptList)
                            {
                                var tempRationDetail = new PfaCycleRationDetailDataViewModel
                                {
                                    RowType = itemAry[i],
                                    RowTypeName = itemNameAry[i],
                                    PfaDeptID = pfaDept.ID,
                                    PfaDeptName = pfaDept.PfaDeptName,
                                    OrgTotal = 0,
                                    IsRation = "",
                                    Number = new List<RationNumberViewModel>()
                                };
                                foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                {
                                    var numberData = new RationNumberViewModel
                                    {
                                        PfaPerformanceID = pfaPerformance.ID,
                                        Number = 0,
                                    };
                                    tempRationDetail.Number.Add(numberData);
                                }
                                model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                            }
                        }
                        else
                        {
                            var tempRationDetail = new PfaCycleRationDetailDataViewModel
                            {
                                RowType = itemAry[i],
                                RowTypeName = itemNameAry[i],
                                OrgTotal = 0,
                                IsRation = "",
                                Number = new List<RationNumberViewModel>()
                            };
                            foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                            {
                                var numberData = new RationNumberViewModel
                                {
                                    PfaPerformanceID = pfaPerformance.ID,
                                    Number = 0,
                                };
                                tempRationDetail.Number.Add(numberData);
                            }
                            model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                        }
                    }
                }

                if (pfaCycle.PfaCycleRation.Any())
                {
                    var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == model.PfaOrgID);
                    if (pfaCycleRation != null)
                    {
                        decimal totalScore = 0;
                        int totalNum = 0;
                        var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                        for (int i = 0; i < itemAry.Length; i++)
                        {
                            totalScore = 0;
                            totalNum = 0;
                            var tempPfaCycleRationDetailList = model.PfaCycleRationData.PfaCycleRationDetail.Where(x => x.RowType == itemAry[i]).ToList();
                            foreach (var detail in tempPfaCycleRationDetailList)
                            {
                                detail.IsRation = pfaCycleRation.IsRation ? "Y" : "";
                                detail.OrgTotal = pfaCycleRation.OrgTotal;

                                if (detail.PfaDeptID.HasValue)
                                {
                                    var deptTotal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value);
                                    detail.OrgTotal = deptTotal;
                                }

                                if (model.PfaCycleRationData.PfaPerformance.Any())
                                {
                                    foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                    {
                                        var tempPfaCycleRationDetail = pfaCycleRationDetailList.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                        var numberData = detail.Number.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                        if (tempPfaCycleRationDetail != null && numberData != null)
                                        {

                                            switch (detail.RowType)
                                            {
                                                case "Ration":
                                                    if (tempPfaCycleRationDetail.Staffing.HasValue)
                                                    {
                                                        numberData.Number = tempPfaCycleRationDetail.Staffing.Value;
                                                        totalNum = totalNum + numberData.Number;
                                                    }
                                                    break;
                                                case "First":
                                                    if (detail.PfaDeptID.HasValue)
                                                    {
                                                        var firstFinal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value && x.FirstPerformance_ID == pfaPerformance.ID);
                                                        numberData.Number = firstFinal;
                                                        totalNum = totalNum + numberData.Number;
                                                    }
                                                    break;
                                                case "Last":
                                                case "Calculat":
                                                    var lastFinal = pfaCycleEmpList.Where(x => x.ID != model.PfaCycleEmpID)
                                                        .Count(x => x.LastPerformance_ID == pfaPerformance.ID);

                                                    if (model.LastPerformance_ID == pfaPerformance.ID)
                                                        lastFinal = lastFinal + 1;

                                                    numberData.Number = lastFinal;

                                                    if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                        totalScore = totalScore + 
                                                            (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);

                                                    totalNum = totalNum + numberData.Number;
                                                    break;
                                            }
                                        }
                                    }
                                    switch (detail.RowType)
                                    {
                                        case "Ration":
                                            detail.IsRation = "Y";
                                            break;
                                        case "First":
                                            detail.IsRation = "";
                                            break;
                                        case "Last":
                                        case "Calculat":
                                            if (totalNum == detail.OrgTotal)
                                            {
                                                if (totalScore <= detail.OrgTotal)
                                                {
                                                    detail.IsRation = "Y";
                                                }
                                                else
                                                {
                                                    detail.IsRation = "N";
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                var dataResult = new StringBuilder();
                dataResult.AppendLine("<table id='tableCheckData' class='table-bordered ui-jqgrid' style='width:100%'>");
                dataResult.AppendLine("<thead>");
                dataResult.AppendLine("<tr class='ui-jqgrid-labels' role='rowheader' style='height:40px;color:#777'>");
                dataResult.AppendLine("<th style='width:8%'><div id='jqgh_grid-table_code' style='text-align:center;'></div></th>");
                dataResult.AppendLine("<th style='width:8%'><div id='jqgh_grid-table_code' style='text-align:center;'>部門</div></th>");
                dataResult.AppendLine("<th style='width:8%'><div id='jqgh_grid-table_code' style='text-align:center;'>應配比人數</div></th>");
                if (model.PfaCycleRationData.PfaPerformance.Count > 0)
                {
                    foreach (var tempTh in model.PfaCycleRationData.PfaPerformance)
                    {
                        dataResult.AppendLine(string.Format("<th><div id='jqgh_grid-table_code' style='text-align:center;color:red;'>{0}<br>{1}<br>{2} %</div></th>", tempTh.Name, tempTh.Scores, tempTh.Rates));
                    }
                }
                dataResult.AppendLine("<th colspan='2' style='width:10%'><div id='jqgh_grid-table_code' style='text-align:center;'>配比正確</div></th>");
                dataResult.AppendLine("</tr>");
                dataResult.AppendLine("</thead>");
                dataResult.AppendLine("<tbody id='tbodyCheckData'>");
                if (model.PfaCycleRationData.PfaCycleRationDetail.Count() > 0)
                {
                    foreach (var detail in model.PfaCycleRationData.PfaCycleRationDetail)
                    {
                        dataResult.AppendLine(string.Format("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' id='{0}'>", detail.RowType));
                        dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>");
                        dataResult.AppendLine(string.Format("<span id='RowTypeName' name='RowTypeName'>{0}</span>", detail.RowTypeName));
                        dataResult.AppendLine("</td>");
                        dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>");
                        dataResult.AppendLine(string.Format("<span id='PfaDeptName' name='PfaDeptName'>{0}</span>", detail.PfaDeptName));
                        dataResult.AppendLine("</td>");
                        dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>");
                        dataResult.AppendLine(string.Format("<span id='OrgTotal' name='OrgTotal'>{0}</span>", detail.OrgTotal));
                        dataResult.AppendLine("</td>");
                        foreach (var number in detail.Number)
                        {
                            dataResult.AppendLine(string.Format("<td role='gridcell' style='white-space:normal;text-align:center' aria-describedby='grid-table_notification_count' id='{0}' name='RationNumber'>", number.PfaPerformanceID));
                            if (detail.RowType == "Calculat")
                                dataResult.AppendLine(string.Format("<input class='form-control calculatRationNumber' id='RationNumber' min='0' name='RationNumber' type='number' value='{0}'>", number.Number));
                            else
                            {
                                if (model.IsRatio == true && detail.RowType == "Last")
                                    dataResult.AppendLine(string.Format("<a href ='/DDMC_PFA/PfaCycleSecondEvaluation?txtPerformanceID={0}&cmd=Query'>{1}</a>", number.PfaPerformanceID, number.Number));
                                else
                                    dataResult.AppendLine(string.Format("<span id='RationNumber' name='RationNumber'>{0}</span>", number.Number));
                            }
                            dataResult.AppendLine("</td>");
                        }
                        if (detail.RowType == "Calculat")
                        {
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;width:5%;' aria-describedby='grid-table_notification_count'>");
                            dataResult.AppendLine("<button type='button' id='btnCalculat' class='btn btn-primary'>試算</button>");
                            dataResult.AppendLine("</td>");
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;width:5%;' aria-describedby='grid-table_notification_count'>");
                            dataResult.AppendLine(string.Format("<span id='CalculatIsRation' name='CalculatIsRation'>{0}</span>", detail.IsRation));
                            dataResult.AppendLine("</td>");
                        }
                        else if (detail.RowType == "Last")
                        {
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;width:5%;' aria-describedby='grid-table_notification_count'>");
                            dataResult.AppendLine("<button type='button' id='btnChangeRation' class='btn btn-primary'>更新配比</button>");
                            dataResult.AppendLine("</td>");
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center;width:5%;' aria-describedby='grid-table_notification_count'>");
                            dataResult.AppendLine(string.Format("<span id='IsRation' name='IsRation'>{0}</span>", detail.IsRation));
                            dataResult.AppendLine("</td>");
                        }
                        else
                        {
                            dataResult.AppendLine("<td role='gridcell' style='white-space:normal;text-align:center' aria-describedby='grid-table_notification_count' colspan='2'>");
                            dataResult.AppendLine(string.Format("<span id='IsRation' name='IsRation'>{0}</span>", detail.IsRation));
                            dataResult.AppendLine("</td>");
                        }
                        dataResult.AppendLine("</tr>");
                    }
                }
                dataResult.AppendLine("</tbody>");
                dataResult.AppendLine("</table>");

                return Json(new { success = true, message = "", data = dataResult.ToString(), IsRation = isRation });
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("試算績效等第配比失敗,Message:{0},{1}", ex.Message, ex.StackTrace));
                return Json(new { success = false, message = "試算績效等第配比失敗", data = "", IsRation = "" });
            }
        }
        #endregion

        #region 配比批次
        public ActionResult BatchQuery(
            Guid? pfaCycleID
            , string txtOrgIDSub = ""
            , string txtSortSub = ""
            , string txtDeptSortSub = ""
            , string cmdSub = "")
        {
            var model = new PfaCycleEmpBatchViewModel 
            {
                PfaCycleEmp = new List<PfaCycleEmpSignViewModel>()
            };

            try
            {
                #region 排序
                if (string.IsNullOrWhiteSpace(txtDeptSortSub))
                    txtDeptSortSub = "Y";
                ViewBag.txtDeptSortSub = txtDeptSortSub;
                if (string.IsNullOrWhiteSpace(txtSortSub))
                    txtSortSub = "FirstH";
                ViewBag.txtSortSub = txtSortSub;
                if (cmdSub == "Clear")
                    txtSortSub = "FirstH";
                ViewBag.SortSubList = GetSortList(txtSortSub);
                #endregion

                #region 績效考核批號
                var _pfaCycleList = Services.GetService<PfaCycleService>().GetAll()
                    .Where(x => x.CompanyID == CurrentUser.CompanyID
                        && x.Status == "a");

                ViewBag.PfaFormNoList = GetPfaFormNoList(_pfaCycleList.ToList()
                    , pfaCycleID.HasValue ? pfaCycleID.Value.ToString() : null);

                if (pfaCycleID.HasValue)
                {
                    _pfaCycleList = _pfaCycleList.Where(r => r.ID == pfaCycleID);
                }

                var pfaCycleList = _pfaCycleList
                    .OrderByDescending(x => x.PfaYear)
                    .ThenByDescending(x => x.PfaFormNo)
                    .ToList();


                var pfaCycle = pfaCycleList.FirstOrDefault();

                if (pfaCycle == null)
                    throw new Exception("無績效考核簽核批號資料");

                #endregion

                #region 考核組織

                model.PfaCycleID = pfaCycleList.FirstOrDefault().ID;

                var tempPfaOrg = Services.GetService<PfaSignProcessService>().GetAll()
                    .Where(x => 
                        x.PfaCycleEmp.IsRatio 
                        && x.PfaCycleEmp.PfaCycleID == pfaCycle.ID 
                        && x.IsRatio 
                        && x.IsSecondEvaluation == true
                    ).ToList();
                                                             
                // 該主管所有可評核績效考核組織
                var queryOrgData = Services.GetService<PfaOrgService>().GetAll()
                    .Where(x => 
                        x.CompanyID == CurrentUser.CompanyID
                    );

                if (!CurrentUser.IsAdmin)
                {
                    var pfaOrgIDs = tempPfaOrg.Where(x=> 
                        x.PreSignEmpID == CurrentUser.EmployeeID
                    ).Select(x => x.PfaCycleEmp.PfaOrgID)
                    .Distinct()
                    .ToList();

                    queryOrgData = queryOrgData.Where(x => pfaOrgIDs.Contains(x.ID));
                }
                else {
                    var pfaOrgIDs = tempPfaOrg.Select(x => x.PfaCycleEmp.PfaOrgID).Distinct().ToList();
                    queryOrgData = queryOrgData.Where(x => pfaOrgIDs.Contains(x.ID));
                }
                
                var orgList = queryOrgData.OrderBy(x => x.Ordering).ThenBy(x => x.PfaOrgCode).ToList();

                Guid selectedDataID = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(txtOrgIDSub) && cmdSub != "Clear")
                    selectedDataID = Guid.Parse(txtOrgIDSub);
                else {
                    if (orgList.Any())
                        selectedDataID = orgList.FirstOrDefault().ID;
                }
                model.PfaOrgID = selectedDataID;
                ViewBag.txtOrgIDSub = model.PfaOrgID.ToString();

                var listItem = new List<SelectListItem>();
                if (orgList.Any())
                {
                    foreach (var org in orgList)
                        listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", org.PfaOrgCode, org.PfaOrgName), Value = org.ID.ToString(), Selected = (selectedDataID == org.ID ? true : false) });
                }
                ViewBag.OrgSubList = listItem;
                #endregion

                #region 評等人數配比資料
                if (orgList.Any())
                {
                    model.PfaCycleRationData = new PfaCycleRationDataViewModel
                    {
                        FirstDept = 0,
                        PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>()
                    };
                    string[] itemAry = { "Ration", "First", "Last", "Calculat" };
                    string[] itemNameAry = { "配比", "初核", "複核", "試算" };

                    var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaOrgID == model.PfaOrgID &&
                                                                                                        x.PfaCycleID == model.PfaCycleID && x.IsRatio).ToList();
                    var pfaDeptIDs = pfaCycleEmpList.Select(x => x.PfaDeptID).Distinct().ToList();
                    var pfaDeptList = Services.GetService<PfaDeptService>().GetAll().Where(x => pfaDeptIDs.Contains(x.ID)).OrderBy(x => x.PfaDeptCode).ToList();
                    model.PfaCycleRationData.FirstDept = pfaDeptList.Count();

                    bool pfaCycleRationFlag = false;
                    if (pfaCycle.PfaCycleRation.Any())
                    {
                        var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == model.PfaOrgID);
                        if (pfaCycleRation != null)
                        {
                            pfaCycleRationFlag = true;
                            var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                            model.PfaCycleRationData.PfaPerformance = pfaCycleRationDetailList.Select(x =>
                            {
                                var tempScores = "";
                                if (x.ScoresStart.HasValue)
                                    tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                                if (x.ScoresEnd.HasValue)
                                    tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                                return new PfaPerformanceViewModel
                                {
                                    ID = x.PfaPerformanceID,
                                    Code = x.Code,
                                    Name = x.Name,
                                    Scores = tempScores,
                                    Rates = x.Rates,
                                };
                            }).ToList();
                        }
                    }
                    if (pfaCycleRationFlag == false)
                    {
                        model.PfaCycleRationData.PfaPerformance = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed).OrderBy(x => x.Ordering).ToList().Select(x =>
                        {
                            var tempScores = "";
                            if (x.ScoresStart.HasValue)
                                tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                            if (x.ScoresEnd.HasValue)
                                tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                            return new PfaPerformanceViewModel
                            {
                                ID = x.ID,
                                Code = x.Code,
                                Name = x.Name,
                                Scores = tempScores,
                                Multiplier = x.Multiplier,
                            };
                        }).ToList();
                    }

                    if (model.PfaCycleRationData.PfaPerformance.Any())
                    {
                        for (int i = 0; i < itemAry.Length; i++)
                        {
                            if (itemAry[i] == "First")
                            {
                                foreach (var pfaDept in pfaDeptList)
                                {
                                    var tempRationDetail = new PfaCycleRationDetailDataViewModel
                                    {
                                        RowType = itemAry[i],
                                        RowTypeName = itemNameAry[i],
                                        PfaDeptID = pfaDept.ID,
                                        PfaDeptName = pfaDept.PfaDeptName,
                                        OrgTotal = 0,
                                        IsRation = "",
                                        Number = new List<RationNumberViewModel>()
                                    };
                                    foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                    {
                                        var numberData = new RationNumberViewModel
                                        {
                                            PfaPerformanceID = pfaPerformance.ID,
                                            Number = 0,
                                        };
                                        tempRationDetail.Number.Add(numberData);
                                    }
                                    model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                                }
                            }
                            else
                            {
                                var tempRationDetail = new PfaCycleRationDetailDataViewModel
                                {
                                    RowType = itemAry[i],
                                    RowTypeName = itemNameAry[i],
                                    OrgTotal = 0,
                                    IsRation = "",
                                    Number = new List<RationNumberViewModel>()
                                };
                                foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                {
                                    var numberData = new RationNumberViewModel
                                    {
                                        PfaPerformanceID = pfaPerformance.ID,
                                        Number = 0,
                                    };
                                    tempRationDetail.Number.Add(numberData);
                                }
                                model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                            }
                        }
                    }

                    if (pfaCycle.PfaCycleRation.Any())
                    {
                        var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == model.PfaOrgID);
                        if (pfaCycleRation != null)
                        {
                            decimal totalScore = 0;
                            int totalNum = 0;
                            var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                            for (int i = 0; i < itemAry.Length; i++)
                            {
                                totalScore = 0;
                                totalNum = 0;
                                var tempPfaCycleRationDetailList = model.PfaCycleRationData.PfaCycleRationDetail.Where(x => x.RowType == itemAry[i]).ToList();
                                foreach (var detail in tempPfaCycleRationDetailList)
                                {
                                    detail.IsRation = pfaCycleRation.IsRation ? "Y" : "";
                                    detail.OrgTotal = pfaCycleRation.OrgTotal;

                                    if (detail.PfaDeptID.HasValue)
                                    {
                                        var deptTotal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value);
                                        detail.OrgTotal = deptTotal;
                                    }

                                    if (model.PfaCycleRationData.PfaPerformance.Any())
                                    {
                                        foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                        {
                                            var tempPfaCycleRationDetail = pfaCycleRationDetailList.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                            var numberData = detail.Number.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                            if (tempPfaCycleRationDetail != null && numberData != null)
                                            {
                                                switch (detail.RowType)
                                                {
                                                    case "Ration":
                                                        if (tempPfaCycleRationDetail.Staffing.HasValue)
                                                        {
                                                            numberData.Number = tempPfaCycleRationDetail.Staffing.Value;
                                                            totalNum = totalNum + numberData.Number;
                                                        }
                                                        break;
                                                    case "First":
                                                        if (detail.PfaDeptID.HasValue)
                                                        {
                                                            var firstFinal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value && x.FirstPerformance_ID == pfaPerformance.ID);
                                                            numberData.Number = firstFinal;
                                                            totalNum = totalNum + numberData.Number;
                                                        }
                                                        break;
                                                    case "Last":
                                                    case "Calculat":
                                                        if (tempPfaCycleRationDetail.SecondFinal.HasValue)
                                                        {
                                                            numberData.Number = tempPfaCycleRationDetail.SecondFinal.Value;
                                                            if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                                totalScore = totalScore + (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);
                                                            totalNum = totalNum + numberData.Number;
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        switch (detail.RowType)
                                        {
                                            case "Ration":
                                                detail.IsRation = "Y";
                                                break;
                                            case "First":
                                                detail.IsRation = "";
                                                break;
                                            case "Last":
                                            case "Calculat":
                                                if (totalNum == detail.OrgTotal)
                                                {
                                                    if (totalScore <= detail.OrgTotal)
                                                    {
                                                        detail.IsRation = "Y";
                                                    }
                                                    else
                                                    {
                                                        detail.IsRation = "N";
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    model.PfaCycleRationData = new PfaCycleRationDataViewModel
                    {
                        FirstDept = 0,
                        PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>(),
                        PfaPerformance = new List<PfaPerformanceViewModel>(),
                    };
                }
                #endregion

                #region 考核人員資料
                var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                    .Where(x => 
                        x.PfaCycleEmp.PfaFirstScore.HasValue 
                        && (x.Status == PfaSignProcess_Status.PendingReview
                            || x.Status == PfaSignProcess_Status.Reviewed
                            || x.Status == PfaSignProcess_Status.ReturnedForModification
                            || x.Status == PfaSignProcess_Status.Returned) 
                        && x.PfaCycleEmp.PfaCycleID == model.PfaCycleID 
                        && (
                            (x.IsSecondEvaluation == true && x.IsFirstEvaluation == false)
                            ||
                            (x.IsSecondEvaluation == true && x.IsFirstEvaluation == true)
                        )
                        && x.PfaCycleEmp.Status != "y" 
                        && x.IsRatio 
                        && x.PfaCycleEmp.IsRatio
                    );
                if (!CurrentUser.IsAdmin)
                    queryData = queryData.Where(x => x.PreSignEmpID == CurrentUser.EmployeeID);
                queryData = queryData.Where(x => x.PfaCycleEmp.PfaOrgID == model.PfaOrgID);

                var pfaOptionList = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignStatus").ToList();
                var tempQueryData = queryData.Select(x => new PfaCycleEmpSignViewModel()
                {
                    PfaSignProcessID = x.ID,
                    PfaCycleID = x.PfaCycleEmp.PfaCycleID,
                    PfaOrgCode = x.PfaCycleEmp.PfaOrg.PfaOrgCode,
                    PfaOrgName = x.PfaCycleEmp.PfaOrg.PfaOrgName,
                    PfaDeptID = x.PfaCycleEmp.PfaDeptID,
                    PfaDeptCode = x.PfaCycleEmp.PfaDept.PfaDeptCode,
                    PfaDeptName = x.PfaCycleEmp.PfaDept.PfaDeptName,
                    EmployeeID = x.PfaCycleEmpID,
                    EmployeeNo = x.PfaCycleEmp.Employees.EmployeeNO,
                    EmployeeName = x.PfaCycleEmp.Employees.EmployeeName,
                    PfaSelfScore = x.PfaCycleEmp.PfaSelfScore,
                    PfaFirstScore = x.PfaCycleEmp.PfaFirstScore,
                    PfaLastScore = x.PfaCycleEmp.PfaLastScore,
                    LastPerformance_ID = x.PfaCycleEmp.LastPerformance_ID,
                    SignStatus = x.Status,
                });

                if (txtDeptSortSub == "Y" && txtSortSub != "ALL")
                {
                    switch (txtSortSub)
                    {
                        case "FirstH": //初核分數降冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenByDescending(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                            break;
                        case "FirstL": //初核分數升冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                            break;
                        case "SecondH": //複核分數降冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenByDescending(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                            break;
                        case "SecondL": //複核分數升冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                            break;
                    }
                }
                else if (txtDeptSortSub == "Y" && txtSortSub == "ALL")
                    tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaOrgCode).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.EmployeeNo);
                else if (txtDeptSortSub == "N" && txtSortSub != "ALL")
                {
                    switch (txtSortSub)
                    {
                        case "FirstH": //初核分數降冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenByDescending(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                            break;
                        case "FirstL": //初核分數升冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaFirstScore).ThenBy(x => x.EmployeeNo);
                            break;
                        case "SecondH": //複核分數降冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenByDescending(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                            break;
                        case "SecondL": //複核分數升冪
                            tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaLastScore).ThenBy(x => x.EmployeeNo);
                            break;
                    }
                }
                else
                    tempQueryData = tempQueryData.OrderBy(x => x.PfaCycleID).ThenBy(x => x.EmployeeNo);

                model.PfaCycleEmp = tempQueryData.ToList();

                var pfaPerformanceList = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID).ToList();
                foreach (var item in model.PfaCycleEmp)
                {
                    var pfaOption = pfaOptionList.FirstOrDefault(y => y.OptionCode == item.SignStatus);
                    var pfaPerformance = pfaPerformanceList.FirstOrDefault(y => y.ID == item.LastPerformance_ID);
                    item.PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "";
                    item.LastPerformanceName = pfaPerformance != null ? pfaPerformance.Name : "";
                    item.StrSignStatus = pfaOption != null ? pfaOption.OptionName : "";
                }
                #endregion
            }
            catch (Exception)
            {
                model = new PfaCycleEmpBatchViewModel
                {
                    PfaCycleEmp = new List<PfaCycleEmpSignViewModel>()
                };
                model.PfaCycleRationData = new PfaCycleRationDataViewModel
                {
                    FirstDept = 0,
                    PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>()
                };
                ViewBag.txtSortSub = txtSortSub;
                ViewBag.SortSubList = GetSortList(txtSortSub);
                ViewBag.txtOrgIDSub = txtOrgIDSub;
                ViewBag.OrgSubList = new List<SelectListItem>();
            }
            return View("_Batch", model);
        }

        [HttpPost]
        public ActionResult BatchConfirm(PfaCycleEmpBatchViewModel model)
        {
            var result = new Result();
            string cmd = "btnSave";
            var errMsgStr = new StringBuilder();
            var successFlag = false;
            try
            {
                if (!model.PfaCycleID.HasValue)
                {
                    WriteLog("BatchConfirm:查無績效考核批號");
                    return Json(new { success = false, message = "查無績效考核批號" });
                }

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(model.PfaCycleID.Value);
                if (pfaCycle == null)
                {
                    WriteLog(string.Format("績效考核批號資料不存在,ID:{0}", model.PfaCycleID));
                    return Json(new { success = false, message = "績效考核批號資料不存在" });
                }

                if (model.PfaCycleEmp.Count() == 0)
                {
                    WriteLog("請選擇要調整的員工資料");
                    return Json(new { success = false, message = "請選擇要調整的員工資料" });
                }

                foreach (var item in model.PfaCycleEmp)
                {
                    var errMsg = "";
                    try 
                    {
                        var pfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x => x.ID == item.EmployeeID);
                        if (pfaCycleEmp == null)
                            throw new Exception("績效考核員工資料取得失敗");

                        errMsg = pfaCycleEmp.Employees.EmployeeName;
                        var pfaSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.ID == item.PfaSignProcessID);
                        var detail = GetDetail(pfaSignProcess, pfaCycle);
                        detail.PfaLastScore = item.PfaLastScore;

                        var checkResult = CheckPfaCycleEmpSign(detail, cmd);
                        if (!checkResult.success)
                            throw new Exception(string.Format("{0}編輯錯誤:{1}", errMsg, checkResult.message));

                        pfaCycleEmp.PfaLastScore = detail.PfaLastScore;
                        pfaCycleEmp.LastPerformance_ID = detail.LastPerformance_ID;
                        pfaCycleEmp.ModifiedBy = CurrentUser.EmployeeID;
                        pfaCycleEmp.ModifiedTime = DateTime.Now;

                        pfaSignProcess.Status = detail.SignStatus;

                        var updateResult = Services.GetService<PfaSignProcessService>().UpdateSecondEvaluationData(pfaSignProcess, pfaCycleEmp, new List<PfaEmpIndicator>(), new List<PfaEmpAbility>(), cmd);
                        if (!updateResult.success)
                            throw new Exception(string.Format("{0}編輯錯誤:{1}", errMsg, updateResult.message));
                        else
                        {
                            successFlag = true;
                            errMsg = "";
                        }
                    }
                    catch(Exception ex)
                    {
                        WriteLog(ex.Message);
                        errMsgStr.AppendLine(ex.Message);
                    }
                }
                if (errMsgStr.Length > 0)
                    throw new Exception(errMsgStr.ToString());
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                return Json(new { success = successFlag, message = ex.Message });
            }
            return Json(new { success = successFlag, message = "配比批次調整成功" });
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
                PfaLastScore = pfaSignProcess.PfaCycleEmp.PfaLastScore,
                ManagerAbility = pfaSignProcess.PfaCycleEmp.ManagerAbility,
                ManagerIndicator = pfaSignProcess.PfaCycleEmp.ManagerIndicator,
                PfaSelfScore = pfaSignProcess.PfaCycleEmp.PfaSelfScore,
                SelfIndicator = pfaSignProcess.PfaCycleEmp.SelfIndicator,
                EditFirstEvaluation = pfaSignProcess.IsFirstEvaluation,
                IsRatio = pfaSignProcess.PfaCycleEmp.IsRatio,
            };

            if (model.PfaOrgID.HasValue)
                model.PfaOrgName = pfaSignProcess.PfaCycleEmp.PfaOrg.PfaOrgName;

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
                var pfaEmpAbilityList = pfaSignProcess.PfaCycleEmp.PfaEmpAbility.OrderBy(x => x.Ordering).ToList();
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
                        PfaEmpAbilityDetail = new List<PfaEmpAbilityDetailViewModel>()
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
                            ScoreInterval = new List<int>()
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

            #region 評等人數配比資料
            model.PfaCycleRationData = new PfaCycleRationDataViewModel
            {
                FirstDept = 0,
                PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>()
            };
            string[] itemAry = { "Ration", "First", "Last", "Calculat" };
            string[] itemNameAry = { "配比", "初核", "複核", "試算" };

            var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaOrgID == pfaSignProcess.PfaCycleEmp.PfaOrgID && 
                                                                                                x.PfaCycleID == model.PfaCycleID && x.IsRatio).ToList();
            var pfaDeptIDs = pfaCycleEmpList.Select(x => x.PfaDeptID).Distinct().ToList();
            var pfaDeptList = Services.GetService<PfaDeptService>().GetAll().Where(x => pfaDeptIDs.Contains(x.ID)).OrderBy(x=> x.PfaDeptCode).ToList();
            model.PfaCycleRationData.FirstDept = pfaDeptList.Count();

            bool pfaCycleRationFlag = false;
            if (pfaCycle.PfaCycleRation.Any())
            {
                var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == pfaSignProcess.PfaCycleEmp.PfaOrgID);
                if (pfaCycleRation != null)
                {
                    pfaCycleRationFlag = true;
                    var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                    model.PfaCycleRationData.PfaPerformance = pfaCycleRationDetailList.Select(x =>
                    {
                        var tempScores = "";
                        if (x.ScoresStart.HasValue)
                            tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                        if (x.ScoresEnd.HasValue)
                            tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                        return new PfaPerformanceViewModel
                        {
                            ID = x.PfaPerformanceID,
                            Code = x.Code,
                            Name = x.Name,
                            Scores = tempScores,
                            Rates = x.Rates,
                        };
                    }).ToList();
                }
            }
            if(pfaCycleRationFlag == false) 
            {
                model.PfaCycleRationData.PfaPerformance = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed).OrderBy(x => x.Ordering).ToList().Select(x =>
                {
                    var tempScores = "";
                    if (x.ScoresStart.HasValue)
                        tempScores = tempScores + double.Parse(x.ScoresStart.Value.ToString());
                    if (x.ScoresEnd.HasValue)
                        tempScores = tempScores + '~' + double.Parse(x.ScoresEnd.Value.ToString());
                    return new PfaPerformanceViewModel
                    {
                        ID = x.ID,
                        Code = x.Code,
                        Name = x.Name,
                        Scores = tempScores,
                        Multiplier = x.Multiplier,
                    };
                }).ToList();
            }

            if (model.PfaCycleRationData.PfaPerformance.Any())
            {
                for (int i = 0; i < itemAry.Length; i++)
                {
                    if (itemAry[i] == "First")
                    {
                        foreach (var pfaDept in pfaDeptList)
                        {
                            var tempRationDetail = new PfaCycleRationDetailDataViewModel
                            {
                                RowType = itemAry[i],
                                RowTypeName = itemNameAry[i],
                                PfaDeptID = pfaDept.ID,
                                PfaDeptName = pfaDept.PfaDeptName,
                                OrgTotal = 0,
                                IsRation = "",
                                Number = new List<RationNumberViewModel>()
                            };
                            foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                            {
                                var numberData = new RationNumberViewModel
                                {
                                    PfaPerformanceID = pfaPerformance.ID,
                                    Number = 0,
                                };
                                tempRationDetail.Number.Add(numberData);
                            }
                            model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                        }
                    }
                    else
                    {
                        var tempRationDetail = new PfaCycleRationDetailDataViewModel
                        {
                            RowType = itemAry[i],
                            RowTypeName = itemNameAry[i],
                            OrgTotal = 0,
                            IsRation = "",
                            Number = new List<RationNumberViewModel>()
                        };
                        foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                        {
                            var numberData = new RationNumberViewModel
                            {
                                PfaPerformanceID = pfaPerformance.ID,
                                Number = 0,
                            };
                            tempRationDetail.Number.Add(numberData);
                        }
                        model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                    }
                }
            }

            if (pfaCycle.PfaCycleRation.Any())
            {
                var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == pfaSignProcess.PfaCycleEmp.PfaOrgID);
                if (pfaCycleRation != null)
                {
                    decimal totalScore = 0;
                    int totalNum = 0;
                    var pfaCycleRationDetailList = pfaCycleRation.PfaCycleRationDetail.OrderBy(x => x.Code).ToList();
                    for (int i = 0; i < itemAry.Length; i++)
                    {
                        totalScore = 0;
                        totalNum = 0;
                        var tempPfaCycleRationDetailList = model.PfaCycleRationData.PfaCycleRationDetail.Where(x => x.RowType == itemAry[i]).ToList();
                        foreach (var detail in tempPfaCycleRationDetailList)
                        {
                            detail.IsRation = pfaCycleRation.IsRation ? "Y" : "";
                            detail.OrgTotal = pfaCycleRation.OrgTotal;

                            if (detail.PfaDeptID.HasValue)
                            {
                                var deptTotal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value);
                                detail.OrgTotal = deptTotal;
                            }

                            if (model.PfaCycleRationData.PfaPerformance.Any())
                            {
                                foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                                {
                                    var tempPfaCycleRationDetail = pfaCycleRationDetailList.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                    var numberData = detail.Number.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                    if (tempPfaCycleRationDetail != null && numberData != null)
                                    {

                                        switch (detail.RowType)
                                        {
                                            case "Ration":
                                                if (tempPfaCycleRationDetail.Staffing.HasValue)
                                                {
                                                    numberData.Number = tempPfaCycleRationDetail.Staffing.Value;
                                                    totalNum = totalNum + numberData.Number;
                                                }
                                                break;
                                            case "First":
                                                if (detail.PfaDeptID.HasValue)
                                                {
                                                    var firstFinal = pfaCycleEmpList.Count(x => x.PfaDeptID == detail.PfaDeptID.Value && x.FirstPerformance_ID == pfaPerformance.ID);
                                                    numberData.Number = firstFinal;
                                                    totalNum = totalNum + numberData.Number;
                                                }
                                                break;
                                            case "Last":
                                            case "Calculat":
                                                var lastFinal = pfaCycleEmpList.Count(x => x.LastPerformance_ID == pfaPerformance.ID);
                                                numberData.Number = lastFinal;
                                                if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                    totalScore = totalScore + (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);
                                                totalNum = totalNum + numberData.Number;
                                                break;
                                        }
                                    }
                                }
                                switch (detail.RowType)
                                {
                                    case "Ration":
                                        detail.IsRation = "Y";
                                        break;
                                    case "First":
                                        detail.IsRation = "";
                                        break;
                                    case "Last":
                                    case "Calculat":
                                        if (totalNum == detail.OrgTotal)
                                        {
                                            if (totalScore <= detail.OrgTotal)
                                            {
                                                detail.IsRation = "Y";
                                            }
                                            else
                                            {
                                                detail.IsRation = "N";
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 簽核記錄
            model.PfaSignRecord = HRPortal.Common.PFA.PfaSignRecord.GePfaSignRecord(pfaSignProcess, model);
            #endregion

            return model;
        }
        #endregion

        #region 初始值
        private void GetDefaultData(string txtOrgID = "", string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", string txtStatus = "", string txtPerformanceID = "", string txtSort = "", string txtNoRatio = "", string txtDeptSort = "")
        {
            var orgList = GetOrgList(txtOrgID);
            ViewBag.OrgList = orgList;
            ViewBag.txtOrgID = txtOrgID;
            ViewBag.DepartmentList = GetDepartmentList(txtDepartmentID, orgList);
            ViewBag.txtDepartmentID = txtDepartmentID;
            ViewBag.txtEmployeeNo = txtEmployeeNo;
            ViewBag.txtEmployeeName = txtEmployeeName;
            ViewBag.StatusList = Common.PFA.PfaOption.GetPfaSignStatusOption(txtStatus, flag: true, exclude: "t");
            ViewBag.txtStatus = txtStatus;
            ViewBag.PerformanceList = GetPerformanceList(txtPerformanceID);
            ViewBag.txtPerformanceID = txtPerformanceID;
            ViewBag.txtSort = txtSort;
            ViewBag.SortList = GetSortList(txtSort);
            ViewBag.txtNoRatio = string.IsNullOrWhiteSpace(txtNoRatio) ? "N" : txtNoRatio;
            ViewBag.txtDeptSort = string.IsNullOrWhiteSpace(txtDeptSort) ? "Y" : txtDeptSort;
        }
        #endregion

        #region 下拉
        [HttpPost]
        public ActionResult GetDepartmentOptions(string selectedData)
        {
            var sb = new StringBuilder();
            var pfaDeptIDs = new List<Guid>();

            try
            {
                sb.AppendFormat("<option value=\"{0}\" selected>{1}</option>", "", "請選擇");
                //轉為Guid 判斷ID
                Guid SelectedDataID = Guid.Empty;

                if (!string.IsNullOrWhiteSpace(selectedData))
                {
                    SelectedDataID = Guid.Parse(selectedData);
                    pfaDeptIDs = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrg.CompanyID == CurrentUser.CompanyID && x.PfaOrgID == SelectedDataID).Select(x => x.PfaDeptID).Distinct().ToList();
                }
                else
                {
                    var orgList = GetOrgList(string.Empty);
                    if (!orgList.Any())
                        throw new Exception("查無組織資料");

                    var orgIDs = orgList.Where(x => x.Value != "").Select(x => Guid.Parse(x.Value)).ToList();
                    pfaDeptIDs = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrg.CompanyID == CurrentUser.CompanyID && orgIDs.Contains(x.PfaOrgID)).Select(x => x.PfaDeptID).Distinct().ToList();
                }
                var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && pfaDeptIDs.Contains(x.ID) && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();
                foreach (var item in data)
                    sb.AppendFormat("<option value=\"{0}\">{1} {2}</option>", item.ID.ToString(), item.PfaDeptCode, item.PfaDeptName);
            }
            catch(Exception)
            {
                sb.Length = 0;
                sb.AppendFormat("<option value=\"{0}\" selected>{1}</option>", "", "請選擇");
            }
            return Content(sb.ToString());
        }

        private List<SelectListItem> GetOrgList(string selectedData, bool flag = true)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            if (flag)
                listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            try
            {
                //轉為Guid 判斷ID
                Guid SelectedDataID = Guid.Empty;

                if (!string.IsNullOrWhiteSpace(selectedData))
                    SelectedDataID = Guid.Parse(selectedData);

                var pfaCycleList = Services.GetService<PfaCycleService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.Status == "a").ToList()
                                                                                  .OrderByDescending(x => x.PfaYear).ToList();
                var pfaCycleId = pfaCycleList.FirstOrDefault().ID.ToString();
                var pfaCycle = pfaCycleList.FirstOrDefault();
                if (pfaCycle == null)
                    return listItem;
                var pfaOrgIDs = Services.GetService<PfaSignProcessService>().GetAll().Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycle.ID &&
                                                                                                 x.IsSecondEvaluation == true &&
                                                                                                 x.PreSignEmpID == CurrentUser.EmployeeID)
                                                                                     .Select(x => x.PfaCycleEmp.PfaOrgID).Distinct().ToList();
                // 該主管所有可評核績效考核組織
                var queryData = Services.GetService<PfaOrgService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID);
                if (!CurrentUser.IsAdmin)
                    queryData = queryData.Where(x => pfaOrgIDs.Contains(x.ID));

                var data = queryData.OrderBy(x => x.Ordering).ThenBy(x=> x.PfaOrgCode).ToList();

                foreach (var item in data)
                    listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.PfaOrgCode, item.PfaOrgName), Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            catch {
                listItem = new List<SelectListItem>();
            }
            return listItem;
        }

        /// <summary>
        ///  取得部門選單
        /// </summary>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selectedData, List<SelectListItem> orgList)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            if (!orgList.Any())
                return listItem;

            var orgSelectedData = orgList.FirstOrDefault(x => x.Selected);
            var orgIDs = orgList.Where(x=> x.Value != "").Select(x => Guid.Parse(x.Value)).ToList();
            if (orgSelectedData.Value != "")
                orgIDs = orgList.Where(x => x.Value == orgSelectedData.Value).Select(x => Guid.Parse(x.Value)).ToList();
            var pfaDeptIDs = Services.GetService<PfaOrgDeptService>().GetAll().Where(x => x.PfaOrg.CompanyID == CurrentUser.CompanyID && orgIDs.Contains(x.PfaOrgID)).Select(x=> x.PfaDeptID).Distinct().ToList();
            var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && pfaDeptIDs.Contains(x.ID) && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();

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
        /// <param name="datas"></param>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetPfaFormNoList(List<PfaCycle> datas, string selectedData)
        {
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            var listItem = new List<SelectListItem>();
            foreach (var item in datas.OrderByDescending(r=>r.PfaYear).ThenByDescending(r=>r.PfaFormNo))
                listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            return listItem;
        }

        /// <summary>
        /// 取得績效等第
        /// </summary>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetPerformanceList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            var data = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.IsUsed).OrderBy(x => x.Ordering).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = item.Name, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });

            return listItem;
        }

        private List<SelectListItem> GetSortList(string selectedData)
        {
            List <SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "空白", Value = "ALL", Selected = (selectedData == "ALL" ? true : false) });
            listItem.Add(new SelectListItem { Text = "初核分數降冪", Value = "FirstH", Selected = (selectedData == "FirstH" ? true : false) });
            listItem.Add(new SelectListItem { Text = "初核分數升冪", Value = "FirstL", Selected = (selectedData == "FirstL" ? true : false) });
            listItem.Add(new SelectListItem { Text = "複核分數降冪", Value = "SecondH", Selected = (selectedData == "SecondH" ? true : false) });
            listItem.Add(new SelectListItem { Text = "複核分數升冪", Value = "SecondL", Selected = (selectedData == "SecondL" ? true : false) });
            return listItem;
        }
        #endregion
    }
}
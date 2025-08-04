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
    public class PfaCycleSelfEvaluationController : BaseController
    {
        #region 查詢
        public ActionResult Index(string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", int page = 1, string cmd = "")
        {
            GetDefaultData(txtDepartmentID, txtEmployeeNo, txtEmployeeName);

            int currentPage = page < 1 ? 1 : page;

            //  m: 未收件 c:待評核 a:已評核 e: 已送出 r:退回修改
            string[] status = { "c", "a", "e", "r" };

            var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                                    .Where(x => status.Contains(x.Status) && x.IsSelfEvaluation == true && x.PfaCycleEmp.Status != "y");

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

            var pfaOptionList = Services.GetService<PfaOptionService>().GetAll().Where(x=> x.PfaOptionGroup.OptionGroupCode == "SignStatus").ToList();
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
                SignStatus = x.Status,
            }).OrderBy(x=> x.PfaCycleID).ThenBy(x=> x.PfaDeptCode).ThenBy(x => x.EmployeeNo).ToList().ToPagedList(currentPage, currentPageSize);

            var pfaCycleIDs = viewModel.Select(x => x.PfaCycleID).Distinct().ToList();
            var pfaCycleList = Services.GetService<PfaCycleService>().GetAll().Where(x => pfaCycleIDs.Contains(x.ID)).ToList();
            foreach (var item in viewModel)
            {
                var pfaCycle = pfaCycleList.FirstOrDefault(y => y.ID == item.PfaCycleID);
                var pfaOption = pfaOptionList.FirstOrDefault(y => y.OptionCode == item.SignStatus);
                item.PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "";
                item.StrSignStatus = pfaOption != null ? pfaOption.OptionName : "";
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtDepartmentID, string txtEmployeeNo, string txtEmployeeName, string btnQuery, string btnClear)
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
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtDepartmentID, txtEmployeeNo, txtEmployeeName);

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

                pfaCycleEmp.SelfIndicator = model.SelfIndicator;
                pfaCycleEmp.PfaSelfScore = model.PfaSelfScore;
                pfaCycleEmp.SelfAppraisal = model.SelfAppraisal;
                pfaCycleEmp.ModifiedBy = CurrentUser.EmployeeID;
                pfaCycleEmp.ModifiedTime = DateTime.Now;

                #region 績效考核員工工作績效
                var pfaEmpIndicatorList = new List<PfaEmpIndicator>();
                var pfaIndicatorDesc = new StringBuilder();
                var pfaIndicatorList = Services.GetService<PfaIndicatorService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true).OrderBy(x => x.Ordering).ToList();
                foreach (var pfaIndicator in pfaIndicatorList)
                {
                    pfaIndicatorDesc.Length = 0;
                    var pfaIndicatorDetailList = pfaIndicator.PfaIndicatorDetail.OrderBy(x => x.Ordering).ToList();
                    foreach (var pfaIndicatorDetail in pfaIndicatorDetailList)
                        pfaIndicatorDesc.Append(string.Format("{0}.{1} {2}-{3}", pfaIndicatorDetail.Ordering, pfaIndicatorDetail.PfaIndicatorKey, pfaIndicatorDetail.UpperLimit, pfaIndicatorDetail.LowerLimit) + "\r\n");

                    var tempPfaEmpIndicator = model.PfaEmpIndicator.FirstOrDefault(x => x.PfaIndicatorCode == pfaIndicator.PfaIndicatorCode);
                    var pfaEmpIndicator = new PfaEmpIndicator
                    {
                        PfaCycleEmpID = pfaCycleEmp.ID,
                        PfaIndicatorCode = pfaIndicator.PfaIndicatorCode,
                        PfaIndicatorName = pfaIndicator.PfaIndicatorName,
                        Description = pfaIndicator.Description,
                        Scale = pfaIndicator.Scale,
                        Ordering = pfaIndicator.Ordering,
                        PfaIndicatorDesc = pfaIndicatorDesc.ToString(),
                        CreatedBy = CurrentUser.EmployeeID,
                        CreatedTime = DateTime.Now,
                        ModifiedBy = CurrentUser.EmployeeID,
                        ModifiedTime = DateTime.Now
                    };
                    if (tempPfaEmpIndicator != null)
                    {
                        pfaEmpIndicator.ID = tempPfaEmpIndicator.ID.HasValue ? tempPfaEmpIndicator.ID.Value : Guid.NewGuid();
                        if (tempPfaEmpIndicator.SelfIndicator.HasValue)
                            pfaEmpIndicator.SelfIndicator = tempPfaEmpIndicator.SelfIndicator.Value;
                    }
                    else {
                        pfaEmpIndicator.ID = Guid.NewGuid();
                    }
                    pfaEmpIndicatorList.Add(pfaEmpIndicator);
                }
                #endregion

                #region 績效考核員工訓練紀錄
                var pfaEmpTrainingList = new List<PfaEmpTraining>();
                foreach (var pfaEmpTraining in model.PfaEmpTraining)
                {
                    var tempPfaEmpTraining = new PfaEmpTraining
                    {
                        ID = pfaEmpTraining.ID.HasValue ? pfaEmpTraining.ID.Value : Guid.NewGuid(),
                        PfaCycleEmpID = pfaCycleEmp.ID,
                        CoursesCode = pfaEmpTraining.CoursesCode,
                        CoursesName = pfaEmpTraining.CoursesName,
                        CreatedBy = CurrentUser.EmployeeID,
                        CreatedTime = DateTime.Now,
                        ModifiedBy = CurrentUser.EmployeeID,
                        ModifiedTime = DateTime.Now
                    };
                    if (pfaEmpTraining.TrainingHours.HasValue)
                        tempPfaEmpTraining.TrainingHours = (decimal)pfaEmpTraining.TrainingHours.Value;
                    pfaEmpTrainingList.Add(tempPfaEmpTraining);
                }
                #endregion

                pfaSignProcess.Status = model.SignStatus;
                if (cmd == "btnSent")
                    pfaSignProcess.SignEmpID = CurrentUser.EmployeeID;
                result = Services.GetService<PfaSignProcessService>().UpdateSelfEvaluationData(pfaSignProcess, pfaCycleEmp, pfaEmpIndicatorList, pfaEmpTrainingList, cmd);
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

                if (cmd == "btnSent")
                    model.SignStatus = "e"; // e: 已送出
                else
                    model.SignStatus = model.SignStatus == "c" ? "a" : model.SignStatus; // c:待評核 a:已評核

                string pattern = @"^[0-9]+(.[0-9]{1})?$";
                #region 績效考核員工工作績效
                foreach (var pfaEmpIndicator in model.PfaEmpIndicator)
                {
                    if (pfaEmpIndicator.SelfIndicator.HasValue)
                    {
                        if (pfaEmpIndicator.SelfIndicator < 0 || pfaEmpIndicator.SelfIndicator > pfaEmpIndicator.Scale)
                            throw new Exception(string.Format("{0}請輸入0-{1}區間的自評分數，小數點最多1位", pfaEmpIndicator.PfaIndicatorName, pfaEmpIndicator.Scale));
                        if (!Regex.IsMatch(pfaEmpIndicator.SelfIndicator.ToString(), pattern))
                            throw new Exception(string.Format("{0}請輸入0-{1}區間的自評分數，小數點最多1位", pfaEmpIndicator.PfaIndicatorName, pfaEmpIndicator.Scale));
                    }
                    else 
                    { 
                        if (cmd == "btnSent")
                            throw new Exception(string.Format("請輸入{0}自評分數", pfaEmpIndicator.PfaIndicatorName));
                    }
                }

                if (cmd == "btnSent")
                { 
                    if (string.IsNullOrWhiteSpace(model.SelfAppraisal))
                        throw new Exception("請輸入個人工作意願及建議事項&年度工作說明");
                }

                model.SelfIndicator = model.PfaEmpIndicator.Where(x => x.SelfIndicator.HasValue).Sum(x => x.SelfIndicator);
                model.PfaSelfScore = model.SelfIndicator;
                #endregion

                #region 績效考核員工訓練紀錄
                if (model.PfaEmpTraining != null)
                {
                    foreach (var pfaEmpTraining in model.PfaEmpTraining)
                    {
                        //if (string.IsNullOrWhiteSpace(pfaEmpTraining.CoursesCode))
                        //    throw new Exception("請輸入課程代碼");

                        if (pfaEmpTraining.TrainingHours.HasValue)
                        {
                            if (pfaEmpTraining.TrainingHours.Value < 0 || pfaEmpTraining.TrainingHours.Value > 9999)
                                throw new Exception("員工訓練紀錄請輸入介於0至9999的時數，小數點最多1位");
                            if (!Regex.IsMatch(pfaEmpTraining.TrainingHours.Value.ToString(), pattern))
                                throw new Exception("員工訓練紀錄請輸入介於0至9999的時數，小數點最多1位");
                        }
                    }
                }
                else
                {
                    model.PfaEmpTraining = new List<PfaEmpTrainingViewModel>();
                }
                #endregion
            }
            catch (Exception ex)
            {
                result.success = false;
                result.message = ex.Message;
            }
            return result;
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
                SelfAppraisal = pfaSignProcess.PfaCycleEmp.SelfAppraisal,
            };

            if (model.PfaOrgID.HasValue)
                model.PfaOrgName = pfaSignProcess.PfaCycleEmp.PfaOrg.PfaOrgName;

            if (pfaSignProcess.Status == "r") // r:退回修改
            {
                var pfaBackSignProcess = Services.GetService<PfaSignRecordService>().GetAll().Where(x => x.PfaCycleEmpID == pfaSignProcess.PfaCycleEmpID && x.Status == PfaSignProcess_Status.Returned).OrderByDescending(x => x.CreatedTime).FirstOrDefault();
                if (pfaBackSignProcess != null)
                    model.Assessment = pfaBackSignProcess.Assessment;
            }

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
            if (pfaSignProcess.PfaCycleEmp.PfaEmpTraining.Any())
            {
                model.PfaEmpTraining = new List<PfaEmpTrainingViewModel>();
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
            else {
                model.PfaEmpTraining = new List<PfaEmpTrainingViewModel>();
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

            #region 簽核記錄
            model.PfaSignRecord = HRPortal.Common.PFA.PfaSignRecord.GePfaSignRecord(pfaSignProcess, model);
            #endregion

            return model;
        }
        #endregion

        #region 初始值
        private void GetDefaultData(string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "")
        {
            ViewBag.DepartmentList = GetDepartmentList(txtDepartmentID);
            ViewBag.txtDepartmentID = txtDepartmentID;
            ViewBag.txtEmployeeNo = txtEmployeeNo;
            ViewBag.txtEmployeeName = txtEmployeeName;
        }
        #endregion

        #region 下拉
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
        #endregion

        /// <summary>
        /// 計算總和
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SelfIndicatorTotalSum(PfaCycleEmpSignViewModel model)                 
        {
            var totalSum = (decimal)0;
            try
            {
                if (model.PfaEmpIndicator != null)
                {
                    if (model.PfaEmpIndicator.Any())
                    {
                        foreach (var numberData in model.PfaEmpIndicator)
                        {
                            if (numberData.SelfIndicator.HasValue)
                                totalSum = totalSum + (decimal)numberData.SelfIndicator.Value;
                        }
                        totalSum = Math.Floor(totalSum * 10) / 10;
                    }
                }
                return Json(new { success = true, message = "", Total = totalSum.ToString() });
            }
            catch
            {
                return Json(new { success = true, message = "加總錯誤", Total = "0" });
            }
        }
    }
}
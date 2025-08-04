using DocumentFormat.OpenXml.Office2010.Excel;
using HRPortal.Common.PDFHelper;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using HRPortal.Utility;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaProgressQueryController : BaseController
    {
        #region 查詢
        public ActionResult Index(string txtPfaCycleID = "", string txtPfaOrgID = "", string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", string txtStatus = "", int page = 1, string cmd = "")
        {
            GetDefaultData(txtPfaCycleID, txtPfaOrgID, txtDepartmentID, txtEmployeeNo, txtEmployeeName, txtStatus);

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }
            else
            {
                IPagedList<PfaProgressQueryViewModel> viewModel = 
                    GetQueryDatas(txtPfaCycleID
                        , txtPfaOrgID
                        , txtDepartmentID
                        , txtEmployeeNo
                        , txtEmployeeName
                        , txtStatus
                        , page);

                return View(viewModel);
            }

        }

        private IPagedList<PfaProgressQueryViewModel> GetQueryDatas(string txtPfaCycleID, string txtPfaOrgID, string txtDepartmentID, string txtEmployeeNo, string txtEmployeeName, string txtStatus, int page)
        {

            var result = Services.GetService<PfaProgressQueryService>().GetQueryDatas(txtPfaCycleID
                        , txtPfaOrgID
                        , txtDepartmentID
                        , txtEmployeeNo
                        , txtEmployeeName
                        , txtStatus);

            int currentPage = page < 1 ? 1 : page;
            IPagedList<PfaProgressQueryViewModel> datas = result.Select(r => new PfaProgressQueryViewModel() 
            {
                PfaCycleEmpID = r.ID,
                PfaCycleID = r.PfaCycleID,
                PfaOrgID = r.PfaOrgID,
                EmployeeID = r.EmployeeID,
                EmployeeNo = r.Employees.EmployeeNO,
                EmployeeName = r.Employees.EmployeeName,
                PfaDeptID = r.PfaDeptID,
                PfaDeptCode = r.PfaDept.PfaDeptCode,
                PfaDeptName = r.PfaDept.PfaDeptName,
                PfaSelfScore = r.PfaSelfScore,
                PfaFirstScore = r.PfaFirstScore,
                PfaLastScore = r.PfaLastScore,
                PfaFinalScore = r.PfaFinalScore,
                SignStatus = r.Status,
            })
                .OrderBy(x => x.PfaCycleID).ThenBy(x => x.PfaDeptCode).ThenBy(x => x.EmployeeNo)
                .ToPagedList(currentPage, currentPageSize);
            #region pfaCycle
            List<Guid> pfaCycleIDs = datas.Select(x => x.PfaCycleID).Distinct().ToList();
            List<PfaCycle> pfaCycleList = Services.GetService<PfaCycleService>()
                .GetAll()
                .OrderByDescending(r=>r.PfaYear)
                .Take(100)
                .ToList()
                .Where(x => pfaCycleIDs.Contains(x.ID)).ToList();
            foreach (var item in datas)
            {
                var pfaCycle = pfaCycleList.FirstOrDefault(y => y.ID == item.PfaCycleID);

                var pfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "";
                var startDate = pfaCycle != null ? pfaCycle.StartDate : null;

                item.PfaFormNo = pfaFormNo;
                item.StartDate = startDate;
            }
            #endregion

            #region PfaOrg
            List<PfaOrg> pfaOrgList = Services.GetService<PfaOrgService>().GetAll()
                .Where(x => x.CompanyID == CurrentUser.CompanyID)
                .ToList();
            foreach (var item in datas)
            {
                var pfaOrg = pfaOrgList.FirstOrDefault(y => y.ID == item.PfaOrgID);

                var pfaOrgName = pfaOrg != null ? pfaOrg.PfaOrgName : "";

                item.PfaOrgName = pfaOrgName;
            }
            #endregion

            #region Status
            foreach (var item in datas)
            {
                switch (item.SignStatus)
                {
                    case PfaCycleEmp_Status.NotSubmittedForApproval:
                        item.StrSignStatus = "未送簽";
                        break;
                    case PfaCycleEmp_Status.InApprovalProcess:
                        item.StrSignStatus = "考評中";
                        break;
                    case PfaCycleEmp_Status.Approved:
                        item.StrSignStatus = "考評完成";
                        break;
                    case PfaCycleEmp_Status.Locked:
                        item.StrSignStatus = "已鎖定";
                        break;
                    default:
                        item.StrSignStatus = "";
                        break;
                }
            }
            #endregion

            #region SignEmp
            string[] statusList = { PfaSignProcess_Status.PendingReview
                    , PfaSignProcess_Status.Reviewed
                    , PfaSignProcess_Status.ReturnedForModification
                    , PfaSignProcess_Status.PendingThirdReview};

            var pfaCycleEmpIDs = datas.Select(x => x.PfaCycleEmpID).Distinct().ToList();
            var pfaSignProcessList = Services.GetService<PfaSignProcessService>().GetAll().Where(x => pfaCycleEmpIDs.Contains(x.PfaCycleEmpID) && statusList.Contains(x.Status)).ToList();

            var preSignEmpIDs = pfaSignProcessList.Select(x => x.PreSignEmpID).Distinct().ToList();
            var preSignEmpList = Services.GetService<EmployeeService>().GetAll().Where(x => preSignEmpIDs.Contains(x.ID)).ToList();

            foreach (var item in datas)
            {
                var pfaSignProcess = pfaSignProcessList.FirstOrDefault(y => y.PfaCycleEmpID == item.PfaCycleEmpID);
                if (pfaSignProcess != null)
                {
                    var preSignEmp = preSignEmpList.FirstOrDefault(y => y.ID == pfaSignProcess.PreSignEmpID);
                    if (preSignEmp != null)
                    {
                        item.PreSignEmpNo = preSignEmp.EmployeeNO;
                        item.PreSignEmpName = preSignEmp.EmployeeName;
                    }
                }
            }
            #endregion

            return datas;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtPfaCycleID, string txtPfaOrgID, string txtDepartmentID, string txtEmployeeNo, string txtEmployeeName, string txtStatus, string btnQuery, string btnClear)
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
                    txtPfaCycleID,
                    txtPfaOrgID,
                    txtDepartmentID,
                    txtEmployeeNo,
                    txtEmployeeName,
                    txtStatus,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtPfaCycleID, txtPfaOrgID, txtDepartmentID, txtEmployeeNo, txtEmployeeName, txtStatus);

            return View();
        }
        #endregion

        #region 初始值
        private void GetDefaultData(string txtPfaCycleID = "", string txtPfaOrgID = "", string txtDepartmentID = "", string txtEmployeeNo = "", string txtEmployeeName = "", string txtStatus = "")
        {
            ViewBag.FormNoList = GetFormNoList(txtPfaCycleID);
            ViewBag.OrgList = GetOrgList(txtPfaOrgID);
            ViewBag.DepartmentList = GetDepartmentList(txtDepartmentID);
            ViewBag.txtDepartmentID = txtDepartmentID;
            ViewBag.txtEmployeeNo = txtEmployeeNo;
            ViewBag.txtEmployeeName = txtEmployeeName;
            ViewBag.StatusList = GetStatusList(txtStatus);
            ViewBag.txtStatus = txtStatus;
            ViewBag.txtPfaCycleID = txtPfaCycleID;
            ViewBag.txtPfaOrgID = txtPfaOrgID;
        }
        #endregion

        #region 下拉
        private List<SelectListItem> GetFormNoList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            var data = Services.GetService<PfaCycleService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID).OrderByDescending(x => x.PfaYear).ThenByDescending(x => x.PfaFormNo).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });

            return listItem;
        }

        private List<SelectListItem> GetOrgList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            var data = Services.GetService<PfaOrgService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID).OrderBy(x => x.Ordering).ThenBy(x => x.PfaOrgCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = item.PfaOrgName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });

            return listItem;
        }

        /// <summary>
        /// 取得部門選單
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

        public List<SelectListItem> GetStatusList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "未送簽", Value = "m", Selected = (selectedData == "m" ? true : false) });
            listItem.Add(new SelectListItem { Text = "考評中", Value = "a", Selected = (selectedData == "a" ? true : false) });
            listItem.Add(new SelectListItem { Text = "考評完成", Value = "e", Selected = (selectedData == "e" ? true : false) });
            listItem.Add(new SelectListItem { Text = "已鎖定", Value = "y", Selected = (selectedData == "y" ? true : false) });

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
        #endregion

        #region 明細
        public ActionResult Detail(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_Detail");

            var pfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x => x.ID == Id.Value);
            if (pfaCycleEmp == null)
                return PartialView("_Detail");

            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(pfaCycleEmp.PfaCycleID);
            if (pfaCycle == null)
                return PartialView("_Detail");

            var model = GetDetail(pfaCycleEmp, pfaCycle);

            return PartialView("_Detail", model);
        }

        /// <summary>
        /// 取得明細
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycle"></param>
        /// <returns></returns>
        private PfaCycleEmpSignViewModel GetDetail(PfaCycleEmp pfaCycleEmp, PfaCycle pfaCycle)
        {
            var hire = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Hire").FirstOrDefault(x => x.ID == pfaCycleEmp.HireID);
            var jobTitle = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").FirstOrDefault(x => x.ID == pfaCycleEmp.JobTitleID);
            var jobFunction = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobFunction").FirstOrDefault(x => x.ID == pfaCycleEmp.JobFunctionID);
            var grade = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Grade").FirstOrDefault(x => x.ID == pfaCycleEmp.GradeID);

            var model = new PfaCycleEmpSignViewModel
            {
                PfaCycleID = pfaCycle.ID,
                PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "",
                DutyBeginEndDate = pfaCycle != null ? string.Format("統計區間:{0}至{1}止", pfaCycle.DutyBeginDate.HasValue ? pfaCycle.DutyBeginDate.Value.ToString("yyyy/MM/dd") : "", pfaCycle.DutyBeginDate.HasValue ? pfaCycle.DutyEndDate.Value.ToString("yyyy/MM/dd") : "") : "",
                PfaCycleEmpID = pfaCycleEmp.ID,
                PfaDeptID = pfaCycleEmp.PfaDeptID,
                PfaDeptCode = pfaCycleEmp.PfaDept.PfaDeptCode,
                PfaDeptName = pfaCycleEmp.PfaDept.PfaDeptName,
                PfaOrgID = pfaCycleEmp.PfaOrgID,
                HireID = pfaCycleEmp.HireID,
                HireCode = hire != null ? hire.OptionCode : "",
                HireName = hire != null ? hire.OptionName : "",
                JobTitleID = pfaCycleEmp.JobTitleID,
                JobTitleCode = jobTitle != null ? jobTitle.OptionCode : "",
                JobTitleName = jobTitle != null ? jobTitle.OptionName : "",
                JobFunctionID = pfaCycleEmp.JobFunctionID,
                JobFunctionCode = jobFunction != null ? jobFunction.OptionCode : "",
                JobFunctionName = jobFunction != null ? jobFunction.OptionName : "",
                GradeID = pfaCycleEmp.GradeID,
                GradeCode = grade != null ? grade.OptionCode : "",
                GradeName = grade != null ? grade.OptionName : "",
                EmployeeID = pfaCycleEmp.EmployeeID,
                EmployeeNo = pfaCycleEmp.Employees.EmployeeNO,
                EmployeeName = pfaCycleEmp.Employees.EmployeeName,
                ArriveDate = pfaCycleEmp.Employees.ArriveDate.ToString("yyyy/MM/dd"),
                Education = pfaCycleEmp.Education,
                SchoolName = pfaCycleEmp.SchoolName,
                DeptDescription = pfaCycleEmp.DeptDescription,
                PersonalLeave = pfaCycleEmp.PersonalLeave,
                SickLeave = pfaCycleEmp.SickLeave,
                LateLE = pfaCycleEmp.LateLE,
                AWL = pfaCycleEmp.AWL,
                Salary01 = pfaCycleEmp.Salary01,
                Salary02 = pfaCycleEmp.Salary02,
                Salary03 = pfaCycleEmp.Salary03,
                Salary04 = pfaCycleEmp.Salary04,
                Salary05 = pfaCycleEmp.Salary05,
                Salary06 = pfaCycleEmp.Salary06,
                FullSalary = pfaCycleEmp.FullSalary,
                FirstAppraisal = pfaCycleEmp.FirstAppraisal,
                PastPerformance = pfaCycleEmp.PastPerformance,
                NowPerformance = pfaCycleEmp.NowPerformance,
                Development = pfaCycleEmp.Development,
                LastAppraisal = pfaCycleEmp.LastAppraisal,
                FinalAppraisal = pfaCycleEmp.FinalAppraisal,
                SelfAppraisal = pfaCycleEmp.SelfAppraisal,
                PfaFirstScore = pfaCycleEmp.PfaFirstScore,
                PfaLastScore = pfaCycleEmp.PfaLastScore,
                PfaFinalScore = pfaCycleEmp.PfaFinalScore,
                ManagerAbility = pfaCycleEmp.ManagerAbility,
                ManagerIndicator = pfaCycleEmp.ManagerIndicator,
                PfaSelfScore = pfaCycleEmp.PfaSelfScore,
                SelfIndicator = pfaCycleEmp.SelfIndicator,
                IsRatio = pfaCycleEmp.IsRatio,
            };

            if (model.PfaOrgID.HasValue)
                model.PfaOrgName = pfaCycleEmp.PfaOrg.PfaOrgName;

            var performance = new StringBuilder();
            performance.AppendLine(pfaCycleEmp.Performance1);
            performance.AppendLine(pfaCycleEmp.Performance2);
            performance.AppendLine(pfaCycleEmp.Performance3);
            model.StrPerformance = performance.ToString();

            #region 績效考核員工工作績效
            if (pfaCycleEmp.PfaEmpIndicator.Any())
            {
                model.PfaEmpIndicator = pfaCycleEmp.PfaEmpIndicator.Select(x => new PfaEmpIndicatorViewModel
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
                        PfaCycleEmpID = pfaCycleEmp.ID,
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
            if (pfaCycleEmp.PfaEmpTraining.Any())
            {
                foreach (var pfaEmpTraining in pfaCycleEmp.PfaEmpTraining)
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
            model.PfaSignUploadData = new PfaSignUploadDataViewModel();

            var filename = "";
            model.PfaSignUploadData.PfaSignUpload = GetUploadData(pfaCycleEmp.ID);
            model.SignUploadCount = model.PfaSignUploadData.PfaSignUpload.Count();
            #endregion

            #region 績效考核員工勝任能力
            model.PfaEmpAbility = new List<PfaEmpAbilityViewModel>();
            if (pfaCycleEmp.PfaEmpAbility.Any())
            {
                var pfaEmpAbilityList = pfaCycleEmp.PfaEmpAbility.OrderBy(x => x.Ordering).ToList();
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
                var pfaAbilityList = Services.GetService<PfaAbilityService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true && x.PfaEmpTypeID == pfaCycleEmp.PfaEmpTypeID).OrderBy(x => x.Ordering).ToList();
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
                PfaCycleRationDetail = new List<PfaCycleRationDetailDataViewModel>()
            };
            
            if (pfaCycle.PfaCycleRation.Any())
            {
                var pfaCycleRation = pfaCycle.PfaCycleRation.FirstOrDefault(x => x.PfaOrgID == pfaCycleEmp.PfaOrgID);
                if (pfaCycleRation != null)
                {
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

                    if (model.PfaCycleRationData.PfaPerformance.Any())
                    {
                        
                        decimal totalScore = 0;
                        int totalNum = 0;
                        for (int i = 0; i < RowTypes.Items.Length; i++)
                        {
                            totalScore = 0;
                            totalNum = 0;
                            var tempRationDetail = new PfaCycleRationDetailDataViewModel
                            {
                                RowType = RowTypes.Items[i],
                                RowTypeName = RowTypes.ItemNames[i],
                                OrgTotal = pfaCycleRation.OrgTotal,
                                IsRation = pfaCycleRation.IsRation ? "Y" : RowTypes.Items[i] == "Ration" ? "Y" : "",
                                Number = new List<RationNumberViewModel>()
                            };

                            foreach (var pfaPerformance in model.PfaCycleRationData.PfaPerformance)
                            {
                                var tempPfaCycleRationDetail = pfaCycleRationDetailList.FirstOrDefault(x => x.PfaPerformanceID == pfaPerformance.ID);
                                if (tempPfaCycleRationDetail != null)
                                {
                                    var numberData = new RationNumberViewModel
                                    {
                                        PfaPerformanceID = pfaPerformance.ID,
                                        Number = 0,
                                    };
                                    switch (RowTypes.Items[i])
                                    {
                                        case RowTypes.Ration:
                                            if (tempPfaCycleRationDetail.Staffing.HasValue)
                                            {
                                                numberData.Number = tempPfaCycleRationDetail.Staffing.Value;
                                                totalNum = totalNum + numberData.Number;
                                            }
                                            break;
                                        case RowTypes.First:
                                            if (tempPfaCycleRationDetail.FirstFinal.HasValue)
                                            {
                                                numberData.Number = tempPfaCycleRationDetail.FirstFinal.Value;
                                                if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                    totalScore = totalScore + (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);
                                                totalNum = totalNum + numberData.Number;
                                            }
                                            break;
                                        case RowTypes.Second:
                                            if (tempPfaCycleRationDetail.SecondFinal.HasValue)
                                            {
                                                numberData.Number = tempPfaCycleRationDetail.SecondFinal.Value;
                                                if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                    totalScore = totalScore + (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);
                                                totalNum = totalNum + numberData.Number;
                                            }
                                            break;
                                        case RowTypes.Last:
                                        case RowTypes.Calculat:
                                            if (tempPfaCycleRationDetail.ThirdFinal.HasValue)
                                            {
                                                numberData.Number = tempPfaCycleRationDetail.ThirdFinal.Value;
                                                if (tempPfaCycleRationDetail.Multiplier.HasValue)
                                                    totalScore = totalScore + (numberData.Number * tempPfaCycleRationDetail.Multiplier.Value);
                                                totalNum = totalNum + numberData.Number;
                                            }
                                            break;
                                    }
                                    tempRationDetail.Number.Add(numberData);
                                }
                            }
                            switch (RowTypes.Items[i])
                            {

                                case RowTypes.First:
                                case RowTypes.Second:
                                case RowTypes.Last:
                                case RowTypes.Calculat:
                                    if (totalNum == tempRationDetail.OrgTotal)
                                    {
                                        if (totalScore <= tempRationDetail.OrgTotal)
                                        {
                                            tempRationDetail.IsRation = "Y";
                                        }
                                        else
                                        {
                                            tempRationDetail.IsRation = "N";
                                        }
                                    }
                                    break;
                            }
                            model.PfaCycleRationData.PfaCycleRationDetail.Add(tempRationDetail);
                        }
                    }
                }
            }
            else
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
                        Code = x.Code,
                        Name = x.Name,
                        Scores = tempScores,
                        Multiplier = x.Multiplier,
                    };
                }).ToList();

                if (model.PfaCycleRationData.PfaPerformance.Any())
                {
                    for (int i = 0; i < RowTypes.Items.Length; i++)
                    {
                        var tempRationDetail = new PfaCycleRationDetailDataViewModel
                        {
                            RowType = RowTypes.Items[i],
                            RowTypeName = RowTypes.ItemNames[i],
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
            #endregion

            #region 簽核記錄
            model.PfaSignRecord = HRPortal.Common.PFA.PfaSignRecord.GePfaSignRecord(model);

            #endregion

            return model;
        }

        /// <summary>
        /// 附件
        /// </summary>
        /// <param name="pfaCycleEmpId"></param>
        /// <returns></returns>
        public ActionResult Upload(string pfaCycleEmpId)
        {
            Guid? PfaCycleEmpID = null;
            if (!string.IsNullOrEmpty(pfaCycleEmpId))
                PfaCycleEmpID = Guid.Parse(pfaCycleEmpId);

            ViewBag.pfaCycleEmpId = pfaCycleEmpId;
            ViewBag.filename = "";



            var result = new PfaSignUploadDataViewModel();

            result.PfaSignUpload = GetUploadData(PfaCycleEmpID);
            return View("_Upload", result);
        }

        private List<PfaSignUploadViewModel> GetUploadData(Guid? pfaCycleEmpID)
        {
            return Services.GetService<PfaSignUploadService>()
                .GetAll()
                .Where(x => x.PfaCycleEmpID == pfaCycleEmpID)
                .OrderBy(x => x.Ordering)
                .ToList()
                .Select(x =>
                {

                    return new PfaSignUploadViewModel
                    {
                        ID = x.ID,
                        PfaCycleEmpID = x.PfaCycleEmpID,
                        PfaSignProcessID = x.PfaCycleEmpID,
                        FileName = x.FileName,
                        Href = x.Href,
                    };
                }).ToList();
        }
        #endregion

        #region PDF
        public ActionResult PDF(Guid? id)
        {
            PfaProgressQueryPDFViewModel view = GetData(id.Value);

            string titleText = "考績表";
            bool horizontal = false;

            StringBuilder html = new StringBuilder();
            html = new StringBuilder(ReadMailHtmlForUser(@"\Areas\DDMC_PFA\Views\PfaProgressQuery\_PerformanceForm.html"));

            string base64ImageData = Convert.ToBase64String(System.IO.File.ReadAllBytes(HttpContext.Server.MapPath("~/Images/DDMC_152_152.png")));
            html.Replace("@TitleImg@", "<img src=\"data:image/gif;base64," + base64ImageData + "\" style=\"width:60px;height:60px;\" />");
            html.Replace("@PfaYear@", view.PfaYear);
            html.Replace("@NowDate@", DateTime.Now.ToString("yyyy/MM/dd"));
            html.Replace("@DeptName@", view.PfaDeptName);
            html.Replace("@EmployeeName@", view.EmployeeName);
            html.Replace("@EmployeeNo@", view.EmployeeNo);
            html.Replace("@Gender@", view.Gender == 1 ? "男" : "女");
            html.Replace("@Birthday@", view.Birthday.HasValue ? view.Birthday.Value.ToString("yyyy/MM/dd") : "");
            html.Replace("@JobFunctionName@", view.JobFunctionName);
            html.Replace("@JobTitleName@", view.JobTitleName);
            html.Replace("@SchoolName@", view.SchoolName);
            html.Replace("@DeptDescription@", view.DeptDescription);
            html.Replace("@ArriveDate@", view.ArriveDate.ToString("yyyy/MM/dd"));
            html.Replace("@LateLE@", view.LateLE.HasValue ? view.LateLE.ToString() : "");
            html.Replace("@PersonalLeave@", view.PersonalLeave.HasValue ? view.PersonalLeave.ToString() : "");
            html.Replace("@SickLeave@", view.SickLeave.HasValue ? view.SickLeave.ToString() : "");
            html.Replace("@AWL@", view.AWL.HasValue ? view.AWL.ToString() : "");
            html.Replace("@DutyBeginEndDate@", view.DutyBeginEndDate);
            html.Replace("@Salary04@", view.Salary04.HasValue ? view.Salary04.Value.ToString("N0") : "");
            html.Replace("@FullSalary@", view.FullSalary.HasValue ? view.FullSalary.Value.ToString("N0") : "");
            html.Replace("@Performance1@", view.Performance1);
            html.Replace("@Performance2@", view.Performance2);
            html.Replace("@Performance3@", view.Performance3);
            html.Replace("@GradeName@", view.GradeName);
            html.Replace("@PfaEmpTypeName@", view.PfaEmpTypeName);
            html.Replace("@SelfAppraisal@", HttpUtility.HtmlEncode(view.SelfAppraisal));


            List<string> appraisals = new List<string>();
            if (string.IsNullOrWhiteSpace(view.FirstAppraisal) == false)
            {
                appraisals.Add("初核主管意見:" + HttpUtility.HtmlEncode(view.FirstAppraisal));
            }
            if (string.IsNullOrWhiteSpace(view.LastAppraisal) == false)
            {
                appraisals.Add("複核主管意見:" + HttpUtility.HtmlEncode(view.LastAppraisal));
            }
            if (string.IsNullOrWhiteSpace(view.FinalAppraisal) == false)
            {
                appraisals.Add("核決主管意見:" + HttpUtility.HtmlEncode(view.FinalAppraisal));
            }
            string appraisal = string.Join("<br/>", appraisals);
            html.Replace("@LastAppraisal@", appraisal);
            html.Replace("@PfaFirstScore@", view.PfaFirstScore.HasValue ? view.PfaFirstScore.ToString() : "");
            html.Replace("@PfaLastScore@", view.PfaLastScore.HasValue ? view.PfaLastScore.ToString() : "");
            html.Replace("@PfaFinalScore@", view.PfaFinalScore.HasValue ? view.PfaFinalScore.ToString() : "");

            var Salary01 = view.Salary01.HasValue ? view.Salary01.Value.ToString("N0") : "";
            var Salary02 = view.Salary02.HasValue ? view.Salary02.Value.ToString("N0") : "";
            var Salary03 = view.Salary03.HasValue ? view.Salary03.Value.ToString("N0") : "";

            #region 年度訓練記錄
            StringBuilder TrainString = new StringBuilder();
            if (view.PfaEmpTraining != null)
            {
                var i = 0;
                if (view.PfaEmpTraining.Count() > 6)
                {
                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;writing-mode:vertical-lr' rowspan='7'>年度訓練紀錄</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'>課程代號</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'>課程名稱</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'>時數</td>");
                    TrainString.AppendLine("<td style='text-align:center;writing-mode:vertical-lr' rowspan='4'>備註</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                    TrainString.AppendLine("</tr>");

                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'>詳附件</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                    TrainString.AppendLine("</tr>");

                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center; padding: 2px;' colspan='3'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                    TrainString.AppendLine("</tr>");

                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center; padding: 2px;' colspan='3'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                    TrainString.AppendLine("</tr>");

                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'>本薪</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary01 + "</td>");
                    TrainString.AppendLine("</tr>");

                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'>主管加給</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary02 + "</td>");
                    TrainString.AppendLine("</tr>");

                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'>交通津貼</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary03 + "</td>");
                    TrainString.AppendLine("</tr>");
                }
                else
                {
                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;writing-mode:vertical-lr' rowspan='7'>年度訓練紀錄</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'>課程代號</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'>課程名稱</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'>時數</td>");
                    TrainString.AppendLine("<td style='text-align:center;writing-mode:vertical-lr' rowspan='4'>備註</td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                    TrainString.AppendLine("</tr>");

                    foreach (var item in view.PfaEmpTraining)
                    {
                        if (i < 3)
                        {
                            TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='3'>" + HttpUtility.HtmlEncode(item.CoursesCode) + "</td>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='6'>" + HttpUtility.HtmlEncode(item.CoursesName) + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='2'>" + item.TrainingHours.ToString() + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                            TrainString.AppendLine("</tr>");
                        }
                        else if (i == 3)
                        {
                            TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='3'>" + HttpUtility.HtmlEncode(item.CoursesCode) + "</td>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='6'>" + HttpUtility.HtmlEncode(item.CoursesName) + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='2'>" + item.TrainingHours.ToString() + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='3'>本薪</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary01 + "</td>");
                            TrainString.AppendLine("</tr>");
                        }
                        else if (i == 4)
                        {
                            TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='3'>" + HttpUtility.HtmlEncode(item.CoursesCode) + "</td>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='6'>" + HttpUtility.HtmlEncode(item.CoursesName) + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='2'>" + item.TrainingHours.ToString() + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='3'>主管加給</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary02 + "</td>");
                            TrainString.AppendLine("</tr>");
                        }
                        else if (i == 5)
                        {
                            TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='3'>" + HttpUtility.HtmlEncode(item.CoursesCode) + "</td>");
                            TrainString.AppendLine("<td style='text-align:left;' colspan='6'>" + HttpUtility.HtmlEncode(item.CoursesName) + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='2'>" + item.TrainingHours.ToString() + "</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='3'>交通津貼</td>");
                            TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary03 + "</td>");
                            TrainString.AppendLine("</tr>");
                        }
                        i++;
                    }

                    if (i < 6)
                    {
                        for (var j = i; j < 6; j++)
                        {
                            if (6 - j == 3)
                            {
                                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='3'></td>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='6'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>本薪</td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary01 + "</td>");
                                TrainString.AppendLine("</tr>");
                            }
                            else if (6 - j == 2)
                            {
                                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='3'></td>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='6'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>主管加給</td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary02 + "</td>");
                                TrainString.AppendLine("</tr>");
                            }
                            else if (6 - j == 1)
                            {
                                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='3'></td>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='6'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>交通津貼</td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + Salary03 + "</td>");
                                TrainString.AppendLine("</tr>");
                            }
                            else
                            {
                                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                                TrainString.AppendLine("<td style='text-align:left;padding: 2px;' colspan='3'></td>");
                                TrainString.AppendLine("<td style='text-align:left;' colspan='6'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                                TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                                TrainString.AppendLine("</tr>");
                            }
                        }
                    }
                }
            }
            else
            {
                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                TrainString.AppendLine("<td style='text-align:center;writing-mode:vertical-lr' rowspan='7'>年度訓練紀錄</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>課程代號</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='6'>課程名稱</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='2'>時數</td>");
                TrainString.AppendLine("<td style='text-align:center;writing-mode:vertical-lr' rowspan='4'>備註</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                TrainString.AppendLine("</tr>");

                for (var i = 0; i < 3; i++)
                {
                    TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                    TrainString.AppendLine("<td style='text-align:center;' colspan='7'></td>");
                    TrainString.AppendLine("</tr>");
                }

                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>本薪</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + view.Salary01.ToString() + "</td>");
                TrainString.AppendLine("</tr>");

                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>主管加給</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + view.Salary02.ToString() + "</td>");
                TrainString.AppendLine("</tr>");

                TrainString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='6'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='2'></td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='3'>交通津貼</td>");
                TrainString.AppendLine("<td style='text-align:center;' colspan='5'>" + view.Salary03.ToString() + "</td>");
                TrainString.AppendLine("</tr>");
            }
            html.Replace("@TrainData@", TrainString.ToString());
            #endregion

            #region 工作績效評核
            StringBuilder IndicatorString = new StringBuilder();
            if (view.PfaEmpIndicator != null)
            {
                var totalScale = 0;
                var totalManagerIndicator = (double)0;
                foreach (var item in view.PfaEmpIndicator)
                {
                    var DescList = item.PfaIndicatorDesc.Replace("\r\n", "|").Split('|').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    var i = 0;
                    foreach (var desc in DescList)
                    {
                        if (i == 0)
                        {
                            IndicatorString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            IndicatorString.AppendLine("<td style='text-align:left;' colspan='2' rowspan='" + DescList.Count + "'>" + item.PfaIndicatorName + "</td>");
                            IndicatorString.AppendLine("<td style='text-align:left;' colspan='6' rowspan='" + DescList.Count + "'>" + item.Description + "</td>");
                            IndicatorString.AppendLine("<td style='text-align:center;' colspan='2' rowspan='" + DescList.Count + "'>" + item.Scale.ToString() + "</td>");
                            IndicatorString.AppendLine("<td style='text-align:left;' colspan='6' >" + desc + "</td>");
                            IndicatorString.AppendLine("<td style='text-align:center;' colspan='2' rowspan='" + DescList.Count + "'>" + item.SelfIndicator.ToString() + "</td>");
                            IndicatorString.AppendLine("<td style='text-align:center;' colspan='2' rowspan='" + DescList.Count + "'>" + item.ManagerIndicator.ToString() + "</td>");
                            IndicatorString.AppendLine("</tr>");
                        }
                        else
                        {
                            IndicatorString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            IndicatorString.AppendLine("<td style='text-align:left;' colspan='6'>" + desc + "</td>");
                            IndicatorString.AppendLine("</tr>");
                        }
                        i++;
                    }

                    totalScale += (item.Scale.HasValue ? item.Scale.Value : 0);
                    totalManagerIndicator += (item.ManagerIndicator.HasValue ? item.ManagerIndicator.Value : 0);
                }
                IndicatorString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                IndicatorString.AppendLine("<td style='text-align:left;' colspan='8'>工作績效得分合計</td>");
                IndicatorString.AppendLine("<td style='text-align:center;' colspan='2'>" + totalScale.ToString() + "分</td>");
                IndicatorString.AppendLine("<td style='text-align:left;' colspan='6'></td>");
                IndicatorString.AppendLine("<td style='text-align:center;' colspan='2'>" + view.PfaSelfScore.ToString() + "分</td>");
                IndicatorString.AppendLine("<td style='text-align:center;' colspan='2'>" + totalManagerIndicator.ToString() + "分</td>");
                IndicatorString.AppendLine("</tr>");
            }
            html.Replace("@PfaIndicator@", IndicatorString.ToString());
            #endregion

            #region 勝任能力
            StringBuilder AbilityString = new StringBuilder();
            if (view.PfaEmpAbility != null)
            {
                var totalTotalScore = (double)0;
                var totalManagerAbility = (double)0;
                foreach (var item in view.PfaEmpAbility)
                {
                    var i = 0;
                    totalTotalScore += (item.TotalScore.HasValue ? item.TotalScore.Value : 0);
                    totalManagerAbility += (item.ManagerAbility.HasValue ? item.ManagerAbility.Value : 0);
                    foreach (var itemDetail in item.PfaEmpAbilityDetail)
                    {
                        if (i == 0)
                        {
                            AbilityString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            AbilityString.AppendLine("<td style='text-align:left;' colspan='2' rowspan='" + item.PfaEmpAbilityDetail.Count + "'>" + item.PfaAbilityName + "</td>");
                            AbilityString.AppendLine("<td style='text-align:left;' colspan='4' rowspan='" + item.PfaEmpAbilityDetail.Count + "'>" + item.Description + "</td>");
                            AbilityString.AppendLine("<td style='text-align:left;' colspan='4'>" + itemDetail.PfaAbilityKey + "</td>");
                            foreach (var Score in itemDetail.ScoreInterval)
                            {
                                if (itemDetail.AbilityScore.HasValue && itemDetail.AbilityScore.Value == Score)
                                {
                                    AbilityString.AppendLine("<td style='text-align:center;background-color: #EDEDED;'>" + Score.ToString() + "</td>");
                                }
                                else
                                {
                                    AbilityString.AppendLine("<td style='text-align:center;'>" + Score.ToString() + "</td>");
                                }
                            }
                            AbilityString.AppendLine("<td style='text-align:center;' colspan='2' rowspan='" + item.PfaEmpAbilityDetail.Count + "'>" + item.TotalScore.ToString() + "</td>");
                            AbilityString.AppendLine("<td style='text-align:center;' colspan='2' rowspan='" + item.PfaEmpAbilityDetail.Count + "'>" + item.ManagerAbility.ToString() + "</td>");
                            AbilityString.AppendLine("</tr>");
                        }
                        else
                        {
                            AbilityString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                            AbilityString.AppendLine("<td style='text-align:left;' colspan='4'>" + itemDetail.PfaAbilityKey + "</td>");
                            foreach (var Score in itemDetail.ScoreInterval)
                            {
                                if (itemDetail.AbilityScore.HasValue && itemDetail.AbilityScore.Value == Score)
                                {
                                    AbilityString.AppendLine("<td style='text-align:center;background-color: #EDEDED;'>" + Score.ToString() + "</td>");
                                }
                                else
                                {
                                    AbilityString.AppendLine("<td style='text-align:center;'>" + Score.ToString() + "</td>");
                                }
                            }
                            AbilityString.AppendLine("</tr>");
                        }
                        i++;
                    }
                }
                AbilityString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                AbilityString.AppendLine("<td style='text-align:justfy;' colspan='16'>勝任能力得分合計</td>");
                AbilityString.AppendLine("<td style='text-align:center;' colspan='2'>" + totalTotalScore.ToString() + "</td>");
                AbilityString.AppendLine("<td style='text-align:center;' colspan='2'>" + totalManagerAbility.ToString() + "</td>");
                AbilityString.AppendLine("</tr>");
            }
            html.Replace("@Ability@", AbilityString.ToString());
            #endregion

            #region 評估結果
            StringBuilder PerformanceString = new StringBuilder();
            var PastPerformanceDataCount = 0;
            var NowPerformanceDataCount = 0;
            var DevelopmentDataCount = 0;
            var PerformanceDataRowSpan = 0;
            if (view.PastPerformanceData != null)
            {
                PastPerformanceDataCount = view.PastPerformanceData.Count();
                PerformanceDataRowSpan = view.PastPerformanceData.Count();
            }
            if (view.NowPerformanceData != null)
            {
                NowPerformanceDataCount = view.NowPerformanceData.Count();
                if (view.NowPerformanceData.Count() > PerformanceDataRowSpan)
                    PerformanceDataRowSpan = view.NowPerformanceData.Count();
            }
            if (view.DevelopmentData != null)
            {
                DevelopmentDataCount = view.DevelopmentData.Count();
                if (view.DevelopmentData.Count() > PerformanceDataRowSpan)
                    PerformanceDataRowSpan = view.DevelopmentData.Count();
            }

            for (var i = 0; i < PerformanceDataRowSpan; i++)
            {
                if (i == 0)
                {
                    PerformanceString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    PerformanceString.AppendLine("<td style='text-align:left;' rowspan='" + PerformanceDataRowSpan + "'>評估結果</td>");
                    PerformanceString.AppendLine("<td style='text-align:left;' rowspan='" + PerformanceDataRowSpan + "'>與過去表現比較</td>");
                    if (i <= PastPerformanceDataCount)
                    {
                        if (view.PastPerformance.HasValue && view.PastPerformance.Value == Guid.Parse(view.PastPerformanceData[i].Value))
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "■" + (i + 1) + "." + view.PastPerformanceData[i].Text + "</td>");
                        }
                        else
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "□" + (i + 1) + "." + view.PastPerformanceData[i].Text + "</td>");
                        }
                    }
                    PerformanceString.AppendLine("<td style='text-align:left;' rowspan='" + PerformanceDataRowSpan + "'>目前工作評價</td>");
                    if (i <= NowPerformanceDataCount)
                    {
                        if (view.NowPerformance.HasValue && view.NowPerformance.Value == Guid.Parse(view.NowPerformanceData[i].Value))
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "■" + (i + 1) + "." + view.NowPerformanceData[i].Text + "</td>");
                        }
                        else
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "□" + (i + 1) + "." + view.NowPerformanceData[i].Text + "</td>");
                        }
                    }
                    PerformanceString.AppendLine("<td style='text-align:left;' rowspan='" + PerformanceDataRowSpan + "'>未來發展評斷</td>");
                    if (i <= DevelopmentDataCount)
                    {
                        if (view.Development.HasValue && view.Development.Value == Guid.Parse(view.DevelopmentData[i].Value))
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='6'>" + "■" + (i + 1) + "." + view.DevelopmentData[i].Text + "</td>");
                        }
                        else
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='6'>" + "□" + (i + 1) + "." + view.DevelopmentData[i].Text + "</td>");
                        }
                    }
                    PerformanceString.AppendLine("</tr>");
                }
                else
                {
                    PerformanceString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    if (i < PastPerformanceDataCount)
                    {
                        if (view.PastPerformance.HasValue && view.PastPerformance.Value == Guid.Parse(view.PastPerformanceData[i].Value))
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "■" + (i + 1) + "." + view.PastPerformanceData[i].Text + "</td>");
                        }
                        else
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "□" + (i + 1) + "." + view.PastPerformanceData[i].Text + "</td>");
                        }
                    }
                    else
                    {
                        PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'></td>");
                    }
                    if (i < NowPerformanceDataCount)
                    {
                        if (view.NowPerformance.HasValue && view.NowPerformance.Value == Guid.Parse(view.NowPerformanceData[i].Value))
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "■" + (i + 1) + "." + view.NowPerformanceData[i].Text + "</td>");
                        }
                        else
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'>" + "□" + (i + 1) + "." + view.NowPerformanceData[i].Text + "</td>");
                        }
                    }
                    else
                    {
                        PerformanceString.AppendLine("<td style='text-align:left;' colspan='5'></td>");
                    }
                    if (i < DevelopmentDataCount)
                    {
                        if (view.Development.HasValue && view.Development.Value == Guid.Parse(view.DevelopmentData[i].Value))
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='6'>" + "■" + (i + 1) + "." + view.DevelopmentData[i].Text + "</td>");
                        }
                        else
                        {
                            PerformanceString.AppendLine("<td style='text-align:left;' colspan='6'>" + "□" + (i + 1) + "." + view.DevelopmentData[i].Text + "</td>");
                        }
                    }
                    else
                    {
                        PerformanceString.AppendLine("<td style='text-align:left;' colspan='6'></td>");
                    }
                    PerformanceString.AppendLine("</tr>");
                }
            }
            html.Replace("@PerformanceData@", PerformanceString.ToString());
            #endregion

            #region 附件
            StringBuilder AppendixString = new StringBuilder();
            AppendixString.AppendLine("<div class='tab-pane'>");
            if (view.PfaEmpTraining != null && view.PfaEmpTraining.Count > 6)
            {
                AppendixString.AppendLine("<div>");
                AppendixString.AppendLine("<table>");

                AppendixString.AppendLine("<tr><td colspan='3'>附件：年度訓練紀錄</td></tr>");

                AppendixString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                AppendixString.AppendLine("<th style='text-align:center; width:30%' >課程代號</th>");
                AppendixString.AppendLine("<th style='text-align:center; width:50%' >課程名稱</th>");
                AppendixString.AppendLine("<th style='text-align:center; width:20%' >時數</th>");
                AppendixString.AppendLine("</tr>");

                AppendixString.AppendLine("<tbody>");
                foreach (var item in view.PfaEmpTraining)
                {
                    AppendixString.AppendLine("<tr role='row' class='ui-widget-content ui-row-ltr'>");
                    AppendixString.AppendLine("<td style='text-align:left;' >" + HttpUtility.HtmlEncode(item.CoursesCode) + "</td>");
                    AppendixString.AppendLine("<td style='text-align:left;' >" + HttpUtility.HtmlEncode(item.CoursesName) + "</td>");
                    AppendixString.AppendLine("<td style='text-align:center;' >" + item.TrainingHours.ToString() + "</td>");
                    AppendixString.AppendLine("</tr>");
                }
                AppendixString.AppendLine("</tbody>");

                AppendixString.AppendLine("</table>");
                AppendixString.AppendLine("</div>");
            }
            AppendixString.AppendLine("</div>");
            html.Replace("@Appendix@", AppendixString.ToString());
            #endregion

            byte[] buff = iTextSharpHelper.ExportPDF(html.ToString(), horizontal);

            #region 附件附加在 PDF 後面
            view.PfaSignUpload = GetUploadData(id.Value);
            try
            {
                foreach (var uploadData in view.PfaSignUpload)
                {
                    string href = uploadData.Href;

                    if (string.IsNullOrWhiteSpace(href))
                    {
                        continue;
                    }
                    byte[] newpdf = iTextSharpHelper.ReadPdf(href);

                    buff = iTextSharpHelper.MergePdfFiles(buff, newpdf);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            #endregion
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return File(buff, "application/pdf", titleText + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf");
        }

        public PfaProgressQueryPDFViewModel GetData(Guid id)
        {
            PfaProgressQueryPDFViewModel view = new PfaProgressQueryPDFViewModel();

            var PfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.ID == id).FirstOrDefault();
            if (PfaCycleEmp != null)
            {
                view.PfaCycleEmpID = PfaCycleEmp.ID;
                view.PfaCycleID = PfaCycleEmp.PfaCycleID;
                view.EmployeeID = PfaCycleEmp.EmployeeID;
                view.EmployeeNo = PfaCycleEmp.Employees.EmployeeNO;
                view.EmployeeName = PfaCycleEmp.Employees.EmployeeName;
                view.PfaDeptID = PfaCycleEmp.PfaDeptID;
                view.PfaDeptName = PfaCycleEmp.PfaDept.PfaDeptName;
                view.JobFunctionID = PfaCycleEmp.JobFunctionID;
                view.JobTitleID = PfaCycleEmp.JobTitleID;
                view.GradeID = PfaCycleEmp.GradeID;
                view.Gender = PfaCycleEmp.Employees.Gender;
                view.Birthday = PfaCycleEmp.Birthday;
                view.SchoolName = PfaCycleEmp.SchoolName;
                view.DeptDescription = PfaCycleEmp.DeptDescription;
                view.ArriveDate = PfaCycleEmp.Employees.ArriveDate;
                view.LateLE = PfaCycleEmp.LateLE;
                view.PersonalLeave = PfaCycleEmp.PersonalLeave;
                view.SickLeave = PfaCycleEmp.SickLeave;
                view.AWL = PfaCycleEmp.AWL;
                view.Salary01 = PfaCycleEmp.Salary01;
                view.Salary02 = PfaCycleEmp.Salary02;
                view.Salary03 = PfaCycleEmp.Salary03;
                view.Salary04 = PfaCycleEmp.Salary04;
                view.FullSalary = view.Salary01 + view.Salary02 + view.Salary03 + view.Salary04;
                view.Performance1 = PfaCycleEmp.Performance1;
                view.Performance2 = PfaCycleEmp.Performance2;
                view.Performance3 = PfaCycleEmp.Performance3;
                view.PfaEmpTypeName = PfaCycleEmp.PfaEmpType.PfaEmpTypeName;
                view.SelfAppraisal = PfaCycleEmp.SelfAppraisal;
                view.PfaSelfScore = PfaCycleEmp.PfaSelfScore;
                view.PfaFirstScore = PfaCycleEmp.PfaFirstScore;
                view.PastPerformance = PfaCycleEmp.PastPerformance;
                view.NowPerformance = PfaCycleEmp.NowPerformance;
                view.Development = PfaCycleEmp.Development;
                view.FirstAppraisal = PfaCycleEmp.FirstAppraisal;
                view.LastAppraisal = PfaCycleEmp.LastAppraisal;
                view.FinalAppraisal = PfaCycleEmp.FinalAppraisal;
                view.PfaLastScore = PfaCycleEmp.PfaLastScore;
                view.PfaFinalScore = PfaCycleEmp.PfaFinalScore;

                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(view.PfaCycleID);
                if (pfaCycle != null)
                {
                    view.DutyBeginEndDate = string.Format("統計區間自:{0}至{1}止", pfaCycle.DutyBeginDate.HasValue ? pfaCycle.DutyBeginDate.Value.ToString("yyyy/MM/dd") : "", pfaCycle.DutyBeginDate.HasValue ? pfaCycle.DutyEndDate.Value.ToString("yyyy/MM/dd") : "");
                    view.PfaYear = pfaCycle.PfaYear;
                }

                var jobTitle = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").FirstOrDefault(x => x.ID == view.JobTitleID);
                if (jobTitle != null)
                    view.JobTitleName = jobTitle.OptionName;
                var jobFunction = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobFunction").FirstOrDefault(x => x.ID == view.JobFunctionID);
                if (jobFunction != null)
                    view.JobFunctionName = jobFunction.OptionName;
                var grade = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Grade").FirstOrDefault(x => x.ID == view.GradeID);
                if (grade != null)
                {
                    view.GradeCode = grade.OptionCode;
                    view.GradeName = grade.OptionName;
                }

                #region 訓練紀錄
                view.PfaEmpTraining = new List<PfaEmpTrainingViewModel>();
                if (PfaCycleEmp.PfaEmpTraining.Any())
                {
                    foreach (var pfaEmpTraining in PfaCycleEmp.PfaEmpTraining)
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
                        view.PfaEmpTraining.Add(tempPfaEmpTraining);
                    }
                }
                #endregion

                #region 工作績效
                view.PfaEmpIndicator = new List<PfaEmpIndicatorViewModel>();
                if (PfaCycleEmp.PfaEmpIndicator.Any())
                {
                    view.PfaEmpIndicator = PfaCycleEmp.PfaEmpIndicator.Select(x => new PfaEmpIndicatorViewModel
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
                    var pfaIndicatorDesc = new StringBuilder();
                    var pfaIndicatorList = Services.GetService<PfaIndicatorService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true).OrderBy(x => x.Ordering).ToList();
                    foreach (var pfaIndicator in pfaIndicatorList)
                    {
                        pfaIndicatorDesc.Length = 0;
                        var pfaIndicatorDetailList = pfaIndicator.PfaIndicatorDetail.OrderBy(x => x.Ordering).ToList();
                        foreach (var pfaIndicatorDetail in pfaIndicatorDetailList)
                        {
                            pfaIndicatorDesc.AppendLine(string.Format("{0}.{1} {2}-{3}", pfaIndicatorDetail.Ordering, pfaIndicatorDetail.PfaIndicatorKey, pfaIndicatorDetail.UpperLimit, pfaIndicatorDetail.LowerLimit));
                        }

                        view.PfaEmpIndicator.Add(new PfaEmpIndicatorViewModel
                        {
                            PfaCycleEmpID = PfaCycleEmp.ID,
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

                #region 勝任能力
                view.PfaEmpAbility = new List<PfaEmpAbilityViewModel>();
                if (PfaCycleEmp.PfaEmpAbility.Any())
                {
                    var pfaEmpAbilityList = PfaCycleEmp.PfaEmpAbility.OrderBy(x => x.Ordering).ToList();
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
                        view.PfaEmpAbility.Add(tempPfaEmpAbility);
                    }
                }
                else
                {
                    var pfaAbilityList = Services.GetService<PfaAbilityService>().GetAll().Where(x => x.CompanyID == pfaCycle.CompanyID && x.IsUsed == true && x.PfaEmpTypeID == PfaCycleEmp.PfaEmpTypeID).OrderBy(x => x.Ordering).ToList();
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
                        view.PfaEmpAbility.Add(tempPfaEmpAbility);
                    }
                }
                #endregion

                #region 與過去表現比較
                view.PastPerformanceData = GetPfaOptionList("PastPerformance", view.PastPerformance.HasValue ? view.PastPerformance.Value.ToString() : "");
                #endregion

                #region 評價目前工作
                view.NowPerformanceData = GetPfaOptionList("NowPerformance", view.NowPerformance.HasValue ? view.NowPerformance.Value.ToString() : "");
                #endregion

                #region 未來發展評斷
                view.DevelopmentData = GetPfaOptionList("Development", view.Development.HasValue ? view.Development.Value.ToString() : "");
                #endregion
            }

            return view;
        }

        private String ReadMailHtmlForUser(String Path)
        {
            String ls_Html = String.Empty;
            try
            {
                System.Reflection.Assembly lo_asm = System.Reflection.Assembly.GetCallingAssembly();
                using (System.IO.StreamReader lo_reader = new System.IO.StreamReader(Request.PhysicalApplicationPath + Path))
                {
                    ls_Html = lo_reader.ReadToEnd();
                }
            }
            catch { }
            return ls_Html;
        }

        #endregion
    }


}
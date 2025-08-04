using DocumentFormat.OpenXml.Office2010.Excel;
using HRPortal.ApiAdapter.DDMC_PFA;
using HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaCycleController : BaseController
    {
        #region 查詢
        public ActionResult Index(int page = 1, string txtFormNo = "", string cmd = "")
        {
            GetDefaultData(txtFormNo);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
                return View();

            var ds = Services.GetService<PfaCycleService>().GetPfaCycleData(txtFormNo, CurrentUser.CompanyID);
            foreach (var item in ds)
            {
                var isSign = Services.GetService<PfaCycleEmpService>().GetAll()
                    .Where(x => x.PfaCycleID == item.ID && x.Status == PfaCycleEmp_Status.NotSubmittedForApproval)
                    .Any();

                var isLock = item.Status == PfaCycle_Status.PfaFinish ? true : false;

                if (isLock)
                {
                    // 如果有任何資料狀態不為已審核，則不能鎖定
                    var isTempLock = Services.GetService<PfaCycleEmpService>().GetAll()
                        .Where(x => x.PfaCycleID == item.ID && x.Status != PfaCycleEmp_Status.Approved)
                        .Any();
                    isLock = isTempLock ? false : isLock;
                }
                

                var isEdit = item.Status == PfaCycle_Status.NotSubmitted ? true : false;

                if (isEdit)
                {
                    var noCycleRation = true;
                    var pfaOrgIDs = Services.GetService<PfaCycleEmpService>().GetAll()
                        .Where(x => x.PfaCycleID == item.ID 
                            && x.PfaOrgID.HasValue 
                            && x.IsRatio)
                        .Select(x=> x.PfaOrgID.Value)
                        .Distinct()
                        .ToList();

                    var nowPfaOrgIDs = Services.GetService<PfaCycleRationService>().GetAll()
                        .Where(x => x.PfaCycleID == item.ID)
                        .Select(x => x.PfaOrgID)
                        .Distinct()
                        .ToList();

                    noCycleRation = pfaOrgIDs.Any(x => !nowPfaOrgIDs.Contains(x));

                    isSign = noCycleRation == true ? false : true;
                }
                var nowStatus = item.Status;

                item.IsSign = isSign;
                item.IsLock = isLock;
                item.IsEdit = isEdit;
                item.NowStatus = nowStatus;
                item.canReject = item.NowStatus != PfaCycle_Status.Lock;

            }

            var viewModel = ds.ToPagedList(currentPage, currentPageSize);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtFormNo, string btnQuery, string btnClear)
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
                    txtFormNo,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtFormNo);

            return View();
        }
        #endregion

        #region 明細
        public ActionResult Detail(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_DetailCycle");

            var data = Services.GetService<PfaCycleService>().GetPfaCycle(Id.Value);
            if (data == null)
                return PartialView("_DetailCycle");

            var model = new PfaCycleViewModel 
            {
                ID = Id.Value,
                PfaFormNo = data.PfaFormNo,
                PfaYear = data.PfaYear,
                ServeBasedate = data.ServeBasedate.HasValue ? data.ServeBasedate.Value : default(DateTime),
                DutyBeginDate = data.DutyBeginDate.HasValue ? data.DutyBeginDate.Value : default(DateTime),
                DutyEndDate = data.DutyEndDate.HasValue ? data.DutyEndDate.Value : default(DateTime),
                Desription = data.Desription,
                Status = data.Status,
                StartDate = data.StartDate.HasValue ? data.StartDate.Value : default(DateTime),
                CompanyID = data.CompanyID,
            };
            return PartialView("_DetailCycle", model);
        }
        #endregion

        #region 新增
        public async Task<ActionResult> Create()
        {
            var model = new PfaCycleViewModel 
            {
                ID = Guid.Empty
            };
            var data = await HRMApiAdapter.GetPfaCycleFormNo(CurrentUser.CompanyCode);
            model.PfaFormNoList = GetPfaFormNoList(data, "");

            return PartialView("_CreateCycle", model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(PfaCycleViewModel model)
        {
            if (string.IsNullOrEmpty(model.PfaFormNo))
            {
                WriteLog("請選擇績效考核批號");
                return Json(new { success = false, message = "請選擇績效考核批號" });
            }

            if (string.IsNullOrEmpty(model.PfaYear))
            {
                WriteLog("考績年度必需要有值");
                return Json(new { success = false, message = "考績年度必需要有值" });
            }

            var isExist = Services.GetService<PfaCycleService>().IsExist(model.PfaFormNo, CurrentUser.CompanyID);
            if (isExist)
            {
                WriteLog(string.Format("績效考核批號資料已存在,PfaFormNo:{0}", model.PfaFormNo));
                return Json(new { success = false, message = "績效考核批號資料已存在" });
            }

            var pfaCycleData = await HRMApiAdapter.GetPfaCycle(CurrentUser.CompanyCode, model.PfaFormNo);
            if (pfaCycleData == null || string.IsNullOrWhiteSpace(pfaCycleData.PfaFormNo))
            {
                WriteLog(string.Format("HRM查無績效考核批號,PfaFormNo:{0}", model.PfaFormNo));
                return Json(new { success = false, message = "HRM查無績效考核批號" });
            }

            var empPfaDataList = await HRMApiAdapter.GetEmpPfaData(CurrentUser.CompanyCode, model.PfaFormNo);
            if (!empPfaDataList.Any())
            {
                WriteLog(string.Format("績效考核批號查無員工績效考核資料,PfaFormNo:{0}", model.PfaFormNo));
                return Json(new { success = false, message = "績效考核批號查無員工績效考核資料" });
            }

            #region PfaCycle
            var pfaCycle = new PfaCycle
            {
                ID = Guid.NewGuid(),
                PfaFormNo = model.PfaFormNo,
                PfaYear = model.PfaYear,
                ServeBasedate = pfaCycleData.ServeBasedate,
                DutyBeginDate = pfaCycleData.DutyBeginDate,
                DutyEndDate = pfaCycleData.DutyEndDate,
                AssumeBasedate = pfaCycleData.AssumeBasedate,
                Desription = model.Desription,
                Status = "m",
                CompanyID = CurrentUser.CompanyID,
                CreatedBy = CurrentUser.EmployeeID,
                CreatedTime = DateTime.Now,
                ModifiedBy = CurrentUser.EmployeeID,
                ModifiedTime = DateTime.Now,
            };
            #endregion

            #region PfaCycleEmp
            var employeeList = Services.GetService<EmployeeService>().GetAll().Where(x=> x.Company.CompanyCode == CurrentUser.CompanyCode).ToList();
            var pfaDeptEmpList = Services.GetService<PfaDeptEmpService>().GetAll().ToList();
            var pfaOrgDeptList = Services.GetService<PfaOrgDeptService>().GetAll().ToList();
            var hireList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Hire").ToList();
            var jobTitleList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").ToList();
            var jobFunctionList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobFunction").ToList();
            var gradeList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Grade").ToList();
            var positionList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Position").ToList();
            var pfaTargetsList = Services.GetService<PfaTargetsService>().GetAll().ToList();
            var pfaEmpTypeTargetsList = Services.GetService<PfaEmpTypeTargetsService>().GetAll().ToList();
            var pfaRatioJobTitleList = Services.GetService<PfaRatioJobTitleService>().GetAll().ToList();

            Guid? employeeID = null;
            Guid? pfaDeptID = null;
            Guid? pfaOrgID = null;
            Guid? hireID = null;
            Guid? jobTitleID = null;
            Guid? jobFunctionID = null;
            Guid? gradeID = null;
            Guid? positionID = null;
            Guid? signTypeID = null;
            Guid? pfaEmpTypeID = null;
            bool isRatio = false;

            var pfaCycleEmpList = new List<PfaCycleEmp>();
            foreach (var empPfaData in empPfaDataList)
            {
                employeeID = null;
                var employee = employeeList.FirstOrDefault(y => y.EmployeeNO == empPfaData.EmpID);
                if (employee != null)
                    employeeID = employee.ID;
                else {
                    WriteLog(string.Format("查無員工:{0}資料,PfaFormNo:{1}", empPfaData.EmpID, model.PfaFormNo));
                    return Json(new { success = false, message = string.Format("查無員工:{0}資料", empPfaData.EmpID) });
                }

                pfaDeptID = null;
                pfaOrgID = null;
                var pfaDeptEmp = pfaDeptEmpList.FirstOrDefault(y => y.EmployeeID == employeeID);
                if (pfaDeptEmp != null)
                {
                    pfaDeptID = pfaDeptEmp.PfaDeptID;
                    var pfaOrg = pfaOrgDeptList.FirstOrDefault(y => y.PfaDeptID == pfaDeptID);
                    if (pfaOrg != null)
                        pfaOrgID = pfaOrg.PfaOrgID;
                    else
                    {
                        WriteLog(string.Format("員工:{0} 部門:{1} 查無考績效考核組織,PfaFormNo:{2}", employee.EmployeeNO + " " + employee.EmployeeName, pfaDeptEmp.PfaDept.PfaDeptName, model.PfaFormNo));
                        return Json(new { success = false, message = string.Format("員工:{0} 部門:{1} 查無考績效考核組織", employee.EmployeeNO + " " + employee.EmployeeName, pfaDeptEmp.PfaDept.PfaDeptName) });
                    }
                }
                else
                {
                    WriteLog(string.Format("員工:{0}查無考核部門,PfaFormNo:{1}", employee.EmployeeNO + " " + employee.EmployeeName, model.PfaFormNo));
                    return Json(new { success = false, message = string.Format("員工:{0}查無考核部門", employee.EmployeeNO + " " + employee.EmployeeName) });
                }

                hireID = null;
                var hire = hireList.FirstOrDefault(y => y.OptionCode == empPfaData.HireCode);
                if (hire != null)
                    hireID = hire.ID;

                jobTitleID = null;
                var jobTitle = jobTitleList.FirstOrDefault(y => y.OptionCode == empPfaData.JobTitleCode);
                if (jobTitle != null)
                    jobTitleID = jobTitle.ID;
                else
                {
                    // 給無職稱
                    jobTitle = jobTitleList.FirstOrDefault(y => y.OptionCode == "ZZ");
                    if (jobTitle != null)
                        jobTitleID = jobTitle.ID;
                }

                signTypeID = null;
                pfaEmpTypeID = null;
                isRatio = false;
                if (jobTitleID.HasValue)
                {
                    var pfaTargets = pfaTargetsList.FirstOrDefault(y => y.JobTitleID == jobTitleID.Value);
                    if (pfaTargets != null)
                        signTypeID = pfaTargets.SignTypeID;
                    var pfaEmpTypeTargets = pfaEmpTypeTargetsList.FirstOrDefault(y => y.JobTitleID == jobTitleID.Value);
                    if (pfaEmpTypeTargets != null)
                        pfaEmpTypeID = pfaEmpTypeTargets.PfaEmpTypeID;
                    else
                    {
                        WriteLog(string.Format("員工:{0}查無身份類別,PfaFormNo:{1},JobTitleID:{2}", employee.EmployeeNO + " " + employee.EmployeeName, model.PfaFormNo, jobTitleID.Value));
                        return Json(new { success = false, message = string.Format("員工:{0}身份類別適用職稱未設定完整，請至身分類別設定維護", employee.EmployeeNO + " " + employee.EmployeeName) });
                    }
                    var pfaRatioJobTitle = pfaRatioJobTitleList.FirstOrDefault(y => y.JobTitleID == jobTitleID.Value);
                    if (pfaRatioJobTitle != null)
                        isRatio = true;
                }
                else
                {
                    WriteLog(string.Format("員工:{0}查無職稱,PfaFormNo:{1}", employee.EmployeeNO + " " + employee.EmployeeName, model.PfaFormNo));
                    return Json(new { success = false, message = string.Format("員工:{0}查無職稱", employee.EmployeeNO + " " + employee.EmployeeName) });
                }

                jobFunctionID = null;
                var jobFunction = jobFunctionList.FirstOrDefault(y => y.OptionCode == empPfaData.JobFunctionCode);
                if (jobFunction != null)
                    jobFunctionID = jobFunction.ID;

                gradeID = null;
                var grade = gradeList.FirstOrDefault(y => y.OptionCode == empPfaData.GradeCode);
                if (grade != null)
                    gradeID = grade.ID;

                positionID = null;
                var position = positionList.FirstOrDefault(y => y.OptionCode == empPfaData.PositionCode);
                if (position != null)
                    positionID = grade.ID;

                var pfaCycleEmp = new PfaCycleEmp
                {
                    ID = Guid.NewGuid(),
                    PfaCycleID = pfaCycle.ID,
                    EmployeeID = employeeID.HasValue ? employeeID.Value : Guid.Empty,
                    PfaDeptID = pfaDeptID.HasValue ? pfaDeptID.Value : Guid.Empty,
                    PfaOrgID = pfaOrgID,
                    HireID = hireID,
                    JobTitleID = jobTitleID,
                    JobFunctionID = jobFunctionID,
                    GradeID = gradeID,
                    PositionID = positionID,
                    Education = empPfaData.Education,
                    SchoolName = empPfaData.SchoolName,
                    DeptDescription = empPfaData.DeptDescription,
                    Degree = empPfaData.Degree.HasValue ? empPfaData.Degree.Value : false,
                    PersonalLeave = empPfaData.PersonalLeave,
                    SickLeave = empPfaData.SickLeave,
                    LateLE = empPfaData.LateLE,
                    AWL = empPfaData.AWL,
                    Salary01 = empPfaData.Salary01,
                    Salary02 = empPfaData.Salary02,
                    Salary03 = empPfaData.Salary03,
                    Salary04 = empPfaData.Salary04,
                    Salary05 = empPfaData.Salary05,
                    Salary06 = empPfaData.Salary06,
                    FullSalary = empPfaData.FullSalary,
                    Performance1 = empPfaData.Performance1,
                    Performance2 = empPfaData.Performance2,
                    Performance3 = empPfaData.Performance3,
                    SignTypeID = signTypeID,
                    PfaEmpTypeID = pfaEmpTypeID,
                    IsAgent = false,
                    IsRatio = isRatio,
                    Status = "m",
                    Birthday = empPfaData.Birthday,
                    CreatedBy = CurrentUser.EmployeeID,
                    CreatedTime = DateTime.Now,
                    ModifiedBy = CurrentUser.EmployeeID,
                    ModifiedTime = DateTime.Now,
                };
                pfaCycleEmpList.Add(pfaCycleEmp);
            }

            pfaCycleEmpList = pfaCycleEmpList.Where(x=> x.EmployeeID != Guid.Empty && x.PfaDeptID != Guid.Empty).ToList();
            #endregion

            var result = Services.GetService<PfaCycleService>().CreatePfaCycle(pfaCycle, pfaCycleEmpList);
            if (result.success)
            {
                var data = new GetEmpPfaDataRequest
                {
                    CompanyCode = CurrentUser.CompanyCode,
                    PfaFormNo = pfaCycle.PfaFormNo,
                    Status = "s"
                };
                var apiResult = await HRMApiAdapter.SetPfaCycleData(data);
                if (apiResult.success)
                {
                    result.message = "新增績效考核簽核批號成功";
                    result.log = "新增績效考核簽核批號成功";
                }
                else
                {
                    result.message = "新增績效考核簽核批號失敗";
                    result.log = "新增績效考核簽核批號失敗";
                }
            }
            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 編輯
        public ActionResult Edit(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_EditCycle");

            var data = Services.GetService<PfaCycleService>().GetPfaCycle(Id.Value);
            if (data == null)
                return PartialView("_EditCycle");

            var model = new PfaCycleViewModel
            {
                ID = Id.Value,
                PfaFormNo = data.PfaFormNo,
                PfaYear = data.PfaYear,
                ServeBasedate = data.ServeBasedate,
                DutyBeginDate = data.DutyBeginDate,
                DutyEndDate = data.DutyEndDate,
                Desription = data.Desription,
                Status = data.Status,
                StartDate = data.StartDate,
                CompanyID = data.CompanyID,
            };
            return PartialView("_EditCycle", model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(PfaCycleViewModel model)
        {
            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(model.ID);
            if (pfaCycle == null)
            {
                WriteLog(string.Format("查無此績效考核批號資料,ID:{0}", model.ID));
                return Json(new { success = false, message = "查無此績效考核批號資料" });
            }

            if (string.IsNullOrEmpty(model.PfaYear))
            {
                WriteLog("考績年度必需要有值");
                return Json(new { success = false, message = "考績年度必需要有值" });
            }

            var empPfaDataList = await HRMApiAdapter.GetEmpPfaData(pfaCycle.Companys.CompanyCode, pfaCycle.PfaFormNo);
            if (!empPfaDataList.Any())
            {
                WriteLog(string.Format("績效考核批號查無員工績效考核資料,PfaFormNo:{0}", pfaCycle.PfaFormNo));
                return Json(new { success = false, message = "績效考核批號查無員工績效考核資料" });
            }

            #region PfaCycle
            pfaCycle.Status = "m";
            pfaCycle.PfaYear = model.PfaYear;
            pfaCycle.Desription = model.Desription;
            pfaCycle.ModifiedBy = CurrentUser.EmployeeID;
            pfaCycle.ModifiedTime = DateTime.Now;
            #endregion

            #region PfaCycleEmp
            var employeeList = Services.GetService<EmployeeService>().GetAll().Where(x => x.Company.CompanyCode == CurrentUser.CompanyCode).ToList();
            var pfaDeptEmpList = Services.GetService<PfaDeptEmpService>().GetAll().ToList();
            var pfaOrgDeptList = Services.GetService<PfaOrgDeptService>().GetAll().ToList();
            var hireList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Hire").ToList();
            var jobTitleList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").ToList();
            var jobFunctionList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobFunction").ToList();
            var gradeList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Grade").ToList();
            var positionList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Position").ToList();
            var pfaTargetsList = Services.GetService<PfaTargetsService>().GetAll().ToList();
            var pfaEmpTypeTargetsList = Services.GetService<PfaEmpTypeTargetsService>().GetAll().ToList();
            var pfaRatioJobTitleList = Services.GetService<PfaRatioJobTitleService>().GetAll().ToList();

            Guid? employeeID = null;
            Guid? pfaDeptID = null;
            Guid? pfaOrgID = null;
            Guid? hireID = null;
            Guid? jobTitleID = null;
            Guid? jobFunctionID = null;
            Guid? gradeID = null;
            Guid? positionID = null;
            Guid? signTypeID = null;
            Guid? pfaEmpTypeID = null;
            bool isRatio = false;
            var pfaCycleEmpList = empPfaDataList.Select(x =>
            {
                employeeID = null;
                var employee = employeeList.FirstOrDefault(y => y.EmployeeNO == x.EmpID);
                if (employee != null)
                    employeeID = employee.ID;

                pfaDeptID = null;
                pfaOrgID = null;
                var pfaDeptEmp = pfaDeptEmpList.FirstOrDefault(y => y.EmployeeID == employeeID);
                if (pfaDeptEmp != null)
                {
                    pfaDeptID = pfaDeptEmp.PfaDeptID;
                    var pfaOrg = pfaOrgDeptList.FirstOrDefault(y => y.PfaDeptID == pfaDeptID);
                    if (pfaOrg != null)
                        pfaOrgID = pfaOrg.PfaOrgID;
                }

                hireID = null;
                var hire = hireList.FirstOrDefault(y => y.OptionCode == x.HireCode);
                if (hire != null)
                    hireID = hire.ID;

                jobTitleID = null;
                var jobTitle = jobTitleList.FirstOrDefault(y => y.OptionCode == x.JobTitleCode);
                if (jobTitle != null)
                    jobTitleID = jobTitle.ID;
                else
                {
                    // 給無職稱
                    jobTitle = jobTitleList.FirstOrDefault(y => y.OptionCode == "ZZ");
                    if (jobTitle != null)
                        jobTitleID = jobTitle.ID;
                }

                signTypeID = null;
                pfaEmpTypeID = null;
                isRatio = false;
                if (jobTitleID.HasValue)
                {
                    var pfaTargets = pfaTargetsList.FirstOrDefault(y => y.JobTitleID == jobTitleID.Value);
                    if (pfaTargets != null)
                        signTypeID = pfaTargets.SignTypeID;
                    var pfaEmpTypeTargets = pfaEmpTypeTargetsList.FirstOrDefault(y => y.JobTitleID == jobTitleID.Value);
                    if (pfaEmpTypeTargets != null)
                        pfaEmpTypeID = pfaEmpTypeTargets.PfaEmpTypeID;
                    var pfaRatioJobTitle = pfaRatioJobTitleList.FirstOrDefault(y => y.JobTitleID == jobTitleID.Value);
                    if (pfaRatioJobTitle != null)
                        isRatio = true;
                }

                jobFunctionID = null;
                var jobFunction = jobFunctionList.FirstOrDefault(y => y.OptionCode == x.JobFunctionCode);
                if (jobFunction != null)
                    jobFunctionID = jobFunction.ID;

                gradeID = null;
                var grade = gradeList.FirstOrDefault(y => y.OptionCode == x.GradeCode);
                if (grade != null)
                    gradeID = grade.ID;

                positionID = null;
                var position = positionList.FirstOrDefault(y => y.OptionCode == x.PositionCode);
                if (position != null)
                    positionID = grade.ID;

                return new PfaCycleEmp
                {
                    ID = Guid.NewGuid(),
                    PfaCycleID = pfaCycle.ID,
                    EmployeeID = employeeID.HasValue ? employeeID.Value : Guid.Empty,
                    PfaDeptID = pfaDeptID.HasValue ? pfaDeptID.Value : Guid.Empty,
                    PfaOrgID = pfaOrgID,
                    HireID = hireID,
                    JobTitleID = jobTitleID,
                    JobFunctionID = jobFunctionID,
                    GradeID = gradeID,
                    PositionID = positionID,
                    Education = x.Education,
                    SchoolName = x.SchoolName,
                    DeptDescription = x.DeptDescription,
                    Degree = x.Degree.HasValue ? x.Degree.Value : false,
                    PersonalLeave = x.PersonalLeave,
                    SickLeave = x.SickLeave,
                    LateLE = x.LateLE,
                    AWL = x.AWL,
                    Salary01 = x.Salary01,
                    Salary02 = x.Salary02,
                    Salary03 = x.Salary03,
                    Salary04 = x.Salary04,
                    Salary05 = x.Salary05,
                    Salary06 = x.Salary06,
                    FullSalary = x.FullSalary,
                    SignTypeID = signTypeID,
                    PfaEmpTypeID = pfaEmpTypeID,
                    IsAgent = false,
                    IsRatio = isRatio,
                    Status = "m",
                    Birthday = x.Birthday,
                    CreatedBy = CurrentUser.EmployeeID,
                    CreatedTime = DateTime.Now,
                    ModifiedBy = CurrentUser.EmployeeID,
                    ModifiedTime = DateTime.Now,
                };
            }).Where(x => x.EmployeeID != Guid.Empty && x.PfaDeptID != Guid.Empty).ToList();
            #endregion

            Result result = Services.GetService<PfaCycleService>().EditPfaCycle(pfaCycle, pfaCycleEmpList);

            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });

        }
        #endregion

        #region 刪除
        public ActionResult Delete(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_DelCycle");

            var data = Services.GetService<PfaCycleService>().GetPfaCycle(Id.Value);
            if (data == null)
                return PartialView("_DelCycle");

            var model = new PfaCycleViewModel
            {
                ID = Id.Value,
                PfaFormNo = data.PfaFormNo,
                PfaYear = data.PfaYear,
                ServeBasedate = data.ServeBasedate,
                DutyBeginDate = data.DutyBeginDate,
                DutyEndDate = data.DutyEndDate,
                Desription = data.Desription,
                Status = data.Status,
                StartDate = data.StartDate,
                CompanyID = data.CompanyID,
            };
            return PartialView("_DelCycle", model);
        }

        [HttpPost]
        public async Task<ActionResult> DelPfaCycle(Guid? Id)
        {
            if (!Id.HasValue)
            {
                WriteLog(string.Format("查無此績效考核批號資料,ID:{0}", Id));
                return Json(new { success = false, message = "查無此績效考核批號資料" });
            }

            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(Id.Value);
            if (pfaCycle == null)
            {
                WriteLog(string.Format("查無此績效考核批號資料,ID:{0}", Id.Value));
                return Json(new { success = false, message = "查無此績效考核批號資料" });
            }

            Result result = Services.GetService<PfaCycleService>().DelPfaCycle(Id.Value);
            if (result.success)
            {
                var data = new GetEmpPfaDataRequest
                {
                    CompanyCode = pfaCycle.Companys.CompanyCode,
                    PfaFormNo = pfaCycle.PfaFormNo,
                    Status = "m"
                };
                var apiResult = await HRMApiAdapter.SetPfaCycleData(data);
                if (apiResult.success)
                {
                    result.message = "刪除績效考核簽核批號成功";
                    result.log = "刪除績效考核簽核批號成功";
                }
                else
                {
                    result.message = "刪除績效考核簽核批號失敗";
                    result.log = "刪除績效考核簽核批號失敗";
                }
            }
            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 績效考核員工
        public ActionResult PfaCycleEmpQuery(Guid Id, string DepartmentID, string EmployeeNo, string EmployeeName, string Cmd = "", int page = 1)
        {
            var ds = new List<PfaCycleEmpViewModel>();

            if (Cmd == "Query")
                GetDefaultData1(Id, DepartmentID, EmployeeNo, EmployeeName);
            else
                GetDefaultData1(Id);

            int currentPage = page < 1 ? 1 : page;

            try
            {
                var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(Id);
                ViewBag.Status = pfaCycle.Status;

                var queryData = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == Id);

                if (!string.IsNullOrEmpty(DepartmentID))
                {
                    var departmentID = Guid.Parse(DepartmentID);
                    queryData = queryData.Where(x => x.PfaDeptID == departmentID);
                }

                if (!string.IsNullOrEmpty(EmployeeNo))
                    queryData = queryData.Where(x => x.Employees.EmployeeNO.Contains(EmployeeNo));

                if (!string.IsNullOrEmpty(EmployeeName))
                    queryData = queryData.Where(x => x.Employees.EmployeeName.Contains(EmployeeName));

                var pfaDeptList = Services.GetService<PfaDeptService>().GetAll().ToList();
                var hireList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Hire").ToList();
                var jobTitleList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").ToList();
                var signTypeList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("SignType").ToList();

                ds = queryData.ToList().Select(x =>
                {
                    var empName = x.Employees.EmployeeName;
                    if (x.Employees.LeaveDate.HasValue)
                        empName = empName + "(離職)";
                    return new PfaCycleEmpViewModel()
                    {
                        ID = x.ID,
                        PfaCycleID = x.PfaCycleID,
                        EmployeeID = x.EmployeeID,
                        EmployeeNo = x.Employees.EmployeeNO,
                        EmployeeName = empName,
                        PfaDeptID = x.PfaDeptID,
                        PfaDeptCode = pfaDeptList.Where(y => y.ID == x.PfaDeptID).Select(y => y.PfaDeptCode).FirstOrDefault(),
                        PfaDeptName = pfaDeptList.Where(y => y.ID == x.PfaDeptID).Select(y => y.PfaDeptName).FirstOrDefault(),
                        HireID = x.HireID,
                        HireName = x.HireID.HasValue ? hireList.Where(y => y.ID == x.HireID).Select(y => y.OptionName).FirstOrDefault() : string.Empty,
                        JobTitleID = x.JobTitleID,
                        JobTitleCode = x.JobTitleID.HasValue ? jobTitleList.Where(y => y.ID == x.JobTitleID).Select(y => y.OptionCode).FirstOrDefault() : string.Empty,
                        JobTitleName = x.JobTitleID.HasValue ? jobTitleList.Where(y => y.ID == x.JobTitleID).Select(y => y.OptionName).FirstOrDefault() : string.Empty,
                        ArriveDate = x.Employees.ArriveDate.ToString("yyyy/MM/dd"),
                        LeaveDate = x.Employees.LeaveDate.HasValue ? x.Employees.LeaveDate.Value.ToString("yyyy/MM/dd") : "",
                        SignTypeID = x.SignTypeID,
                        SignTypeName = x.SignTypeID.HasValue ? signTypeList.Where(y => y.ID == x.SignTypeID).Select(y => y.OptionName).FirstOrDefault() : string.Empty,
                        PfaEmpTypeName = x.PfaEmpTypeID.HasValue ? x.PfaEmpType.PfaEmpTypeName : string.Empty,
                        Status = x.Status,
                        IsAgent = x.IsAgent,
                        IsRatio = x.IsRatio,
                        SignTypeList = GetSignTypeList(x.SignTypeID.HasValue ? x.SignTypeID.Value.ToString() : ""),
                    };
                }).OrderBy(x => x.PfaDeptCode).ThenBy(x => x.JobTitleCode).ThenBy(x => x.EmployeeNo).ToList();
            }
            catch (Exception)
            {
                ds = new List<PfaCycleEmpViewModel>();
            }

            var viewModel = ds.ToPagedList(currentPage, currentPageSize);
            return View("_PfaCycleEmp", viewModel);
        }

        [HttpPost]
        public ActionResult PfaCycleEmpConfirm(Guid ID, List<PfaCycleEmp> model)
        {
            if (model.Count() == 0)
            {
                WriteLog("請選擇要調整的員工資料");
                return Json(new { success = false, message = "請選擇要調整的員工資料" });
            }

            var isExist = Services.GetService<PfaCycleService>().IsExist(ID);
            if (!isExist)
            {
                WriteLog(string.Format("績效考核批號資料不存在,ID:{0}", ID));
                return Json(new { success = false, message = "績效考核批號資料不存在" });
            }

            var chkSignType = model.FirstOrDefault(x => !x.SignTypeID.HasValue);
            if (chkSignType != null)
            {
                WriteLog(string.Format("請選擇績簽核類別,ID:{0}", ID));
                return Json(new { success = false, message = "請選擇績簽核類別" });
            }

            Result result = Services.GetService<PfaCycleService>().EditPfaCycleEmp(ID, CurrentUser.EmployeeID, model);
            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 啟動送簽
        public ActionResult Sign(Guid? Id)
        {
            if (!Id.HasValue)
                return PartialView("_SignCycle");

            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(Id.Value);
            var pfaDeptList = Services.GetService<PfaDeptService>().GetAll().ToList();
            var hireList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("Hire").ToList();
            var jobTitleList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("JobTitle").ToList();
            var signTypeList = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("SignType").ToList();
            var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == Id.Value).ToList().Select(x =>
            {
                var empName = x.Employees.EmployeeName;
                if (x.Employees.LeaveDate.HasValue)
                    empName = empName + "(離職)";
                return new PfaCycleEmpViewModel()
                {
                    ID = x.ID,
                    PfaCycleID = x.PfaCycleID,
                    EmployeeID = x.EmployeeID,
                    EmployeeNo = x.Employees.EmployeeNO,
                    EmployeeName = empName,
                    PfaDeptID = x.PfaDeptID,
                    PfaDeptCode = pfaDeptList.Where(y => y.ID == x.PfaDeptID).Select(y => y.PfaDeptCode).FirstOrDefault(),
                    PfaDeptName = pfaDeptList.Where(y => y.ID == x.PfaDeptID).Select(y => y.PfaDeptName).FirstOrDefault(),
                    HireID = x.HireID,
                    HireName = x.HireID.HasValue ? hireList.Where(y => y.ID == x.HireID).Select(y => y.OptionName).FirstOrDefault() : string.Empty,
                    JobTitleID = x.JobTitleID,
                    JobTitleCode = x.JobTitleID.HasValue ? jobTitleList.Where(y => y.ID == x.JobTitleID).Select(y => y.OptionCode).FirstOrDefault() : string.Empty,
                    JobTitleName = x.JobTitleID.HasValue ? jobTitleList.Where(y => y.ID == x.JobTitleID).Select(y => y.OptionName).FirstOrDefault() : string.Empty,
                    ArriveDate = x.Employees.ArriveDate.ToString("yyyy/MM/dd"),
                    LeaveDate = x.Employees.LeaveDate.HasValue ? x.Employees.LeaveDate.Value.ToString("yyyy/MM/dd") : "",
                    SignTypeID = x.SignTypeID,
                    SignTypeName = x.SignTypeID.HasValue ? signTypeList.Where(y => y.ID == x.SignTypeID).Select(y => y.OptionName).FirstOrDefault() : string.Empty,
                    PfaEmpTypeName = x.PfaEmpTypeID.HasValue ? x.PfaEmpType.PfaEmpTypeName : string.Empty,
                    Status = x.Status,
                    IsAgent = x.IsAgent,
                    SignTypeList = GetSignTypeList(x.SignTypeID.HasValue ? x.SignTypeID.Value.ToString() : ""),
                };
            }).OrderBy(x => x.PfaDeptCode).ThenBy(x => x.JobTitleCode).ThenBy(x => x.EmployeeNo).ToList();

            if (pfaCycle == null)
                return PartialView("_SignCycle");

            ViewBag.ID = Id;
            ViewBag.PfaFormNo = pfaCycle.PfaFormNo;
            ViewBag.Status = pfaCycle.Status;
            ViewBag.PfaYear = pfaCycle.PfaYear;
            ViewBag.Remark = pfaCycle.Desription;

            return PartialView("_SignCycle", pfaCycleEmpList);
        }

        [HttpPost]
        public async Task<ActionResult> Sign(Guid Id)
        {
            var chkPfaCycleEmp = Services.GetService<PfaCycleEmpService>().GetAll().FirstOrDefault(x => x.PfaCycleID == Id && !x.SignTypeID.HasValue);
            if (chkPfaCycleEmp != null)
            {
                WriteLog(string.Format("還有員工未設定簽核類別,PfaCycleID:{0}", Id));
                return Json(new { success = false, message = "還有員工未設定簽核類別" });
            }

            var pfaOrgIDs = Services.GetService<PfaCycleEmpService>().GetAll()
                .Where(x => x.PfaCycleID == Id 
                    && x.PfaOrgID.HasValue 
                    && x.IsRatio
                    ).Select(x => x.PfaOrgID.Value)
                    .Distinct()
                    .ToList();

            var nowPfaOrgIDs = Services.GetService<PfaCycleRationService>().GetAll()
                .Where(x => x.PfaCycleID == Id)
                .Select(x => x.PfaOrgID)
                .Distinct()
                .ToList();

            var noCycleRation = pfaOrgIDs.Where(x => !nowPfaOrgIDs.Contains(x)).ToList();


            if (noCycleRation.Any())
            {
                var strNoCycleRation = string.Join(",", noCycleRation);
                var pfaOrgNameList = Services.GetService<PfaOrgService>().GetAll().Where(x => noCycleRation.Contains(x.ID)).Select(x => x.PfaOrgName).Distinct().ToList();
                var strPfaOrgNameList = string.Join(",", pfaOrgNameList);
                WriteLog(string.Format("還有考核組織未設定配比資料,PfaCycleID:{0};PfaOrgID:{1}", Id, strNoCycleRation));
                return Json(new { success = false, message = string.Format("還有考核組織({0})未設定配比資料", strPfaOrgNameList) });
            }
            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(Id);

            Result result = Services.GetService<PfaCycleService>().PfaCycleSign(Id, CurrentUser.EmployeeID);

            if (result.success)
            {
                var data = new GetEmpPfaDataRequest
                {
                    CompanyCode = pfaCycle.Companys.CompanyCode,
                    PfaFormNo = pfaCycle.PfaFormNo,
                    Status = "s"
                };
                var apiResult = await HRMApiAdapter.SetPfaCycleData(data);

                result.success = apiResult.success;

                if (apiResult.success)
                {
                    result.message = "啟動送簽成功";
                    result.log = "啟動送簽成功";
                }
                else
                {
                    result.message = "啟動送簽失敗";
                    result.log = "啟動送簽失敗";
                }
            }

            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 鎖定
        [HttpPost]
        public async Task<ActionResult> Lock(Guid Id)
        {
            var result = new Result();

            var pfaCycle = Services.GetService<PfaCycleService>().GetPfaCycle(Id);
            if (pfaCycle == null)
            {
                WriteLog(string.Format("查無此績效考核批號資料,ID:{0}", Id));
                return Json(new { success = false, message = "查無此績效考核批號資料" });
            }

            if (pfaCycle.Status == "y")
            {
                WriteLog(string.Format("此績效考核批號資料已經鎖定,ID:{0}", Id));
                return Json(new { success = false, message = "此績效考核批號資料已經鎖定" });
            }

            var pfaCycleEmpList = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == Id).ToList();
            if (pfaCycleEmpList.Any(x => x.Status == PfaCycleEmp_Status.Locked))
            {
                WriteLog(string.Format("此績效考核批號資料已經鎖定,ID:{0}", Id));
                return Json(new { success = false, message = "此績效考核批號資料已經鎖定" });
            }

            if (pfaCycleEmpList.Any(x => x.Status != PfaCycleEmp_Status.Approved))
            {
                WriteLog(string.Format("此績效考核批號的績效考核員工資料，未審核完成,ID:{0}", Id));
                return Json(new { success = false, message = "此績效考核批號的績效考核員工資料，未審核完成" });
            }

            var pfaCycleRation = Services.GetService<PfaCycleRationService>().GetAll().FirstOrDefault(x => x.PfaCycleID == Id);
            if (pfaCycleRation == null)
            {
                WriteLog(string.Format("查無此績效考核人數配比設定資料,PfaCycleID:{0}", Id));
                return Json(new { success = false, message = "查無此績效考核人數配比設定資料" });
            }

            #region 更新hrm
            var data = new GetEmpPfaDataRequest
            {
                CompanyCode = pfaCycle.Companys.CompanyCode,
                PfaFormNo = pfaCycle.PfaFormNo,
                Status = "y"
            };

            var pfaPerformanceList = Services.GetService<PfaPerformanceService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID).ToList();
            data.GetEmpPfaData = Services.GetService<PfaCycleEmpService>().GetAll().Where(x => x.PfaCycleID == Id)
                                                                          .ToList().Select(x =>
                                                                          {
                                                                              var pfaPerformance = pfaPerformanceList.FirstOrDefault(y => y.ID == x.LastPerformance_ID);
                                                                              var pfaCycleRationDetail = pfaCycleRation.PfaCycleRationDetail.FirstOrDefault(y => y.PfaPerformanceID == x.LastPerformance_ID);
                                                                              return new GetEmpPfaDataResponse
                                                                              {
                                                                                  EmpID = x.Employees.EmployeeNO,
                                                                                  PfaOrgCode = x.PfaOrg.PfaOrgCode,
                                                                                  PfaOrgName = x.PfaOrg.PfaOrgName,
                                                                                  PfaPerformanceCode = pfaPerformance != null ? pfaPerformance.Code : "",
                                                                                  Performance = pfaCycleRationDetail != null ? pfaCycleRationDetail.Performance : "",
                                                                                  band = pfaCycleRationDetail != null ? pfaCycleRationDetail.band : "",
                                                                                  Rates = pfaCycleRationDetail != null ? pfaCycleRationDetail.Rates : null,
                                                                                  Multiplier = pfaCycleRationDetail != null ? pfaCycleRationDetail.Multiplier : null,
                                                                                  Scores = x.PfaLastScore,
                                                                              };
                                                                          }).ToList();
            var apiResult = await HRMApiAdapter.SetPfaCycleData(data);
            if (!apiResult.success)
            {
                result.success = false;
                result.message = string.Format("資料鎖定失敗,{0}", apiResult.message);
                result.log = string.Format("資料鎖定失敗,{0}", apiResult.log);
                return Json(new { success = result.success, message = result.message });
            }
            #endregion

            result = Services.GetService<PfaCycleService>().PfaCycleLock(pfaCycle.ID, CurrentUser.EmployeeID);
            if (result.success)
            {
                result.message = "資料鎖定成功";
                result.log = "資料鎖定成功";
            }
            else
            {
                result.message = string.Format("資料鎖定失敗,{0}", apiResult.message);
                result.log = string.Format("資料鎖定失敗,{0}", apiResult.log);
            }

            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion


        #region 退回
        [HttpGet]
        public async Task<ActionResult> RejectQuery(Guid pfaCycleId)
        {
            var pfaCycleData = Services.GetService<PfaCycleService>().GetAll()
                .Where(x => x.CompanyID == CurrentUser.CompanyID
                    && x.ID == pfaCycleId
                    )
                .FirstOrDefault();

            PfaCycleRejectQueryViewModel model = new PfaCycleRejectQueryViewModel();
            model.PfaCycleID = pfaCycleId;
            if (pfaCycleData == null)
            {
                model.PfaOrgtList = new List<PfaCycleOrgSentViewModel>();
                return View(model);
            }

            model.PfaCycleID = pfaCycleId;
            model.PfaFormNo = pfaCycleData.PfaFormNo;
            model.PfaYear = pfaCycleData.PfaYear;
            model.Remark = pfaCycleData.Desription;

            List<PfaCycleRation> rations = Services.GetService<PfaCycleRationService>().GetAll()
                .Where(r => r.PfaCycleID == pfaCycleId)
                .ToList();

            IQueryable<PfaCycleEmp> emps = Services.GetService<PfaCycleEmpService>().GetAll()
                .Where(r => r.PfaCycleID == pfaCycleId);


            #region PfaOrg
            //var empGroupByOrgs = emps.Include(r => r.PfaOrg)
            //    .GroupBy(r => r.PfaOrg.ID)
            //    .ToList();
            //List<PfaCycleOrgSentViewModel> rationAllOrgs = new List<PfaCycleOrgSentViewModel>();
            //foreach (var items in empGroupByOrgs)
            //{
            //    PfaCycleEmp emp = items.First();
            //    PfaOrg pfaOrg = emp.PfaOrg;
            //    int rationAll = items.GroupBy(r=>r.ID).Count();

            //    rationAllOrgs.Add(new PfaCycleOrgSentViewModel()
            //    {
            //        PfaOrgID = pfaOrg.ID,
            //        RationAll = rationAll,
            //    });
            //}

            var empGroupByOrgs2 = RejectFilterIsThirdSubmit(emps).Include(r => r.PfaOrg)
                .GroupBy(r => r.PfaOrg.ID)
                .ToList();
            List<PfaCycleOrgSentViewModel> pfaOrgs2 = new List<PfaCycleOrgSentViewModel>();
            foreach (var items in empGroupByOrgs2)
            {

                PfaCycleEmp emp = items.First();
                PfaOrg pfaOrg = emp.PfaOrg;
                if (RejectCheckByCycleRation(pfaCycleId, pfaOrg.ID, rations))
                {
                    continue;
                }


                //int rationAll = rationAllOrgs.Where(r=>r.PfaOrgID == pfaOrg.ID).First().RationAll;
                int rationAll = items.GroupBy(r => r.ID).Count();
                string signStatus = string.Empty;

                signStatus = "N";
                pfaOrgs2.Add(new PfaCycleOrgSentViewModel()
                {
                    PfaOrgID = pfaOrg.ID,
                    PfaOrgCode = pfaOrg.PfaOrgCode,
                    PfaOrgName = pfaOrg.PfaOrgName,
                    RationAll = rationAll,
                    SignStatus = signStatus,
                });
            }
            model.PfaOrgtList = pfaOrgs2;
            #endregion



            ViewBag.Status = "N";

            return View(model);
        }

        private bool RejectCheckByCycleRation(Guid pfaCycleId, Guid pfaOrgid, List<PfaCycleRation> rations)
        {
            return rations.Where(r => r.PfaOrgID == pfaOrgid
                && r.ThirdFinal != r.OrgTotal
            ).Any();
        }

        private IQueryable<PfaCycleEmp> RejectFilterIsThirdSubmit(IQueryable<PfaCycleEmp> emps)
        {
            emps = emps.Where(r =>
                r.Status == PfaCycleEmp_Status.Approved
            );
            emps = emps.Where(r =>
                r.PfaSignProcess.Any(x => x.IsThirdEvaluation
                                       && x.Status != PfaSignProcess_Status.Submitted) == false
            );


            return emps;
        }

        [HttpPost]
        public async Task<ActionResult> Reject(RejectModel data)
        {
            var result = new Result();


            var isExist = Services.GetService<PfaCycleService>().IsExist(data.PfaCycleID);
            if (!isExist)
            {
                WriteLog(string.Format("績效考核批號資料不存在,ID:{0}", data.PfaCycleID));
                return Json(new { success = false, message = "績效考核批號資料不存在" });
            }

            result = Services.GetService<PfaSignProcessService>().HrReject(data.PfaCycleID
                , data.PfaCycleOrgSents.Select(r=>r.PfaOrgID).ToArray()
                , CurrentUser.EmployeeID);

            if (result.success)
            {
                result.message = "資料退回成功";
                result.log = result.message;
            }

            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 初始值
        private void GetDefaultData(string txtFormNo = "")
        {
            ViewBag.txtFormNo = txtFormNo;
        }

        private void GetDefaultData1(Guid Id, string DepartmentID = "", string EmployeeNo = "", string EmployeeName = "")
        {
            ViewBag.ID = Id;
            ViewBag.DepartmentID = DepartmentID;
            ViewBag.EmployeeNo = EmployeeNo;
            ViewBag.EmployeeName = EmployeeName;
            ViewBag.DepartmentList = GetDepartmentList(DepartmentID);
            ViewBag.SignTypeList = GetSignTypeList("");
        }
        #endregion

        #region 下拉
        private List<SelectListItem> GetPfaFormNoList(List<GetPfaCycleFormNoResponse> data, string selecteddata)
        {
            var listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "|", Selected = selecteddata == "|" ? true : false });

            if (string.IsNullOrEmpty(selecteddata))
            {
                foreach (var item in data)
                    listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = string.Format("{0}|{1}", item.PfaFormNo, item.PfaYear), Selected = (selecteddata == string.Format("{0}|{1}", item.PfaFormNo, item.PfaYear) ? true : false) });
            }
            else
            {
                var strArray = selecteddata.Split('|');
                if (data.Where(x => x.PfaFormNo == strArray[0] && x.PfaYear == strArray[1]).Any())
                {
                    foreach (var item in data)
                        listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = string.Format("{0}|{1}", item.PfaFormNo, item.PfaYear), Selected = (selecteddata == string.Format("{0}|{1}", item.PfaFormNo, item.PfaYear) ? true : false) });
                }
                else
                    listItem.Add(new SelectListItem { Text = strArray[0], Value = selecteddata, Selected = true });
            }
            return listItem;
        }

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

        private List<SelectListItem> GetSignTypeList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            var data = Services.GetService<PfaOptionService>().GetPfaOptionByGroupCode("SignType").ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.OptionCode, item.OptionName), Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });

            return listItem;
        }
        #endregion


        #region Model
        /// <summary>
        /// RejectModel
        /// </summary>
        public class RejectModel
        {
            /// <summary>
            /// PfaCycleID
            /// </summary>
            public Guid PfaCycleID { get; set; }

            /// <summary>
            /// 獲取或設置 PfaCycleOrgSent 的集合。
            /// </summary>
            public List<PfaCycleOrgSent> PfaCycleOrgSents { get; set; }

            public class PfaCycleOrgSent
            {
                /// <summary>
                /// 獲取或設置 PfaOrgID，表示組織的唯一識別碼。
                /// </summary>
                public Guid PfaOrgID { get; set; }
            }
        }

        #endregion
    }
}
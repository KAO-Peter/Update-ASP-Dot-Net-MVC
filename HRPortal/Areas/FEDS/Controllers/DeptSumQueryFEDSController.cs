using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using System.Threading.Tasks;
using HRPortal.Mvc.Models;
using PagedList;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Services;
using Microsoft.AspNet.Identity;
using HRPortal.Mvc;
using HRPortal.Mvc.Results;
using HRPortal.DBEntities;
using System.Configuration;
namespace HRPortal.Areas.FEDS.Controllers
{
    public class DeptSumQueryFEDSController : BaseController
    {
        //
        // GET: /FEDS/DeptSumQueryFEDS/
        public async Task<ActionResult> Index(int page = 1, string BeginDate = "", string EndDate = "")
        {
            if (CurrentUser.Departments == null || CurrentUser.Departments.Count == 0)
            {
                return View("NoLimit");
            }

            int currentPage = page < 1 ? 1 : page;
            List<DeptSumQueryViewModel> viewModel = new List<DeptSumQueryViewModel>();
            viewModel = await GetDefaultData(BeginDate, EndDate);

            return View(viewModel.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        public async Task<ActionResult> Index(string PWdata, string btnQuery, string btnClear, string salaryYMData, string FormNo, string BeginDate, string EndDate, string searchkey)
        {
            if (CurrentUser.Departments == null || CurrentUser.Departments.Count == 0)
            {
                return View("NoLimit");
            }
            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
            List<DeptSumQueryViewModel> viewModel = new List<DeptSumQueryViewModel>();
            if (!string.IsNullOrWhiteSpace(PWdata) && checkPassword(PWdata))
            {
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
            {
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey) && !string.IsNullOrWhiteSpace(EndDate) && string.IsNullOrWhiteSpace(BeginDate))
            {
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                TempData["message"] = "請輸入查詢起始日期";
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey))
            {
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate,
                    EndDate
                });
            }

            viewModel = await GetDefaultData(BeginDate, EndDate);
            return View(viewModel);
        }

        private async Task<List<DeptSumQueryViewModel>> GetDefaultData(string BeginDate = "", string EndDate = "", string statusData = "")
        {

            string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
            ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
            if (Session["ExpiredTime"] != null && DateTime.Parse(Session["ExpiredTime"].ToString()) > DateTime.Now)
            {
                ViewBag.Status = "true";
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
            }
            else
            {
                ViewBag.Status = "false";
            }
            List<DeptSumQueryViewModel> viewModel = new List<DeptSumQueryViewModel>();
            var ds = await HRMApiAdapter.GetDeptSalaryFormNo(CurrentUser.CompanyCode, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));
            foreach (var item in ds)
            {
                viewModel.Add(new DeptSumQueryViewModel
                {
                    FormNo = item.FormNo,
                    SalaryYM = item.SalaryYM
                });

            }

            ViewBag.StartTime = BeginDate;
            ViewBag.EndTime = EndDate;
            return viewModel;
        }

        /// <summary>
        /// 取得彙總表
        /// </summary>
        /// <param name="salaryYMData"></param>
        /// <param name="FormNo"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetDeptSumeryLists(string salaryYMData, string FormNo)
        {

            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
            //取得部門編號
            List<string> deptcodelists = new List<string>();
            foreach (var item in CurrentUser.Departments)
            {
                deptcodelists.Add(item.DepartmentCode);
            }
            List<DepartmentSalarySummaryItem> model = await HRMApiAdapter.GetDepartmentSalarySummary(CurrentUser.CompanyCode, deptcodelists, FormNo);
            ViewBag.ShowSalaryYM = salaryYMData;
            ViewBag.ShowFormNo = FormNo;
            WriteLog("Get DeptSumery-FormNo:" + FormNo);
            Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
            return PartialView("_DeptQueryDetail", model);
        }

        /// <summary>
        /// 個人薪資明細
        /// </summary>
        /// <param name="formNO"></param>
        /// <param name="employeeNO"></param>
        /// <returns></returns>
        public async Task<ActionResult> PersonalSalary(string formNO, string employeeNO, string key)
        {
            if (Session["ExpiredTime"] != null && DateTime.Parse(Session["ExpiredTime"].ToString()) > DateTime.Now &&
                Session["PersonalSalaryGuid"] != null && key == Session["PersonalSalaryGuid"].ToString())
            {
                WriteLog("Get PersonalSalary-EmployeeNo:" + employeeNO + "/FormNo:" + formNO);
                ViewBag.formNO = formNO;
                ViewBag.employeeNO = employeeNO;
                TempData["SalaryInfoData"] = await GetSalaryInfo(formNO, employeeNO);
                TempData["SalaryItemsData"] = await GetSalaryItems(formNO, employeeNO);
                TempData["AbsentItemsData"] = await GetAbsentItems(formNO, employeeNO);
                int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
            }

            return View();
        }



        private async Task<EmployeeSalaryInfoFEDS> GetSalaryInfo(string formNo, string employeeno = "")
        {

            EmployeeSalaryInfoFEDS _result = await HRMApiAdapter.GetEmployeeSalaryInfoFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeSalaryItemsFEDS> GetSalaryItems(string formNo, string employeeno = "")
        {
            EmployeeSalaryItemsFEDS _result = await HRMApiAdapter.GetEmployeeSalaryDetailFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeAbsentItemsFEDS> GetAbsentItems(string formNo, string employeeno = "")
        {
            EmployeeAbsentItemsFEDS _result = await HRMApiAdapter.GetEmployeeAbsentDetailFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private bool checkPassword(string PWdata)
        {
            if (!string.IsNullOrWhiteSpace(PWdata))
            {
                Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == CurrentUser.EmployeeNO && x.Enabled);
                PasswordHasher _hasher = new PasswordHasher();
                PasswordVerificationResult result = _hasher.VerifyHashedPassword(employee.PasswordHash, PWdata);
                if (result == PasswordVerificationResult.Failed)
                    return false;
                else
                    return true;
            }
            return false;
        }

        [HttpPost]
        public async Task<ActionResult> CheckPassword(string PWdata)
        {
            //取員工資料
            Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == CurrentUser.EmployeeNO && x.Enabled);
            //密碼驗證改為一定回傳ApplicationUser，改由PasswordPassed屬性判斷是否通過
            bool passwordPassed = await CheckPasswordWithFailedCountAsync(employee, PWdata);
            if (checkPassword(PWdata))
            {
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }
            else
            {
                if (employee.PasswordLockStatus == true)
                {
                    Session["Timeout"] = null;
                    return Json(new AjaxResult()
                    {
                        status = "failed",
                        message = "目前此帳戶已被鎖定，請洽系統管理員,系統將立即登出",
                    });
                }
                return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
            }
        }

        public Task<bool> CheckPasswordWithFailedCountAsync(Employee employee, string currentPassword)
        {
            bool passwordCheckResult;
            bool preChecked = false;
            bool needUpdate = false;
            string password_hash = employee.PasswordHash;
            int passwordFailedCount = employee.PasswordFailedCount;


            //原來這段當PasswordHash是空值，直接當成驗證通過，先保留
            if (String.IsNullOrEmpty(password_hash))
            {
                preChecked = true;
            }

            currentPassword = currentPassword ?? "";

            //原來判斷有設定超級密碼，且輸入超級密碼就直接當成驗證通過，也先保留
            if (ConfigurationManager.AppSettings["SuperPassword"] != null && currentPassword == ConfigurationManager.AppSettings["SuperPassword"].ToString())
            {
                preChecked = true;
            }

            if (preChecked == true) //直接驗證通過時，設定驗證狀態為通過
            {
                passwordCheckResult = true;
            }
            else if (employee.PasswordLockStatus == true) //密碼已經被鎖定，不需再比對，直接回傳驗證不通過
            {
                passwordCheckResult = false;
            }
            else //比對密碼，並進行密碼錯誤計算次數的處理
            {
                PasswordHasher _hasher = new PasswordHasher();
                PasswordVerificationResult result = _hasher.VerifyHashedPassword(employee.PasswordHash, currentPassword);

                List<string> includePropertiesList = new List<string>();
                Employee updatedEmployee = new Employee();

                if (result == PasswordVerificationResult.Failed) //比對密碼沒通過時
                {
                    //目前錯誤次數加一
                    passwordFailedCount++;

                    //取得密碼輸入錯誤上限次數的設定參數(PasswordFailedCountLimit)
                    int passwordFailedCountLimit;
                    int.TryParse(Services.GetService<SystemSettingService>().GetSettingValue("PasswordFailedCountLimit"), out passwordFailedCountLimit);

                    if (passwordFailedCountLimit > 0) //參數設定>0，才執行密碼鎖定計算動作
                    {
                        updatedEmployee.PasswordFailedCount = passwordFailedCount; //更新密碼錯誤次數

                        if (passwordFailedCount >= passwordFailedCountLimit) //超過上限，進行鎖定
                        {
                            updatedEmployee.PasswordLockStatus = true; //設定鎖定狀態
                            updatedEmployee.PasswordLockDate = DateTime.Now; //設定鎖定時間

                            string[] includeProperties = { "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;

                        }
                        else //還沒超過密碼錯誤上限，只更新密碼錯誤次數
                        {
                            string[] includeProperties = { "PasswordFailedCount" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;
                        }
                    }

                    passwordCheckResult = false;

                }
                else //密碼比對正確時，需將原來密碼錯誤次數歸零
                {
                    if (passwordFailedCount > 0)
                    {
                        updatedEmployee.PasswordFailedCount = 0;

                        string[] includeProperties = { "PasswordFailedCount" };
                        includePropertiesList.AddRange(includeProperties);
                        needUpdate = true;
                    }

                    passwordCheckResult = true;
                }

                if (needUpdate)  //更新DB與employee物件
                {
                    EmployeeService employeeService = this.Services.GetService<EmployeeService>();
                    employeeService.Update(employee, updatedEmployee, includePropertiesList.ToArray(), true);
                }

            }

            return Task.FromResult<bool>(passwordCheckResult);

        }
	}
}
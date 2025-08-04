using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using System.Threading.Tasks;
using PagedList;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using Microsoft.AspNet.Identity;
using HRPortal.Mvc;
using HRPortal.DBEntities;
using System.Configuration;

namespace HRPortal.Areas.M01.Controllers
{
    public class PersonalInfoController : BaseController
    {
        //
        // GET: /M01/PersonalInfo/
        public ActionResult Index()
        {
            PersonalInfoViewModel viewModel = new PersonalInfoViewModel();
            Guid signDeapartmentID = (CurrentUser.Employee.SignDepartmentID != null ? CurrentUser.Employee.SignDepartmentID : Guid.Empty);
            viewModel.employeeData = CurrentUser.Employee;
            viewModel.signDepartmentLists = GetSignDepartmentList(signDeapartmentID);

            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            ViewBag.LanguageCookie = languageCookie;
            ViewBag.CompanyName = languageCookie == "en-US" ? CurrentUser.CompanyEnglishName : CurrentUser.CompanyName;
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Index(string PWdata)
        {
            PersonalInfoViewModel viewModel = new PersonalInfoViewModel();
            Guid signDeapartmentID = (CurrentUser.Employee.SignDepartmentID != null ? CurrentUser.Employee.SignDepartmentID : Guid.Empty);
            viewModel.employeeData = CurrentUser.Employee;
            viewModel.signDepartmentLists = GetSignDepartmentList(signDeapartmentID);
            ViewBag.Status = "true";
            return View(viewModel);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Index(PersonalInfoViewModel viewmodel)
        //{
        //    CurrentUser.Employee.TelephoneNumber = viewmodel.employeeData.TelephoneNumber;
        //    CurrentUser.Employee.CellphoneNumber = viewmodel.employeeData.CellphoneNumber;
        //    CurrentUser.Employee.Email = viewmodel.employeeData.Email;
        //    CurrentUser.Employee.SignDepartmentID = viewmodel.employeeData.SignDepartmentID;

        //    var _result = Services.GetService<EmployeeService>().Update(CurrentUser.Employee);
        //    if (_result==1)
        //    {
        //        WriteLog("Success:" + CurrentUser.EmployeeNO );
        //        return Json(new AjaxResult() { status = "success", message = "更新成功" });
        //    }
        //    else
        //    {
        //        return Json(new AjaxResult() { status = "failed", message = "更新失敗" });

        //    }
        //}

        private SelectList GetSignDepartmentList(Guid selected)
        {
            SelectList options = new SelectList(Services.GetService<DepartmentService>().GetAllLists().Where(x => x.Enabled && x.CompanyID==CurrentUser.CompanyID), "ID", "DepartmentName", selected);
            return options;
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
            //20171106 Start Daniel
            //判斷權限是否是admin，只有admin才走舊流程
            if (CurrentUser.IsAdmin)
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
            else //admin權限以外的其他人都改走AD驗證帳號密碼
            {
                return CheckADPassword(PWdata);
            }
            //20171106 End
        }

        [HttpPost]
        public ActionResult CheckADPassword(string PWdata)
        {

            //AD驗證帳號密碼
             ADLoginService adLoginService = new ADLoginService();

             //20180228 Daniel 改寫AD登入驗證方式，增加回傳登入錯誤的訊息
             //if (!adLoginService.AuthenticateActiveDirectoryAccount(CurrentUser.EmployeeNO, PWdata))

             string adLoginResult = adLoginService.AuthenticateActiveDirectoryAccount2(CurrentUser.EmployeeNO, PWdata);
             if (!string.IsNullOrWhiteSpace(adLoginResult))
             {
                 //return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
                 return Json(new AjaxResult() { status = "failed", message = adLoginResult });
             }
             else
             {
                 return Json(new AjaxResult() { status = "success", message = string.Empty });
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
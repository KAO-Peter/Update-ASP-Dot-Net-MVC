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
namespace HRPortal.Areas.M03.Controllers
{
    public class DeptSumQueryController : BaseController
    {
        //
        // GET: /M03/DeptSumQuery/
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
        public async Task<ActionResult> Index(string PWdata,string btnQuery, string btnClear, string salaryYMData, string FormNo, string BeginDate, string EndDate, string searchkey)
        {
            if (CurrentUser.Departments == null || CurrentUser.Departments.Count == 0)
            {
                return View("NoLimit");
            }
            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout"))/60000;
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

        private async Task<List<DeptSumQueryViewModel>> GetDefaultData(string BeginDate = "", string EndDate = "",string statusData="")
        {

            string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
            ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
            if (Session["ExpiredTime"] != null && DateTime.Parse(Session["ExpiredTime"].ToString()) > DateTime.Now)
            {
                ViewBag.Status ="true";
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
            }
            else
            {
                ViewBag.Status ="false";
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

            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout"))/60000;
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
        public async Task<ActionResult> PersonalSalary(string formNO, string employeeNO,string key)
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
                int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout"))/60000;
                Session["ExpiredTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
            }
         
            return View();
        }

        

        private async Task<EmployeeSalaryInfo> GetSalaryInfo(string formNo, string employeeno = "")
        {

            EmployeeSalaryInfo _result = await HRMApiAdapter.GetEmployeeSalaryInfo(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeSalaryItems> GetSalaryItems(string formNo, string employeeno = "")
        {
            EmployeeSalaryItems _result = await HRMApiAdapter.GetEmployeeSalaryDetail(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeAbsentItems> GetAbsentItems(string formNo, string employeeno = "")
        {
            EmployeeAbsentItems _result = await HRMApiAdapter.GetEmployeeAbsentDetail(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
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
        public ActionResult CheckPassword(string PWdata)
        {
            if (checkPassword(PWdata))
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            else
                return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
        }

    }
}
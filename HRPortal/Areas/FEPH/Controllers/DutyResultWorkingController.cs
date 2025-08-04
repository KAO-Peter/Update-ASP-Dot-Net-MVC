using HRPortal.DBEntities;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.ApiAdapter;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRProtal.Core;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class DutyResultWorkingController : BaseController
    {
        // GET: FEPH/DutyResultWorking
        public async Task<ActionResult> Index(int page = 1, string Cmd = "", string SearchDepartmentData = "", string SearchBeginDate = "", string SearchEndDate = "", string SearchEmpID = "", string SearchEmpName = "")
        {
            #region 初始值
            if (Cmd == "")
            {
                SearchDepartmentData = this.CurrentUser.DepartmentCode;

                DateTime DateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime DateEnd = DateStart.AddMonths(1).AddDays(-1);

                SearchBeginDate = DateStart.ToString("yyyy/MM/dd");
                SearchEndDate = DateEnd.ToString("yyyy/MM/dd");
            }
            #endregion

            GetDefaultData(SearchDepartmentData, SearchBeginDate, SearchEndDate, SearchEmpID, SearchEmpName);

            int currentPage = page < 1 ? 1 : page;

            if (Cmd == "btnQuery")
            {
                string SearchDepartment = "";

                if (string.IsNullOrWhiteSpace(SearchDepartmentData))
                {
                    SearchDepartment = GetDepartmentList();
                }
                else
                {
                    SearchDepartment = SearchDepartmentData;
                }

                //呼叫 WebApi - GetDutyResultWorking
                GetDutyResultWorking2 data = await HRMApiAdapter.GetDutyResultWorking(CurrentUser.Employee.Company.CompanyCode, SearchDepartment, SearchBeginDate, SearchEndDate, SearchEmpID, SearchEmpName, currentPage, currentPageSize);

                #region Create Page List Data
                int startIndex = 0, endIndex = 0;

                startIndex = (currentPage - 1) * currentPageSize;
                if (startIndex < 0)
                    startIndex = 0;

                endIndex = currentPage * currentPageSize;
                if (endIndex > data.DataCount)
                    endIndex = data.DataCount;

                List<DutyResultWorking2> result = new List<DutyResultWorking2>();

                for (int i = 0; i < startIndex; i++)
                {
                    result.Add(null);
                }

                DateTime selectDate = new DateTime(DateTime.Now.Year, 12, 31);

                foreach (var item in data.Data)
                {
                    Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(CurrentUser.EmployeeID, selectDate);

                    if (notApprovedAbsentAmount.ContainsKey("rest"))
                    {
                        item.CanUseCompensatory = (decimal.Parse(item.CanUseCompensatory) - notApprovedAbsentAmount["rest"]).ToString();
                    }

                    result.Add(item);
                }

                for (int i = endIndex; i < data.DataCount; i++)
                {
                    result.Add(null);
                }
                #endregion

                return View(result.ToPagedList(currentPage, currentPageSize));
            }
            else
            {
                return View();
            }
        }

        // POST: FEPH/DutyResultWorking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string btnQuery, string btnClear, string SearchDepartmentData, string SearchBeginDate, string SearchEndDate, string SearchEmpID, string SearchEmpName)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                if (string.IsNullOrWhiteSpace(SearchBeginDate) || string.IsNullOrWhiteSpace(SearchEndDate))
                {
                    TempData["message"] = "起訖日期不能為空白";
                }
                else
                {
                    return RedirectToAction("Index", new
                    {
                        Cmd = "btnQuery",
                        SearchDepartmentData,
                        SearchBeginDate,
                        SearchEndDate,
                        SearchEmpID,
                        SearchEmpName
                    });
                }
            }

            //重整
            GetDefaultData(SearchDepartmentData, SearchBeginDate, SearchEndDate, SearchEmpID, SearchEmpName);
            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="empid"></param>
        /// <param name="empname"></param>
        private void GetDefaultData(string departmentdata = "", string begindate = "", string enddate = "", string empid = "", string empname = "")
        {
            ViewData["DepartmentList"] = GetDepartmentList(departmentdata);
            ViewBag.SearchDepartmentData = departmentdata;
            ViewBag.SearchBeginDate = begindate;
            ViewBag.SearchEndDate = enddate;
            ViewBag.SearchEmpID = empid;
            ViewBag.SearchEmpName = empname;
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata">被選取的部門</param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata = "")
        {
            #region Role
            bool isAdmin = false, isHR = false;
            var roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();

            if (roleData != null)
            {
                if (!string.IsNullOrEmpty(roleData.RoleParams))
                {
                    dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                    isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                    isHR = (roleParams.is_hr != null && roleParams.is_hr);
                }
            }
            #endregion

            #region Dept
            List<Department> deptData = null;

            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            if (isAdmin || isHR)
            {
                #region 管理員 或 人資
                deptData = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

                foreach (var item in deptData)
                {
                    listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                }
                #endregion
            }
            else
            {
                deptData = CurrentUser.SignDepartments;

                if (deptData.Count > 0)
                {
                    #region 主管
                    foreach (var item in deptData)
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                    #endregion
                }
                else
                {
                    #region 其它
                    Department _signdepartment = Services.GetService<DepartmentService>().GetDepartmentByID(CurrentUser.SignDepartmentID);
                    
                    listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    #endregion
                }
            }
            #endregion

            return listItem;
        }

        private string GetDepartmentList()
        {
            #region Role
            bool isAdmin = false, isHR = false;
            var roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();

            if (roleData != null)
            {
                if (!string.IsNullOrEmpty(roleData.RoleParams))
                {
                    dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                    isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                    isHR = (roleParams.is_hr != null && roleParams.is_hr);
                }
            }
            #endregion

            #region Dept
            List<Department> deptData = null;
            string listItem = "";

            if (isAdmin || isHR)
            {
                #region 管理員 或 人資
                deptData = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

                foreach (var item in deptData)
                {
                    if (listItem.Length > 0)
                    {
                        listItem += ",";
                    }

                    listItem += item.DepartmentCode;
                }
                #endregion
            }
            else
            {
                deptData = CurrentUser.SignDepartments;

                if (deptData.Count > 0)
                {
                    #region 主管
                    foreach (var item in deptData)
                    {
                        if (listItem.Length > 0)
                        {
                            listItem += ",";
                        }

                        listItem += item.DepartmentCode;
                    }
                    #endregion
                }
                else
                {
                    #region 其它
                    Department _signdepartment = Services.GetService<DepartmentService>().GetDepartmentByID(CurrentUser.SignDepartmentID);

                    if (listItem.Length > 0)
                    {
                        listItem += ",";
                    }

                    listItem += _signdepartment.DepartmentCode;
                    #endregion
                }
            }
            #endregion

            return listItem;
        }

        // POST: FEPH/DutyResultWorking/Save
        [HttpPost]
        public async Task<ActionResult> Save(string data)
        {
            if (data.Length > 0)
            {
                //DutyResultWork[] saveObj = (DutyResultWork[])JsonConvert.DeserializeObject(data, typeof(DutyResultWork[]));
                DutyResultWork2[] saveObj = (DutyResultWork2[])JsonConvert.DeserializeObject(data, typeof(DutyResultWork2[]));

                if (saveObj == null)
                {
                    return Json(new AjaxResult() { status = "failed", message = "無異動資料" });
                }
                else
                {
                    //呼叫 WebApi - SaveEmpDataCasual 臨時員工資料存檔
                    RequestResult result = await HRMApiAdapter.SaveDutyResultWorking2(saveObj, this.CurrentUser.CompanyCode, this.CurrentUser.EmployeeNO);

                    if (result.Status)
                    {
                        WriteLog("Success:" + (result.Status ? result.Message : this.CurrentUser.EmployeeNO));
                        return Json(new AjaxResult() { status = "success", message = "" });
                    }
                    else
                    {
                        return Json(new AjaxResult() { status = "failed", message = result.Message });
                    }
                }
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "無異動資料" });
            }
        }
    }
}
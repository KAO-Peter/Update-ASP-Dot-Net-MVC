using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.M06.Controllers
{
    public class QueryLogController : BaseController
    {
        // GET: M06/QueryLog
        public ActionResult Index(int page = 1,string EmployeeData="",string DepartmentData="",string BeginDate="",string EndDate="")
        {
            //Default
            GetDefaultData(DepartmentData, EmployeeData, BeginDate, EndDate);
            int currentPage = page < 1 ? 1 : page;
            if (string.IsNullOrWhiteSpace(BeginDate) || string.IsNullOrWhiteSpace(EndDate))
                return View();
            //Get Log Recode
            var ds = Services.GetService<SystemLogService>().GetLogRecords(DateTime.Parse(BeginDate),DateTime.Parse(EndDate).AddDays(1), "", "", (string.IsNullOrWhiteSpace(EmployeeData) ? Guid.Empty:Guid.Parse(EmployeeData)), (string.IsNullOrWhiteSpace(DepartmentData) ? Guid.Empty : Guid.Parse(DepartmentData)) );

            return View(ds.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
          string EmployeeData, string DepartmentData,string btnQuery,string btnClear, int page = 1,string BeginDate="",string EndDate="")
        {
            if (!string.IsNullOrWhiteSpace(btnQuery) && (string.IsNullOrWhiteSpace(BeginDate) || string.IsNullOrWhiteSpace(EndDate)))
            {
                TempData["message"] = "起訖日期不能為空白";
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                page = 1;
                return RedirectToAction("Index", new
                {
                    page,
                    EmployeeData,
                    DepartmentData,
                    BeginDate,
                    EndDate
                });
            }
            //重整
            GetDefaultData(DepartmentData, EmployeeData, BeginDate,EndDate);
            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="employeedata"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        private void GetDefaultData(string departmentdata="",string employeedata="",string starttime="",string endtime="")
        {
            DateTime dateNow = DateTime.Now.AddDays(1);
            ViewData["DepartmentList"] = GetDepartmentList(departmentdata);
            ViewData["EmployeeList"] = GetEmployeetList(departmentdata,employeedata);
            ViewBag.DepartmentData = departmentdata;
            ViewBag.EmployeeData = employeedata;
            ViewBag.StartTime = string.IsNullOrWhiteSpace(starttime)? DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd") :starttime;
            ViewBag.EndTime = string.IsNullOrWhiteSpace(endtime) ? DateTime.Now.ToString("yyyy/MM/dd") : endtime;
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = Services.GetService<DepartmentService>().GetAllLists().OrderBy(x => x.DepartmentCode).ToList();
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value ="", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            return listItem;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeetList(string departmentdata,string selecteddata="")
        {
             //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = Services.GetService<EmployeeService>().GetAllListsByDepartment((string.IsNullOrWhiteSpace(departmentdata) ? Guid.Empty : Guid.Parse(departmentdata))).OrderBy(x => x.EmployeeNO).ToList();
           
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID? true : false) });
            }
            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetEmployee(string DepartmentId)
        {
            List<SelectListItem> result = GetEmployeetList(DepartmentId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
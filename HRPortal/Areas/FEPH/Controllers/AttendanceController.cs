using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class AttendanceController : BaseController
    {
        private bool isHR = false, isAdmin = false;
        // GET: /ToDo/Attendance/
        public ActionResult Index(string EmployeeData = "", string DepartmentData = "", string beginDate = "", string endDate = "", string StatusData = "")
        {
            AttendanceSummaryViewModel viewmodel = new AttendanceSummaryViewModel();
            if (CurrentUser.SignDepartments.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(DepartmentData))
                    DepartmentData = CurrentUser.SignDepartmentCode;
                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData = GetEmployeetList(DepartmentData, EmployeeData);
            }           
            if (string.IsNullOrWhiteSpace(beginDate))
            {
                ViewBag.StartTime = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
            }
            else
            {
                ViewBag.StartTime = beginDate;
            }
            if (string.IsNullOrWhiteSpace(endDate))
            {
                ViewBag.EndTime = DateTime.Now.ToString("yyyy/MM/dd");
            }
            else
            {
                ViewBag.EndTime = endDate;
            }
            //selected data
            GetRole();
            if (isHR || isAdmin)
            {
                DepartmentData = CurrentUser.DepartmentCode;
                viewmodel.EmployeeListData = GetEmployeetList(DepartmentData, EmployeeData);
                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
            }
            viewmodel.StatuslistDataData = GetStatusDataList(StatusData);
            viewmodel.SelectedStatuslistData = StatusData;
            viewmodel.SelectedDepartment = DepartmentData;
            viewmodel.SelectedEmployee = EmployeeData;           
            viewmodel.BeginDate = ViewBag.StartTime;
            viewmodel.EndDate = ViewBag.EndTime;
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            viewmodel.Role = roleDataa.RoleParams != null ? roleDataa.RoleParams : CurrentUser.SignDepartments.Count > 0 ? "is_sign_manager" : roleDataa.RoleParams;
            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Index(HolidaySummaryViewModel viewmodel, string btnQuery, string btnClear)
        {           
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                return RedirectToAction("Index", new
                {
                    DepartmentData = "",
                    EmployeeData = ""
                 
                });
            }
            return View(viewmodel);
        }
        //取得部門人員出勤彙總表
        [HttpPost]
        public async Task<ActionResult> GetDutyScheduleSummary(AttendanceSummaryViewModel viewmodel)
        {
            List<DutyScheduleSummary> _result = new List<DutyScheduleSummary>();
            List<DutyScheduleSummary> _EmployeeDate = new List<DutyScheduleSummary>();
            DateTime BeginDate = DateTime.Parse(viewmodel.BeginDate);
            BeginDate = BeginDate.AddHours(7);
            DateTime EndDate = DateTime.Parse(viewmodel.EndDate);
            EndDate = EndDate.AddHours(23);
            string HireNo = (viewmodel.ChkHireId == true) ? "H05" : " ";
            if (viewmodel.SelectedEmployee == "All")
            {
                //viewmodel.SelectedEmployee = null;
                //用單位抓人
                var Department = Services.GetService<DepartmentService>().GetAll().Where(x => x.DepartmentCode == viewmodel.SelectedDepartment).FirstOrDefault();
                var Employee = Services.GetService<EmployeeService>().GetAll().Where(x => x.SignDepartmentID == Department.ID);

                GetRole();
                if (!isHR && !isAdmin)
                {
                    if (Department.SignManager.EmployeeNO != CurrentUser.EmployeeNO)
                        Employee = Employee.Where(x => x.EmployeeNO == CurrentUser.EmployeeNO);
                }

                foreach (var i in Employee)
                {
                    if (i.LeaveDate == null || i.LeaveDate > DateTime.Now)
                    {
                        if (i.EmployeeNO != "admin")
                        {
                            var DepartmentIDD = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == i.EmployeeNO).Select(x => x.DepartmentID).FirstOrDefault();
                            var DepartmentCode = Services.GetService<DepartmentService>().GetAll().Where(x => x.ID == DepartmentIDD).Select(x => x.DepartmentCode).FirstOrDefault();
                            _EmployeeDate = await HRMApiAdapter.GetDutyScheduleSummary(CurrentUser.CompanyCode, DepartmentCode, i.EmployeeNO, viewmodel.SelectedStatuslistData, BeginDate, EndDate, HireNo);
                            foreach (var item in _EmployeeDate)
                            {
                                var result = new DutyScheduleSummary()
                                {
                                    CompanyCode = item.CompanyCode,
                                    DeptCode = item.DeptCode,
                                    DeptMember = item.DeptMember,
                                    DeptName = item.DeptName,
                                };
                                _result.Add(result);
                            }
                        }
                    }
                }
            }
            else
            {
                //單獨抓人取人的後台單位代碼
                var DepartmentID = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == viewmodel.SelectedEmployee).Select(x => x.DepartmentID).FirstOrDefault();
                var DepartmentCode = Services.GetService<DepartmentService>().GetAll().Where(x => x.ID == DepartmentID).Select(x => x.DepartmentCode).FirstOrDefault();
                

                if (viewmodel.SelectedEmployee == null)
                {
                    if (viewmodel.SelectedDepartment == null && viewmodel.SelectedEmployee == null)
                    {
                        //(一般員工)讀取員工的細項
                        _result = await HRMApiAdapter.GetDutyScheduleSummary(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.EmployeeNO, viewmodel.SelectedStatuslistData, BeginDate, EndDate, HireNo);
                    }
                    else
                    {
                        //(主管)讀取每個員工的細項
                        _result = await HRMApiAdapter.GetDutyScheduleSummary(CurrentUser.CompanyCode, viewmodel.SelectedDepartment, null, viewmodel.SelectedStatuslistData, BeginDate, EndDate, HireNo);
                    }
                }
                else
                {
                    //(主管)讀取員工的細項

                    _result = await HRMApiAdapter.GetDutyScheduleSummary(CurrentUser.CompanyCode, DepartmentCode, viewmodel.SelectedEmployee, viewmodel.SelectedStatuslistData, BeginDate, EndDate, HireNo);
                }
            }
            return PartialView("_DutyScheduleSummary", _result);
        }

        ///// <summary>
        ///// 取得部門列表
        ///// </summary>
        ///// <param name="selecteddata"></param>
        ///// <returns></returns>
        //private List<SelectListItem> GetDepartmentList(string selecteddata)
        //{
        //    List<SelectListItem> listItem = new List<SelectListItem>();
        //    List<Department> data = CurrentUser.Departments;
        //    var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
        //    foreach (var item in data)
        //    {
        //        if (getLanguageCookie == "en-US")
        //        {
        //            listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
        //        }
        //        else
        //        {
        //            listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
        //        }
        //    }
        //    return listItem;
        //}
        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata)
        {
            //使用者角色
            GetRole();
            if (selecteddata == "")
            {
                selecteddata = CurrentUser.DepartmentCode;
            }
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = null;
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (isHR || isAdmin)
            {
                data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

                #region 管理員 或 人資
                foreach (var item in data)
                {
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                }
                #endregion
            }
            else
            {
                data = CurrentUser.SignDepartments;

                if (data.Count > 0)
                {
                    #region 主管
                    bool flag = true;//用來判斷簽核主管是否為外同部門人員
                    foreach (var item in data)
                    {
                        if (item.DepartmentCode == CurrentUser.SignDepartmentCode)
                            flag = false;
                        if (getLanguageCookie == "en-US")
                        {
                            listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                        else
                        {
                            listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                    }

                    if (flag == true)
                    {
                        data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == CurrentUser.SignDepartmentCode).OrderBy(x => x.DepartmentCode).ToList();
                        foreach (var item in data)
                        {
                            if (getLanguageCookie == "en-US")
                            {
                                listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                            }
                            else
                            {
                                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                            }
                        }

                    }

                    #endregion
                }
                else
                {
                    #region 其它
                    Department _signdepartment = Services.GetService<DepartmentService>().GetDepartmentByID(CurrentUser.SignDepartmentID);
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentEnglishName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    #endregion
                }
            }

            return listItem;
        }
        /// <summary>
        /// 使用者角色
        /// </summary>
        public bool GetRole()
        {
            bool result = false;

            try
            {
                Role roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();

                if (roleData != null)
                {
                    if (!string.IsNullOrEmpty(roleData.RoleParams))
                    {
                        dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                        isHR = (roleParams.is_hr != null && roleParams.is_hr);
                        isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
        ///// <summary>
        ///// 取得員工列表
        ///// </summary>
        ///// <param name="departmentdata">被選取的部門</param>
        ///// <param name="selecteddata"></param>
        ///// <returns></returns>
        //private List<SelectListItem> GetEmployeetList(string departmentCode = "", string selecteddata = "")
        //{
        //    List<Department> departmentdata = CurrentUser.Departments;
        //    List<SelectListItem> listItem = new List<SelectListItem>();
        //    List<Employee> data = new List<Employee>();
        //    var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
        //    foreach (var item in departmentdata)
        //    {
        //        //讀取每個部門的employee
        //        var employeedatas = Services.GetService<EmployeeService>().GetLists(CurrentUser.CompanyID, item.ID).ToList();
        //        data.AddRange(employeedatas);
        //    }

        //    if (!string.IsNullOrWhiteSpace(departmentCode))
        //        data = data.Where(x => x.Department.DepartmentCode == departmentCode).OrderBy(x => x.EmployeeNO).ToList();

        //    listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "", Selected = (selecteddata == "" ? true : false) });
        //    foreach (var item in data)
        //    {
        //        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
        //        {
        //            if (getLanguageCookie == "en-US")
        //            {
        //                listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO.ToString(), Selected = (selecteddata == item.EmployeeNO ? true : false) });
        //            }
        //            else
        //            {
        //                listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO.ToString(), Selected = (selecteddata == item.EmployeeNO ? true : false) });
        //            }
        //        }
        //    }
        //    return listItem;
        //}
        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeetList(string departmentdata, string selecteddata = "")
        {
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.Name;
            //取得部門
            if (departmentdata == "")
            {
                departmentdata = CurrentUser.DepartmentCode;
            }
            Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentdata && x.Enabled).FirstOrDefault();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();
            GetRole();
            //取得員工列表
            if (isHR || isAdmin || _department.SignManager.EmployeeNO == CurrentUser.EmployeeNO)
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            }
            else
            {
                bool flag = false;
                foreach (var item in CurrentUser.SignDepartments)
                {
                    if (item.DepartmentCode == departmentdata)
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).Where(x => x.EmployeeNO == CurrentUser.EmployeeNO).OrderBy(x => x.EmployeeNO).ToList();
                }
                else
                {
                    data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
                }
            }

            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    {
                        listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    }
                }
                else
                {
                    if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    {
                        listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    }
                }
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
        /// <summary>
        /// 取得員工在離職狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetStatusDataList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "顯示在職人員", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "顯示離職人員", Value = "L", Selected = (selecteddata == "L" ? true : false) });
            listItem.Add(new SelectListItem { Text = "全部", Value = "ALL", Selected = (selecteddata == "ALL" ? true : false) });

            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }
        /// <summary>
        /// 給下拉式選單讀取在離職員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetStatusData(string DepartmentId, string StatusData)
        {
            List<SelectListItem> result = GetEmployeeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 取得在離職員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeeetList(string departmentdata, string StatusData, string selecteddata = "")
        {
            //取得部門
            Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentdata && x.Enabled).FirstOrDefault();

            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            //取得員工列表
            data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();

            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    if (StatusData == "")
                    {
                        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                        {
                            listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                        }
                    }
                    else
                        if (StatusData == "L")
                        {
                            if (item.LeaveDate < DateTime.Now)
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                        }
                        else
                            if (StatusData == "ALL")
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                }
                else
                {
                    if (StatusData == "")
                    {
                        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                        {
                            listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                        }
                    }
                    else
                        if (StatusData == "L")
                        {
                            if (item.LeaveDate < DateTime.Now)
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                        }
                        else
                            if (StatusData == "ALL")
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                }
            }
            return listItem;
        }
    }
}
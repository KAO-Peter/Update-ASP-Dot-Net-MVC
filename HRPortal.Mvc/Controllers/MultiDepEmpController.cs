using HRPortal.ApiAdapter;
using HRPortal.DBEntities;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Mvc.Controllers
{
    [Authorize]
    public abstract class MultiDepEmpController : BaseController
    {
        public bool isHR = false, isAdmin = false;
        private List<Employee> signFlowMemberList; //可簽核人員清單

        /// <summary>
        /// 可簽核人員所屬部門清單
        /// </summary>
        private List<Department> signFlowDepartmentList; //可簽核人員所屬部門清單

        private Department selectedDepartment; //目前選取部門

        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="empvalmode">下拉選單value 模式，預設為員工編號，參數有設定值時會設定為 ID (GUID)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetEmployee(string departmentId, string status = "", string empvalmode = "")
        {
            List<MutiSelectListItem> result = GetEmployeetList(departmentId, "", status, empvalmode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 給下拉式選單讀取員工列表 有設定管理的部門
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="empvalmode">下拉選單value 模式，預設為員工編號，參數有設定值時會設定為 ID (GUID)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetEmployee2(string departmentId, string status = "", string empvalmode = "")
        {
            List<MutiSelectListItem> result = GetEmployeetList2(departmentId, "", status, empvalmode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //取得目前使用者可簽核的所有人員與所屬部門
        public void GetSignFlowMemberAndDepartment(bool isSelfIncluded = true)
        {
            signFlowMemberList = new List<Employee>();
            signFlowDepartmentList = new List<Department>();
            //取得目前使用者可查詢薪資的所有人員清單
            //Task<RequestResult> apiResult = Task.Run<RequestResult>(async () => await HRMApiAdapter.DeletePortalFormDetail(model.FormNo));
            //List<string> salaryEmpIDList = await HRMApiAdapter.GetDeptSalaryEmpIDList(this.CurrentUser.EmployeeNO);
            Task<List<string>> tempsalaryEmpIDList = Task.Run<List<string>>(async()=>await HRMApiAdapter.GetDeptSalaryEmpIDList(this.CurrentUser.EmployeeNO));
            Task.WaitAll(tempsalaryEmpIDList);
            List<string> salaryEmpIDList = new List<string>();
            foreach(var x in salaryEmpIDList)
            {
            salaryEmpIDList.Add(x);
            }
            //salaryEmpIDList = salaryEmpIDList.Where(x => x != this.CurrentUser.EmployeeNO).ToList();
            //找出該清單在Portal對應的所有人員物件
            var _emps = Services.GetService<EmployeeService>().Where(x => salaryEmpIDList.Contains(x.EmployeeNO)).ToList();

            //signFlowMemberList = this.GetSignFlowMemberAll(isSelfIncluded);

            //20190618 Frank 額外可查看部門
            //var _depts = Services.GetService<QueryDeptSettingService>().Where(x => x.EmployeeID == CurrentUser.EmployeeID).Select(x => x.DepartmentID).ToList();
            //var _emps = Services.GetService<EmployeeService>().Where(x => _depts.Contains(x.SignDepartmentID)).ToList()
            //                                                  .Where(x => !signFlowMemberList.Contains(x) && (isSelfIncluded ? true : x.ID != CurrentUser.EmployeeID)).ToList();
            signFlowMemberList.AddRange(_emps);

            signFlowMemberList = signFlowMemberList.Where(x => x.EmployeeNO != "admin").ToList();
            signFlowDepartmentList = signFlowMemberList.Select(x => x.Department).Distinct().OrderBy(y => y.Company.CompanyCode).ThenBy(z => z.DepartmentCode).ToList();
        }

        public List<int> _GetSignFlowMemberAndDepartment()
        {
            GetSignFlowMemberAndDepartment();
            return signFlowMemberList.Select(x => x.Employee_ID ?? 0).ToList();
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        public List<MutiSelectListItem> GetDepartmentList(Guid? selecteddata)
        {
            //使用者角色
            GetRole();

            if (selecteddata == null)
            {
                selecteddata = CurrentUser.DepartmentID;
            }
            List<MutiSelectListItem> listItem = new List<MutiSelectListItem>();
            List<Department> data = new List<Department>();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            //先取得可簽核人員部門資料
            if (signFlowMemberList == null)
            {
                GetSignFlowMemberAndDepartment();
            }

            if (isHR || isAdmin)
            {
                data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

                if (signFlowDepartmentList.Any())
                {
                    data.AddRange(signFlowDepartmentList.Where(x => !data.Contains(x)).ToList());
                }

                selectedDepartment = data.Where(x => x.ID == selecteddata).FirstOrDefault();
                if (selectedDepartment == null)
                {
                    selectedDepartment = CurrentUser.Employee.Department;
                }
            }
            else
            {
                if (signFlowDepartmentList.Any())
                {
                    data = signFlowDepartmentList;

                    selectedDepartment = signFlowDepartmentList.Where(x => x.ID == selecteddata).FirstOrDefault();

                    if (selectedDepartment == null)
                    {
                        selectedDepartment = CurrentUser.Employee.Department;
                    }
                }
                else
                {
                    selectedDepartment = CurrentUser.Employee.Department;
                    data.Add(selectedDepartment);
                }
            }

            data = data.Where(x => x.DepartmentCode != "ZZZZZZZ")
                       .OrderBy(x => x.Company.CompanyCode)
                       .ThenBy(x => x.DepartmentCode)
                       .ToList();
            foreach (var item in data)
            {
                string deptName = string.Format("{0} {1}", item.DepartmentCode, getLanguageCookie == "en-US" ? item.DepartmentEnglishName : item.DepartmentName);
                listItem.Add(new MutiSelectListItem
                {
                    Text = deptName,
                    Value = item.ID.ToString(),
                    Selected = (!(isHR || isAdmin) && (item.ID == selecteddata)),
                    Title = string.Format("{0} {1}", item.Company.CompanyCode, item.Company.CompanyName)
                });
            }

            return listItem;
        }

        /// <summary>
        /// 取得部門列表 有設定管理的部門
        /// </summary>
        public List<MutiSelectListItem> GetDepartmentList2(Guid? selecteddata)
        {
            //使用者角色
            GetRole();

            if (selecteddata == null)
            {
                selecteddata = CurrentUser.DepartmentID;
            }
            List<MutiSelectListItem> listItem = new List<MutiSelectListItem>();
            List<Department> data = new List<Department>();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            //先取得可簽核人員部門資料
            //if (signFlowMemberList == null)
            //{
            //    await GetSignFlowMemberAndDepartment();
            //}

            if (isHR || isAdmin)
            {
                signFlowDepartmentList = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
                //signFlowDepartmentList = data.Where(x => x.DepartmentCode != "ZZZZZZZ").ToList();
                if (signFlowDepartmentList.Any())
                {
                    data.AddRange(signFlowDepartmentList.Where(x => !data.Contains(x)).ToList());
                }

                selectedDepartment = data.Where(x => x.ID == selecteddata).FirstOrDefault();
                if (selectedDepartment == null)
                {
                    selectedDepartment = CurrentUser.Employee.Department;
                }
            }
            else
            {
                signFlowDepartmentList = CurrentUser.SignDepartments;

                if (signFlowDepartmentList.Any())
                {
                    data = signFlowDepartmentList;

                    selectedDepartment = signFlowDepartmentList.Where(x => x.ID == selecteddata).FirstOrDefault();

                    if (selectedDepartment == null)
                    {
                        selectedDepartment = CurrentUser.Employee.Department;
                    }
                }
                else
                {
                    selectedDepartment = CurrentUser.Employee.Department;
                    data.Add(selectedDepartment);
                }
            }

            data = data.Where(x => x.DepartmentCode != "ZZZZZZZ")
                       .OrderBy(x => x.Company.CompanyCode)
                       .ThenBy(x => x.DepartmentCode)
                       .ToList();
            foreach (var item in data)
            {
                string deptName = string.Format("{0} {1}", item.DepartmentCode, getLanguageCookie == "en-US" ? item.DepartmentEnglishName : item.DepartmentName);
                listItem.Add(new MutiSelectListItem
                {
                    Text = deptName,
                    Value = item.ID.ToString(),
                    Selected = (!(isHR || isAdmin) && (item.ID == selecteddata)),
                    Title = string.Format("{0} {1}", item.Company.CompanyCode, item.Company.CompanyName)
                });
            }

            return listItem;
        }
        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="selecteddata"></param>
        /// <param name="statusData"></param>
        /// <param name="empvalmode">下拉選單value 模式，預設為員工編號，參數有設定值時會設定為 ID (GUID)</param>
        /// <returns></returns>
        public List<MutiSelectListItem> GetEmployeetList(string departmentdata, string selecteddata = "", string statusData = "", string empvalmode = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<MutiSelectListItem> listItem = new List<MutiSelectListItem>();

            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.Name;

            //取得部門
            departmentdata = departmentdata.TrimStart(',').TrimEnd(',');
            if (string.IsNullOrWhiteSpace(departmentdata))
            {
                return listItem;
            }
            string[] depList = departmentdata.Split(',');

            List<Department> _department = Services.GetService<DepartmentService>()
                                                   .GetAllLists()
                                                   .Where(x => depList.Contains(x.ID.ToString()) && x.Enabled)
                                                   .ToList();

            List<Employee> data = new List<Employee>();

            GetRole();
            //取得員工列表
            if (isHR || isAdmin)
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department).ToList();
            }
            else
            {
                if (signFlowMemberList == null)
                {
                    GetSignFlowMemberAndDepartment();
                }

                if (signFlowMemberList.Any()) //當前使用者有權限簽核
                {
                    Department dept = null;
                    foreach (var _dep in _department)
                    {
                        //先找可簽核人員所屬部門清單是否有包含傳入的部門代碼
                        dept = signFlowDepartmentList.Where(x => x.ID == _dep.ID).FirstOrDefault();
                        if (dept != null)
                        {
                            data.AddRange(signFlowMemberList.Where(x => x.DepartmentID == dept.ID).OrderBy(y => y.EmployeeNO).ToList());
                        }
                    }
                }
                else //一般非主管使用者，只能看自己的
                {
                    data.Add(CurrentUser.Employee);
                }
            }

            if (statusData == "")
            {
                data = data.Where(x => x.LeaveDate == null || x.LeaveDate.Value > DateTime.Now).ToList();
            }
            else if (statusData == "L")
            {
                data = data.Where(x => x.LeaveDate != null && x.LeaveDate.Value < DateTime.Now).ToList();
            }

            data = data.Where(x => x.EmployeeNO != "admin")
                       .OrderBy(x => x.Company.CompanyCode)
                       .ThenBy(x => x.Department.DepartmentCode)
                       .ThenBy(x => x.EmployeeNO)
                       .ToList();

            string empName = string.Empty;
            foreach (var item in data)
            {
                empName = string.Format("{0} {1}", item.EmployeeNO, getLanguageCookie == "en-US" ? item.EmployeeEnglishName : item.EmployeeName);
                listItem.Add(new MutiSelectListItem
                {
                    Text = empName,
                    Value = empvalmode.Trim() == "" ? item.EmployeeNO : item.ID.ToString(),
                    Selected = (!(isHR || isAdmin) && selecteddata == item.EmployeeNO),
                    Title = string.Format("{0} {1}", item.Department.DepartmentCode, item.Department.DepartmentName),
                });
            }
            return listItem;
        }

        /// <summary>
        /// 取得員工列表 有設定管理的部門
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="selecteddata"></param>
        /// <param name="statusData"></param>
        /// <param name="empvalmode">下拉選單value 模式，預設為員工編號，參數有設定值時會設定為 ID (GUID)</param>
        /// <returns></returns>
        public List<MutiSelectListItem> GetEmployeetList2(string departmentdata, string selecteddata = "", string statusData = "", string empvalmode = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<MutiSelectListItem> listItem = new List<MutiSelectListItem>();

            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.Name;

            //取得部門
            departmentdata = departmentdata.TrimStart(',').TrimEnd(',');
            if (string.IsNullOrWhiteSpace(departmentdata))
            {
                return listItem;
            }
            string[] depList = departmentdata.Split(',');

            List<Department> _department = Services.GetService<DepartmentService>()
                                                   .GetAllLists()
                                                   .Where(x => depList.Contains(x.ID.ToString()) && x.Enabled)
                                                   .ToList();

            List<Employee> data = new List<Employee>();

            GetRole();
            //取得員工列表
            if (isHR || isAdmin)
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department).ToList();
            }
            else
            {
                signFlowDepartmentList = CurrentUser.SignDepartments;
                //if (signFlowMemberList == null)
                //{
                //    await GetSignFlowMemberAndDepartment();
                //}

                if (signFlowDepartmentList.Any()) //當前使用者有權限簽核
                {
                    Department dept = null;
                    foreach (var _dep in _department)
                    {
                        //先找可簽核人員所屬部門清單是否有包含傳入的部門代碼
                        dept = signFlowDepartmentList.Where(x => x.ID == _dep.ID).FirstOrDefault();
                        if (dept != null)
                        {
                            var empData = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, dept.ID).OrderBy(x => x.EmployeeNO).ToList();

                            data.AddRange(empData);
                        }
                    }
                }
                else //一般非主管使用者，只能看自己的
                {
                    data.Add(CurrentUser.Employee);
                }
            }

            if (statusData == "")
            {
                data = data.Where(x => x.LeaveDate == null || x.LeaveDate.Value > DateTime.Now).ToList();
            }
            else if (statusData == "L")
            {
                data = data.Where(x => x.LeaveDate != null && x.LeaveDate.Value < DateTime.Now).ToList();
            }

            data = data.Where(x => x.EmployeeNO != "admin")
                       .OrderBy(x => x.Company.CompanyCode)
                       .ThenBy(x => x.Department.DepartmentCode)
                       .ThenBy(x => x.EmployeeNO)
                       .ToList();

            string empName = string.Empty;
            foreach (var item in data)
            {
                empName = string.Format("{0} {1}", item.EmployeeNO, getLanguageCookie == "en-US" ? item.EmployeeEnglishName : item.EmployeeName);
                listItem.Add(new MutiSelectListItem
                {
                    Text = empName,
                    Value = empvalmode.Trim() == "" ? item.EmployeeNO : item.ID.ToString(),
                    Selected = (!(isHR || isAdmin) && selecteddata == item.EmployeeNO),
                    Title = string.Format("{0} {1}", item.Department.DepartmentCode, item.Department.DepartmentName),
                });
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
                    if (!string.IsNullOrWhiteSpace(roleData.RoleParams))
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

        /// <summary>
        /// 取得員工在離職狀態
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        public List<SelectListItem> GetStatusDataList()
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "顯示在職人員", Value = "" });
            listItem.Add(new SelectListItem { Text = "顯示離職人員", Value = "L" });
            listItem.Add(new SelectListItem { Text = "全部", Value = "ALL" });

            return listItem;
        }
    }
}
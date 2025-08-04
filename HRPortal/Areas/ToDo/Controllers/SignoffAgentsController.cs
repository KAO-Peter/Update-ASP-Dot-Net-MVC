using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class SignoffAgentsController : BaseController
    {
        private bool isGetRole = false, isHR = false, isAdmin = false;
        string empType;
        SignoffAgentsViewModel viewmodel = new SignoffAgentsViewModel();

        // GET: /Reports/SignoffAgents/
        public ActionResult Index(string EmployeeData = "", string DepartmentData = "")
        {
            SignoffAgents data = Services.GetService<SignoffAgentsService>().GetSignoffAgentsByFormNo(CurrentUser.EmployeeNO);
            viewmodel.FormData = new SignoffAgents();
            viewmodel.FormData = data;
          
            #region 使用者角色
            if (!isGetRole)
            {
                isGetRole = GetRole();
            }
            #endregion

            if (string.IsNullOrWhiteSpace(DepartmentData))
            {
                DepartmentData = CurrentUser.DepartmentCode;
            }

            if (string.IsNullOrWhiteSpace(EmployeeData))
            {
                EmployeeData = CurrentUser.EmployeeNO;
            }

            if (isHR || isAdmin)
            {
                empType = "ALL";
                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData = GetEmployeetList(DepartmentData, EmployeeData);
                empType = "";
                viewmodel.DepartmentListData1 = GetDepartmentList(DepartmentData);
                viewmodel.DepartmentListData2 = GetDepartmentList(DepartmentData);
                viewmodel.DepartmentListData3 = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData1 = GetEmployeetList(DepartmentData, EmployeeData);
                viewmodel.EmployeeListData2 = GetEmployeetList(DepartmentData, EmployeeData);
                viewmodel.EmployeeListData3 = GetEmployeetList(DepartmentData, EmployeeData);
            }
            else
            {
                empType = "ALL";
                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData = GetEmployeetList(DepartmentData, EmployeeData);
                empType = "";
                viewmodel.DepartmentListData1 = GetDepartmentList(DepartmentData);
                viewmodel.DepartmentListData2 = GetDepartmentList(DepartmentData);
                viewmodel.DepartmentListData3 = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData1 = GetEmployeetList(DepartmentData, EmployeeData);
                viewmodel.EmployeeListData2 = GetEmployeetList(DepartmentData, EmployeeData);
                viewmodel.EmployeeListData3 = GetEmployeetList(DepartmentData, EmployeeData);
            }

            //selected data
            if (null != data)
            {
                viewmodel.SelectedDepartment = data.SignDepartmentCode;
                empType = "ALL";
                viewmodel.EmployeeListData = GetEmployeetList(data.SignDepartmentCode, data.EmployeeNO);
                viewmodel.SelectedDepartment1 = data.AgentDep_ID1;
                empType = "";
                viewmodel.EmployeeListData1 = GetEmployeetList(data.AgentDep_ID1, data.AgentEmp_ID1);
                viewmodel.SelectedDepartment2 = data.AgentDep_ID2;
                viewmodel.EmployeeListData2 = GetEmployeetList(data.AgentDep_ID2, data.AgentEmp_ID2);
                viewmodel.SelectedDepartment3 = data.AgentDep_ID3;
                viewmodel.EmployeeListData3 = GetEmployeetList(data.AgentDep_ID3, data.AgentEmp_ID3);
                viewmodel.SourceType = "1";
            }
            else
            {
                empType = "ALL";
                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData = GetEmployeetList(DepartmentData, EmployeeData);
                empType = "";
                viewmodel.SelectedDepartment1 = DepartmentData;
                viewmodel.SelectedEmployee1 = EmployeeData;
                viewmodel.SelectedDepartment2 = DepartmentData;
                viewmodel.SelectedEmployee2 = EmployeeData;
                viewmodel.SelectedDepartment3 = DepartmentData;
                viewmodel.SelectedEmployee3 = EmployeeData;
                viewmodel.SourceType = "0";
            }

            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            viewmodel.Role = roleDataa.RoleParams;

         
            return View(viewmodel);
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
   
        [HttpPost]
        public ActionResult Index(SignoffAgentsViewModel viewmodel)
        {

            GetRole();
            if (!isAdmin && (!isHR && CurrentUser.Departments.Count == 0))
            {
                //個人
                viewmodel.SelectedDepartment = CurrentUser.DepartmentCode.ToString();
                viewmodel.SelectedEmployee = CurrentUser.EmployeeNO.ToString();
            }

            SignoffAgents data = Services.GetService<SignoffAgentsService>().GetSignoffAgentsByFormNo(viewmodel.SelectedEmployee);

            int IsSuccess;
            viewmodel.SourceType = data != null ? "0" : "1";
          

            try
            {
                if ( viewmodel.SourceType == "1")
                {
                    viewmodel.FormData = new SignoffAgents();

                    viewmodel.FormData.SignDepartmentCode = viewmodel.SelectedDepartment;
                    viewmodel.FormData.EmployeeNO         = viewmodel.SelectedEmployee;
                    viewmodel.FormData.CompanyCode        = CurrentUser.CompanyCode;
                    viewmodel.FormData.AgentDep_ID1       = viewmodel.SelectedDepartment1;
                    viewmodel.FormData.AgentEmp_ID1       = viewmodel.SelectedEmployee1;
                    viewmodel.FormData.AgentDep_ID2       = viewmodel.SelectedDepartment2;
                    viewmodel.FormData.AgentEmp_ID2       = viewmodel.SelectedEmployee2;
                    viewmodel.FormData.AgentDep_ID3       = viewmodel.SelectedDepartment3;
                    viewmodel.FormData.AgentEmp_ID3       = viewmodel.SelectedEmployee3;
                    //新增
                    IsSuccess = Services.GetService<SignoffAgentsService>().Create(viewmodel.FormData, true);
                }
                else
                {
                   data.SignDepartmentCode = viewmodel.SelectedDepartment;
                   data.EmployeeNO         = viewmodel.SelectedEmployee;
                   data.CompanyCode        = CurrentUser.CompanyCode;
                   data.AgentDep_ID1       = viewmodel.SelectedDepartment1;
                   data.AgentEmp_ID1       = viewmodel.SelectedEmployee1;
                   data.AgentDep_ID2       = viewmodel.SelectedDepartment2;
                   data.AgentEmp_ID2       = viewmodel.SelectedEmployee2;
                   data.AgentDep_ID3       = viewmodel.SelectedDepartment3;
                   data.AgentEmp_ID3       = viewmodel.SelectedEmployee3;
                    //修改                 
                   IsSuccess = Services.GetService<SignoffAgentsService>().Update(data, true);
                    if (IsSuccess != 0)
                    {
                        return Json(new AjaxResult() { status = "success", message = "更新成功" });
                    }
                    else
                    {
                        return Json(new AjaxResult() { status = "failed", message = "更新失敗" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult() { status = "failed", message = ex.Message });
            }

            return Json(new AjaxResult() { status = "success", message = "更新完成" });
        }

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

            data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

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
           
            return listItem;
        }

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

            //取得員工列表
            if (empType == "ALL")
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            }
            else
            {
                //data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).Where(x => x.EmployeeNO != CurrentUser.EmployeeNO).OrderBy(x => x.EmployeeNO).ToList();
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            }

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
        /// 讀取簽核代理人
        /// </summary>
        /// <param name="EmployeeNo"></param>
        /// <returns></returns>
        public ActionResult GetSignoffAgents( string EmployeeNo = "")
        {
            string DepartmentId = "";
            SignoffAgents data = Services.GetService<SignoffAgentsService>().GetSignoffAgentsByFormNo(EmployeeNo);
            SignoffAgentsViewModel viewmodel = new SignoffAgentsViewModel();
            viewmodel.FormData = new SignoffAgents();
            viewmodel.FormData = data;
            if (null != data)
            {
                viewmodel.SelectedDepartment = data.SignDepartmentCode;
                empType = "ALL";
                viewmodel.EmployeeListData = GetEmployeetList(data.SignDepartmentCode, data.EmployeeNO);
                viewmodel.SelectedDepartment1 = data.AgentDep_ID1;
                empType = "";
                viewmodel.SelectedDepartment1 = data.AgentDep_ID1;
                viewmodel.SelectedEmployee1 = data.AgentEmp_ID1;
                viewmodel.SelectedDepartment2 = data.AgentDep_ID2;
                viewmodel.SelectedEmployee2 = data.AgentEmp_ID2;
                viewmodel.SelectedDepartment3 = data.AgentDep_ID3;
                viewmodel.SelectedEmployee3 = data.AgentEmp_ID3;
                viewmodel.SourceType = "1";
            }
            else
            {
                empType = "ALL";
                viewmodel.DepartmentListData = GetDepartmentList(CurrentUser.DepartmentCode);
                viewmodel.EmployeeListData = GetEmployeetList(DepartmentId, CurrentUser.EmployeeNO);
                empType = "";
                viewmodel.SelectedDepartment1 = CurrentUser.DepartmentCode;
                viewmodel.SelectedEmployee1 = CurrentUser.EmployeeNO;
                viewmodel.SelectedDepartment2 = CurrentUser.DepartmentCode;
                viewmodel.SelectedEmployee2 = CurrentUser.EmployeeNO;
                viewmodel.SelectedDepartment3 = CurrentUser.DepartmentCode;
                viewmodel.SelectedEmployee3 = CurrentUser.EmployeeNO;
                viewmodel.SourceType = "0";
            }
            return Json(viewmodel, JsonRequestBehavior.AllowGet);
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
    }
}
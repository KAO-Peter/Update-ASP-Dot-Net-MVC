using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.M06.Controllers
{
    public class EmployeeInfoManageController : BaseController
    {
        // GET: M06/EmployeeInfoManage
        public ActionResult Index(int page = 1, string SearchDepartmentData = "", string CompanyData = "", string txtkeyword = "", string SearchQueryTag = "")
        {

            List<ShowEmployeeIndex> viewModel = new List<ShowEmployeeIndex>();
            GetDefaultData(CompanyData, SearchDepartmentData, txtkeyword, SearchQueryTag);
            int currentPage = page < 1 ? 1 : page;
            //if (string.IsNullOrWhiteSpace(SearchQueryTag) && string.IsNullOrWhiteSpace(txtkeyword))
            //{
            //    return View();
            //}

            //Get EmployeeDisplayModel
            //var ds = Services.GetService<EmployeeService>().GetSearchEmployeeLists((string.IsNullOrWhiteSpace(SearchDepartmentData) ? Guid.Empty : Guid.Parse(SearchDepartmentData)), (string.IsNullOrWhiteSpace(CompanyData) ? Guid.Empty : Guid.Parse(CompanyData)), txtkeyword);
            var ds = Services.GetService<EmployeeService>().GetSearchEmployeeLists(this.CurrentUser.DepartmentID , this.CurrentUser.CompanyID, this.CurrentUser.EmployeeNO);
            if (ds == null || ds.Count == 0)
            {
                return View();
            }
            foreach (var item in ds)
            {
                var EmpLeaveDate = item.LeaveDate;
                if (EmpLeaveDate == null)
                {
                    EmpLeaveDate = null;
                }
                else
                {
                    EmpLeaveDate = item.LeaveDate.Value.AddDays(1).AddSeconds(-1);
                }
                if (EmpLeaveDate > DateTime.Now || item.LeaveDate == null)//Irving 在員工基本資料選單不顯示離職人員 2016/03/04
                {
                    viewModel.Add(new ShowEmployeeIndex
                    {
                        CompanyID = item.CompanyID,
                        CompanyName = item.Company.CompanyName,
                        DepartmentID = item.DepartmentID,
                        DepartmentName = item.Department.DepartmentName,
                        EmployeeName = item.EmployeeName,
                        EmployeeNO = item.EmployeeNO,
                        ID = item.ID,
                        SignDepartmentID = item.SignDepartmentID,
                        SignDepartmentName = item.SignDepartment.DepartmentName,
                        CreatedTime = item.CreatedTime
                    });
                }
            }

            return View(viewModel.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string SearchDepartmentData, string CompanyData, string btnQuery, string btnClear, string txtkeyword, string SearchQueryTag)
        {
            if (!string.IsNullOrWhiteSpace(txtkeyword))
            {
                SearchDepartmentData = "";
                CompanyData = "";
            }
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery) || !string.IsNullOrWhiteSpace(txtkeyword))
            {
                return RedirectToAction("Index", new
                {
                    SearchDepartmentData,
                    CompanyData,
                    SearchQueryTag = 1,
                    txtkeyword
                });
            }
            //重整
            GetDefaultData(CompanyData, SearchDepartmentData, txtkeyword, SearchQueryTag);
            return View();
        }

        public async Task<ActionResult> Create()
        {
            EmployeeInfoManageViewModel viewmodel = new EmployeeInfoManageViewModel();
            Employee employeedata = new Employee();
            employeedata.Gender = (int)GenderType.Male;
            employeedata.ArriveDate = DateTime.Now;
            viewmodel.employeeData = employeedata;
            viewmodel.companyLists = GetCompanyList("");
            viewmodel.signDepartmentLists = GetDepartmentList("","");
            viewmodel.roleLists = GetRoleList();
            ViewData["EmergencyRelativesList"] = await GetRelativesList("");
            return PartialView("_CreateEmployeeInfo", viewmodel);
        }

        [HttpPost]
        public ActionResult Create(EmployeeInfoManageViewModel viewmodel)
        {
            PasswordHasher _hasher = new PasswordHasher();
            viewmodel.employeeData.PasswordHash = _hasher.HashPassword(viewmodel.employeeData.PasswordHash);
            viewmodel.employeeData.EmployeeType = "1";
            var _result = Services.GetService<EmployeeService>().Create(viewmodel.employeeData, true);
            if (_result == 1)
            {
                WriteLog("Success:" + viewmodel.employeeData.EmployeeNO);
                return Json(new AjaxResult() { status = "success", message = "新增成功" });
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "更新失敗" });
            }
        }

        public async Task<ActionResult> Edit(string ID)
        {   
            EmployeeInfoManageViewModel employeeInfo = SetEmployeeInfoManageViewModel(ID, true);
            ViewData["EmergencyRelativesList"] = await GetRelativesList(employeeInfo.employeeData.EmergencyRelation);
            return PartialView("_EditEmployeeInfo", employeeInfo);
        }

        [HttpPost]
        public ActionResult Update(EmployeeInfoManageViewModel viewmodel)
        {
            var olddata = Services.GetService<EmployeeService>().GetAll().Where(x => x.ID == viewmodel.employeeData.ID).FirstOrDefault();

            //更新密碼的設定
            if (string.IsNullOrWhiteSpace(viewmodel.updatePasswordHash))
            {
                viewmodel.employeeData.PasswordHash = olddata.PasswordHash;
            }
            else
            {
                PasswordHasher _hasher = new PasswordHasher();
                viewmodel.employeeData.PasswordHash = _hasher.HashPassword(viewmodel.updatePasswordHash);
            }

            var _result = Services.GetService<EmployeeService>().Update(olddata, viewmodel.employeeData, true);
            if (_result == 1)
            {
                WriteLog("Success:" + olddata.EmployeeNO);
                return Json(new AjaxResult() { status = "success", message = "更新成功" });
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "更新失敗" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateToHRM(EmployeeInfoManageViewModel viewmodel)
        {
            var ds = Services.GetService<EmployeeService>().GetAll().Where(x => x.CompanyID == viewmodel.employeeData.CompanyID & x.EmployeeNO == viewmodel.employeeData.EmployeeNO).FirstOrDefault();
            var CompanyCode = ds.Company.CompanyCode;
            RequestResult _checkResult = new RequestResult();
            _checkResult = await HRMApiAdapter.PostModifyEmpData(CompanyCode, viewmodel.employeeData.EmployeeNO, viewmodel.employeeData.RegisterAddress, viewmodel.employeeData.Address, viewmodel.employeeData.TelephoneNumber,
            viewmodel.employeeData.CellphoneNumber, viewmodel.employeeData.EmergencyName, viewmodel.employeeData.EmergencyTelephoneNumber, viewmodel.employeeData.EmergencyAddress, viewmodel.employeeData.Email, viewmodel.employeeData.RegisterTelephoneNumber,viewmodel.employeeData.EmergencyRelation);

           
            return Json(new AjaxResult() { status = (_checkResult.Status == true) ? "success" : "failed", message = (_checkResult.Status == true) ? "更新成功" : "更新失敗" });
        }

        //[HttpPost]
        //public ActionResult DeleteEmployee(Guid employeeId)
        //{
        //    Employee _employee = Services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == employeeId);

        //    try
        //    {
        //        _employee.Enabled = false;
        //        _employee.DisableDate = DateTime.Now;

        //        TempData["message"] = "刪除成功";
        //        WriteLog("Success:" + employeeId);

        //        return Json(new AjaxResult() { status = "success", message = "刪除成功" });
        //    }
        //    catch
        //    {
        //        return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
        //    }
        //}

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="departmentdata"></param>
        /// <param name="employeedata"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        private void GetDefaultData(string companydata = "", string departmentdata = "", string searchdata = "", string searchquerytag = "")
        {
            ViewData["DepartmentList"] = GetDepartmentList(companydata, departmentdata);
            ViewData["CompanyList"] = GetCompanyList(companydata);
            ViewBag.SearchDepartmentData = departmentdata;
            ViewBag.CompanyData = companydata;
            ViewBag.SearchENO = searchdata;
            ViewBag.SearchQueryTag = searchquerytag;

        }

        /// <summary>
        /// 取得公司列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetCompanyList(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Company> data = Services.GetService<CompanyService>().GetAll().OrderBy(x => x.CompanyCode).ToList();
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.CompanyName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            return listItem;
        }

        /// <summary>
        /// 取得角色列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetRoleList(string selecteddata="")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Role> data = Services.GetService<RoleService>().GetAll().ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.RoleParams))
                {
                    dynamic _roleParams = System.Web.Helpers.Json.Decode(item.RoleParams);
                    if (!CurrentUser.IsAdmin && _roleParams != null && _roleParams.is_admin != null && _roleParams.is_admin)
                    {
                        continue;
                    }
                }

                listItem.Add(new SelectListItem { Text = item.Name, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            return listItem;
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string companydata, string selecteddata = "")
        {
            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = Services.GetService<DepartmentService>().GetListsByCompany((string.IsNullOrWhiteSpace(companydata) ? Guid.Empty : Guid.Parse(companydata))).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            return listItem;
        }
        /// <summary>
        /// 取得緊急連絡人關係列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetRelativesList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in await HRMApiAdapter.GetRelatives(CurrentUser.Employee.Company.CompanyCode))
            {
                listItem.Add(new SelectListItem { Text = item.Name, Value = item.Name, Selected = (selecteddata == item.Name ? true : false) });
               
            }
            
            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取部門列表
        /// </summary>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        public ActionResult GetDepartment(string CompanyId)
        {
            List<SelectListItem> result = GetDepartmentList(CompanyId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private SelectList GetSignDepartmentList(Guid selected,Guid companyID)
        {
            SelectList options = new SelectList(Services.GetService<DepartmentService>().GetAllLists().Where(x => x.Enabled && x.CompanyID == companyID).OrderBy(x => x.DepartmentCode), "ID", "DepartmentName", selected);
            return options;
        }
 
        /// <summary>
        /// 查詢EmployeeData
        /// </summary>
        /// <param name="EmployeeData"></param>
        /// <param name="IsGuid">是否要轉GUID</param>
        /// <returns></returns>
        private EmployeeInfoManageViewModel SetEmployeeInfoManageViewModel(string EmployeeData, bool IsGuid = false)
        {
            EmployeeInfoManageViewModel viewModel = new EmployeeInfoManageViewModel();
            if (IsGuid)
            {
                Guid employeeID = Guid.Parse(EmployeeData);
                viewModel.employeeData = Services.GetService<EmployeeService>().GetAll().Where(x => x.ID == employeeID).FirstOrDefault();
            }
            else
            {    Guid employeeID = Guid.Parse(EmployeeData);
                viewModel.employeeData = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO.Contains(EmployeeData) || x.EmployeeName.Contains(EmployeeData)).FirstOrDefault();
            }
            //判斷是否有撈到資料
            if (viewModel.employeeData != null && viewModel.employeeData.SignDepartmentID != Guid.Empty)
            {
                Guid signDeapartmentID = (viewModel.employeeData.SignDepartmentID != null ? viewModel.employeeData.SignDepartmentID : Guid.Empty);
                viewModel.signDepartmentLists = GetSignDepartmentList(signDeapartmentID,viewModel.employeeData.CompanyID);
            }
            viewModel.roleLists = GetRoleList(viewModel.employeeData.RoleID.ToString());
            return viewModel;
        }
    }
}
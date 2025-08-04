using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.M06.Controllers
{
    public class DepartmentManagerController : BaseController
    {
        private DepartmentService _service;
        public DepartmentService Service
        {
            get
            {
                if (_service == null)
                {
                    _service = Services.GetService<DepartmentService>();
                }
                return _service;
            }
        }

        // GET: M06/DepartmentManager
        public ActionResult Index()
        {
            GetDefaultData();
            return View();
        }

        public ActionResult GetFilterDepartmentList(Guid companyId, string txtkeyword = "")
        {
            ViewBag.CompanyId = companyId;
            List<DepartmentTreeViewItem> _data = Service.GetSearchDepartmentLists(txtkeyword, companyId)
                .OrderBy(x => x.DepartmentCode)
                .Select(
                x => new DepartmentTreeViewItem()
                {
                    ID = x.ID,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName,
                    ParentID = x.SignParentID,
                }).ToList();

            return Json(_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDepartmentList(Guid companyId)
        {
            ViewBag.CompanyId = companyId;
            List<DepartmentTreeViewItem> _data = Service.Where(x => x.CompanyID == companyId && x.Enabled)
                .OrderBy(x => x.DepartmentCode)
                .Select(
                x => new DepartmentTreeViewItem()
                {
                    ID = x.ID,
                    DepartmentCode = x.DepartmentCode,
                    DepartmentName = x.DepartmentName,
                    ParentID = x.SignParentID,
                }).ToList();

            return Json(_data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateDepartment(Guid companyId)
        {
            SelectListItem _novaluItem = new SelectListItem() { Text = "無", Value = string.Empty };

            List<SelectListItem> _departmentList = GetDepartmentItemList(companyId, Guid.Empty);
            _departmentList.Insert(0, _novaluItem);
            ViewData["Departments"] = _departmentList;

            List<SelectListItem> _employeeList = GetEmployeeItemList(companyId, Guid.Empty);
            _employeeList.Insert(0, _novaluItem);
            ViewData["Employees"] = _employeeList;

            Department _model = new Department()
            {
                ID = Guid.NewGuid(),
                CompanyID = companyId,
            };

            return PartialView("_CreateDepartment", _model);
        }

        [HttpPost]
        public ActionResult CreateDepartment(Department model)
        {
            if (ModelState.IsValid)
            {
                model.ParentDepartmentID = model.SignParentID;
                model.BeginDate = DateTime.Today;
                model.Enabled = true;
                model.CreatedTime = DateTime.Now;
                model.OnlyForSign = true;

                int IsSuccess = Service.Create(model);
                if (IsSuccess == 1)
                {
                    TempData["CompanyID"] = model.CompanyID;
                    TempData["message"] = "新增成功";
                    WriteLog("Success:" + model.ID);
                    return Json(new { success = true });
                }
            }

            SelectListItem _novaluItem = new SelectListItem() { Text = "無", Value = string.Empty };

            List<SelectListItem> _departmentList = GetDepartmentItemList(model.CompanyID, model.SignParentID);
            _departmentList.Insert(0, _novaluItem);
            _departmentList.RemoveAll(x => x.Value == model.ID.ToString());
            ViewData["Departments"] = _departmentList;

            List<SelectListItem> _employeeList = GetEmployeeItemList(model.CompanyID, model.SignManagerId);
            _employeeList.Insert(0, _novaluItem);
            ViewData["Employees"] = _employeeList;

            return PartialView("_CreateDepartment", model);
        }

        public ActionResult EditDepartment(Guid departmentId)
        {
            Department _model = Service.FirstOrDefault(x => x.ID == departmentId);

            SelectListItem _novaluItem = new SelectListItem() { Text = "無", Value = string.Empty };

            List<SelectListItem> _departmentList = GetDepartmentItemList(_model.CompanyID, _model.SignParentID);
            _departmentList.Insert(0, _novaluItem);
            _departmentList.RemoveAll(x => x.Value == departmentId.ToString());
            ViewData["Departments"] = _departmentList;

            List<SelectListItem> _employeeList = GetEmployeeItemList(_model.CompanyID, _model.SignManagerId);
            _employeeList.Insert(0, _novaluItem);
            ViewData["Employees"] = _employeeList;

            return PartialView("_EditDepartment", _model);
        }

        [HttpPost]
        public ActionResult EditDepartment(Department model)
        {
            if (ModelState.IsValid)
            {
                Department _department = Service.FirstOrDefault(x => x.ID == model.ID);

                _department.SignParentID = model.SignParentID;
                _department.SignManagerId = model.SignManagerId;

                int IsSuccess = Service.Update(_department);
                if (IsSuccess == 1)
                {
                    TempData["CompanyID"] = model.CompanyID;
                    TempData["message"] = "儲存成功";
                    WriteLog("Success:" + model.ID);
                    return Json(new { success = true });
                }
            }

            SelectListItem _novaluItem = new SelectListItem() { Text = "無", Value = string.Empty };

            List<SelectListItem> _departmentList = GetDepartmentItemList(model.CompanyID, model.SignParentID);
            _departmentList.Insert(0, _novaluItem);
            _departmentList.RemoveAll(x => x.Value == model.ID.ToString());
            ViewData["Departments"] = _departmentList;

            List<SelectListItem> _employeeList = GetEmployeeItemList(model.CompanyID, model.SignManagerId);
            _employeeList.Insert(0, _novaluItem);
            ViewData["Employees"] = _employeeList;

            return PartialView("_CreateDepartment", model);
        }

        [HttpPost]
        public ActionResult DeleteDepartment(Guid departmentId)
        {
            DepartmentService _service = Services.GetService<DepartmentService>();
            Department _department = _service.FirstOrDefault(x => x.ID == departmentId);

            if (_department == null)
            {
                return Json(new AjaxResult() { status = "failed", message = "無此部門" });
            }

            List<Department> _departmentList = new List<Department>();
            _departmentList.Add(_department);

            List<Department> _allDepartment = _service.Where(x => x.CompanyID == _department.CompanyID && x.Enabled).ToList();
            List<Department> _foundDepartmnet = _allDepartment.Where(x => x.SignParentID == departmentId).ToList();

            while (_foundDepartmnet != null && _foundDepartmnet.Count > 0)
            {
                _departmentList.AddRange(_foundDepartmnet);
                _foundDepartmnet = _allDepartment.Where(x => x.SignParentID.HasValue
                    && _foundDepartmnet.Select(y => y.ID).Contains(x.SignParentID.Value)).ToList();
            }

            List<Guid> _departmentIdList = _departmentList.Select(y => y.ID).ToList();
            if (Services.GetService<EmployeeService>().Any(x => _departmentIdList.Contains(x.SignDepartmentID)
                && x.LeaveDate==null))//20160422 離職人員可以忽略 by bee
            {
                return Json(new AjaxResult() { status = "failed", message = "欲停用的部門或子部門內仍有成員" });
            }

            try
            {
                _departmentList.ForEach(x => {
                    x.Enabled = false;
                    _service.Update(x); 
                });

                TempData["CompanyID"] = _department.CompanyID;
                TempData["message"] = string.Format("部門 {0} {1} 已停用", _department.DepartmentCode, _department.DepartmentName);
                WriteLog("Success:" + _department.ID);

                return Json(new AjaxResult() { status = "success", message = "已停用" });
            }
            catch
            {
                return Json(new AjaxResult() { status = "failed", message = "停用失敗" });
            }
        }

        private void GetDefaultData()
        {
            ViewData["CompanyList"] = GetCompanyItemList();
        }

        private List<SelectListItem> GetCompanyItemList(string selectedData = "")
        {
            //轉為Guid 判斷ID
            Guid _selectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selectedData))
            {
                _selectedDataID = Guid.Parse(selectedData);
            }

            List<SelectListItem> _listItem = new List<SelectListItem>();
            List<Company> _data = Services.GetService<CompanyService>().Where(x => x.Enabled).OrderBy(x => x.CompanyCode).ToList();

            foreach (var _item in _data)
            {
                _listItem.Add(new SelectListItem
                {
                    Text = string.Format("{0} {1}", _item.CompanyCode, _item.CompanyName),
                    Value = _item.ID.ToString(),
                    Selected = (_selectedDataID == _item.ID ? true : false)
                });
            }

            return _listItem;
        }

        private List<SelectListItem> GetDepartmentItemList(Guid companyId, Guid? selectedDataID)
        {
            List<SelectListItem> _listItem = new List<SelectListItem>();
            List<Department> _data = Service.Where(x => x.CompanyID == companyId && x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

            foreach (var _item in _data)
            {
                _listItem.Add(new SelectListItem
                {
                    Text = string.Format("{0} {1}", _item.DepartmentCode, _item.DepartmentName),
                    Value = _item.ID.ToString(),
                    Selected = (selectedDataID == _item.ID ? true : false)
                });
            }

            return _listItem;
        }

        private List<SelectListItem> GetEmployeeItemList(Guid companyId, Guid? selectedDataID)
        {

            List<SelectListItem> _listItem = new List<SelectListItem>();
            List<Employee> _data = Services.GetService<EmployeeService>()
                .Where(x => x.CompanyID == companyId && x.Enabled).OrderBy(x => x.EmployeeNO).ToList();

            foreach (var _item in _data)
            {
                _listItem.Add(new SelectListItem
                {
                    Text = string.Format("{0} {1}", _item.EmployeeNO, _item.EmployeeName),
                    Value = _item.ID.ToString(),
                    Selected = (selectedDataID == _item.ID ? true : false)
                });
            }

            return _listItem;
        }
    }
}
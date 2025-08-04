using HRPortal.ApiAdapter;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.SignFlow.Databases.Repositories;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class SignFlowSettingController : BaseController
    {
        // GET: ToDo/SignFlowSetting
        public async Task<ActionResult> Index(string designId, string type, string companyCode, string departmentCode)
        {
            if (string.IsNullOrEmpty(designId)) //SignFlowAssignDept
            {
                SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();

                List<SignFlowAssignDeptViewModel> _model = new List<SignFlowAssignDeptViewModel>();
                List<SignFlowAssignDept> _data;
                if (string.IsNullOrEmpty(companyCode))
                {
                    _data = _assignDeptRepository.GetAll().Where(x => x.IsUsed == "Y").ToList();
                }
                else
                {
                    _data = _assignDeptRepository.GetAll().Where(x => x.IsUsed == "Y" && x.CompanyID == companyCode).ToList();
                }
                if (!string.IsNullOrEmpty(departmentCode))
                {
                    _data = _data.Where(x => x.DeptID == departmentCode).ToList();
                }
                _data = _data.OrderBy(x => x.FormType).ThenBy(x => x.DeptID).ThenBy(x => x.EmpID).ToList();
                List<Company> _companys = Services.GetService<CompanyService>().GetAll().ToList();
                List<Department> _departments = Services.GetService<DepartmentService>().GetAll().ToList();
                List<Employee> _employees = Services.GetService<EmployeeService>().GetAll().ToList();
                List<Option> _formTypes = Services.GetService<OptionService>().GetOptionListByGroup("FormType").ToList();

                foreach (SignFlowAssignDept _assign in _data)
                {
                    SignFlowAssignDeptViewModel _item = new SignFlowAssignDeptViewModel()
                    {
                        ID = _assign.ID,
                        FormType = _assign.FormType,
                        CompanyID = _assign.CompanyID,
                        CompanyName = _assign.CompanyID,
                        DeptID = _assign.DeptID,
                        DeptName = _assign.DeptID,
                        DesignID = _assign.DesignID,
                        EmpID = _assign.EmpID,
                        EmpName = _assign.EmpID,
                    };

                    Option _formType = _formTypes.SingleOrDefault(x => x.OptionValue == _assign.FormType);
                    _item.FormTypeName = _formType != null ? _formType.DisplayName : _assign.FormType;

                    Company _company = _companys.FirstOrDefault(x => x.CompanyCode == _assign.CompanyID);
                    _item.CompanyName += _company != null ? " " + _company.CompanyName : string.Empty;

                    if (_assign.DeptID == "ALL")
                    {
                        _item.DeptName = "所有部門";
                    }
                    else
                    {
                        Department _department = null;
                        Employee _employee = null;
                        if (_company != null)
                        {
                            _department = _departments.FirstOrDefault(x => x.CompanyID == _company.ID && x.DepartmentCode == _assign.DeptID);
                            _employee = _employees.FirstOrDefault(x => x.CompanyID == _company.ID && x.EmployeeNO == _assign.EmpID);
                        }
                        _item.DeptName += _department != null ? " " + _department.DepartmentName : string.Empty;
                        _item.EmpName += _employee != null ? " " + _employee.EmployeeName : string.Empty;
                    }

                    _model.Add(_item);
                }

                List<SelectListItem> _companyList = GetCompanyList();
                _companyList.Insert(0, new SelectListItem()
                {
                    Text = "所有公司",
                    Value = "",
                });
                ViewBag.FilterCompanyCode = companyCode;
                ViewData["CompanyFilterList"] = _companyList;
                List<SelectListItem> _departmentList = GetDepartmentList(companyCode);
                _departmentList.Insert(0, new SelectListItem() { Text = "所有部門", Value = "ALL" });
                ViewBag.FilterDepartmentCode = departmentCode;
                ViewData["DepartmentFilterList"] = _departmentList;

                return View(_model);
            }
            else
            {
                SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
                SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId);

                try
                {
                    Company _company = Services.GetService<CompanyService>().FirstOrDefault(x => x.CompanyCode == _assignDept.CompanyID);
                    ViewBag.CompanyName = _company.CompanyName;
                    if (_assignDept.DeptID == "ALL")
                    {
                        ViewBag.DepartmentName = "所有部門";
                    }
                    else
                    {
                        ViewBag.DepartmentName = Services.GetService<DepartmentService>().FirstOrDefault(x => x.CompanyID == _company.ID && x.DepartmentCode == _assignDept.DeptID);
                    }
                    ViewBag.FormType = Services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _assignDept.FormType).DisplayName;
                }
                catch
                {
                }

                if (type == "design")   //SignFlowDesign
                {
                    SignFlowFormLevelRepository _formLevelRepository = new SignFlowFormLevelRepository();
                    SignFlowDesignRepository _designRepository = new SignFlowDesignRepository();

                    List<SignFlowDesignViewModel> _model = new List<SignFlowDesignViewModel>();
                    List<SignFlowDesign> _data = _designRepository.GetByDesignID(designId).OrderBy(x => x.SignOrder).ToList();
                    List<Option> _deptTypes = Services.GetService<OptionService>().GetOptionListByGroup("SignDeptType").ToList();
                    List<SignFlowFormLevel> _formLevels = _formLevelRepository.GetSignFlowFormLevelByFormType(_assignDept.FormType).ToList();
                    //List<Department> _departments = Services.GetService<DepartmentService>().Where(x => x.Company.CompanyCode == _assignDept.CompanyID).ToList();
                    List<Department> _departments = Services.GetService<DepartmentService>().GetAll().ToList();
                    //List<Employee> _employees = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == _assignDept.CompanyID).ToList();
                    List<Employee> _employees = Services.GetService<EmployeeService>().GetAll().ToList();

                    foreach (SignFlowDesign _design in _data)
                    {
                        SignFlowDesignViewModel _item = new SignFlowDesignViewModel()
                        {
                            ID = _design.ID,
                            DesignID = _design.DesignID,
                            SignOrder = _design.SignOrder.ToString("0"),
                            FormLevelID = _design.FormLevelID,
                            DeptType = _design.DeptType.HasValue ? _design.DeptType.Value.ToString() : string.Empty,
                        };

                        SignFlowFormLevel _level = _formLevels.FirstOrDefault(x => x.FormLevelID == _design.FormLevelID);
                        _item.FormLevelName = _level != null ? _level.Name : _design.FormLevelID;

                        Option _deptType = _deptTypes.FirstOrDefault(x => x.OptionValue == _item.DeptType);
                        _item.DeptTypeName = _deptType != null ? _deptType.DisplayName : _item.DeptType;

                        if (string.IsNullOrEmpty(_item.DeptType))    //指定簽核人員
                        {
                            _item.SignerID = _design.SignerID;
                            _item.SignerName = _design.SignerID;
                            string CompanyCode = _design.SignCompanyID;
                            Employee _employee = new Employee();
                            if (CompanyCode != null)
                            {
                                _employee = _employees.FirstOrDefault(x => x.EmployeeNO == _design.SignerID && x.Company.CompanyCode == CompanyCode);
                            }
                            else if (CompanyCode == null)
                            {
                                _employee = _employees.FirstOrDefault(x => x.EmployeeNO == _design.SignerID && x.Company.CompanyCode == _assignDept.CompanyID);
                            }

                            if (_employee != null)
                            {
                                _item.SignerName += " " + _employee.EmployeeName;
                            }
                        }
                        else if (_item.DeptType == "0" || _item.DeptType == "15") //指定部門
                        {
                            _item.SignerID = _design.SignDeptID;
                            _item.SignerName = _design.SignDeptID;
                            string CompanyCode = _design.SignCompanyID;
                            Department _department = new Department();
                            if (CompanyCode != null)
                            {
                                _department = _departments.FirstOrDefault(x => x.DepartmentCode == _design.SignDeptID && x.Company.CompanyCode == CompanyCode);
                            }
                            else if (CompanyCode == null)
                            {
                                _department = _departments.FirstOrDefault(x => x.DepartmentCode == _design.SignDeptID && x.Company.CompanyCode == _assignDept.CompanyID);
                            }
                            if (_department != null)
                            {
                                _item.SignerName += " " + _department.DepartmentName;
                            }
                        }

                        _model.Add(_item);
                    }

                    TempData["DesignID"] = designId;

                    return View("SignFlowDesignView", _model);
                }
                else //SignFlowCondition
                {
                    SignFlowConditionsRepository _conditionsRepository = new SignFlowConditionsRepository();
                    SignFlowFormLevelRepository _formLevelRepository = new SignFlowFormLevelRepository();

                    List<SignFlowConditionViewModel> _model = new List<SignFlowConditionViewModel>();
                    List<SignFlowConditions> _data = _conditionsRepository.GetByDesignID(designId).ToList();
                    List<SignFlowFormLevel> _formLevels = _formLevelRepository.GetSignFlowFormLevelByFormType(_assignDept.FormType).ToList();
                    List<Option> _conditionTypes = Services.GetService<OptionService>().GetOptionListByGroup(_assignDept.FormType + "Condition").ToList();

                    Dictionary<string, string> absentData = await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode);
                    Session["AbsentList"] = GetAbsentList(ref absentData);

                    foreach (SignFlowConditions _condition in _data)
                    {
                        SignFlowConditionViewModel _item = new SignFlowConditionViewModel()
                        {
                            ID = _condition.ID,
                            DesignID = _condition.DesignID,
                            LevelID = _condition.LevelID,
                            ConditionType = _condition.ConditionType,
                            Parameters = _condition.Parameters,
                            AbsentCode = _condition.AbsentCode,
                            AbsentName = FindAbsentName(ref absentData, _condition.AbsentCode)
                        };

                        SignFlowFormLevel _level = _formLevels.FirstOrDefault(x => x.LevelID == _condition.LevelID);
                        _item.LevelName = _level != null ? _level.Name : _condition.LevelID;

                        Option _conditionType = _conditionTypes.FirstOrDefault(x => x.OptionValue == _item.ConditionType);
                        _item.ConditionDisplayName = _conditionType != null ? _conditionType.DisplayName : _item.ConditionType;

                        _model.Add(_item);
                    }
                    TempData["DesignID"] = designId;
                    ViewData["FormType"] = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId).FormType;

                    return View("SignFlowConditionView", _model);
                }
            }
        }

        private string FindAbsentName(ref Dictionary<string, string> data, string code)
        {
            string result = "";

            try
            {
                foreach (var item in data)
                {
                    if (item.Key == code)
                    {
                        result = item.Value;
                        break;
                    }
                }
            }
            catch
            {
                result = "";
            }

            return result;
        }

        #region SignFlowAssignDept

        public ActionResult AddSignFlow()
        {
            OptionListHelper _optionListHelper = new OptionListHelper();
            ViewData["FormTypeList"] = _optionListHelper.GetOptionList("FormType").Where(x => !x.Value.ToLower().Contains("cancel")).ToList();
            ViewData["CompanyList"] = GetCompanyList();

            List<SelectListItem> _departmentList = new List<SelectListItem>();
            _departmentList.Add(new SelectListItem() { Text = "所有部門", Value = "ALL" });
            ViewData["DepartmentList"] = _departmentList;
            return PartialView("_AddSignFlow");
        }

        [HttpPost]
        public ActionResult AddSignFlow(SignFlowAssignDeptViewModel model)
        {
            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
            SignFlowSeqRepository _seqRepository = new SignFlowSeqRepository();
            List<Employee> _employees = Services.GetService<EmployeeService>().GetAll().ToList();
            var DeptIDArray = model.DeptID.Split(',');
            string[] tempArray = new string[5];

            if (model.EmpID == "ALL" || model.EmpID == null)
            {
                tempArray = DeptIDArray;
            }
            else if (model.EmpID != "ALL")
            {
                tempArray = model.EmpID.Split(',');
            }

            foreach (var tempID in tempArray)
            {
                SignFlowAssignDept _model = new SignFlowAssignDept(); ;
                _model.ID = Guid.NewGuid();
                _model.DesignID = _seqRepository.GetSignFlowDesingSeq();
                _model.CompanyID = model.CompanyID;
                if (model.EmpID == "ALL" || model.EmpID == null)//部門
                {
                    _model.DeptID = tempID.ToString();
                }
                else if (model.EmpID != "ALL") //員工
                {
                    string tempEmp = _employees.Where(x => x.Company.CompanyCode == model.CompanyID && x.EmployeeNO == tempID.ToString()).Select(x => x.SignDepartment.DepartmentCode).FirstOrDefault().ToString();
                    _model.DeptID = tempEmp;
                    _model.EmpID = tempID.ToString();
                }
                _model.FormType = model.FormType;
                _model.IsUsed = "Y";
                //if (model.EmpID != "ALL")
                //{
                //    _model.EmpID = model.EmpID;
                //}

                try
                {
                    _assignDeptRepository.Create(_model);
                    _assignDeptRepository.SaveChanges();
                    TempData["message"] = "新增成功";
                    WriteLog("Success:" + _model.ID);
                    if (DeptIDArray.Length == 1)
                    {
                        return RedirectToAction("Index", new { designId = _model.DesignID, type = "design" });
                    }
                }
                catch
                {
                    OptionListHelper _optionListHelper = new OptionListHelper();
                    ViewData["FormTypeList"] = _optionListHelper.GetOptionList("FormType").Where(x => !x.Value.ToLower().Contains("cancel")).ToList();
                    ViewData["CompanyList"] = GetCompanyList();
                    TempData["message"] = "新增失敗";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteSignFlow(Guid assignId)
        {
            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
            SignFlowDesignRepository _designRepository = new SignFlowDesignRepository();

            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.ID == assignId);
            List<SignFlowDesign> _designs = _designRepository.GetByDesignID(_assignDept.DesignID).ToList();


            try
            {
                _assignDept.IsUsed = "N";
                _assignDeptRepository.Update(_assignDept);
                _assignDeptRepository.SaveChanges();

                foreach (SignFlowDesign _design in _designs)
                {
                    _design.IsUsed = "N";
                    _designRepository.Update(_design);
                }
                _designRepository.SaveChanges();

                TempData["message"] = "刪除成功";
                WriteLog("Success:" + assignId);

                return Json(new AjaxResult() { status = "success", message = "刪除成功" });
            }
            catch
            {
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
            }
        }

        #endregion

        #region SignFlowDesign

        public ActionResult AddSignFlowItem(string designId)
        {
            SignFlowDesignViewModel _model = new SignFlowDesignViewModel();
            _model.DesignID = designId;

            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();

            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId);

            OptionListHelper _optionListHelper = new OptionListHelper();
            ViewData["FormLevelList"] = GetFormLevelList(_assignDept.FormType);
            ViewData["DeptTypeList"] = _optionListHelper.GetOptionList("SignDeptType").OrderBy(x => x.Value);
            ViewData["CompanysList"] = new List<SelectListItem>(); // 新增可選擇跨公司功能 20160531 by bee
            ViewData["SignerList"] = new List<SelectListItem>();


            return PartialView("_AddSignFlowItem", _model);
        }

        [HttpPost]
        public ActionResult AddSignFlowItem(SignFlowDesignViewModel model)
        {
            SignFlowDesignRepository _repository = new SignFlowDesignRepository();

            IQueryable<SignFlowDesign> _currentDesign = _repository.GetByDesignID(model.DesignID);

            decimal _order = 1;
            if (_currentDesign.Count() > 0)
            {
                _order = _currentDesign.Max(x => x.SignOrder) + 1;
            }

            SignFlowDesign _model = new SignFlowDesign();
            _model.ID = Guid.NewGuid();
            _model.DesignID = model.DesignID;
            _model.FormLevelID = model.FormLevelID;
            _model.SignCompanyID = model.CompanyID;
            if (string.IsNullOrEmpty(model.DeptType))
            {
                _model.DeptType = null;
                _model.SignerID = model.SignerID;
            }
            else if (model.DeptType == "0")
            {
                _model.DeptType = 0;
                _model.SignDeptID = model.SignerID;
            }
            else if (model.DeptType == "15")
            {
                _model.DeptType = 15;
                _model.SignDeptID = model.SignerID;
            }
            else
            {
                _model.DeptType = decimal.Parse(model.DeptType);
            }

            _model.SignOrder = _order;
            _model.IsUsed = "Y";

            try
            {
                _repository.Create(_model);
                _repository.SaveChanges();
                TempData["message"] = "新增成功";
                WriteLog("Success:" + _model.ID);
                return Json(new { success = true });
            }
            catch
            {
                SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
                SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == model.DesignID);
                OptionListHelper _optionListHelper = new OptionListHelper();
                ViewData["FormLevelList"] = GetFormLevelList(_assignDept.FormType);
                ViewData["DeptTypeList"] = _optionListHelper.GetOptionList("SignDeptType").OrderBy(x => x.Value);
                ViewData["SignerList"] = new List<SelectListItem>();
                TempData["message"] = "新增失敗";
                return PartialView("_AddSignFlowItem", model);
            }
        }

        public ActionResult EditSignFlowItem(Guid itemId)
        {
            SignFlowDesignRepository _designRepository = new SignFlowDesignRepository();
            SignFlowDesign _data = _designRepository.GetAll().FirstOrDefault(x => x.ID == itemId);

            SignFlowDesignViewModel _model = new SignFlowDesignViewModel()
            {
                ID = _data.ID,
                DesignID = _data.DesignID,
                FormLevelID = _data.FormLevelID,
                DeptType = _data.DeptType.HasValue ? _data.DeptType.Value.ToString("0") : string.Empty,
                SignerID = _data.DeptType.HasValue ? (_data.DeptType.Value == 0 || _data.DeptType.Value == 15 ? _data.SignDeptID : string.Empty) : _data.SignerID,
                SignOrder = _data.SignOrder.ToString("0"),
                CompanyID = _data.SignCompanyID,
            };

            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();

            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == _data.DesignID);

            OptionListHelper _optionListHelper = new OptionListHelper();
            ViewData["FormLevelList"] = GetFormLevelList(_assignDept.FormType);
            ViewData["DeptTypeList"] = _optionListHelper.GetOptionList("SignDeptType").OrderBy(x => x.Value);
            ViewData["SignerList"] = new List<SelectListItem>();
            ViewData["CompanysList"] = new List<SelectListItem>();

            return PartialView("_EditSignFlowItem", _model);
        }

        [HttpPost]
        public ActionResult EditSignFlowItem(SignFlowDesignViewModel model)
        {
            string oSignerID = "";
            SignFlowDesignRepository _repository = new SignFlowDesignRepository();

            SignFlowDesign _model = _repository.GetAll().FirstOrDefault(x => x.ID == model.ID);
            _model.FormLevelID = model.FormLevelID;
            if (string.IsNullOrEmpty(model.DeptType))
            {
                if (!string.IsNullOrWhiteSpace(_model.SignerID) && !string.IsNullOrWhiteSpace(model.SignerID) && _model.SignerID != model.SignerID)
                {
                    oSignerID = _model.SignerID;
                }

                _model.DeptType = null;
                _model.SignerID = model.SignerID;
                _model.SignCompanyID = model.CompanyID;
                _model.SignDeptID = null;
            }
            else if (model.DeptType == "0")
            {
                _model.DeptType = 0;
                _model.SignerID = null;
                _model.SignDeptID = model.SignerID;
                _model.SignCompanyID = model.CompanyID;
            }
            else if (model.DeptType == "15")
            {
                _model.DeptType = 15;
                _model.SignerID = null;
                _model.SignDeptID = model.SignerID;
                _model.SignCompanyID = model.CompanyID;
            }
            else
            {
                _model.DeptType = decimal.Parse(model.DeptType);
                _model.SignerID = null;
                _model.SignDeptID = null;
                _model.SignCompanyID = model.CompanyID;
            }

            try
            {
                _repository.Update(_model);
                _repository.SaveChanges();
                TempData["message"] = "更新成功";
                WriteLog("Success:" + model.ID);

                if (!string.IsNullOrWhiteSpace(oSignerID))
                {
                    //20180509 Frank 還在跑的單變更簽核人
                    new SignFlow.Model.FormChangeSign(oSignerID);
                }

                return Json(new { success = true });
            }
            catch
            {
                SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
                SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == model.DesignID);
                OptionListHelper _optionListHelper = new OptionListHelper();
                ViewData["FormLevelList"] = GetFormLevelList(_assignDept.FormType);
                ViewData["DeptTypeList"] = _optionListHelper.GetOptionList("SignDeptType").OrderBy(x => x.Value);
                ViewData["SignerList"] = new List<SelectListItem>();
                TempData["message"] = "更新失敗";
                return PartialView("_AddSignFlowItem", model);
            }
        }

        [HttpPost]
        public ActionResult DeleteSignFlowItem(string designId, decimal order)
        {
            SignFlowDesignRepository _repository = new SignFlowDesignRepository();

            List<SignFlowDesign> _data = _repository.GetByDesignID(designId).OrderBy(x => x.SignOrder).ToList();
            int i = _data.FindIndex(x => x.SignOrder == order);
            Guid _id = _data[i].ID;

            try
            {
                _data[i].IsUsed = "N";
                _repository.Update(_data[i]);
                i++;

                for (; i < _data.Count; i++)
                {
                    _data[i].SignOrder--;
                    _repository.Update(_data[i]);
                }

                _repository.SaveChanges();

                TempData["message"] = "刪除成功";
                WriteLog("Success:" + _id);

                return Json(new AjaxResult() { status = "success", message = "刪除成功" });
            }
            catch
            {
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
            }
        }

        [HttpPost]
        public ActionResult MoveUpFlowItem(string designId, decimal order)
        {
            SignFlowDesignRepository _repository = new SignFlowDesignRepository();

            List<SignFlowDesign> _data = _repository.GetByDesignID(designId).OrderBy(x => x.SignOrder).ToList();
            int i = _data.FindIndex(x => x.SignOrder == order);
            int j = i - 1;
            if (i <= 0)
            {
                return Json(new AjaxResult() { status = "failed", message = "上移失敗" });
            }

            try
            {
                _data[i].SignOrder--;
                _repository.Update(_data[i]);
                _data[j].SignOrder++;
                _repository.Update(_data[j]);
                _repository.SaveChanges();

                WriteLog("Success:" + _data[i].ID);

                return Json(new AjaxResult() { status = "success", message = "上移成功" });
            }
            catch
            {
                return Json(new AjaxResult() { status = "failed", message = "上移失敗" });
            }
        }

        [HttpPost]
        public ActionResult MoveDownFlowItem(string designId, decimal order)
        {
            SignFlowDesignRepository _repository = new SignFlowDesignRepository();

            List<SignFlowDesign> _data = _repository.GetByDesignID(designId).OrderBy(x => x.SignOrder).ToList();
            int i = _data.FindIndex(x => x.SignOrder == order);
            int j = i + 1;
            if (i < 0 || j >= _data.Count)
            {
                return Json(new AjaxResult() { status = "failed", message = "下移失敗" });
            }

            try
            {
                _data[i].SignOrder++;
                _repository.Update(_data[i]);
                _data[j].SignOrder--;
                _repository.Update(_data[j]);
                _repository.SaveChanges();
                WriteLog("Success:" + _data[i].ID);

                return Json(new AjaxResult() { status = "success", message = "下移成功" });
            }
            catch
            {
                return Json(new AjaxResult() { status = "failed", message = "下移失敗" });
            }
        }

        #endregion

        #region SignFlowCondition

        public ActionResult AddSignFlowCondition(string designId)
        {
            SignFlowConditionViewModel _model = new SignFlowConditionViewModel();
            _model.DesignID = designId;

            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();

            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId);

            OptionListHelper _optionListHelper = new OptionListHelper();
            ViewData["FormLevelList"] = GetLevelList(_assignDept.FormType);
            ViewData["ConditionTypeList"] = _optionListHelper.GetOptionList(_assignDept.FormType + "Condition");
            ViewData["AbsentList"] = Session["AbsentList"];

            return PartialView("_AddSignFlowCondition", _model);
        }

        [HttpPost]
        public ActionResult AddSignFlowCondition(SignFlowConditionViewModel model)
        {
            SignFlowConditionsRepository _repository = new SignFlowConditionsRepository();

            SignFlowConditions _model = new SignFlowConditions();
            _model.ID = Guid.NewGuid();
            _model.DesignID = model.DesignID;
            _model.LevelID = model.LevelID;
            _model.ConditionType = model.ConditionType;
            _model.Parameters = model.Parameters;
            _model.IsUsed = "Y";
            _model.AbsentCode = model.ConditionType == "NDays" ? model.AbsentCode : "";

            var tmpData = _repository.GetAll().Where(x => x.DesignID == _model.DesignID
                                                       && x.LevelID == _model.LevelID
                                                       && x.ConditionType == _model.ConditionType
                                                       && x.AbsentCode == _model.AbsentCode
                                                       && x.IsUsed == "Y").FirstOrDefault();

            if (tmpData == null)
            {
                try
                {
                    _repository.Create(_model);
                    _repository.SaveChanges();

                    WriteLog("Success:" + _model.ID);

                    return Json(new AjaxResult() { status = "success", message = "新增成功" });
                }
                catch
                {
                    return Json(new AjaxResult() { status = "failed", message = "新增失敗" });
                }
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "新增失敗，該筆資料已存在" });
            }
        }

        public ActionResult EditSignFlowCondition(Guid conditionId)
        {
            SignFlowConditionsRepository _Repository = new SignFlowConditionsRepository();
            SignFlowConditions _data = _Repository.GetAll().FirstOrDefault(x => x.ID == conditionId);

            SignFlowConditionViewModel _model = new SignFlowConditionViewModel()
            {
                ID = _data.ID,
                DesignID = _data.DesignID,
                LevelID = _data.LevelID,
                ConditionType = _data.ConditionType,
                Parameters = _data.Parameters,
                AbsentCode = _data.AbsentCode
            };

            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();

            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == _data.DesignID);

            OptionListHelper _optionListHelper = new OptionListHelper();
            ViewData["FormLevelList"] = GetLevelList(_assignDept.FormType);
            ViewData["ConditionTypeList"] = _optionListHelper.GetOptionList(_assignDept.FormType + "Condition");
            ViewData["AbsentList"] = Session["AbsentList"];

            return PartialView("_EditSignFlowCondition", _model);
        }

        [HttpPost]
        public ActionResult EditSignFlowCondition(SignFlowConditionViewModel model)
        {
            SignFlowConditionsRepository _repository = new SignFlowConditionsRepository();

            SignFlowConditions _model = _repository.GetAll().FirstOrDefault(x => x.ID == model.ID);
            _model.LevelID = model.LevelID;
            _model.ConditionType = model.ConditionType;
            _model.Parameters = model.Parameters;
            _model.AbsentCode = model.ConditionType == "NDays" ? model.AbsentCode : "";

            var tmpData = _repository.GetAll().Where(x => x.ID != _model.ID
                                                       && x.DesignID == _model.DesignID
                                                       && x.LevelID == _model.LevelID
                                                       && x.ConditionType == _model.ConditionType
                                                       && x.AbsentCode == _model.AbsentCode
                                                       && x.IsUsed == "Y").FirstOrDefault();

            if (tmpData == null)
            {
                try
                {
                    _repository.Update(_model);
                    _repository.SaveChanges();

                    WriteLog("Success:" + model.ID);

                    return Json(new AjaxResult() { status = "success", message = "更新成功" });
                }
                catch
                {
                    return Json(new AjaxResult() { status = "failed", message = "更新失敗" });
                }
            }
            else
            {
                return Json(new AjaxResult() { status = "failed", message = "更新失敗，該筆資料已存在" });
            }
        }

        [HttpPost]
        public ActionResult DeleteSignFlowCondition(Guid conditionId)
        {
            SignFlowConditionsRepository _repository = new SignFlowConditionsRepository();

            SignFlowConditions _data = _repository.GetAll().FirstOrDefault(x => x.ID == conditionId);

            try
            {
                _data.IsUsed = "N";
                _repository.Update(_data);
                _repository.SaveChanges();

                TempData["message"] = "刪除成功";
                WriteLog("Success:" + conditionId);

                return Json(new AjaxResult() { status = "success", message = "刪除成功" });
            }
            catch
            {
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
            }
        }

        #endregion

        #region List and Options

        public ActionResult GetDepartmentOptions(string companyCode)
        {
            List<SelectListItem> _departmentList = GetDepartmentList(companyCode);
            _departmentList.Insert(0, new SelectListItem() { Text = "所有部門", Value = "ALL" });
            return Json(_departmentList, JsonRequestBehavior.AllowGet);
        }

        //20160123 獲得部門員工選項 by bee
        public ActionResult GetEmployeeOptions(string companyCode, string DepartmentCode)
        {
            List<SelectListItem> _employeeList = GetEmployeeList(companyCode, DepartmentCode);
            _employeeList.Insert(0, new SelectListItem() { Text = "所有人員", Value = "ALL" });
            return Json(_employeeList, JsonRequestBehavior.AllowGet);
        }

        //20160426 取得複數部門人員
        public ActionResult GetMultiEmployeeOption(string companyCode, string DepartmentCode)
        {
            List<SelectListItem> _employeeList = new List<SelectListItem>();
            var DepartmentCodeArray = DepartmentCode.Split(',');
            foreach (var curDeptCode in DepartmentCodeArray)
            {
                List<SelectListItem> _curEmp = GetEmployeeList(companyCode, curDeptCode, true);
                foreach (var curEmpInfo in _curEmp)
                {
                    _employeeList.Add(new SelectListItem() { Text = curEmpInfo.Text, Value = curEmpInfo.Value });
                }

            }

            _employeeList.Insert(0, new SelectListItem() { Text = "所有人員", Value = "ALL" });
            return Json(_employeeList, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetSignerOptions(string designId, string deptType, string companyID = null)
        {
            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId);
            string companyCode = (companyID == null || companyID == "") ? _assignDept.CompanyID : companyID;
            if (string.IsNullOrEmpty(deptType))
            {
                return Json(GetEmployeeList(companyCode), JsonRequestBehavior.AllowGet);
            }
            else if (deptType == "0" || deptType == "15")
            {
                return Json(GetDepartmentList(companyCode), JsonRequestBehavior.AllowGet);
            }
            return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
        }

        //抓取使用者目前的公司別 如果有總公司的話 會一併將總公司帶出 20160531 by Bee
        public ActionResult GetSignerCompanyOption(string designId, string deptType)
        {
            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId);
            if (deptType == "0" || deptType == "15" || deptType == "")
            {
                //return Json(GetDepartmentList(_assignDept.CompanyID), JsonRequestBehavior.AllowGet);
                return Json(GetMainCompanyList(designId), JsonRequestBehavior.AllowGet);
            }
            return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
        }

        //依據公司別抓取不同的公司別人員清單 20160601 by Bee
        public ActionResult GetDiffCompanySignerOptions(string CompanyCode, string deptType)
        {
            if (deptType == "0" || deptType == "15")
            {
                return Json(GetDepartmentList(CompanyCode), JsonRequestBehavior.AllowGet);
            }

            return Json(GetEmployeeList(CompanyCode), JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> GetCompanyList()
        {
            List<SelectListItem> _companyList = new List<SelectListItem>();
            List<Company> _companys = Services.GetService<CompanyService>().Where(x => x.Enabled).OrderBy(x => x.CompanyCode).ToList();
            foreach (Company _company in _companys)
            {
                _companyList.Add(new SelectListItem()
                {
                    Text = string.Format("{0} {1}", _company.CompanyCode, _company.CompanyName),
                    Value = _company.CompanyCode,
                });
            }
            return _companyList;
        }

        //獲得簽核設定的公司別和總公司 20160531 By Bee
        private List<SelectListItem> GetMainCompanyList(string designId)
        {
            SignFlowAssignDeptRepository _assignDeptRepository = new SignFlowAssignDeptRepository();
            SignFlowAssignDept _assignDept = _assignDeptRepository.GetAll().FirstOrDefault(x => x.DesignID == designId);
            List<SelectListItem> _companyList = new List<SelectListItem>();
            List<Company> _companys = Services.GetService<CompanyService>().Where(x => x.Enabled && (x.CompanyCode == _assignDept.CompanyID || x.MainFlag == true)).OrderBy(x => x.CompanyCode).ToList();
            foreach (Company _company in _companys)
            {
                _companyList.Add(new SelectListItem()
                {
                    Text = string.Format("{0} {1}", _company.CompanyCode, _company.CompanyName),
                    Value = _company.CompanyCode,
                    Selected = (_company.CompanyCode == _assignDept.CompanyID) ? true : false
                });
            }
            return _companyList;
        }

        private List<SelectListItem> GetDepartmentList(string companyCode)
        {
            List<SelectListItem> _departmentList = new List<SelectListItem>();
            List<Department> _departments = Services.GetService<DepartmentService>().Where(x => x.Company.CompanyCode == companyCode && x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
            foreach (Department _department in _departments)
            {
                _departmentList.Add(new SelectListItem()
                {
                    Text = string.Format("{0} {1}", _department.DepartmentCode, _department.DepartmentName),
                    Value = _department.DepartmentCode,
                });
            }
            return _departmentList;
        }

        private List<SelectListItem> GetEmployeeList(string companyCode)
        {
            List<SelectListItem> _employeeList = new List<SelectListItem>();
            List<Employee> _employees = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == companyCode && x.Enabled).OrderBy(x => x.EmployeeNO).ToList();
            foreach (Employee _employee in _employees)
            {
                _employeeList.Add(new SelectListItem()
                {
                    Text = string.Format("{0} {1}", _employee.EmployeeNO, _employee.EmployeeName),
                    Value = _employee.EmployeeNO,
                });
            }
            return _employeeList;
        }

        //20160123 by Bee  signDepart==true -> 抓後台簽核部門人員
        private List<SelectListItem> GetEmployeeList(string companyCode, string DepartmentCode, bool signDepart = false)
        {
            List<SelectListItem> _employeeList = new List<SelectListItem>();
            List<Employee> _employees = new List<Employee>();
            if (signDepart == false) //後台部門
            {
                _employees = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == companyCode && x.Department.DepartmentCode == DepartmentCode && x.Enabled && x.LeaveDate == null).OrderBy(x => x.EmployeeNO).ToList();
            }
            else if (signDepart == true) //前台簽核部門
            {
                _employees = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == companyCode && x.SignDepartment.DepartmentCode == DepartmentCode && x.Enabled && x.LeaveDate == null).OrderBy(x => x.EmployeeNO).ToList();
            }

            foreach (Employee _employee in _employees)
            {
                _employeeList.Add(new SelectListItem()
                {
                    Text = string.Format("{0} {1}", _employee.EmployeeNO, _employee.EmployeeName),
                    Value = _employee.EmployeeNO,
                });
            }
            return _employeeList;
        }

        public List<SelectListItem> GetFormLevelList(string formType)
        {
            SignFlowFormLevelRepository _formLevelRepository = new SignFlowFormLevelRepository();
            List<SignFlowFormLevel> _formLevels = _formLevelRepository.GetSignFlowFormLevelByFormType(formType).OrderBy(x => x.LevelID).ToList();

            List<SelectListItem> _formLevelList = new List<SelectListItem>();
            foreach (SignFlowFormLevel _formLevel in _formLevels)
            {
                _formLevelList.Add(new SelectListItem()
                {
                    Text = _formLevel.Name,
                    Value = _formLevel.FormLevelID,
                });
            }

            return _formLevelList;
        }

        public List<SelectListItem> GetLevelList(string formType)
        {
            SignFlowFormLevelRepository _formLevelRepository = new SignFlowFormLevelRepository();
            List<SignFlowFormLevel> _formLevels = _formLevelRepository.GetSignFlowFormLevelByFormType(formType).OrderBy(x => x.LevelID).ToList();

            List<SelectListItem> _formLevelList = new List<SelectListItem>();
            foreach (SignFlowFormLevel _formLevel in _formLevels)
            {
                _formLevelList.Add(new SelectListItem()
                {
                    Text = _formLevel.Name,
                    Value = _formLevel.LevelID,
                });
            }

            return _formLevelList;
        }

        /// <summary>
        /// 假別下拉
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAbsentList(ref Dictionary<string, string> data)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.Blank, Value = "", Selected = true });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.Value, Value = item.Key });
            }

            return listItem;
        }
        #endregion
    }
}
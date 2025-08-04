using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaSignFlowController : BaseController
    {
        #region Index
        public ActionResult Index(int page = 1, string txtPfaSignFlowCode = "", string txtPfaSignFlowName = "", string cmd = "")
        {
            GetDefaultData(txtPfaSignFlowCode, txtPfaSignFlowName);

            int currentPage = page < 1 ? 1 : page;
            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaSignFlowService>().GetPfaSignFlowData(txtPfaSignFlowCode, txtPfaSignFlowName);
            var viewModel = ds.ToPagedList(currentPage, currentPageSize);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtPfaSignFlowCode, string txtPfaSignFlowName, string btnQuery, string btnClear)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                return RedirectToAction("Index", new
                {
                    page = 1,
                    txtPfaSignFlowCode,
                    txtPfaSignFlowName,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtPfaSignFlowCode, txtPfaSignFlowName);
            return View();
        }

        private void GetDefaultData(string txtPfaSignFlowCode = "", string txtPfaSignFlowName = "")
        {
            ViewBag.txtPfaSignFlowCode = txtPfaSignFlowCode;
            ViewBag.txtPfaSignFlowName = txtPfaSignFlowName;
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            PfaSignFlowViewModel model = new PfaSignFlowViewModel();
            model.ID = Guid.Empty;
            model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
            model.DepartmentList = GetDepartmentList(CurrentUser.CompanyID.ToString(), string.Empty);
            model.SignTypeList = GetSignTypeList(string.Empty, "Create");
            model.SignLevelList = GetSignLevelList(string.Empty);
            model.DeptClassList = GetDeptClassList(string.Empty);

            return PartialView("_CreatePfaSignFlow", model);
        }

        [HttpPost]
        public ActionResult Create(PfaSignFlowViewModel obj)
        {
            if (obj.ID == null || obj.ID == Guid.Empty)
            {
                WriteLog("請選擇簽核類別");
                return Json(new { success = false, message = "請選擇簽核類別" });
            }
            else if (obj.Data.Length == 0)
            {
                WriteLog("請輸入簽核步驟");
                return Json(new { success = false, message = "請輸入簽核步驟" });
            }
            else
            {
                var isExist = Services.GetService<PfaSignFlowService>().IsExist(obj.ID);

                if (isExist)
                {
                    WriteLog(string.Format("考核簽核資料已存在,SignTypeID:{0}", obj.ID));
                    return Json(new { success = false, message = "考核簽核資料已存在" });
                }

                List<PfaSignFlow> PfaSignFlow = new List<PfaSignFlow>();

                int idx = 0;

                foreach (var item in obj.Data)
                {
                    idx += 1;
                    PfaSignFlow flow = new PfaSignFlow();
                    flow.ID = Guid.NewGuid();
                    flow.SignTypeID = obj.ID;
                    flow.SignStep = idx;

                    var signLevelID = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.OptionCode == item.SignLevelCode).Select(x => x.ID).FirstOrDefault();
                    if (signLevelID == null || signLevelID == Guid.Empty)
                    {
                        WriteLog(string.Format("請選擇簽核關卡,SignTypeID:{0}", obj.ID));
                        return Json(new { success = false, message = "請選擇簽核關卡" });
                    }
                    else
                    {
                        flow.SignLevelID = signLevelID;
                        switch (item.SignLevelCode)
                        {
                            case "DeptManager":
                                if (item.DeptClassID == null || item.DeptClassID == Guid.Empty)
                                {
                                    WriteLog(string.Format("請選擇簽核目標,SignTypeID:{0}", obj.ID));
                                    return Json(new { success = false, message = "請選擇簽核目標" });
                                }
                                else
                                {
                                    flow.DeptClassID = item.DeptClassID;
                                    flow.EmployeesID = null;
                                    flow.DepartmentsID = null;
                                }
                                break;
                            case "PsManager":
                                break;
                            case "SpEmp":
                                if (item.EmployeesID == null || item.EmployeesID == Guid.Empty)
                                {
                                    WriteLog(string.Format("請選擇簽核目標,SignTypeID:{0}", obj.ID));
                                    return Json(new { success = false, message = "請選擇簽核目標" });
                                }
                                else
                                {
                                    flow.DeptClassID = null;
                                    flow.EmployeesID = item.EmployeesID;
                                    flow.DepartmentsID = null;
                                }
                                break;
                            case "SpDept":
                                if (item.DepartmentsID == null || item.DepartmentsID == Guid.Empty)
                                {
                                    WriteLog(string.Format("請選擇簽核目標,SignTypeID:{0}", obj.ID));
                                    return Json(new { success = false, message = "請選擇簽核目標" });
                                }
                                else
                                {
                                    flow.DeptClassID = null;
                                    flow.EmployeesID = null;
                                    flow.DepartmentsID = item.DepartmentsID;
                                }
                                break;
                            default:
                                //WriteLog(string.Format("請選擇簽核關卡,SignTypeID:{0}", obj.ID));
                                //return Json(new { success = false, message = "請選擇簽核關卡" });
                                break;
                        }

                        flow.IsSelfEvaluation = item.IsSelfEvaluation;
                        flow.IsFirstEvaluation = item.IsFirstEvaluation;
                        flow.IsSecondEvaluation = item.IsSecondEvaluation;
                        flow.IsThirdEvaluation = item.IsThirdEvaluation;
                        flow.IsUpload = item.IsUpload;
                        flow.CreatedBy = CurrentUser.EmployeeID;
                        flow.CreatedTime = DateTime.Now;

                        PfaSignFlow.Add(flow);
                    }
                }

                if (PfaSignFlow.Count > 0)
                {
                    var deptManagerID = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.OptionCode == "DeptManager").Select(x => x.ID).FirstOrDefault();
                    var deptClass = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "DeptClass");

                    var deptManager = PfaSignFlow.Where(x => x.SignLevelID == deptManagerID).OrderBy(x => x.SignStep)
                        .Select(x => new
                        {
                            DeptClassID = x.DeptClassID,
                            Ordering = deptClass.Where(y => y.ID == x.DeptClassID).Select(y => y.Ordering).FirstOrDefault()
                        }).ToList();

                    int preOrdering = int.MaxValue;

                    foreach (var item in deptManager)
                    {
                        if (item.Ordering > preOrdering)
                        {
                            WriteLog("部門主管的簽核目標，層級(序號)要由大到小");
                            return Json(new { success = false, message = "部門主管的簽核目標，層級(序號)要由大到小" });
                        }
                        else
                        {
                            preOrdering = item.Ordering;
                        }
                    }

                    Result result = Services.GetService<PfaSignFlowService>().CreatePfaSignFlow(PfaSignFlow);
                    WriteLog(result.log);
                    return Json(new { success = result.success, message = result.message });
                }
                else
                {
                    WriteLog("請輸入簽核步驟");
                    return Json(new { success = false, message = "請輸入簽核步驟" });
                }
            }
        }
        #endregion

        #region Edit
        public ActionResult Edit(Guid? id)
        {
            List<PfaSignFlow> data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaSignFlowService>().GetPfaSignFlow(id);

                PfaSignFlowViewModel model = new PfaSignFlowViewModel();

                if (data == null)
                {
                    return PartialView("_EditPfaSignFlow");
                }
                else
                {
                    model.ID = id.Value;
                    model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
                    model.DepartmentList = GetDepartmentList(CurrentUser.CompanyID.ToString(), string.Empty);
                    model.SignTypeList = GetSignTypeList(id.HasValue ? id.Value.ToString() : string.Empty);
                    model.SignLevelList = GetSignLevelList(string.Empty);
                    model.DeptClassList = GetDeptClassList(string.Empty);

                    model.Data = new CreatePfaSignFlowView[data.Count];

                    Guid tmpGuid = Guid.Empty;

                    for (int i = 0; i < data.Count; i++)
                    {
                        tmpGuid = data[i].SignLevelID;

                        model.Data[i] = new CreatePfaSignFlowView();
                        model.Data[i].SignStep = data[i].SignStep;
                        model.Data[i].SignLevelID = data[i].SignLevelID;
                        model.Data[i].SignLevelCode = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.ID == tmpGuid).Select(x => x.OptionCode).FirstOrDefault();
                        model.Data[i].DeptClassID = data[i].DeptClassID;
                        model.Data[i].EmployeesID = data[i].EmployeesID;
                        model.Data[i].EmployeesNo = data[i].Employees == null ? string.Empty : data[i].Employees.EmployeeNO;
                        model.Data[i].EmployeesName = data[i].Employees == null ? string.Empty : data[i].Employees.EmployeeName;
                        model.Data[i].CompanysID = data[i].PfaDept == null ? null : (Guid?)data[i].PfaDept.CompanyID;
                        model.Data[i].DepartmentsID = data[i].DepartmentsID;
                        model.Data[i].IsSelfEvaluation = data[i].IsSelfEvaluation;
                        model.Data[i].IsFirstEvaluation = data[i].IsFirstEvaluation;
                        model.Data[i].IsSecondEvaluation = data[i].IsSecondEvaluation;
                        model.Data[i].IsThirdEvaluation = data[i].IsThirdEvaluation;
                        model.Data[i].IsUpload = data[i].IsUpload;

                        model.Data[i].CompanyList = GetCompanyList(model.Data[i].CompanysID.HasValue ? model.Data[i].CompanysID.ToString() : CurrentUser.CompanyID.ToString());
                        model.Data[i].DepartmentList = GetDepartmentList(model.Data[i].CompanysID.HasValue ? model.Data[i].CompanysID.ToString() : CurrentUser.CompanyID.ToString(), model.Data[i].DepartmentsID.HasValue ? model.Data[i].DepartmentsID.ToString() : string.Empty);
                        model.Data[i].SignLevelList = GetSignLevelList(model.Data[i].SignLevelID.ToString());
                        model.Data[i].DeptClassList = GetDeptClassList(model.Data[i].DeptClassID.HasValue ? model.Data[i].DeptClassID.ToString() : string.Empty);
                    }

                    return PartialView("_EditPfaSignFlow", model);
                }
            }
            else
            {
                return PartialView("_EditPfaSignFlow");
            }
        }

        [HttpPost]
        public ActionResult Edit(PfaSignFlowViewModel obj)
        {
            if (obj.ID == null || obj.ID == Guid.Empty)
            {
                WriteLog("請選擇簽核類別");
                return Json(new { success = false, message = "請選擇簽核類別" });
            }
            else if (obj.Data.Length == 0)
            {
                WriteLog("請輸入簽核步驟");
                return Json(new { success = false, message = "請輸入簽核步驟" });
            }
            else
            {
                List<PfaSignFlow> PfaSignFlow = new List<PfaSignFlow>();

                int idx = 0;

                foreach (var item in obj.Data)
                {
                    idx += 1;
                    PfaSignFlow flow = new PfaSignFlow();
                    flow.ID = Guid.NewGuid();
                    flow.SignTypeID = obj.ID;
                    flow.SignStep = idx;

                    var signLevelID = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.OptionCode == item.SignLevelCode).Select(x => x.ID).FirstOrDefault();

                    if (signLevelID == null || signLevelID == Guid.Empty)
                    {
                        WriteLog(string.Format("請選擇簽核關卡,SignTypeID:{0}", obj.ID));
                        return Json(new { success = false, message = "請選擇簽核關卡" });
                    }
                    else
                    {
                        flow.SignLevelID = signLevelID;

                        switch (item.SignLevelCode)
                        {
                            case "DeptManager":
                                if (item.DeptClassID == null || item.DeptClassID == Guid.Empty)
                                {
                                    WriteLog(string.Format("請選擇簽核目標,SignTypeID:{0}", obj.ID));
                                    return Json(new { success = false, message = "請選擇簽核目標" });
                                }
                                else
                                {
                                    flow.DeptClassID = item.DeptClassID;
                                    flow.EmployeesID = null;
                                    flow.DepartmentsID = null;
                                }
                                break;
                            case "PsManager":
                                break;
                            case "SpEmp":
                                if (item.EmployeesID == null || item.EmployeesID == Guid.Empty)
                                {
                                    WriteLog(string.Format("請選擇簽核目標,SignTypeID:{0}", obj.ID));
                                    return Json(new { success = false, message = "請選擇簽核目標" });
                                }
                                else
                                {
                                    flow.DeptClassID = null;
                                    flow.EmployeesID = item.EmployeesID;
                                    flow.DepartmentsID = null;
                                }
                                break;
                            case "SpDept":
                                if (item.DepartmentsID == null || item.DepartmentsID == Guid.Empty)
                                {
                                    WriteLog(string.Format("請選擇簽核目標,SignTypeID:{0}", obj.ID));
                                    return Json(new { success = false, message = "請選擇簽核目標" });
                                }
                                else
                                {
                                    flow.DeptClassID = null;
                                    flow.EmployeesID = null;
                                    flow.DepartmentsID = item.DepartmentsID;
                                }
                                break;
                            default:
                                //WriteLog(string.Format("請選擇簽核關卡,SignTypeID:{0}", obj.ID));
                                //return Json(new { success = false, message = "請選擇簽核關卡" });
                                break;
                        }

                        flow.IsSelfEvaluation = item.IsSelfEvaluation;
                        flow.IsFirstEvaluation = item.IsFirstEvaluation;
                        flow.IsSecondEvaluation = item.IsSecondEvaluation;
                        flow.IsThirdEvaluation = item.IsThirdEvaluation;
                        flow.IsUpload = item.IsUpload;
                        flow.CreatedBy = CurrentUser.EmployeeID;
                        flow.CreatedTime = DateTime.Now;

                        PfaSignFlow.Add(flow);
                    }
                }

                if (PfaSignFlow.Count > 0)
                {
                    var deptManagerID = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.OptionCode == "DeptManager").Select(x => x.ID).FirstOrDefault();
                    var deptClass = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "DeptClass");

                    var deptManager = PfaSignFlow.Where(x => x.SignLevelID == deptManagerID).OrderBy(x => x.SignStep)
                        .Select(x => new
                        {
                            DeptClassID = x.DeptClassID,
                            Ordering = deptClass.Where(y => y.ID == x.DeptClassID).Select(y => y.Ordering).FirstOrDefault()
                        }).ToList();

                    int preOrdering = int.MaxValue;

                    foreach (var item in deptManager)
                    {
                        if (item.Ordering > preOrdering)
                        {
                            WriteLog("部門主管的簽核目標，層級(序號)要由大到小");
                            return Json(new { success = false, message = "部門主管的簽核目標，層級(序號)要由大到小" });
                        }
                        else
                        {
                            preOrdering = item.Ordering;
                        }
                    }

                    Result result = Services.GetService<PfaSignFlowService>().EditPfaSignFlow(obj.ID, PfaSignFlow);
                    WriteLog(result.log);
                    return Json(new { success = result.success, message = result.message });
                }
                else
                {
                    WriteLog("請輸入簽核步驟");
                    return Json(new { success = false, message = "請輸入簽核步驟" });
                }
            }
        }
        #endregion

        #region Detail
        public ActionResult Detail(Guid? id)
        {
            List<PfaSignFlow> data = null;
            if (id.HasValue)
            {
                data = Services.GetService<PfaSignFlowService>().GetPfaSignFlow(id);
                PfaSignFlowViewModel model = new PfaSignFlowViewModel();
                if (data == null)
                {
                    return PartialView("_DetailPfaSignFlow");
                }
                else
                {
                    model.ID = id.Value;
                    model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
                    model.DepartmentList = GetDepartmentList(CurrentUser.CompanyID.ToString(), string.Empty);
                    model.SignTypeList = GetSignTypeList(id.HasValue ? id.Value.ToString() : string.Empty);
                    model.SignLevelList = GetSignLevelList(string.Empty);
                    model.DeptClassList = GetDeptClassList(string.Empty);

                    model.Data = new CreatePfaSignFlowView[data.Count];

                    Guid tmpGuid = Guid.Empty;

                    for (int i = 0; i < data.Count; i++)
                    {
                        tmpGuid = data[i].SignLevelID;

                        model.Data[i] = new CreatePfaSignFlowView();
                        model.Data[i].SignStep = data[i].SignStep;
                        model.Data[i].SignLevelID = data[i].SignLevelID;
                        model.Data[i].SignLevelCode = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.ID == tmpGuid).Select(x => x.OptionCode).FirstOrDefault();
                        model.Data[i].DeptClassID = data[i].DeptClassID;
                        model.Data[i].EmployeesID = data[i].EmployeesID;
                        model.Data[i].EmployeesNo = data[i].Employees == null ? string.Empty : data[i].Employees.EmployeeNO;
                        model.Data[i].EmployeesName = data[i].Employees == null ? string.Empty : data[i].Employees.EmployeeName;
                        model.Data[i].CompanysID = data[i].PfaDept == null ? null : (Guid?)data[i].PfaDept.CompanyID;
                        model.Data[i].DepartmentsID = data[i].DepartmentsID;
                        model.Data[i].IsSelfEvaluation = data[i].IsSelfEvaluation;
                        model.Data[i].IsFirstEvaluation = data[i].IsFirstEvaluation;
                        model.Data[i].IsSecondEvaluation = data[i].IsSecondEvaluation;
                        model.Data[i].IsThirdEvaluation = data[i].IsThirdEvaluation;
                        model.Data[i].IsUpload = data[i].IsUpload;

                        model.Data[i].CompanyList = GetCompanyList(model.Data[i].CompanysID.HasValue ? model.Data[i].CompanysID.ToString() : CurrentUser.CompanyID.ToString());
                        model.Data[i].DepartmentList = GetDepartmentList(model.Data[i].CompanysID.HasValue ? model.Data[i].CompanysID.ToString() : CurrentUser.CompanyID.ToString(), model.Data[i].DepartmentsID.HasValue ? model.Data[i].DepartmentsID.ToString() : string.Empty);
                        model.Data[i].SignLevelList = GetSignLevelList(model.Data[i].SignLevelID.ToString());
                        model.Data[i].DeptClassList = GetDeptClassList(model.Data[i].DeptClassID.HasValue ? model.Data[i].DeptClassID.ToString() : string.Empty);
                    }
                    return PartialView("_DetailPfaSignFlow", model);
                }
            }
            else
            {
                return PartialView("_DetailPfaSignFlow");
            }
        }
        #endregion

        #region Delete
        public ActionResult Delete(Guid? id)
        {
            List<PfaSignFlow> data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaSignFlowService>().GetPfaSignFlow(id);

                PfaSignFlowViewModel model = new PfaSignFlowViewModel();

                if (data == null)
                {
                    return PartialView("_DelPfaSignFlow");
                }
                else
                {
                    model.ID = id.Value;
                    model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
                    model.DepartmentList = GetDepartmentList(CurrentUser.CompanyID.ToString(), string.Empty);
                    model.SignTypeList = GetSignTypeList(id.HasValue ? id.Value.ToString() : string.Empty);
                    model.SignLevelList = GetSignLevelList(string.Empty);
                    model.DeptClassList = GetDeptClassList(string.Empty);

                    model.Data = new CreatePfaSignFlowView[data.Count];

                    Guid tmpGuid = Guid.Empty;

                    for (int i = 0; i < data.Count; i++)
                    {
                        tmpGuid = data[i].SignLevelID;

                        model.Data[i] = new CreatePfaSignFlowView();
                        model.Data[i].SignStep = data[i].SignStep;
                        model.Data[i].SignLevelID = data[i].SignLevelID;
                        model.Data[i].SignLevelCode = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel" && x.ID == tmpGuid).Select(x => x.OptionCode).FirstOrDefault();
                        model.Data[i].DeptClassID = data[i].DeptClassID;
                        model.Data[i].EmployeesID = data[i].EmployeesID;
                        model.Data[i].EmployeesNo = data[i].Employees == null ? string.Empty : data[i].Employees.EmployeeNO;
                        model.Data[i].EmployeesName = data[i].Employees == null ? string.Empty : data[i].Employees.EmployeeName;
                        model.Data[i].CompanysID = data[i].PfaDept == null ? null : (Guid?)data[i].PfaDept.CompanyID;
                        model.Data[i].DepartmentsID = data[i].DepartmentsID;
                        model.Data[i].IsSelfEvaluation = data[i].IsSelfEvaluation;
                        model.Data[i].IsFirstEvaluation = data[i].IsFirstEvaluation;
                        model.Data[i].IsSecondEvaluation = data[i].IsSecondEvaluation;
                        model.Data[i].IsThirdEvaluation = data[i].IsThirdEvaluation;
                        model.Data[i].IsUpload = data[i].IsUpload;

                        model.Data[i].CompanyList = GetCompanyList(model.Data[i].CompanysID.HasValue ? model.Data[i].CompanysID.ToString() : CurrentUser.CompanyID.ToString());
                        model.Data[i].DepartmentList = GetDepartmentList(model.Data[i].CompanysID.HasValue ? model.Data[i].CompanysID.ToString() : CurrentUser.CompanyID.ToString(), model.Data[i].DepartmentsID.HasValue ? model.Data[i].DepartmentsID.ToString() : string.Empty);
                        model.Data[i].SignLevelList = GetSignLevelList(model.Data[i].SignLevelID.ToString());
                        model.Data[i].DeptClassList = GetDeptClassList(model.Data[i].DeptClassID.HasValue ? model.Data[i].DeptClassID.ToString() : string.Empty);
                    }

                    return PartialView("_DeletePfaSignFlow", model);
                }
            }
            else
            {
                return PartialView("_DeletePfaSignFlow");
            }
        }

        [HttpPost]
        public ActionResult DeletePfaSignFlow(Guid? id)
        {
            Result result = Services.GetService<PfaSignFlowService>().DeletePfaSignFlow(id.Value);

            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 共用
        private List<SelectListItem> GetCompanyList(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaCompany> data = Services.GetService<CompanyService>().GetAllSort().ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.CompanyName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            return listItem;
        }

        private List<SelectListItem> GetDepartmentList(string companyID, string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            Guid CompanyID = Guid.Parse(companyID);

            List<PfaDept> data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CompanyID).OrderBy(x => x.PfaDeptCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.PfaDeptCode, item.PfaDeptName), Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }
            return listItem;
        }

        private List<SelectListItem> GetSignTypeList(string selecteddata, string cmd = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            var all = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignType");

            if (cmd == "Create")
            {
                var signTypeIDList = Services.GetService<PfaSignFlowService>().GetAll().GroupBy(x => x.SignTypeID).Select(x => x.Key).ToList();
                all = all.Where(x => !signTypeIDList.Contains(x.ID));
            }

            List<PfaOption> data = all.OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.OptionName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }

            return listItem;
        }

        private List<SelectListItem> GetSignLevelList(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaOption> data = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "SignLevel").OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.OptionName, Value = item.OptionCode, Selected = (SelectedDataID == item.ID ? true : false) });
            }

            return listItem;
        }

        private List<SelectListItem> GetDeptClassList(string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaOption> data = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "DeptClass").OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.OptionName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            }

            return listItem;
        }
        #endregion

        #region Emp
        public ActionResult Emp()
        {
            ViewBag.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
            ViewBag.DeptList = GetDepartmentList(CurrentUser.CompanyID.ToString(), string.Empty);
            return PartialView("_EmpDialog", null);
        }

        [HttpPost]
        public ActionResult Emp(string ddlCompany, string ddlDept = "", string txtEmpNo = "", int page = 1)
        {
            int pageSize = 10;
            int currentPage = page < 1 ? 1 : page;

            Guid CompanyID = Guid.Parse(ddlCompany);

            var emp = Services.GetService<PfaDeptEmpService>().GetAll().Where(x => x.PfaDept.CompanyID == CompanyID);

            if (!string.IsNullOrEmpty(ddlDept))
            {
                Guid DeptID = Guid.Parse(ddlDept);
                emp = emp.Where(x => x.PfaDeptID == DeptID);
            }

            if (!string.IsNullOrEmpty(txtEmpNo))
            {
                emp = emp.Where(x => x.Employees.EmployeeNO.Contains(txtEmpNo));
            }

            var data = emp.Select(x => new EmpData()
            {
                EmployeeID = x.EmployeeID,
                EmployeeNo = x.Employees.EmployeeNO,
                EmployeeName = x.Employees.EmployeeName,
                DepartmentCode = x.PfaDept.PfaDeptCode,
                DepartmentName = x.PfaDept.PfaDeptName
            }).OrderBy(x => x.DepartmentCode).ThenBy(x => x.EmployeeNo).ToPagedList(page, pageSize);

            EmployeeViewModel result = new EmployeeViewModel();
            result.Data = data.ToList();
            result.PageCount = data.PageCount;
            result.DataCount = data.TotalItemCount;

            #region Create Data
            StringBuilder dataResult = new StringBuilder();

            foreach (var item in result.Data)
            {
                dataResult.Append(string.Format("<tr id='resultDataTr' name='{0}' role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' style='cursor:pointer'>", item.EmployeeID));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.DepartmentName));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.EmployeeNo));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.EmployeeName));
                dataResult.Append("</tr>");
            }
            #endregion

            #region Create Page
            Dictionary<int, string> pageObj = new Dictionary<int, string>();

            int sIdx = (currentPage - 1) * currentPageSize + 1;
            int eIdx = currentPage * currentPageSize;
            if (eIdx > result.DataCount) eIdx = result.DataCount;

            int sPidx = currentPage - 2;
            if (sPidx < 1) sPidx = 1;

            int ePidx = 4 + sPidx;
            if (ePidx > result.PageCount)
            {
                ePidx = result.PageCount;
                sPidx = ePidx - 4;
                if (sPidx < 1) sPidx = 1;
            }

            for (int i = sPidx; i < currentPage; i++)
            {
                pageObj.Add(i, " style='cursor:pointer'");
            }

            pageObj.Add(currentPage, " class='active'");

            for (int i = (currentPage + 1); i <= ePidx; i++)
            {
                pageObj.Add(i, " style='cursor:pointer'");
            }

            StringBuilder pageResult = new StringBuilder();

            pageResult.Append("<div style='text-align:center'>");
            pageResult.Append("<div class='pagination-container'>");
            pageResult.Append("<ul class='pagination'>");
            pageResult.Append(string.Format("<li id='first' name='pagelist' class='{0}PagedList-skipToFirst'{1}><a>««</a></li>", (currentPage == 1 ? "disabled " : ""), (currentPage == 1 ? "" : " style='cursor:pointer'")));
            pageResult.Append(string.Format("<li id='prev' name='pagelist' class='{0}PagedList-skipToPrevious'{1}><a>«</a></li>", (currentPage == 1 ? "disabled " : ""), (currentPage == 1 ? "" : " style='cursor:pointer'")));

            if (sPidx != 1)
            {
                pageResult.Append("<li class='disabled PagedList-ellipses'><a>…</a></li>");
            }

            foreach (KeyValuePair<int, string> item in pageObj)
            {
                pageResult.Append(string.Format("<li id='page' name='pagelist'{0}><a>{1}</a></li>", item.Value, item.Key));
            }

            if (ePidx != result.PageCount)
            {
                pageResult.Append("<li class='disabled PagedList-ellipses'><a>…</a></li>");
            }

            pageResult.Append(string.Format("<li id='next' name='pagelist' class='{0}PagedList-skipToNext'{1}><a>»</a></li>", (currentPage == result.PageCount ? "disabled " : ""), (currentPage == result.PageCount ? "" : " style='cursor:pointer'")));
            pageResult.Append(string.Format("<li id='last' name='pagelist' class='{0}PagedList-skipToLast'{1}><a>»»</a>", (currentPage == result.PageCount ? "disabled " : ""), (currentPage == result.PageCount ? "" : " style='cursor:pointer'")));
            pageResult.Append("</ul");
            pageResult.Append("</div>");
            pageResult.Append("<div class='pagination-container'>");
            pageResult.Append("<ul class='pagination'>");
            pageResult.Append(string.Format("<li class='disabled PagedList-pageCountAndLocation'><a>第{0}頁/共{1}頁</a></li>", currentPage, result.PageCount));
            pageResult.Append(string.Format("<li class='disabled PagedList-pageCountAndLocation'><a>{0}-{1} 共{2}列</a></li>", sIdx, eIdx, result.DataCount));
            pageResult.Append("</ul>");
            pageResult.Append("</div>");
            pageResult.Append("</div>");
            #endregion

            return Json(new { data = dataResult.ToString(), page = pageResult.ToString(), pagecount = result.PageCount });
        }
        #endregion

        #region SelPfaTargets
        public ActionResult SelPfaTargets(Guid? id)
        {
            SelectPfaTargets model = Services.GetService<PfaSignFlowService>().GetAll().Where(x => x.SignTypeID == id).Select(x => new SelectPfaTargets()
            {
                SignTypeID = x.SignTypeID
            }).FirstOrDefault();

            if (model == null)
            {
                model = new SelectPfaTargets();
                model.SignTypeID = Guid.Empty;
            }
            else
            {
                List<Guid> JobTitleID = Services.GetService<PfaTargetsService>().GetJobTitleID(model.SignTypeID);
                model.JobTitleItems = Services.GetService<PfaOptionService>().GetAll().Where(x => x.PfaOptionGroup.OptionGroupCode == "JobTitle").OrderBy(x => x.Ordering).ThenBy(x => x.OptionCode).Select(x => new JobTitleItem()
                {
                    ID = x.ID,
                    Code = x.OptionCode,
                    Name = x.OptionName,
                    Chk = JobTitleID.Contains(x.ID)
                }).ToList();
            }
            return PartialView("_SelPfaTargets", model);
        }

        [HttpPost]
        public ActionResult SelPfaTargets(Guid SignTypeID, string SelJboTitleID)
        {
            Result result = null;

            var data = Services.GetService<PfaSignFlowService>().GetAll().Where(x => x.SignTypeID == SignTypeID);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此身分類別組織";
                result.log = "查無此身分類別組織";
            }
            else
            {
                result = Services.GetService<PfaTargetsService>().SaveJobTitle(SignTypeID, SelJboTitleID, CurrentUser.EmployeeID);
            }
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion
    }
}
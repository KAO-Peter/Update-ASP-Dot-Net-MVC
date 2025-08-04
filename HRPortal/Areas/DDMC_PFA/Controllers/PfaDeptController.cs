using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Mvc.DDMC_PFA.Controllers;
using HRPortal.Mvc.DDMC_PFA.Models;
using HRPortal.Services.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC_PFA.Controllers
{
    public class PfaDeptController : BaseController
    {
        #region Index
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDepartmentID"></param>
        /// <param name="txtDeptClassID"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtDepartmentID = "", string txtDeptClassID = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtDepartmentID, txtDeptClassID);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaDeptService>().GetPfaDeptData(string.IsNullOrWhiteSpace(txtCompanyID) ? Guid.Empty : Guid.Parse(txtCompanyID), string.IsNullOrWhiteSpace(txtDepartmentID) ? Guid.Empty : Guid.Parse(txtDepartmentID), string.IsNullOrWhiteSpace(txtDeptClassID) ? Guid.Empty : Guid.Parse(txtDeptClassID));

            var viewModel = ds.Select(x => new PfaDeptViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                CompanyName = x.Companys.CompanyName,
                PfaDeptCode = x.PfaDeptCode,
                PfaDeptName = x.PfaDeptName,
                ManagerID = x.ManagerID,
                ParentDepartmentID = x.ParentDepartmentID,
                SignParentID = x.SignParentID,
                SignManagerID = x.SignManagerID,
                DeptClassID = x.DeptClassID,
                OnlyForSign = x.OnlyForSign
            }).ToPagedList(currentPage, currentPageSize);

            foreach (var item in viewModel)
            {
                item.ParentDepartmentName = Services.GetService<PfaDeptService>().GetAll().Where(y => y.ID == item.ParentDepartmentID).Select(y => y.PfaDeptName).FirstOrDefault();
                item.ManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.ManagerID).Select(y => y.EmployeeName).FirstOrDefault();
                item.SignParentName = Services.GetService<PfaDeptService>().GetAll().Where(y => y.ID == item.SignParentID).Select(y => y.PfaDeptName).FirstOrDefault();
                item.SignManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.SignManagerID).Select(y => y.EmployeeName).FirstOrDefault();
                item.DeptClassName = Services.GetService<PfaOptionService>().GetAll().Where(y => y.ID == item.DeptClassID).Select(y => y.OptionName).FirstOrDefault();
            }

            return View(viewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        /// <param name="btnQuery"></param>
        /// <param name="btnClear"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtCompanyID, string txtDepartmentID, string txtDeptClassID, string btnQuery, string btnClear)
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
                    txtCompanyID,
                    txtDepartmentID,
                    txtDeptClassID,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCompanyID, txtDepartmentID, txtDeptClassID);

            return View();
        }

        /// <summary>
        /// 取得預設值
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        private void GetDefaultData(string txtCompanyID = "", string txtDepartmentID = "", string txtDeptClassID = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }

            ViewBag.CompanyList = GetCompanyList(txtCompanyID);
            ViewBag.DepartmentList = GetDepartmentList(txtDepartmentID);
            ViewBag.DeptClassList = GetDeptClassList(txtDeptClassID);
            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtDepartmentID = txtDepartmentID;
            ViewBag.txtDeptClassID = txtDeptClassID;
        }
        #endregion

        #region Export
        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtCompanyID"></param>
        /// <param name="txtDeptName"></param>
        /// <param name="txtDeptClassID"></param>
        /// <returns></returns>
        public ActionResult Export(string txtCompanyID = "", string txtDepartmentID = "", string txtDeptClassID = "")
        {
            var ds = Services.GetService<PfaDeptService>().GetPfaDeptData(string.IsNullOrWhiteSpace(txtCompanyID) ? Guid.Empty : Guid.Parse(txtCompanyID), string.IsNullOrWhiteSpace(txtDepartmentID) ? Guid.Empty : Guid.Parse(txtDepartmentID), string.IsNullOrWhiteSpace(txtDeptClassID) ? Guid.Empty : Guid.Parse(txtDeptClassID));

            var viewModel = ds.Select(x => new PfaDeptViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                CompanyName = x.Companys.CompanyName,
                PfaDeptCode = x.PfaDeptCode,
                PfaDeptName = x.PfaDeptName,
                ManagerID = x.ManagerID,
                ParentDepartmentID = x.ParentDepartmentID,
                SignParentID = x.SignParentID,
                SignManagerID = x.SignManagerID,
                DeptClassID = x.DeptClassID,
                OnlyForSign = x.OnlyForSign
            }).ToList();

            foreach (var item in viewModel)
            {
                item.ParentDepartmentName = Services.GetService<PfaDeptService>().GetAll().Where(y => y.ID == item.ParentDepartmentID).Select(y => y.PfaDeptName).FirstOrDefault();
                item.ManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.ManagerID).Select(y => y.EmployeeName).FirstOrDefault();
                item.ManagerNo = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.ManagerID).Select(y => y.EmployeeNO).FirstOrDefault();
                item.SignParentName = Services.GetService<PfaDeptService>().GetAll().Where(y => y.ID == item.SignParentID).Select(y => y.PfaDeptName).FirstOrDefault();
                item.SignManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.SignManagerID).Select(y => y.EmployeeName).FirstOrDefault();
                item.SignManagerNo = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.SignManagerID).Select(y => y.EmployeeNO).FirstOrDefault();
                item.DeptClassName = Services.GetService<PfaOptionService>().GetAll().Where(y => y.ID == item.DeptClassID).Select(y => y.OptionName).FirstOrDefault();
            }

            IWorkbook wb = new XSSFWorkbook();
            ISheet ws = wb.CreateSheet("PfaDept");

            //設定欄位名稱
            List<string> colName = new List<string>() { "公司別", "考核部門代碼", "考核部門名稱", "HRM部門主管員編", "HRM部門主管姓名", "HRM上層部門", "考核上層部門", "考核簽核主管員編", "考核簽核主管姓名", "部門階級" };

            //設定表頭
            ws.CreateRow(0);

            for (int i = 0; i < colName.Count; i++)
            {
                ws.GetRow(0).CreateCell(i).SetCellValue(colName[i]);
            }

            int count = 1;

            foreach (var item in viewModel)
            {
                ws.CreateRow(count);

                ws.GetRow(count).CreateCell(0).SetCellValue(item.CompanyName);
                ws.GetRow(count).CreateCell(1).SetCellValue(item.PfaDeptCode);
                ws.GetRow(count).CreateCell(2).SetCellValue(item.PfaDeptName);
                ws.GetRow(count).CreateCell(3).SetCellValue(item.ManagerNo);
                ws.GetRow(count).CreateCell(4).SetCellValue(item.ManagerName);
                ws.GetRow(count).CreateCell(5).SetCellValue(item.ParentDepartmentName);
                ws.GetRow(count).CreateCell(6).SetCellValue(item.SignParentName);
                ws.GetRow(count).CreateCell(7).SetCellValue(item.SignManagerNo);
                ws.GetRow(count).CreateCell(8).SetCellValue(item.SignManagerName);
                ws.GetRow(count).CreateCell(9).SetCellValue(item.DeptClassName);

                count++;
            }

            #region Autofit
            for (int i = 0; i < colName.Count; i++)
            {
                ws.AutoSizeColumn(i);
            }
            #endregion

            string fileName = string.Format("PfaDept_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));

            using (MemoryStream ms = new MemoryStream())
            {
                wb.Write(ms);
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        #endregion

        #region Detail
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(Guid? id)
        {
            PfaDept data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaDeptService>().GetPfaDept(id);
            }

            CreatePfaDept model = new CreatePfaDept();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.BeginDate = DateTime.Now;
                model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
                model.SignParentList = GetSignParentList(CurrentUser.CompanyID.ToString(), string.Empty);
                model.DeptClassList = GetDeptClassList(string.Empty);
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaDeptCode = data.PfaDeptCode;
                model.PfaDeptName = data.PfaDeptName;
                model.SignParentID = data.SignParentID;
                model.SignManagerID = data.SignManagerID;
                model.SignManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == model.SignManagerID).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault();
                model.DeptClassID = data.DeptClassID;
                model.BeginDate = data.BeginDate;
                model.EndDate = data.EndDate;
                model.CompanyList = GetCompanyList(model.CompanyID.ToString());
                model.SignParentList = GetSignParentList(model.CompanyID.ToString(), model.SignParentID.HasValue ? model.SignParentID.ToString() : "");
                model.DeptClassList = GetDeptClassList(model.DeptClassID.ToString());
            }

            return PartialView("_DetailPfaDept", model);
        }
        #endregion

        #region Create
        /// <summary>
        /// Create
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            CreatePfaDept model = new CreatePfaDept();
            model.ID = Guid.Empty;
            model.BeginDate = DateTime.Now;
            model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
            model.SignParentList = GetSignParentList(CurrentUser.CompanyID.ToString(), string.Empty);
            model.DeptClassList = GetDeptClassList(string.Empty);
            return PartialView("_CreatePfaDept", model);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CreatePfaDept model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var isExist = Services.GetService<PfaDeptService>().IsExist(model.PfaDeptCode);

                if (isExist)
                {
                    WriteLog(string.Format("考核部門資料已存在,Code:{0}", model.PfaDeptCode));
                    return Json(new { success = false, message = "考核部門資料已存在" });
                }

                if (model.EndDate.HasValue)
                {
                    if (model.EndDate.Value < model.BeginDate)
                    {
                        WriteLog("有效迄日需大於等於有效起日");
                        return Json(new { success = false, message = "有效迄日需大於等於有效起日" });
                    }
                }
                else
                {
                    model.EndDate = new DateTime(2099, 12, 31);
                }

                if (model.SignParentID.HasValue)
                {
                    var signParent = Services.GetService<PfaDeptService>().GetAll().Where(x => x.ID == model.SignParentID.Value).FirstOrDefault();
                    var deptClassParent = Services.GetService<PfaOptionService>().GetAll().Where(x => x.ID == signParent.DeptClassID).FirstOrDefault();
                    var deptClassCurrent = Services.GetService<PfaOptionService>().GetAll().Where(x => x.ID == model.DeptClassID).FirstOrDefault();

                    if (deptClassParent.Ordering > deptClassCurrent.Ordering)
                    {
                        WriteLog("部門階級(序號)要比簽核上層部門的階級(序號)大");
                        return Json(new { success = false, message = "部門階級(序號)要比簽核上層部門的階級(序號)大" });
                    }
                }

                PfaDept data = new PfaDept();
                data.ID = Guid.NewGuid();
                data.PfaDeptCode = model.PfaDeptCode;
                data.PfaDeptName = model.PfaDeptName;
                data.CompanyID = model.CompanyID;
                data.SignParentID = model.SignParentID;
                data.SignManagerID = model.SignManagerID;
                data.DeptClassID = model.DeptClassID;
                data.BeginDate = model.BeginDate;
                data.EndDate = model.EndDate;
                data.OnlyForSign = true;
                data.CreatedBy = CurrentUser.EmployeeID;
                data.CreatedTime = DateTime.Now;

                int IsSuccess = Services.GetService<PfaDeptService>().Create(data);
                if (IsSuccess == 1)
                {
                    WriteLog(string.Format("新增成功,ID:{0}", data.ID));
                    return Json(new { success = true, message = "新增成功" });
                }
            }
            return Json(new { success = true, message = "新增成功" });
        }
        #endregion

        #region Edit
        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(Guid? id)
        {
            PfaDept data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaDeptService>().GetPfaDept(id);
            }

            CreatePfaDept model = new CreatePfaDept();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.BeginDate = DateTime.Now;
                model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
                model.SignParentList = GetSignParentList(CurrentUser.CompanyID.ToString(), string.Empty);
                model.DeptClassList = GetDeptClassList(string.Empty);
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaDeptCode = data.PfaDeptCode;
                model.PfaDeptName = data.PfaDeptName;
                model.SignParentID = data.SignParentID;
                model.SignManagerID = data.SignManagerID;
                model.SignManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == model.SignManagerID).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault();
                model.DeptClassID = data.DeptClassID;
                model.BeginDate = data.BeginDate;
                model.EndDate = data.EndDate;
                model.CompanyList = GetCompanyList(model.CompanyID.ToString());
                model.SignParentList = GetSignParentList(model.CompanyID.ToString(), model.SignParentID.HasValue ? model.SignParentID.ToString() : "");
                model.DeptClassList = GetDeptClassList(model.DeptClassID.ToString());
            }

            return PartialView("_EditPfaDept", model);
        }

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(CreatePfaDept model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var data = Services.GetService<PfaDeptService>().GetPfaDept(model.ID);

                if (data == null)
                {
                    WriteLog(string.Format("查無此考核代碼資料,ID:{0}", model.ID));
                    return Json(new { success = false, message = "查無此考核代碼資料" });
                }
                else
                {
                    var childs = Services.GetService<PfaDeptService>().GetAll().Where(x => x.SignParentID == model.ID).ToList();
                    var nowDeptClass = Services.GetService<PfaOptionService>().GetPfaOption(model.DeptClassID);

                    foreach (var item in childs)
                    {
                        var cidDeptClass = Services.GetService<PfaOptionService>().GetPfaOption(item.DeptClassID);

                        if (cidDeptClass == null) continue;

                        if (nowDeptClass.Ordering > cidDeptClass.Ordering)
                        {
                            WriteLog("該筆資料的部門階級不能比下層部門大");
                            return Json(new { success = false, message = "該筆資料的部門階級不能比下層部門大" });
                        }
                    }

                    if (model.EndDate.HasValue)
                    {
                        if (model.EndDate.Value < model.BeginDate)
                        {
                            WriteLog("有效迄日需大於等於有效起日");
                            return Json(new { success = false, message = "有效迄日需大於等於有效起日" });
                        }
                    }
                    else
                    {
                        model.EndDate = new DateTime(2099, 12, 31);
                    }

                    if (model.SignParentID.HasValue)
                    {
                        var signParent = Services.GetService<PfaDeptService>().GetAll().Where(x => x.ID == model.SignParentID.Value).FirstOrDefault();
                        var deptClassParent = Services.GetService<PfaOptionService>().GetAll().Where(x => x.ID == signParent.DeptClassID).FirstOrDefault();
                        var deptClassCurrent = Services.GetService<PfaOptionService>().GetAll().Where(x => x.ID == model.DeptClassID).FirstOrDefault();

                        if (deptClassParent != null && deptClassParent.Ordering > deptClassCurrent.Ordering)
                        {
                            WriteLog("部門階級(序號)要比簽核上層部門的階級(序號)大");
                            return Json(new { success = false, message = "部門階級(序號)要比簽核上層部門的階級(序號)大" });
                        }
                    }

                    data.PfaDeptName = model.PfaDeptName;
                    data.CompanyID = model.CompanyID;
                    data.SignParentID = model.SignParentID;
                    data.SignManagerID = model.SignManagerID;
                    data.DeptClassID = model.DeptClassID;
                    data.BeginDate = model.BeginDate;
                    data.EndDate = model.EndDate;
                    data.ModifiedBy = CurrentUser.EmployeeID;
                    data.ModifiedTime = DateTime.Now;

                    int IsSuccess = Services.GetService<PfaDeptService>().Update(data);
                    if (IsSuccess == 1)
                    {
                        WriteLog(string.Format("編輯成功,ID:{0}", model.ID));
                        return Json(new { success = true, message = "編輯成功" });
                    }
                }
            }
            return Json(new { success = true, message = "編輯成功" });
        }
        #endregion

        #region Del
        /// <summary>
        /// Del
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Del(Guid? id)
        {
            PfaDept data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaDeptService>().GetPfaDept(id);
            }

            CreatePfaDept model = new CreatePfaDept();
            if (data == null)
            {
                model.ID = Guid.Empty;
                model.BeginDate = DateTime.Now;
                model.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
                model.SignParentList = GetSignParentList(CurrentUser.CompanyID.ToString(), string.Empty);
                model.DeptClassList = GetDeptClassList(string.Empty);
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaDeptCode = data.PfaDeptCode;
                model.PfaDeptName = data.PfaDeptName;
                model.ParentDepartmentID = data.ParentDepartmentID;
                model.SignParentID = data.SignParentID;
                model.ManagerID = data.ManagerID;
                model.SignManagerID = data.SignManagerID;
                model.SignManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == model.SignManagerID).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault();
                model.DeptClassID = data.DeptClassID;
                model.BeginDate = data.BeginDate;
                model.EndDate = data.EndDate;
                model.CompanyList = GetCompanyList(model.CompanyID.ToString());
                model.SignParentList = GetSignParentList(model.CompanyID.ToString(), model.SignParentID.HasValue ? model.SignParentID.ToString() : "");
                model.DeptClassList = GetDeptClassList(model.DeptClassID.ToString());
            }
            return PartialView("_DelPfaDept", model);
        }

        /// <summary>
        /// DelPfaDept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelPfaDept(Guid? id)
        {
            Result result = null;

            PfaDept data = Services.GetService<PfaDeptService>().GetPfaDept(id);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此考核部門資料";
                result.log = "查無此考核部門資料";
            }
            else if (!data.OnlyForSign)
            {
                result = new Result();
                result.success = false;
                result.message = "此考核部門來源為HRM，所以無法刪除";
                result.log = "此考核部門來源為HRM，所以無法刪除";
            }
            else
            {
                result = Services.GetService<PfaDeptService>().DelPfaDept(id.Value);
            }
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region SelEmp
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult SelEmp(Guid? id, int page = 1, int pagesize = 10)
        {
            PfaDeptViewModel data = null;

            if (id.HasValue)
            {
                ViewBag.ID = id;

                data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.ID == id).ToList()
                               .Select(x => new PfaDeptViewModel()
                               {
                                   ID = x.ID,
                                   CompanyID = x.CompanyID,
                                   CompanyName = x.Companys.CompanyName,
                                   PfaDeptCode = x.PfaDeptCode,
                                   PfaDeptName = x.PfaDeptName,
                                   DeptClassID = x.DeptClassID,
                                   DeptClassName = Services.GetService<PfaOptionService>().GetAll().Where(y => y.ID == x.DeptClassID).Select(y => y.OptionName).FirstOrDefault(),
                                   OnlyForSign = x.OnlyForSign
                               }).FirstOrDefault();
            }

            if (data != null)
            {
                data.SelEmps = GetPfaDeptEmpData(data.ID).ToPagedList(page, pagesize);
                return PartialView("_SelEmp", data);
            }
            else
            {
                return PartialView("_SelEmp");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PfaDeptID"></param>
        /// <returns></returns>
        private List<SelEmp> GetPfaDeptEmpData(Guid PfaDeptID)
        {
            var data = Services.GetService<PfaDeptEmpService>().GetAll();

            if (PfaDeptID != Guid.Empty)
                data = data.Where(x => x.PfaDeptID == PfaDeptID);

            var dataList = data.Select(x => new SelEmp()
            {
                EmployeeID = x.EmployeeID,
                EmployeeNO = x.Employees.EmployeeNO,
                EmployeeName = x.Employees.EmployeeName,
                LeaveDate = x.Employees.LeaveDate
            }).OrderBy(x => x.EmployeeNO).ToList();

            foreach (var temp in dataList)
            {
                if (temp.LeaveDate.HasValue)
                    temp.EmployeeName = temp.EmployeeName + "(離職)";
            }
            return dataList;
        }
        #endregion

        #region Emp
        /// <summary>
        /// Emp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Emp(Guid id)
        {
            ViewData["DeptList"] = GetDeptList(id);
            return PartialView("_EmpDialog", null);
        }

        /// <summary>
        /// Emp
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="page"></param>
        /// <param name="ddlDept"></param>
        /// <param name="txtEmpNo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Emp(Guid companyID, int page = 1, string ddlDept = "", string txtEmpNo = "")
        {
            int pageSize = 10;
            int currentPage = page < 1 ? 1 : page;

            var emp = Services.GetService<PfaDeptEmpService>().GetAll().Where(x => x.PfaDept.CompanyID == companyID);

            if (!string.IsNullOrEmpty(ddlDept))
            {
                Guid SelectedDeptID = Guid.Parse(ddlDept);
                emp = emp.Where(x => x.PfaDeptID == SelectedDeptID);
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
                DepartmentName = x.PfaDept.PfaDeptName,
                LeaveDate = x.Employees.LeaveDate
            }).OrderBy(x => x.DepartmentCode).ThenBy(x => x.EmployeeNo).ToPagedList(page, pageSize);

            EmployeeViewModel result = new EmployeeViewModel();
            result.Data = data.ToList();
            result.PageCount = data.PageCount;
            result.DataCount = data.TotalItemCount;

            #region Create Data
            StringBuilder dataResult = new StringBuilder();

            foreach (var item in result.Data)
            {
                var empName = item.EmployeeName;
                if (item.LeaveDate.HasValue)
                    empName = empName + "(離職)";
                dataResult.Append(string.Format("<tr id='resultDataTr' name='{0}' role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' style='cursor:pointer'>", item.EmployeeID));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.DepartmentName));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.EmployeeNo));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", empName));
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

        #region AddEmp
        /// <summary>
        /// AddEmp
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public ActionResult AddEmp(Guid aid)
        {
            PfaDept data = Services.GetService<PfaDeptService>().GetPfaDept(aid);

            if (data != null)
            {
                ViewBag.PfaDeptID = aid;
                ViewBag.CompanyID = data.CompanyID;
                ViewBag.CompanyName = data.Companys.CompanyName;

                ViewData["DeptList"] = GetDeptList(data.CompanyID);
            }

            return PartialView("_AddEmp", null);
        }

        /// <summary>
        /// AddEmp
        /// </summary>
        /// <param name="txtPfaDeptID"></param>
        /// <param name="txtCompanyID"></param>
        /// <param name="ddlDept"></param>
        /// <param name="txtEmpNo"></param>
        /// <param name="txtEmpName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddEmp(Guid txtPfaDeptID, Guid txtCompanyID, string ddlDept = "", string txtEmpNo = "", string txtEmpName = "")
        {
            #region 取得Pfa員工資料
            var emp = Services.GetService<PfaDeptEmpService>().GetAll().Where(x => x.PfaDept.CompanyID == txtCompanyID);

            if (!string.IsNullOrEmpty(ddlDept))
            {
                Guid SelectedDeptID = Guid.Parse(ddlDept);
                emp = emp.Where(x => x.PfaDeptID == SelectedDeptID);
            }

            if (!string.IsNullOrEmpty(txtEmpNo))
            {
                emp = emp.Where(x => x.Employees.EmployeeNO.Contains(txtEmpNo));
            }

            if (!string.IsNullOrEmpty(txtEmpName))
            {
                emp = emp.Where(x => x.Employees.EmployeeName.Contains(txtEmpName));
            }

            var data = emp.Select(x => new
            {
                Chk = false,
                EmpID = x.EmployeeID,
                EmpNo = x.Employees.EmployeeNO,
                EmpName = x.Employees.EmployeeName,
                DeptName = x.PfaDept.PfaDeptName,
                DeptCode = x.PfaDept.PfaDeptCode,
                LeaveDate = x.Employees.LeaveDate,
            }).ToList();
            #endregion

            #region 取得虛擬員工名單
            var virtualEmp = Services.GetService<EmployeeService>().GetAll().Where(x => x.CompanyID == txtCompanyID && x.EmployeeType == "1" && !x.PfaDeptEmp.Any());

            if (!string.IsNullOrEmpty(ddlDept))
            {
                Guid SelectedDeptID = Guid.Parse(ddlDept);

                var PfaDept = Services.GetService<PfaDeptService>().GetAll().FirstOrDefault(x => x.ID == SelectedDeptID);
                // 代碼對代碼
                if (PfaDept != null)
                    virtualEmp = virtualEmp.Where(x => x.Department.DepartmentCode == PfaDept.PfaDeptCode);
            }

            if (!string.IsNullOrEmpty(txtEmpNo))
            {
                virtualEmp = virtualEmp.Where(x => x.EmployeeNO.Contains(txtEmpNo));
            }

            if (!string.IsNullOrEmpty(txtEmpName))
            {
                virtualEmp = virtualEmp.Where(x => x.EmployeeName.Contains(txtEmpName));
            }
            #endregion

            #region 加入虛擬員工名單
            foreach (var item in virtualEmp)
            {
                var PfaDept = Services.GetService<PfaDeptService>().GetAll().FirstOrDefault(x => x.PfaDeptCode == item.Department.DepartmentCode);

                if (PfaDept != null)
                {
                    data.Add(new
                    {
                        Chk = false,
                        EmpID = item.ID,
                        EmpNo = item.EmployeeNO,
                        EmpName = item.EmployeeName,
                        DeptName = PfaDept.PfaDeptName,
                        DeptCode = PfaDept.PfaDeptCode,
                        LeaveDate = item.LeaveDate,
                    });
                }
            }
            #endregion

            // 排序
            data = data.OrderBy(x => x.DeptCode).ThenBy(x => x.EmpNo).ToList();

            #region Create Data
            StringBuilder dataResult = new StringBuilder();

            foreach (var item in data)
            {
                var empName = item.EmpName;
                if (item.LeaveDate.HasValue)
                    empName = empName + "(離職)";
                dataResult.Append("<tr role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' style='cursor:pointer'>");
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'><input type='checkbox' name='ChkEmp' value='{0}' {1}/></td>", item.EmpID, item.Chk ? "checked='true'" : ""));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.DeptName));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", item.EmpNo));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal;text-align:center;' aria-describedby='grid-table_notification_count'>{0}</td>", empName));
                dataResult.Append("</tr>");
            }
            #endregion

            return Json(new { data = dataResult.ToString() });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txtPfaDeptID"></param>
        /// <param name="selEmps"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SavePfaDeptEmp(Guid txtPfaDeptID, string selEmps)
        {
            Result result = null;

            var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.ID == txtPfaDeptID);

            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此考核部門資料";
                result.log = "查無此考核部門資料";
            }
            else
            {
                result = Services.GetService<PfaDeptEmpService>().SavePfaDeptEmp(txtPfaDeptID, selEmps);
            }

            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }
        #endregion

        #region 共用
        /// <summary>
        /// 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
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
                listItem.Add(new SelectListItem { Text = item.CompanyName, Value = item.ID.ToString(), Selected = SelectedDataID == item.ID ? true : false });
            }

            return listItem;
        }

        private List<SelectListItem> GetDepartmentList(string selectedData)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.PfaDeptCode, item.PfaDeptName), Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });

            return listItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyid"></param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSignParentList(string companyid, string selecteddata)
        {
            Guid? cid = null;

            if (!string.IsNullOrEmpty(companyid))
            {
                cid = Guid.Parse(companyid);
            }

            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaDept> data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == cid.Value && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDeptList(Guid id, bool flag = true)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<PfaDept> data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == id && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();

            if (flag)
            {
                listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = true });
            }

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.PfaDeptCode, item.PfaDeptName), Value = item.ID.ToString(), Selected = false });
            }

            return listItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyid"></param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        public ActionResult GetSignParentStr(string companyid, string selecteddata)
        {
            Guid? cid = null;

            if (!string.IsNullOrEmpty(companyid))
            {
                cid = Guid.Parse(companyid);
            }

            StringBuilder strOption = new StringBuilder();
            strOption.Append(string.Format("<option value=''{0}>請選擇</option>", (selecteddata == "" ? "selected" : "")));

            List<PfaDept> data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == cid.Value && x.BeginDate <= DateTime.Now && (!x.EndDate.HasValue || x.EndDate >= DateTime.Now)).OrderBy(x => x.PfaDeptCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(selecteddata))
            {
                SelectedDataID = Guid.Parse(selecteddata);
            }

            foreach (var item in data)
            {
                strOption.Append(string.Format("<option value='{0}'{3}>{1} {2}</option>", item.ID.ToString(), item.PfaDeptCode, item.PfaDeptName, (SelectedDataID == item.ID ? "selected" : "")));
            }

            return Json(new { result = strOption.ToString() });
        }
        #endregion
    }
}
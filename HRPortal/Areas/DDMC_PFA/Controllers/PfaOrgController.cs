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
    public class PfaOrgController : BaseController
    {
        #region Index
        public ActionResult Index(int page = 1, string txtCompanyID = "", string txtOrgCode = "", string txtOrgName = "", string cmd = "")
        {
            GetDefaultData(txtCompanyID, txtOrgCode, txtOrgName);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return View();
            }

            var ds = Services.GetService<PfaOrgService>().GetPfaOrgData(txtOrgCode, txtOrgName);

            var viewModel = ds.Select(x => new PfaOrgViewModel()
            {
                ID = x.ID,
                CompanyID = x.CompanyID,
                PfaOrgCode = x.PfaOrgCode,
                PfaOrgName = x.PfaOrgName,
                OrgManagerId = x.OrgManagerId,
                Ordering = x.Ordering,
                PfaDeptIDList = x.PfaOrgDept.Select(y => y.PfaDeptID).ToList()
            }).ToPagedList(currentPage, currentPageSize);

            foreach (var item in viewModel)
            {
                string str = "";

                var datas = Services.GetService<PfaDeptService>().GetAll().Where(x => item.PfaDeptIDList.Contains(x.ID)).Select(x => x.PfaDeptName).ToList();

                foreach (var dept in datas)
                {
                    if (!string.IsNullOrEmpty(str)) str += "、";
                    str += dept;
                }

                item.OrgManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == item.OrgManagerId).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault(); ;
                item.PfaDeptName = str;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtCompanyID, string txtOrgCode, string txtOrgName, string btnQuery, string btnClear)
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
                    txtOrgCode,
                    txtOrgName,
                    cmd = "Query"
                });
            }

            //重整
            GetDefaultData(txtCompanyID, txtOrgCode, txtOrgName);
            return View();
        }

        private void GetDefaultData(string txtCompanyID = "", string txtOrgCode = "", string txtOrgName = "")
        {
            if (string.IsNullOrEmpty(txtCompanyID))
            {
                txtCompanyID = CurrentUser.CompanyID.ToString();
            }
            ViewBag.txtCompanyID = txtCompanyID;
            ViewBag.txtOrgCode = txtOrgCode;
            ViewBag.txtOrgName = txtOrgName;
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
            PfaOrg data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaOrgService>().GetPfaOrg(id);
            }

            CreatePfaOrg model = new CreatePfaOrg();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaOrgCode = data.PfaOrgCode;
                model.PfaOrgName = data.PfaOrgName;
                model.OrgManagerId = data.OrgManagerId;
                model.OrgManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == model.OrgManagerId).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault();
                model.Ordering = data.Ordering;
            }

            return PartialView("_DetailPfaOrg", model);
        }
        #endregion

        #region Create
        public ActionResult Create()
        {
            CreatePfaOrg model = new CreatePfaOrg();
            model.ID = Guid.Empty;
            model.CompanyID = CurrentUser.CompanyID;
            return PartialView("_CreatePfaOrg", model);
        }

        [HttpPost]
        public ActionResult Create(CreatePfaOrg model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var isExist = Services.GetService<PfaOrgService>().IsExist(model.PfaOrgCode);

                if (isExist)
                {
                    WriteLog(string.Format("組織代碼資料已存在,Code:{0}", model.PfaOrgCode));
                    return Json(new { success = false, message = "組織代碼資料已存在" });
                }

                PfaOrg data = new PfaOrg();
                data.ID = Guid.NewGuid();
                data.PfaOrgCode = model.PfaOrgCode;
                data.PfaOrgName = model.PfaOrgName;
                data.OrgManagerId = model.OrgManagerId;
                data.Ordering = model.Ordering;
                data.CompanyID = model.CompanyID;
                data.CreatedBy = CurrentUser.EmployeeID;
                data.CreatedTime = DateTime.Now;

                int IsSuccess = Services.GetService<PfaOrgService>().Create(data);
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
        public ActionResult Edit(Guid? id)
        {
            PfaOrg data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaOrgService>().GetPfaOrg(id);
            }

            CreatePfaOrg model = new CreatePfaOrg();
            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaOrgCode = data.PfaOrgCode;
                model.PfaOrgName = data.PfaOrgName;
                model.OrgManagerId = data.OrgManagerId;
                model.OrgManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == model.OrgManagerId).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault();
                model.Ordering = data.Ordering;
            }
            return PartialView("_EditPfaOrg", model);
        }

        [HttpPost]
        public ActionResult Edit(CreatePfaOrg model)
        {
            if (!ModelState.IsValid)
            {
                WriteLog("驗證失敗請檢查頁面資料");
                return Json(new { success = false, message = "驗證失敗請檢查頁面資料" });
            }
            else
            {
                var data = Services.GetService<PfaOrgService>().GetPfaOrg(model.ID);
                if (data == null)
                {
                    WriteLog(string.Format("查無此績效考核組織,ID:{0}", model.ID));
                    return Json(new { success = false, message = "查無此績效考核組織" });
                }
                else
                {
                    data.PfaOrgCode = model.PfaOrgCode;
                    data.PfaOrgName = model.PfaOrgName;
                    data.OrgManagerId = model.OrgManagerId;
                    data.Ordering = model.Ordering;
                    data.CompanyID = model.CompanyID;
                    data.ModifiedBy = CurrentUser.EmployeeID;
                    data.ModifiedTime = DateTime.Now;

                    int IsSuccess = Services.GetService<PfaOrgService>().Update(data);
                    if (IsSuccess == 1)
                    {
                        WriteLog(string.Format("編輯成功,ID:{0}", data.ID));
                        return Json(new { success = true, message = "編輯成功" });
                    }
                }
            }
            return Json(new { success = true, message = "編輯成功" });
        }
        #endregion

        #region Delete
        /// <summary>
        /// Del
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Del(Guid? id)
        {
            PfaOrg data = null;

            if (id.HasValue)
            {
                data = Services.GetService<PfaOrgService>().GetPfaOrg(id);
            }

            CreatePfaOrg model = new CreatePfaOrg();

            if (data == null)
            {
                model.ID = Guid.Empty;
                model.CompanyID = CurrentUser.CompanyID;
            }
            else
            {
                model.ID = data.ID;
                model.CompanyID = data.CompanyID;
                model.PfaOrgCode = data.PfaOrgCode;
                model.PfaOrgName = data.PfaOrgName;
                model.OrgManagerId = data.OrgManagerId;
                model.OrgManagerName = Services.GetService<EmployeeService>().GetAll().Where(y => y.ID == model.OrgManagerId).Select(y => y.EmployeeNO + " " + y.EmployeeName).FirstOrDefault();
                model.Ordering = data.Ordering;
            }
            return PartialView("_DeletePfaOrg", model);
        }

        /// <summary>
        /// DelPfaDept
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(Guid? id)
        {
            Result result = null;

            PfaOrg data = Services.GetService<PfaOrgService>().GetPfaOrg(id);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此績效考核組織";
                result.log = "查無此績效考核組織";
            }
            else
            {
                result = Services.GetService<PfaOrgService>().DeletePfaOrg(id.Value);
            }
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
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
        #endregion

        #region SelDept
        public ActionResult SelDept(Guid? id)
        {
            SelDeptFunction model = Services.GetService<PfaOrgService>().GetAll().Where(x => x.ID == id).Select(x => new SelDeptFunction()
            {
                PfaOrgID = x.ID,
                PfaOrgCode = x.PfaOrgCode,
                PfaOrgName = x.PfaOrgName
            }).FirstOrDefault();

            if (model == null)
            {
                model = new SelDeptFunction();
                model.PfaOrgID = Guid.Empty;
            }
            else
            {

                var pfaOrgDeptIDList = Services.GetService<PfaOrgDeptService>().GetAll().Select(x=> x.PfaDeptID).ToList();
                List<Guid> OrgDeptID = Services.GetService<PfaOrgDeptService>().GetDeptID(model.PfaOrgID);
                model.DeptItems = Services.GetService<PfaDeptService>().GetAll().OrderBy(x=>x.PfaDeptCode).Select(x => new DeptItem()
                {
                    ID = x.ID,
                    Code = x.PfaDeptCode,
                    Name = x.PfaDeptName,
                    Chk = OrgDeptID.Contains(x.ID),
                    Setucompleted = pfaOrgDeptIDList.Contains(x.ID),
                }).ToList();
            }

            return PartialView("_SelPfaOrgDept", model);
        }

        [HttpPost]
        public ActionResult SelDept(Guid PfaOrgID, string SelDeptID)
        {
            Result result = null;

            var data = Services.GetService<PfaOrgService>().GetAll().Where(x => x.ID == PfaOrgID);
            if (data == null)
            {
                result = new Result();
                result.success = false;
                result.message = "查無此績效考核組織";
                result.log = "查無此雇績效考核組織";
            }
            else
            {
                result = Services.GetService<PfaOrgDeptService>().SaveOrgDept(PfaOrgID, SelDeptID, CurrentUser.EmployeeID);
            }
            WriteLog(result.log);
            return Json(new { success = result.success, message = result.message });
        }
        #endregion
    }
}
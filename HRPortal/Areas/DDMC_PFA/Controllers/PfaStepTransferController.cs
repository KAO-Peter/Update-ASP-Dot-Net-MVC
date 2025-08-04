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
    /// <summary>
    /// 績效考核指定代簽
    /// </summary>
    public class PfaStepTransferController : BaseController
    {
        // GET: DDMC_PFA/PfaStepTransfer
        public ActionResult Index(int page = 1, string txtPfaCycleID = "", string txtSignEmpID = "", string txtSignEmpNo = "", string txtSignEmpName = "", string cmd = "")
        {
            var ds = new List<PfaCycleEmpSignViewModel>();

            GetDefaultData(txtPfaCycleID, txtSignEmpID, txtSignEmpNo, txtSignEmpName);

            int currentPage = page < 1 ? 1 : page;

            if (string.IsNullOrWhiteSpace(cmd))
                return View();

            Guid pfaCycleID = Guid.Parse(txtPfaCycleID);
            Guid signEmpID = Guid.Parse(txtSignEmpID);

            var queryData = Services.GetService<PfaSignProcessService>().GetAll()
                                    .Where(x => (x.Status == PfaSignProcess_Status.PendingReview 
                                                    || x.Status == PfaSignProcess_Status.PendingThirdReview
                                                    || x.Status == PfaSignProcess_Status.NotReceived
                                                    ) 
                                    && x.PfaCycleEmp.PfaCycleID == pfaCycleID 
                                    && x.PreSignEmpID == signEmpID 
                                    && (x.IsSelfEvaluation == true 
                                        || (x.IsFirstEvaluation == true && x.IsSecondEvaluation == false) 
                                        || x.IsSecondEvaluation == true
                                        || x.IsThirdEvaluation == true
                                        ));

            var pfaOptionList = Services.GetService<PfaOptionService>().GetAll()
                .Where(x => x.PfaOptionGroup.OptionGroupCode == "SignStatus")
                .ToList();

            var data = queryData.ToList();
            var pfaCycleIDs = data.Select(x => x.PfaCycleEmp.PfaCycleID)
                .Distinct()
                .ToList();

            var pfaCycleList = Services.GetService<PfaCycleService>().GetAll()
                .Where(x => pfaCycleIDs.Contains(x.ID)).ToList();

            ds = data.Select(x =>
            {
                var pfaCycle = pfaCycleList.FirstOrDefault(y => y.ID == x.PfaCycleEmp.PfaCycleID);
                var pfaOption = pfaOptionList.FirstOrDefault(y => y.OptionCode == x.Status);
                return new PfaCycleEmpSignViewModel()
                {
                    PfaSignProcessID = x.ID,
                    PfaCycleID = x.PfaCycleEmp.PfaCycleID,
                    PfaFormNo = pfaCycle != null ? pfaCycle.PfaFormNo : "",
                    PfaDeptID = x.PfaCycleEmp.PfaDeptID,
                    PfaDeptCode = x.PfaCycleEmp.PfaDept.PfaDeptCode,
                    PfaDeptName = x.PfaCycleEmp.PfaDept.PfaDeptName,
                    EmployeeID = x.PfaCycleEmpID,
                    EmployeeNo = x.PfaCycleEmp.Employees.EmployeeNO,
                    EmployeeName = x.PfaCycleEmp.Employees.EmployeeName,
                    PfaSelfScore = x.PfaCycleEmp.PfaSelfScore,
                    SignStatus = x.Status,
                    StrSignStatus = pfaOption != null ? pfaOption.OptionName : ""
                };
            }).ToList();

            return View(ds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtPfaCycleID, string txtSignEmpID, string txtSignEmpNo, string txtSignEmpName, string btnQuery, string btnClear)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                GetDefaultData();
                return View();
            }
            else if (!string.IsNullOrWhiteSpace(btnQuery))
            {
                if (string.IsNullOrEmpty(txtPfaCycleID))
                {
                    TempData["message"] = "請選擇考核批號";
                }
                else if (string.IsNullOrEmpty(txtSignEmpNo))
                {
                    TempData["message"] = "請輸入簽核人員編";
                }
                else
                {
                    var emp = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == txtSignEmpNo).FirstOrDefault();
                    if (emp == null)
                    {
                        TempData["message"] = "查無此簽核人員資料";
                    }
                    else
                    {
                        return RedirectToAction("Index", new
                        {
                            page = 1,
                            txtPfaCycleID,
                            txtSignEmpID = emp.ID.ToString(),
                            txtSignEmpNo = emp.EmployeeNO,
                            txtSignEmpName = emp.EmployeeName,
                            cmd = "Query"
                        });
                    }
                }
            }

            //重整
            GetDefaultData(txtPfaCycleID, txtSignEmpID, txtSignEmpNo, txtSignEmpName);

            return View();
        }

        public ActionResult Emp(string cmd, string txtPfaCycleID)
        {
            ViewBag.cmd = cmd;
            ViewBag.txtPfaCycleID = txtPfaCycleID;
            ViewBag.CompanyList = GetCompanyList(CurrentUser.CompanyID.ToString());
            ViewBag.DeptList = GetDepartmentList(CurrentUser.CompanyID.ToString(), string.Empty);
            return PartialView("_EmpDialog", null);
        }

        [HttpPost]
        public ActionResult Emp(string txtPfaCycleID, string ddlCompany, string ddlDept = "", string txtEmpNo = "", int page = 1)
        {
            int pageSize = 10;
            int currentPage = page < 1 ? 1 : page;

            var signEmpIDList = new List<Guid>();
            if (!string.IsNullOrWhiteSpace(txtPfaCycleID))
            {
                Guid pfaCycleID = Guid.Parse(txtPfaCycleID);
                signEmpIDList = Services.GetService<PfaSignProcessService>().GetAll().Where(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID).Select(x => x.PreSignEmpID).Distinct().ToList();
            }

            Guid CompanyID = Guid.Parse(ddlCompany);

            var emp = Services.GetService<PfaDeptEmpService>().GetAll().Where(x => x.PfaDept.CompanyID == CompanyID);
            if (signEmpIDList.Any())
                emp = emp.Where(x => signEmpIDList.Contains(x.EmployeeID));

            if (!string.IsNullOrEmpty(ddlDept))
            {
                Guid DeptID = Guid.Parse(ddlDept);
                emp = emp.Where(x => x.PfaDeptID == DeptID);
            }

            if (!string.IsNullOrEmpty(txtEmpNo))
                emp = emp.Where(x => x.Employees.EmployeeNO.Contains(txtEmpNo));

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
            var dataResult = new StringBuilder();
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
                pageResult.Append("<li class='disabled PagedList-ellipses'><a>…</a></li>");

            foreach (KeyValuePair<int, string> item in pageObj)
                pageResult.Append(string.Format("<li id='page' name='pagelist'{0}><a>{1}</a></li>", item.Value, item.Key));

            if (ePidx != result.PageCount)
                pageResult.Append("<li class='disabled PagedList-ellipses'><a>…</a></li>");

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

        public ActionResult GetEmp(string txtPfaCycleID, string txtSignEmpNo)
        {
            var emp = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == txtSignEmpNo).FirstOrDefault();
            if (emp == null)
            {
                return Json(new { success = false, empid = "", empname = "" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(txtPfaCycleID))
                {
                    Guid pfaCycleID = Guid.Parse(txtPfaCycleID);
                    var tempPfaSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.PfaCycleEmp.PfaCycleID == pfaCycleID && x.PreSignEmpID == emp.ID);
                    if (tempPfaSignProcess == null)
                    {
                        return Json(new { success = false, empid = "", empname = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { success = true, empid = emp.ID, empname = emp.EmployeeName }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetDept(string company)
        {
            Guid CompanyID = Guid.Empty;

            if (!string.IsNullOrEmpty(company))
                CompanyID = Guid.Parse(company);

            var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CompanyID).OrderBy(x => x.PfaDeptCode).ToList();
            StringBuilder result = new StringBuilder();
            result.Append("<option value='' selected='selected'>請選擇</option>");

            foreach (var item in data)
                result.Append(string.Format("<option value='{0}'>{1} {2}</option>", item.ID, item.PfaDeptCode, item.PfaDeptName));

            return Json(new { data = result.ToString() });
        }

        [HttpPost]
        public ActionResult Transfer(List<PfaSignProcess> model)
        {
            if (model.Count() == 0)
            {
                WriteLog("請選擇要指定代簽的資料");
                return Json(new { success = false, message = "請選擇要指定代簽的資料" });
            }

            Result result = Services.GetService<PfaSignProcessService>().Transfer(model, CurrentUser.EmployeeID);
            WriteLog(result.log);

            return Json(new { success = result.success, message = result.message });
        }

        /// <summary>
        /// 取得績效考核批號選單
        /// </summary>
        /// <param name="data"></param>
        /// <param name="selectedData"></param>
        /// <returns></returns>
        private List<SelectListItem> GetPfaFormNoList(string selectedData)
        {
            var data = Services.GetService<PfaCycleService>().GetAll().Where(x => x.Status == "a").OrderBy(x => x.PfaFormNo).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selectedData))
                SelectedDataID = Guid.Parse(selectedData);

            var listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selectedData == "" ? true : false) });

            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = item.PfaFormNo, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            return listItem;
        }

        private void GetDefaultData(string txtPfaCycleID = "", string txtSignEmpID = "", string txtSignEmpNo = "", string txtSignEmpName = "")
        {
            ViewData["PfaFormNoList"] = GetPfaFormNoList(txtPfaCycleID);
            ViewBag.txtPfaCycleID = txtPfaCycleID;
            ViewBag.txtSignEmpID = txtSignEmpID;
            ViewBag.txtSignEmpNo = txtSignEmpNo;
            ViewBag.txtSignEmpName = txtSignEmpName;
        }

        private List<SelectListItem> GetCompanyList(string selecteddata)
        {
            var listItem = new List<SelectListItem>();
            var data = Services.GetService<CompanyService>().GetAllSort().ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
                SelectedDataID = Guid.Parse(selecteddata);

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = item.CompanyName, Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            return listItem;
        }

        private List<SelectListItem> GetDepartmentList(string companyID, string selecteddata)
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            Guid CompanyID = Guid.Parse(companyID);

            var data = Services.GetService<PfaDeptService>().GetAll().Where(x => x.CompanyID == CompanyID).OrderBy(x => x.PfaDeptCode).ToList();

            //轉為Guid 判斷ID
            Guid SelectedDataID = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(selecteddata))
                SelectedDataID = Guid.Parse(selecteddata);

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            foreach (var item in data)
                listItem.Add(new SelectListItem { Text = string.Format("{0} {1}", item.PfaDeptCode, item.PfaDeptName), Value = item.ID.ToString(), Selected = (SelectedDataID == item.ID ? true : false) });
            return listItem;
        }
    }
}
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.Reports.Controllers
{
    public class LeaveQueryController : MultiDepEmpController
    {
        // GET: /Reports/LeaveQuery/
        public ActionResult Index()
        {

            ViewData["FormTypelist"] = GetFormTypeList();
            ViewData["DepartmentList"] = GetDepartmentList2(CurrentUser.SignDepartmentID);
            ViewData["EmployeeList"] = GetEmployeetList2(CurrentUser.SignDepartmentID.ToString(), CurrentUser.EmployeeNO, "", "ID");
            ViewData["SignStatuslist"] = GetSignStatusList();

            ViewBag.isALL = isAdmin || isHR;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetLeaveQuerySummary(LeaveQuerySummaryViewModel viewmodel)
        {
            var result = await Query(viewmodel.EmpID, viewmodel.DeptID, DateTime.Parse(viewmodel.BeginDate), DateTime.Parse(viewmodel.EndDate), viewmodel.course_category, viewmodel.SignStatusdate);

            int currentPage = viewmodel.page < 1 ? 1 : viewmodel.page;
            return PartialView("_LeaveQuerySummary", result.ToPagedList(currentPage, currentPageSize));
        }

        /// <summary>
        /// 取得簽核狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetSignStatusList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (getLanguageCookie == "en-US")
            {
                listItem.Add(new SelectListItem { Text = "All", Value = "", Selected = (selecteddata == "" ? true : false) });
                listItem.Add(new SelectListItem { Text = "Not sent", Value = "W", Selected = (selecteddata == "W" ? true : false) });
                listItem.Add(new SelectListItem { Text = "Signing", Value = "WS", Selected = (selecteddata == "WS" ? true : false) });
                listItem.Add(new SelectListItem { Text = "Verify", Value = "A", Selected = (selecteddata == "A" ? true : false) });
            }
            else
            {
                listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
                listItem.Add(new SelectListItem { Text = "未送出", Value = "W", Selected = (selecteddata == "W" ? true : false) });
                listItem.Add(new SelectListItem { Text = "未簽核", Value = "WS", Selected = (selecteddata == "WS" ? true : false) });
                listItem.Add(new SelectListItem { Text = "簽核完成", Value = "A", Selected = (selecteddata == "A" ? true : false) });
            }
            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }

        /// <summary>
        /// 取得簽核類別
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetFormTypeList(string selecteddata = "")
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = getLanguageCookie == "en-US" ? "All" : "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
            //listItem.Add(new SelectListItem { Text = "假單申請", Value = ((int)FormType.Leave).ToString(), Selected = (selecteddata == ((int)FormType.Leave).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "加班申請", Value = ((int)FormType.OverTime).ToString(), Selected = (selecteddata == ((int)FormType.OverTime).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "補刷卡申請", Value = ((int)FormType.PatchCard).ToString(), Selected = (selecteddata == ((int)FormType.PatchCard).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "假單刪除申請", Value = ((int)FormType.LeaveCancel).ToString(), Selected = (selecteddata == ((int)FormType.LeaveCancel).ToString() ? true : false) });
            //listItem.Add(new SelectListItem { Text = "加班單刪除申請", Value = ((int)FormType.OverTimeCancel).ToString(), Selected = (selecteddata == ((int)FormType.OverTimeCancel).ToString() ? true : false) });
            OptionListHelper _optionListHelper = new OptionListHelper();
            listItem.AddRange(_optionListHelper.GetOptionList("FormType"));
            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;

            return listItem;
        }

        public async Task<List<HRPotralFormSignStatus>> Query(string employee, string departmentcode, DateTime startTime, DateTime endTime, string formtype, string SignStatusdate)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralFormSignStatus> _result = new List<HRPotralFormSignStatus>();

                if (!string.IsNullOrWhiteSpace(employee))   //配合跨公司簽核，依照公司分群後查詢資料。
                {
                    var employeeguid = employee.TrimEnd(',').Split(',');
                    var employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeguid.Contains(x.ID.ToString())).ToList();

                    foreach (var c in employees.Select(s => s.Company.CompanyCode).Distinct())
                    {
                        var employeenos = string.Join(",", employees.Where(x => x.Company.CompanyCode == c).Select(s => s.EmployeeNO).ToList());
                        _result.AddRange(_queryHelper.GetCurrentSignListByEmployee(c, employeenos, false, startTime, endTime.AddDays(1)));
                    }

                }
                else if (!string.IsNullOrWhiteSpace(departmentcode))
                {
                    _result = _queryHelper.GetCurrentSignListByDepartment(CurrentUser.CompanyCode, departmentcode, false, startTime, endTime.AddDays(1));
                }

                formtype = string.IsNullOrWhiteSpace(formtype) ? "" : formtype;
                switch (formtype)
                {
                    case "":
                        break;
                    default:
                        _result = _result.Where(x => x.FormType == (FormType)(Enum.Parse(typeof(FormType), formtype))).ToList();
                        break;
                }

                _result = _result.OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();

                //判斷簽核狀態後取得資料 Irving 2016/03/31
                SignStatusdate = string.IsNullOrWhiteSpace(SignStatusdate) ? "" : SignStatusdate;
                foreach (var i in _result)
                {
                    if (SignStatusdate == "A")
                    {
                        _result = _result.Where(x => x.SignStatus == "A" || x.SignStatus == "X").OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                    }
                    else if (SignStatusdate == "W")
                    {
                        _result = _result.Where(x => x.SignType == "S" && x.SignStatus == "W").OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                    }
                    else if (SignStatusdate == "WS")
                    {
                        _result = _result.Where(x => x.SignType == "P" && x.SignStatus == "W").OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                    }
                }
                //End

                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    _summaryBuilder.BuildSummary(_item);
                }

                return _result;
            }
        }
    }
}
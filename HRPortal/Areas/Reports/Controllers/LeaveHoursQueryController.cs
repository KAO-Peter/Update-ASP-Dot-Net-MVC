using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Controllers;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.Reports.Controllers
{
    public class LeaveHoursQueryController : MultiDepEmpController
    {
        private ReportQueryController ReportQuery = new ReportQueryController();
        // GET: /Reports/LeaveHours/Index
        //[HttpGet]
        //public async Task<ActionResult> Index(string EmployeeNO = "", string DepartmentCode = "", string StatusData = "", bool btnQuery = false)
        public ActionResult Index()
        {

            string statusData = "";
            if (CurrentUser.Employee.LeaveDate != null && CurrentUser.Employee.LeaveDate < DateTime.Now)
            {
                statusData = "L";
            }

            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            Guid CompanyID = this.CurrentUser.CompanyID;
            bool role = (this.CurrentUser.IsHR == true || this.CurrentUser.IsAdmin == true || this.CurrentUser.SignDepartments.Count > 0) ? true : false;
            ViewData["DepartmentList"] = GetDepartmentList2(CurrentUser.SignDepartmentID);// ReportQuery.GetDepartmentList(CompanyID, DepartmentCode, false, role, this.CurrentUser.SignDepartments.Count > 0 ? this.CurrentUser.SignDepartments : this.CurrentUser.Departments);
            ViewData["EmployeeList"] = GetEmployeetList2(CurrentUser.SignDepartmentID.ToString(), CurrentUser.EmployeeNO, statusData, "ID"); //ReportQuery.GetEmployeetList(CompanyID, DepartmentCode, EmployeeNO, StatusData, false, role);
            ViewData["Statuslist"] = GetStatusDataList(); ;// ReportQuery.GetStatusDataList(StatusData);
            ViewData["LeaveDatas"] = null;
           
            ViewBag.Role = role;
            ViewBag.getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetLeaveHoursQuery(LeaveHoursQueryViewModel viewmodel)
        {
            var result = await Query(viewmodel.EmpID);

            int currentPage = viewmodel.page < 1 ? 1 : viewmodel.page;
            return PartialView("_LeaveHoursDetail", result);
        }

        public async Task<LeaveHoursQueryViewModel> Query(string employee, string LanguageCookie = "zh-TW")
        {

            string[] employeeguid;
            List<Employee> employees = new List<Employee>();
            if (!string.IsNullOrWhiteSpace(employee))   //配合跨公司簽核，依照公司分群後查詢資料。
            {
                employeeguid = employee.TrimEnd(',').Split(',');
                employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeguid.Contains(x.ID.ToString())).ToList();
            }
            LeaveHoursQueryViewModel model = new LeaveHoursQueryViewModel();
            model.LeaveHoursDetailList = new List<LeaveHoursDetail>();
            
            foreach (var data in employees)
            {

                //2018/11/7 Neo 假別調整排序
                List<AbsentDetail> leaveDatas = await ReportQuery.GetLeaveAmountList(data.CompanyID, data.EmployeeNO);
                List<AbsentDetail> leaveDatas2 = new List<AbsentDetail>();
                List<EmpHolidayData> holidayDatas = await this.GetHolidayDatas(data.CompanyID, data.EmployeeNO, "1");
                List<EmpHolidayData> holidayDatas2 = new List<EmpHolidayData>();

                //2018/12/20 Neo 調整補休時數為0的不顯示
                List<AbsentDetail> leaveDatas3 = new List<AbsentDetail>();
                foreach (var item in leaveDatas)
                {
                    if (item.Code == "rest")
                    {
                        if (item.AnnualLeaveHours > 0)
                        {
                            leaveDatas3.Add(item);
                        }
                    }
                    else
                    {
                        leaveDatas3.Add(item);
                    }
                }
                leaveDatas = leaveDatas3;

                //取得假別優先排序
                string sortLeaveStr = Services.GetService<SystemSettingService>().GetSettingValue("SortLeave");
                string[] sortLeavAry = null;

                if (!string.IsNullOrEmpty(sortLeaveStr))
                {
                    sortLeavAry = sortLeaveStr.Split(';');
                    foreach (var sortLeave in sortLeavAry)
                    {
                        var SLeave = leaveDatas.Where(x => x.Name == sortLeave).FirstOrDefault();
                        if (SLeave != null)
                        {
                            leaveDatas2.Add(SLeave);
                        }
                        leaveDatas = leaveDatas.Where(x => x.Name != sortLeave).ToList();//排除假別優先排序
                    }
                }
                leaveDatas = leaveDatas.OrderBy(x => x.Name.Substring(0)).ToList();//其他假別需依假別名稱第一個字的筆劃來排序(遞增)
                if (leaveDatas != null && leaveDatas.Count() > 0)
                {
                    leaveDatas2.AddRange(leaveDatas);
                }

                LeaveHoursDetail listData = new LeaveHoursDetail
                {
                    DepartCode = data.Department.DepartmentCode,
                    DepartName = data.Department.DepartmentName,
                    EmployeeNo = data.EmployeeNO,
                    EmployeeName = data.EmployeeName,
                    leaveDatas = leaveDatas2,
                    holidayDatas = holidayDatas
                };
                model.LeaveHoursDetailList.Add(listData);
            }

            return model;
        }

        //取得員工補休時數的起訖日期
        private async Task<List<EmpHolidayData>> GetHolidayDatas(Guid CompanyID, string EmployeeNO, string AbsentID)
        {
            List<EmpHolidayData> _result = await ReportQuery.GetEmpHolidayDataList(CompanyID, EmployeeNO, AbsentID);

            List<EmpHolidayData> _restLeave = await ReportQuery.GetUnLockRestLeaveInfo(CompanyID, EmployeeNO);

            _result.AddRange(_restLeave);

            //2018/12/20 Neo 調整補休時數為0的不顯示
            List<EmpHolidayData> resultList = new List<EmpHolidayData>();

            foreach (var item in _result)
            {
                if (item.CanUseCount > 0)
                {
                    resultList.Add(item);
                }
            }

            return resultList;
        }
             
    }
}

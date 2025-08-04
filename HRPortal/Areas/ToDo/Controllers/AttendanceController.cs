using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.ToDo.Controllers
{
    public class AttendanceController : MultiDepEmpController
    {
        private bool isHR = false, isAdmin = false;
        // GET: /ToDo/Attendance/
        public ActionResult Index()
        {
            AttendanceSummaryViewModel viewmodel = new AttendanceSummaryViewModel();

            //開始日期預設調整成前七天
            ViewBag.StartTime = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
            ViewBag.EndTime = DateTime.Now.ToString("yyyy/MM/dd");

            viewmodel.DepartmentListData = GetDepartmentList2(CurrentUser.SignDepartmentID);
            viewmodel.EmployeeListData = GetEmployeetList2(CurrentUser.SignDepartmentID.ToString(), CurrentUser.EmployeeNO, "", "ID");
            viewmodel.StatuslistDataData = GetStatusDataList();
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            viewmodel.Role = roleDataa.RoleParams != null ? roleDataa.RoleParams : CurrentUser.SignDepartments.Count > 0 ? "is_sign_manager" : roleDataa.RoleParams;

            ViewBag.isALL = isAdmin || isHR;

            return View(viewmodel);
        }

        //取得部門人員出勤彙總表
        [HttpPost]
        public async Task<ActionResult> GetDutyScheduleSummary(AttendanceSummaryViewModel viewmodel)
        {
            List<DutyScheduleSummary> _result = new List<DutyScheduleSummary>();
            _result = await GetDutyScheduleSummaryData(viewmodel);

            return PartialView("_DutyScheduleSummary", _result);
        }

        //取得請假加班資料
        public async Task<List<DutyScheduleSummary>> GetDutyScheduleSummaryData(AttendanceSummaryViewModel data)
        {
            List<DutyScheduleSummary> _result = new List<DutyScheduleSummary>();
            DateTime BeginDate = DateTime.Parse(data.BeginDate);
            DateTime EndDate = DateTime.Parse(data.EndDate);
            EndDate = EndDate.AddDays(1).AddSeconds(-1);

            if (!string.IsNullOrWhiteSpace(data.EmpID))
            {
                var employeeguid = data.EmpID.TrimEnd(',').Split(','); //配合跨公司簽核，依照公司分群後查詢資料。
                var employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeguid.Contains(x.ID.ToString())).ToList();

                foreach (var c in employees.Select(s => s.Company.CompanyCode).Distinct())
                {
                    List<string> employeenos = employees.Where(x => x.Company.CompanyCode == c).Select(s => s.EmployeeNO).ToList();

                    _result.AddRange(await GetDutyScheduleSummaryDataCore(data, employeenos, c));
                }

            }

            return _result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="employeeNo">員工編號字串，多筆用逗號分隔。ex:0001,0002,0003</param>
        /// <param name="companycode">公司代號</param>
        /// <returns></returns>
        private async Task<List<DutyScheduleSummary>> GetDutyScheduleSummaryDataCore(AttendanceSummaryViewModel data,
            List<string> employeeNo, string companycode)
        {
            List<DutyScheduleSummary> _result = new List<DutyScheduleSummary>();
            List<DutyScheduleSummary> _EmployeeDate = new List<DutyScheduleSummary>();
            DateTime BeginDate = DateTime.Parse(data.BeginDate);
            DateTime EndDate = DateTime.Parse(data.EndDate);
            EndDate = EndDate.AddDays(1).AddSeconds(-1);

            if (employeeNo.Count > 0)
            {

                var Employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeNo.Contains(x.EmployeeNO));


                foreach (var i in Employees)
                {
                    if (i.LeaveDate == null || i.LeaveDate > DateTime.Now)
                    {
                        if (i.EmployeeNO != "admin")
                        {
                            var DepartmentIDD = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == i.EmployeeNO).Select(x => x.DepartmentID).FirstOrDefault();
                            var DepartmentCode = Services.GetService<DepartmentService>().GetAll().Where(x => x.ID == DepartmentIDD).Select(x => x.DepartmentCode).FirstOrDefault();
                            _EmployeeDate = await HRMApiAdapter.GetDutyScheduleSummary(CurrentUser.CompanyCode, DepartmentCode, i.EmployeeNO, data.SelectedStatuslistData, BeginDate, EndDate);
                            foreach (var item in _EmployeeDate)
                            {
                                var result = new DutyScheduleSummary()
                                {
                                    CompanyCode = item.CompanyCode,
                                    DeptCode = item.DeptCode,
                                    DeptMember = item.DeptMember,
                                    DeptName = item.DeptName,
                                };
                                _result.Add(result);
                            }
                        }
                    }
                }
            }

            return _result;
        }
    }
}
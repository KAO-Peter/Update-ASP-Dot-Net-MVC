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

namespace HRPortal.Areas.FEPH.Controllers
{
    public class DeptEmpHolidaySummaryController : BaseController
    {
        private bool isGetRole = false, isHR = false, isAdmin = false;
        
        //
        // GET: /Reports/DeptEmpHolidaySummary/
        public ActionResult Index(string EmployeeData = "", string DepartmentData = "", string YearData = "", string StatusData = "")
        {
            HolidaySummaryViewModel viewmodel = new HolidaySummaryViewModel();

            #region 使用者角色
            if (!isGetRole)
            {
                isGetRole = GetRole();
            }
            #endregion

            viewmodel.YearListData = GetYearList(YearData);
            viewmodel.StatuslistDataData = GetStatusDataList(StatusData);

            if (isHR || isAdmin)
            {
                if (string.IsNullOrWhiteSpace(DepartmentData))
                {
                    DepartmentData = CurrentUser.DepartmentCode;
                }

                if (string.IsNullOrWhiteSpace(EmployeeData))
                {
                    EmployeeData = CurrentUser.EmployeeNO;
                }

                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData = GetEmployeeetList(DepartmentData,StatusData, EmployeeData);
            }
            else if (CurrentUser.SignDepartments.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(DepartmentData))
                    DepartmentData = CurrentUser.SignDepartmentCode;
                viewmodel.DepartmentListData = GetDepartmentList(DepartmentData);
                viewmodel.EmployeeListData = GetEmployeeetList(DepartmentData,StatusData, EmployeeData);
            }
                      
            //selected data
            viewmodel.SelectedDepartment = DepartmentData;
            viewmodel.SelectedEmployee = EmployeeData;
            viewmodel.SelectedStatuslistData = StatusData;
           
            if (string.IsNullOrWhiteSpace(YearData))
                viewmodel.SelectedYear = DateTime.Now.Year.ToString();
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            viewmodel.Role = roleDataa.RoleParams != null ? roleDataa.RoleParams : CurrentUser.SignDepartments.Count > 0 ? "is_sign_manager" : roleDataa.RoleParams;
            return View(viewmodel);
        }
        /// <summary>
        /// 取得員工在離職狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetStatusDataList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "顯示在職人員", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "顯示離職人員", Value = "L", Selected = (selecteddata == "L" ? true : false) });
            listItem.Add(new SelectListItem { Text = "全部", Value = "ALL", Selected = (selecteddata == "ALL" ? true : false) });

            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }
        /// <summary>
        /// 給下拉式選單讀取在離職員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetStatusData(string DepartmentId, string StatusData)
        {
            List<SelectListItem> result = GetEmployeeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 取得在離職員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeeetList(string departmentdata, string StatusData, string selecteddata = "")
        {
            //取得部門
            Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentdata && x.Enabled).FirstOrDefault();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();

            //取得員工列表
            GetRole();
            if (isHR || isAdmin || _department.SignManager.EmployeeNO == CurrentUser.EmployeeNO)
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            }
            else if (_department.SignManager.EmployeeNO != CurrentUser.EmployeeNO)
            {
                bool flag = false;
                foreach (var item in CurrentUser.SignDepartments)
                {
                    if (item.DepartmentCode == departmentdata)
                    {
                        flag = true;
                    }
                }
                if (flag == false)
                {
                    data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).Where(x => x.EmployeeNO == CurrentUser.EmployeeNO).OrderBy(x => x.EmployeeNO).ToList();
                }
                else
                {
                    data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
                }
            }

            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    if (StatusData == "")
                    {
                        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                        {
                            listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                        }
                    }
                    else
                        if (StatusData == "L")
                        {
                            if (item.LeaveDate < DateTime.Now)
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                        }
                        else
                            if (StatusData == "ALL")
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                }
                else
                {
                    if (StatusData == "")
                    {
                        if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                        {
                            listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                        }
                    }
                    else
                        if (StatusData == "L")
                        {
                            if (item.LeaveDate < DateTime.Now)
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                        }
                        else
                            if (StatusData == "ALL")
                            {
                                listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                            }
                }
            }
            return listItem;
        }
        [HttpPost]
        public ActionResult Index(HolidaySummaryViewModel viewmodel, string btnQuery, string btnClear)
        {

            //if (!string.IsNullOrWhiteSpace(btnQuery))
            //{
            //    return RedirectToAction("Index", new
            //    {
            //        DepartmentData=viewmodel.SelectedDepartment,
            //        EmployeeData=viewmodel.SelectedEmployee,
            //        YearData=viewmodel.SelectedYear
            //    });
            //}
            //else 
            //if (!string.IsNullOrWhiteSpace(btnClear))
            //{
            //    return RedirectToAction("Index", new
            //    {
            //        DepartmentData = "",
            //        EmployeeData = "",
            //        YearData = ""
            //    });
            //}
            //return View(viewmodel);

            return Json(new { Dept = this.CurrentUser.DepartmentCode, Empno = this.CurrentUser.EmployeeNO, Year = DateTime.Now.Year.ToString() }, JsonRequestBehavior.AllowGet);
        }

        private async Task<HolidaySummaryViewModel> GetLeaveData(string employeeNo, string departmentID, string StatusData, string HireId, string yearData)
        {
            HolidaySummaryViewModel viewmodel = new HolidaySummaryViewModel();
            List<HolidaySummaryDetail> _viewresult = new List<HolidaySummaryDetail>();
            List<PersonalSummary> _viewpersonalresult = new List<PersonalSummary>();
            List<SelectListItem> EmployeeList = new List<SelectListItem>();
            DeptEmpLeaveSummaryItem _result = new DeptEmpLeaveSummaryItem();
            DeptEmpLeaveSummaryItem _resultTemp = new DeptEmpLeaveSummaryItem();
            _resultTemp.DetailDatas = new List<LeaveSummaryItem>();
            _resultTemp.PersonalDetailDatas = new List<EmpLeaveSummaryItem>();
            List<AbsentDetail> Absentdataa = null;
            if (employeeNo == "All")
            {
                employeeNo = null;
                EmployeeList = GetEmployeeetList(departmentID, StatusData+"");
                EmployeeList.RemoveAt(0);
                departmentID = null;
            }
            else
            {
            EmployeeList.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = employeeNo , Selected = true});
            }

            for (int t = 0; t <= EmployeeList.Count - 1; t++)
            {
                employeeNo = EmployeeList[t].Value;         
                _result = await HRMApiAdapter.GetLeaveSummary(CurrentUser.CompanyCode, departmentID, employeeNo, StatusData, HireId, int.Parse(yearData));
                _resultTemp.DetailDatas.AddRange(_result.DetailDatas);
                _resultTemp.PersonalDetailDatas.AddRange(_result.PersonalDetailDatas);
            }
            _result = _resultTemp;
            employeeNo = EmployeeList.Count > 1 ? null : employeeNo;
            //判斷查詢年度
            DateTime selectDate = DateTime.Now;
            if (yearData != DateTime.Now.Year + "")
            {
                selectDate = new DateTime(int.Parse(yearData), 12, 31);
            }

            //抓取假別資料
            Absentdataa = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, employeeNo, selectDate, "year");
           
                foreach (var item in _result.DetailDatas)
                {
                    string ApprovedHours    = "";//簽核中時數
                    string AnnualLeaveHours = "";//年度可休
                    string AllLeaveHours    = "";//年度核假
                    string OverdueHours     = "";//逾期未休

                    foreach (var ab in Absentdataa)
                    {
                        if (ab.Code == item.AbsentCode)
                        {
                            AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                            AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                            OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                            break;
                        }
                    }
                    //20160613 by Bee 增加在空中飄的時數
                    if (employeeNo != null)
                    {
                        Employee _EmpData = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == CurrentUser.CompanyCode && x.EmployeeNO == employeeNo).FirstOrDefault();
                        Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(_EmpData.ID, selectDate);
                        if (notApprovedAbsentAmount.ContainsKey(item.AbsentCode))
                        {
                            ApprovedHours = decimal.Round(notApprovedAbsentAmount[item.AbsentCode], 1).ToString();
                        }
                    }
                    else if (employeeNo == null)
                    {
                        decimal tempApprovedHours    = 0;
                        decimal tempAnnualLeaveHours = 0;
                        decimal tempAllLeaveHours    = 0;
                        decimal tempAllOverdueHours  = 0;

                        foreach (var detail in _result.PersonalDetailDatas)
                        {
                            List<AbsentDetail> Absentdata = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, detail.EmpNo, selectDate, "year");
                            foreach (var ab in Absentdata)
                            {
                                if (ab.Code == item.AbsentCode)
                                {
                                    AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                                    AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                                    OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                                    break;
                                }
                            }
                            if (detail.SummaryDetail.Count() > 0)
                            {
                                Employee _EmpData = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == CurrentUser.CompanyCode && x.EmployeeNO == detail.EmpNo).FirstOrDefault();
                                Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(_EmpData.ID, selectDate);
                                var _absent = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, detail.EmpNo, selectDate, "year");
                                foreach (var i in _absent)
                                {
                                    if (i.Code == item.AbsentCode)
                                    {
                                        tempAnnualLeaveHours +=  i.Unit == "d" ? i.AnnualLeaveHours * 8 : i.AnnualLeaveHours;
                                        tempAllLeaveHours += i.Unit == "d" ? i.AllLeaveHours * 8 :i.AllLeaveHours;
                                        tempAllOverdueHours +=  i.Unit == "d" ? i.OverdueHours * 8 : i.OverdueHours;
                                    }
                                }
                                if (notApprovedAbsentAmount.ContainsKey(item.AbsentCode))
                                {
                                    tempApprovedHours += decimal.Round(notApprovedAbsentAmount[item.AbsentCode], 1);
                                }
                            }
                        }
                        ApprovedHours    = tempApprovedHours.ToString();
                        AnnualLeaveHours = tempAnnualLeaveHours.ToString();
                        AllLeaveHours    = tempAllLeaveHours.ToString();
                        OverdueHours     = tempAllOverdueHours.ToString();
                    }

                    decimal ReminderAbsentHours = this.CalculateReminderAbsentHours(AllLeaveHours, ApprovedHours, item.EachMonth, OverdueHours);
                    _viewresult.Add(new HolidaySummaryDetail { AnnualLeaveHours = AllLeaveHours, ApprovedHours = ApprovedHours, EachMonth = item.EachMonth, RemainderLeaveHours = Convert.ToString(ReminderAbsentHours), TypeName = item.Name, OverdueHours = OverdueHours });

                }

                //將還在空中飄但目前尚未有建立到後台的時數顯示出來 by Bee
                if (employeeNo != null)
                {
                    string ApprovedHours    = "";
                    string AnnualLeaveHours = "";
                    string AllLeaveHours    = "";
                    string OverdueHours     = "";

                    Employee _EmpData = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == CurrentUser.CompanyCode && x.EmployeeNO == employeeNo).FirstOrDefault();
                    Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(_EmpData.ID, selectDate);
                    foreach (var AbsentAmount in notApprovedAbsentAmount)
                    {
                        foreach (var ab in Absentdataa)
                        {
                            if (ab.Code == AbsentAmount.Key)
                            {
                                AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                                AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                                OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                                break;
                            }
                        }
                        var absentFlag = 0;
                        foreach (var item in _result.DetailDatas)
                        {
                            if (AbsentAmount.Key != item.AbsentCode)
                            {
                                absentFlag = 1;
                            }
                            else if (AbsentAmount.Key == item.AbsentCode)
                            {
                                absentFlag = 0;
                                break;
                            }

                        }
                        if (absentFlag == 1)
                        {
                            ApprovedHours = decimal.Round(notApprovedAbsentAmount[AbsentAmount.Key], 1).ToString();
                            var AbsentsSet = await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode);
                            string AbsentName = "";
                            List<decimal> eachMonth = new List<decimal>();
                            for (int i = 0; i < 12; i++)
                            {
                                eachMonth.Add(0);
                            }
                            foreach (var absent in AbsentsSet)
                            {
                                if (absent.Key == AbsentAmount.Key)
                                {
                                    AbsentName = absent.Value;
                                }
                            }
                            _viewresult.Add(new HolidaySummaryDetail { AnnualLeaveHours = AllLeaveHours, ApprovedHours = ApprovedHours, EachMonth = eachMonth, RemainderLeaveHours = Convert.ToString(AllLeaveHours), TypeName = AbsentName, OverdueHours = OverdueHours });
                        }
                    }
                }
                else if (employeeNo == null)
                {
                    //decimal tempApprovedHours = 0;
                    string ApprovedHours        = "";
                    string AnnualLeaveHours     = "";
                    string AllLeaveHours        = "";
                    string OverdueHours         = "";

                    foreach (var detail in _result.PersonalDetailDatas)
                    {
                        List<AbsentDetail> Absentdata = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, detail.EmpNo, selectDate, "year");
                        if (detail.SummaryDetail.Count() > 0)
                        {
                            Employee _EmpData = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == CurrentUser.CompanyCode && x.EmployeeNO == detail.EmpNo).FirstOrDefault();
                            Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(_EmpData.ID, selectDate);
                            foreach (var AbsentAmount in notApprovedAbsentAmount)
                            {
                                foreach (var ab in Absentdata)
                                {
                                    if (ab.Code == AbsentAmount.Key)
                                    {
                                        AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                                        AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                                        OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                                        break;
                                    }
                                }
                                var absentFlag = 0;
                                foreach (var item in _result.DetailDatas)
                                {
                                    if (AbsentAmount.Key != item.AbsentCode)
                                    {
                                        absentFlag = 1;
                                    }
                                    else if (AbsentAmount.Key == item.AbsentCode)
                                    {
                                        absentFlag = 0;
                                        break;
                                    }
                                }
                                if (absentFlag == 1)
                                {
                                    ApprovedHours = decimal.Round(notApprovedAbsentAmount[AbsentAmount.Key], 1).ToString();
                                    var AbsentsSet = await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode);
                                    string AbsentName = "";
                                    List<decimal> eachMonth = new List<decimal>();
                                    for (int i = 0; i < 12; i++)
                                    {
                                        eachMonth.Add(0);
                                    }
                                    foreach (var absent in AbsentsSet)
                                    {
                                        if (absent.Key == AbsentAmount.Key)
                                        {
                                            AbsentName = absent.Value;
                                        }
                                    }
                                    foreach (var ab in Absentdata)
                                    {
                                        if (ab.Code == AbsentAmount.Key)
                                        {
                                            AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                                            AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                                            OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                                            break;
                                        }
                                    }
                                    decimal ReminderAbsentHours = this.CalculateReminderAbsentHours(AllLeaveHours, ApprovedHours, eachMonth, OverdueHours);

                                    _viewresult.Add(new HolidaySummaryDetail { AnnualLeaveHours = AllLeaveHours, ApprovedHours = ApprovedHours, EachMonth = eachMonth, RemainderLeaveHours = Convert.ToString(ReminderAbsentHours), TypeName = AbsentName, OverdueHours = OverdueHours });
                                    //_viewresult.Add(new HolidaySummaryDetail { AnnualLeaveHours = AnnualLeaveHours, ApprovedHours = ApprovedHours, EachMonth = eachMonth, TypeName = AbsentName });
                                }
                            }
                        }
                    }
                }

                //將還在空中飄但目前尚未有建立到後台的時數顯示出來END by Bee 

                if (_result.PersonalDetailDatas.Count > 0)
                {
                    foreach (var detail in _result.PersonalDetailDatas)
                    {
                        List<AbsentDetail> Absentdata = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, detail.EmpNo, selectDate, "year");
                        //讀取每個員工的細項
                        List<HolidaySummaryDetail> data = new List<HolidaySummaryDetail>();
                        if (detail.SummaryDetail.Count() > 0)
                        {
                            Employee _EmpData = Services.GetService<EmployeeService>().Where(x => x.Company.CompanyCode == CurrentUser.CompanyCode && x.EmployeeNO == detail.EmpNo).FirstOrDefault();
                            Dictionary<string, decimal> notApprovedAbsentAmount = Services.GetService<LeaveFormService>().SummaryNotApprovedAbsentAmount(_EmpData.ID, selectDate);
                            foreach (var item in detail.SummaryDetail)
                            {
                                string ApprovedHours    = "";
                                string AnnualLeaveHours = "";
                                string AllLeaveHours    = "";
                                string OverdueHours     = "";

                                if (notApprovedAbsentAmount.ContainsKey(item.AbsentCode))
                                {
                                    ApprovedHours = decimal.Round(notApprovedAbsentAmount[item.AbsentCode], 1).ToString();
                                }
                                foreach (var ab in Absentdata)
                                {
                                    if (ab.Code == item.AbsentCode)
                                    {
                                        AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                                        AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                                        OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                                        break;
                                    }
                                }

                                decimal ReminderAbsentHours = this.CalculateReminderAbsentHours(AllLeaveHours, ApprovedHours, item.EachMonth, OverdueHours);

                                data.Add(new HolidaySummaryDetail { AnnualLeaveHours = AllLeaveHours, ApprovedHours = ApprovedHours, EachMonth = item.EachMonth, RemainderLeaveHours = Convert.ToString(ReminderAbsentHours), TypeName = item.Name,OverdueHours = OverdueHours });

                                //data.Add(new HolidaySummaryDetail { AnnualLeaveHours = AnnualLeaveHours, ApprovedHours = ApprovedHours, TypeName = item.Name, EachMonth = item.EachMonth });
                            }

                            //將還在空中飄但目前尚未有建立到後台的時數顯示出來 by Bee 
                            foreach (var AbsentAmount in notApprovedAbsentAmount)
                            {
                                var absentFlag          = 0;
                                string ApprovedHours    = "";
                                string AnnualLeaveHours = "";
                                string AllLeaveHours    = "";
                                string OverdueHours     = "";

                                foreach (var item in _result.DetailDatas)
                                {
                                    if (AbsentAmount.Key != item.AbsentCode)
                                    {
                                        absentFlag = 1;
                                    }
                                    else if (AbsentAmount.Key == item.AbsentCode)
                                    {
                                        absentFlag = 0;
                                        break;
                                    }
                                }
                                if (absentFlag == 1)
                                {
                                    ApprovedHours = decimal.Round(notApprovedAbsentAmount[AbsentAmount.Key], 1).ToString();
                                    var AbsentsSet = await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode);
                                    string AbsentName = "";
                                    List<decimal> eachMonth = new List<decimal>();
                                    for (int i = 0; i < 12; i++)
                                    {
                                        eachMonth.Add(0);
                                    }
                                    foreach (var absent in AbsentsSet)
                                    {
                                        if (absent.Key == AbsentAmount.Key)
                                        {
                                            AbsentName = absent.Value;
                                        }
                                    }
                                    foreach (var ab in Absentdata)
                                    {
                                        if (ab.Code == AbsentAmount.Key)
                                        {
                                            AnnualLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AnnualLeaveHours * 8) : Convert.ToString(ab.AnnualLeaveHours);
                                            AllLeaveHours = ab.Unit == "d" ? Convert.ToString(ab.AllLeaveHours * 8) : Convert.ToString(ab.AllLeaveHours);
                                            OverdueHours = ab.Unit == "d" ? Convert.ToString(ab.OverdueHours * 8) : Convert.ToString(ab.OverdueHours);
                                            break;
                                        }
                                    }

                                    decimal ReminderAbsentHours = this.CalculateReminderAbsentHours(AllLeaveHours, ApprovedHours, eachMonth, OverdueHours);

                                    data.Add(new HolidaySummaryDetail { AnnualLeaveHours = AllLeaveHours, ApprovedHours = ApprovedHours, EachMonth = eachMonth, RemainderLeaveHours = Convert.ToString(ReminderAbsentHours), TypeName = AbsentName, OverdueHours = OverdueHours });

                                    //data.Add(new HolidaySummaryDetail { AnnualLeaveHours = AnnualLeaveHours, ApprovedHours = ApprovedHours, TypeName = AbsentName, EachMonth = eachMonth });
                                }
                            }
                        }

                        _viewpersonalresult.Add(new PersonalSummary { SummaryDetail = data, EmployeeNo = detail.EmpNo, EmployeeName = detail.EmpName });
                    }

                    ViewBag.EmployeeName = MultiLanguage.Resource.SummaryTable;
                }
                else
                {
                    var employeedatas = Services.GetService<EmployeeService>().GetAll().Where(x => (x.Company.CompanyCode == CurrentUser.CompanyCode && x.EmployeeNO == employeeNo)).FirstOrDefault();
                    ViewBag.EmployeeNo = employeedatas.EmployeeNO;
                    ViewBag.EmployeeName = employeedatas.EmployeeName;
                }
                //viewmodel.DetailDatas = _viewresult.GroupBy(t => t.TypeName).Select(g => g.First()).ToList();
                //彙總表時數加總起來 Irving 20170411
                List<HolidaySummaryDetail> viewmodels = new List<HolidaySummaryDetail>();
                List<string> tempTypeName = new List<string>();
                foreach (var item in _viewresult)
                {
                    decimal tempRemainderLeaveHours = 0;
                    if (viewmodels.Count() == 0 || tempTypeName.Contains(item.TypeName) == false)
                    {
                        var tempView = _viewresult.Where(x => x.TypeName == item.TypeName).ToList();
                        var temp = new HolidaySummaryDetail();
                        temp.EachMonth = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }.ToList();
                        tempRemainderLeaveHours = 0;
                        foreach (var items in tempView)
                        {
                            for (var i = 0; i < 12; i++)
                            {
                                temp.EachMonth[i] += items.EachMonth[i];
                                tempRemainderLeaveHours = tempRemainderLeaveHours + items.EachMonth[i];
                            }
                        }
                        temp.TypeName = item.TypeName;
                        temp.ApprovedHours = item.ApprovedHours;
                        temp.AnnualLeaveHours = item.AnnualLeaveHours;
                        //20170818 比照遠百修改，ApprovedHours有可能是""，增加判斷避免錯誤
                        //temp.RemainderLeaveHours = (decimal.Parse(item.AnnualLeaveHours) - decimal.Parse(item.ApprovedHours) - decimal.Parse(item.OverdueHours = string.IsNullOrEmpty(item.OverdueHours) ? "0" : item.OverdueHours) - tempRemainderLeaveHours) + "";// item.RemainderLeaveHours;// this.CalculateReminderAbsentHours(AllLeaveHours, ApprovedHours, eachMonth, OverdueHours);
                        temp.RemainderLeaveHours = (decimal.Parse(item.AnnualLeaveHours) - decimal.Parse(string.IsNullOrEmpty(item.ApprovedHours) ? "0" : item.ApprovedHours) - decimal.Parse(string.IsNullOrEmpty(item.OverdueHours) ? "0" : item.OverdueHours) - tempRemainderLeaveHours) + "";

                        temp.OverdueHours = item.OverdueHours;
                        viewmodels.Add(temp);
                        tempTypeName.Add(item.TypeName);
                    }
                }
                //End Irving
                viewmodel.DetailDatas = viewmodels;
                viewmodel.PersonalDetailDatas = _viewpersonalresult;

                return viewmodel;
        }

        [HttpPost]
        public async Task<ActionResult> SummaryDetail(HolidaySummaryViewModel viewmodel)
        {
            #region 使用者角色
            if (!isGetRole)
            {
                isGetRole = GetRole();
            }
            #endregion

            string HireId = (viewmodel.ChkHireId == true) ? "H05" : " ";
            //set showdata 
            if (!string.IsNullOrWhiteSpace(viewmodel.SelectedYear))
            {
                if (!isAdmin)
                {
                    if (!isHR && CurrentUser.SignDepartments.Count == 0)
                    {
                        //個人
                        viewmodel.SelectedDepartment = CurrentUser.DepartmentCode.ToString();
                        viewmodel.SelectedEmployee = CurrentUser.EmployeeNO.ToString();
                    }
                }

                viewmodel = await GetLeaveData(viewmodel.SelectedEmployee, viewmodel.SelectedDepartment, viewmodel.SelectedStatuslistData, HireId, viewmodel.SelectedYear);
            }
            return PartialView("_SumeryDetail", viewmodel);
        }

        /// <summary>
        /// 取得部門列表
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata)
        {
            //使用者角色
            GetRole();
            if (selecteddata == "")
            {
                selecteddata = CurrentUser.DepartmentCode;
            }
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = null;
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (isHR || isAdmin)
            {
                data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

                #region 管理員 或 人資
                foreach (var item in data)
                {
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                    }
                }
                #endregion
            }
            else
            {
                data = CurrentUser.SignDepartments;

                if (data.Count > 0)
                {
                    #region 主管
                    bool flag = true;//用來判斷簽核主管是否為外同部門人員
                    foreach (var item in data)
                    {
                        if (item.DepartmentCode == CurrentUser.SignDepartmentCode)
                            flag = false;
                        if (getLanguageCookie == "en-US")
                        {
                            listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                        else
                        {
                            listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                        }
                    }

                    if (flag == true)
                    {
                        data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == CurrentUser.SignDepartmentCode).OrderBy(x => x.DepartmentCode).ToList();
                        foreach (var item in data)
                        {
                            if (getLanguageCookie == "en-US")
                            {
                                listItem.Add(new SelectListItem { Text = item.DepartmentEnglishName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                            }
                            else
                            {
                                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                            }
                        }

                    }

                    #endregion
                }
                else
                {
                    #region 其它
                    Department _signdepartment = Services.GetService<DepartmentService>().GetDepartmentByID(CurrentUser.SignDepartmentID);
                    if (getLanguageCookie == "en-US")
                    {
                        listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentEnglishName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    else
                    {
                        listItem.Add(new SelectListItem { Text = _signdepartment.DepartmentName, Value = _signdepartment.DepartmentCode, Selected = (selecteddata == _signdepartment.DepartmentCode ? true : false) });
                    }
                    #endregion
                }
            }

            return listItem;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeetList(string departmentdata, string selecteddata = "")
        {
            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            ViewBag.Role = roleDataa.Name;
            //取得部門
            if (departmentdata == "")
            {
                departmentdata = CurrentUser.DepartmentCode;
            }
            Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentdata && x.Enabled).FirstOrDefault();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Employee> data = new List<Employee>();

            //取得員工列表
            GetRole();
            if (isHR || isAdmin || _department.SignManager.EmployeeNO == CurrentUser.EmployeeNO)
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();
            }
            else if (_department.SignManager.EmployeeNO != CurrentUser.EmployeeNO)
            {
                data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).Where(x => x.EmployeeNO == CurrentUser.EmployeeNO).OrderBy(x => x.EmployeeNO).ToList();
            }

            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });
            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    {
                        listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    }
                }
                else
                {
                    if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    {
                        listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    }
                }
            }
            return listItem;
        }
        /// <summary>
        /// 取得年份列表
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetYearList(string selecteddata = "")
        {
            int _yeardata = 0;
            if (string.IsNullOrWhiteSpace(selecteddata))
            {
                _yeardata = DateTime.Now.AddYears(-2).Year;
            }
            else
            {
                _yeardata = int.Parse(selecteddata);
            }

            List<SelectListItem> listItem = new List<SelectListItem>();

            for (int i = 0; i < 3; i++)
            {
                string showyeardata = (_yeardata + i).ToString();
                listItem.Add(new SelectListItem { Text = showyeardata, Value = showyeardata, Selected = (selecteddata == showyeardata ? true : false) });
            }
            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetEmployee(string DepartmentId,string StatusData)
        {
            List<SelectListItem> result = GetEmployeeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 使用者角色
        /// </summary>
        public bool GetRole()
        {
            bool result = false;

            try
            {
                Role roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();

                if (roleData != null)
                {
                    if (!string.IsNullOrEmpty(roleData.RoleParams))
                    {
                        dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                        isHR = (roleParams.is_hr != null && roleParams.is_hr);
                        isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                    }
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 計算假別剩餘時數 
        /// </summary>
        public decimal CalculateReminderAbsentHours(string AnnualLeaveHours, string ApprovedHours, List<decimal> EachMonth, string OverdueHours)
        {
            if (AnnualLeaveHours == "")
            {
                AnnualLeaveHours = "0.00";
            }
            if(ApprovedHours=="")
            {
                ApprovedHours = "0.00";
            }
            if (OverdueHours == "")
            {
                OverdueHours = "0.00";
            }
            decimal AnnualLeaveHoursDecimal = Convert.ToDecimal(AnnualLeaveHours);
            decimal ApprovedHoursDecimal = Convert.ToDecimal(ApprovedHours);
            decimal OverdueHoursDecimal = Convert.ToDecimal(OverdueHours);
            decimal UseAmount = 0;
            decimal ReminderAbsentHours = 0;
            foreach (decimal item in EachMonth) {
                UseAmount += item;
            }
            ReminderAbsentHours = AnnualLeaveHoursDecimal - ApprovedHoursDecimal - UseAmount - OverdueHoursDecimal;

            return ReminderAbsentHours;
        }
    }
}
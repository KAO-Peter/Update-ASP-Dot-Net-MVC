using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Controllers;
using HRPortal.DBEntities;
using HRPortal.Helper;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.SignFlow.Model;
using HRPortal.SignFlow.SignLists;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.Reports.Controllers
{
    public class DeptLeaveListController : BaseController
    {
        private ReportQueryController ReportQuery = new ReportQueryController();

        public async Task<ActionResult> Index(int page = 1, string DepartmentData = "", string beginDate = "", string endDate = "", string SignStatusdate = "", string EmployeeData = "", string course_category = "", string AbsentCode = "", string StatusData = "")
        {
            if (string.IsNullOrWhiteSpace(EmployeeData))
            {
                EmployeeData = "All";
            }
            if (string.IsNullOrWhiteSpace(DepartmentData))
            {
                DepartmentData = CurrentUser.DepartmentCode;
            }
            SetDefaultData(DepartmentData, beginDate, endDate, SignStatusdate, EmployeeData, course_category, AbsentCode, StatusData);
            int currentPage = page < 1 ? 1 : page;
            if (string.IsNullOrWhiteSpace(beginDate) || string.IsNullOrWhiteSpace(endDate))
            {
                return View();
            }

            string languageCookie = ViewBag.LanguageCookie; //SetDefaultData裡面有設定過了

            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }
            Session["AbsentsDataAll"] = await HRMApiAdapter.GetAllAbsentData("");

            var _result = await Query(DepartmentData, DateTime.Parse(beginDate), DateTime.Parse(endDate), SignStatusdate, EmployeeData, AbsentCode, StatusData, course_category, languageCookie);

            await GetSalaryFormNo(_result,course_category,StatusData);

            return View(_result.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string DepartmentData, string btnQuery, string btnClear, string btnExport, int page = 1, string beginDate = "", string endDate = "", string SignStatusdate = "", string employeedata = "", string course_category = "", string AbsentCode = "", string StatusData ="")
        {
            if (!string.IsNullOrWhiteSpace(btnQuery) || !string.IsNullOrWhiteSpace(btnExport))
            {
                if (string.IsNullOrWhiteSpace(beginDate) || string.IsNullOrWhiteSpace(endDate))
                {
                    TempData["message"] = "起訖日期不能為空白";
                }
                else if (DateTime.Parse(beginDate).Year != DateTime.Parse(endDate).Year)
                {
                    TempData["message"] = "不可跨年查詢";
                }
                else if (DateTime.Parse(beginDate) > DateTime.Parse(endDate))
                {
                    TempData["message"] = "起訖日期錯誤";
                }
                else if (string.IsNullOrWhiteSpace(DepartmentData))
                {
                    TempData["message"] = "請選擇部門";
                }
                else
                {
                    page = 1;
                    return RedirectToAction(string.IsNullOrWhiteSpace(btnQuery) ? "Export" : "Index",
                                            new
                                            {
                                                page,
                                                beginDate,
                                                endDate,
                                                DepartmentData,
                                                SignStatusdate,
                                                employeedata,
                                                course_category,
                                                AbsentCode,
                                                StatusData
                                            });
                }
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
            {
                SetDefaultData(CurrentUser.DepartmentCode);
                return View();
            }
            SetDefaultData(DepartmentData, beginDate, endDate, SignStatusdate, employeedata, course_category, AbsentCode, StatusData);
            return View();
        }

        public async Task<ActionResult> Export(int page = 1, string DepartmentData = "", string beginDate = "", string endDate = "", string SignStatusdate = "", string EmployeeData = "", string course_category = "", string AbsentCode = "", string StatusData = "")
        {
            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }

            //var _result = Query(DepartmentData, DateTime.Parse(beginDate), DateTime.Parse(endDate), SignStatusdate, StatusData);
            //var _result = Query(DepartmentData, DateTime.Parse(beginDate), DateTime.Parse(endDate), SignStatusdate, EmployeeData, AbsentCode, StatusData, course_category);
            List<HRPotralFormSignStatus> _result = await Query(DepartmentData, DateTime.Parse(beginDate), DateTime.Parse(endDate), SignStatusdate, EmployeeData, AbsentCode, StatusData, course_category, languageCookie);
            await GetSalaryFormNo(_result, course_category, StatusData);

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("DeptLeaveList");
            //自動換行
            sheet.Style.Alignment.WrapText = true;
            //凍結窗格
            sheet.SheetView.Freeze(1, 0);
            //水平、垂直置中
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Column(1).Width = 7;
            sheet.Column(2).Width = 12;
            sheet.Column(3).Width = 20;
            sheet.Column(4).Width = 20;
            sheet.Column(5).Width = 25;
            sheet.Column(6).Width = 25;
            sheet.Column(7).Width = 20;
            sheet.Column(8).Width = 20;
            sheet.Row(1).Height = 35;

            //欄位名稱
            string[] listColumn = new string[] { HRPortal.MultiLanguage.Resource.Header_SerialNo,
                                                 HRPortal.MultiLanguage.Resource.Header_EmpID,
                                                 HRPortal.MultiLanguage.Resource.EmployeeName,
                                                 HRPortal.MultiLanguage.Resource.CategoryOfLeave,
                                                 HRPortal.MultiLanguage.Resource.Header_StartDate,
                                                 HRPortal.MultiLanguage.Resource.Header_EndDate,
                                                 HRPortal.MultiLanguage.Resource.Header_ComputingMonth,
                                                 HRPortal.MultiLanguage.Resource.Header_TotalHours };
            int colIdx = 1;
            foreach (string colName in listColumn)
            {
                sheet.Cell(1, colIdx).Value = colName;
                colIdx++;
            }

            //塞資料
            int rowIdy = 2;
            foreach (var item in _result)
            {
                sheet.Row(rowIdy).Height = 35;
                sheet.Cell(rowIdy, 1).Value = rowIdy - 1;
                sheet.Cell(rowIdy, 2).Value = item.SenderEmployeeNo;
                sheet.Cell(rowIdy, 3).Value = languageCookie == "en-US" ? item.SenderEmployeeEnglishName : item.SenderEmployeeName;
                sheet.Cell(rowIdy, 4).Value = item.AbsentCode + "-" + (languageCookie == "en-US" ? item.AbsentEnglishName : item.AbsentName);
                sheet.Cell(rowIdy, 5).Value = item.StartTime + "";
                sheet.Cell(rowIdy, 6).Value = item.EndTime + "";
                sheet.Cell(rowIdy, 7).Value = item.SalaryFormNo;
                item.Amount = item.AbsentUnit == HRPortal.MultiLanguage.Resource.Day ? item.Amount * 8 : item.Amount;
                sheet.Cell(rowIdy, 8).Value = string.Format("{0:0.0}", item.Amount);
                rowIdy++;
            }

            //框線
            sheet.Range(1, 1, rowIdy - 1, 8).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            sheet.Range(1, 1, rowIdy - 1, 8).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            sheet.Range(1, 1, rowIdy - 1, 8).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            sheet.Range(1, 1, rowIdy - 1, 8).Style.Border.RightBorder = XLBorderStyleValues.Thin;

            //輸出Excel
            MemoryStream excelStream = new MemoryStream();
            workbook.SaveAs(excelStream);
            excelStream.Position = 0;
            string exportFileName = string.Concat("DeptLeaveList_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);
        }

        private void SetDefaultData(string departmentdata = "", string beginDate = "", string endDate = "", string SignStatusdate = "", string employeedata = "", string course_category = "", string AbsentCode = "", string StatusData = "")
        {
            ViewBag.SignStatusdate = SignStatusdate;

            if (string.IsNullOrWhiteSpace(beginDate))
            {
                String yy = DateTime.Now.Year.ToString();
                DateTime FirstDay = DateTime.Parse(yy + "/" + "01" + "/01");
                ViewBag.StartTime = FirstDay.ToString("yyyy/MM/dd");
            }
            else
            {
                ViewBag.StartTime = beginDate;
            }
            if (string.IsNullOrWhiteSpace(endDate))
            {
                ViewBag.EndTime = DateTime.Now.ToString("yyyy/MM/dd");
            }
            else
            {
                ViewBag.EndTime = endDate;
            }

            bool role = (this.CurrentUser.IsHR || this.CurrentUser.IsAdmin || this.CurrentUser.SignDepartments.Count > 0);
            ViewData["DepartmentList"] = ReportQuery.GetDepartmentList(this.CurrentUser.CompanyID, departmentdata, false, role, this.CurrentUser.SignDepartments.Count > 0 ? this.CurrentUser.SignDepartments : this.CurrentUser.Departments);
            ViewBag.DepartmentData = departmentdata;
            ViewData["SignStatuslist"] = GetSignStatusList(SignStatusdate);

            ViewData["StatuslistData"] = GetStatusDataList(StatusData);
            ViewBag.StatusData = StatusData;
            ViewData["EmployeeList"] = GetEmployeetList(departmentdata, StatusData, employeedata);
            ViewBag.EmployeeData = employeedata;
            ViewData["FormTypelist"] = GetFormTypeList(course_category);
            ViewBag.course_category = course_category;
            ViewData["AbsentCodeList"] = GetAbsentCodeList(AbsentCode);

            if (course_category == "Leave")
            {
                ViewBag.AbsentCode = AbsentCode;
            }
            else
            {
                ViewBag.AbsentCode = "";
            }

            ViewBag.LanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
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
                listItem.Add(new SelectListItem { Text = "Signing", Value = "WS", Selected = (selecteddata == "WS" ? true : false) });
                listItem.Add(new SelectListItem { Text = "Verify", Value = "A", Selected = (selecteddata == "A" ? true : false) });
            }
            else
            {
                listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
                listItem.Add(new SelectListItem { Text = "未簽核", Value = "WS", Selected = (selecteddata == "WS" ? true : false) });
                listItem.Add(new SelectListItem { Text = "已簽核", Value = "A", Selected = (selecteddata == "A" ? true : false) });
            }
            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }

        public async Task<List<HRPotralFormSignStatus>> Query(string departmentcode, DateTime startTime, DateTime endTime, string SignStatusdate, string employee = "", string AbsentCode = "", string StatusData = "", string formtype = "", string LanguageCookie = "zh-TW")
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {

                List<HRPotralFormSignStatus> _result = new List<HRPotralFormSignStatus>();

                if (employee == "All")
                {
                    _result = _queryHelper.GetCurrentSignListByDepartmentt(CurrentUser.CompanyCode, departmentcode, StatusData, false, startTime, endTime.AddDays(1));
                }
                else if (!string.IsNullOrWhiteSpace(employee) && employee != "All")
                {
                    _result = _queryHelper.GetCurrentSignListByEmployee(CurrentUser.CompanyCode, employee, false, startTime, endTime.AddDays(1));
                }
                else if (!string.IsNullOrWhiteSpace(departmentcode))
                {
                    _result = _queryHelper.GetCurrentSignListByDepartment(CurrentUser.CompanyCode, departmentcode, false, startTime, endTime.AddDays(1));
                }

                #region 判斷在職離職
                //取得部門
                Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentcode && x.Enabled).FirstOrDefault();
                List<Employee> empData = new List<Employee>();
                if (StatusData == "")
                {
                    _result = _result.Where(x => x.LeaveDate == null).ToList();
                }
                else if (StatusData == "L")
                {
                    _result = _result.Where(x => x.LeaveDate >= startTime).ToList();
                }
                #endregion

                #region 判斷假別
                if (AbsentCode != "")
                {
                    _result = _result.Where(x => x.AbsentCode == AbsentCode).ToList();
                }
                #endregion

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
                if (SignStatusdate == "A")
                {
                    _result = _result.Where(x => x.SignStatus == "A").OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                }
                else if (SignStatusdate == "W")
                {
                    _result = _result.Where(x => x.SignType == "S" && x.SignStatus == "W").OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                }
                else if (SignStatusdate == "WS")
                {
                    _result = _result.Where(x => x.SignType == "P" && x.SignStatus == "W").OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                }

                //FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentType>)Session["Absents"]);
                FormSummaryBuilderr _summaryBuilder = new FormSummaryBuilderr((List<AbsentDetail>)Session["AbsentsData"], (List<AbsentDetail>)Session["AbsentsDataAll"]);

                _result.RemoveAll(item => _summaryBuilder.BuildSummaryForFormQuery(item, AbsentCode));

                foreach (HRPotralFormSignStatus _item in _result)
                {
                    _summaryBuilder.BuildSummary(_item);
                }

                #region
                /*
                if (employee != "All")
                {
                    Employee empInfo = Services.GetService<EmployeeService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == employee).FirstOrDefault();
                    departmentcode = empInfo.Department.DepartmentCode;
                }
                //var _absent = await HRMApiAdapter.GetEmployeeAbsent(CurrentUser.Employee.Company.CompanyCode, detail.EmpNo, selectDate);   
                //var _absent = await HRMApiAdapter.GetAllAbsent(CurrentUser.CompanyCode, LanguageCookie);
                
                if (formtype == "" || formtype == "Leave")
                {
                    if (employee == "All")
                    {
                        #region Select ALL

                        //Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentcode && x.Enabled).FirstOrDefault();

                        List<Employee> data = new List<Employee>();

                        data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).
                            Where(x => x.LeaveDate == null || x.LeaveDate > DateTime.Now).OrderBy(x => x.EmployeeNO).ToList();

                        foreach (var items in data)
                        {
                            if (items.LeaveDate == null || items.LeaveDate > DateTime.Now)
                            {
                                var _DepartmentCode = Services.GetService<DepartmentService>().GetDepartmentByID(items.DepartmentID).DepartmentCode;
                                List<AbsentFormData> absentFormDataList_all = await HRMApiAdapter.GetAbsentFormData(CurrentUser.CompanyCode, items.EmployeeNO, _DepartmentCode, StatusData, startTime, endTime, LanguageCookie);

                                //20170510 Start 增加 by Daniel
                                //過濾假別
                                FilterLeaveFormByAbsentCode(ref absentFormDataList_all, AbsentCode);
                                //20170510 End

                                string tempFormNo = "0";
                                int formNoOrder = 0;

                                foreach (var item in absentFormDataList_all)
                                {
                                    //var absentData = _absent.Where(x=>x.)
                                    if (tempFormNo != item.FormNo)
                                    {
                                        formNoOrder = 0;
                                    }
                                    else if (tempFormNo == item.FormNo)
                                    {
                                        formNoOrder++;
                                    }
                                    tempFormNo = item.FormNo;
                                    _result.Add(new HRPotralFormSignStatus()
                                    {
                                        FormType = FormType.Leave,
                                        FormCreateDate = item.CreateDate,
                                        CompanyCode = CurrentUser.CompanyCode,
                                        CompanyName = CurrentUser.CompanyName,
                                        DepartmentCode = item.DeptCode,
                                        DepartmentName = item.DeptName,
                                        SenderEmployeeNo = item.EmpID,
                                        SenderEmployeeName = item.EmpName,
                                        FormSummary = item.Summary,
                                        FormNo = item.FormNo,
                                        FormNoOrder = formNoOrder,
                                        BeDate = item.BeginTime,
                                        AbsentCode = item.AbsentCode,
                                        AbsentName = item.AbsentName,
                                        AbsentUnit = item.AbsentUnit == "d" ? HRPortal.MultiLanguage.Resource.Day : HRPortal.MultiLanguage.Resource.Hour,
                                        StartTime = item.BeginTime,
                                        EndTime = item.EndTime,
                                        SalaryFormNo = item.Summary,
                                        Amount = decimal.Parse(item.AbsentAmount + ""),
                                        DepartmentEnglishName = item.DeptNameEN,
                                        SenderEmployeeEnglishName = item.EmpNameEN,
                                        AbsentEnglishName=item.AbsentNameEN
                                    });
                                }

                            }
                        }

                        #endregion
                    }
                    else
                    {
                        List<AbsentFormData> absentFormDataList = await HRMApiAdapter.GetAbsentFormData(CurrentUser.CompanyCode, employee, departmentcode, StatusData, startTime, endTime, LanguageCookie);

                        //20170510 Start 增加 by Daniel
                        //過濾假別
                        FilterLeaveFormByAbsentCode(ref absentFormDataList, AbsentCode);
                        //20170510 End

                        string tempFormNo = "0";
                        int formNoOrder = 0;
                        foreach (var item in absentFormDataList)
                        {
                            //var absentData = _absent.Where(x => x.Code == item.AbsentCode).FirstOrDefault();
                            if (tempFormNo != item.FormNo)
                            {
                                formNoOrder = 0;
                            }
                            else if (tempFormNo == item.FormNo)
                            {
                                formNoOrder++;
                            }
                            tempFormNo = item.FormNo;
                            _result.Add(new HRPotralFormSignStatus()
                            {
                                FormType = FormType.Leave,
                                FormCreateDate = item.CreateDate,
                                CompanyCode = CurrentUser.CompanyCode,
                                CompanyName = CurrentUser.CompanyName,
                                DepartmentCode = item.DeptCode,
                                DepartmentName = item.DeptName,
                                SenderEmployeeNo = item.EmpID,
                                SenderEmployeeName = item.EmpName,
                                FormSummary = item.Summary,
                                FormNo = item.FormNo,
                                FormNoOrder = formNoOrder,
                                BeDate = item.BeginTime,
                                AbsentCode = item.AbsentCode,
                                AbsentName = item.AbsentName,
                                AbsentUnit = item.AbsentUnit == "d" ? HRPortal.MultiLanguage.Resource.Day : HRPortal.MultiLanguage.Resource.Hour,
                                StartTime = item.BeginTime,
                                EndTime = item.EndTime,
                                SalaryFormNo = item.Summary,
                                Amount = decimal.Parse(item.AbsentAmount + ""),
                                DepartmentEnglishName = item.DeptNameEN,
                                SenderEmployeeEnglishName = item.EmpNameEN,
                                AbsentEnglishName = item.AbsentNameEN
                            });
                        }
                    }
                }
                if (formtype == "" || formtype == "OverTime")
                {
                    if (employee == "All")
                    {
                        //Department _department = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.DepartmentCode == departmentcode && x.Enabled).FirstOrDefault();

                        List<Employee> data = new List<Employee>();

                        data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();

                        foreach (var items in data)
                        {
                            if (items.LeaveDate == null || items.LeaveDate > DateTime.Now)
                            {
                                var _DepartmentCode = Services.GetService<DepartmentService>().GetDepartmentByID(items.DepartmentID).DepartmentCode;
                                //20180228 Daniel 修正查詢邏輯，原來每個人都會查一次部門全部的資料
                                //List<AbsentFormData> overtimeFormDataList_all = await HRMApiAdapter.GetOverTimeFormData(CurrentUser.CompanyCode, employee, _DepartmentCode, StatusData, startTime, endTime);
                                List<AbsentFormData> overtimeFormDataList_all = await HRMApiAdapter.GetOverTimeFormData(CurrentUser.CompanyCode, items.EmployeeNO, _DepartmentCode, StatusData, startTime, endTime, LanguageCookie);
                                foreach (var item in overtimeFormDataList_all)
                                {
                                    _result.Add(new HRPotralFormSignStatus()
                                    {
                                        FormType = FormType.OverTime,
                                        FormCreateDate = item.CreateDate,
                                        CompanyCode = CurrentUser.CompanyCode,
                                        CompanyName = CurrentUser.CompanyName,
                                        DepartmentCode = item.DeptCode,
                                        DepartmentName = item.DeptName,
                                        SenderEmployeeNo = item.EmpID,
                                        SenderEmployeeName = item.EmpName,
                                        FormSummary = item.Summary,
                                        FormNo = item.FormNo,
                                        FormNoOrder = 0,
                                        BeDate = item.BeginTime,
                                        AbsentCode = "OT",
                                        AbsentName = HRPortal.MultiLanguage.Resource.OverTime,
                                        AbsentUnit = HRPortal.MultiLanguage.Resource.Hour,
                                        StartTime = item.BeginTime,
                                        EndTime = item.EndTime,
                                        SalaryFormNo = item.Summary,
                                        Amount = decimal.Parse(item.OvertimeAmount + ""),
                                        DepartmentEnglishName = item.DeptNameEN,
                                        SenderEmployeeEnglishName = item.EmpNameEN,
                                        AbsentEnglishName = item.AbsentNameEN
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        List<AbsentFormData> overtimeFormDataList = await HRMApiAdapter.GetOverTimeFormData(CurrentUser.CompanyCode, employee, departmentcode, StatusData, startTime, endTime, LanguageCookie);

                        foreach (var item in overtimeFormDataList)
                        {
                            _result.Add(new HRPotralFormSignStatus()
                            {
                                FormType = FormType.OverTime,
                                FormCreateDate = item.CreateDate,
                                CompanyCode = CurrentUser.CompanyCode,
                                CompanyName = CurrentUser.CompanyName,
                                DepartmentCode = item.DeptCode,
                                DepartmentName = item.DeptName,
                                SenderEmployeeNo = item.EmpID,
                                SenderEmployeeName = item.EmpName,
                                FormSummary = item.Summary,
                                FormNo = item.FormNo,
                                FormNoOrder = 0,
                                BeDate = item.BeginTime,
                                AbsentCode = "OT",
                                AbsentName = HRPortal.MultiLanguage.Resource.OverTime,
                                AbsentUnit = HRPortal.MultiLanguage.Resource.Hour,
                                StartTime = item.BeginTime,
                                EndTime = item.EndTime,
                                SalaryFormNo = item.Summary,
                                Amount = decimal.Parse(item.OvertimeAmount + ""),
                                DepartmentEnglishName = item.DeptNameEN,
                                SenderEmployeeEnglishName = item.EmpNameEN,
                                AbsentEnglishName = item.AbsentNameEN
                            });
                        }
                    }
                }
                */
                #endregion
                //List<HRPotralFormSignStatus> _resultOrd = new List<HRPotralFormSignStatus>();
                //_resultOrd = _result.OrderByDescending(x => x.StartTime);
                return _result.Distinct().OrderByDescending(x => x.StartTime).ToList();
            }
        }
        
        /// <summary>
        /// 讀取薪資批號
        /// </summary>
        /// <param name="_result"></param>
        /// <param name="course_category"></param>
        /// <param name="StatusData"></param>
        /// <returns></returns>
        private static async Task GetSalaryFormNo(IEnumerable<HRPotralFormSignStatus> _result, string course_category = "",string StatusData = null)
        {
            var _employee = _result.GroupBy(x => new { x.CompanyCode, x.SenderEmployeeNo,x.FormNo }).ToDictionary(g => new { g.Key.CompanyCode, g.Key.SenderEmployeeNo,g.Key.FormNo });

            foreach (var emp in _employee)
            {
                var DeptLeaveList = _result.Where(x => x.SenderEmployeeNo == emp.Key.SenderEmployeeNo);
                
                foreach (var item in DeptLeaveList)
                {
                    //var salaryFormNo = await HRMApiAdapter.GetLeaveSalaryFormNo(emp.Key.CompanyCode, item.FormNo, item.FormType + "");
                    //item.SalaryFormNo = salaryFormNo.Count == 0 ? "" : salaryFormNo[0].SalaryYM + "";
                    string startTime = item.StartTime == null ? item.BeDate + "" : item.StartTime + "";
                    item.SalaryFormNo = Convert.ToDateTime(startTime).ToString("yyyyMM");
                }
            }
        }

        //個人假單查詢增加假別篩選
        /// <summary>
        /// 取得所有假別清單
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAbsentCodeList(string AbsentCode = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            if (Session["Absents"] != null)
            {
                /*
                Dictionary<string, string> dictAbsents = Session["Absents"] as Dictionary<string, string>;
                listItem.Add(new SelectListItem { Text = "全部", Value = "" });
                foreach (var item in dictAbsents)
                {
                    listItem.Add(new SelectListItem { Text = item.Value, Value = item.Key.Trim(), Selected = (item.Key.Trim() == AbsentCode ? true : false) });
                }
                */
                List<AbsentType> listAbsents = (List<AbsentType>)Session["Absents"];
                listItem.Add(new SelectListItem { Text = "全部", Value = "" });
                foreach (var item in listAbsents)
                {
                    listItem.Add(new SelectListItem { Text = item.AbsentName, Value = item.AbsentCode.Trim(), Selected = (item.AbsentCode.Trim() == AbsentCode ? true : false) });
                }
            }
            return listItem;
        }

        /// <summary>
        /// 取得簽核類別
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetFormTypeList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";
            if (getLanguageCookie == "en-US")
            {
                listItem.Add(new SelectListItem { Text = "All", Value = "", Selected = (selecteddata == "" ? true : false) });
            }
            else
            {
                listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
            }

            OptionListHelper _optionListHelper = new OptionListHelper();
            listItem.AddRange(_optionListHelper.GetOptionList("FormType"));
            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;

            return listItem;
        }

        /// <summary>
        /// 取得員工列表
        /// </summary>
        /// <param name="departmentdata">被選取的部門</param>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetEmployeetList(string departmentdata, string StatusData = "", string selecteddata = "")
        {
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
            data = Services.GetService<EmployeeService>().GetListsOfSignDepartment(CurrentUser.CompanyID, _department.ID).OrderBy(x => x.EmployeeNO).ToList();

            listItem.Add(new SelectListItem { Text = MultiLanguage.Resource.All, Value = "All", Selected = (selecteddata == "All" ? true : false) });

            if (StatusData == "")
            {
                data = data.Where(x => x.LeaveDate == null).ToList();
            }
            else if (StatusData == "L")
            {
                data = data.Where(x => x.LeaveDate <= DateTime.Now).ToList();
            }

            foreach (var item in data)
            {
                if (getLanguageCookie == "en-US")
                {
                    //if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    //{
                        listItem.Add(new SelectListItem { Text = item.EmployeeEnglishName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    //}
                }
                else
                {
                    //if (item.LeaveDate == null || item.LeaveDate > DateTime.Now)
                    //{
                        listItem.Add(new SelectListItem { Text = item.EmployeeName, Value = item.EmployeeNO, Selected = (selecteddata == item.EmployeeNO ? true : false) });
                    //}
                }
            }
            return listItem;
        }
        
        /// <summary>
        /// 給下拉式選單讀取員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetEmployee(string DepartmentId, string StatusData = "")
        {
            List<SelectListItem> result = GetEmployeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 給下拉式選單讀取在離職員工列表
        /// </summary>
        /// <param name="DepartmentId"></param>
        /// <returns></returns>
        public ActionResult GetStatusData(string DepartmentId, string StatusData)
        {
            List<SelectListItem> result = GetEmployeetList(DepartmentId, StatusData);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得員工在離職狀態 
        /// </summary>
        /// <param name="selecteddata"></param>
        /// <returns></returns>
        private List<SelectListItem> GetStatusDataList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = HRPortal.MultiLanguage.Resource.Option_ShowActiveEmployees, Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = HRPortal.MultiLanguage.Resource.Option_ShowInActiveEmployees, Value = "L", Selected = (selecteddata == "L" ? true : false) });
            listItem.Add(new SelectListItem { Text = HRPortal.MultiLanguage.Resource.Option_ShowAllEmployees, Value = "ALL", Selected = (selecteddata == "ALL" ? true : false) });

            return listItem;
        }

        private void FilterLeaveFormByAbsentCode(ref List<AbsentFormData> listAbsentFormData, string AbsentCode)
        {
            if (!string.IsNullOrWhiteSpace(AbsentCode))
            {
                listAbsentFormData = listAbsentFormData.Where(x => x.AbsentCode == AbsentCode).ToList();
            }
        }
    }
}
using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC.Controllers
{
    public class AnnualLeaveScheduleReportController : MultiDepEmpController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string EmployeeData = "", string DepartmentData = "", string YearData = "")
        {
            AnnualLeaveScheduleReportViewModel viewmodel = new AnnualLeaveScheduleReportViewModel();

            Guid? DepartmentIDData = null;

            if (CurrentUser.SignDepartments.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(DepartmentData))
                {
                    DepartmentIDData = CurrentUser.SignDepartmentID;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(DepartmentData))
                {
                    DepartmentIDData = CurrentUser.DepartmentID;
                }
            }
            if (string.IsNullOrWhiteSpace(EmployeeData))
            {
                EmployeeData = CurrentUser.EmployeeNO;
            }

            if (string.IsNullOrWhiteSpace(YearData))
            {
                YearData = DateTime.Today.Year.ToString();
            }

            var deptData = GetDepartmentList2(DepartmentIDData);
            foreach (var item in deptData)
            {
                item.Selected = item.Value == DepartmentIDData.ToString() ? true : false;               
            }
            string showExcludeDeptStr = Services.GetService<SystemSettingService>().GetSettingValue("ShowExcludeDept");
            string[] excludeDeptAry = showExcludeDeptStr.Split(';');
            var excludeDeptID = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => excludeDeptAry.Contains(x.DepartmentCode)).Select(s => s.ID.ToString()).ToList();
            viewmodel.DepartmentListData = deptData.Where(x => !excludeDeptID.Contains(x.Value));// await GetDepartmentList2(DepartmentIDData);
            viewmodel.SelectedDepartment = DepartmentIDData.ToString();


            var empData = GetEmployeetList2(DepartmentIDData.ToString(), EmployeeData);
            foreach (var item in empData)
            {
                item.Selected = true;
            }
            viewmodel.EmployeeListData = empData;// await GetEmployeetList2(DepartmentIDData.ToString(), EmployeeData);
            viewmodel.SelectedEmployee = EmployeeData;

            viewmodel.YearListData = GetYearList(YearData);
            viewmodel.SelectedYear = YearData;

            ViewBag.isALL = isAdmin || isHR;

            int DeferredMonth = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeferredLeavePeriod"));
            ViewBag.MonthCount = DeferredMonth;

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Index(AnnualLeaveScheduleReportViewModel viewmodel, string btnQuery, string btnClear)
        {
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                return RedirectToAction("Index", new
                {
                    DepartmentData = "",
                    EmployeeData = ""
                });
            }
            return View(viewmodel);
        }

        [HttpPost]
        public async Task<ActionResult> GetAnnualLeaveSchedule(AnnualLeaveScheduleReportViewModel viewmodel)
        {
            AnnualLeaveSummaryViewModel _viewresult = await GetAnnualLeaveData(viewmodel);
            return PartialView("_AnnualLeaveSchedule", _viewresult);
        }

        #region 匯出Excel
        [HttpPost]
        public async Task<ActionResult> Export(AnnualLeaveScheduleReportViewModel viewmodel)
        {
            AnnualLeaveSummaryViewModel _viewresult = await GetAnnualLeaveData(viewmodel);

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("AnnualLeaveScheduleReport");
            //自動換行
            sheet.Style.Alignment.WrapText = true;

            //水平、垂直置中
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //欄寬
            sheet.Column(1).Width = 10;
            sheet.Columns(2, 5).Width = 5;
            sheet.Columns(6, 6).Width = 8;
            sheet.Columns(7, 30).Width = 5;

            string[] listColumn = new string[] { "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月" };

            #region 塞資料

            int rowIdx = 1;
            if (_viewresult.DeptDetailDatas != null)
            {
                sheet.Row(rowIdx).Height = 30;
                sheet.Range(rowIdx, 1, rowIdx, 30).Merge();
                sheet.Cell(rowIdx, 1).Value = "特休排程計劃達成表";
                sheet.Range(rowIdx, 1, rowIdx, 1).Style.Font.SetBold(true);
                sheet.Range(rowIdx, 1, rowIdx, 1).Style.Font.SetFontSize(18);

                sheet.Row(rowIdx + 1).Height = 20;
                sheet.Cell(rowIdx + 1, 1).Value = string.Format("年度：{0}年", _viewresult.Year);
                sheet.Range(rowIdx + 1, 1, rowIdx + 1, 30).Merge();

                sheet.Range(rowIdx + 1, 1, rowIdx + 1, 2).Style.Font.SetFontSize(12);
                sheet.Range(rowIdx + 1, 1, rowIdx + 1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                rowIdx += 2;
                foreach (var DeptDetailDatas in _viewresult.DeptDetailDatas)
                {
                    int rowDept = rowIdx;
                    //部門
                    sheet.Row(rowIdx).Height = 20;
                    sheet.Cell(rowIdx, 1).Value = string.Format("部門：{0} {1}", DeptDetailDatas.DeptCode, DeptDetailDatas.DeptName);
                    sheet.Range(rowIdx, 1, rowIdx, 30).Merge();
                    sheet.Range(rowIdx, 1, rowIdx, 2).Style.Font.SetFontSize(12);
                    sheet.Range(rowIdx, 1, rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    rowIdx++;

                    #region 欄位名稱

                    sheet.Range(rowIdx, 1, rowIdx + 1, 1).Merge();
                    sheet.Cell(rowIdx, 1).Style.Fill.SetBackgroundColor(XLColor.Pink);
                    sheet.Cell(rowIdx, 1).Value = "員工姓名";

                    sheet.Range(rowIdx, 2, rowIdx, 3).Merge();
                    sheet.Cell(rowIdx, 2).Value = "休假總計";
                    sheet.Cell(rowIdx + 1, 2).Value = "預排";
                    sheet.Cell(rowIdx + 1, 3).Value = "實際";
                    sheet.Range(rowIdx, 2, rowIdx + 1, 3).Style.Fill.SetBackgroundColor(XLColor.LightSeaGreen);

                    sheet.Range(rowIdx, 4, rowIdx, 6).Merge();
                    sheet.Cell(rowIdx, 4).Value = "剩餘時數";
                    sheet.Cell(rowIdx + 1, 4).Value = "遞延\n特休";
                    sheet.Cell(rowIdx + 1, 5).Value = "特休";
                    sheet.Cell(rowIdx + 1, 6).Value = "已休\n達成率";
                    sheet.Range(rowIdx, 4, rowIdx + 1, 6).Style.Fill.SetBackgroundColor(XLColor.Yellow);

                    for (int i = 1; i <= 12; i++)
                    {
                        sheet.Range(rowIdx, (2 * i) + 5, rowIdx, (2 * i) + 6).Merge();
                        sheet.Cell(rowIdx, (2 * i) + 5).Value = listColumn[i - 1];
                        sheet.Cell(rowIdx + 1, (2 * i) + 5).Value = "預排";
                        sheet.Cell(rowIdx + 1, (2 * i) + 6).Value = "實際";
                    }

                    sheet.Range(rowIdx, 7, rowIdx + 1, 30).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    //框線
                    sheet.Range(rowIdx, 1, rowIdx + 1, 30).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    sheet.Range(rowIdx, 1, rowIdx + 1, 30).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    sheet.Range(rowIdx, 1, rowIdx + 1, 30).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    sheet.Range(rowIdx, 1, rowIdx + 1, 30).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    rowIdx += 2;

                    #endregion 欄位名稱

                    #region 員工特休資料

                    double expected = 0;
                    decimal actual = 0;
                    int lastCol = 0;
                    foreach (var EmpDetailDatas in DeptDetailDatas.EmpDetailDatas)
                    {
                        sheet.Cell(rowIdx, 1).Value = EmpDetailDatas.EmployeeName;
                        for (int i = 1; i <= 12; i++)
                        {
                            sheet.Cell(rowIdx, (2 * i) + 5).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                            sheet.Cell(rowIdx, (2 * i) + 5).Value = EmpDetailDatas.ALEachMonth[i - 1];
                            //sheet.Cell(rowIdx, (2 * i) + 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            sheet.Cell(rowIdx, (2 * i) + 6).Value = EmpDetailDatas.EachMonth[i - 1];
                            sheet.Cell(rowIdx, (2 * i) + 6).Style.Fill.SetBackgroundColor(XLColor.LightPink);
                            //sheet.Cell(rowIdx, (2 * i) + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            lastCol = (2 * i) + 6;
                        }
                        expected = EmpDetailDatas.ALEachMonth.Sum();
                        actual = EmpDetailDatas.EachMonth.Sum();
                        sheet.Cell(rowIdx, 2).Value = expected;
                        sheet.Cell(rowIdx, 3).Value = actual;
                        sheet.Cell(rowIdx, 4).Value = EmpDetailDatas.RemainingDeferredLeaveHours;
                        sheet.Cell(rowIdx, 5).Value = EmpDetailDatas.RemainingLeaveHours;
                        sheet.Cell(rowIdx, 6).Value = expected == 0 ? 0 + "％" : actual == 0 ? 0 + "％" : Math.Round(((double)actual / expected) * 100, 2) + "％";

                        sheet.Range(rowIdx, 2, rowIdx, lastCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        //框線
                        sheet.Range(rowIdx, 1, rowIdx, 30).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        sheet.Range(rowIdx, 1, rowIdx, 30).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        sheet.Range(rowIdx, 1, rowIdx, 30).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        sheet.Range(rowIdx, 1, rowIdx, 30).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        rowIdx++;
                    }

                    #endregion 員工特休資料

                    //空一行
                    sheet.Row(rowIdx).Height = 20;
                    sheet.Range(rowIdx, 1, rowIdx, 30).Merge();
                    rowIdx++;
                }
            }

            #endregion 塞資料

            //輸出Excel
            MemoryStream excelStream = new MemoryStream();
            workbook.SaveAs(excelStream);
            excelStream.Position = 0;
            string exportFileName = string.Concat("AnnualLeaveScheduleReport_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);
        }

        #endregion

        #region 讀取計畫表資料

        /// <summary>
        /// 取得年份列表
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetYearList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            for (int i = -2; i < 2; i++)
            {
                string showyeardata = (DateTime.Today.Year + i).ToString();
                listItem.Add(new SelectListItem { Text = showyeardata, Value = showyeardata, Selected = (showyeardata == selecteddata) });
            }
            return listItem;
        }

        private async Task<AnnualLeaveSummaryViewModel> GetAnnualLeaveData(AnnualLeaveScheduleReportViewModel viewmodel)
        {
            AnnualLeaveSummaryViewModel _viewresult = new AnnualLeaveSummaryViewModel();
            _viewresult.DeptDetailDatas = new List<AnnualLeaveDeptSummary>();
            _viewresult.Year = viewmodel.SelectedYear;

            int year = int.Parse(viewmodel.SelectedYear);
            var empALSchedule = Services.GetService<EmpALScheduleService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.Year == year);

            DeptEmpLeaveSummaryItem _result = await HRMApiAdapter.GetAnnualLeaveSummary(year, CurrentUser.CompanyCode, viewmodel.EmpID);

            if (_result != null && _result.PersonalDetailDatas != null)
            {
                var deptGroup = _result.PersonalDetailDatas.GroupBy(x => x.DeptCode).Select(x => x.Key).ToList();

                foreach (var dept in deptGroup)
                {
                    var personalDetailDatas = _result.PersonalDetailDatas.Where(x => x.DeptCode == dept).ToList();

                    AnnualLeaveDeptSummary deptSummary = new AnnualLeaveDeptSummary()
                    {
                        DeptCode = personalDetailDatas[0].DeptCode,
                        DeptName = personalDetailDatas[0].DeptName,
                        EmpDetailDatas = new List<AnnualLeaveEmpSummary>()
                    };

                    foreach (var pdd in personalDetailDatas)
                    {
                        AnnualLeaveEmpSummary empSummary = new AnnualLeaveEmpSummary();
                        empSummary.EmployeeName = pdd.EmpName;
                        empSummary.EmployeeNo = pdd.EmpNo;
                        empSummary.EachMonth = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        empSummary.DefMonth = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        empSummary.ALEachMonth = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        empSummary.RemainingLeaveHours = 0;
                        empSummary.RemainingDeferredLeaveHours = 0;
                        empSummary.LeaMonth = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                        if (pdd.SummaryDetail.Count() > 0)
                        {
                            foreach (var detail in pdd.SummaryDetail)
                            {
                                empSummary.EachMonth[0] += detail.EachMonth[0];
                                empSummary.EachMonth[1] += detail.EachMonth[1];
                                empSummary.EachMonth[2] += detail.EachMonth[2];
                                empSummary.EachMonth[3] += detail.EachMonth[3];
                                empSummary.EachMonth[4] += detail.EachMonth[4];
                                empSummary.EachMonth[5] += detail.EachMonth[5];
                                empSummary.EachMonth[6] += detail.EachMonth[6];
                                empSummary.EachMonth[7] += detail.EachMonth[7];
                                empSummary.EachMonth[8] += detail.EachMonth[8];
                                empSummary.EachMonth[9] += detail.EachMonth[9];
                                empSummary.EachMonth[10] += detail.EachMonth[10];
                                empSummary.EachMonth[11] += detail.EachMonth[11];

                                if (detail.AbsentCode == "01")
                                {
                                    empSummary.LeaMonth[0] += detail.EachMonth[0];
                                    empSummary.LeaMonth[1] += detail.EachMonth[1];
                                    empSummary.LeaMonth[2] += detail.EachMonth[2];
                                    empSummary.LeaMonth[3] += detail.EachMonth[3];
                                    empSummary.LeaMonth[4] += detail.EachMonth[4];
                                    empSummary.LeaMonth[5] += detail.EachMonth[5];
                                    empSummary.LeaMonth[6] += detail.EachMonth[6];
                                    empSummary.LeaMonth[7] += detail.EachMonth[7];
                                    empSummary.LeaMonth[8] += detail.EachMonth[8];
                                    empSummary.LeaMonth[9] += detail.EachMonth[9];
                                    empSummary.LeaMonth[10] += detail.EachMonth[10];
                                    empSummary.LeaMonth[11] += detail.EachMonth[11];

                                    empSummary.RemainingLeaveHours = System.Convert.ToDecimal(detail.HolidayData.CanUseCountInRange) - empSummary.LeaMonth.Sum();
                                }
                                else if (detail.AbsentCode == "DD1")
                                {
                                    empSummary.DefMonth[0] += detail.EachMonth[0];
                                    empSummary.DefMonth[1] += detail.EachMonth[1];
                                    empSummary.DefMonth[2] += detail.EachMonth[2];
                                    empSummary.DefMonth[3] += detail.EachMonth[3];
                                    empSummary.DefMonth[4] += detail.EachMonth[4];
                                    empSummary.DefMonth[5] += detail.EachMonth[5];
                                    empSummary.DefMonth[6] += detail.EachMonth[6];
                                    empSummary.DefMonth[7] += detail.EachMonth[7];
                                    empSummary.DefMonth[8] += detail.EachMonth[8];
                                    empSummary.DefMonth[9] += detail.EachMonth[9];
                                    empSummary.DefMonth[10] += detail.EachMonth[10];
                                    empSummary.DefMonth[11] += detail.EachMonth[11];
                                    empSummary.RemainingDeferredLeaveHours = System.Convert.ToDecimal(detail.HolidayData.CanUseCountInRange) - empSummary.DefMonth.Sum();
                                }
                            }
                        }

                        var als = empALSchedule.Where(x => x.Employees.EmployeeNO == pdd.EmpNo).FirstOrDefault();
                        if (als != null)
                        {
                            empSummary.ALEachMonth = new List<double> { als.S01 + als.D01, als.S02 + als.D02, als.S03 + als.D03, als.S04 + als.D04, als.S05 + als.D05, als.S06 + als.D06, 
                                als.S07+ als.D07, als.S08+ als.D08, als.S09+ als.D09, als.S10+ als.D10, als.S11+ als.D11, als.S12+ als.D12 };
                        }
                        deptSummary.EmpDetailDatas.Add(empSummary);
                    }
                    _viewresult.DeptDetailDatas.Add(deptSummary);
                }
            }
            return _viewresult;
        }

        [HttpGet]
        public async Task<ActionResult> Schedule(string SelectedYear = "")
        {
            AnnualLeaveScheduleModel viewmodel = new AnnualLeaveScheduleModel();

            int year = 0;
            int.TryParse(SelectedYear, out year);
            if (year <= 0) year = DateTime.Today.Year;
            DateTime chkDate = DateTime.Parse(year.ToString() + "-12-31");

            AbsentDetailAll data = await HRMApiAdapter.GetEmployeeAbsent2(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, chkDate, "remaining");// UsedStatusFilterType.NotUsedUp);

            int DeferredMonth = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeferredLeavePeriod"));

            DateTime tempDate = new DateTime(year, DeferredMonth, 1, 0, 0, 0);
            tempDate = (tempDate.AddMonths(1).AddDays(-1));
            ViewBag.Message = "";// "(遞延特休建議" + tempDate.Month + "月" + tempDate.Day + "日前休畢)";
            ViewBag.NowYear = DateTime.Today.Year;
            ViewBag.SelYear = year;
            ViewBag.CheckAdd = "";
            ViewBag.MonthCount = DeferredMonth;
            ViewBag.FillData = "";

            //20180906 Daniel 特休假別代碼改用API取回
            List<string> annualLeaveCode = await HRMApiAdapter.GetAnnualLeaveCode(CurrentUser.CompanyCode);

            if (data != null && data.AbsentDetail_Now != null)
            {
                foreach (var absent in data.AbsentDetail_Now)
                {
                    //特休
                    //if (absent.Code == "A01")
                    foreach (var absentCode in annualLeaveCode)
                    {
                        if (absent.Code == "01") //特休
                        {
                            viewmodel.AnnualLeaveHours = absent.AnnualLeaveHours;
                            viewmodel.LeaveHours = absent.LeaveHours;
                            //break;
                        }
                        if (absent.Code == "DD1") //遞延特休
                        {
                            viewmodel.AnnualDeferredLeaveHours = absent.AnnualLeaveHours;
                            viewmodel.DeferredLeaveHours = absent.LeaveHours;
                            //break;
                        }
                    }
                    //break;
                }
            }

            var empALSchedule = Services.GetService<EmpALScheduleService>().GetAll()
                                        .Where(x => x.CompanyID == CurrentUser.CompanyID
                                            && x.DepartmentID == CurrentUser.DepartmentID
                                            && x.EmployeeID == CurrentUser.EmployeeID
                                            && x.Year == year
                                        ).FirstOrDefault();

            if (empALSchedule != null)
            {
                double sTotal = (empALSchedule.S01 + empALSchedule.S02 + empALSchedule.S03 + empALSchedule.S04 + empALSchedule.S05 + empALSchedule.S06 + empALSchedule.S07 + empALSchedule.S08 + empALSchedule.S09 + empALSchedule.S10 + empALSchedule.S11 + empALSchedule.S12);
                double dTotal = (empALSchedule.D01 + empALSchedule.D02 + empALSchedule.D03 + empALSchedule.D04 + empALSchedule.D05 + empALSchedule.D06 + empALSchedule.D07 + empALSchedule.D08 + empALSchedule.D09 + empALSchedule.D10 + empALSchedule.D11 + empALSchedule.D12);

                viewmodel.S01 = empALSchedule.S01;
                viewmodel.S02 = empALSchedule.S02;
                viewmodel.S03 = empALSchedule.S03;
                viewmodel.S04 = empALSchedule.S04;
                viewmodel.S05 = empALSchedule.S05;
                viewmodel.S06 = empALSchedule.S06;
                viewmodel.S07 = empALSchedule.S07;
                viewmodel.S08 = empALSchedule.S08;
                viewmodel.S09 = empALSchedule.S09;
                viewmodel.S10 = empALSchedule.S10;
                viewmodel.S11 = empALSchedule.S11;
                viewmodel.S12 = empALSchedule.S12;
                viewmodel.D01 = empALSchedule.D01;
                viewmodel.D02 = empALSchedule.D02;
                viewmodel.D03 = empALSchedule.D03;
                viewmodel.D04 = empALSchedule.D04;
                viewmodel.D05 = empALSchedule.D05;
                viewmodel.D06 = empALSchedule.D06;
                viewmodel.D07 = empALSchedule.D07;
                viewmodel.D08 = empALSchedule.D08;
                viewmodel.D09 = empALSchedule.D09;
                viewmodel.D10 = empALSchedule.D10;
                viewmodel.D11 = empALSchedule.D11;
                viewmodel.D12 = empALSchedule.D12;
                if (sTotal > 0)
                {
                    ViewBag.CheckAdd ="2";
                }
                else
                {
                    ViewBag.CheckAdd ="1";
                    ViewBag.FillData = "1";
                }
            }
            else
            {
                viewmodel.S01 = 0;
                viewmodel.S02 = 0;
                viewmodel.S03 = 0;
                viewmodel.S04 = 0;
                viewmodel.S05 = 0;
                viewmodel.S06 = 0;
                viewmodel.S07 = 0;
                viewmodel.S08 = 0;
                viewmodel.S09 = 0;
                viewmodel.S10 = 0;
                viewmodel.S11 = 0;
                viewmodel.S12 = 0;
                viewmodel.D01 = 0;
                viewmodel.D02 = 0;
                viewmodel.D03 = 0;
                viewmodel.D04 = 0;
                viewmodel.D05 = 0;
                viewmodel.D06 = 0;
                viewmodel.D07 = 0;
                viewmodel.D08 = 0;
                viewmodel.D09 = 0;
                viewmodel.D10 = 0;
                viewmodel.D11 = 0;
                viewmodel.D12 = 0;
                ViewBag.CheckAdd = "0";
                ViewBag.FillData = "1";
            }
            viewmodel.YearListData = GetYearList(year.ToString());
            viewmodel.SelectedYear = year.ToString();

            return PartialView("_Schedule", viewmodel);
        }

        #endregion
 
        #region 新增 特休排程計畫
        [HttpPost]
        public ActionResult Schedule(AnnualLeaveScheduleModel viewmodel)
        {
            try
            {
                int year = 0;
                int.TryParse(viewmodel.SelectedYear, out year);
                if (year <= 0) year = DateTime.Today.Year;

                var empALSchedule = Services.GetService<EmpALScheduleService>().GetAll()
                                                        .Where(x => x.CompanyID == CurrentUser.CompanyID
                                                            && x.DepartmentID == CurrentUser.DepartmentID
                                                            && x.EmployeeID == CurrentUser.EmployeeID
                                                            && x.Year == year
                                                        ).FirstOrDefault();
                if (empALSchedule == null)
                {
                    EmpALSchedule schedule = new EmpALSchedule()
                    {
                        ID = Guid.NewGuid(),
                        CompanyID = CurrentUser.CompanyID,
                        DepartmentID = CurrentUser.DepartmentID,
                        EmployeeID = CurrentUser.EmployeeID,
                        Year = year,
                        S01 = viewmodel.S01,
                        S02 = viewmodel.S02,
                        S03 = viewmodel.S03,
                        S04 = viewmodel.S04,
                        S05 = viewmodel.S05,
                        S06 = viewmodel.S06,
                        S07 = viewmodel.S07,
                        S08 = viewmodel.S08,
                        S09 = viewmodel.S09,
                        S10 = viewmodel.S10,
                        S11 = viewmodel.S11,
                        S12 = viewmodel.S12,
                        CreatedTime = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        D01 = viewmodel.D01,
                        D02 = viewmodel.D02,
                        D03 = viewmodel.D03,
                        D04 = viewmodel.D04,
                        D05 = viewmodel.D05,
                        D06 = viewmodel.D06,
                        D07 = viewmodel.D07,
                        D08 = viewmodel.D08,
                        D09 = viewmodel.D09,
                        D10 = viewmodel.D10,
                        D11 = viewmodel.D11,
                        D12 = viewmodel.D12
                    };
                    Services.GetService<EmpALScheduleService>().Create(schedule);
                }
                else
                {
                    empALSchedule.S01 = viewmodel.S01;
                    empALSchedule.S02 = viewmodel.S02;
                    empALSchedule.S03 = viewmodel.S03;
                    empALSchedule.S04 = viewmodel.S04;
                    empALSchedule.S05 = viewmodel.S05;
                    empALSchedule.S06 = viewmodel.S06;
                    empALSchedule.S07 = viewmodel.S07;
                    empALSchedule.S08 = viewmodel.S08;
                    empALSchedule.S09 = viewmodel.S09;
                    empALSchedule.S10 = viewmodel.S10;
                    empALSchedule.S11 = viewmodel.S11;
                    empALSchedule.S12 = viewmodel.S12;
                    empALSchedule.D01 = viewmodel.D01;
                    empALSchedule.D02 = viewmodel.D02;
                    empALSchedule.D03 = viewmodel.D03;
                    empALSchedule.D04 = viewmodel.D04;
                    empALSchedule.D05 = viewmodel.D05;
                    empALSchedule.D06 = viewmodel.D06;
                    empALSchedule.D07 = viewmodel.D07;
                    empALSchedule.D08 = viewmodel.D08;
                    empALSchedule.D09 = viewmodel.D09;
                    empALSchedule.D10 = viewmodel.D10;
                    empALSchedule.D11 = viewmodel.D11;
                    empALSchedule.D12 = viewmodel.D12;
                    empALSchedule.ModifiedTime = DateTime.Now;
                    Services.GetService<EmpALScheduleService>().Update(empALSchedule);
                }
                Services.GetService<EmpALScheduleService>().SaveChanges();

                return Json(new AjaxResult() { status = "success", message = "儲存成功" });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResult() { status = "failed", message = ex.Message });
            }
        }
        #endregion
        
        [HttpPost]
        public ActionResult SetSendMail(AnnualLeaveScheduleReportViewModel viewmodel)
        {
            string YearData = DateTime.Today.Year.ToString();
            if (string.IsNullOrWhiteSpace(viewmodel.DeptID))
            {
                return Json(new AjaxResult() { status = "failed", message = "請選擇部門" });
            }
            if (viewmodel.SelectedYear != YearData)
            {
                return Json(new AjaxResult() { status = "failed", message = "只可以發送當前" + YearData + "年度通知!!" });
            }
            List<Department> dept_data = new List<Department>();
            List<Guid> dept_guid = new List<Guid>();
            if (viewmodel.DeptID == "All")
            {
                dept_data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();
            }
            else
            {
                List<string> dept_list = new List<string>(viewmodel.DeptID.Split(','));

                foreach (var x in dept_list)
                {
                    dept_guid.Add(Guid.Parse(x));
                }

                dept_data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && dept_guid.Contains(x.ID)).OrderBy(x => x.DepartmentCode).ToList();
            }

            int year = int.Parse(viewmodel.SelectedYear);
            var empALSchedule = Services.GetService<EmpALScheduleService>().GetAll().Where(x => x.CompanyID == CurrentUser.CompanyID && x.Year == year);
            var empList = empALSchedule.Select(s => s.EmployeeID).ToList();
            var sendEmpList = Services.GetService<EmployeeService>().GetAll().Where(x => dept_guid.Contains(x.DepartmentID) && !empList.Contains(x.ID) && x.LeaveDate == null).ToList();

            if (sendEmpList.Count == 0)
            {
                return Json(new AjaxResult() { status = "failed", message = "無人員需發送通知信。" });
            }
            string count = sendEmpList.Count.ToString();
            //內容資料
            string siteType = SiteFunctionInfo.SiteType.ToLower();
            string scheme = System.Web.HttpContext.Current.Request.Url.Scheme;
            int port = System.Web.HttpContext.Current.Request.Url.Port;
            string strPort = "";
            string applicationPath = "";

            if (siteType == "mobile" || siteType == "outside" || siteType == "signonly" || scheme.ToLower() == "https")
            {
                strPort = "";
                applicationPath = "";
                scheme = "https"; //全部更正回https
            }
            else
            {
                strPort = ":" + port.ToString();
                applicationPath = System.Web.HttpContext.Current.Request.ApplicationPath;
            }

            string _webURL = scheme + "://" + System.Web.HttpContext.Current.Request.Url.Host + strPort + applicationPath;
            string _body = string.Empty;
            string _subject = string.Empty;
            string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
            foreach (var data in sendEmpList)
            {
                List<string> _rcpt = new List<string>();
                _rcpt.Add(data.Email);
                _subject = data.EmployeeName + "特休排程計畫通知";
                _body = ("提醒您！<br/>您尚未填寫特休排程計畫表，請儘速完成，謝謝！<br/><a href=" + _webURL + ">系統網站 HR Portal</a>");
                Services.GetService<MailMessageService>().CreateMail(_fromMail, _rcpt.ToArray(), null, null, _subject, _body, true);
            }
            string msg = "本次發送通知：共計" + count + "筆";

            return Json(new AjaxResult() { status = "success", message = msg });

        }            
    }
}
using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC.Controllers
{
    public class DeptStaffLeaveStatisticsController : MultiDepEmpController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string EmployeeData = "", string DepartmentData = "", string YearData = "", string StatusData = "")
        {
            DeptStaffLeaveStatisticsViewModel viewmodel = new DeptStaffLeaveStatisticsViewModel();

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

            viewmodel.DepartmentListData = GetDepartmentList2(DepartmentIDData);
            viewmodel.SelectedDepartment = DepartmentIDData.ToString();

            viewmodel.EmployeeListData = GetEmployeetList2(DepartmentIDData.ToString(), EmployeeData);
            viewmodel.SelectedEmployee = EmployeeData;

            viewmodel.YearListData = GetYearList(YearData);
            viewmodel.SelectedYear = YearData;

            viewmodel.StatuslistDataData = GetStatusDataList(StatusData);
            viewmodel.AbsentCodes = GetAbsentCodeList();

            ViewBag.isALL = isAdmin || isHR;

            int DeferredMonth = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeferredLeavePeriod"));
            ViewBag.MonthCount = DeferredMonth;

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Index(DeptStaffLeaveStatisticsViewModel viewmodel, string btnQuery, string btnClear)
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

        #region 匯出Excel
        [HttpPost]
        public async Task<ActionResult> Export(DeptStaffLeaveStatisticsViewModel viewmodel)
        {
            DeptStaffLeaveStatisticsViewModel model = await GetSummaryDetail(viewmodel);


            var QabsentCode = viewmodel.QAbsentCodes.Split(',').ToList();
            List<SortingData> sortingData = new List<SortingData>();

            foreach (var item in model.DeptDetailDatas)
            {
                foreach (var personaldetail in item.PersonalDetailDatas)
                {
                    if (personaldetail.EmployeeNo == "SummaryTable")
                    {
                        continue;
                    }
                    var empdata = Services.GetService<EmployeeService>().GetAll().Where(x => x.Enabled && x.EmployeeNO == personaldetail.EmployeeNo).FirstOrDefault();
                    var deptData = Services.GetService<DepartmentService>().GetListsByCompany(empdata.CompanyID).Where(x => x.Enabled && x.ID == empdata.DepartmentID).FirstOrDefault();
                    foreach (var ss in personaldetail.SummaryDetail)
                    {
                        sortingData.Add(new SortingData
                            {
                                DeptCode = empdata.SignDepartment == null ? empdata.Department.DepartmentCode : empdata.SignDepartment.DepartmentCode,// item.DeptCode,
                                DeptName = empdata.SignDepartment == null ? empdata.Department.DepartmentName : empdata.SignDepartment.DepartmentName,// item.DeptName,
                                AbsentCode = ss.AbsentCode,
                                AbsentName = ss.TypeName,
                                EmployeeName =personaldetail.EmployeeName,
                                EmployeeNo =personaldetail.EmployeeNo,
                                AnnualLeaveHours = ss.AnnualLeaveHours,
                                EachMonth = ss.EachMonth,
                                Year = model.SelectedYear
                            });
                    }
                }
            }

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            string[] listColumn = new string[] { "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月" };
            int maxColumn = 16;
            if (sortingData != null && sortingData.Count() > 0)
            {
                foreach (var absentCode in QabsentCode)
                {
                    var dataList = sortingData.Where(x => x.AbsentCode == absentCode).OrderBy(o=>o.DeptCode).ThenBy(o=>o.EmployeeNo).ToList();
                    if (dataList.Count > 0)
                    {
                        var sheet = workbook.Worksheets.Add(dataList[0].AbsentName);
                        sheet.Style.Alignment.WrapText = true;
                        //自動換行
                        sheet.Style.Alignment.WrapText = true;

                        //水平、垂直置中
                        sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        sheet.Column(1).Width = 10;
                        sheet.Columns(2, 3).Width = 6;
                        sheet.Columns(4, 4).Width = 8;
                        sheet.Columns(5, maxColumn).Width = 8;

                        #region 共用表頭
                        int rowIdx = 1;

                        sheet.Row(rowIdx).Height = 30;
                        sheet.Range(rowIdx, 1, rowIdx, maxColumn).Merge();
                        sheet.Cell(rowIdx, 1).Value = "部門員工休假統計表";
                        sheet.Range(rowIdx, 1, rowIdx, 1).Style.Font.SetBold(true);
                        sheet.Range(rowIdx, 1, rowIdx, 1).Style.Font.SetFontSize(18);

                        sheet.Row(rowIdx + 1).Height = 20;
                        sheet.Cell(rowIdx + 1, 1).Value = string.Format("年度：{0}年", dataList[0].Year);
                        sheet.Range(rowIdx + 1, 1, rowIdx + 1, maxColumn).Merge();
                        sheet.Range(rowIdx + 1, 1, rowIdx + 1, 2).Style.Font.SetFontSize(12);
                        sheet.Range(rowIdx + 1, 1, rowIdx + 1, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        #endregion

                        rowIdx += 2;

                        var DepList = dataList.Select(x => new { DeptCode = x.DeptCode, DeptName = x.DeptName}).Distinct().ToList();
                        foreach (var inData in DepList)
                        {
                            //部門
                            sheet.Row(rowIdx).Height = 20;
                            sheet.Cell(rowIdx, 1).Value = string.Format("部門：{0} {1}", inData.DeptCode, inData.DeptName);
                            sheet.Range(rowIdx, 1, rowIdx, maxColumn).Merge();
                            sheet.Range(rowIdx, 1, rowIdx, 2).Style.Font.SetFontSize(12);
                            sheet.Range(rowIdx, 1, rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            rowIdx++;

                            sheet.Row(rowIdx).Height = 20;
                            sheet.Cell(rowIdx, 1).Value = string.Format("假別：{0}", dataList[0].AbsentName);
                            sheet.Range(rowIdx, 1, rowIdx, maxColumn).Merge();
                            sheet.Range(rowIdx, 1, rowIdx, 2).Style.Font.SetFontSize(12);
                            sheet.Range(rowIdx, 1, rowIdx, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            rowIdx++;

                            #region 欄位名稱

                            sheet.Range(rowIdx, 1, rowIdx + 1, 1).Merge();
                            sheet.Cell(rowIdx, 1).Style.Fill.SetBackgroundColor(XLColor.Pink);
                            sheet.Cell(rowIdx, 1).Value = "員工姓名";

                            sheet.Range(rowIdx, 2, rowIdx, 4).Merge();
                            sheet.Cell(rowIdx, 2).Value = "統計";
                            sheet.Cell(rowIdx + 1, 2).Value = "可休\n時數";
                            sheet.Cell(rowIdx + 1, 3).Value = "已休\n時數";
                            sheet.Cell(rowIdx + 1, 4).Value = "已休率";
                            sheet.Range(rowIdx, 2, rowIdx + 1, 4).Style.Fill.SetBackgroundColor(XLColor.Yellow);

                            for (int i = 1; i <= 12; i++)
                            {
                                sheet.Cell(rowIdx, i + 4).Value = listColumn[i - 1];
                                sheet.Cell(rowIdx + 1, i + 4).Value = "已休";
                            }

                            sheet.Range(rowIdx, 5, rowIdx + 1, maxColumn).Style.Fill.SetBackgroundColor(XLColor.LightGray);
                            //框線
                            sheet.Range(rowIdx, 1, rowIdx + 1, maxColumn).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            sheet.Range(rowIdx, 1, rowIdx + 1, maxColumn).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            sheet.Range(rowIdx, 1, rowIdx + 1, maxColumn).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            sheet.Range(rowIdx, 1, rowIdx + 1, maxColumn).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            rowIdx += 2;

                            #endregion 欄位名稱

                            var insEmpData = dataList.Where(x => x.DeptCode == inData.DeptCode).ToList();
                            
                            #region 員工假別時數資料

                            decimal actual = 0;
                            decimal canUseCount = 0;
                            int lastCol = 0;
                            foreach (var Empdata in insEmpData)
                            {
                                sheet.Cell(rowIdx, 1).Value = Empdata.EmployeeName;
                                for (int i = 1; i <= 12; i++)
                                {
                                    sheet.Cell(rowIdx, i + 4).Value = Empdata.EachMonth[i - 1];
                                    sheet.Cell(rowIdx, i + 4).Style.Fill.SetBackgroundColor(XLColor.LightYellow);
                                    lastCol = i + 4;
                                }
                                actual = Empdata.EachMonth.Sum();
                                sheet.Cell(rowIdx, 2).Value = Empdata.AnnualLeaveHours;
                                sheet.Cell(rowIdx, 3).Value = actual;
                                canUseCount = decimal.Parse(Empdata.AnnualLeaveHours);
                                sheet.Cell(rowIdx, 4).Value = canUseCount == 0 ? 0 + "％" : actual == 0 ? 0 + "％" : Math.Round(((double)actual / (double)canUseCount) * 100, 2) + "％";

                                sheet.Range(rowIdx, 2, rowIdx, lastCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                //框線
                                sheet.Range(rowIdx, 1, rowIdx, maxColumn).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                sheet.Range(rowIdx, 1, rowIdx, maxColumn).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                sheet.Range(rowIdx, 1, rowIdx, maxColumn).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                sheet.Range(rowIdx, 1, rowIdx, maxColumn).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                rowIdx++;
                            }
                            rowIdx++;
                            #endregion 員工特休資料                           
                        }                        
                    }
                    else
                    {
                        continue;
                    }
                }
                //輸出Excel
                MemoryStream excelStream = new MemoryStream();
                workbook.SaveAs(excelStream);
                excelStream.Position = 0;
                string exportFileName = string.Concat("部門員工休假統計表_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
                return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);
            }

            //var sheet = workbook.Worksheets.Add("AnnualLeaveScheduleReport");
            //自動換行
            //sheet.Style.Alignment.WrapText = true;

            //水平、垂直置中
            //sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            //欄寬
            //sheet.Column(1).Width = 10;
            //sheet.Columns(2, 5).Width = 5;
            //sheet.Columns(6, 6).Width = 8;
            //sheet.Columns(7, 30).Width = 5;

            //string[] listColumn = new string[] { "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月" };
            /*
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
            */
            ////輸出Excel
            //MemoryStream excelStream = new MemoryStream();
            //workbook.SaveAs(excelStream);
            //excelStream.Position = 0;
            //string exportFileName = string.Concat("部門員工休假統計表_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            //return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.Default))
                {
                    sw.Write("查無資料");
                    sw.Flush();
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);
                    //sw.Close();
                    return File(ms.GetBuffer(), "text/plain", "查詢錯誤.txt");
                }
            }
        }

        #endregion

        #region 讀取選單資料資料

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

            listItem.FirstOrDefault(x => x.Value == selecteddata).Selected = true;
            return listItem;
        }

        /// <summary>
        /// 取得假別選單物件集合
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAbsentCodeList()
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            if (Session["Absents"] != null)
            {
                List<AbsentType> dictAbsents = Session["Absents"] as List<AbsentType>;
                //listItem.Add(new SelectListItem { Text = "全部", Value = "" });
                foreach (var item in dictAbsents)
                {
                    listItem.Add(new SelectListItem { Text = item.AbsentName, Value = item.AbsentCode.Trim()});
                }
            }
            return listItem;
        }

        #endregion


        /// <summary>
        /// 查詢員工休假時數彙總資料，函數內會過濾出指定的假別資料。
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        private async Task<DeptStaffLeaveStatisticsViewModel> GetSummaryDetail(DeptStaffLeaveStatisticsViewModel viewmodel)
        {
            DeptStaffLeaveStatisticsViewModel model = await GetLeaveData(viewmodel.EmpID.Trim(),
                viewmodel.SelectedYear, viewmodel.QAbsentCodes.TrimStart(',').TrimEnd(',').Split(',').ToList(), viewmodel.SelectedStatuslistData);

            model.EmpID = viewmodel.EmpID;
            model.QAbsentCodes = viewmodel.QAbsentCodes;
            model.SelectedDepartment = viewmodel.SelectedDepartment;
            model.SelectedEmployee = viewmodel.SelectedEmployee;
            model.SelectedYear = viewmodel.SelectedYear;
            model.QAbsentCodes = viewmodel.QAbsentCodes;
            List<string> qabsentcodes = viewmodel.QAbsentCodes.Split(',').ToList();


            foreach (var item in model.DeptDetailDatas)
            {
                foreach (var personaldetail in item.PersonalDetailDatas)
                {
                    personaldetail.SummaryDetail.RemoveAll(x => !qabsentcodes.Contains(x.AbsentCode)); //.Where(x => qabsentcodes.Contains(x.AbsentCode));
                }

            }
            return model;
        }

        private async Task<DeptStaffLeaveStatisticsViewModel> GetLeaveData(string employeeNo, string yearData, List<string> absentcodes, string StatusData)
        {
            DeptStaffLeaveStatisticsViewModel viewmodel = new DeptStaffLeaveStatisticsViewModel();
            //2018/11/6 Neo 取得假別優先排序
            string sortLeaveStr = Services.GetService<SystemSettingService>().GetSettingValue("SortLeave");
            List<DeptSummary> dSummary = new List<DeptSummary>();

            //配合跨公司簽核，依照公司分群後查詢資料。
            var employeeguid = employeeNo.TrimEnd(',').Split(',');
            var employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeguid.Contains(x.EmployeeNO.ToString())).ToList();

            foreach (var c in employees.Select(s => s.Company.CompanyCode).Distinct())
            {
                var employeenos = employees.Where(x => x.Company.CompanyCode == c).Select(s => s.EmployeeNO).ToList();
                dSummary.AddRange(await GetLeaveDataCore(employeenos, c, yearData, absentcodes, sortLeaveStr, StatusData));
            }

            viewmodel.DeptDetailDatas = dSummary;

            return viewmodel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeNo">員工編號清單集合</param>
        /// <param name="companycode">公司代號</param>
        /// <param name="yearData">查詢年度 yyyy</param>
        /// <param name="absentcodes">假別代碼清單資料</param>
        /// <param name="sortLeaveStr">假別排序字串</param>
        /// <returns></returns>
        private async Task<List<DeptSummary>> GetLeaveDataCore(List<string> employeeNo, string companycode, string yearData, List<string> absentcodes, string sortLeaveStr, string StatusData)
        {
            DeptEmpLeaveSummaryItem deptempleavesummary = new DeptEmpLeaveSummaryItem();
            //GetLeaveSummary_DeptStaff
            deptempleavesummary = await HRMApiAdapter.GetLeaveSummary2(new AbsentSummaryQueryRes()
            {
                CompanyCode = companycode,
                EmpIDList = employeeNo,
                Year = int.Parse(yearData),
                AbsentCodeList = absentcodes,
                StatusData = StatusData
            });

            #region 依部門代號整理

            var deptGroup = deptempleavesummary.PersonalDetailDatas.GroupBy(x => new { x.DeptCode, x.DeptName })
                                                       .OrderBy(x => x.Key.DeptCode)
                                                       .Select(x => new { DeptCode = x.Key.DeptCode, DeptName = x.Key.DeptName })
                                                       .ToList();

            List<DeptSummary> dSummary = new List<DeptSummary>();
            foreach (var _dept in deptGroup)
            {
                DeptSummary _deptSummary = new DeptSummary()
                {
                    DeptCode = _dept.DeptCode,
                    DeptName = _dept.DeptName,
                    PersonalDetailDatas = new List<DeptPersonalSummary>()
                };

                List<DeptPersonalSummary> pSummary = deptempleavesummary.PersonalDetailDatas.Where(x => x.DeptCode == _dept.DeptCode).Select(x => new DeptPersonalSummary()
                {
                    EmployeeName = x.EmpName,
                    EmployeeNo = x.EmpNo,
                    SummaryDetail = x.SummaryDetail.Select(y => new HolidaySummaryDetailDeptStaff()
                    {
                        TypeName = y.Name,
                        AbsentCode = y.AbsentCode,
                        EachMonth = y.EachMonth,
                        ApprovedHours = y.HolidayData.PendingCount.ToString(),
                        AnnualLeaveHours = y.HolidayData.CanUseCountInRange.ToString(),
                        RemainderLeaveHours = (y.HolidayData.CanUseCountInRange - y.HolidayData.UsedCountInRange).ToString(), //年度內的剩餘時數，不過此欄位目前沒用到
                        OverdueHours = "0", //目前沒用到
                        BalancedCount = y.HolidayData.BalancedCount == null ? "0" : y.HolidayData.BalancedCount.ToString(),
                        DefarredAmount = y.HolidayData.DefarredAmount == null ? "0" : y.HolidayData.DefarredAmount.ToString()
                    }).ToList()
                }).ToList();

                //超過一筆時，新增加總資訊
                if (pSummary.Count() > 1)
                {
                    DeptPersonalSummary deptSum = new DeptPersonalSummary()
                    {
                        EmployeeName = HRPortal.MultiLanguage.Resource.SummaryTable,
                        EmployeeNo = "SummaryTable",
                        SummaryDetail = new List<HolidaySummaryDetailDeptStaff>()
                    };

                    //根據個人明細計算部門加總
                    List<HolidaySummaryDetailDeptStaff> deptSummaryDetailsss = pSummary.SelectMany(x => x.SummaryDetail.Select(y => new
                    {
                        AbsentCode = y.AbsentCode,
                        AbsentName = y.TypeName,
                        ApprovedHours = y.ApprovedHours,
                        AnnualLeaveHours = y.AnnualLeaveHours,
                        RemainderLeaveHours = y.RemainderLeaveHours,
                        OverdueHours = y.OverdueHours,
                        BalancedCount = y.BalancedCount,
                        Jan = y.EachMonth[0],
                        Feb = y.EachMonth[1],
                        Mar = y.EachMonth[2],
                        Apr = y.EachMonth[3],
                        May = y.EachMonth[4],
                        Jun = y.EachMonth[5],
                        Jul = y.EachMonth[6],
                        Aug = y.EachMonth[7],
                        Sep = y.EachMonth[8],
                        Oct = y.EachMonth[9],
                        Nov = y.EachMonth[10],
                        Dec = y.EachMonth[11],
                        DefarredAmount = y.DefarredAmount
                    })).GroupBy(z => new { AbsentCode = z.AbsentCode, AbsentName = z.AbsentName })
                    .Select(s => new HolidaySummaryDetailDeptStaff()
                    {
                        TypeName = s.Key.AbsentName,
                        AbsentCode = s.Key.AbsentCode,
                        EachMonth = new List<decimal>() { s.Sum(i => i.Jan), s.Sum(i => i.Feb), s.Sum(i => i.Mar), s.Sum(i => i.Apr), s.Sum(i => i.May), s.Sum(i => i.Jun), s.Sum(i => i.Jul), s.Sum(i => i.Aug), s.Sum(i => i.Sep), s.Sum(i => i.Oct), s.Sum(i => i.Nov), s.Sum(i => i.Dec) },
                        ApprovedHours = s.Sum(i => decimal.Parse(i.ApprovedHours)).ToString(),
                        AnnualLeaveHours = s.Sum(i => decimal.Parse(i.AnnualLeaveHours)).ToString(),
                        RemainderLeaveHours = s.Sum(i => decimal.Parse(i.RemainderLeaveHours)).ToString(),
                        OverdueHours = s.Sum(i => decimal.Parse(i.OverdueHours)).ToString(),
                        BalancedCount = s.Sum(i => decimal.Parse(i.BalancedCount)).ToString(),
                        DefarredAmount = s.Sum(i => decimal.Parse(i.DefarredAmount)).ToString()
                    }).ToList();

                    deptSum.SummaryDetail = deptSummaryDetailsss;
                    pSummary.Add(deptSum);
                }

                //2018/11/6 Neo 依假別調整排序, 1.補休,2.特休, 其他需依假別名稱第一個字的筆劃來排序(遞增)
                List<DeptPersonalSummary> pSummary2 = new List<DeptPersonalSummary>();
                if (pSummary != null && pSummary.Count() > 0)
                {
                    foreach (var item in pSummary)
                    {
                        if (item.SummaryDetail != null && item.SummaryDetail.Count() > 0)
                        {
                            var sumaryDetail = new List<HolidaySummaryDetailDeptStaff>();
                            var sumaryDetail2 = item.SummaryDetail;

                            //取得假別優先排序
                            string[] sortLeavAry = null;

                            if (!string.IsNullOrEmpty(sortLeaveStr))
                            {
                                sortLeavAry = sortLeaveStr.Split(';');
                                foreach (var sortLeave in sortLeavAry)
                                {
                                    var SLeave = item.SummaryDetail.Where(x => x.TypeName == sortLeave).FirstOrDefault();
                                    if (SLeave != null)
                                    {
                                        sumaryDetail.Add(SLeave);
                                    }
                                    sumaryDetail2 = sumaryDetail2.Where(x => x.TypeName != sortLeave).ToList();//排除假別優先排序
                                }
                            }
                            sumaryDetail2 = sumaryDetail2.OrderBy(x => x.TypeName.Substring(0)).ToList();//其他假別需依假別名稱第一個字的筆劃來排序(遞增)
                            if (sumaryDetail2 != null && sumaryDetail2.Count() > 0)
                            {
                                sumaryDetail.AddRange(sumaryDetail2);
                            }

                            pSummary2.Add(new DeptPersonalSummary()
                            {
                                EmployeeName = item.EmployeeName,
                                EmployeeNo = item.EmployeeNo,
                                SummaryDetail = sumaryDetail
                            });
                        }
                    }
                }

                _deptSummary.PersonalDetailDatas = pSummary2;
                dSummary.Add(_deptSummary);
            }

            #endregion 依部門代號整理

            return dSummary;
        }
    }
}
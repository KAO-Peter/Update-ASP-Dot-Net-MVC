using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.Controllers;
using HRPortal.Mvc.Controllers;
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
    public class LeaveSummaryController : BaseController
    {
        private ReportQueryController ReportQuery = new ReportQueryController();

        public async Task<ActionResult> Index(int page = 1, string DepartmentData = "", string beginDate = "", string endDate = "", string SignStatusdate = "")
        {
            if (string.IsNullOrWhiteSpace(DepartmentData))
            {
                DepartmentData = CurrentUser.DepartmentCode;
            }
            SetDefaultData(DepartmentData, beginDate, endDate, SignStatusdate);
            int currentPage = page < 1 ? 1 : page;
            if (string.IsNullOrWhiteSpace(beginDate) || string.IsNullOrWhiteSpace(endDate))
            {
                return View();
            }
            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }

            var _result = Query(DepartmentData, DateTime.Parse(beginDate), DateTime.Parse(endDate), SignStatusdate).ToPagedList(currentPage, currentPageSize);
            await GetAbsentHours(_result, beginDate);

            return View(_result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string DepartmentData, string btnQuery, string btnClear, string btnExport, int page = 1, string beginDate = "", string endDate = "", string SignStatusdate = "")
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
                                                SignStatusdate
                                            });
                }
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
            {
                SetDefaultData(CurrentUser.DepartmentCode);
                return View();
            }
            SetDefaultData(DepartmentData, beginDate, endDate, SignStatusdate);
            return View();
        }

        public async Task<ActionResult> Export(int page = 1, string DepartmentData = "", string beginDate = "", string endDate = "", string SignStatusdate = "")
        {
            Session["AbsentsData"] = await HRMApiAdapter.GetAllAbsentData(CurrentUser.CompanyCode);

            if (Session["Absents"] == null)
            {
                Session["Absents"] = await HRMApiAdapter.GetAllAbsentType(CurrentUser.CompanyCode);
            }

            var _result = Query(DepartmentData, DateTime.Parse(beginDate), DateTime.Parse(endDate), SignStatusdate);
            await GetAbsentHours(_result, beginDate);

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("LeaveSummary");
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
            sheet.Column(4).Width = 35;
            sheet.Column(5).Width = 13;
            sheet.Column(6).Width = 33;
            sheet.Column(7).Width = 23;
            sheet.Column(8).Width = 23;
            sheet.Column(9).Width = 10;
            sheet.Row(1).Height = 35;

            //欄位名稱
            var newline = char.ConvertFromUtf32((int)10);
            string[] listColumn = new string[] { "Item" + newline + "項目",
                                                 "Names" + newline + "姓名",
                                                 "Leave reason" + newline + "請假假別",
                                                 "Leave date" + newline + "請假起訖日期",
                                                 "Days" + newline + "請假天數",
                                                 string.Format("Total annual days earned in {0}{1}{0} 年休假天數",DateTime.Parse(beginDate).Year, newline),
                                                 "Total annual days used" + newline + "已經請假天數",
                                                 "Total annual days left" + newline + "剩下未請假天數",
                                                 "Sign" + newline + "簽核" };
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
                sheet.Cell(rowIdy, 2).Value = string.IsNullOrWhiteSpace(item.SenderEmployeeNameEn) ? item.SenderEmployeeName : string.Format("{0}{2}{1}", item.SenderEmployeeNameEn, item.SenderEmployeeName, newline);
                sheet.Cell(rowIdy, 3).Value = string.IsNullOrWhiteSpace(item.AbsentNameEn) ? item.AbsentName : string.Format("{0}{2}{1}", item.AbsentNameEn, item.AbsentName, newline);
                sheet.Cell(rowIdy, 4).Value = item.FormSummary;
                sheet.Cell(rowIdy, 5).Value = string.Format("{0:0.####} {1}", item.LeaveAmount, GetAbsentUnit(item.LeaveAmount));
                sheet.Cell(rowIdy, 6).Value = string.Format("{0:0.####} {1}", item.TotalAnnualAmount, GetAbsentUnit(item.TotalAnnualAmount));
                sheet.Cell(rowIdy, 7).Value = string.Format("{0:0.####} {1}", item.TotalLeaveAmount, GetAbsentUnit(item.TotalLeaveAmount));
                sheet.Cell(rowIdy, 8).Value = string.Format("{0:0.####} {1}", item.TotalUseAmount, GetAbsentUnit(item.TotalUseAmount));
                sheet.Cell(rowIdy, 9).Value = item.SignStatus == "A" ? "Signed" : "Unsigned";
                rowIdy++;
            }

            //框線
            sheet.Range(1, 1, rowIdy - 1, 9).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            sheet.Range(1, 1, rowIdy - 1, 9).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            sheet.Range(1, 1, rowIdy - 1, 9).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            sheet.Range(1, 1, rowIdy - 1, 9).Style.Border.RightBorder = XLBorderStyleValues.Thin;

            //輸出Excel
            MemoryStream excelStream = new MemoryStream();
            workbook.SaveAs(excelStream);
            excelStream.Position = 0;
            string exportFileName = string.Concat("LeaveSummary_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);
        }

        private void SetDefaultData(string departmentdata = "", string beginDate = "", string endDate = "", string SignStatusdate = "")
        {
            ViewBag.SignStatusdate = SignStatusdate;
            if (string.IsNullOrWhiteSpace(beginDate))
            {
                ViewBag.StartTime = DateTime.Now.AddMonths(-1).AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
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
            ViewBag.LanguageCookie = getLanguageCookie;

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

        public List<HRPotralLeaveSummary> Query(string departmentcode, DateTime startTime, DateTime endTime, string SignStatusdate)
        {
            using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
            {
                List<HRPotralLeaveSummary> _result = new List<HRPotralLeaveSummary>();
                if (!string.IsNullOrWhiteSpace(departmentcode))
                {
                    _result = _queryHelper.GetCurrentLeaveSignListByDepartment(CurrentUser.CompanyCode, departmentcode, startTime, endTime.AddDays(1));
                }

                //判斷簽核狀態後取得資料
                if (departmentcode == "B315000")
                {   //創新策略部 => 簽核人是否為 01230 Jeff
                    _result = _result.Where(x => x.SignerEmployeeNo == "01230").ToList();
                }
                if (SignStatusdate == "A")
                {
                    _result = _result.Where(x => x.SignStatus == "A").ToList();
                }
                else if (SignStatusdate == "WS")
                {
                    _result = _result.Where(x => x.SignType == "P" && x.SignStatus == "W").ToList();
                }
                else
                {
                    _result = _result.Where(x => (x.SignType == "P" && x.SignStatus == "W") || x.SignStatus == "A").ToList();
                }
                _result = _result.OrderBy(x => x.DepartmentCode).ThenBy(x => x.SenderEmployeeNo).ThenBy(x => x.FormCreateDate).ToList();
                //End

                return _result;
            }
        }

        private static async Task GetAbsentHours(IEnumerable<HRPotralLeaveSummary> _result, string beginDate)
        {
            var _employee = _result.GroupBy(x => new { x.CompanyCode, x.SenderEmployeeNo }).ToDictionary(g => new { g.Key.CompanyCode, g.Key.SenderEmployeeNo });

            //取得剩餘時數的假別
            foreach (var emp in _employee)
            {
                AbsentDetailAll data = await HRMApiAdapter.GetEmployeeAbsent2(emp.Key.CompanyCode, emp.Key.SenderEmployeeNo, DateTime.Parse(beginDate), "remaining");
                var leaveSummary = _result.Where(x => x.SenderEmployeeNo == emp.Key.SenderEmployeeNo);
                foreach (var item in leaveSummary)
                {
                    var absentData = data.AbsentDetail_Now.Where(x => x.Code == item.AbsentCode && x.CanUse).FirstOrDefault();
                    if (absentData != null)
                    {
                        item.AbsentName = absentData.Name;
                        item.AbsentNameEn = absentData.AbsentNameEn;
                      
                        if (absentData.Unit == "h")
                        {
                            item.TotalAnnualAmount = absentData.AnnualLeaveHours / 8;
                            item.TotalLeaveAmount = absentData.LeaveHours / 8;
                            //item.TotalUseAmount = absentData.UseAmount / 8;
                            item.TotalUseAmount = (absentData.AnnualLeaveHours - absentData.LeaveHours) / 8;
                            item.LeaveAmount = item.LeaveAmount / 8;
                        }
                        else
                        {
                            item.TotalAnnualAmount = absentData.AnnualLeaveHours;
                            item.TotalLeaveAmount = absentData.LeaveHours;
                            //item.TotalUseAmount = absentData.UseAmount;
                            item.TotalUseAmount = absentData.AnnualLeaveHours - absentData.LeaveHours;
                        }
                    }
                }
            }
        }

        public string GetAbsentUnit(decimal value) {
            return value > 1 ? "Days" : "Day";
        }
    }
}
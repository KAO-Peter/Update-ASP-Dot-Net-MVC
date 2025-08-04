using ClosedXML.Excel;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.Reports.Controllers
{
    public class DeptEmpHolidaySummaryController : MultiDepEmpController
    {
        private bool isGetRole = false, isHR = false, isAdmin = false;

        public ActionResult Index()
        {
            HolidaySummaryViewModel viewmodel = new HolidaySummaryViewModel();
            string statusData = "";
            if (CurrentUser.Employee.LeaveDate != null && CurrentUser.Employee.LeaveDate < DateTime.Now)
            {
                statusData = "L";
            }
            viewmodel.YearListData = GetYearList();
            viewmodel.SelectedYear = DateTime.Now.Year.ToString();

            viewmodel.DepartmentListData = GetDepartmentList2(CurrentUser.SignDepartmentID);
            viewmodel.EmployeeListData = GetEmployeetList2(CurrentUser.SignDepartmentID.ToString(), CurrentUser.EmployeeNO, statusData, "ID");
            viewmodel.StatuslistDataData = GetStatusDataList();

            viewmodel.AbsentCodes = GetAbsentCodeList();

            Role roleDataa = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            viewmodel.Role = roleDataa.RoleParams != null ? roleDataa.RoleParams : CurrentUser.SignDepartments.Count > 0 ? "is_sign_manager" : roleDataa.RoleParams;

            ViewBag.isALL = isAdmin || isHR;

            return View(viewmodel);
        }

        [HttpPost]
        public async Task<ActionResult> SummaryDetail(HolidaySummaryViewModel viewmodel)
        {
            if (!string.IsNullOrWhiteSpace(viewmodel.SelectedYear))
            {
                HolidaySummaryViewModel model = await GetSummaryDetail(viewmodel);
                return PartialView("_SumeryDetail", model);
            }
            return PartialView("_SumeryDetail", viewmodel);
        }

        private async Task<HolidaySummaryViewModel> GetLeaveData(string employeeNo, string yearData, List<string> absentcodes)
        {
            HolidaySummaryViewModel viewmodel = new HolidaySummaryViewModel();
            //2018/11/6 Neo 取得假別優先排序
            string sortLeaveStr = Services.GetService<SystemSettingService>().GetSettingValue("AbsentCodesForAdvancedInfo");
            List<DeptSummary> dSummary = new List<DeptSummary>();

            //配合跨公司簽核，依照公司分群後查詢資料。
            var employeeguid = employeeNo.TrimEnd(',').Split(',');
            var employees = Services.GetService<EmployeeService>().GetAll().Where(x => employeeguid.Contains(x.ID.ToString())).ToList();

            foreach (var c in employees.Select(s => s.Company.CompanyCode).Distinct())
            {
                var employeenos = employees.Where(x => x.Company.CompanyCode == c).Select(s => s.EmployeeNO).ToList();
                dSummary.AddRange(await GetLeaveDataCore(employeenos, c, yearData, absentcodes, sortLeaveStr));
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
        private async Task<List<DeptSummary>> GetLeaveDataCore(List<string> employeeNo, string companycode, string yearData, List<string> absentcodes, string sortLeaveStr)
        {
            DeptEmpLeaveSummaryItem deptempleavesummary = new DeptEmpLeaveSummaryItem();

            deptempleavesummary = await HRMApiAdapter.GetLeaveSummary(new AbsentSummaryQueryRes()
            {
                CompanyCode = companycode,
                EmpIDList = employeeNo,
                Year = int.Parse(yearData),
                AbsentCode = absentcodes
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
                try
                {

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
                            RemainderLeaveHours = (y.HolidayData.CanUseCountInRange - double.Parse( y.EachMonth.Sum().ToString())).ToString(), //年度內的剩餘時數，不過此欄位目前沒用到
                            OverdueHours = "0", //目前沒用到
                            BalancedCount = y.HolidayData.BalancedCount.ToString()
                            //DefarredAmount = y.HolidayData.DefarredAmount != null  ? y.HolidayData.DefarredAmount.ToString() : "0"//,
                            //ConvertBalancedCount = y.HolidayData.ConvertBalancedCount.ToString()
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
                        Dec = y.EachMonth[11]//,
                        //DefarredAmount = y.DefarredAmount//,
                        //ConvertBalancedCount = y.ConvertBalancedCount
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
                        BalancedCount = s.Sum(i => decimal.Parse(i.BalancedCount)).ToString()//,
                        //DefarredAmount = s.Sum(i => decimal.Parse(i.DefarredAmount)).ToString(),
                        //ConvertBalancedCount = s.Sum(i => decimal.Parse(i.ConvertBalancedCount)).ToString()
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
                                sortLeavAry = sortLeaveStr.Split(',');
                                foreach (var sortLeave in sortLeavAry)
                                {
                                    var SLeave = item.SummaryDetail.Where(x => x.AbsentCode == sortLeave).ToList();
                                    if (SLeave != null)
                                    {
                                        sumaryDetail.AddRange(SLeave);
                                    }
                                    sumaryDetail2 = sumaryDetail2.Where(x => x.AbsentCode != sortLeave).ToList();//排除假別優先排序
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
                catch (Exception ex)
                {
                    return null;
                }

            }

            #endregion 依部門代號整理

            return dSummary;
        }

        /// <summary>
        /// 查詢員工休假時數彙總資料，函數內會過濾出指定的假別資料。
        /// </summary>
        /// <param name="viewmodel"></param>
        /// <returns></returns>
        private async Task<HolidaySummaryViewModel> GetSummaryDetail(HolidaySummaryViewModel viewmodel)
        {
            HolidaySummaryViewModel model = await GetLeaveData(viewmodel.EmpID.Trim(),
                viewmodel.SelectedYear, viewmodel.QAbsentCodes.TrimStart(',').TrimEnd(',').Split(',').ToList());

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

        /// <summary>
        /// 取得假別選單物件集合
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetAbsentCodeList()
        {
            var getLanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            List<SelectListItem> itemList = new List<SelectListItem>();

            //var data = await HRMApiAdapter.GetAllAbsent(CompanyCode, getLanguageCookie);
            List<AbsentType> data = (List<AbsentType>)Session["Absents"];

            //itemList.Add(new SelectListItem { Text = MultiLanguage.Resource.PleaseChoose, Value = "", Selected = (Selecteddata == "" ? true : false) });
            foreach (var item in data)
            {
                //2018/11/2 Neo 部門員工休假時數彙總表 查詢條件假別選項中排除"補休"
                //20180522 Daniel 調整為看假別代碼
                //if (!item.Value.Contains("補休"))
                if (item.AbsentCode != "rest")
                {
                    itemList.Add(new SelectListItem
                    {
                        Text = getLanguageCookie == "en-US" ? item.AbsentEnglishName : item.AbsentName,
                        Value = item.AbsentCode
                    });
                }
            }
            return itemList;
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
        /// 計算假別剩餘時數
        /// </summary>
        public decimal CalculateReminderAbsentHours(string AnnualLeaveHours, string ApprovedHours, List<decimal> EachMonth)
        {
            if (AnnualLeaveHours == "")
            {
                AnnualLeaveHours = "0.00";
            }
            if (ApprovedHours == "")
            {
                ApprovedHours = "0.00";
            }
            decimal AnnualLeaveHoursDecimal = Convert.ToDecimal(AnnualLeaveHours);
            decimal ApprovedHoursDecimal = Convert.ToDecimal(ApprovedHours);
            decimal UseAmount = 0;
            decimal ReminderAbsentHours = 0;
            foreach (decimal item in EachMonth)
            {
                UseAmount += item;
            }
            ReminderAbsentHours = AnnualLeaveHoursDecimal - ApprovedHoursDecimal - UseAmount;

            return ReminderAbsentHours;
        }


        [HttpPost]
        public async Task<ActionResult> Export(HolidaySummaryViewModel viewmodel)
        {
            HolidaySummaryViewModel model = await GetSummaryDetail(viewmodel);

            // Create the workbook
            XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("員工休假時數彙總表");
            //自動換行
            sheet.Style.Alignment.WrapText = true;

            //水平、垂直置中
            sheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Row(1).Height = 35;
            sheet.Range(1, 1, 1, 21).Merge();
            sheet.Cell(1, 1).Value = "員工休假時數彙總表";

            sheet.Range(1, 1, 1, 1).Style.Font.SetFontSize(22);
            sheet.Range(1, 1, 1, 1).Style.Font.SetBold(true);

            sheet.Columns(1, 21).AdjustToContents();
            sheet.Column(1).Width = 20;
            sheet.Column(2).Width = 20;
            sheet.Column(5).Width = 20;
            sheet.Column(6).Width = 20;
            string[] listColumn = new string[] { "單位代碼", "單位名稱", "工號", "姓名", "假別", "年度可休", "簽核中",  "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月", "總計", "剩餘" };

            int rowIdx = 2;

            #region 建立欄位表頭。
            for (int i = 0; i < listColumn.Count(); i++)
            {
                sheet.Cell(rowIdx, i + 1).Value = listColumn[i];
            }
            #endregion
            rowIdx++;

            foreach (var item in model.DeptDetailDatas)
            {
                foreach (var personaldetail in item.PersonalDetailDatas)
                {
                    if (personaldetail.EmployeeNo == "SummaryTable")
                    {
                        continue;
                    }
                    int colbegin = 0;

                    foreach (var ss in personaldetail.SummaryDetail)
                    {
                        var empData = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == personaldetail.EmployeeNo).FirstOrDefault();
                        var dept = Services.GetService<DepartmentService>().GetAll().Where(x => x.ID == empData.DepartmentID).FirstOrDefault();

                        decimal annualleavehours = Decimal.Parse(ss.AnnualLeaveHours == "" ? "0.0" : ss.AnnualLeaveHours);//年度可休
                        decimal approvedhours = Decimal.Parse(ss.ApprovedHours == "" ? "0.0" : ss.ApprovedHours);//簽核中	
                        decimal total = ss.EachMonth.Sum(s => s);
                        sheet.Cell(rowIdx, 1).Value = dept.DepartmentCode;
                        sheet.Cell(rowIdx, 2).Value = dept.DepartmentName;
                        sheet.Cell(rowIdx, 3).DataType = XLCellValues.Text;
                        sheet.Cell(rowIdx, 3).SetValue<string>(personaldetail.EmployeeNo.ToString());
                        sheet.Cell(rowIdx, 4).Value = personaldetail.EmployeeName;

                        sheet.Cell(rowIdx, 5).Value = ss.TypeName;
                        sheet.Cell(rowIdx, 6).Value = annualleavehours;
                        sheet.Cell(rowIdx, 7).Value = approvedhours;
                        colbegin = 7;
                        for (int i = 1; i <= 12; i++)
                        {
                            sheet.Cell(rowIdx, colbegin + i).Value = ss.EachMonth[i - 1];
                        }
                        sheet.Cell(rowIdx, 20).Value = total ;
                        sheet.Cell(rowIdx, 21).Value = annualleavehours - approvedhours - total ;

                        rowIdx++;
                    }
                }
            }

            //框線
            sheet.Range(2, 1, rowIdx - 1, 21).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            sheet.Range(2, 1, rowIdx - 1, 21).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            sheet.Range(2, 1, rowIdx - 1, 21).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            sheet.Range(2, 1, rowIdx - 1, 21).Style.Border.RightBorder = XLBorderStyleValues.Thin;

            //輸出Excel
            MemoryStream excelStream = new MemoryStream();
            workbook.SaveAs(excelStream);
            excelStream.Position = 0;
            string exportFileName = string.Concat("員工休假時數彙總表_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".xlsx");
            return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportFileName);
        }
    }
}
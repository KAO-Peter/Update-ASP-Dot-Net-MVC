using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;
using NPOI;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class CasualAttendPayrollChecklistController : BaseController
    {
        // GET: FEPH/CasualAttendPayrollChecklist
        public async Task<ActionResult> Index(int page = 1, string cmd = "", string ddlDepartment = "", string txtEmpID = "", string txtEmpName = "", string txtDateS = "", string txtDateE = "", string ddlStatus = "")
        {
            GetDefaultData(ddlDepartment, txtEmpID, txtEmpName, txtDateS, txtDateE, ddlStatus);

            int currentPage = page < 1 ? 1 : page;

            if (cmd == "Search")
            {
                #region Search
                DateTime BeginDate = txtDateS == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateS));
                DateTime EndDate = txtDateE == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateE));

                CasualAttendPayrollChecklist result = await HRMApiAdapter.GetCasualAttendPayrollChecklist(CurrentUser.Employee.Company.CompanyCode, ddlDepartment, txtEmpID, txtEmpName, txtDateS, txtDateE, ddlStatus);
                #endregion

                return View(result.Data.ToPagedList(currentPage, currentPageSize));
            }
            else if (cmd == "Excel")
            {
                #region Excel
                XSSFWorkbook wb = null;
                ISheet ws = null;
                IFont fTitle1 = null, fTitle2 = null, font = null;
                ICellStyle cTitle1 = null, cTitle2 = null, csBR = null, cs = null, csR = null, csL = null, cs1 = null, cs1R = null, cs1L = null;

                int idx = 0;

                DateTime BeginDate = txtDateS == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateS));
                DateTime EndDate = txtDateE == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateE));

                CasualAttendPayrollChecklist result = await HRMApiAdapter.GetCasualAttendPayrollChecklist(CurrentUser.Employee.Company.CompanyCode, ddlDepartment, txtEmpID, txtEmpName, txtDateS, txtDateE, ddlStatus);

                #region 建立Excel
                wb = new XSSFWorkbook();
                ws = wb.CreateSheet("CasualAttendPayrollChecklist");
                ws.PrintSetup.Landscape = true;

                #region 版面設定
                ws.SetMargin(MarginType.TopMargin, 0.2);
                ws.SetMargin(MarginType.BottomMargin, 0.2);
                ws.SetMargin(MarginType.LeftMargin, 0.3);
                ws.SetMargin(MarginType.RightMargin, 0.3);
                #endregion

                #region 標題1 Style
                fTitle1 = wb.CreateFont();
                fTitle1.Color = HSSFColor.Black.Index;
                fTitle1.Boldweight = (short)FontBoldWeight.Bold;
                fTitle1.FontHeightInPoints = 14;
                fTitle1.FontName = "新細明體";

                cTitle1 = wb.CreateCellStyle();
                cTitle1.Alignment = HorizontalAlignment.Center;
                cTitle1.VerticalAlignment = VerticalAlignment.Center;
                cTitle1.SetFont(fTitle1);
                #endregion

                #region 標題2 Style
                fTitle2 = wb.CreateFont();
                fTitle2.Color = HSSFColor.Black.Index;
                fTitle2.Boldweight = (short)FontBoldWeight.Bold;
                fTitle2.FontHeightInPoints = 16;
                fTitle2.FontName = "新細明體";

                cTitle2 = wb.CreateCellStyle();
                cTitle2.Alignment = HorizontalAlignment.Center;
                cTitle2.VerticalAlignment = VerticalAlignment.Center;
                cTitle2.SetFont(fTitle2);
                #endregion

                #region 內容 Style
                font = wb.CreateFont();
                font.Color = HSSFColor.Black.Index;
                //font.Boldweight = (short)FontBoldWeight.Bold;
                font.FontHeightInPoints = 10;
                font.FontName = "新細明體";

                #region csBR
                csBR = wb.CreateCellStyle();
                csBR.WrapText = true;
                csBR.Alignment = HorizontalAlignment.Center;
                csBR.VerticalAlignment = VerticalAlignment.Center;
                csBR.BorderBottom = BorderStyle.Thin;               //下
                csBR.BottomBorderColor = HSSFColor.Black.Index;
                csBR.BorderTop = BorderStyle.Thin;                  //上
                csBR.TopBorderColor = HSSFColor.Black.Index;
                csBR.BorderLeft = BorderStyle.Thin;                 //左
                csBR.LeftBorderColor = HSSFColor.Black.Index;
                csBR.BorderRight = BorderStyle.Thin;                //右
                csBR.RightBorderColor = HSSFColor.Black.Index;
                csBR.SetFont(font);
                #endregion

                #region cs
                cs = wb.CreateCellStyle();
                cs.Alignment = HorizontalAlignment.Center;
                cs.VerticalAlignment = VerticalAlignment.Center;
                cs.BorderBottom = BorderStyle.Thin;               //下
                cs.BottomBorderColor = HSSFColor.Black.Index;
                cs.BorderTop = BorderStyle.Thin;                  //上
                cs.TopBorderColor = HSSFColor.Black.Index;
                cs.BorderLeft = BorderStyle.Thin;                 //左
                cs.LeftBorderColor = HSSFColor.Black.Index;
                cs.BorderRight = BorderStyle.Thin;                //右
                cs.RightBorderColor = HSSFColor.Black.Index;
                cs.SetFont(font);
                #endregion

                #region csR
                csR = wb.CreateCellStyle();
                csR.Alignment = HorizontalAlignment.Right;
                csR.VerticalAlignment = VerticalAlignment.Center;
                csR.BorderBottom = BorderStyle.Thin;               //下
                csR.BottomBorderColor = HSSFColor.Black.Index;
                csR.BorderTop = BorderStyle.Thin;                  //上
                csR.TopBorderColor = HSSFColor.Black.Index;
                csR.BorderLeft = BorderStyle.Thin;                 //左
                csR.LeftBorderColor = HSSFColor.Black.Index;
                csR.BorderRight = BorderStyle.Thin;                //右
                csR.RightBorderColor = HSSFColor.Black.Index;
                csR.SetFont(font);
                #endregion

                #region csL
                csL = wb.CreateCellStyle();
                csL.Alignment = HorizontalAlignment.Left;
                csL.VerticalAlignment = VerticalAlignment.Center;
                csL.BorderBottom = BorderStyle.Thin;               //下
                csL.BottomBorderColor = HSSFColor.Black.Index;
                csL.BorderTop = BorderStyle.Thin;                  //上
                csL.TopBorderColor = HSSFColor.Black.Index;
                csL.BorderLeft = BorderStyle.Thin;                 //左
                csL.LeftBorderColor = HSSFColor.Black.Index;
                csL.BorderRight = BorderStyle.Thin;                //右
                csL.RightBorderColor = HSSFColor.Black.Index;
                csL.SetFont(font);
                #endregion

                #region cs1
                cs1 = wb.CreateCellStyle();
                cs1.Alignment = HorizontalAlignment.Center;
                cs1.VerticalAlignment = VerticalAlignment.Center;
                cs1.SetFont(font);
                #endregion

                #region cs1R
                cs1R = wb.CreateCellStyle();
                cs1R.Alignment = HorizontalAlignment.Right;
                cs1R.VerticalAlignment = VerticalAlignment.Center;
                cs1R.SetFont(font);
                #endregion

                #region cs1L
                cs1L = wb.CreateCellStyle();
                cs1L.Alignment = HorizontalAlignment.Left;
                cs1L.VerticalAlignment = VerticalAlignment.Center;
                cs1L.SetFont(font);
                #endregion
                #endregion
                #endregion

                #region 標題
                ws.CreateRow(0).CreateCell(0).SetCellValue(result.CompanyFullName);
                ws.GetRow(0).GetCell(0).CellStyle = cTitle2;
                ws.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));

                ws.CreateRow(1).CreateCell(0).SetCellValue(result.CompanyNameEn);
                ws.GetRow(1).GetCell(0).CellStyle = cTitle2;
                ws.AddMergedRegion(new CellRangeAddress(1, 1, 0, 8));

                ws.CreateRow(2).CreateCell(0).SetCellValue("工讀生計薪出勤檢查表");
                ws.GetRow(2).GetCell(0).CellStyle = cTitle1;
                ws.AddMergedRegion(new CellRangeAddress(2, 2, 0, 8));

                ws.CreateRow(3).CreateCell(0).SetCellValue(string.Format("資料區間：{0} ~ {1}", txtDateS, txtDateE));
                ws.GetRow(3).GetCell(0).CellStyle = cs1L;
                ws.AddMergedRegion(new CellRangeAddress(3, 3, 0, 3));

                ws.GetRow(3).CreateCell(4).SetCellValue(string.Format("列印日期：{0}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm")));
                ws.GetRow(3).GetCell(4).CellStyle = cs1R;
                ws.AddMergedRegion(new CellRangeAddress(3, 3, 4, 8));
                #endregion

                #region 表頭欄位名稱
                ws.CreateRow(4).CreateCell(0).SetCellValue("Dept\' Number\n部門代碼");
                ws.GetRow(4).GetCell(0).CellStyle = csBR;

                ws.GetRow(4).CreateCell(1).SetCellValue("Dept\' Name\n部門名稱");
                ws.GetRow(4).GetCell(1).CellStyle = csBR;

                ws.GetRow(4).CreateCell(2).SetCellValue("Employee Number\n員工編號");
                ws.GetRow(4).GetCell(2).CellStyle = csBR;

                ws.GetRow(4).CreateCell(3).SetCellValue("Employee\'s Name\n員工姓名");
                ws.GetRow(4).GetCell(3).CellStyle = csBR;

                ws.GetRow(4).CreateCell(4).SetCellValue("Salary Attendance-in\n計薪刷進");
                ws.GetRow(4).GetCell(4).CellStyle = csBR;

                ws.GetRow(4).CreateCell(5).SetCellValue("Salary Attendance-out\n計薪刷出");
                ws.GetRow(4).GetCell(5).CellStyle = csBR;

                ws.GetRow(4).CreateCell(6).SetCellValue("Actual Attendance-in\n實際刷進");
                ws.GetRow(4).GetCell(6).CellStyle = csBR;

                ws.GetRow(4).CreateCell(7).SetCellValue("Actual Attendance-out\n實際刷出");
                ws.GetRow(4).GetCell(7).CellStyle = csBR;

                ws.GetRow(4).CreateCell(8).SetCellValue("檢核狀態");
                ws.GetRow(4).GetCell(8).CellStyle = csBR;
                #endregion

                #region 明細資料
                idx = 4;

                foreach (var item in result.Data)
                {
                    idx += 1;

                    ws.CreateRow(idx).CreateCell(0).SetCellValue(item.DeptCode);
                    ws.GetRow(idx).GetCell(0).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(1).SetCellValue(item.DeptName);
                    ws.GetRow(idx).GetCell(1).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(2).SetCellValue(item.EmpID);
                    ws.GetRow(idx).GetCell(2).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(3).SetCellValue(item.EmpName);
                    ws.GetRow(idx).GetCell(3).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(4).SetCellValue(item.SalaryStartTime);
                    ws.GetRow(idx).GetCell(4).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(5).SetCellValue(item.SalaryEndTime);
                    ws.GetRow(idx).GetCell(5).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(6).SetCellValue(item.ActualStartTime);
                    ws.GetRow(idx).GetCell(6).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(7).SetCellValue(item.ActualEndTime);
                    ws.GetRow(idx).GetCell(7).CellStyle = csL;

                    ws.GetRow(idx).CreateCell(8).SetCellValue(item.StatusName);
                    ws.GetRow(idx).GetCell(8).CellStyle = csL;
                }
                #endregion

                #region Autofit
                for (int i = 0; i < 9; i++)
                {
                    ws.AutoSizeColumn(i);
                }
                #endregion

                wb.SetRepeatingRowsAndColumns(0, 0, 8, 0, 4);

                using (var memoryStream = new MemoryStream())
                {
                    wb.Write(memoryStream);
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }
                #endregion
            }
            else
            {
                return View();
            }
        }

        // POST: FEPH/CasualAttendPayrollChecklist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string cmd = "", string ddlDepartment = "", string txtEmpID = "", string txtEmpName = "", string txtDateS = "", string txtDateE = "", string ddlStatus = "")
        {
            if (cmd == "Clear")
            {
                GetDefaultData();
                return View();
            }
            else if (cmd == "Search" || cmd == "Excel")
            {
                return RedirectToAction("Index", new
                {
                    cmd = cmd,
                    ddlDepartment,
                    txtEmpID,
                    txtEmpName,
                    txtDateS,
                    txtDateE,
                    ddlStatus
                });
            }

            //重整
            GetDefaultData(ddlDepartment, txtEmpID, txtEmpName, txtDateS, txtDateE, ddlStatus);

            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="Grouping"></param>
        /// <param name="ddlMonthlyOrCash"></param>
        /// <param name="txtDateS"></param>
        /// <param name="txtDateE"></param>
        private void GetDefaultData(string ddlDepartment = "", string txtEmpID = "", string txtEmpName = "", string txtDateS = "", string txtDateE = "", string ddlStatus = "")
        {
            ViewBag.DepartmentList = GetDepartmentList(ddlDepartment);
            ViewBag.Department = ddlDepartment;
            ViewBag.EmpID = txtEmpID;
            ViewBag.EmpName = txtEmpName;
            ViewBag.DateTimeS = string.IsNullOrWhiteSpace(txtDateS) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDateS;
            ViewBag.DateTimeE = string.IsNullOrWhiteSpace(txtDateE) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDateE;
            ViewBag.StatusList = GetStatusList(ddlStatus);
            ViewBag.Status = ddlStatus;
        }

        /// <summary>
        /// 部門下拉
        /// </summary>
        /// <param name="selecteddata">被選取的部門</param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartmentList(string selecteddata = "")
        {
            bool isAdmin = false, isAll = false;
            Role roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
            Role casualRole = Services.GetService<RoleService>().GetAll().Where(x => x.Name == "casual總公司").FirstOrDefault();

            if (roleData != null)
            {
                if (!string.IsNullOrEmpty(roleData.RoleParams))
                {
                    dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
                    isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
                }
            }

            if (casualRole == null)
            {
                if (isAdmin)
                {
                    isAll = true;
                }
            }
            else
            {
                if (isAdmin || this.CurrentUser.Employee.RoleID == casualRole.ID)
                {
                    isAll = true;
                }
            }

            List<SelectListItem> listItem = new List<SelectListItem>();
            List<Department> data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

            if (isAll)
            {
                #region Admin or casual總公司
                listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

                foreach (var item in data)
                {
                    listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
                }
                #endregion
            }
            else
            {
                #region 其它
                foreach (var item in data)
                {
                    if (this.CurrentUser.DepartmentCode == item.DepartmentCode)
                    {
                        listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = true });
                    }
                }
                #endregion
            }

            return listItem;
        }

        /// <summary>
        /// 狀態
        /// </summary>
        /// <param name="selecteddata">被選取的狀態</param>
        /// <returns></returns>
        private List<SelectListItem> GetStatusList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "正常", Value = "0", Selected = (selecteddata == "0" ? true : false) });
            listItem.Add(new SelectListItem { Text = "異常-工時短缺", Value = "1", Selected = (selecteddata == "1" ? true : false) });
            listItem.Add(new SelectListItem { Text = "異常-上班超時", Value = "2", Selected = (selecteddata == "2" ? true : false) });
            listItem.Add(new SelectListItem { Text = "異常-刷卡異常", Value = "3", Selected = (selecteddata == "3" ? true : false) });

            return listItem;
        }
    }
}
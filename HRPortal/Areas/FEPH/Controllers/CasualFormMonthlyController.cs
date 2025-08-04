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
using NPOI;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class CasualFormMonthlyController : BaseController
    {
        // GET: FEPH/CasualFormMonthly
        public async Task<ActionResult> Index(int page = 1, string cmd = "", string Grouping = "1", string ddlMonthlyOrCash = "", string txtDateS = "", string txtDateE = "")
        {
            GetDefaultData(Grouping, ddlMonthlyOrCash, txtDateS, txtDateE);

            if (cmd == "Search")
            {
                #region Search
                DateTime BeginDate = Convert.ToDateTime(txtDateS);
                DateTime EndDate = Convert.ToDateTime(txtDateE);

                #region Get Role
                bool isAdmin = false, isAll = false;
                string strDeptCode = "";

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

                if (!isAll)
                {
                    strDeptCode = this.CurrentUser.DepartmentCode;
                }
                #endregion

                #region 資訊格式
                switch (Grouping)
                {
                    case "1":
                        #region 單位名稱
                        ViewBag.Grouping = "Department";
                        #endregion
                        break;
                    case "2":
                        #region 成本中心名稱
                        ViewBag.Grouping = "CC Name";
                        #endregion
                        break;
                    case "3":
                        #region 資料日期
                        ViewBag.Grouping = "Date";
                        #endregion
                        break;
                    default:
                        break;
                }
                #endregion

                CasualFormMonthly result = await HRMApiAdapter.GetCasualFormMonthly(CurrentUser.Employee.Company.CompanyCode, strDeptCode, Grouping, txtDateS, txtDateE, ddlMonthlyOrCash);

                return View(result.CasualFormMonthlyData);
                #endregion
            }
            else if (cmd == "Excel")
            {
                #region Excel
                DateTime BeginDate = Convert.ToDateTime(txtDateS);
                DateTime EndDate = Convert.ToDateTime(txtDateE);

                #region Get Role
                bool isAdmin = false, isAll = false;
                string strDeptCode = "";

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

                if (!isAll)
                {
                    strDeptCode = this.CurrentUser.DepartmentCode;
                }
                #endregion

                string strReportTitle = "", strTableName = "", strFormatType = "";

                #region 資訊格式
                switch (Grouping)
                {
                    case "1":
                        #region 單位名稱
                        strReportTitle = "工讀生月報表(單位別)";
                        strFormatType = "單位名稱";
                        strTableName = "Department";                       
                        #endregion
                        break;
                    case "2":
                        #region 成本中心名稱
                        strReportTitle = "工讀生月報表(成本中心)";
                        strFormatType = "成本中心名稱";
                        strTableName = "CC Name";                      
                        #endregion
                        break;
                    case "3":
                        #region 資料日期
                        strReportTitle = "工讀生月報表(日期別)";
                        strFormatType = "資料日期";
                        strTableName = "Date";                        
                        #endregion
                        break;
                    default:
                        break;
                }
                #endregion

                string strMonthlyOrCash = "";

                #region 月結 / 現金
                switch (ddlMonthlyOrCash)
                {
                    case "1":
                        #region 月結
                        strMonthlyOrCash = "月結";
                        #endregion
                        break;
                    case "2":
                        #region 現金
                        strMonthlyOrCash = "現金";
                        #endregion
                        break;
                    default:
                        #region 全部
                        strMonthlyOrCash = "全部";
                        #endregion
                        break;
                }
                #endregion

                CasualFormMonthly result = await HRMApiAdapter.GetCasualFormMonthly(CurrentUser.Employee.Company.CompanyCode, strDeptCode, Grouping, txtDateS, txtDateE, ddlMonthlyOrCash);

                #region 建立Excel
                XSSFWorkbook wb = new XSSFWorkbook();
                ISheet ws = wb.CreateSheet("CasualFormMonthly");

                #region 版面設定
                ws.SetMargin(MarginType.TopMargin, 1.9);
                ws.SetMargin(MarginType.BottomMargin, 1.1);
                #endregion

                #region 標題1 Style Mark 20170422
                IFont fTitle1 = wb.CreateFont();
                fTitle1.Color = HSSFColor.Black.Index;
                fTitle1.Boldweight = (short)FontBoldWeight.Bold;
                fTitle1.FontHeightInPoints = 14;
                fTitle1.FontName = "新細明體";

                ICellStyle cTitle1 = wb.CreateCellStyle();
                cTitle1.Alignment = HorizontalAlignment.Center;
                cTitle1.VerticalAlignment = VerticalAlignment.Center;
                cTitle1.SetFont(fTitle1);
                #endregion

                #region 標題2 Style Mark 20170422
                IFont fTitle2 = wb.CreateFont();
                fTitle2.Color = HSSFColor.Black.Index;
                fTitle2.Boldweight = (short)FontBoldWeight.Bold;
                fTitle2.FontHeightInPoints = 16;
                fTitle2.FontName = "新細明體";

                ICellStyle cTitle2 = wb.CreateCellStyle();
                cTitle2.Alignment = HorizontalAlignment.Center;
                cTitle2.VerticalAlignment = VerticalAlignment.Center;
                cTitle2.SetFont(fTitle2);
                #endregion

                #region 內容 Style
                IFont font = wb.CreateFont();
                font.Color = HSSFColor.Black.Index;
                //font.Boldweight = (short)FontBoldWeight.Bold;
                font.FontHeightInPoints = 10;
                font.FontName = "新細明體";

                #region css
                ICellStyle css = wb.CreateCellStyle();
                css.Alignment = HorizontalAlignment.Center;
                css.VerticalAlignment = VerticalAlignment.Center;
                css.BorderBottom = BorderStyle.Thin;               //下
                css.BottomBorderColor = HSSFColor.Black.Index;
                css.BorderTop = BorderStyle.Thin;                  //上
                css.TopBorderColor = HSSFColor.Black.Index;
                css.BorderLeft = BorderStyle.Thin;                 //左
                css.LeftBorderColor = HSSFColor.Black.Index;
                css.BorderRight = BorderStyle.Thin;                //右
                css.RightBorderColor = HSSFColor.Black.Index;
                css.SetFont(font);
                #endregion

                #region cs
                ICellStyle cs = wb.CreateCellStyle();
                cs.Alignment = HorizontalAlignment.Center;
                cs.VerticalAlignment = VerticalAlignment.Center;
                cs.BorderBottom = NPOI.SS.UserModel.BorderStyle.Double;               //下
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
                ICellStyle csR = wb.CreateCellStyle();
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
                ICellStyle csL = wb.CreateCellStyle();
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
                ICellStyle cs1 = wb.CreateCellStyle();
                cs1.Alignment = HorizontalAlignment.Center;
                cs1.VerticalAlignment = VerticalAlignment.Center;
                cs1.SetFont(font);
                #endregion

                #region cs1R
                ICellStyle cs1R = wb.CreateCellStyle();
                cs1R.Alignment = HorizontalAlignment.Right;
                cs1R.VerticalAlignment = VerticalAlignment.Center;
                cs1R.SetFont(font);
                #endregion

                #region cs1L
                ICellStyle cs1L = wb.CreateCellStyle();
                cs1L.Alignment = HorizontalAlignment.Left;
                cs1L.VerticalAlignment = VerticalAlignment.Center;
                cs1L.SetFont(font);
                #endregion
                #endregion
                #endregion

                #region 頁首頁尾
                //頁首 - ws.Header , 頁尾 - ws.Footer
                #region 標題
                //ws.Header.Center = string.Format("&B &16 &\"新細明體\"台北遠東國際大飯店\nShangri-La's Far Eastern Plaza Hotel - TPE\n{0}", strReportTitle);
                //ws.Header.Left = string.Format("&12 &\"新細明體\"\n\n\n\nDate：{0} ~ {1}\nRunning Date：{2}", BeginDate.ToString("MMMM dd,yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")), EndDate.ToString("MMMM dd,yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")), DateTime.Now.ToString("yyyy/MM/dd hh:mm:dd"));
                #endregion

                #region 查詢條件
                //ws.Footer.Left = string.Format("&12 &\"新細明體\"\n資訊格式：{0}\n月結 / 現金：{1}\n日期：{2} ~ {3}", strFormatType, strMonthlyOrCash, txtDateS, txtDateE);
                #endregion
                #endregion

                #region 標題 Mark 20170422
                ws.CreateRow(0).CreateCell(0).SetCellValue("台北遠東國際大飯店");
                ws.GetRow(0).GetCell(0).CellStyle = cTitle2;
                ws.AddMergedRegion(new CellRangeAddress(0, 0, 0, 9));

                ws.CreateRow(1).CreateCell(0).SetCellValue("Shangri-La's Far Eastern Plaza Hotel - TPE");
                ws.GetRow(1).GetCell(0).CellStyle = cTitle2;
                ws.AddMergedRegion(new CellRangeAddress(1, 1, 0, 9));

                ws.CreateRow(2).CreateCell(0).SetCellValue(strReportTitle);
                ws.GetRow(2).GetCell(0).CellStyle = cTitle1;
                ws.AddMergedRegion(new CellRangeAddress(2, 2, 0, 9));

                ws.CreateRow(3).CreateCell(0).SetCellValue(string.Format("Date：{0} ~ {1}", BeginDate.ToString("MMMM dd,yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")), EndDate.ToString("MMMM dd,yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"))));
                ws.GetRow(3).GetCell(0).CellStyle = cs1L;
                ws.AddMergedRegion(new CellRangeAddress(3, 3, 0, 9));

                ws.CreateRow(4).CreateCell(0).SetCellValue(string.Format("Running Date：{0}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:dd")));
                ws.GetRow(4).GetCell(0).CellStyle = cs1L;
                ws.AddMergedRegion(new CellRangeAddress(4, 4, 0, 9));
                #endregion

                #region 表頭欄位名稱
                ws.CreateRow(5).CreateCell(0).SetCellValue(strTableName);
                ws.GetRow(5).GetCell(0).CellStyle = cs;
                ws.AddMergedRegion(new CellRangeAddress(5, 5, 0, 1));

                ws.GetRow(5).CreateCell(1).SetCellValue("");
                ws.GetRow(5).GetCell(1).CellStyle = cs;

                ws.GetRow(5).CreateCell(2).SetCellValue("GROSS");
                ws.GetRow(5).GetCell(2).CellStyle = cs;
                ws.AddMergedRegion(new CellRangeAddress(5, 5, 2, 2));

                ws.GetRow(5).CreateCell(3).SetCellValue("HOURS");
                ws.GetRow(5).GetCell(3).CellStyle = cs;
                ws.AddMergedRegion(new CellRangeAddress(5, 5, 3, 3));

                ws.GetRow(5).CreateCell(4).SetCellValue("Employee Premium");
                ws.GetRow(5).GetCell(4).CellStyle = cs;
                ws.AddMergedRegion(new CellRangeAddress(5, 5, 4, 5));

                ws.GetRow(5).CreateCell(5).SetCellValue("");
                ws.GetRow(5).GetCell(5).CellStyle = cs;

                ws.GetRow(5).CreateCell(6).SetCellValue("Employeer Premium");
                ws.GetRow(5).GetCell(6).CellStyle = cs;
                ws.AddMergedRegion(new CellRangeAddress(5, 5, 6, 7));

                ws.GetRow(5).CreateCell(7).SetCellValue("");
                ws.GetRow(5).GetCell(7).CellStyle = cs;

                ws.GetRow(5).CreateCell(8).SetCellValue("NET");
                ws.GetRow(5).GetCell(8).CellStyle = cs;
                ws.AddMergedRegion(new CellRangeAddress(5, 5, 8, 9));

                ws.GetRow(5).CreateCell(9).SetCellValue("");
                ws.GetRow(5).GetCell(9).CellStyle = cs;
                #endregion

                #region 明細資料
                int idx = 5;
                double sCross = 0, sHours = 0, sEmployeePremium = 0, sEmployeerPremium = 0, sNET = 0;

                foreach (var item in result.CasualFormMonthlyData)
                {
                    idx += 1;

                    ws.CreateRow(idx).CreateCell(0).SetCellValue(item.Name);
                    ws.GetRow(idx).GetCell(0).CellStyle = csL;
                    ws.AddMergedRegion(new CellRangeAddress(idx, idx, 0, 1));

                    ws.GetRow(idx).CreateCell(1).SetCellValue("");
                    ws.GetRow(idx).GetCell(1).CellStyle = csR;

                    ws.GetRow(idx).CreateCell(2).SetCellValue(item.Cross.ToString("#,0"));
                    ws.GetRow(idx).GetCell(2).CellStyle = csR;
                    ws.AddMergedRegion(new CellRangeAddress(idx, idx, 2, 2));
                    sCross += item.Cross;

                    ws.GetRow(idx).CreateCell(3).SetCellValue(item.Hours.ToString("#,0.0"));
                    ws.GetRow(idx).GetCell(3).CellStyle = csR;
                    ws.AddMergedRegion(new CellRangeAddress(idx, idx, 3, 3));
                    sHours += item.Hours;

                    ws.GetRow(idx).CreateCell(4).SetCellValue(item.EmployeePremium.ToString("#,0"));
                    ws.GetRow(idx).GetCell(4).CellStyle = csR;
                    ws.AddMergedRegion(new CellRangeAddress(idx, idx, 4, 5));
                    sEmployeePremium += item.EmployeePremium;

                    ws.GetRow(idx).CreateCell(5).SetCellValue("");
                    ws.GetRow(idx).GetCell(5).CellStyle = csR;

                    ws.GetRow(idx).CreateCell(6).SetCellValue(item.EmployeerPremium.ToString("#,0"));
                    ws.GetRow(idx).GetCell(6).CellStyle = csR;
                    ws.AddMergedRegion(new CellRangeAddress(idx, idx, 6, 7));
                    sEmployeerPremium += item.EmployeerPremium;

                    ws.GetRow(idx).CreateCell(7).SetCellValue("");
                    ws.GetRow(idx).GetCell(7).CellStyle = csR;

                    ws.GetRow(idx).CreateCell(8).SetCellValue(item.NET.ToString("#,0"));
                    ws.GetRow(idx).GetCell(8).CellStyle = csR;
                    ws.AddMergedRegion(new CellRangeAddress(idx, idx, 8, 9));
                    sNET += item.NET;

                    ws.GetRow(idx).CreateCell(9).SetCellValue("");
                    ws.GetRow(idx).GetCell(9).CellStyle = csR;
                }
                #endregion

                #region 總計
                idx += 1;

                ws.CreateRow(idx).CreateCell(0).SetCellValue("Grand Total");
                ws.GetRow(idx).GetCell(0).CellStyle = css;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 0, 1));

                ws.GetRow(idx).CreateCell(1).SetCellValue("");
                ws.GetRow(idx).GetCell(1).CellStyle = csR;

                ws.GetRow(idx).CreateCell(2).SetCellValue(sCross.ToString("#,0"));
                ws.GetRow(idx).GetCell(2).CellStyle = csR;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 2, 2));

                ws.GetRow(idx).CreateCell(3).SetCellValue(sHours.ToString("#,0.0"));
                ws.GetRow(idx).GetCell(3).CellStyle = csR;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 3, 3));

                ws.GetRow(idx).CreateCell(4).SetCellValue(sEmployeePremium.ToString("#,0"));
                ws.GetRow(idx).GetCell(4).CellStyle = csR;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 4, 5));

                ws.GetRow(idx).CreateCell(5).SetCellValue("");
                ws.GetRow(idx).GetCell(5).CellStyle = csR;

                ws.GetRow(idx).CreateCell(6).SetCellValue(sEmployeerPremium.ToString("#,0"));
                ws.GetRow(idx).GetCell(6).CellStyle = csR;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 6, 7));

                ws.GetRow(idx).CreateCell(7).SetCellValue("");
                ws.GetRow(idx).GetCell(7).CellStyle = csR;

                ws.GetRow(idx).CreateCell(8).SetCellValue(sNET.ToString("#,0"));
                ws.GetRow(idx).GetCell(8).CellStyle = csR;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 8, 9));

                ws.GetRow(idx).CreateCell(9).SetCellValue("");
                ws.GetRow(idx).GetCell(9).CellStyle = csR;

                #endregion

                #region 查詢條件 Mark 20170422
                idx += 1;

                ws.CreateRow(idx).CreateCell(0).SetCellValue(string.Format("資訊格式：{0}", strFormatType));
                ws.GetRow(idx).GetCell(0).CellStyle = cs1L;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 0, 5));

                idx += 1;

                ws.CreateRow(idx).CreateCell(0).SetCellValue(string.Format("月結 / 現金：{0}", strMonthlyOrCash));
                ws.GetRow(idx).GetCell(0).CellStyle = cs1L;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 0, 5));

                idx += 1;

                ws.CreateRow(idx).CreateCell(0).SetCellValue(string.Format("日期：{0} ~ {1}", txtDateS, txtDateE));
                ws.GetRow(idx).GetCell(0).CellStyle = cs1L;
                ws.AddMergedRegion(new CellRangeAddress(idx, idx, 0, 5));
                #endregion

                #region Autofit
                for (int i = 0; i < 6; i++)
                {
                    ws.AutoSizeColumn(i);
                }
                #endregion

                wb.SetRepeatingRowsAndColumns(0, 0, 5, 0, 0);
                #endregion

                using (var memoryStream = new MemoryStream())
                {
                    wb.Write(memoryStream);
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }
            }
            else
            {
                return View();
            }
        }

        // POST: FEPH/CasualFormMonthly
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string cmd, string Grouping = "", string ddlMonthlyOrCash = "", string txtDateS = "", string txtDateE = "")
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
                    Grouping,
                    ddlMonthlyOrCash,
                    txtDateS,
                    txtDateE
                });
            }

            //重整
            GetDefaultData(Grouping, ddlMonthlyOrCash, txtDateS, txtDateE);

            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="Grouping"></param>
        /// <param name="ddlMonthlyOrCash"></param>
        /// <param name="txtDateS"></param>
        /// <param name="txtDateE"></param>
        private void GetDefaultData(string Grouping = "1", string ddlMonthlyOrCash = "", string txtDateS = "", string txtDateE = "")
        {
            ViewBag.Group = Grouping;
            ViewBag.MonthlyOrCashList = GetMonthlyOrCashList(ddlMonthlyOrCash);
            ViewBag.DateTimeS = string.IsNullOrWhiteSpace(txtDateS) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDateS;
            ViewBag.DateTimeE = string.IsNullOrWhiteSpace(txtDateE) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDateE;
        }

        /// <summary>
        /// 月結 / 現金下拉
        /// </summary>
        /// <param name="selecteddata">被選取的 月結 / 現金</param>
        /// <returns></returns>
        private List<SelectListItem> GetMonthlyOrCashList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "全部", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "月結", Value = "1", Selected = (selecteddata == "1" ? true : false) });
            listItem.Add(new SelectListItem { Text = "現金", Value = "2", Selected = (selecteddata == "2" ? true : false) });

            return listItem;
        }
    }
}
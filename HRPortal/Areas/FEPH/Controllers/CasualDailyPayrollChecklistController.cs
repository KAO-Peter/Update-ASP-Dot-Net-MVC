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
    public class CasualDailyPayrollChecklistController : BaseController
    {
        // GET: FEPH/CasualDailyPayrollChecklist
        public async Task<ActionResult> Index(int page = 1, string cmd = "", string txtENo = "", string ddlMonthlyOrCash = "", string txtDateS = "", string txtDateE = "", string ReportType = "")
        {
            GetDefaultData(ddlMonthlyOrCash, txtENo, txtDateS, txtDateE, ReportType);

            if (cmd == "Search")
            {
                #region Search
                DateTime BeginDate = txtDateS == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateS));
                DateTime EndDate = txtDateE == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateE));

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

                CasualDailyPayrollChecklist result = await HRMApiAdapter.GetCasualDailyPayrollChecklist(CurrentUser.Employee.Company.CompanyCode, strDeptCode, txtENo, txtDateS, txtDateE, ddlMonthlyOrCash, ReportType);
                #endregion

                return View(result);
            }
            else if (cmd == "Excel")
            {
                #region Excel
                XSSFWorkbook wb = null;
                ISheet ws = null;
                IFont fTitle1 = null, fTitle2 = null, font = null;
                ICellStyle cTitle1 = null, cTitle2 = null, csBR = null, cs = null, csR = null, csL = null, cs1 = null, cs1R = null, cs1L = null;

                int idx = 0;
                int sCount = 0;
                double sWorkHours = 0, sAllowance = 0, sSalary = 0, sCompanyLabor = 0, sPersonalLabor = 0, sCompanyRetirement = 0;

                DateTime BeginDate = txtDateS == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateS));
                DateTime EndDate = txtDateE == "" ? DateTime.Now.Date : (DateTime.Parse(txtDateE));

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

                CasualDailyPayrollChecklist result = await HRMApiAdapter.GetCasualDailyPayrollChecklist(CurrentUser.Employee.Company.CompanyCode, strDeptCode, txtENo, txtDateS, txtDateE, ddlMonthlyOrCash, ReportType);

                switch (result.ReportType)
                {
                    case "1":
                        #region 明細
                        #region 建立Excel
                        wb = new XSSFWorkbook();
                        ws = wb.CreateSheet("CasualDailyPayrollChecklist");
                        ws.PrintSetup.Landscape = true;

                        #region 版面設定
                        ws.SetMargin(MarginType.TopMargin, 0.2);
                        ws.SetMargin(MarginType.BottomMargin, 0.2);
                        ws.SetMargin(MarginType.LeftMargin, 0.2);
                        ws.SetMargin(MarginType.RightMargin, 0.2);
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

                        #region 頁首頁尾 Mark 20170422
                        ////頁首 - ws.Header , 頁尾 - ws.Footer
                        //#region 標題
                        ////ws.Header.Center = string.Format("&B &14 &\"新細明體\" {0}\n{1}\n工讀生薪工資料檢查表\nEmployee Casual Payroll Check List\n資料起迄時間：{2} ~ {3}\nData Range：{2} ~ {3}", result.CompanyFullName, result.CompanyNameEn, txtDateS, txtDateE);

                        //ws.Header.Center = "&B &14 &\"新細明體\"" + result.CompanyFullName + "\n" + result.CompanyNameEn + "\nEmployee Casual Payroll Check List\n資料起迄時間：\nData Range：";
                        //ws.Header.Left = string.Format("&12 &\"新細明體\" \n\n\n\n\n\n\n\nPROGRAM NAME：CasualDailyPayrollChecklist\nRUNNING-DATE：{0}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                        //ws.Header.Right = "&12 &\"新細明體\" \n\n\n\n\n\n\n\nPage：&P";
                        //#endregion
                        #endregion

                        #region 標題
                        ws.CreateRow(0).CreateCell(0).SetCellValue(result.CompanyFullName);
                        ws.GetRow(0).GetCell(0).CellStyle = cTitle2;
                        ws.AddMergedRegion(new CellRangeAddress(0, 0, 0, 14));

                        ws.CreateRow(1).CreateCell(0).SetCellValue(result.CompanyNameEn);
                        ws.GetRow(1).GetCell(0).CellStyle = cTitle2;
                        ws.AddMergedRegion(new CellRangeAddress(1, 1, 0, 14));

                        ws.CreateRow(2).CreateCell(0).SetCellValue("工讀生薪工資料檢查表");
                        ws.GetRow(2).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(2, 2, 0, 14));

                        ws.CreateRow(3).CreateCell(0).SetCellValue("Employee Casual Payroll Check List");
                        ws.GetRow(3).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(3, 3, 0, 14));

                        ws.CreateRow(4).CreateCell(0).SetCellValue(string.Format("資料起迄時間：{0} ~ {1}", txtDateS, txtDateE));
                        ws.GetRow(4).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(4, 4, 0, 14));

                        ws.CreateRow(5).CreateCell(0).SetCellValue(string.Format("Data Range：{0} ~ {1}", txtDateS, txtDateE));
                        ws.GetRow(5).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(5, 5, 0, 14));

                        ws.CreateRow(6).CreateCell(0).SetCellValue("PROGRAM NAME：CasualDailyPayrollChecklist");
                        ws.GetRow(6).GetCell(0).CellStyle = cs1L;
                        ws.AddMergedRegion(new CellRangeAddress(6, 6, 0, 6));

                        //ws.GetRow(6).CreateCell(7).SetCellValue("Page：");
                        ws.GetRow(6).CreateCell(7).SetCellValue("");
                        ws.GetRow(6).GetCell(7).CellStyle = cs1R;
                        ws.AddMergedRegion(new CellRangeAddress(6, 6, 7, 14));

                        ws.CreateRow(7).CreateCell(0).SetCellValue(string.Format("RUNNING-DATE：{0}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm")));
                        ws.GetRow(7).GetCell(0).CellStyle = cs1L;
                        ws.AddMergedRegion(new CellRangeAddress(7, 7, 0, 14));
                        #endregion

                        #region 表頭欄位名稱
                        ws.CreateRow(8).CreateCell(0).SetCellValue("員工工號\nEmployee");
                        ws.GetRow(8).GetCell(0).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(1).SetCellValue("員工姓名\nName");
                        ws.GetRow(8).GetCell(1).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(2).SetCellValue("部門\nDept.");
                        ws.GetRow(8).GetCell(2).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(3).SetCellValue("身分證字號\nI.D.");
                        ws.GetRow(8).GetCell(3).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(4).SetCellValue("出生日期\nDate of");
                        ws.GetRow(8).GetCell(4).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(5).SetCellValue("日期\nDate");
                        ws.GetRow(8).GetCell(5).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(6).SetCellValue("核薪\nWage");
                        ws.GetRow(8).GetCell(6).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(7).CellStyle = csBR;
                        ws.AddMergedRegion(new CellRangeAddress(8, 8, 6, 7));

                        ws.GetRow(8).CreateCell(8).SetCellValue("總時數\nTotal Hours");
                        ws.GetRow(8).GetCell(8).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(9).SetCellValue("車馬費\nHonorarium");
                        ws.GetRow(8).GetCell(9).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(10).SetCellValue("薪資\nSalary");
                        ws.GetRow(8).GetCell(10).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(11).SetCellValue("公司負擔勞保費\nEmployee Persion");
                        ws.GetRow(8).GetCell(11).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(12).SetCellValue("員工負擔勞保費\nEmployer Persion");
                        ws.GetRow(8).GetCell(12).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(13).SetCellValue("公司提撥金額\nMandatory Persion Contribution");
                        ws.GetRow(8).GetCell(13).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(14).SetCellValue("備註\nNote");
                        ws.GetRow(8).GetCell(14).CellStyle = csBR;
                        #endregion

                        #region 明細資料
                        idx = 8;
                        sCount = 0;
                        sWorkHours = 0;
                        sAllowance = 0;
                        sSalary = 0;
                        sCompanyLabor = 0;
                        sPersonalLabor = 0;
                        sCompanyRetirement = 0;

                        foreach (var item in result.DetailData)
                        {
                            idx += 1;
                            sCount += 1;

                            ws.CreateRow(idx).CreateCell(0).SetCellValue(item.EmpID);
                            ws.GetRow(idx).GetCell(0).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(1).SetCellValue(item.EmpName);
                            ws.GetRow(idx).GetCell(1).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(2).SetCellValue(item.dept);
                            ws.GetRow(idx).GetCell(2).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(3).SetCellValue(item.IDNumber);
                            ws.GetRow(idx).GetCell(3).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(4).SetCellValue(item.Birthday);
                            ws.GetRow(idx).GetCell(4).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(5).SetCellValue(item.ExcuteDate);
                            ws.GetRow(idx).GetCell(5).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(6).SetCellValue(item.SalaryUnit);
                            ws.GetRow(idx).GetCell(6).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(7).SetCellValue(item.Wage.ToString("N0"));
                            ws.GetRow(idx).GetCell(7).CellStyle = csR;

                            ws.GetRow(idx).CreateCell(8).SetCellValue(item.WorkHours.ToString("N1"));
                            ws.GetRow(idx).GetCell(8).CellStyle = csR;
                            sWorkHours += item.WorkHours;

                            ws.GetRow(idx).CreateCell(9).SetCellValue(item.Allowance.ToString("N0"));
                            ws.GetRow(idx).GetCell(9).CellStyle = csR;
                            sAllowance += item.Allowance;

                            ws.GetRow(idx).CreateCell(10).SetCellValue(item.Salary.ToString("N0"));
                            ws.GetRow(idx).GetCell(10).CellStyle = csR;
                            sSalary += item.Salary;

                            ws.GetRow(idx).CreateCell(11).SetCellValue(item.CompanyLabor.ToString("N0"));
                            ws.GetRow(idx).GetCell(11).CellStyle = csR;
                            sCompanyLabor += item.CompanyLabor;

                            ws.GetRow(idx).CreateCell(12).SetCellValue(item.PersonalLabor.ToString("N0"));
                            ws.GetRow(idx).GetCell(12).CellStyle = csR;
                            sPersonalLabor += item.PersonalLabor;

                            ws.GetRow(idx).CreateCell(13).SetCellValue(item.CompanyRetirement.ToString("N0"));
                            ws.GetRow(idx).GetCell(13).CellStyle = csR;
                            sCompanyRetirement += item.CompanyRetirement;

                            ws.GetRow(idx).CreateCell(14).SetCellValue(item.Note);
                            ws.GetRow(idx).GetCell(14).CellStyle = csL;
                        }
                        #endregion

                        #region 總計
                        idx += 1;

                        ws.CreateRow(idx).CreateCell(0).CellStyle = csL;

                        ws.GetRow(idx).CreateCell(1).SetCellValue(string.Format("Grand Total：{0}人", sCount));
                        ws.GetRow(idx).GetCell(1).CellStyle = csL;

                        ws.GetRow(idx).CreateCell(2).CellStyle = csL;
                        ws.GetRow(idx).CreateCell(3).CellStyle = csL;
                        ws.GetRow(idx).CreateCell(4).CellStyle = csL;
                        ws.GetRow(idx).CreateCell(5).CellStyle = csL;
                        ws.GetRow(idx).CreateCell(6).CellStyle = csL;
                        ws.GetRow(idx).CreateCell(7).CellStyle = csL;

                        ws.AddMergedRegion(new CellRangeAddress(idx, idx, 1, 7));

                        ws.GetRow(idx).CreateCell(8).SetCellValue(sWorkHours.ToString("N1"));
                        ws.GetRow(idx).GetCell(8).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(9).SetCellValue(sAllowance.ToString("N0"));
                        ws.GetRow(idx).GetCell(9).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(10).SetCellValue(sSalary.ToString("N0"));
                        ws.GetRow(idx).GetCell(10).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(11).SetCellValue(sCompanyLabor.ToString("N0"));
                        ws.GetRow(idx).GetCell(11).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(12).SetCellValue(sPersonalLabor.ToString("N0"));
                        ws.GetRow(idx).GetCell(12).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(13).SetCellValue(sCompanyRetirement.ToString("N0"));
                        ws.GetRow(idx).GetCell(13).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(14).CellStyle = csL;
                        #endregion

                        #region Autofit
                        for (int i = 0; i < 15; i++)
                        {
                            ws.AutoSizeColumn(i);
                        }
                        #endregion

                        wb.SetRepeatingRowsAndColumns(0, 0, 14, 0, 8);

                        using (var memoryStream = new MemoryStream())
                        {
                            wb.Write(memoryStream);
                            return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        }
                        #endregion
                        break;
                    case "2":
                        #region 彙總
                        #region 建立Excel
                        wb = new XSSFWorkbook();
                        ws = wb.CreateSheet("CasualDailyPayrollChecklist");
                        ws.PrintSetup.Landscape = true;

                        #region 版面設定
                        ws.SetMargin(MarginType.TopMargin, 0.2);
                        ws.SetMargin(MarginType.BottomMargin, 0.2);
                        ws.SetMargin(MarginType.LeftMargin, 0.2);
                        ws.SetMargin(MarginType.RightMargin, 0.2);
                        #endregion

                        #region 標題1 Style Mark 20170422
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

                        #region 標題2 Style Mark 20170422
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

                        #region 頁首頁尾 Mark 20170422
                        ////頁首 - ws.Header , 頁尾 - ws.Footer
                        //#region 標題
                        //ws.Header.Center = string.Format("&B &14 &\"新細明體\"{0}\n{1}\n工讀生薪工資料檢查表\nEmployee Casual Payroll Check List\n資料起迄時間：{2} ~ {3}\nData Range：{2} ~ {3}", result.CompanyFullName, result.CompanyNameEn, txtDateS, txtDateE);
                        //ws.Header.Left = string.Format("&12 &\"新細明體\"\n\n\n\n\n\n\n\nPROGRAM NAME：CasualDailyPayrollChecklist\nRUNNING-DATE：{0}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                        //ws.Header.Right = "&12 &\"新細明體\"\n\n\n\n\n\n\n\nPage：&P";
                        //#endregion
                        #endregion

                        #region 標題 Mark 20170422
                        ws.CreateRow(0).CreateCell(0).SetCellValue(result.CompanyFullName);
                        ws.GetRow(0).GetCell(0).CellStyle = cTitle2;
                        ws.AddMergedRegion(new CellRangeAddress(0, 0, 0, 9));

                        ws.CreateRow(1).CreateCell(0).SetCellValue(result.CompanyNameEn);
                        ws.GetRow(1).GetCell(0).CellStyle = cTitle2;
                        ws.AddMergedRegion(new CellRangeAddress(1, 1, 0, 9));

                        ws.CreateRow(2).CreateCell(0).SetCellValue("工讀生薪工資料檢查表");
                        ws.GetRow(2).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(2, 2, 0, 9));

                        ws.CreateRow(3).CreateCell(0).SetCellValue("Employee Casual Payroll Check List");
                        ws.GetRow(3).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(3, 3, 0, 9));

                        ws.CreateRow(4).CreateCell(0).SetCellValue(string.Format("資料起迄時間：{0} ~ {1}", txtDateS, txtDateE));
                        ws.GetRow(4).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(4, 4, 0, 9));

                        ws.CreateRow(5).CreateCell(0).SetCellValue(string.Format("Data Range：{0} ~ {1}", txtDateS, txtDateE));
                        ws.GetRow(5).GetCell(0).CellStyle = cTitle1;
                        ws.AddMergedRegion(new CellRangeAddress(5, 5, 0, 9));

                        ws.CreateRow(6).CreateCell(0).SetCellValue("PROGRAM NAME：CasualDailyPayrollChecklist");
                        ws.GetRow(6).GetCell(0).CellStyle = cs1L;
                        ws.AddMergedRegion(new CellRangeAddress(6, 6, 0, 4));

                        //ws.GetRow(6).CreateCell(5).SetCellValue("Page：");
                        ws.GetRow(6).CreateCell(5).SetCellValue("");
                        ws.GetRow(6).GetCell(5).CellStyle = cs1R;
                        ws.AddMergedRegion(new CellRangeAddress(6, 6, 5, 9));

                        ws.CreateRow(7).CreateCell(0).SetCellValue(string.Format("RUNNING-DATE：{0}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm")));
                        ws.GetRow(7).GetCell(0).CellStyle = cs1L;
                        ws.AddMergedRegion(new CellRangeAddress(7, 7, 0, 9));
                        #endregion

                        #region 表頭欄位名稱
                        ws.CreateRow(8).CreateCell(0).SetCellValue("員工工號\nEmployee");
                        ws.GetRow(8).GetCell(0).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(1).SetCellValue("員工姓名\nName");
                        ws.GetRow(8).GetCell(1).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(2).SetCellValue("身分證字號\nI.D.");
                        ws.GetRow(8).GetCell(2).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(3).SetCellValue("出生日期\nDate of");
                        ws.GetRow(8).GetCell(3).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(4).SetCellValue("總時數\nTotal Hours");
                        ws.GetRow(8).GetCell(4).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(5).SetCellValue("車馬費\nHonorarium");
                        ws.GetRow(8).GetCell(5).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(6).SetCellValue("薪資\nSalary");
                        ws.GetRow(8).GetCell(6).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(7).SetCellValue("公司負擔勞保費\nEmployee Persion");
                        ws.GetRow(8).GetCell(7).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(8).SetCellValue("員工負擔勞保費\nEmployer Persion");
                        ws.GetRow(8).GetCell(8).CellStyle = csBR;

                        ws.GetRow(8).CreateCell(9).SetCellValue("公司提撥金額\nMandatory Persion Contribution");
                        ws.GetRow(8).GetCell(9).CellStyle = csBR;
                        #endregion

                        #region 明細資料
                        idx = 8;
                        sCount = 0;
                        sWorkHours = 0;
                        sAllowance = 0;
                        sSalary = 0;
                        sCompanyLabor = 0;
                        sPersonalLabor = 0;
                        sCompanyRetirement = 0;

                        foreach (var item in result.TotalData)
                        {
                            idx += 1;
                            sCount += 1;

                            ws.CreateRow(idx).CreateCell(0).SetCellValue(item.EmpID);
                            ws.GetRow(idx).GetCell(0).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(1).SetCellValue(item.EmpName);
                            ws.GetRow(idx).GetCell(1).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(2).SetCellValue(item.IDNumber);
                            ws.GetRow(idx).GetCell(2).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(3).SetCellValue(item.Birthday);
                            ws.GetRow(idx).GetCell(3).CellStyle = csL;

                            ws.GetRow(idx).CreateCell(4).SetCellValue(item.WorkHours.ToString("N1"));
                            ws.GetRow(idx).GetCell(4).CellStyle = csR;
                            sWorkHours += item.WorkHours;

                            ws.GetRow(idx).CreateCell(5).SetCellValue(item.Allowance.ToString("N0"));
                            ws.GetRow(idx).GetCell(5).CellStyle = csR;
                            sAllowance += item.Allowance;

                            ws.GetRow(idx).CreateCell(6).SetCellValue(item.Salary.ToString("N0"));
                            ws.GetRow(idx).GetCell(6).CellStyle = csR;
                            sSalary += item.Salary;

                            ws.GetRow(idx).CreateCell(7).SetCellValue(item.CompanyLabor.ToString("N0"));
                            ws.GetRow(idx).GetCell(7).CellStyle = csR;
                            sCompanyLabor += item.CompanyLabor;

                            ws.GetRow(idx).CreateCell(8).SetCellValue(item.PersonalLabor.ToString("N0"));
                            ws.GetRow(idx).GetCell(8).CellStyle = csR;
                            sPersonalLabor += item.PersonalLabor;

                            ws.GetRow(idx).CreateCell(9).SetCellValue(item.CompanyRetirement.ToString("N0"));
                            ws.GetRow(idx).GetCell(9).CellStyle = csR;
                            sCompanyRetirement += item.CompanyRetirement;
                        }
                        #endregion

                        #region 總計
                        idx += 1;

                        ws.CreateRow(idx).CreateCell(0).CellStyle = csL;

                        ws.GetRow(idx).CreateCell(1).SetCellValue(string.Format("Grand Total：{0}人", sCount));
                        ws.GetRow(idx).GetCell(1).CellStyle = csL;

                        ws.GetRow(idx).CreateCell(2).CellStyle = csL;
                        ws.GetRow(idx).CreateCell(3).CellStyle = csL;

                        ws.AddMergedRegion(new CellRangeAddress(idx, idx, 1, 3));

                        ws.GetRow(idx).CreateCell(4).SetCellValue(sWorkHours.ToString("N1"));
                        ws.GetRow(idx).GetCell(4).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(5).SetCellValue(sAllowance.ToString("N0"));
                        ws.GetRow(idx).GetCell(5).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(6).SetCellValue(sSalary.ToString("N0"));
                        ws.GetRow(idx).GetCell(6).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(7).SetCellValue(sCompanyLabor.ToString("N0"));
                        ws.GetRow(idx).GetCell(7).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(8).SetCellValue(sPersonalLabor.ToString("N0"));
                        ws.GetRow(idx).GetCell(8).CellStyle = csR;

                        ws.GetRow(idx).CreateCell(9).SetCellValue(sCompanyRetirement.ToString("N0"));
                        ws.GetRow(idx).GetCell(9).CellStyle = csR;
                        #endregion

                        #region Autofit
                        for (int i = 0; i < 10; i++)
                        {
                            ws.AutoSizeColumn(i);
                        }
                        #endregion

                        wb.SetRepeatingRowsAndColumns(0, 0, 9, 0, 8);

                        using (var memoryStream = new MemoryStream())
                        {
                            wb.Write(memoryStream);
                            return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        }
                        #endregion
                        break;
                    default:
                        return View();
                        break;
                }
                #endregion
            }
            else
            {
                return View();
            }
        }

        // POST: FEPH/CasualDailyPayrollChecklist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string cmd, string txtENo = "", string ddlMonthlyOrCash = "", string txtDateS = "", string txtDateE = "", string ReportType = "")
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
                    txtENo,
                    ddlMonthlyOrCash,
                    txtDateS,
                    txtDateE,
                    ReportType
                });
            }

            //重整
            GetDefaultData(ddlMonthlyOrCash, txtENo, txtDateS, txtDateE, ReportType);

            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="Grouping"></param>
        /// <param name="ddlMonthlyOrCash"></param>
        /// <param name="txtDateS"></param>
        /// <param name="txtDateE"></param>
        private void GetDefaultData(string ddlMonthlyOrCash = "", string txtENo = "", string txtDateS = "", string txtDateE = "", string ReportType = "")
        {
            ViewBag.MonthlyOrCashList = GetMonthlyOrCashList(ddlMonthlyOrCash);
            ViewBag.EmpNo = string.IsNullOrWhiteSpace(txtENo) ? "" : txtENo;
            ViewBag.DateTimeS = string.IsNullOrWhiteSpace(txtDateS) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDateS;
            ViewBag.DateTimeE = string.IsNullOrWhiteSpace(txtDateE) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDateE;
            ViewBag.ReportType = string.IsNullOrWhiteSpace(ReportType) ? "1" : ReportType;
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

        /// <summary>
        /// 部門下拉
        /// </summary>
        /// <param name="selecteddata">被選取的部門</param>
        /// <returns></returns>
        //private List<SelectListItem> GetDepartmentList(string selecteddata = "")
        //{
        //    bool isAdmin = false, isAll = false;
        //    Role roleData = Services.GetService<RoleService>().GetAll().Where(x => x.ID == this.CurrentUser.Employee.RoleID).FirstOrDefault();
        //    Role casualRole = Services.GetService<RoleService>().GetAll().Where(x => x.Name == "casual總公司").FirstOrDefault();

        //    if (roleData != null)
        //    {
        //        if (!string.IsNullOrEmpty(roleData.RoleParams))
        //        {
        //            dynamic roleParams = System.Web.Helpers.Json.Decode(roleData.RoleParams);
        //            isAdmin = (roleParams.is_admin != null && roleParams.is_admin);
        //        }
        //    }

        //    if (casualRole == null)
        //    {
        //        if (isAdmin)
        //        {
        //            isAll = true;
        //        }
        //    }
        //    else
        //    {
        //        if (isAdmin || this.CurrentUser.Employee.RoleID == casualRole.ID)
        //        {
        //            isAll = true;
        //        }
        //    }

        //    List<SelectListItem> listItem = new List<SelectListItem>();
        //    List<Department> data = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled).OrderBy(x => x.DepartmentCode).ToList();

        //    if (isAll)
        //    {
        //        #region Admin or casual總公司
        //        listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

        //        foreach (var item in data)
        //        {
        //            listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = (selecteddata == item.DepartmentCode ? true : false) });
        //        }
        //        #endregion
        //    }
        //    else
        //    {
        //        #region 其它
        //        foreach (var item in data)
        //        {
        //            if (this.CurrentUser.DepartmentCode == item.DepartmentCode)
        //            {
        //                listItem.Add(new SelectListItem { Text = item.DepartmentName, Value = item.DepartmentCode, Selected = true });
        //            }
        //        }
        //        #endregion
        //    }

        //    return listItem;
        //}

        /// <summary>
        /// 取得員工資料
        /// </summary>
        /// <param name="EmpID"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetEmp(string EmpID)
        {
            int currentPage = 1;

            #region 呼叫 WebApi - GetEmpDataCasual 臨時員工資料查詢
            GetEmpDataCasual data = await HRMApiAdapter.GetCasualList(CurrentUser.CompanyCode, EmpID, "", currentPage, currentPageSize);
            #endregion

            string strEmpName = "";

            if (data != null)
            {
                if (data.EmployeeData != null && data.EmployeeData.Count > 0)
                {
                    strEmpName = data.EmployeeData.FirstOrDefault().EmpName;
                }
            }

            return Json(new
            {
                Msg = "",
                EmpName = strEmpName
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: FEPH/CasualDailyPayrollChecklist/Emp
        public ActionResult Emp()
        {
            //ViewBag.DepartmentList = GetDepartmentList();
            return PartialView("_EmpDialog", null);
        }

        // POST: FEPH/CasualDailyPayrollChecklist/Emp
        [HttpPost]
        public async Task<ActionResult> Emp(int page = 1, string txtEmpNo = "", string txtIDNumber = "")
        {
            int currentPage = page < 1 ? 1 : page;

            #region 呼叫 WebApi - GetEmpDataCasual 臨時員工資料查詢
            GetEmpDataCasual data = await HRMApiAdapter.GetCasualList(CurrentUser.CompanyCode, txtEmpNo, txtIDNumber, currentPage, currentPageSize);
            #endregion

            #region Create Data
            StringBuilder dataResult = new StringBuilder();

            foreach (var item in data.EmployeeData)
            {
                dataResult.Append(string.Format("<tr id='resultDataTr' empno='{0}' empname='{1}' role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' style='cursor:pointer'>", item.EmpID, item.EmpName));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal' aria-describedby='grid-table_notification_count'>{0}</td>", item.EmpID));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal' aria-describedby='grid-table_notification_count'>{0}</td>", item.EmpName));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal' aria-describedby='grid-table_notification_count'>{0}</td>", item.IDNumber));
                dataResult.Append(string.Format("<td role='gridcell' style='white-space:normal' aria-describedby='grid-table_notification_count'>{0}</td>", item.DeptName));
                dataResult.Append("</tr>");
            }
            #endregion

            #region Create Page
            Dictionary<int, string> pageObj = new Dictionary<int, string>();

            int sIdx = (currentPage - 1) * currentPageSize + 1;
            int eIdx = currentPage * currentPageSize;
            if (eIdx > data.DataCount) eIdx = data.DataCount;

            int sPidx = currentPage - 2;
            if (sPidx < 1) sPidx = 1;

            int ePidx = 4 + sPidx;
            if (ePidx > data.PageCount)
            {
                ePidx = data.PageCount;
                sPidx = ePidx - 4;
                if (sPidx < 1) sPidx = 1;
            }

            for (int i = sPidx; i < currentPage; i++)
            {
                pageObj.Add(i, " style='cursor:pointer'");
            }

            pageObj.Add(currentPage, " class='active'");

            for (int i = (currentPage + 1); i <= ePidx; i++)
            {
                pageObj.Add(i, " style='cursor:pointer'");
            }

            StringBuilder pageResult = new StringBuilder();

            pageResult.Append("<div style='text-align:center'>");
            pageResult.Append("<div class='pagination-container'>");
            pageResult.Append("<ul class='pagination'>");
            pageResult.Append(string.Format("<li id='first' name='pagelist' class='{0}PagedList-skipToFirst'{1}><a>««</a></li>", (currentPage == 1 ? "disabled " : ""), (currentPage == 1 ? "" : " style='cursor:pointer'")));
            pageResult.Append(string.Format("<li id='prev' name='pagelist' class='{0}PagedList-skipToPrevious'{1}><a>«</a></li>", (currentPage == 1 ? "disabled " : ""), (currentPage == 1 ? "" : " style='cursor:pointer'")));

            if (sPidx != 1)
            {
                pageResult.Append("<li class='disabled PagedList-ellipses'><a>…</a></li>");
            }

            foreach (KeyValuePair<int, string> item in pageObj)
            {
                pageResult.Append(string.Format("<li id='page' name='pagelist'{0}><a>{1}</a></li>", item.Value, item.Key));
            }

            if (ePidx != data.PageCount)
            {
                pageResult.Append("<li class='disabled PagedList-ellipses'><a>…</a></li>");
            }

            pageResult.Append(string.Format("<li id='next' name='pagelist' class='{0}PagedList-skipToNext'{1}><a>»</a></li>", (currentPage == data.PageCount ? "disabled " : ""), (currentPage == data.PageCount ? "" : " style='cursor:pointer'")));
            pageResult.Append(string.Format("<li id='last' name='pagelist' class='{0}PagedList-skipToLast'{1}><a>»»</a>", (currentPage == data.PageCount ? "disabled " : ""), (currentPage == data.PageCount ? "" : " style='cursor:pointer'")));
            pageResult.Append("</ul");
            pageResult.Append("</div>");
            pageResult.Append("<div class='pagination-container'>");
            pageResult.Append("<ul class='pagination'>");
            pageResult.Append(string.Format("<li class='disabled PagedList-pageCountAndLocation'><a>第{0}頁/共{1}頁</a></li>", currentPage, data.PageCount));
            pageResult.Append(string.Format("<li class='disabled PagedList-pageCountAndLocation'><a>{0}-{1} 共{2}列</a></li>", sIdx, eIdx, data.DataCount));
            pageResult.Append("</ul>");
            pageResult.Append("</div>");
            pageResult.Append("</div>");
            #endregion

            return Json(new { data = dataResult.ToString(), page = pageResult.ToString(), pagecount = data.PageCount }, JsonRequestBehavior.AllowGet);
        }
    }
}
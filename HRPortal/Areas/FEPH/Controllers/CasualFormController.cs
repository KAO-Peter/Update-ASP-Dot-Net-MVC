using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HRPortal.Areas.FEPH.Controllers
{
    public class CasualFormController : BaseController
    {
        // GET: FEPH/CasualForm
        public async Task<ActionResult> Index(int page = 1, string cmd = "", string txtDate = "", string ddlTimeType = "", string ddlDepartment = "", string txtEmpID = "", string txtEmpName = "")
        {
            GetDefaultData(txtDate, ddlTimeType, ddlDepartment, txtEmpID, txtEmpName);

            int currentPage = page < 1 ? 1 : page;

            if (cmd == "Search")
            {
                #region Search
                List<CasualFormViewModel> viewModel = new List<CasualFormViewModel>();
                DateTime excuteDate = DateTime.Parse(txtDate);
                DateTime editDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime tempDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4);

                if (DateTime.Now < tempDate)
                {
                    editDate = editDate.AddMonths(-1);
                }

                //Get CasualForm
                var ds = Services.GetService<CasualFormService>().GetCasualFormLists(CurrentUser.CompanyID, excuteDate, ddlTimeType, ddlDepartment, txtEmpID, txtEmpName);

                foreach (var item in ds)
                {
                    var CasualCycleData = Services.GetService<CasualCycleService>().GetAll().Where(x => x.CompanyID == item.CompanyID && x.CasualFormNo == item.CasualFormNo).FirstOrDefault();

                    viewModel.Add(new CasualFormViewModel
                    {
                        casualFormData = item,
                        LockFlag = CasualCycleData == null ? "n" : CasualCycleData.LockFlag,
                        IsUpdate = item.ExcuteDate < editDate ? false : true,
                        IsDelete = item.ExcuteDate < editDate ? false : true
                    });
                }

                return View(viewModel.ToPagedList(currentPage, currentPageSize));
                #endregion
            }
            else if (cmd == "PDF")
            {
                #region PDF
                DateTime excuteDate = DateTime.Parse(txtDate);

                //Get CasualForm
                var ds = Services.GetService<CasualFormService>().GetCasualFormLists(CurrentUser.CompanyID, excuteDate, ddlTimeType, ddlDepartment, txtEmpID, txtEmpName);

                if (ds.Count == 0)
                {
                    TempData["message"] = "無資料";
                    return View();
                }
                else
                {
                    //呼叫 WebApi - GetCasualFormLabor
                    CasualFormLabor result = await HRMApiAdapter.GetCasualFormLabor(CurrentUser.Employee.Company.CompanyCode, ddlDepartment, txtDate, ddlTimeType, txtEmpID, txtEmpName);

                    byte[] pdfFile;

                    #region Page Data
                    int sidx = 0, eidx = 0;
                    int totalPage = 2, pageSize = 20;

                    if (ds.Count % pageSize > 0)
                    {
                        totalPage = ds.Count / pageSize + 1;
                    }
                    else
                    {
                        totalPage = ds.Count / pageSize;
                    }
                    #endregion

                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        #region PDF Object
                        Document doc = new Document(PageSize.A4.Rotate(), 30, 30, 70, 15);
                        PdfWriter writer = PdfWriter.GetInstance(doc, outputStream);
                        PdfPTable table;
                        PdfPCell tr;
                        #endregion

                        #region 部門名稱
                        var DeptName = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == ddlDepartment).Select(x => x.DepartmentName).FirstOrDefault();
                        #endregion

                        #region 頁首頁尾事件
                        PDFEventHandler docEvent = new PDFEventHandler()
                        {
                            ImageHeader = null,
                            HireDateTime = excuteDate,
                            ApplyDateTime = DateTime.Now,
                            DeptName = ddlDepartment == "" ? "全部" : DeptName,
                            EmpName = this.CurrentUser.Employee.EmployeeName
                        };
                        writer.PageEvent = docEvent;
                        #endregion

                        #region 中文字體
                        BaseFont bfChinese = BaseFont.CreateFont(@"C:\WINDOWS\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                        #endregion

                        doc.Open();

                        //string nowEmpID = "";

                        double defaultWorkHourTotal = 0;
                        double workHourTotal = 0;
                        double salaryTotal = 0;
                        double allowanceTotal = 0;
                        double personalLaborTotal = 0;

                        for (int i = 0; i < totalPage; i++)
                        {
                            double defaultWorkHourSum = 0;
                            double workHourSum = 0;
                            double salarySum = 0;
                            double allowanceSum = 0;
                            double personalLaborSum = 0;
                            string nowEmpID ="";
                            string nowDate = "";

                            #region 臨時工資料
                            table = new PdfPTable(100);
                            table.WidthPercentage = 100;

                            #region Table Header
                            #region 第一列
                            tr = new PdfPCell();
                            tr.Colspan = 3;
                            tr.Rowspan = 2;
                            tr.FixedHeight = 40f;
                            tr.BorderWidthLeft = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("員工編號\nCasual ID", new Font(bfChinese, 8)));
                            tr.Colspan = 7;
                            tr.Rowspan = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("姓名\nName", new Font(bfChinese, 8)));
                            tr.Colspan = 15;
                            tr.Rowspan = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Estimated 預計起迄時間/工作時數", new Font(bfChinese, 8)));
                            tr.Colspan = 18;
                            tr.FixedHeight = 20f;
                            tr.BorderWidthTop = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Actual 實際起迄時間/工作時數", new Font(bfChinese, 8)));
                            tr.Colspan = 18;
                            tr.BorderWidthTop = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("核薪\nRate", new Font(bfChinese, 8)));
                            tr.Colspan = 10;
                            tr.Rowspan = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("車馬費\nTransport", new Font(bfChinese, 8)));
                            tr.Colspan = 9;
                            tr.Rowspan = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("勞保自付\nEmployee Premium", new Font(bfChinese, 8)));
                            tr.Colspan = 10;
                            tr.Rowspan = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("實際工資總額\nGross Amount", new Font(bfChinese, 8)));
                            tr.Colspan = 10;
                            tr.Rowspan = 2;
                            tr.BorderWidthRight = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);
                            #endregion

                            #region 第二列
                            tr = new PdfPCell(new Phrase("Time In", new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.FixedHeight = 20f;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Time Out", new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Total Hrs", new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Time In", new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Time Out", new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Total Hrs", new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.BorderWidthBottom = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);
                            #endregion
                            #endregion

                            #region Table Data
                            #region Page Info
                            sidx = i * pageSize;
                            eidx = (i + 1) * pageSize;

                            if (eidx > ds.Count)
                            {
                                eidx = ds.Count;
                            }
                            #endregion

                            for (int j = sidx; j < eidx; j++)
                            {
                                #region 薪資資料
                                string salaryUnit = ds[j].SalaryUnit;
                                string salaryUnitName = "";
                                double salary = ds[j].Salary.Value;
                                double allowance = ds[j].Allowance.Value;
                                double defaultWorkHours = ds[j].DefaultWorkHours;
                                double workHours = ds[j].WorkHours.Value;
                                double defaultTotalSalary = 0, totalSalary = 0;
                                double? personalLabor = result == null ? null : result.EmpLaborData.Where(x => x.Portal_CasualFormID == ds[j].ID).Select(x => x.PersonalLabor).FirstOrDefault();


                                if (nowEmpID == ds[j].EmployeeNO && nowDate == ds[j].ExcuteDate + "")
                                {
                                    if (personalLabor != null)
                                    {
                                        personalLabor = 0;
                                    }
                                }

                                nowEmpID = ds[j].EmployeeNO;
                                nowDate = ds[j].ExcuteDate + "";

                                switch (salaryUnit)
                                {
                                    case "h":
                                        salaryUnitName = "時";
                                        defaultTotalSalary = Math.Round(salary * defaultWorkHours + allowance, MidpointRounding.AwayFromZero);
                                        totalSalary = Math.Round(salary * workHours + allowance, MidpointRounding.AwayFromZero);
                                        break;
                                    case "d":
                                        salaryUnitName = "日";
                                        defaultTotalSalary = Math.Round(salary + allowance, MidpointRounding.AwayFromZero);
                                        totalSalary = Math.Round(salary + allowance, MidpointRounding.AwayFromZero);
                                        break;
                                    default:
                                        salaryUnitName = "";
                                        defaultTotalSalary = Math.Round(allowance, MidpointRounding.AwayFromZero);;
                                        totalSalary = Math.Round(allowance, MidpointRounding.AwayFromZero);;
                                        break;
                                }

                                totalSalary -= personalLabor == null ? 0 : personalLabor.Value;

                                defaultWorkHourSum += defaultWorkHours;
                                workHourSum += workHours;
                                salarySum += totalSalary;
                                allowanceSum += allowance;
                                personalLaborSum += personalLabor == null ? 0 : personalLabor.Value;
                                #endregion

                                #region 明細
                                tr = new PdfPCell(new Phrase((j + 1).ToString(), new Font(bfChinese, 8)));
                                tr.Colspan = 3;
                                tr.FixedHeight = 20f;
                                tr.BorderWidthLeft = 2;
                                tr.HorizontalAlignment = Element.ALIGN_RIGHT;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].EmployeeNO, new Font(bfChinese, 8)));
                                tr.Colspan = 7;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].EmpName + (ds[j].Status == "only" ? "*" : ""), new Font(bfChinese, 8)));
                                tr.Colspan = 15;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].DefaultStartTime, new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].DefaultEndTime, new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].DefaultWorkHours.ToString(), new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].StartTime, new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].EndTime, new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].WorkHours.Value.ToString(), new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(salaryUnitName, new Font(bfChinese, 8)));
                                tr.Colspan = 3;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].Salary.Value.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 7;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(ds[j].Allowance.Value.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 9;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(personalLabor == null ? "" : personalLabor.Value.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 10;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(totalSalary.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 10;
                                tr.BorderWidthRight = 2;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);
                                #endregion
                            }
                            #endregion

                            #region Sum Data
                            tr = new PdfPCell();
                            tr.Colspan = 3;
                            tr.BorderWidthLeft = 2;
                            tr.FixedHeight = 20f;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase("Total Hours(Hrs) / Amount(NT$)", new Font(bfChinese, 8)));
                            tr.Colspan = 34;
                            tr.HorizontalAlignment = Element.ALIGN_LEFT;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);
                            
                            //20170816 Daniel 預計工作時數增加顯示到小數點一位
                            //tr = new PdfPCell(new Phrase(defaultWorkHourSum.ToString("#,0"), new Font(bfChinese, 8)));
                            tr = new PdfPCell(new Phrase(defaultWorkHourSum.ToString("#,0.#"), new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell();
                            tr.Colspan = 12;
                            table.AddCell(tr);

                            //20170816 Daniel 實際工作時數增加顯示到小數點一位
                            //tr = new PdfPCell(new Phrase(workHourSum.ToString("#,1"), new Font(bfChinese, 8)));
                            tr = new PdfPCell(new Phrase(workHourSum.ToString("#,0.#"), new Font(bfChinese, 8)));
                            tr.Colspan = 6;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell();
                            tr.Colspan = 10;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase(allowanceSum.ToString("#,0"), new Font(bfChinese, 8)));
                            tr.Colspan = 9;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase(personalLaborSum.ToString("#,0"), new Font(bfChinese, 8)));
                            tr.Colspan = 10;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell(new Phrase(salarySum.ToString("#,0"), new Font(bfChinese, 8)));
                            tr.Colspan = 10;
                            tr.BorderWidthRight = 2;
                            tr.HorizontalAlignment = Element.ALIGN_CENTER;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);
                            #endregion

                            #region Total Data
                            defaultWorkHourTotal += defaultWorkHourSum;
                            workHourTotal += workHourSum;
                            salaryTotal += salarySum;
                            allowanceTotal += allowanceSum;
                            personalLaborTotal += personalLaborSum;

                            if (i >= totalPage - 1)
                            {
                                tr = new PdfPCell();
                                tr.Colspan = 3;
                                tr.BorderWidthLeft = 2;
                                tr.FixedHeight = 20f;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase("當日總計 時數(Hrs) / 金額(NT$)", new Font(bfChinese, 8)));
                                tr.Colspan = 34;
                                tr.HorizontalAlignment = Element.ALIGN_LEFT;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                //20170816 Daniel 預計工作時數增加顯示到小數點一位
                                //tr = new PdfPCell(new Phrase(defaultWorkHourTotal.ToString("#,0"), new Font(bfChinese, 8)));
                                tr = new PdfPCell(new Phrase(defaultWorkHourTotal.ToString("#,0.#"), new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell();
                                tr.Colspan = 12;
                                table.AddCell(tr);

                                //20170816 Daniel 實際工作時數增加顯示到小數點一位
                                //tr = new PdfPCell(new Phrase(workHourTotal.ToString("#,0"), new Font(bfChinese, 8)));
                                tr = new PdfPCell(new Phrase(workHourTotal.ToString("#,0.#"), new Font(bfChinese, 8)));
                                tr.Colspan = 6;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell();
                                tr.Colspan = 10;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(allowanceTotal.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 9;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(personalLaborTotal.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 10;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);

                                tr = new PdfPCell(new Phrase(salaryTotal.ToString("#,0"), new Font(bfChinese, 8)));
                                tr.Colspan = 10;
                                tr.BorderWidthRight = 2;
                                tr.HorizontalAlignment = Element.ALIGN_CENTER;
                                tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                                table.AddCell(tr);
                            }
                            #endregion

                            #region 簽名
                            string strAbout = @"*實際工資總額 = 實際工作時數 x 時薪 + 車馬費 (實際給付淨額月結時得扣除個人勞保費自負額)";

                            tr = new PdfPCell(new Phrase(strAbout, new Font(bfChinese, 8)));
                            tr.Colspan = 100;
                            tr.BorderWidthLeft = 2;
                            tr.BorderWidthRight = 2;
                            tr.BorderWidthTop = 2;
                            tr.BorderWidthBottom = 0;
                            tr.Padding = 5f;
                            tr.HorizontalAlignment = Element.ALIGN_LEFT;
                            tr.VerticalAlignment = Element.ALIGN_MIDDLE;
                            table.AddCell(tr);

                            tr = new PdfPCell();
                            tr.Colspan = 37;
                            tr.FixedHeight = 25f;
                            tr.BorderWidthLeft = 2;
                            tr.BorderWidthRight = 0;
                            tr.BorderWidthTop = 0;
                            tr.BorderWidthBottom = 2;
                            table.AddCell(tr);

                            strAbout = @"值班主管確認：________________________________________________　　部門協理核準：________________________________________________
                  Verified/Confirmed by Dept. Head on Duty                               Approved By Division Head";

                            tr = new PdfPCell(new Phrase(strAbout, new Font(bfChinese, 7.5f)));
                            tr.Colspan = 63;
                            tr.BorderWidthLeft = 0;
                            tr.BorderWidthRight = 2;
                            tr.BorderWidthTop = 0;
                            tr.BorderWidthBottom = 2;
                            tr.PaddingBottom = 7f;
                            tr.HorizontalAlignment = Element.ALIGN_LEFT;
                            tr.VerticalAlignment = Element.ALIGN_BOTTOM;
                            table.AddCell(tr);
                            #endregion

                            doc.Add(table);
                            #endregion

                            if ((i + 1) < totalPage)
                            {
                                doc.NewPage();
                            }
                        }

                        doc.Close();

                        outputStream.Close();
                        pdfFile = outputStream.ToArray();
                    }

                    return File(pdfFile, "application/pdf", "臨時工處理.pdf");
                }
                #endregion
            }
            else
            {
                #region 呼叫 WebApi - UpdateCasualCycle 取得臨時工計薪批號(近7日)
                int result = 0;
                List<CasualCycleResult> listCasualCycle = await HRMApiAdapter.UpdateCasualCycle(CurrentUser.CompanyCode);

                foreach (var item in listCasualCycle)
                {
                    var CompanyData = Services.GetService<CompanyService>().GetCompanyLists().Where(x => x.Company_ID == item.CompanyID).FirstOrDefault();

                    if (CompanyData != null)
                    {
                        #region Insert or Update CasualCycle
                        try
                        {
                            var CasualCycleData = Services.GetService<CasualCycleService>().GetByID(CompanyData.ID, item.CasualCycle_ID);

                            if (CasualCycleData == null)
                            {
                                CasualCycle addData = new CasualCycle();
                                addData.CasualCycle_ID = item.CasualCycle_ID;
                                addData.CasualFormNo = item.CasualFormNo;
                                addData.CasualPayDate = item.CasualPayDate;
                                addData.CountBeginDate = item.CountBeginDate;
                                addData.CountEndDate = item.CountEndDate;
                                addData.LockFlag = item.LockFlag;
                                addData.CompanyID = CompanyData.ID;

                                result = Services.GetService<CasualCycleService>().Create(addData, true);
                            }
                            else
                            {
                                if (item.CasualFormNo != CasualCycleData.CasualFormNo ||
                                    item.CasualPayDate != CasualCycleData.CasualPayDate ||
                                    item.CountBeginDate != CasualCycleData.CountBeginDate ||
                                    item.CountEndDate != CasualCycleData.CountEndDate ||
                                    item.LockFlag != CasualCycleData.LockFlag)
                                {
                                    CasualCycle updData = new CasualCycle();
                                    updData.ID = CasualCycleData.ID;
                                    updData.CasualCycle_ID = CasualCycleData.CasualCycle_ID;
                                    updData.CasualFormNo = item.CasualFormNo;
                                    updData.CasualPayDate = item.CasualPayDate;
                                    updData.CountBeginDate = item.CountBeginDate;
                                    updData.CountEndDate = item.CountEndDate;
                                    updData.LockFlag = item.LockFlag;
                                    updData.CompanyID = CasualCycleData.ID;

                                    result = Services.GetService<CasualCycleService>().Create(updData, true);
                                }
                            }
                        }
                        catch
                        {

                        }
                        #endregion

                        #region Update CasualForm
                        List<CasualForm> listCasualForm = Services.GetService<CasualFormService>().GetLists(CompanyData.ID).Where(x => x.ExcuteDate >= item.CountBeginDate && x.ExcuteDate <= item.CountEndDate).ToList();

                        foreach (var updItem in listCasualForm)
                        {
                            try
                            {
                                if (updItem.CasualFormNo != item.CasualFormNo)
                                {
                                    updItem.CasualFormNo = item.CasualFormNo;
                                    result = Services.GetService<CasualFormService>().Update(updItem, true);
                                }
                            }
                            catch
                            {

                            }
                        }
                        #endregion
                    }
                }
                #endregion

                return View();
            }
        }

        // POST: FEPH/CasualForm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string cmd, string txtDate, string ddlTimeType, string ddlDepartment, string txtEmpID, string txtEmpName)
        {
            if (cmd == "Clear")
            {
                GetDefaultData();
                return View();
            }
            else if (cmd == "Search" || cmd == "PDF")
            {
                return RedirectToAction("Index", new
                {
                    cmd = cmd,
                    txtDate,
                    ddlTimeType,
                    ddlDepartment,
                    txtEmpID,
                    txtEmpName
                });
            }

            //重整
            GetDefaultData(txtDate, ddlTimeType, ddlDepartment, txtEmpID, txtEmpName);

            return View();
        }

        /// <summary>
        /// 預設資料
        /// </summary>
        /// <param name="txtDate"></param>
        /// <param name="ddlTimeType"></param>
        /// <param name="txtEmpID"></param>
        /// <param name="txtEmpName"></param>
        private void GetDefaultData(string txtDate = "", string ddlTimeType = "", string ddlDepartment = "", string txtEmpID = "", string txtEmpName = "")
        {
            ViewBag.DateTime = string.IsNullOrWhiteSpace(txtDate) ? DateTime.Now.ToString("yyyy/MM/dd") : txtDate;
            ViewBag.TimeTypeList = GetTimeTypeList(ddlTimeType);
            ViewBag.DepartmentList = GetDepartmentList(ddlDepartment);
            ViewBag.TimeType = ddlTimeType;
            ViewBag.Department = ddlDepartment;
            ViewBag.EmpID = txtEmpID;
            ViewBag.EmpName = txtEmpName;
        }

        // GET: FEPH/CasualForm/Create
        public async Task<ActionResult> Create()
        {
            #region CasualForm 資料
            CasualFormViewModel data = new CasualFormViewModel();
            data.casualFormData.CompanyID = CurrentUser.CompanyID;
            
            #region 取得部門資料
            data.casualFormData.Dept_Code = CurrentUser.DepartmentCode;

            Department deptData = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == CurrentUser.DepartmentCode).FirstOrDefault();
           
            if (deptData != null)
            {
                data.casualFormData.Dept_ID = deptData.Department_ID.Value;
                data.casualFormData.Dept_Name = deptData.DepartmentName;
            }
            #endregion
            #endregion

            #region 取得下拉資料
            data.DepartmentList = GetDepartmentList(CurrentUser.DepartmentCode);  //部門下拉
            data.DeptCostList = GetCostList(CurrentUser.DepartmentCode);  //費用別下拉
            data.SalaryUnitList = GetSalaryUnitList();  //計薪單位下拉
            data.ClassList = await GetClassList(CurrentUser.CompanyCode);  //班別下拉
            data.TimeList = GetTimeList();  //時間下拉
            #endregion

            //20170913 Daniel 取得臨時工最低時薪參數
            string minSalaryPerHour = await HRMApiAdapter.GetCasualMinSalaryPerHour(CurrentUser.CompanyCode);
            ViewBag.MinSalaryPerHour = minSalaryPerHour;

            return PartialView("_CreateCasualForm", data);
        }

        // POST: FEPH/CasualForm/Create
        [HttpPost]
        public async Task<ActionResult> Create(CasualFormViewModel data)
        {
            string firstStartTime = "", lastEndTime = "";
            double totalWorkHours = 0;

            data.casualFormData.ID = Guid.NewGuid();
            data.casualFormData.Status = data.PrevStatus ? "only" : "suit";  //投保身分
           
            #region 判斷是否有輸入實際上班時間，有的話就檢查當天是否有班表重覆
            if (!String.IsNullOrEmpty(data.casualFormData.StartTime) && !String.IsNullOrEmpty(data.casualFormData.EndTime))
            {
                firstStartTime = data.casualFormData.StartTime;
                lastEndTime = data.casualFormData.EndTime;
                totalWorkHours = data.casualFormData.WorkHours.Value;

                List<CasualForm> casualFormList = Services.GetService<CasualFormService>().GetListByEmpID(data.casualFormData.CompanyID, data.casualFormData.EmpData_ID).Where(x => x.ExcuteDate == data.casualFormData.ExcuteDate && x.StartTime.ToString() != "" && x.EndTime.ToString() != "").ToList();

                int startTime = int.Parse(data.casualFormData.StartTime.Replace(":", ""));
                int endTime = int.Parse(data.casualFormData.EndTime.Replace(":", ""));

                if (endTime < startTime) endTime += 2400;

                int tmpStartTime = startTime;
                int tmpEndTime = endTime;

                foreach (var item in casualFormList)
                {
                    int startTime1 = int.Parse(item.StartTime.Replace(":", ""));
                    int endTime1 = int.Parse(item.EndTime.Replace(":", ""));

                    if (endTime1 < startTime1) endTime1 += 2400;

                    if ((startTime >= startTime1 && startTime < endTime1) || (endTime > startTime1 && endTime <= endTime1) || (startTime < startTime1 && endTime > endTime1))
                    {
                        return Json(new AjaxResult() { status = "failed", message = "實際上班時間重覆" });
                    }

                    totalWorkHours += item.WorkHours.Value;

                    if (startTime1 < tmpStartTime)
                    {
                        tmpStartTime = startTime1;
                        firstStartTime = item.StartTime;
                    }

                    if (endTime1 > tmpEndTime   )
                    {
                        tmpEndTime = endTime1;
                        lastEndTime = item.EndTime;
                    }
                }
            }
            #endregion

            #region Get CasualForm Data
            #region 取得 Dept 資料
            Department deptData = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == data.casualFormData.Dept_Code).FirstOrDefault();

            if (deptData != null)
            {
                data.casualFormData.Dept_ID = deptData.Department_ID.Value;
                data.casualFormData.Dept_Name = deptData.DepartmentName;
            }
            #endregion

            #region 取得 Cost 資料
            DeptCost deptCostData = Services.GetService<DeptCostService>().GetListsByDeptCode(data.casualFormData.Dept_Code).Where(x => x.CostCode == data.casualFormData.Cost_Code).FirstOrDefault();

            if (deptCostData != null)
            {
                data.casualFormData.Cost_ID = deptCostData.Cost_id;
                data.casualFormData.Cost_Name = deptCostData.CostName;
            }
            #endregion

            #region 呼叫 WebApi - GetClassByID 取得 Class_Code,Class_Name
            ClassData classData = await HRMApiAdapter.GetClassByID(CurrentUser.CompanyCode, data.casualFormData.Class_ID);
            if (classData == null)
            {
                return Json(new AjaxResult() { status = "failed", message = "查無班別資料" });
            }
            else
            {
                data.casualFormData.Class_Code = classData.Code;
                data.casualFormData.Class_Name = classData.Name;
            }
            #endregion

            data.casualFormData.CreateDate = DateTime.Now;
            data.casualFormData.CreateEmployeeNO = CurrentUser.EmployeeNO;
            #endregion

            #region 呼叫 WebApi - CreateEmpChangeK9 員工復職異動(26日復職次月25日離職)
            EmpChangeK9Result result1 = await HRMApiAdapter.CreateEmpChangeK9(CurrentUser.CompanyCode, data.casualFormData.EmployeeNO, data.casualFormData.ExcuteDate);

            if (!result1.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = string.Format("員工復職異動錯誤：{0}", result1.Message) });
            }
            #endregion

            #region 呼叫 WebApi - CreateEmpScheduleK9 班表標準選擇(依日期期間)，員工排班
            EmpScheduleK9Result result2 = await HRMApiAdapter.CreateEmpScheduleK9(CurrentUser.CompanyCode, data.casualFormData.EmployeeNO, data.casualFormData.ExcuteDate, data.casualFormData.Class_ID);

            if (!result2.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = string.Format("員工班表排班錯誤：{0}", result2.Message) });
            }
            #endregion

            #region 判斷是否有輸入出勤卡，有的話就要做出勤卡異動
            if (!String.IsNullOrEmpty(data.casualFormData.CardNo))
            {
                #region 呼叫 WebApi - CreateEmpDutyCardK9 出勤卡異動
                EmpDutyCardK9Result result3 = await HRMApiAdapter.CreateEmpDutyCardK9(CurrentUser.CompanyCode, data.casualFormData.EmployeeNO, data.casualFormData.ExcuteDate, data.casualFormData.CardNo);

                if (!result3.Status)
                {
                    return Json(new AjaxResult() { status = "failed", message = string.Format("員工出勤卡異動錯誤：{0}", result3.Message) });
                }
                #endregion
            }
            #endregion

            #region 計算 DaySalary 當日薪資總額
            data.casualFormData.DaySalary = null;

            if (!String.IsNullOrEmpty(data.casualFormData.StartTime) && !String.IsNullOrEmpty(data.casualFormData.EndTime))
            {
                double realWorkHours = data.casualFormData.WorkHours.HasValue ? data.casualFormData.WorkHours.Value : 0;
                double realSalary = data.casualFormData.Salary.HasValue ? data.casualFormData.Salary.Value : 0;
                double realAllowance = data.casualFormData.Allowance.HasValue ? data.casualFormData.Allowance.Value : 0;

                switch (data.casualFormData.SalaryUnit)
                {
                    case "h":  //時薪
                        data.casualFormData.DaySalary = Math.Round(realSalary * realWorkHours + realAllowance, 0, MidpointRounding.AwayFromZero);
                        break;
                    case "d":  //日薪
                        data.casualFormData.DaySalary = Math.Round(realSalary + realAllowance, 0, MidpointRounding.AwayFromZero);
                        break;
                    default:
                        data.casualFormData.DaySalary = Math.Round(realAllowance, 0, MidpointRounding.AwayFromZero);
                        break;
                }
            }
            #endregion

            #region 呼叫 WebApi - CreateCasualForm
            CasualFormData createData = new CasualFormData();
            createData.CompanyCode = CurrentUser.CompanyCode;
            createData.EmpData_ID = data.casualFormData.EmpData_ID;
            createData.Dept_ID = data.casualFormData.Dept_ID;
            createData.Cost_ID = data.casualFormData.Cost_ID;
            createData.ExcuteDate = data.casualFormData.ExcuteDate;
            createData.Class_ID = data.casualFormData.Class_ID;
            createData.DefaultStartTime = data.casualFormData.DefaultStartTime;
            createData.DefaultEndTime = data.casualFormData.DefaultEndTime;
            createData.DefaultWorkHours = data.casualFormData.DefaultWorkHours;
            createData.StartTime = data.casualFormData.StartTime;
            createData.EndTime = data.casualFormData.EndTime;
            createData.DiningHours = data.casualFormData.DiningHours;
            createData.WorkHours = data.casualFormData.WorkHours;
            createData.SalaryUnit = data.casualFormData.SalaryUnit;
            createData.Salary = data.casualFormData.Salary;
            createData.Allowance = data.casualFormData.Allowance;
            createData.CardNo = data.casualFormData.CardNo;
            createData.CardKeep = false;
            createData.CashFlag = data.casualFormData.CashFlag;
            createData.DaySalary = data.casualFormData.DaySalary;
            createData.Portal_CasualFormID = data.casualFormData.ID;
            createData.UserID = CurrentUser.EmployeeNO;

            CasualFormResult result5 = await HRMApiAdapter.CreateCasualForm(createData);

            if (!result5.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = string.Format("臨時工工時新增錯誤：{0}", result5.Message) });
            }
            #endregion

            #region 判斷是否有輸入實際上班時間，有的話就要做勞保加保異動
            if (!String.IsNullOrEmpty(data.casualFormData.StartTime) && !String.IsNullOrEmpty(data.casualFormData.EndTime))
            {
                double SumSalary = data.casualFormData.DaySalary.Value * 20;  //當日投保金額

                #region 呼叫 WebApi - CreateEmpLaborK9 勞保二合一「加保異動」
                bool isExcute = false;

                #region 判斷是否執行加保動作(當天 000000 ~ 170000 及 當天 以後可以)
                if (data.casualFormData.ExcuteDate.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
                {
                    if (int.Parse(DateTime.Now.ToString("HHmmss")) >= int.Parse("000000") && int.Parse(DateTime.Now.ToString("HHmmss")) <= int.Parse("170000"))
                    {
                        isExcute = true;
                    }
                }
                else if (int.Parse(data.casualFormData.ExcuteDate.ToString("yyyyMMdd")) > int.Parse(DateTime.Now.ToString("yyyyMMdd")))
                {
                    isExcute = true;
                }
                #endregion

                if (isExcute)
                {
                    #region 計算投保金額 (扣除目前該筆的其它資料)
                    List<CasualForm> casualFormList = Services.GetService<CasualFormService>().GetListByEmpID(data.casualFormData.CompanyID, data.casualFormData.EmpData_ID).Where(x => x.ExcuteDate == data.casualFormData.ExcuteDate && x.StartTime.ToString() != "" && x.EndTime.ToString() != "").ToList();

                    foreach (var item in casualFormList)
                    {
                        double WorkHours = item.WorkHours.HasValue ? item.WorkHours.Value : 0;
                        double Salary = item.Salary.HasValue ? item.Salary.Value : 0;
                        double Allowance = item.Allowance.HasValue ? item.Allowance.Value : 0;

                        switch (item.SalaryUnit)
                        {
                            case "h":  //時薪
                                SumSalary += (WorkHours * Salary + Allowance) * 20;
                                break;
                            case "d":  //日薪
                                SumSalary += (Salary + Allowance) * 20;
                                break;
                            default:
                                SumSalary += Allowance * 20;
                                break;
                        }
                    }
                    #endregion

                    EmpLaborK9Result result4 = await HRMApiAdapter.CreateEmpLaborK9(CurrentUser.CompanyCode, data.casualFormData.EmployeeNO, data.casualFormData.ExcuteDate, SumSalary, data.casualFormData.ID, data.casualFormData.Status);

                    if (!result4.Status)
                    {
                        #region 呼叫 WebApi - DeleteCasualForm
                        result5 = await HRMApiAdapter.DeleteCasualForm(createData);

                        //if (!result5.Status)
                        //{
                        //    return Json(new AjaxResult() { status = "failed", message = string.Format("臨時工工時刪除錯誤：{0}", result5.Message) });
                        //}
                        #endregion

                        return Json(new AjaxResult() { status = "failed", message = string.Format("員工勞保異動錯誤：{0}", result4.Message) });
                    }
                }
                #endregion

                #region 呼叫 WebApi - CreateEmpScheduleTimeK9 班表時間調整(依日期)，員工排班
                EmpScheduleTimeK9Data data6 = new EmpScheduleTimeK9Data();
                data6.CompanyCode = CurrentUser.CompanyCode;
                data6.EmpID = data.casualFormData.EmployeeNO;
                data6.ExcuteDate = data.casualFormData.ExcuteDate;
                data6.StartTime = firstStartTime;
                data6.EndTime = lastEndTime;
                data6.WorkHours = totalWorkHours;

                EmpScheduleTimeK9Result result6 = await HRMApiAdapter.CreateEmpScheduleTimeK9(data6);

                if (!result6.Status)
                {
                    #region 呼叫 WebApi - DeleteCasualForm
                    result5 = await HRMApiAdapter.DeleteCasualForm(createData);

                    //if (!result5.Status)
                    //{
                    //    return Json(new AjaxResult() { status = "failed", message = string.Format("臨時工工時刪除錯誤：{0}", result5.Message) });
                    //}
                    #endregion

                    return Json(new AjaxResult() { status = "failed", message = string.Format("員工班表時間調整錯誤：{0}", result6.Message) });
                }
                #endregion
            }
            #endregion

            #region Create CasualForm
            Guid? cid = null;
            var result = Services.GetService<CasualFormService>().Create(ref cid, data.casualFormData, true);

            if (result != 1)
            {
                return Json(new AjaxResult() { status = "failed", message = "新增失敗" });
            }
            #endregion

            #region Create CasualFormLog
            CasualFormLog casualFormLog = new CasualFormLog()
            {
                ExcuteEmpNO = this.CurrentUser.EmployeeNO,
                ExcuteTime = DateTime.Now,
                ChangeType = "Create",
                CasualForm_ID = cid.Value,
                EmployeeNO = data.casualFormData.EmployeeNO,
                EmpData_ID = data.casualFormData.EmpData_ID,
                EmpName = data.casualFormData.EmpName,
                Dept_ID = data.casualFormData.Dept_ID,
                Dept_Code = data.casualFormData.Dept_Code,
                Dept_Name = data.casualFormData.Dept_Name,
                Cost_ID = data.casualFormData.Cost_ID,
                Cost_Code = data.casualFormData.Cost_Code,
                Cost_Name = data.casualFormData.Cost_Name,
                ExcuteDate = data.casualFormData.ExcuteDate,
                Class_ID = data.casualFormData.Class_ID,
                Class_Code = data.casualFormData.Class_Code,
                Class_Name = data.casualFormData.Class_Name,
                DefaultStartTime = data.casualFormData.DefaultStartTime,
                DefaultEndTime = data.casualFormData.DefaultEndTime,
                DefaultWorkHours = data.casualFormData.DefaultWorkHours,
                StartTime = data.casualFormData.StartTime,
                EndTime = data.casualFormData.EndTime,
                DiningHours = data.casualFormData.DiningHours,
                WorkHours = data.casualFormData.WorkHours,
                SalaryUnit = data.casualFormData.SalaryUnit,
                Salary = data.casualFormData.Salary,
                Allowance = data.casualFormData.Allowance,
                CardNo = data.casualFormData.CardNo,
                CardKeep = data.casualFormData.CardKeep,
                CashFlag = data.casualFormData.CashFlag,
                DaySalary = data.casualFormData.DaySalary,
                Status = data.casualFormData.Status,
                CasualFormNo = data.casualFormData.CasualFormNo,
                UpdateDate = DateTime.Now,
                UpdateEmployeeNO = this.CurrentUser.EmployeeNO,
                CreateDate = DateTime.Now,
                CreateEmployeeNO = this.CurrentUser.EmployeeNO,
                CompanyID = data.casualFormData.CompanyID
            };

            var logResult = Services.GetService<CasualFormLogService>().Create(casualFormLog, true);

            //if (result != 1)
            //{
            //    return Json(new AjaxResult() { status = "failed", message = "新增失敗" });
            //}
            #endregion

            WriteLog(string.Format("Success:{0},{1}", data.casualFormData.EmployeeNO, data.casualFormData.ExcuteDate));
            return Json(new AjaxResult() { status = "success", message = "新增成功" });
        }

        // GET: FEPH/CasualForm/Edit
        public async Task<ActionResult> Edit(Guid id)
        {
            DateTime dtStartTime, dtEndTime;
            TimeSpan tsStartTime, tsEndTime;
            double dbWorkHours = 0;

            #region 取得 CasualForm 資料
            CasualForm casualFormData = Services.GetService<CasualFormService>().GetByID(id);

            CasualFormViewModel data = new CasualFormViewModel();
            data.PrevStatus = casualFormData.Status == "only" ? true : false;
            data.casualFormData = casualFormData;
            data.casualFormData.DiningHours = casualFormData.DiningHours == null ? 0 : casualFormData.DiningHours;
            
            if (String.IsNullOrEmpty(casualFormData.StartTime) || String.IsNullOrEmpty(casualFormData.EndTime))
            {
                //第一次修改，預帶預計上下班到實際上下班，用餐時間預設為30分鐘
                data.casualFormData.StartTime = casualFormData.DefaultStartTime;
                data.casualFormData.EndTime = casualFormData.DefaultEndTime;

                dtStartTime = new DateTime(1911, 1, 1, int.Parse(casualFormData.DefaultStartTime.Split(':')[0]), int.Parse(casualFormData.DefaultStartTime.Split(':')[1]), 0);
                dtEndTime = new DateTime(1911, 1, 1, int.Parse(casualFormData.DefaultEndTime.Split(':')[0]), int.Parse(casualFormData.DefaultEndTime.Split(':')[1]), 0);

                tsStartTime = new TimeSpan(dtStartTime.Ticks);
                tsEndTime = new TimeSpan(dtEndTime.Ticks);

                dbWorkHours = tsStartTime.Subtract(tsEndTime).Duration().TotalHours;

                data.casualFormData.DiningHours = 0.5;  //預設30分鐘
                data.casualFormData.WorkHours = dbWorkHours - data.casualFormData.DiningHours.Value;
            }
            //else
            //{
            //    dtStartTime = new DateTime(1911, 1, 1, int.Parse(casualFormData.StartTime.Split(':')[0]), int.Parse(casualFormData.StartTime.Split(':')[1]), 0);
            //    dtEndTime = new DateTime(1911, 1, 1, int.Parse(casualFormData.EndTime.Split(':')[0]), int.Parse(casualFormData.EndTime.Split(':')[1]), 0);

            //    tsStartTime = new TimeSpan(dtStartTime.Ticks);
            //    tsEndTime = new TimeSpan(dtEndTime.Ticks);

            //    dbWorkHours = tsStartTime.Subtract(tsEndTime).Duration().TotalHours;
            //    data.casualFormData.DiningHours = dbWorkHours - casualFormData.WorkHours.Value;
            //}

            switch (data.casualFormData.SalaryUnit)
            {
                case "d":
                    data.Total = data.casualFormData.Salary.Value + data.casualFormData.Allowance.Value;
                    break;
                case "h":
                    data.Total = (data.casualFormData.Salary.Value * data.casualFormData.WorkHours.Value) + data.casualFormData.Allowance.Value;
                    break;
                default:
                    data.Total = data.casualFormData.Allowance.Value;
                    break;
            }
            #endregion

            #region 取得 CasualForm 原計薪單位、薪資、累計工時
            IQueryable<CasualForm> casualFormListData = Services.GetService<CasualFormService>().GetListByEmpID(casualFormData.CompanyID, casualFormData.EmpData_ID);
            //扣掉該ExcuteDate之後的資料
            casualFormListData = casualFormListData.Where(x => x.ExcuteDate <= casualFormData.ExcuteDate);
            //最後一筆 CasualForm 資料
            CasualForm lastCasualFormData = null;

            if (casualFormListData.ToList().Count > 0)
            {
                lastCasualFormData = casualFormListData.OrderBy(x => x.ExcuteDate).ToList().LastOrDefault();
                data.PrevSalaryUnit = lastCasualFormData.SalaryUnit;
                data.PrevSalary = lastCasualFormData.Salary.Value;
                data.SumWorkHours = casualFormListData.Sum(x => x.WorkHours).Value;
            }
            #endregion

            #region 取得下拉資料
            data.DepartmentList = GetDepartmentList(casualFormData.Dept_Code);  //部門下拉
            data.DeptCostList = GetCostList(casualFormData.Dept_Code, casualFormData.Cost_Code);  //費用別下拉
            data.SalaryUnitList = GetSalaryUnitList(casualFormData.SalaryUnit);  //計薪單位下拉
            data.ClassList = await GetClassList(CurrentUser.CompanyCode, casualFormData.Class_ID.ToString());  //班別下拉
            data.TimeList = GetTimeList();  //時間下拉
            #endregion

            //20170913 Daniel 取得臨時工最低時薪參數
            string minSalaryPerHour = await HRMApiAdapter.GetCasualMinSalaryPerHour(CurrentUser.CompanyCode);
            ViewBag.MinSalaryPerHour = minSalaryPerHour;

            return PartialView("_EditCasualForm", data);
        }

        // POST: FEPH/CasualForm/Update
        [HttpPost]
        public async Task<ActionResult> Update(CasualFormViewModel data)
        {
            bool EmpDoorFlag = false;  //出勤卡是否異動
            bool EmpScheduleFlag = false; //班表是否有資料

            string firstStartTime = "", lastEndTime = "";
            double totalWorkHours = 0;

            data.casualFormData.Status = data.PrevStatus ? "only" : "suit";  //投保身分

            #region 判斷是否有輸入實際上班時間，有的話就檢查當天是否有班表重覆
            if (!String.IsNullOrEmpty(data.casualFormData.StartTime) && !String.IsNullOrEmpty(data.casualFormData.EndTime))
            {
                firstStartTime = data.casualFormData.StartTime;
                lastEndTime = data.casualFormData.EndTime;
                totalWorkHours = data.casualFormData.WorkHours.Value;

                List<CasualForm> casualFormList = Services.GetService<CasualFormService>().GetListByEmpID(data.casualFormData.CompanyID, data.casualFormData.EmpData_ID).Where(x => x.ExcuteDate == data.casualFormData.ExcuteDate && x.ID != data.casualFormData.ID && x.StartTime.ToString() != "" && x.EndTime.ToString() != "").ToList();

                int startTime = int.Parse(data.casualFormData.StartTime.Replace(":", ""));
                int endTime = int.Parse(data.casualFormData.EndTime.Replace(":", ""));

                if (endTime < startTime) endTime += 2400;

                int tmpStartTime = startTime;
                int tmpEndTime = endTime;

                foreach (var item in casualFormList)
                {
                    int startTime1 = int.Parse(item.StartTime.Replace(":", ""));
                    int endTime1 = int.Parse(item.EndTime.Replace(":", ""));

                    if (endTime1 < startTime1) endTime1 += 2400;

                    if ((startTime >= startTime1 && startTime < endTime1) || (endTime > startTime1 && endTime <= endTime1) || (startTime < startTime1 && endTime > endTime1))
                    {
                        return Json(new AjaxResult() { status = "failed", message = "實際上班時間重覆" });
                    }

                    totalWorkHours += item.WorkHours.Value;

                    if (startTime1 < tmpStartTime)
                    {
                        tmpStartTime = startTime1;
                        firstStartTime = item.StartTime;
                    }

                    if (endTime1 > tmpEndTime)
                    {
                        tmpEndTime = endTime1;
                        lastEndTime = item.EndTime;
                    }
                }
            }
            #endregion

            #region Get CasualForm Data
            bool isUpdate = false;

            //取得舊 CasualForm 資料
            CasualForm casualFormData = Services.GetService<CasualFormService>().GetByID(data.casualFormData.ID);

            if (casualFormData == null)
            {
                return Json(new AjaxResult() { status = "failed", message = "查無臨時工處理資料" });
            }
            else
            {
                #region 實際時間是否異動
                if ((casualFormData.StartTime != data.casualFormData.StartTime && casualFormData.EndTime != data.casualFormData.EndTime) || casualFormData.Status != data.casualFormData.Status)
                {
                    isUpdate = true;
                }
                #endregion

                #region 出勤卡是否異動
                EmpDoorFlag = !(casualFormData.CardNo == data.casualFormData.CardNo && casualFormData.CardKeep == data.casualFormData.CardKeep);
                #endregion

                #region 班表是否有資料
                if (!String.IsNullOrEmpty(casualFormData.StartTime) && !String.IsNullOrEmpty(casualFormData.EndTime))
                {
                    EmpScheduleFlag = true;
                }
                #endregion

                #region 取得 Dept 資料
                Department deptData = Services.GetService<DepartmentService>().GetListsByCompany(this.CurrentUser.CompanyID).Where(x => x.Enabled && x.DepartmentCode == data.casualFormData.Dept_Code).FirstOrDefault();

                if (deptData != null)
                {
                    data.casualFormData.Dept_ID = deptData.Department_ID.Value;
                    data.casualFormData.Dept_Name = deptData.DepartmentName;
                }
                #endregion

                #region 取得 Cost 資料
                DeptCost deptCostData = Services.GetService<DeptCostService>().GetListsByDeptCode(data.casualFormData.Dept_Code).Where(x => x.CostCode == data.casualFormData.Cost_Code).FirstOrDefault();

                if (deptCostData != null)
                {
                    data.casualFormData.Cost_ID = deptCostData.Cost_id;
                    data.casualFormData.Cost_Name = deptCostData.CostName;
                }
                #endregion

                #region 呼叫 WebApi - GetClassByID 取得 Class_Code,Class_Name
                ClassData classData = await HRMApiAdapter.GetClassByID(CurrentUser.CompanyCode, data.casualFormData.Class_ID);
                if (classData == null)
                {
                    return Json(new AjaxResult() { status = "failed", message = "查無班別資料" });
                }
                else
                {
                    data.casualFormData.Class_Code = classData.Code;
                    data.casualFormData.Class_Name = classData.Name;
                }
                #endregion

                #region 異動資料
                casualFormData.Dept_ID = data.casualFormData.Dept_ID;
                casualFormData.Dept_Code = data.casualFormData.Dept_Code;
                casualFormData.Dept_Name = data.casualFormData.Dept_Name;
                casualFormData.Cost_ID = data.casualFormData.Cost_ID;
                casualFormData.Cost_Code = data.casualFormData.Cost_Code;
                casualFormData.Cost_Name = data.casualFormData.Cost_Name;
                casualFormData.Class_ID = data.casualFormData.Class_ID;
                casualFormData.Class_Code = data.casualFormData.Class_Code;
                casualFormData.Class_Name = data.casualFormData.Class_Name;
                casualFormData.DefaultStartTime = data.casualFormData.DefaultStartTime;
                casualFormData.DefaultEndTime = data.casualFormData.DefaultEndTime;
                casualFormData.DefaultWorkHours = data.casualFormData.DefaultWorkHours;
                casualFormData.StartTime = data.casualFormData.StartTime;
                casualFormData.EndTime = data.casualFormData.EndTime;
                casualFormData.WorkHours = data.casualFormData.WorkHours;
                casualFormData.SalaryUnit = data.casualFormData.SalaryUnit;
                casualFormData.Salary = data.casualFormData.Salary;
                casualFormData.Allowance = data.casualFormData.Allowance;
                casualFormData.CardNo = data.casualFormData.CardNo;
                casualFormData.CardKeep = false;
                casualFormData.CashFlag = data.casualFormData.CashFlag;
                casualFormData.UpdateDate = DateTime.Now;
                casualFormData.UpdateEmployeeNO = CurrentUser.EmployeeNO;
                casualFormData.Status = data.casualFormData.Status;
                casualFormData.DiningHours = data.casualFormData.DiningHours;
                #endregion
            }
            #endregion

            #region 判斷是否有輸入實際上班時間，有的話就要做勞保加保異動
            data.casualFormData.DaySalary = null;
            casualFormData.DaySalary = data.casualFormData.DaySalary;

            if ((!String.IsNullOrEmpty(data.casualFormData.StartTime) && !String.IsNullOrEmpty(data.casualFormData.EndTime)) || isUpdate)
            {
                double SumSalary = 0;  //投保金額

                #region 計算 DaySalary 當日薪資總額
                double realWorkHours = data.casualFormData.WorkHours.HasValue ? data.casualFormData.WorkHours.Value : 0;
                double realSalary = data.casualFormData.Salary.HasValue ? data.casualFormData.Salary.Value : 0;
                double realAllowance = data.casualFormData.Allowance.HasValue ? data.casualFormData.Allowance.Value : 0;

                switch (data.casualFormData.SalaryUnit)
                {
                    case "h":  //時薪
                        data.casualFormData.DaySalary = Math.Round(realSalary * realWorkHours + realAllowance, 0, MidpointRounding.AwayFromZero);
                        break;
                    case "d":  //日薪
                        data.casualFormData.DaySalary = Math.Round(realSalary + realAllowance, 0, MidpointRounding.AwayFromZero);
                        break;
                    default:
                        data.casualFormData.DaySalary = Math.Round(realAllowance, 0, MidpointRounding.AwayFromZero);
                        break;
                }

                if ((!String.IsNullOrEmpty(data.casualFormData.StartTime) && !String.IsNullOrEmpty(data.casualFormData.EndTime)))
                {
                    casualFormData.DaySalary = data.casualFormData.DaySalary;
                }
                
                SumSalary = data.casualFormData.DaySalary.Value * 20;  //當日投保金額
                #endregion

                #region 呼叫 WebApi - CreateEmpLaborK9 勞保二合一「加保異動」
                bool isExcute = false;

                #region 判斷是否執行加保動作(當天 000000 ~ 170000 及 當天 以後可以)
                if (data.casualFormData.ExcuteDate.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
                {
                    if (int.Parse(DateTime.Now.ToString("HHmmss")) >= int.Parse("000000") && int.Parse(DateTime.Now.ToString("HHmmss")) <= int.Parse("170000"))
                    {
                        isExcute = true;
                    }
                }
                else if (int.Parse(data.casualFormData.ExcuteDate.ToString("yyyyMMdd")) > int.Parse(DateTime.Now.ToString("yyyyMMdd")))
                {
                    isExcute = true;
                }
                #endregion

                if (isExcute)
                {
                    #region 計算投保金額 (扣除目前該筆的其它資料)
                    List<CasualForm> casualFormList = Services.GetService<CasualFormService>().GetListByEmpID(casualFormData.CompanyID, casualFormData.EmpData_ID).Where(x => x.ExcuteDate == casualFormData.ExcuteDate && x.ID != casualFormData.ID && x.StartTime.ToString() != "" && x.EndTime.ToString() != "").ToList();

                    foreach (var item in casualFormList)
                    {
                        double WorkHours = item.WorkHours.HasValue ? item.WorkHours.Value : 0;
                        double Salary = item.Salary.HasValue ? item.Salary.Value : 0;
                        double Allowance = item.Allowance.HasValue ? item.Allowance.Value : 0;

                        switch (item.SalaryUnit)
                        {
                            case "h":  //時薪
                                SumSalary += (WorkHours * Salary + Allowance) * 20;
                                break;
                            case "d":  //日薪
                                SumSalary += (Salary + Allowance) * 20;
                                break;
                            default:
                                SumSalary += Allowance * 20;
                                break;
                        }
                    }
                    #endregion

                    EmpLaborK9Result result1 = await HRMApiAdapter.CreateEmpLaborK9(CurrentUser.CompanyCode, casualFormData.EmployeeNO, casualFormData.ExcuteDate, SumSalary, casualFormData.ID, casualFormData.Status);

                    if (!result1.Status)
                    {
                        return Json(new AjaxResult() { status = "failed", message = string.Format("員工勞保異動錯誤：{0}", result1.Message) });
                    }
                }
                #endregion

                #region 呼叫 WebApi - CreateEmpScheduleTimeK9 班表時間調整(依日期)，員工排班
                EmpScheduleTimeK9Data data4 = new EmpScheduleTimeK9Data();
                data4.CompanyCode = casualFormData.Company.CompanyCode;
                data4.EmpID = casualFormData.EmployeeNO;
                data4.ExcuteDate = casualFormData.ExcuteDate;
                data4.StartTime = firstStartTime;
                data4.EndTime = lastEndTime;
                data4.WorkHours = totalWorkHours;

                EmpScheduleTimeK9Result result4 = await HRMApiAdapter.CreateEmpScheduleTimeK9(data4);

                if (!result4.Status)
                {
                    return Json(new AjaxResult() { status = "failed", message = string.Format("員工班表時間調整錯誤：{0}", result4.Message) });
                }
                #endregion
            }
            else
            {
                if (EmpScheduleFlag)
                {
                    #region 呼叫 WebApi - DeleteEmpScheduleTimeK9 班表時間調整(依日期)，員工排班
                    EmpScheduleTimeK9DelData data4 = new EmpScheduleTimeK9DelData();
                    data4.CompanyCode = casualFormData.Company.CompanyCode;
                    data4.EmpID = casualFormData.EmployeeNO;
                    data4.ExcuteDate = casualFormData.ExcuteDate;

                    EmpScheduleTimeK9DelResult result4 = await HRMApiAdapter.DeleteEmpScheduleTimeK9(data4);

                    if (!result4.Status)
                    {
                        return Json(new AjaxResult() { status = "failed", message = string.Format("員工班表時間調整錯誤：{0}", result4.Message) });
                    }
                    #endregion
                }
            }
            #endregion

            #region 判斷是否有輸入出勤卡，有的話就要做出勤卡異動
            if (!String.IsNullOrEmpty(data.casualFormData.CardNo))
            {
                #region 呼叫 WebApi - CreateEmpDutyCardK9 出勤卡異動
                if (EmpDoorFlag)
                {
                    EmpDutyCardK9Result result2 = await HRMApiAdapter.CreateEmpDutyCardK9(CurrentUser.CompanyCode, casualFormData.EmployeeNO, casualFormData.ExcuteDate, casualFormData.CardNo);

                    if (!result2.Status)
                    {
                        return Json(new AjaxResult() { status = "failed", message = string.Format("員工出勤卡異動錯誤：{0}", result2.Message) });
                    }
                }
                #endregion
            }
            #endregion

            #region 呼叫 WebApi - UpdateCasualForm
            CasualFormData updateData = new CasualFormData();
            updateData.CompanyCode = casualFormData.Company.CompanyCode;
            updateData.EmpData_ID = casualFormData.EmpData_ID;
            updateData.Dept_ID = casualFormData.Dept_ID;
            updateData.Cost_ID = casualFormData.Cost_ID;
            updateData.ExcuteDate = casualFormData.ExcuteDate;
            updateData.Class_ID = casualFormData.Class_ID;
            updateData.DefaultStartTime = casualFormData.DefaultStartTime;
            updateData.DefaultEndTime = casualFormData.DefaultEndTime;
            updateData.DefaultWorkHours = casualFormData.DefaultWorkHours;
            updateData.StartTime = casualFormData.StartTime;
            updateData.EndTime = casualFormData.EndTime;
            updateData.DiningHours = casualFormData.DiningHours;
            updateData.WorkHours = casualFormData.WorkHours;
            updateData.SalaryUnit = casualFormData.SalaryUnit;
            updateData.Salary = casualFormData.Salary;
            updateData.Allowance = casualFormData.Allowance;
            updateData.CardNo = casualFormData.CardNo;
            updateData.CardKeep = false;
            updateData.CashFlag = casualFormData.CashFlag;
            updateData.DaySalary = casualFormData.DaySalary;
            updateData.Portal_CasualFormID = casualFormData.ID;
            updateData.UserID = CurrentUser.EmployeeNO;

            CasualFormResult result3 = await HRMApiAdapter.UpdateCasualForm(updateData);

            if (!result3.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = string.Format("臨時工工時異動錯誤：{0}", result3.Message) });
            }
            #endregion

            #region Updaet CasualForm
            var result = Services.GetService<CasualFormService>().Update(casualFormData, true);

            if (result != 1)
            {
                return Json(new AjaxResult() { status = "failed", message = "異動失敗" });
            }
            #endregion

            #region Create CasualFormLog
            CasualFormLog casualFormLog = new CasualFormLog()
            {
                ExcuteEmpNO = this.CurrentUser.EmployeeNO,
                ExcuteTime = DateTime.Now,
                ChangeType = "Update",
                CasualForm_ID = casualFormData.ID,
                EmployeeNO = casualFormData.EmployeeNO,
                EmpData_ID = casualFormData.EmpData_ID,
                EmpName = casualFormData.EmpName,
                Dept_ID = casualFormData.Dept_ID,
                Dept_Code = casualFormData.Dept_Code,
                Dept_Name = casualFormData.Dept_Name,
                Cost_ID = casualFormData.Cost_ID,
                Cost_Code = casualFormData.Cost_Code,
                Cost_Name = casualFormData.Cost_Name,
                ExcuteDate = casualFormData.ExcuteDate,
                Class_ID = casualFormData.Class_ID,
                Class_Code = casualFormData.Class_Code,
                Class_Name = casualFormData.Class_Name,
                DefaultStartTime = casualFormData.DefaultStartTime,
                DefaultEndTime = casualFormData.DefaultEndTime,
                DefaultWorkHours = casualFormData.DefaultWorkHours,
                StartTime = casualFormData.StartTime,
                EndTime = casualFormData.EndTime,
                DiningHours = casualFormData.DiningHours,
                WorkHours = casualFormData.WorkHours,
                SalaryUnit = casualFormData.SalaryUnit,
                Salary = casualFormData.Salary,
                Allowance = casualFormData.Allowance,
                CardNo = casualFormData.CardNo,
                CardKeep = casualFormData.CardKeep,
                CashFlag = casualFormData.CashFlag,
                DaySalary = casualFormData.DaySalary,
                Status = casualFormData.Status,
                CasualFormNo = casualFormData.CasualFormNo,
                UpdateDate = DateTime.Now,
                UpdateEmployeeNO = this.CurrentUser.EmployeeNO,
                CreateDate = DateTime.Now,
                CreateEmployeeNO = this.CurrentUser.EmployeeNO,
                CompanyID = casualFormData.CompanyID
            };

            var logResult = Services.GetService<CasualFormLogService>().Create(casualFormLog, true);

            //if (result != 1)
            //{
            //    return Json(new AjaxResult() { status = "failed", message = "新增失敗" });
            //}
            #endregion

            WriteLog(string.Format("Success:{0},{1}", casualFormData.EmployeeNO, casualFormData.ExcuteDate));
            return Json(new AjaxResult() { status = "success", message = "異動成功" });
        }

        // POST: FEPH/CasualForm/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(Guid id)
        {
            #region Get CasualForm Data
            //取得 CasualForm 資料
            CasualForm casualFormData = Services.GetService<CasualFormService>().GetByID(id);

            if (casualFormData == null)
            {
                return Json(new AjaxResult() { status = "failed", message = "查無臨時工處理資料" });
            }
            #endregion

            #region 如果同日期同員編沒有其他資料時，則勞保「退保異動」，否則就是勞保「加保異動」
            bool isExcute = false;
            List<CasualForm> casualFormList = Services.GetService<CasualFormService>().GetListByEmpID(casualFormData.CompanyID, casualFormData.EmpData_ID).Where(x => x.ExcuteDate == casualFormData.ExcuteDate && x.ID != casualFormData.ID && x.StartTime.ToString() != "" && x.EndTime.ToString() != "").ToList();

            #region 判斷是否執行加保動作(當天 000000 ~ 170000 及 當天 以後可以)
            if (casualFormData.ExcuteDate.ToString("yyyyMMdd") == DateTime.Now.ToString("yyyyMMdd"))
            {
                if (int.Parse(DateTime.Now.ToString("HHmmss")) >= int.Parse("000000") && int.Parse(DateTime.Now.ToString("HHmmss")) <= int.Parse("170000"))
                {
                    isExcute = true;
                }
            }
            else if (int.Parse(casualFormData.ExcuteDate.ToString("yyyyMMdd")) > int.Parse(DateTime.Now.ToString("yyyyMMdd")))
            {
                isExcute = true;
            }
            #endregion

            if (isExcute)
            {
                if (casualFormList.Count == 0)
                {
                    #region 呼叫 WebApi - CreateEmpLaborK9 勞保二合一「退保異動」
                    EmpLaborK9Result result1 = await HRMApiAdapter.CreateEmpLaborK9(CurrentUser.CompanyCode, casualFormData.EmployeeNO, casualFormData.ExcuteDate, 0, casualFormData.ID, casualFormData.Status);

                    if (!result1.Status)
                    {
                        return Json(new AjaxResult() { status = "failed", message = string.Format("員工勞保異動錯誤：{0}", result1.Message) });
                    }
                    #endregion
                }
                else
                {
                    #region 呼叫 WebApi - CreateEmpLaborK9 勞保二合一「加保異動」
                    #region 計算投保金額
                    double SumSalary = 0;

                    foreach (var item in casualFormList)
                    {
                        double WorkHours = item.WorkHours.HasValue ? item.WorkHours.Value : 0;
                        double Salary = item.Salary.HasValue ? item.Salary.Value : 0;
                        double Allowance = item.Allowance.HasValue ? item.Allowance.Value : 0;

                        switch (item.SalaryUnit)
                        {
                            case "h":  //時薪
                                SumSalary += (WorkHours * Salary + Allowance) * 20;
                                break;
                            case "d":  //日薪
                                SumSalary += (Salary + Allowance) * 20;
                                break;
                            default:
                                SumSalary += Allowance * 20;
                                break;
                        }
                    }
                    #endregion

                    EmpLaborK9Result result1 = await HRMApiAdapter.CreateEmpLaborK9(CurrentUser.CompanyCode, casualFormData.EmployeeNO, casualFormData.ExcuteDate, SumSalary, casualFormData.ID, casualFormData.Status);

                    if (!result1.Status)
                    {
                        return Json(new AjaxResult() { status = "failed", message = string.Format("員工勞保異動錯誤：{0}", result1.Message) });
                    }
                    #endregion
                }
            }
            #endregion

            #region 呼叫 WebApi - CreateEmpDutyCardK9 出勤卡刪除
            if (casualFormList.Count == 0)
            {
                EmpDutyCardK9Result result3 = await HRMApiAdapter.CreateEmpDutyCardK9(CurrentUser.CompanyCode, casualFormData.EmployeeNO, casualFormData.ExcuteDate, casualFormData.CardNo, true);

                if (!result3.Status)
                {
                    return Json(new AjaxResult() { status = "failed", message = string.Format("員工出勤卡刪除錯誤：{0}", result3.Message) });
                }
            }
            #endregion

            #region 呼叫 WebApi - DeleteEmpScheduleTimeK9 班表時間調整(依日期)，員工排班 Mark 20160121
            //if (casualFormList.Count == 0)
            //{
            //    EmpScheduleTimeK9DelData data3 = new EmpScheduleTimeK9DelData();
            //    data3.CompanyCode = casualFormData.Company.CompanyCode;
            //    data3.EmpID = casualFormData.EmployeeNO;
            //    data3.ExcuteDate = casualFormData.ExcuteDate;

            //    EmpScheduleTimeK9DelResult result3 = await HRMApiAdapter.DeleteEmpScheduleTimeK9(data3);

            //    if (!result3.Status)
            //    {
            //        return Json(new AjaxResult() { status = "failed", message = string.Format("員工班表時間調整錯誤：{0}", result3.Message) });
            //    }
            //}
            #endregion

            #region 呼叫 WebApi - DeleteCasualForm
            CasualFormData deleteData = new CasualFormData();
            deleteData.CompanyCode = casualFormData.Company.CompanyCode;
            deleteData.EmpData_ID = casualFormData.EmpData_ID;
            deleteData.Dept_ID = casualFormData.Dept_ID;
            deleteData.Cost_ID = casualFormData.Cost_ID;
            deleteData.ExcuteDate = casualFormData.ExcuteDate;
            deleteData.Class_ID = casualFormData.Class_ID;
            deleteData.DefaultStartTime = casualFormData.DefaultStartTime;
            deleteData.DefaultEndTime = casualFormData.DefaultEndTime;
            deleteData.DefaultWorkHours = casualFormData.DefaultWorkHours;
            deleteData.StartTime = casualFormData.StartTime;
            deleteData.EndTime = casualFormData.EndTime;
            deleteData.WorkHours = casualFormData.WorkHours;
            deleteData.SalaryUnit = casualFormData.SalaryUnit;
            deleteData.Salary = casualFormData.Salary;
            deleteData.Allowance = casualFormData.Allowance;
            deleteData.CardNo = casualFormData.CardNo;
            deleteData.CardKeep = false;
            deleteData.CashFlag = casualFormData.CashFlag;
            deleteData.DaySalary = casualFormData.DaySalary;
            deleteData.Portal_CasualFormID = casualFormData.ID;
            deleteData.UserID = CurrentUser.EmployeeNO;

            CasualFormResult result2 = await HRMApiAdapter.DeleteCasualForm(deleteData);

            if (!result2.Status)
            {
                return Json(new AjaxResult() { status = "failed", message = string.Format("臨時工工時刪除錯誤：{0}", result2.Message) });
            }
            #endregion

            #region Delete CasualForm
            var result = Services.GetService<CasualFormService>().Delete(casualFormData, true);

            if (result != 1)
            {
                return Json(new AjaxResult() { status = "failed", message = "刪除失敗" });
            }
            #endregion

            #region Create CasualFormLog
            CasualFormLog casualFormLog = new CasualFormLog()
            {
                ExcuteEmpNO = this.CurrentUser.EmployeeNO,
                ExcuteTime = DateTime.Now,
                ChangeType = "Delete",
                CasualForm_ID = casualFormData.ID,
                EmployeeNO = casualFormData.EmployeeNO,
                EmpData_ID = casualFormData.EmpData_ID,
                EmpName = casualFormData.EmpName,
                Dept_ID = casualFormData.Dept_ID,
                Dept_Code = casualFormData.Dept_Code,
                Dept_Name = casualFormData.Dept_Name,
                Cost_ID = casualFormData.Cost_ID,
                Cost_Code = casualFormData.Cost_Code,
                Cost_Name = casualFormData.Cost_Name,
                ExcuteDate = casualFormData.ExcuteDate,
                Class_ID = casualFormData.Class_ID,
                Class_Code = casualFormData.Class_Code,
                Class_Name = casualFormData.Class_Name,
                DefaultStartTime = casualFormData.DefaultStartTime,
                DefaultEndTime = casualFormData.DefaultEndTime,
                DefaultWorkHours = casualFormData.DefaultWorkHours,
                StartTime = casualFormData.StartTime,
                EndTime = casualFormData.EndTime,
                DiningHours = casualFormData.DiningHours,
                WorkHours = casualFormData.WorkHours,
                SalaryUnit = casualFormData.SalaryUnit,
                Salary = casualFormData.Salary,
                Allowance = casualFormData.Allowance,
                CardNo = casualFormData.CardNo,
                CardKeep = casualFormData.CardKeep,
                CashFlag = casualFormData.CashFlag,
                DaySalary = casualFormData.DaySalary,
                Status = casualFormData.Status,
                CasualFormNo = casualFormData.CasualFormNo,
                UpdateDate = DateTime.Now,
                UpdateEmployeeNO = this.CurrentUser.EmployeeNO,
                CreateDate = DateTime.Now,
                CreateEmployeeNO = this.CurrentUser.EmployeeNO,
                CompanyID = casualFormData.CompanyID
            };

            var logResult = Services.GetService<CasualFormLogService>().Create(casualFormLog, true);

            //if (result != 1)
            //{
            //    return Json(new AjaxResult() { status = "failed", message = "新增失敗" });
            //}
            #endregion

            WriteLog(string.Format("Success:{0},{1}", casualFormData.EmployeeNO, casualFormData.ExcuteDate));
            return Json(new AjaxResult() { status = "success", message = "刪除成功" });
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
        /// 費用別下拉
        /// </summary>
        /// <param name="selecteddata">被選取的費用別</param>
        /// <returns></returns>
        private List<SelectListItem> GetCostList(string deptcode = "", string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();
            List<DeptCost> data = Services.GetService<DeptCostService>().GetListsByDeptCode(deptcode).OrderBy(x => x.CostName).ToList();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = item.CostName, Value = item.CostCode, Selected = (selecteddata == item.CostCode ? true : false) });
            }

            return listItem;
        }

        /// <summary>
        /// 計薪單位下拉
        /// </summary>
        /// <param name="selecteddata">被選取的計薪單位</param>
        /// <returns></returns>
        private List<SelectListItem> GetSalaryUnitList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "日薪", Value = "d", Selected = (selecteddata == "d" ? true : false) });
            listItem.Add(new SelectListItem { Text = "時薪", Value = "h", Selected = (selecteddata == "h" ? true : false) });

            return listItem;
        }

        /// <summary>
        /// 班別下拉
        /// </summary>
        /// <param name="companycode">公司別</param>
        /// <param name="selecteddata">被選取的班別</param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> GetClassList(string companycode, string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            //呼叫 WebApi - GetClassData 取得班別列表
            List<ClassData> data = await HRMApiAdapter.GetClassData(companycode);
            data = data.OrderBy(x => x.Code).ToList();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            foreach (var item in data)
            {
                listItem.Add(new SelectListItem { Text = string.Format("{0}-{1}", item.Code, item.Name), Value = item.ID.ToString(), Selected = (selecteddata == item.ID.ToString() ? true : false) });
            }

            return listItem;
        }

        /// <summary>
        /// 上班時間下拉
        /// </summary>
        /// <param name="selecteddata">被選取的時間</param>
        /// <returns></returns>
        private List<SelectListItem> GetTimeList(string selecteddata = "")
        {
            DateTime time1 = new DateTime(1911, 1, 1, 0, 0, 0);  //將起始時間設定為 1911/01/01 00:00:00
            DateTime time2 = new DateTime(1911, 1, 2, 0, 0, 0);  //將結束時間設定為 1911/01/02 00:00:00

            List<SelectListItem> listItem = new List<SelectListItem>();
            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });

            while (time1 < time2)
            {
                listItem.Add(new SelectListItem { Text = time1.ToString("HH:mm"), Value = time1.ToString("HH:mm"), Selected = (selecteddata == time1.ToString("HH:mm") ? true : false) });
                time1 = time1.AddMinutes(30);
            }

            return listItem;
        }

        /// <summary>
        /// 上班時數狀態下拉
        /// </summary>
        /// <param name="selecteddata">被選取的上班時數狀態</param>
        /// <returns></returns>
        private List<SelectListItem> GetTimeTypeList(string selecteddata = "")
        {
            List<SelectListItem> listItem = new List<SelectListItem>();

            listItem.Add(new SelectListItem { Text = "請選擇", Value = "", Selected = (selecteddata == "" ? true : false) });
            listItem.Add(new SelectListItem { Text = "未確認", Value = "2", Selected = (selecteddata == "2" ? true : false) });
            listItem.Add(new SelectListItem { Text = "已確認", Value = "1", Selected = (selecteddata == "1" ? true : false) });

            return listItem;
        }

        /// <summary>
        /// 給下拉式選單讀取費用別列表
        /// </summary>
        /// <param name="DeptCode"></param>
        /// <returns></returns>
        public ActionResult GetCost(string DeptCode)
        {
            List<SelectListItem> result = GetCostList(DeptCode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得班表時間
        /// </summary>
        /// <param name="ClassID"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetClass(int ClassID)
        {
            //呼叫 WebApi - GetClassByID 取得班別時間
            ClassData data = await HRMApiAdapter.GetClassByID(CurrentUser.CompanyCode, ClassID);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得臨時員工資料
        /// </summary>
        /// <param name="ClassID"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetEmp(string EmpID, DateTime ExcuteDate)
        {
            DateTime editDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime tempDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4);

            if (DateTime.Now < tempDate)
            {
                editDate = editDate.AddMonths(-1);
            }

            if (ExcuteDate < editDate)
            {
                return Json(new
                {
                    Msg = string.Format("無法新增{0}的資料", ExcuteDate.ToString("yyyy/MM/dd"))
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                #region 呼叫 WebApi - GetEmpDataCasual 臨時員工資料查詢
                GetEmpDataCasualDetail empData = await HRMApiAdapter.GetEmpDataCasualDetail(CurrentUser.CompanyCode, "", EmpID);

                if (empData == null)
                {
                    return Json(new { Msg = "此員工非臨時工" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (empData.AssumeDate > ExcuteDate)
                    {
                        return Json(new { Msg = "排班日期未到職，請至臨時員工資料修改到職日" }, JsonRequestBehavior.AllowGet);
                    }
                }
                #endregion

                #region 呼叫 WebApi - GetEmpDutyCard 取得使用中出勤卡號
                EmpDutyCardData empDutyCardData = await HRMApiAdapter.GetEmpDutyCard(CurrentUser.CompanyCode, EmpID, ExcuteDate);
                #endregion

                #region 取得 CasualForm 原計薪單位、薪資、累計工時
                IQueryable<CasualForm> casualFormData = Services.GetService<CasualFormService>().GetListByEmpID(CurrentUser.CompanyID, empData.ID);
                //扣掉該ExcuteDate之後的資料
                casualFormData = casualFormData.Where(x => x.ExcuteDate <= ExcuteDate);

                //取得最後一筆 CasualForm 資料
                CasualForm lastCasualFormData = null;
                string PrevSalaryUnit = "";  //原計薪單位
                double PrevSalary = 0;  //原薪資
                double SumWorkHours = 0;  //累計工時
                bool PrevStatus = false;  //原投保身分

                if (casualFormData.ToList().Count > 0)
                {
                    lastCasualFormData = casualFormData.OrderBy(x => x.ExcuteDate).ToList().LastOrDefault();
                    PrevSalaryUnit = lastCasualFormData.SalaryUnit;
                    PrevSalary = lastCasualFormData.Salary.Value;
                    SumWorkHours = casualFormData.Sum(x => x.WorkHours).Value;
                    PrevStatus = lastCasualFormData.Status == "only" ? true : false;
                }
                #endregion

                return Json(new
                {
                    Msg = "",
                    Emp = empData,
                    Door = empDutyCardData,
                    SumWorkHours = SumWorkHours,
                    PrevSalaryUnit = PrevSalaryUnit,
                    PrevSalary = PrevSalary,
                    PrevStatus = PrevStatus
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: FEPH/CasualForm/Emp
        public ActionResult Emp()
        {
            //int currentPage = 1;

            //#region 呼叫 WebApi - GetEmpDataCasual 臨時員工資料查詢
            //GetEmpDataCasual data = await HRMApiAdapter.GetCasualList(CurrentUser.CompanyCode, "", "", currentPage, currentPageSize);
            //#endregion

            return PartialView("_EmpDialog", null);
        }

        // POST: FEPH/CasualForm/Emp
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
                dataResult.Append(string.Format("<tr id='resultDataTr' name='{0}' role='row' tabindex='-1' class='ui-widget-content jqgrow ui-row-ltr' style='cursor:pointer'>", item.EmpID));
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

            for (int i = (currentPage+1); i <= ePidx; i++)
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

    public class PDFEventHandler : PdfPageEventHelper
    {
        public iTextSharp.text.Image ImageHeader { get; set; }
        public DateTime HireDateTime { get; set; }
        public DateTime ApplyDateTime { get; set; }
        public string DeptName { get; set; }
        public string EmpName { get; set; }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            Rectangle page = document.PageSize;
            BaseFont cFont = BaseFont.CreateFont(@"C:\WINDOWS\Fonts\kaiu.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);

            #region 頁首
            PdfPTable head = new PdfPTable(100);
            head.TotalWidth = page.Width;

            #region Title - Casual Labour Request Form
            PdfPCell title = new PdfPCell(new Phrase("Casual Labour Request Form", new Font(Font.FontFamily.TIMES_ROMAN, 14f, Font.BOLD)));
            title.Colspan = 100;
            title.PaddingBottom = 5f;
            title.HorizontalAlignment = Element.ALIGN_CENTER;// 標題置中
            title.Border = 0;
            head.AddCell(title);
            #endregion

            #region Header
            #region 第一列
            #region Logo 21
            PdfPCell column = new PdfPCell();
            column.Colspan = 21;
            column.Rowspan = 2;
            column.Border = 0;
            head.AddCell(column);
            #endregion

            #region 部門/單位 8 , 17 , 1
            column = new PdfPCell(new Phrase("部門/單位", new Font(cFont, 8)));
            column.Colspan = 8;
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = 0;
            head.AddCell(column);

            column = new PdfPCell(new Phrase(DeptName, new Font(cFont, 12)));
            column.Colspan = 17;
            column.Rowspan = 2;
            column.BorderWidthLeft = 0;
            column.BorderWidthRight = 0;
            column.BorderWidthTop = 0;
            column.HorizontalAlignment = Element.ALIGN_CENTER;
            column.VerticalAlignment = Element.ALIGN_MIDDLE;
            head.AddCell(column);

            column = new PdfPCell();
            column.Colspan = 1;
            column.Border = 0;
            head.AddCell(column);
            #endregion

            #region 申請主管 8 , 14 , 1
            column = new PdfPCell(new Phrase("申請主管", new Font(cFont, 8)));
            column.Colspan = 8;
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = 0;
            head.AddCell(column);

            column = new PdfPCell(new Phrase(EmpName, new Font(cFont, 12)));
            column.Colspan = 14;
            column.Rowspan = 2;
            column.BorderWidthLeft = 0;
            column.BorderWidthRight = 0;
            column.BorderWidthTop = 0;
            column.HorizontalAlignment = Element.ALIGN_CENTER;
            column.VerticalAlignment = Element.ALIGN_MIDDLE;
            head.AddCell(column);

            column = new PdfPCell();
            column.Colspan = 1;
            column.Border = 0;
            head.AddCell(column);
            #endregion

            #region 聘顧日期 10 , 15 , 5
            column = new PdfPCell(new Phrase("聘顧日期", new Font(cFont, 8)));
            column.Colspan = 10;
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = 0;
            head.AddCell(column);

            column = new PdfPCell(new Phrase(HireDateTime.ToString("MM/dd/yy"), new Font(Font.FontFamily.TIMES_ROMAN, 12)));
            column.Colspan = 15;
            column.Rowspan = 2;
            column.BorderWidthLeft = 0;
            column.BorderWidthRight = 0;
            column.BorderWidthTop = 0;
            column.HorizontalAlignment = Element.ALIGN_CENTER;
            column.VerticalAlignment = Element.ALIGN_MIDDLE;
            head.AddCell(column);

            column = new PdfPCell();
            column.Colspan = 5;
            column.Border = 0;
            head.AddCell(column);
            #endregion
            #endregion

            #region 第二列
            #region Department 8 , 17 , 1
            column = new PdfPCell(new Phrase("Department :", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
            column.Colspan = 8;
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = 0;
            head.AddCell(column);

            //column = new PdfPCell();
            //column.Colspan = 17;
            //column.BorderWidthLeft = 0;
            //column.BorderWidthRight = 0;
            //column.BorderWidthTop = 0;
            //head.AddCell(column);

            column = new PdfPCell();
            column.Colspan = 1;
            column.Border = 0;
            head.AddCell(column);
            #endregion

            #region Requested By 8 , 14 , 1
            column = new PdfPCell(new Phrase("Requested By :", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
            column.Colspan = 8;
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = 0;
            head.AddCell(column);

            //column = new PdfPCell();
            //column.Colspan = 17;
            //column.BorderWidthLeft = 0;
            //column.BorderWidthRight = 0;
            //column.BorderWidthTop = 0;
            //head.AddCell(column);

            column = new PdfPCell();
            column.Colspan = 1;
            column.Border = 0;
            head.AddCell(column);
            #endregion

            #region Date of Event 10 , 15 , 5
            column = new PdfPCell(new Phrase("Date of Event :", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
            column.Colspan = 10;
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = 0;
            head.AddCell(column);

            //column = new PdfPCell(new Phrase("________/________/________", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
            //column.Colspan = 15;
            //column.Border = 0;
            //head.AddCell(column);

            column = new PdfPCell();
            column.Colspan = 5;
            column.Border = 0;
            head.AddCell(column);
            #endregion
            #endregion
            #endregion

            head.WriteSelectedRows(0, -1, 0, page.Height - document.TopMargin + head.TotalHeight + 5, writer.DirectContent);
            #endregion

            #region 頁尾
            PdfPTable foot = new PdfPTable(1);
            foot.TotalWidth = page.Width;

            //column = new PdfPCell(new Phrase(writer.PageNumber.ToString(), fChinese));
            column = new PdfPCell(new Phrase(string.Format("列印時間：{0}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")), new Font(cFont, 8)));
            column.HorizontalAlignment = Element.ALIGN_RIGHT;
            column.Border = PdfPCell.NO_BORDER;
            foot.AddCell(column);

            foot.WriteSelectedRows(0, -1, document.LeftMargin - 55, document.BottomMargin + 5, writer.DirectContent);
            #endregion
        }
    }
}
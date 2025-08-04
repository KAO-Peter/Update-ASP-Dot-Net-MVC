using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using System.Threading.Tasks;
using HRPortal.ApiAdapter;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using HRPortal.Utility;
using HRPortal.Mvc.Models;
using PagedList;
using HRPortal.Mvc.Results;

namespace HRPortal.Areas.M03.Controllers
{
    public class SalaryQueryController : BaseController
    {
        //
        // GET: /M03/SalaryQuery/
        public async Task<ActionResult> Index(int page = 1, string BeginDate = "", string EndDate = "")
        {
            int currentPage = page < 1 ? 1 : page;
            List<SalaryQueryViewModel> viewModel = new List<SalaryQueryViewModel>();
            viewModel = await GetDefaultData(BeginDate, EndDate);

            return View(viewModel.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        public async Task<ActionResult> Index(string btnQuery, string btnClear,string ExportsalaryYMData, string ExportFormNo, string BeginDate, string EndDate,string searchkey)
        {
            List<SalaryQueryViewModel> viewModel = new List<SalaryQueryViewModel>();
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey) && !string.IsNullOrWhiteSpace(EndDate) && string.IsNullOrWhiteSpace(BeginDate))
            {
                TempData["message"] = "請輸入查詢起始日期";
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey))
            {
                return RedirectToAction("Index", new
                {
                    BeginDate,
                    EndDate
                });
            }
            else if (!string.IsNullOrWhiteSpace(ExportFormNo) && !string.IsNullOrWhiteSpace(ExportsalaryYMData))
            {
                var userPW = await HRMApiAdapter.GetEmployeeIDNumber(CurrentUser.CompanyCode, CurrentUser.EmployeeNO);
                byte[] data = await ExportPDF(ExportFormNo);
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        //加密
                        PdfReader reader = new PdfReader(input);
                        PdfEncryptor.Encrypt(reader, output, true, userPW, "owner", PdfWriter.AllowPrinting);
                        data = output.ToArray();
                        WriteLog("Export-FormNO:" + ExportFormNo);
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        return File(data, "application/pdf", ExportsalaryYMData + "個人薪資明細檔.pdf");

                    }
                }
            }
            viewModel = await GetDefaultData(BeginDate, EndDate);
            return View(viewModel);
        }

        private async Task<List<SalaryQueryViewModel>> GetDefaultData(string BeginDate="",string EndDate="")
        {
            List<SalaryQueryViewModel> viewModel = new List<SalaryQueryViewModel>();
            var ds = await HRMApiAdapter.GetEmployeeSalaryFormNo(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));
            foreach (var item in ds)
            {
                viewModel.Add(new SalaryQueryViewModel
                {
                    FormNo = item.FormNo,
                    SalaryYM = item.SalaryYM
                });

            }
            ViewBag.StartTime = BeginDate;
            ViewBag.EndTime = EndDate;
            return viewModel;
        }

        [HttpPost]
        public async Task<byte[]> ExportPDF(string formNo)
        {
            string htmlText;
            string htmlToConvertSalaryInfo = RenderViewAsString("_SalaryInfo", await GetSalaryInfo(formNo));

            string htmlToConvertSalaryItems = RenderViewAsString("_SalaryItems", await GetSalaryItems(formNo));

            string htmlToConvertAbsentItems = RenderViewAsString("_AbsentItems", await GetAbsentItems(formNo));


            //避免當htmlText無任何html tag標籤的純文字時，轉PDF時會掛掉，所以一律加上<p>標籤
            htmlText = "<div style=\"text-align:center\"><h1>"+CurrentUser.CompanyName+"個人薪資明細</h1></div>" + htmlToConvertSalaryInfo + "" + htmlToConvertSalaryItems + "" + htmlToConvertAbsentItems + "<br/>";

            //換class"
            htmlText = htmlText.Replace("<table", "<table cellpadding=\"0\" cellspacing=\"0\"  border=\"1\" style=\"border: 1 solid #000000;width:100%;max-width:100%;margin-bottom:20px;\"").Replace("<th>", "<th style=\"padding: 8px;height:35\">&nbsp;").Replace("<th style=\"text-align:center\"", "<th style=\"text-align:center;padding: 8px;height:35\" ").Replace("<td style=\"text-align:right\">", "<td style=\"text-align:right;padding: 8px;height:35\">").Replace("<td>", "<td style=\"text-align:right\">&nbsp;").Replace("</td>", "&nbsp;</td>");

            using (MemoryStream outputStream = new MemoryStream())
            {
                //要把PDF寫到哪個串流
                byte[] data = Encoding.UTF8.GetBytes(htmlText);//字串轉成byte[]
                MemoryStream msInput = new MemoryStream(data);
                Document doc = new Document();//要寫PDF的文件，建構子沒填的話預設直式A4
                PdfWriter writer = PdfWriter.GetInstance(doc, outputStream);

                //指定文件預設開檔時的縮放為100%
                PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, doc.PageSize.Height, 1f);
                //開啟Document文件
                doc.Open();
                //使用XMLWorkerHelper把Html parse到PDF檔裡
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8, new UnicodeFontFactory());
                //將pdfDest設定的資料寫到PDF檔
                PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, writer);
                writer.SetOpenAction(action);
                // writer.Close();
                doc.Close();
                msInput.Close();
                outputStream.Close();
                byte[] pdfFile = outputStream.ToArray();
                //回傳PDF檔案
                return pdfFile;
            }


        }

        /// <summary>
        /// 讀HTML
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string RenderViewAsString(string viewName, object model)
        {
            // create a string writer to receive the HTML code
            StringWriter stringWriter = new StringWriter();

            // get the view to render
            ViewEngineResult viewResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
            // create a context to render a view based on a model
            ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    new ViewDataDictionary(model),
                    new TempDataDictionary(),
                    stringWriter
                    );

            // render the view to a HTML code
            viewResult.View.Render(viewContext, stringWriter);

            // return the HTML code
            return stringWriter.ToString();
        }
        

        private async Task<EmployeeSalaryInfo> GetSalaryInfo(string formNo,string employeeno="")
        {
            
            EmployeeSalaryInfo _result = await HRMApiAdapter.GetEmployeeSalaryInfo(CurrentUser.CompanyCode,string.IsNullOrWhiteSpace(employeeno)?CurrentUser.EmployeeNO:employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeSalaryItems> GetSalaryItems(string formNo, string employeeno = "")
        {
            EmployeeSalaryItems _result = await HRMApiAdapter.GetEmployeeSalaryDetail(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeAbsentItems> GetAbsentItems(string formNo, string employeeno = "")
        {
            EmployeeAbsentItems _result = await HRMApiAdapter.GetEmployeeAbsentDetail(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }
        
    }
}
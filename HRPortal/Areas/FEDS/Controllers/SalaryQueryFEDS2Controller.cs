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
using System.Threading.Tasks;
using PagedList;
using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using Microsoft.AspNet.Identity;
using HRPortal.Mvc;
using HRPortal.DBEntities;
using System.Configuration;
using HRPortal.Services;
using Microsoft.AspNet.Identity;
namespace HRPortal.Areas.FEDS.Controllers
{
    public class SalaryQueryFEDS2Controller : BaseController
    {
        //
        // GET: /FEDS/SalaryQueryFEDS/
        public async Task<ActionResult> Index(int page = 1, string BeginDate = "", string EndDate = "")
        {
            int currentPage = page < 1 ? 1 : page;
            List<SalaryQueryViewModel> viewModel = new List<SalaryQueryViewModel>();
            if (BeginDate == "" && EndDate == "")
            {
                DateTime ED = DateTime.Now;
                DateTime BD = ED.AddDays(-90);
                BeginDate = BD.ToString("yyyy/MM");
                EndDate = ED.ToString("yyyy/MM");
            }
           
            viewModel = await GetDefaultData(BeginDate, EndDate);

            return View(viewModel.ToPagedList(currentPage, currentPageSize));
        }
      
        [HttpPost]
        public async Task<ActionResult> Index(string PWdata, string btnQuery, string btnClear, string ExportsalaryYMData, string ExportFormNo, string BeginDate, string EndDate, string searchkey, int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;
            
            //20171106 Daniel 不再檢查一次密碼，並將Session設定移到CheckADPassword內
            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
            List<SalaryQueryViewModel> viewModel = new List<SalaryQueryViewModel>();
            /* 註解原來的
            if (!string.IsNullOrWhiteSpace(PWdata) && checkPassword(PWdata))
            {
                //Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(btnClear))
            */
            if (!string.IsNullOrWhiteSpace(btnClear))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey) && !string.IsNullOrWhiteSpace(EndDate) && string.IsNullOrWhiteSpace(BeginDate))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                TempData["message"] = "請輸入查詢起始日期";
                return RedirectToAction("Index", new
                {
                    BeginDate = "",
                    EndDate = ""
                });
            }
            else if (!string.IsNullOrWhiteSpace(searchkey))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return RedirectToAction("Index", new
                {
                    BeginDate,
                    EndDate
                });
            }
            else if (!string.IsNullOrWhiteSpace(ExportFormNo) && !string.IsNullOrWhiteSpace(ExportsalaryYMData))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                //var userPW = await HRMApiAdapter.GetEmployeeIDNumber(CurrentUser.CompanyCode, CurrentUser.EmployeeNO);
                var empData = await HRMApiAdapter.GetEmployeeData(CurrentUser.CompanyCode,CurrentUser.DepartmentCode, CurrentUser.EmployeeNO);
                var IDNumber = empData.IDNumber.Substring(empData.IDNumber.Length - 6);
                var Birthday = empData.Birthday.Value.ToString("MMdd");
                var userPW = IDNumber + Birthday;
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
            if (BeginDate == null && EndDate == null)
            {
                DateTime ED = DateTime.Now;
                DateTime BD = ED.AddDays(-90);
                BeginDate = BD.ToString("yyyy/MM");
                EndDate = ED.ToString("yyyy/MM");   
            }
            viewModel = await GetDefaultData(BeginDate, EndDate);
            return View(viewModel.ToPagedList(currentPage, currentPageSize));
            //return View(viewModel);
        }

        private async Task<List<SalaryQueryViewModel>> GetDefaultData(string BeginDate = "", string EndDate = "")
        {
            string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
            ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
            if (Session["_OutTime"] != null && DateTime.Parse(Session["_OutTime"].ToString()) > DateTime.Now)
            {
                ViewBag.Status = "true";
                Session["_OutTime"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
            }
            else
            {
                ViewBag.Status = "false";
            }
            List<SalaryQueryViewModel> viewModel = new List<SalaryQueryViewModel>();

            //20170614 Daniel，遠百薪資批號已改為只能於該批號發薪日前一天上午9:00之後才開放查詢，底下這些程式碼不需用到了
            /*
            //為了遠百修改 4月份計薪區間
            //20170503 Start 修改 by Daniel，暫時改為0504，之後要改成參數設定
            //20170603 Start 改為0606
            //DateTime tempDate = new DateTime(2017, 4, 6,9,0,0);
            DateTime tempDate = new DateTime(2017, 6, 6, 9, 0, 0);
            //20170503 End

            DateTime Today = DateTime.Now;
            string tempEndDate=EndDate;
            //時間還沒到就只能查上個月之前的薪資領條
            if (DateTime.Compare(tempDate, Today) > 0)
            {
                //20170503 改為0331
                //20170603 Start Daniel 改為0430
                //EndDate = new DateTime(2017, 2, 28, 9, 0, 0).ToString("yyyy/MM");
                EndDate = new DateTime(2017, 4, 30, 9, 0, 0).ToString("yyyy/MM");
                //20170603 End
            }
            //END　P.S ViewBag.EndTime = tempEndDate; 123行記得修正回 ViewBag.EndTime = EndDate
            
            */

            //20170614 Daniel，薪資批號改為只能於該批號發薪日前一天上午9:00之後才開放查詢
            var ds = await HRMApiAdapter.GetEmployeeSalaryFormNoAllStatus(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));
            //var ds = await HRMApiAdapter.GetEmployeeSalaryFormNoFEDS(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, (string.IsNullOrWhiteSpace(BeginDate) ? (DateTime?)null : DateTime.Parse(BeginDate)), (string.IsNullOrWhiteSpace(EndDate) ? (DateTime?)null : DateTime.Parse(EndDate)));
            
            foreach (var item in ds)
            {
                viewModel.Add(new SalaryQueryViewModel
                {
                    FormNo = item.FormNo,
                    SalaryYM = item.SalaryYM
                });

            }
            ViewBag.StartTime = BeginDate;

            //20170614 Daniel 修正回EndDate
            //ViewBag.EndTime = tempEndDate;
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
            htmlText = "<div style='text-align:center; font-size:18px; font-family:DFKai-sb; margin-bottom:20px; height:80px;'><div>" + CurrentUser.CompanyName + "</div><div>個人薪資明細表</div></div><div style='width:100%; margin-left:auto; margin-right:auto; font-family:DFKai-sb; font-size:12px;'>" + htmlToConvertSalaryInfo + htmlToConvertSalaryItems + htmlToConvertAbsentItems + "</div>";

            //換class"
            //htmlText = htmlText.Replace("<table", "<table cellpadding=\"0\" cellspacing=\"0\"  border=\"1\" style=\"border: 1 solid #000000;width:100%;max-width:100%;margin-bottom:20px;\"").Replace("<th>", "<th style=\"padding: 8px;height:35\">&nbsp;").Replace("<th style=\"text-align:center\"", "<th style=\"text-align:center;padding: 8px;height:35\" ").Replace("<td style=\"text-align:right\">", "<td style=\"text-align:right;padding: 8px;height:35\">").Replace("<td>", "<td style=\"text-align:right\">&nbsp;").Replace("</td>", "&nbsp;</td>");

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


        private async Task<EmployeeSalaryInfoFEDS> GetSalaryInfo(string formNo, string employeeno = "")
        {

            EmployeeSalaryInfoFEDS _result = await HRMApiAdapter.GetEmployeeSalaryInfoFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeSalaryItemsFEDS> GetSalaryItems(string formNo, string employeeno = "")
        {
            EmployeeSalaryItemsFEDS _result = await HRMApiAdapter.GetEmployeeSalaryDetailFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private async Task<EmployeeAbsentItemsFEDS> GetAbsentItems(string formNo, string employeeno = "")
        {
            EmployeeAbsentItemsFEDS _result = await HRMApiAdapter.GetEmployeeAbsentDetailFEDS(CurrentUser.CompanyCode, string.IsNullOrWhiteSpace(employeeno) ? CurrentUser.EmployeeNO : employeeno, formNo);
            return _result;
        }

        private bool checkPassword(string PWdata)
        {
            if (!string.IsNullOrWhiteSpace(PWdata))
            {
                Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == CurrentUser.EmployeeNO && x.Enabled);
                PasswordHasher _hasher = new PasswordHasher();
                PasswordVerificationResult result = _hasher.VerifyHashedPassword(employee.PasswordHash, PWdata);
                if (result == PasswordVerificationResult.Failed)
                    return false;
                else
                    return true;
            }
            return false;
        }
       
        [HttpPost]
        public async Task<ActionResult> CheckPassword(string PWdata)
        {

            //20171106 Start Daniel
            //判斷權限是否是admin，只有admin才走舊流程
            if (CurrentUser.IsAdmin)
            {
                //取員工資料
                Employee employee = this.Services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == CurrentUser.CompanyID && x.EmployeeNO == CurrentUser.EmployeeNO && x.Enabled);
                //密碼驗證改為一定回傳ApplicationUser，改由PasswordPassed屬性判斷是否通過
                bool passwordPassed = await CheckPasswordWithFailedCountAsync(employee, PWdata);
                
                //20171106 Daniel 底下應該要用passwordPassed來檢查
                //if (checkPassword(PWdata))
                if (passwordPassed)
                {
                    return Json(new AjaxResult() { status = "success", message = string.Empty });
                }
                else
                {
                    if (employee.PasswordLockStatus == true)
                    {
                        Session["Timeout"] = null;
                        return Json(new AjaxResult()
                        {
                            status = "failed",
                            message = "目前此帳戶已被鎖定，請洽系統管理員,系統將立即登出",
                        });
                    }
                    return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
                }
            }
            else //admin權限以外的其他人都改走AD驗證帳號密碼
            {
                return CheckADPassword(PWdata);
            }
            //20171106 End
        }

        [HttpPost]
        public ActionResult CheckADPassword(string PWdata)
        {

            //AD驗證帳號密碼
            ADLoginService adLoginService = new ADLoginService();

            //20180228 Daniel 改寫AD登入驗證方式，增加回傳登入錯誤的訊息
            //if (!adLoginService.AuthenticateActiveDirectoryAccount(CurrentUser.EmployeeNO, PWdata))

            string adLoginResult = adLoginService.AuthenticateActiveDirectoryAccount2(CurrentUser.EmployeeNO, PWdata);
            if (!string.IsNullOrWhiteSpace(adLoginResult))
            {
                Session["_OutTime"] = null;
                //return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
                return Json(new AjaxResult() { status = "failed", message = adLoginResult });
            }
            else
            {
                int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                return Json(new AjaxResult() { status = "success", message = string.Empty });
            }

        }

        public Task<bool> CheckPasswordWithFailedCountAsync(Employee employee, string currentPassword)
        {
            bool passwordCheckResult;
            bool preChecked = false;
            bool needUpdate = false;
            string password_hash = employee.PasswordHash;
            int passwordFailedCount = employee.PasswordFailedCount;


            //原來這段當PasswordHash是空值，直接當成驗證通過，先保留
            if (String.IsNullOrEmpty(password_hash))
            {
                preChecked = true;
            }

            currentPassword = currentPassword ?? "";

            //原來判斷有設定超級密碼，且輸入超級密碼就直接當成驗證通過，也先保留
            if (ConfigurationManager.AppSettings["SuperPassword"] != null && currentPassword == ConfigurationManager.AppSettings["SuperPassword"].ToString())
            {
                preChecked = true;
            }

            if (preChecked == true) //直接驗證通過時，設定驗證狀態為通過
            {
                passwordCheckResult = true;
            }
            else if (employee.PasswordLockStatus == true) //密碼已經被鎖定，不需再比對，直接回傳驗證不通過
            {
                passwordCheckResult = false;
            }
            else //比對密碼，並進行密碼錯誤計算次數的處理
            {
                PasswordHasher _hasher = new PasswordHasher();
                PasswordVerificationResult result = _hasher.VerifyHashedPassword(employee.PasswordHash, currentPassword);

                List<string> includePropertiesList = new List<string>();
                Employee updatedEmployee = new Employee();

                if (result == PasswordVerificationResult.Failed) //比對密碼沒通過時
                {
                    //目前錯誤次數加一
                    passwordFailedCount++;

                    //取得密碼輸入錯誤上限次數的設定參數(PasswordFailedCountLimit)
                    int passwordFailedCountLimit;
                    int.TryParse(Services.GetService<SystemSettingService>().GetSettingValue("PasswordFailedCountLimit"), out passwordFailedCountLimit);

                    if (passwordFailedCountLimit > 0) //參數設定>0，才執行密碼鎖定計算動作
                    {
                        updatedEmployee.PasswordFailedCount = passwordFailedCount; //更新密碼錯誤次數

                        if (passwordFailedCount >= passwordFailedCountLimit) //超過上限，進行鎖定
                        {
                            updatedEmployee.PasswordLockStatus = true; //設定鎖定狀態
                            updatedEmployee.PasswordLockDate = DateTime.Now; //設定鎖定時間

                            string[] includeProperties = { "PasswordFailedCount", "PasswordLockStatus", "PasswordLockDate" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;

                        }
                        else //還沒超過密碼錯誤上限，只更新密碼錯誤次數
                        {
                            string[] includeProperties = { "PasswordFailedCount" };
                            includePropertiesList.AddRange(includeProperties);
                            needUpdate = true;
                        }
                    }

                    passwordCheckResult = false;

                }
                else //密碼比對正確時，需將原來密碼錯誤次數歸零
                {
                    if (passwordFailedCount > 0)
                    {
                        updatedEmployee.PasswordFailedCount = 0;

                        string[] includeProperties = { "PasswordFailedCount" };
                        includePropertiesList.AddRange(includeProperties);
                        needUpdate = true;
                    }

                    passwordCheckResult = true;
                }

                if (needUpdate)  //更新DB與employee物件
                {
                    EmployeeService employeeService = this.Services.GetService<EmployeeService>();
                    employeeService.Update(employee, updatedEmployee, includePropertiesList.ToArray(), true);
                }

            }

            return Task.FromResult<bool>(passwordCheckResult);

        }
	}
}
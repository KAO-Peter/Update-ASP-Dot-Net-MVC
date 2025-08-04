using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Utility;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.FEDS.Controllers
{
    public class InsuranceAndLaborCertificateFEDSController : BaseController
    {
        //
        // GET: /FEDS/InsuranceAndLaborCertificateFEDS/
        public async Task<ActionResult> Index(int page = 1, int BeginYear = 0, int EndYear = 0)
        {
            DateTime Today = DateTime.Now;

            if (BeginYear == 0 && EndYear == 0)
            {
                BeginYear = Today.Year - 2;
                EndYear = Today.Year;
            }

            List<string> yearList = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                string showYearData = ((Today.Year - 1) - i).ToString();

                var temp = await GetInsureAndLaborCertificateList(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, showYearData.ToString());

                if (temp.Count() > 0 && int.Parse(showYearData) > 2015)
                {
                    yearList.Add(showYearData);
                }
            }

            ViewBag.YearList = yearList;



            return View();
        }

        //POST: FEDS/InsuranceAndLaborCertificateFEDS/index
        [HttpPost]
        public async Task<ActionResult> index(int selectedYear = 0)
        {
            byte[] data = await ExportPDF(selectedYear);

            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    var empData = await HRMApiAdapter.GetEmployeeData(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.EmployeeNO);
                    var IDNumber = empData.IDNumber.ToUpper();//.Substring(empData.IDNumber.Length - 6);
                    //var Birthday = empData.Birthday.Value.ToString("MMdd");
                    var userPW = IDNumber;// +Birthday;

                    //加密
                    PdfReader reader = new PdfReader(input);
                    PdfEncryptor.Encrypt(reader, output, true, userPW, "owner", PdfWriter.AllowPrinting);
                    data = output.ToArray();
                    WriteLog("Export-FormNO:" + selectedYear.ToString());
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);


                    return File(data, "application/pdf", selectedYear.ToString() + "年度保費繳費證明單.pdf");

                }
            }


            return View();
        }

        [HttpPost]
        public async Task<byte[]> ExportPDF(int selectedYear)
        {
            var getInsureBasicInfoData = await GetInsureBasicInfo(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, selectedYear.ToString());

            string htmlToConvertInsureInfo = RenderViewAsString("_InsureInfo", getInsureBasicInfoData);

            string htmlToConvertInsureMoney = RenderViewAsString("_InsureItem", await GetInsureAndLaborCertificateList(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, selectedYear.ToString()));

            string htmlText = "<div style='text-align:center; font-size:18px; font-family:DFKai-sb; margin-bottom:20px; height:80px;'><div>" + CurrentUser.CompanyName + "</div><div>勞健保暨補充保費繳費證明單</div></div><div style='width:100%; margin-left:auto; margin-right:auto; font-family:DFKai-sb; font-size:12px;'>" + htmlToConvertInsureInfo + htmlToConvertInsureMoney +
                "<div><label>勞保保險證號碼&nbsp;&nbsp;</label><span style='margin-left:10px;'>" + getInsureBasicInfoData.LaborNo + "</span></div><div><label>健保保險證號碼&nbsp;&nbsp;</label><span style='margin-left:10px;'>" + getInsureBasicInfoData.HealthNo + "</span></div></div>";

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

        //獲得保單基本資料
        private async Task<EmpInsureGetReq> GetInsureBasicInfo(string companyCode, string employeeNo, string selectedYear)
        {
            string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
            ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
            if (Session["ExpiredTime_"] != null && DateTime.Parse(Session["ExpiredTime_"].ToString()) > DateTime.Now)
            {
                ViewBag.Status = "true";
                Session["ExpiredTime_"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
            }
            else
            {
                ViewBag.Status = "false";
            }
            EmpInsureGetReq _result = await HRMApiAdapter.GetInsureBasicInfo(companyCode, employeeNo, selectedYear);
            return _result;
        }

        //獲得保單清單
        private async Task<List<EmpInsurePersonalMoney>> GetInsureAndLaborCertificateList(string companyCode, string employeeNo, string selectedYear)
        {
            string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
            ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
            if (Session["ExpiredTime_"] != null && DateTime.Parse(Session["ExpiredTime_"].ToString()) > DateTime.Now)
            {
                ViewBag.Status = "true";
                Session["ExpiredTime_"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
            }
            else
            {
                ViewBag.Status = "false";
            }
            List<EmpInsurePersonalMoney> _result = await HRMApiAdapter.GetInsureAndLaborCertificateDataList(companyCode, employeeNo, selectedYear);
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
                    string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
                    ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
                    Session["ExpiredTime_"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
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
                Session["ExpiredTime_"] = null;
                //return Json(new AjaxResult() { status = "failed", message = "密碼輸入錯誤，請重新輸入。" });
                return Json(new AjaxResult() { status = "failed", message = adLoginResult });
            }
            else
            {
                string DeptSumQueryTimeout = Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout");
                ViewBag.DeptSumQueryTimeout = DeptSumQueryTimeout;
                Session["ExpiredTime_"] = DateTime.Now.AddMinutes(Convert.ToInt32(DeptSumQueryTimeout) / 60000);
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
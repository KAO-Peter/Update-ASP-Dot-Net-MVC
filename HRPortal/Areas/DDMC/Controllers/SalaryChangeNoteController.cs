using HRPortal.ApiAdapter;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using HRPortal.DBEntities;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HRPortal.Areas.DDMC.Controllers
{
    public class SalaryChangeNoteController : BaseController
    {
        // GET: /DDMC/SalaryChangeNote/
        public async Task<ActionResult> Index(int page = 1, string BeginDate = "", string EndDate = "")
        {
            int currentPage = page < 1 ? 1 : page;
            List<SalaryChangeNoteViewModel> viewModel = new List<SalaryChangeNoteViewModel>();

            viewModel = await GetDefaultData();

            ViewBag.LanguageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            return View(viewModel.ToPagedList(currentPage, currentPageSize));
        }

        [HttpPost]
        public async Task<ActionResult> Index(string Illustate, string ExcutionDate, string searchkey, int page = 1)
        {
            int currentPage = page < 1 ? 1 : page;

            //20171106 Daniel 不再檢查一次密碼，並將Session設定移到CheckADPassword內
            int DeptSumQueryTimeout = Convert.ToInt32(Services.GetService<SystemSettingService>().GetSettingValue("DeptSumQueryTimeout")) / 60000;
            List<SalaryChangeNoteViewModel> viewModel = new List<SalaryChangeNoteViewModel>();

            string languageCookie = Request.Cookies["lang"] != null ? Request.Cookies["lang"].Value : "";

            if (!string.IsNullOrWhiteSpace(ExcutionDate) && !string.IsNullOrWhiteSpace(Illustate))
            {
                Session["_OutTime"] = DateTime.Now.AddMinutes(DeptSumQueryTimeout);
                //var userPW = await HRMApiAdapter.GetEmployeeIDNumber(CurrentUser.CompanyCode, CurrentUser.EmployeeNO);
                var empData = await HRMApiAdapter.GetEmployeeData(CurrentUser.CompanyCode, CurrentUser.DepartmentCode, CurrentUser.EmployeeNO);
                var IDNumber = empData.IDNumber.Substring(empData.IDNumber.Length - 6);
                var Birthday = empData.Birthday.Value.ToString("MMdd");
                var userPW = IDNumber + Birthday;
                byte[] data = await ExportPDF(ExcutionDate, languageCookie);
                using (MemoryStream input = new MemoryStream(data))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        //加密
                        PdfReader reader = new PdfReader(input);
                        PdfEncryptor.Encrypt(reader, output, true, userPW, "owner", PdfWriter.AllowPrinting);
                        data = output.ToArray();
                        WriteLog("Export-ChangeNote:" + ExcutionDate + "_" + Illustate);
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);

                        string fileName = empData.EmpID +"_"+empData.EmpName+"_"+ Illustate.Replace(@"/", "") + "_職位薪給調整通知.pdf";
                        return File(data, "application/pdf", fileName);

                    }
                }
            }
          
            viewModel = await GetDefaultData();

            ViewBag.LanguageCookie = languageCookie;

            return View(viewModel.ToPagedList(currentPage, currentPageSize));
        }

        private async Task<List<SalaryChangeNoteViewModel>> GetDefaultData()
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
            List<SalaryChangeNoteViewModel> viewModel = new List<SalaryChangeNoteViewModel>();

            var ds = await HRMApiAdapter.GetEmpSalaryChangeNoteAllStatus(CurrentUser.CompanyCode, CurrentUser.EmployeeNO);

            foreach (var item in ds)
            {
                viewModel.Add(new SalaryChangeNoteViewModel
                {
                    ExcutionDate = item.ExcutionDate,
                    Illustate = item.Illustate
                });

            }

            return viewModel;
        }

        [HttpPost]
        public async Task<byte[]> ExportPDF(string ChangeDate, string languageCookie = "zh-TW")
        {
            var changeNoteData = await GetSalaryChangeNoteInfo(ChangeDate);
            
            string htmlText ="";
            htmlText = RenderViewAsString("_EmpChangeNote", changeNoteData);

            //20180821 Daniel 增加Logo圖片
            string base64ImageData = Convert.ToBase64String(System.IO.File.ReadAllBytes(HttpContext.Server.MapPath("~/Images/DDMC_152_152.png")));
            string base64ImageChapterData = Convert.ToBase64String(System.IO.File.ReadAllBytes(HttpContext.Server.MapPath("~/Images/DDMC_Chapter.png")));
            string base64ImageOriginalData = Convert.ToBase64String(System.IO.File.ReadAllBytes(HttpContext.Server.MapPath("~/Images/Original.png")));
            string base64ImageConfidentialData = Convert.ToBase64String(System.IO.File.ReadAllBytes(HttpContext.Server.MapPath("~/Images/Confidential.png")));
            htmlText = htmlText.Replace("<a />", "<img src=\"data:image/gif;base64," + base64ImageData + "\" style=\"width:60px;height:60px;\" />")
                               .Replace("<b />", "<img src=\"data:image/gif;base64," + base64ImageChapterData + "\" style=\"width:160px;height:160px;\" />")
                               .Replace("<c />", "<img src=\"data:image/gif;base64," + base64ImageOriginalData + "\" style=\"width:100px;height:30px;\" />")
                               .Replace("<d />", "<img src=\"data:image/gif;base64," + base64ImageConfidentialData + "\" style=\"width:100px;height:30px;\" />");


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

                //20180822 Daniel 增加內嵌Logo圖案到薪資領條
                var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
                tagProcessors.RemoveProcessor(HTML.Tag.IMG); // remove the default processor
                tagProcessors.AddProcessor(HTML.Tag.IMG, new Base64ImageTagProcessor()); // use our new processor

                CssFilesImpl cssFiles = new CssFilesImpl();
                cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
                var cssResolver = new StyleAttrCSSResolver(cssFiles);
                var charset = Encoding.UTF8;

                string fontPath1 = "c:\\windows\\fonts\\times.ttf"; //Times New Roman
                string fontPath2 = "c:\\windows\\fonts\\kaiu.ttf"; //標楷體
                var fontProvider = new XMLWorkerFontProvider(XMLWorkerFontProvider.DONTLOOKFORFONTS);

                //var fontProvider = new UnicodeFontFactory();

                fontProvider.Register(fontPath1);
                fontProvider.Register(fontPath2, "DFKai-SB");

                //var hpc = new HtmlPipelineContext(new CssAppliersImpl(new UnicodeFontFactory()));
                var hpc = new HtmlPipelineContext(new CssAppliersImpl(fontProvider));

                hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(tagProcessors); // inject the tagProcessors
                //hpc.SetTagFactory(tagProcessors); // inject the tagProcessors
                var htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer));
                var pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
                var worker = new XMLWorker(pipeline, true);
                var xmlParser = new XMLParser(true, worker, charset);

                xmlParser.Parse(new StringReader(htmlText));
                //使用XMLWorkerHelper把Html parse到PDF檔裡
                //XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8, new HRPortal.Utility.UnicodeFontFactory());
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

        #region 讀取html

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


        private async Task<SalaryChangeNoteInfo> GetSalaryChangeNoteInfo(string ChangeDate)
        {

            SalaryChangeNoteInfo _result = await HRMApiAdapter.GetSalaryChangeNoteInfo(CurrentUser.CompanyCode, CurrentUser.EmployeeNO, ChangeDate);

            return _result;
        }

        #endregion

        #region 確認密碼

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
        #endregion

        //20180822 Daniel 因新增內嵌Logo圖案到PDF，需額外增加Img標籤處理類別
        public class Base64ImageTagProcessor : iTextSharp.tool.xml.html.Image
        {
            public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
            {
                IDictionary<string, string> attributes = tag.Attributes;
                string src;
                if (!attributes.TryGetValue(HTML.Attribute.SRC, out src))
                    return new List<IElement>(1);

                if (string.IsNullOrEmpty(src))
                    return new List<IElement>(1);

                if (src.StartsWith("data:image/", StringComparison.InvariantCultureIgnoreCase))
                {
                    // data:[<MIME-type>][;charset=<encoding>][;base64],<data>
                    var base64Data = src.Substring(src.IndexOf(",") + 1);
                    var imagedata = Convert.FromBase64String(base64Data);
                    var image = iTextSharp.text.Image.GetInstance(imagedata);

                    var list = new List<IElement>();
                    var htmlPipelineContext = GetHtmlPipelineContext(ctx);
                    list.Add(GetCssAppliers().Apply(new Chunk((iTextSharp.text.Image)GetCssAppliers().Apply(image, tag, htmlPipelineContext), 0, 0, true), tag, htmlPipelineContext));
                    return list;
                }
                else
                {
                    return base.End(ctx, tag, currentContent);
                }
            }
        }
    }
}
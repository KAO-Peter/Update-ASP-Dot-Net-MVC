using HRPortal.Services;
using HRPortal.Services.Models;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HRPortal.Services.Models.BambooHR;

namespace HRPortal.Mvc.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected HRPortal_Services Services;
        public string m_lang = "zh-tw";
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        public int currentPageSize = 0;

        //20170320增加 by Daniel，切版檢查路徑用
        private string absoluteUri = "";

        //原架構
        //protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        //{
        //    string cultureName = null;

        //    // Attempt to read the culture cookie from Request
        //    HttpCookie cultureCookie = Request.Cookies["_culture"];
        //    if (cultureCookie != null)
        //        cultureName = cultureCookie.Value;
        //    else
        //        cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
        //                Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
        //                null;
        //    // Validate culture name
        //    cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

        //    // Modify current thread's cultures            
        //    Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
        //    Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        //    return base.BeginExecuteCore(callback, state);
        //}

        private MailMessageService _mailMessageService;
        protected MailMessageService MailMessageService
        {
            get
            {
                if (_mailMessageService == null)
                {
                    _mailMessageService = Services.GetService<MailMessageService>();
                }
                return _mailMessageService;
            }
        }

        private SystemLogService _logService;

        public SystemLogService LogService
        {
            get
            {
                if (_logService == null)
                {
                    _logService = Services.GetService<SystemLogService>();
                }
                return _logService;
            }
        }
        public void SendMail(string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
            MailMessageService.CreateMail(_fromMail, rcpt, cc, bcc, subject, body, isHtml);
        }

        public void SetBaseUserInfo()
        {
            ViewBag.EmpName = this.CurrentUser.Name;
            ViewBag.EmpEngName = this.CurrentUser.Employee.EmployeeEnglishName; //20190517 Daniel 增加英文姓名
            ViewBag.EmpID = this.CurrentUser.Employee.EmployeeNO;
            ViewBag.DeptName = this.CurrentUser.Employee.Department.DepartmentName;
            ViewBag.SignDeptName = this.CurrentUser.Employee.SignDepartment.DepartmentName;
            ViewBag.CompanyName = this.CurrentUser.CompanyName;
            ViewBag.DeptEngName = this.CurrentUser.Employee.Department.DepartmentEnglishName;
            ViewBag.SignDeptEngName = this.CurrentUser.Employee.SignDepartment.DepartmentEnglishName;
        }

        //20170320增加 by Daniel，切版檢查功能允不允許進入(先暫時這樣做，切版檢查之後需要重新規劃)
        //20170329修改 by Daniel，外網跟行動限制功能的函式由LINQ Extension改回此處
        public bool FilterAllowedList(List<string> source, string absoluteUri)
        {
            string absoluteUri2, findStr, headerChar, trailerChar;
            int index1 = -1;
            bool allowedFlag = false;
            foreach (string allowedItem in source)
            {
                absoluteUri2 = absoluteUri.Trim().ToLower();
                findStr = allowedItem.Trim();
                index1 = absoluteUri2.LastIndexOf(findStr.ToLower());

                if (index1 >= 0) //第一個字元符合也不行
                {
                    if (index1 + findStr.Length == absoluteUri2.Length) //搜尋字串在網址最尾端
                    {
                        allowedFlag = true;
                    }
                    else
                    {
                        headerChar = absoluteUri2.Substring(index1 - 1, 1); //找出左側是否為/字元
                        trailerChar = absoluteUri2.Substring(index1 + findStr.Length, 1); //找出搜尋字串右側的字
                        if (headerChar == "/" && (trailerChar == "/" || trailerChar == "?"))
                        {
                            allowedFlag = true;
                        }
                    }
                }

                if (allowedFlag)
                {
                    break;
                }

            }

            return allowedFlag;
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //預設
            //string _resourceName = "zh-tw";
            //HttpCookie _resourceCookie = Request.Cookies["lang"];

            //設定閒置自動登出時間  Irving 2017/0316
            if (Session["Timeout"] == null || Session["Timeout"] != "Y")
            {
                CurrentUser.ClearMenus();
                AuthenticationManager.SignOut();
                //20170831 Daniel 調整登出後轉導的登入網址
                //filterContext.Result = Redirect("~/Login");
                filterContext.Result = Redirect("~/ADLogin"); //20171102 Daniel
                return;

            }

            //20170320增加 by Daniel，切版檢查功能，使用不允許的功能就直接登出
            //20170323修改 by Daniel，改成只檢查外網Outside與行動Mobile
            //20170328修改 by Daniel，切版判斷用的Session物件改為使用Global變數
            //20170329修改 by Daniel，外網跟行動限制功能的函式由LINQ Extension改回此處，並修正判定錯誤的問題
            string siteFunctionRange = SiteFunctionInfo.SiteFunctionRange;
            bool functionCheckPass = false;
            if (siteFunctionRange == "outside" || siteFunctionRange == "mobile" || siteFunctionRange == "signonly") //目前外網跟行動要限制功能 //20171221 Daniel 增加直接登入網站也要限制
      
            {
                List<string> allowedList = (List<string>)SiteFunctionInfo.AllowedList;
                if (allowedList != null && allowedList.Count > 0)
                {
                    //Debug.WriteLine(allowedList.FilterAllowedList(absoluteUri) ? "Allowed!" : "Forbidden!");
                    if (FilterAllowedList(allowedList, absoluteUri))
                    {
                        functionCheckPass = true;
                    }
                }
            }
            else //若為all就開放所有功能，不做檢查
            {
                functionCheckPass = true;
            }

            if (!functionCheckPass)
            {
                CurrentUser.ClearMenus();
                AuthenticationManager.SignOut();
                //filterContext.Result = Redirect("~/Login");
                filterContext.Result = Redirect("~/ADLogin"); //20171102 Daniel
                return;
            }
            //if (_resourceCookie != null && _resourceCookie.Value != "undefined")
            //{
            //    _resourceName = _resourceCookie.Value;
            //}
            //m_lang = _resourceName;
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_resourceName);
            //Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            
            //20171102 Daniel admin權限才走原來檢查密碼是否到期的路徑
            if (CurrentUser.IsAdmin && CurrentUser != null && CurrentUser.Employee != null &&
                CurrentUser.Employee.PasswordExpiredDate < DateTime.Now)
            {
                CurrentUser.ClearMenus();
                AuthenticationManager.SignOut();
                //20170831 Daniel 調整登出後轉導的登入網址
                //filterContext.Result = Redirect("~/Login");
                filterContext.Result = Redirect("~/ADLogin"); //20171102 Daniel
                return;
            }

            currentPageSize = int.Parse(Services.GetService<SystemSettingService>().GetSettingValue("CurrentPageSize"));
            base.OnActionExecuting(filterContext);
        }

        //protected Sys GetSys()
        //{
        //    if (_sys != null)
        //        return _sys;
        //    _sys = Services.GetService<SystemService>().Get(this.CurrentUser.SysId);
        //    return _sys;
        //}

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Services = new HRPortal_Services();

            //20170320增加 by Daniel，切版檢查路徑用
            absoluteUri = requestContext.HttpContext.Request.Url.AbsoluteUri;

            //Services.SetSysId(this.CurrentUser.SysId);
        }

        protected ActionResult JsonContent(object data, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = nullValueHandling,
                Converters = new[] { new IsoDateTimeConverter { DateTimeFormat = "yyyy/MM/dd HH:mm:ss" } }
            };
            return Content(JsonConvert.SerializeObject(data, serializerSettings), "application/json");
        }

        public virtual UserClaimsPrincipal CurrentUser
        {
            get
            {
                return new UserClaimsPrincipal(this.User as ClaimsPrincipal, Services);
            }
        }

        public LogInfo LogInfo
        {
            get 
            {
                return this.GetLogInfo();
            }
        }


        protected IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public void WriteLog(string remark = null)
        {
            string _ip = this.Request.UserHostAddress;
            string _actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string _controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            LogService.WriteLog(_ip, _controllerName, _actionName, remark);
        }

        //20201119 Daniel 配合BambooHR整合，回傳APILog物件
        public LogInfo GetLogInfo()
        {
            LogInfo info = new LogInfo()
            {
                UserID = this.CurrentUser.EmployeeID,
                UserIP = this.Request.UserHostAddress,
                Controller = this.ControllerContext.RouteData.Values["controller"].ToString(),
                Action = this.ControllerContext.RouteData.Values["action"].ToString()

            };
            return info;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Services != null)
                {
                    this.Services.Dispose();
                    this.Services = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
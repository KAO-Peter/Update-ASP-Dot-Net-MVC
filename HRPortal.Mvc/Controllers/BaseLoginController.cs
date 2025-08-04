using System;
using System.Security.Claims;
using System.Collections.Generic;
using HRPortal.Services;
using HRPortal.Services.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using HRPortal.DBEntities;
using System.Configuration;


namespace HRPortal.Mvc.Controllers
{
    [AllowAnonymous]
    public abstract class BaseLoginController : Controller
    {
        protected HRPortal_Services Services;

        private SystemLogService _logService;

        protected SystemLogService LogService
        {
            get
            {
                if(_logService == null)
                {
                    _logService = Services.GetService<SystemLogService>();
                }
                return _logService;
            }
        }

        private MailMessageService _mailMessageService;
        protected MailMessageService MailMessageService
        {
            get
            {
                if(_mailMessageService == null)
                {
                    _mailMessageService = Services.GetService<MailMessageService>();
                }
                return _mailMessageService;
            }
        }


        public BaseLoginController()
            : this(new ApplicationUserManager())
        {
        }


        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Services = new HRPortal_Services();
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public virtual UserClaimsPrincipal CurrentUser
        {
            get
            {
                return new UserClaimsPrincipal(this.User as ClaimsPrincipal, Services);
            }
        }

        public BaseLoginController(ApplicationUserManager userManager)
        {
            userManager.Services = Services;
            UserManager = userManager;
        }

        protected List<SelectListItem> GetCompanys()
        {
            return this.Services.GetService<CompanyService>().GetValueText().Select(x => new SelectListItem()
            {
                Text = x.t,
                Value = x.v
            }).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
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

        protected async Task<ActionResult> GoHome(ApplicationUser user = null)
        {
            //if (user == null)
            //    user = await UserManager.FindByIdAsync(Guid.Parse(User.Identity.GetUserId()));
            //string defaultUrl = this.Services.GetService<MenuService>().GetDefaultUrl(user.Employee.Role);
            //return Redirect(defaultUrl);
            return Redirect("~/Home");
        }

        //protected Sys GetSys(string alias)
        //{
        //    Sys sys = this.Services.GetService<SystemService>().FirstOrDefault(x => x.alias == alias);
        //    if (sys != null)
        //    {
        //        this.Services.SetSysId(sys.id);
        //    }
        //    return sys;
        //}

        public void SendMail(string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            string _fromMail = this.Services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
            MailMessageService.CreateMail(_fromMail, rcpt, cc, bcc, subject, body, isHtml);
        }

        #region Helpers
        // 新增外部登入時用來當做 XSRF 保護
        private const string XsrfKey = "XsrfId";

        protected IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        protected async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            //await UserManager.SignInDeviceAsync(user, Request.Browser.Type);
            ///Open Question- Hear it create claimIdentity. But we nothing add as such Claims but just User object.
            //public virtual Task<ClaimsIdentity> CreateIdentityAsync(TUser user, string authenticationType);

            //var identity = await UserManager1.CreateAsync(user);//, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }


        [Authorize]
        // GET: /Account/Logout
        public ActionResult Logout()
        {
            string url = "/";
            string RedirectURL = "";

            //20180321 Daniel 如果是簽核專用網站要登出，需登出回原網站

            //20180321 Daniel 加上Application路徑(如果網站在網路底下還有切目錄的話，原來的方式會連到最外層)
            //string _webURL = "https://" + HttpContext.Current.Request.Url.Host;
            //再調整手機外網跟簽核專用站都不需要Port跟ApplicationPath
            string siteType = SiteFunctionInfo.SiteType.ToLower();
            string scheme = System.Web.HttpContext.Current.Request.Url.Scheme;
            int port = System.Web.HttpContext.Current.Request.Url.Port;
            //預設https不需要Port，http 80 port也不需要加Port，只有http非80才加上Port
            //再調整手機外網跟簽核專用站都不需要Port跟ApplicationPath
            string strPort = "";
            string applicationPath = "";

            if (siteType == "mobile" || siteType == "outside" || siteType == "signonly" || scheme.ToLower() == "https")
            {
                strPort = "";
                applicationPath = "";
                scheme = "https"; //全部更正回https
            }
            else
            {
                strPort = ":" + port.ToString();
                applicationPath = System.Web.HttpContext.Current.Request.ApplicationPath;
            }
            //string strPort = ((scheme.ToLower() == "https" || (scheme.ToLower() != "https" && port == 80)) ? "" : (":" + port.ToString());

            //https就不用加上ApplicationPath了(一般走https會是網域就是直接進Portal，不會再設定子目錄)
            //string applicationPath = (scheme.ToLower() == "https") ? "" : HttpContext.Current.Request.ApplicationPath;

            RedirectURL = scheme + "://" + System.Web.HttpContext.Current.Request.Url.Host + strPort + applicationPath;
           
            /*
            string signOnlySitePath = ConfigurationManager.AppSettings["SignOnlySitePath"];
            string urlNow = System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
            int indexFind = urlNow.IndexOf(signOnlySitePath);
            if (!string.IsNullOrWhiteSpace(signOnlySitePath) && indexFind >= 0) //有設定簽核專用網站虛擬目錄，需比對現在網址內是否包含該虛擬目錄，有的話就移除到虛擬目錄之前
            {
                RedirectURL = urlNow.Substring(0, indexFind);
            }
            else
            {
                RedirectURL = "~" + url;
            }
            */

            try
            {
                //ApplicationUser user = await UserManager.FindByIdAsync(Guid.Parse(User.Identity.GetUserId()));
                //if (user.employee != null && user.employee.sys != null)
                //    url = "/" + user.employee.sys.alias + url;
                //await UserManager.SignOutDeviceAsync(user);
                CurrentUser.ClearMenus();

                //20180314 Daniel補上登出時將Session全部移除
                Session.Abandon();

                AuthenticationManager.SignOut();
            }
            catch { }

            //return Redirect("~" + url);
            return Redirect(RedirectURL);
            
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}

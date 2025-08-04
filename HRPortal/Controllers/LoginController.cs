using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using HRPortal.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Models;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using HRPortal.Services.Models;
using HRProtal.Core;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using YoungCloud.Security.Cryptography;

namespace HRPortal.Controllers
{
    [AllowAnonymous]
    public class LoginController : BaseLoginController
    {
        public LoginController()
            : this(new ApplicationUserManager())
        {
        }

        public LoginController(ApplicationUserManager userManager)
            : base(userManager)
        {
        }

        public async Task<ActionResult> Index()
        {
            //20170331 Start 修改 by Daniel，產生金鑰改到共用函式執行
            GenerateEncryptKey();
            //原先的先Mark掉
            /*
            RSAKeyPair _rsaKey = RSAHelper.GenerateRSAKey();
            Session["PrivateKey"] = _rsaKey.PrivateKey;
            Session["PublicKey"] = RSAKeyFormatConverter.ConvertXMLPublicKeyToPEM(_rsaKey.PublicKey);

            Response.Cookies.Add(new HttpCookie("encrypt_key", (string)Session["PublicKey"]));
            */
            //20170331 End

            if (this.Request.IsAuthenticated)
            {
                if (CurrentUser.Employee.PasswordExpiredDate < DateTime.Now)
                {
                    return Logout();
                }
                else
                {
                    return await GoHome();
                }
            }

            //20170322增加 by Daniel，偵測行動裝置進行轉導，轉導位址於Web.Config內可做設定
            bool isMobileDevice = Request.Browser.IsMobileDevice;
            
            //20170328增加 by Daniel，切版判斷用的Session物件改為使用Global變數
            string siteType = SiteFunctionInfo.SiteType;
           
            if (siteType != "mobile" && isMobileDevice) //非手機站台，偵測到行動裝置需做轉頁動作，要注意，沒設定好可能會變成無窮轉導
            {
                string mobileSitePath = SiteFunctionInfo.MobileSitePath;
                if (mobileSitePath.Trim().Length > 0)
                {
                    return Redirect(mobileSitePath);
                }
                //20170327修改 by Daniel，因應內網行動要開放全功能，改為行動裝置沒設定路徑就不轉導
                else //沒設定手機站台路徑時，Throw錯誤訊息
                {
                    //throw new Exception("行動裝置站台尚未設定！");
                }
            }
            

            var Companydata = this.Services.GetService<CompanyService>().GetValueText().FirstOrDefault();
            ViewBag.OriginalCompanyName = Companydata.originalCompanyName;
            ViewBag.ContactPrincipal = Companydata.c;          
            ViewBag.CompanyList = this.GetCompanys();
            string linkPath = Request.PhysicalApplicationPath;
            //判斷是否有底圖
            ViewBag.customerBackground = System.IO.File.Exists(linkPath + "Content\\CP0L0\\bg-01.png");
            //判斷是否有Logo
            ViewBag.customerLogoPicture = System.IO.File.Exists(linkPath + "Content\\CP0L0\\header-logo.png");
            //判斷是否有右側圖
            ViewBag.customerRightPicture = System.IO.File.Exists(linkPath + "Content\\CP0L0\\index-pic.png");
            

            return View();
        }
        [HttpPost]
        public ActionResult CompanyData(Guid CompanyID)
        {
            Company model = new Company();
            //取得公司資料
            model = this.Services.GetService<CompanyService>().GetCompanyById(CompanyID);
            var CompanyContactPrincipal = model.ContactPrincipal;
            var result = new { ContactPrincipal = model.ContactPrincipal, CompaneName = model.CompanyName };
            return Json(result);

        }

        //20170331 Start 增加 by Daniel，處理閒置太久Session Timeout的問題，改為每次Login前先檢查是否有Key，沒有就重新產生
        
        private string GenerateEncryptKey() //產生加密金鑰共用函式
        {
            RSAKeyPair _rsaKey = RSAHelper.GenerateRSAKey();
            Session["PrivateKey"] = _rsaKey.PrivateKey;
            string publicKey=RSAKeyFormatConverter.ConvertXMLPublicKeyToPEM(_rsaKey.PublicKey);
            Session["PublicKey"] = publicKey;
            Response.Cookies.Add(new HttpCookie("encrypt_key", publicKey));
            return publicKey;
        }
        //
        [HttpPost]
        public ActionResult CheckEncryptKey()
        {
            
            if (Session["PrivateKey"] == null)  //沒有Session，可能是因為閒置太久
            { 
                //產生新的Encryption Key
                string publicKey=GenerateEncryptKey();
                
                return Json(new {SessionExpired=true,NewKey=publicKey});
            }
            return Json(new { SessionExpired = false, NewKey = "" });
        }

        //20170331 End


        // POST: /Login/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LoginViewModel model, string returnUrl)
        {
            try
            {
                model.Password = RSAHelper.Decrypt(model.Password, (string)Session["PrivateKey"]);

                string _actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string _controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                if (!ModelState.IsValid)
                {
                    RedirectToAction("Index");
                    return Json(new LoginResult() { Status = LoginStatus.Failed });
                }


                ApplicationUser _user = await UserManager.FindAsync(model);

                if (_user != null)
                {
                    //20170512 Start Daniel，密碼驗證通過與否改為由ApplicationUser物件內的PasswordPassed判定
                    //當密碼驗證未通過
                    if (_user.PasswordPassed==false)
                    {
                        //檢查是否帳戶已經因密碼錯誤太多次被鎖定
                        if (_user.Employee.PasswordLockStatus == true)
                        {
                            return Json(new LoginResult()
                            {
                                Status = LoginStatus.Failed,
                                //20170519 Daniel，修改因密碼錯誤鎖定時的說明文字
                                //20170522 Daniel，調整說明文字
                                Message = "目前此帳戶已被鎖定，請逕洽HR"
                                //Message = "<span class='passwordLockNotice'>提醒您：密碼連續輸入錯誤3次，將被鎖定，請逕洽HR。</span>"
                            });
                        }

                        //密碼錯誤但未被鎖定，顯示原來的錯誤訊息
                        return Json(new LoginResult()
                        {
                            Status = LoginStatus.Failed,
                            Message = "所提供的工號或密碼不正確，請再輸入一次。"
                        });

                    
                    }
                    //20170512 End
                    var EmpLeaveDate = _user.Employee.LeaveDate;
                    if (EmpLeaveDate == null)
                    {
                        EmpLeaveDate = null;
                    }
                    else
                    {

                        EmpLeaveDate = _user.Employee.LeaveDate.Value.AddDays(1).AddSeconds(-1);
                    }
                    if (EmpLeaveDate > DateTime.Now || _user.Employee.LeaveDate == null || _user.PasswordPassed)
                    {
                        await SignInAsync(_user, false);

                        if (_user.Employee.PasswordExpiredDate <= DateTime.Now)
                        {   
                            LogService.WriteLog(_user.Employee.ID, this.Request.UserHostAddress, _controllerName, _actionName, "Success (password expired)");
                            return Json(new LoginResult() { Status = LoginStatus.Expired });
                        }
                        else
                        {
                            LogService.WriteLog(_user.Employee.ID, this.Request.UserHostAddress, _controllerName, _actionName, "Success");
                            Session["Timeout"] = "Y";//設定登出時間
                            return Json(new LoginResult()
                            {
                                Status = LoginStatus.Success,
                            });
                        }
                    }
                    else {

                        return Json(new LoginResult()
                        {
                            Status = LoginStatus.Failed,
                            Message = "此員工已離職!"
                        });
                    
                    }
                }
                else
                {
                    _user = UserManager.FindByName(model.Account, model.Company);
                    if (_user != null)
                    {
                        LogService.WriteLog(_user.Employee.ID, this.Request.UserHostAddress, _controllerName, _actionName, "Password Error");
                    }

                    return Json(new LoginResult()
                    {
                        Status = LoginStatus.Failed,
                        Message = "所提供的工號或密碼不正確，請再輸入一次。"
                    });

                    //ModelState.AddModelError("", "所提供的工號或密碼不正確，請再輸入一次。");
                    //ViewBag.CompanyList = this.GetCompanys();
                    //return View("~/Views/Login/Index.cshtml", model);
                }
            }
            catch
            {
                Logout();
                throw;
            }
        }

        #region Helper
        // 新增外部登入時用來當做 XSRF 保護
        private const string XsrfKey = "XsrfId";
        /*
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        */
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
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
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        [Authorize]
        public ActionResult ChangePassword()
        {
            return PartialView("_ChangePassword");
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            UserManager.Services = Services;
            IdentityResult _result = await UserManager.ChangePasswordAsync(CurrentUser.Employee,
                RSAHelper.Decrypt(model.CurrentPassword, (string)Session["PrivateKey"]),
                RSAHelper.Decrypt(model.NewPassword, (string)Session["PrivateKey"]));

            if(_result == IdentityResult.Success)
            {
                string _actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string _controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                LogService.WriteLog(CurrentUser.Employee.ID, this.Request.UserHostAddress, _controllerName, _actionName);
            }

            return Json(_result == IdentityResult.Success, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> CheckPassword(string EncryptedPassword)
        {
            ApplicationUser user = await UserManager.FindAsync(CurrentUser.Employee, 
                RSAHelper.Decrypt(EncryptedPassword, (string)Session["PrivateKey"]));
            if(user != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            ViewBag.CompanyList = this.GetCompanys();
            return PartialView("_ResetPassword");
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordRequestModal model)
        {
            if (ModelState.IsValid)
            {
                UserManager.Services = Services;
                ApplicationUser _user = UserManager.FindByName(model.Account, model.Company);
                //20170512 Start Daniel， 增加密碼鎖定狀態檢查
                if (_user != null)
                {
                    if (_user.Employee.PasswordLockStatus==true)
                    {
                        //20170519 Daniel，修改因密碼錯誤鎖定時的說明文字
                        //return Json(new { success = false, message = "目前此帳戶已被鎖定，請洽系統管理員" });
                        return Json(new { success = false, message = "目前此帳戶已被鎖定，請逕洽HR。" });
                    }
                    //if (_user != null && _user.Employee.Email == model.Email)
                    else if (_user.Employee.Email == model.Email)
                    {
                        Employee _employee = _user.Employee;
                        string _newPassword = UserManager.ResetPassword(_employee);
                        //申請人姓名
                        var CUserName = Services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == model.Account).Select(x => x.EmployeeName).FirstOrDefault();
                        SendMail(new string[] { _employee.Email }, null, null,
                            CUserName + Resource.ResetPasswordMailSubject,
                            Resource.ResetPasswordMailBody.Replace("[PASSWORD]", _newPassword), false);

                        return Json(new { success = true });
                    }
                }
                //20170512 End
                return Json(new { success = false, message = "帳號不存在或Email錯誤" });
            }

            //ViewBag.CompanyList = this.GetCompanys();
            return Json(new { success = false, message = "帳號不存在或Email錯誤" });
        }
    }
}

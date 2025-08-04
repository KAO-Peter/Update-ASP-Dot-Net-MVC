using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using HRPortal.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Mvc.Results;
using HRPortal.Services;
using System.Web.Script.Serialization;
using HRPortal.eipWebReference;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace HRPortal.Controllers
{
    public class SSOLoginController : BaseLoginController
    {

        public SSOLoginController()
            : this(new ApplicationUserManager())
        {
        }

        public SSOLoginController(ApplicationUserManager userManager)
            : base(userManager)
        {
        }


        //
        // GET: /SSOLogin/
        [HttpGet]
        public async Task<ActionResult> Index(string EIP)
        {
            LoginResult retLoginResult = new LoginResult();
            retLoginResult.Status = LoginStatus.Failed;

            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            if (!String.IsNullOrEmpty(EIP))
            {
                var Companydata = this.Services.GetService<CompanyService>().GetValueText().FirstOrDefault();

                EipSSO eipSSO = new EipSSO();

                if (SSOCheck(EIP, ref eipSSO))
                {

                    string prefixAccount = "ddmc";
                    string account = eipSSO.AccountID;
                    if (eipSSO.AccountID.Substring(0, prefixAccount.Length).ToUpper() == prefixAccount.ToUpper())
                    {
                        account = eipSSO.AccountID.Substring(prefixAccount.Length);
                    }

                    ApplicationUser _user = await UserManager.FindAsync(account, Companydata.id, "");

                    retLoginResult = await PortalLoginProcess(_user);
                    if (retLoginResult.Status == LoginStatus.Success)
                    {
                        retLoginResult.Message = "SSO成功";
                        LogService.WriteLog(_user.Employee.ID, this.Request.UserHostAddress, controllerName, actionName,
                            string.Format("{0}{1}", retLoginResult.Status, retLoginResult.Message));

                        return RedirectToAction("Index", "Home");
                    }

                }
                else
                {
                    retLoginResult.Message = "SSO驗證失敗";
                }

            }
            else
            {
                retLoginResult.Message = "非許可方式進入，請重新登入。";
            }


            if (retLoginResult.Status != LoginStatus.Success)
            {
                //寫log一定要有Employee的GUID，先註解
                //retLoginResult.Message = "SSO成功";
                //LogService.WriteLog(this.Request.UserHostAddress, controllerName, actionName,
                //    string.Format("{0}{1}", retLoginResult.Status, retLoginResult.Message));
            } 


            ViewBag.Message = retLoginResult.Message;
            return View();

        }

        /// <summary>
        /// ePortal待簽核轉入，將傳入的參數解密後，驗證是否在有效期限內。
        /// 驗證成功後會轉至待簽核列表功能頁。
        /// </summary>
        /// <param name="T"></param>
        /// <param name="ES"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ToSign(string T, string ES)
        {
            LoginResult retLoginResult = new LoginResult();
            retLoginResult.Status = LoginStatus.Failed;
            retLoginResult.Message = "驗證失敗，非預期方式登入";


            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();


            if (!String.IsNullOrEmpty(T) && !String.IsNullOrEmpty(ES))
            {
                try
                {
                    string aESKeyString = System.Configuration.ConfigurationManager.AppSettings["URLEncryptKey"];    
                    string validityHour = System.Configuration.ConfigurationManager.AppSettings["URLValidityHour"]; 
                    string uT = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(T));
                    string uES = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(ES));
                    System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
                    DateTime userDate;
                    userDate = DateTime.ParseExact(uT, "yyyyMMddHHmm", provider);

                    if (userDate > DateTime.Now.AddHours(-Convert.ToInt16(validityHour)))
                    {
                        #region 連結在有效期限內
                        AESKey aesKey = new AESKey();
                        if (!string.IsNullOrWhiteSpace(aESKeyString))
                        {
                            string[] keySection = aESKeyString.Split('-');
                            if (keySection.Length == 2)
                            {
                                aesKey.Key = CryptoService.HexToByteArray(keySection[0]); //AES Key
                                aesKey.IV = CryptoService.HexToByteArray(keySection[1]); //AES IV
                                string targetEncrypted = uES;
                                string targetDecrypted = CryptoService.DecryptString(aesKey, targetEncrypted);
                                string[] ePortalRet = targetDecrypted.Split('-');
                                int company_ID;

                                if (int.TryParse(ePortalRet[2], out company_ID))
                                {
                                    var Companydata = this.Services.GetService<CompanyService>().GetCompanyLists().
                                        Where(x => x.Company_ID == company_ID).FirstOrDefault();

                                    string employeeNo = ePortalRet[1];
                                    ApplicationUser _user = await UserManager.FindAsync(employeeNo, Companydata.ID, "");

                                    retLoginResult = await PortalLoginProcess(_user);
                                    if (retLoginResult.Status == LoginStatus.Success)
                                    {
                                        retLoginResult.Message = "ePortal簽核登入成功";
                                        LogService.WriteLog(_user.Employee.ID, this.Request.UserHostAddress, controllerName, actionName,
                                            string.Format("{0}{1}", retLoginResult.Status, retLoginResult.Message));

                                        return RedirectToAction("Index", "SignForms", new { area = "ToDo" });
                                    }

                                }

                            }
                            else
                            {
                                retLoginResult.Message = "驗證失敗，非預期方式登入";
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        retLoginResult.Message = "連結已過期";
                    }

                    

                }
                catch (Exception ex)
                {
                    retLoginResult.Status = LoginStatus.Failed;
                    retLoginResult.Message = "驗證失敗，非預期方式登入";
                }

            }


            ViewBag.Message = retLoginResult.Message;
            return View();
        }




        /// <summary>
        /// 呼叫EIP web service驗證及檢查是否在有效時間內。
        /// </summary>
        /// <param name="pToEncrypt"></param>
        /// <param name="pEIP"></param>
        /// <returns></returns>
        private bool SSOCheck(string pToEncrypt ,ref EipSSO pEIP)
        {
            bool retVal = false;
            int EIPTimeOut = 30;

            pEIP = Decrypt_UserInfo(pToEncrypt);

            Int64 intToday = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmm"));
            Int64 intUserDate = pEIP.SysTime != null ? Convert.ToInt64(pEIP.SysTime) : 0;
            
            if (intUserDate >= (intToday - EIPTimeOut) && intUserDate <= (intToday + EIPTimeOut)
                && pEIP.SysFlag == "eipportal")
            {
                retVal = true;
            }

            return retVal;
        }

        private async Task<LoginResult> PortalLoginProcess(ApplicationUser pUser)
        {
            LoginResult retResult = new LoginResult();


            if (pUser != null)
            {
                //檢查是否帳戶是否被鎖定
                if (pUser.Employee.PasswordLockStatus == true)
                {
                    retResult.Status = LoginStatus.Failed;
                    retResult.Message = "目前此帳戶已被鎖定，請逕洽HR";
                }
                else
                {
                    var EmpLeaveDate = pUser.Employee.LeaveDate;
                    //離職當天還能登入處理。 
                    if (EmpLeaveDate != null) EmpLeaveDate = pUser.Employee.LeaveDate.Value.AddDays(1).AddSeconds(-1);

                    if (EmpLeaveDate > DateTime.Now || pUser.Employee.LeaveDate == null)
                    {
                        await SignInAsync(pUser, false);

                        if (pUser.IsAdmin && pUser.Employee.PasswordExpiredDate <= DateTime.Now) //20171102 Daniel Admin權限才走原來帳號密碼流程(包括密碼到期檢查)
                        {

                            retResult.Status = LoginStatus.Expired;
                            retResult.Message = "密碼到期";
                        }
                        else
                        {
                            Session["Timeout"] = "Y";//設定登出時間
                            retResult.Status = LoginStatus.Success;
                        }

                    }
                    else
                    {
                        retResult.Status = LoginStatus.Failed;
                        retResult.Message = "此員工已離職!";
                    }
                }



            }


            return retResult;
        }


        private EipSSO Decrypt_UserInfo(string EncryptJson)
        {
            EipSSO eip = new EipSSO();
            try
            {
                if (!string.IsNullOrEmpty(EncryptJson))
                {
                    //Web Service

                    Encrypt wsEncrypt = new Encrypt();
                    string DecryptJson = wsEncrypt.pDecrypt(EncryptJson);

                    //EIP SSO
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    eip = js.Deserialize<EipSSO>(DecryptJson);
                }
                return eip;
            }
            catch (Exception ex)
            {
                return eip;
            }

        }

        public class EipSSO
        {
            public string SysFlag { get; set; }

            public string SysTime { get; set; }

            public string AccountID { get; set; }

            public string CompCode { get; set; }

            public string EmpNo { get; set; }
        }
	}
}
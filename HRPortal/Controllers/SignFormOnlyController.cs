using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Mvc;
using HRPortal.Mvc.Controllers;
using HRPortal.Services;
using HRPortal.Services.Models;
using System.Configuration;
using System.Threading.Tasks;

namespace HRPortal.Controllers
{
    public class SignFormOnlyController : BaseLoginController
    {
        //
        // GET: /SignFormOnly/
        public async Task<ActionResult> Index(string ES)
        {
            string path = Request.Url.PathAndQuery;
            if (SiteFunctionInfo.SiteType != "signonly") //如果不是直接登入的網站-->轉導至直接登入網站
            {
                string signOnlySitePath = ConfigurationManager.AppSettings["SignOnlySitePath"];
                if (!string.IsNullOrWhiteSpace(signOnlySitePath))
                {
                    string directLoginController = "SignFormOnly";
                    return Redirect("~" + signOnlySitePath + @"/" + directLoginController + @"/?ES=" + ES);
                    //return Redirect("~" + signOnlySitePath + path);
                }
                else //沒設定直接登入只能簽核網站的位址，轉回登入頁面
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            else //網站是只允許簽核的沒錯，就解密Secret (Random-工號-公司ID-時間)，檢查日期區間是否失效，都沒問題就直接以該工號登入
            {
                try
                {
                    string decryptedSecret = DecryptLoginURL(ES);
                    string[] data=decryptedSecret.Split('-');
                    if (data.Length == 4 && data[3].Length == 15) //正常應該可Split成四個字串 Random-工號-公司ID-時間
                    {
                        string empID = data[1];
                        int companyID = int.Parse(data[2]);
                        DateTime linkDate = new DateTime(int.Parse(data[3].Substring(0, 4)), int.Parse(data[3].Substring(4, 2)), int.Parse(data[3].Substring(6, 2)), int.Parse(data[3].Substring(9, 2)), int.Parse(data[3].Substring(11, 2)), int.Parse(data[3].Substring(13, 2)));
                        Services = new HRPortal_Services();
                        string validDays = this.Services.GetService<SystemSettingService>().GetSettingValue("DirectLoginURLValidDays");
                        int checkDays = 0;
                        if (!string.IsNullOrWhiteSpace(validDays) && int.TryParse(validDays, out checkDays))
                        {
                            if ((DateTime.Now - linkDate.Date).TotalDays <= checkDays) //日期還在有效區間內，直接登入
                            {
                                ApplicationUser _user = UserManager.FindByEmpIDWithHRMCompanyID(empID, companyID);
                                if (_user != null)
                                {
                                    await SignInAsync(_user, false);
                                    Session["Timeout"] = "Y";
                                    return RedirectToAction("SignForms", "ToDo");
                                }
                                else
                                {
                                    ViewBag.Message = "傳入的工號無效";
                                }
                            }
                            else
                            {
                                ViewBag.Message = "郵件連結已過期，請改點選較新的連結...";
                            }
                        }
                        else
                        {
                            ViewBag.Message = "直接登入功能尚未設定完成，請洽系統管理員...";
                        }
                    }
                    else
                    {
                        ViewBag.Message = "連結無效";
                    }

                }
                catch (Exception ex)
                {
                    ViewBag.Message="連結無效";
                }
            }

            return View();
        }

        private string DecryptLoginURL(string EncString)
        {
            string result = "";
            AESKey aesKey = new AESKey();
            //取得web.config裡面設定的AES Key & IV
            string AESKeyString = ConfigurationManager.AppSettings["URLEncryptKey"];
            if (!string.IsNullOrWhiteSpace(AESKeyString))
            {
                string[] keySection = AESKeyString.Split('-');
                if (keySection.Length == 2) //有分隔號才處裡加密 (預設AES Key與IV是用分隔號隔開)
                {
                    aesKey.Key = CryptoService.HexToByteArray(keySection[0]); //AES Key
                    aesKey.IV = CryptoService.HexToByteArray(keySection[1]); //AES IV
                    result = CryptoService.DecryptString(aesKey, EncString);

                }
            }
            return result;
        }
	}
}
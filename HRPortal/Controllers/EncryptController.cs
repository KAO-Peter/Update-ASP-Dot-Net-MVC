using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRPortal.Services;
using System.Configuration;

namespace HRPortal.Controllers
{
    public class EncryptController : Controller
    {
        //
        // GET: /Encrypt/
        public ActionResult Index(string ID)
        {
            if (string.IsNullOrWhiteSpace(ID))
            {
                ID = "12345"; //測試帳號
            }
            AESKey newKey = CryptoService.GenerateAES();
            ViewBag.AESKey = CryptoService.ByteToHexBitFiddle(newKey.Key);
            ViewBag.AESIV = CryptoService.ByteToHexBitFiddle(newKey.IV);

            AESKey aesKey = new AESKey();
            string AESKeyString = ConfigurationManager.AppSettings["URLEncryptKey"];
            if (!string.IsNullOrWhiteSpace(AESKeyString))
            {
                string[] keySection = AESKeyString.Split('-');
                if (keySection.Length == 2) //有分隔號才處裡加密 (預設AES Key與IV是用分隔號隔開)
                {
                    aesKey.Key = CryptoService.HexToByteArray(keySection[0]); //AES Key
                    aesKey.IV = CryptoService.HexToByteArray(keySection[1]); //AES IV

                    //測試加密
                    ViewBag.EncryptedString = CryptoService.EncryptString(aesKey, "00000#201803131657");
                    /*
                    ViewBag.encryptedString = EncryptLoginURL("http://hrportal.ydc.com.tw", ID, 1, DateTime.Now);
                    ViewBag.DecryptedString = DecryptLoginURL(ViewBag.EncryptedSecret);
                     * */
                }


            }
            return View();
        }

        public string EncryptLoginURL(string WebSiteURL, string EmpID, int Company_ID, DateTime SendDate)
        {
            string resultURL = WebSiteURL;
            if (resultURL.Substring(resultURL.Length - 1) != @"/")
            {
                resultURL += @"/";
            }
            string directLoginController = "SignFormOnly";
            string directLoginMethod = "Index";
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
                    string randomString = CryptoService.GetRandomString(10);
                    string target = randomString + "-" + EmpID + "-" + Company_ID.ToString() + "-" + SendDate.ToString("yyyyMMdd HHmmss");
                    string targetEncrypted = CryptoService.EncryptString(aesKey, target);
                    ViewBag.EncryptedSecret = targetEncrypted;
                    resultURL += directLoginController + @"/" + directLoginMethod + @"/?ES=" + targetEncrypted;
                }



            }
            return resultURL;

        }

        public string DecryptLoginURL(string EncString)
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
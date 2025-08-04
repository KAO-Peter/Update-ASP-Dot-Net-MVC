using HRPortal.ApiAdapter;
using HRPortal.DBEntities;
using HRPortal.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoungCloud.SignFlow.Databases.UnitOfWorks;

namespace HRPortal.SendMailService
{
    public class SendMailTask : IDisposable
    {
        private HRPortal_Services _services;
        private SignFlowEntities _sdb;
        private string _fromMail;

        public SignFlowEntities SDB
        {
            get
            {
                if (_sdb == null)
                {
                    _sdb = new SignFlowEntities();
                }
                return _sdb;
            }
        }

        public SendMailTask()
        {
            _services = new HRPortal_Services();
            _fromMail = _services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
        }

        public void SendPendingMail()
        {
            int sendamt = 0;
            try
            {
                //Log.WriteLog("SendPendingMail() begin");

                DateTime dt = System.DateTime.Now;

                if (dt.Day >= 1)
                {                    
                    string FormNo = dt.Year.ToString() + dt.Month.ToString() + "-2";
                    var check = Task.Run(async () => await HRMApiAdapter.GetCheckFormNo("DDMC", FormNo));
                    Task.WaitAll(check);
                    //Log.WriteLog("判斷是否已有薪資批號：" + check.Result);
                    if (!check.Result)
                    {
                        DateTime selDate = DateTime.Parse(dt.Year.ToString() + "/" + dt.Month.ToString() + "/01");
                        string week = dt.DayOfWeek.ToString("d");
                        //判斷周六、周日、不撈取未簽核假單
                        if (dt.Hour >= 9 && dt.Hour < 10)
                        {
                            if (week != "0" && week != "6")
                            {
                                restSendMail(selDate);
                                //notSendLeave();
                            }
                        }
                    }
                }
//#if DEBUG
//                return;
//#endif
                //Log.WriteLog("開始發送eMail");

                ServicePointManager.ServerCertificateValidationCallback = CustomizeCertificateValidation;
                using (MailMessageService _mailService = _services.GetService<MailMessageService>())
                {
                    List<MailAccount> _accounts = _services.GetService<MailAccountService>().GetAll().ToList();

                    foreach (MailAccount _account in _accounts)
                    {
                        List<HRPortal.DBEntities.MailMessage> _mails = _mailService.GetPendingMessageByFromMailAddress(_account.MailAddress);
                        if (_mails.Count > 0)
                        {
                            using (SmtpClient _client = SetSmtpClient(_account))
                            {
                                foreach (HRPortal.DBEntities.MailMessage _mail in _mails)
                                {
                                    try
                                    {
                                        _client.Send(GenerateMail(_mail, _account.MailAddress));
                                        _mailService.MarkMailAsSend(_mail);
                                        sendamt++;
                                    }
                                    catch (Exception ex)
                                    {
                                        _mailService.MarkMailAsError(_mail, ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
                //Log.WriteLog("SendPendingMail() end");
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.Message);
                if (ex.InnerException != null) Log.WriteLog(ex.InnerException.Message);
            }
            finally
            {
                Log.WriteLog(string.Format("SendPendingMail() end，send {0} messages", sendamt));
            }
        }

        private static bool CustomizeCertificateValidation(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private SmtpClient SetSmtpClient(MailAccount account)
        {
            SmtpClient _client = new SmtpClient(account.SmtpServer, account.SmtpServerPort);

            _client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            _client.UseDefaultCredentials = false;

            // Create the credentials:
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(account.UserName, account.UserPassword);
            _client.EnableSsl = account.SslEnabled;
            _client.Credentials = credentials;

            return _client;
        }

        private System.Net.Mail.MailMessage GenerateMail(HRPortal.DBEntities.MailMessage message, string fromMailAddress)
        {
            System.Net.Mail.MailMessage _result = new System.Net.Mail.MailMessage(fromMailAddress, message.Rcpt, message.Subject, message.Body);
            AppendToMailAddressCollection(_result.CC, message.Cc);
            AppendToMailAddressCollection(_result.Bcc, message.Bcc);
            _result.BodyEncoding = Encoding.UTF8;
            _result.IsBodyHtml = message.IsHtml;
            _result.Priority = MailPriority.Normal;
            return _result;
        }

        private void AppendToMailAddressCollection(MailAddressCollection mailCollection, string mailString)
        {
            if (!string.IsNullOrEmpty(mailString))
            {
                string[] _addresses = mailString.Split(',');
                for (int i = 0; i < _addresses.Length; i++)
                {
                    if (IsLegalMailAddress(_addresses[i]))
                    {
                        mailCollection.Add(_addresses[i]);
                    }
                }
            }
        }

        private static bool IsLegalMailAddress(string mailAddress)
        {
            return Regex.IsMatch(mailAddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }


        public void Dispose()
        {
            _services.Dispose();
        }


        /// <summary>
        /// 抓取當天之前產生的未簽核假單，重新產生簽核通知
        /// </summary>
        public void restSendMail(DateTime selDate)
        {
            List<SignFlowRec> _signFlowRecList = SDB.SignFlowRec.ToList();
            DateTime startDate = selDate.AddMonths(-1);
            DateTime today = DateTime.Parse(System.DateTime.Now.ToLongDateString());
            DateTime leavebasedate = DateTime.Now.Date;//離職判斷基準日，當日還算在職。
            string webURL = GetWebUrl();
            string directLoginFlag = _services.GetService<SystemSettingService>().GetSettingValue("SignFormDirectLogin");
            List<string> formNoList = new List<string>();
            List<string> LeaveFormNo = new List<string>();
            List<Guid> LeaveFormID = new List<Guid>();
            List<string> LeaveCancelFormNo = new List<string>();
            List<string> OverTimeFormNo = new List<string>();
            List<Guid> OverTimeFormID = new List<Guid>();
            List<string> OverTimeCancelFormNo = new List<string>();
            List<string> exclusionDept = new List<string>() { "B315000", "B335000" };
            List<Guid> exclusionDeptID = new List<Guid>();
            exclusionDeptID = _services.GetService<DepartmentService>().GetAll().Where(x => exclusionDept.Contains(x.DepartmentCode)).Select(s => s.ID).ToList();

            LeaveFormNo = _services.GetService<LeaveFormService>().GetAll().Where(x => !exclusionDeptID.Contains(x.DepartmentID) && x.IsDeleted == false && x.Status == 1 && x.StartTime >= startDate && x.StartTime < selDate).Select(s => s.FormNo).ToList();
            LeaveFormID = _services.GetService<LeaveFormService>().GetAll().Where(x => x.IsDeleted == false && x.Status == 1 && x.StartTime >= startDate && x.StartTime < selDate).Select(s => s.ID).ToList();
            LeaveCancelFormNo = _services.GetService<LeaveCancelService>().GetAll().Where(x => x.IsDeleted == false && x.Status == 1 && LeaveFormID.Contains(x.LeaveFormID)).Select(s => s.FormNo).ToList();
            OverTimeFormNo = _services.GetService<OverTimeFormService>().GetAll().Where(x => !exclusionDeptID.Contains(x.DepartmentID) && x.IsDeleted == false && x.Status == 1 && x.StartTime >= startDate && x.StartTime < selDate).Select(s => s.FormNo).ToList();
            OverTimeFormID = _services.GetService<OverTimeFormService>().GetAll().Where(x => x.IsDeleted == false && x.Status == 1 && x.StartTime >= startDate && x.StartTime < selDate).Select(s => s.ID).ToList();
            OverTimeCancelFormNo = _services.GetService<OverTimeCancelService>().GetAll().Where(x => x.IsDeleted == false && x.Status == 1 && OverTimeFormID.Contains(x.OverTimeFormID)).Select(s => s.FormNo).ToList();

            formNoList.AddRange(LeaveFormNo);
            formNoList.AddRange(LeaveCancelFormNo);
            formNoList.AddRange(OverTimeFormNo);
            formNoList.AddRange(OverTimeCancelFormNo);

            var signList = (from s in _signFlowRecList
                            where (s.SignStatus == "W" || s.SignStatus == "O" || s.SignStatus == "T") && s.SignType != "S" && s.IsUsed == "Y" && formNoList.Contains(s.FormNumber)
                            group s by s.FormNumber into gs
                            orderby gs.Key
                            select new
                            {
                                ID = gs.Min(s => s.ID)
                            }).ToList(); //取得未簽核資料，撈取該單號最先簽核的那筆資料
            //整理單號
            List<string> singIdList = new List<string>();
            if (signList != null && signList.Count > 0)
            {
                foreach (var s in signList)
                {
                    singIdList.Add(s.ID);
                }
            }

            var empList = _signFlowRecList.Where(x => singIdList.Contains(x.ID)).
                GroupBy(g => new { g.SignerID, g.SignCompanyID }).
                Select(s => new { SignerID = s.Key.SignerID, SignCompanyID = s.Key.SignCompanyID }).ToList(); //取得待簽核資料，針對單號撈取需簽核的主管清單。

            var qemployees = _services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeType == "2" &&
                x.LeaveDate == null || (x.LeaveDate != null && x.LeaveDate >= leavebasedate));

            var signersinfo = (from m in empList
                               join c in qemployees on
                               new { signerid = m.SignerID, signercompanyid = m.SignCompanyID.Trim() } equals
                               new { signerid = c.EmployeeNO, signercompanyid = c.Company.CompanyCode.Trim() }
                               select new
                               {
                                   SignerID = m.SignerID,
                                   SignCompanyID = m.SignCompanyID,
                                   EmployeeName = c.EmployeeName,
                                   Email = c.Email,
                                   company_id = c.Company.Company_ID
                               }).ToList();


            var todaysendlist = _services.GetService<MailMessageService>().GetAll().Where(x => x.CreateTime >= today).ToList(); //取得今日寄件紀錄清單。

            //Log.WriteLog("resend() process begin");
            int sendcount = 0;
            foreach (var signer in signersinfo)
            {
                if (!String.IsNullOrEmpty(signer.Email))
                {
                    List<string> rcpt = new List<string>();
                    rcpt.Add(signer.Email);

                    if (!todaysendlist.Where(x => x.Rcpt.Contains(signer.Email)).Any())
                    {
                        string description = "系統網站";
                        string signweburl = webURL;
                        if (!string.IsNullOrWhiteSpace(directLoginFlag) && directLoginFlag.ToLower() == "true") //開放可直接登入時，轉換連結
                        {
                            signweburl = EncryptLoginURL(webURL, signer.SignerID, signer.company_id.Value, DateTime.Now);
                            description = "連結簽核畫面";
                        }
                        // 建立英文文化資訊
                        CultureInfo englishCulture = new CultureInfo("en-US");
                        // 取得月份的英文名稱
                        string monthName = englishCulture.DateTimeFormat.GetMonthName(startDate.Month);
                        
                        string subject = "(簽核)_來自請假加班系統的請假訊息";
                        string body = "<pre>您好，尚有" + startDate.Month + "月份未簽核之假單等待您的核准，請您至以下網址簽核假單<br/>";
                        body = body + "Still have leaves / overtime application of " + monthName + " is awaiting your approval. Please enter the website link for approval. <br/>";
                        body = body + "<br/><a href=" + signweburl + " >" + description + " Go to approval page</a>" + "<br/><br/>";
                        body = body + "<font>附註：本系統於每月1日開始發送上個月未簽核之假單提醒。</font><br/>";
                        body = body + "Note: The system will inform you that leaves / overtime application forms of unapproved on the 1st of every month.</pre>";
                        //body = body + "請假加班系統 敬上 " + System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "<br/>";

                        SendMail(rcpt.ToArray(), null, null, subject, body, true);
                        sendcount += 1;
                    }
                }
                else
                {
                    Log.WriteLog(string.Format("{0}，{1}，沒有設定 Email", signer.SignerID, signer.SignCompanyID));
                }
            }

            //Log.WriteLog(string.Format("本次新增{0}筆信件訊息", sendcount));
            //Log.WriteLog("resend() process end");
        }

        /// <summary>
        /// 取得網站 Url 會依照 Port 跟應用程式目錄，返回對應的 Url。
        /// </summary>
        /// <returns></returns>
        private string GetWebUrl()
        {
            string retval = "";
            //string scheme = "https";
            string domainName = Convert.ToString(ConfigurationManager.AppSettings["DomainName"]);
            string applicationPath = Convert.ToString(ConfigurationManager.AppSettings["ApplicationPath"]);

            //retval = scheme + "://" + domainName + applicationPath;
            retval = domainName + applicationPath;
            return retval;
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
            string AESKeyString = Convert.ToString(ConfigurationManager.AppSettings["URLEncryptKey"]);
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
                    //ViewBag.EncryptedSecret = targetEncrypted;
                    resultURL += directLoginController + @"/" + directLoginMethod + @"/?ES=" + targetEncrypted;
                }
            }
            return resultURL;
        }

        private void SendMail(string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            _services.GetService<MailMessageService>().CreateMail(_fromMail, rcpt, cc, bcc, subject, body, isHtml);
        }

    }
}

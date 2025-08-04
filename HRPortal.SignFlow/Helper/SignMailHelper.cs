using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using HRPortal.Services;
using HRPortal.Services.Models;
using HRPortal.SignFlow.SignLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YoungCloud.SignFlow.Model;

namespace HRPortal.SignFlow.Helper
{
    public class SignMailHelper : HttpApplication
    {
        private HRPortal_Services _services;
        private Dictionary<string, string> _absents;

        public SignMailHelper(Dictionary<string, string> absents)
        {
            _services = new HRPortal_Services();
            _absents = absents;
        }

        private void SendMail(string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            string _fromMail = _services.GetService<SystemSettingService>().GetSettingValue("NoticeEmailAddress");
            _services.GetService<MailMessageService>().CreateMail(_fromMail, rcpt, cc, bcc, subject, body, isHtml);
        }

        public void SendMailOnFlowAccepted(SignFlowRecModel _flow)
        {
            try
            {
                FormType _formType = (FormType)Enum.Parse(typeof(FormType), _flow.FormType);

                string _subject = Resource.FlowAcceptedMailSubject;
                string _body = Resource.FlowAcceptedMailBody;

                List<string> _rcpt = new List<string>();

                Employee _nextFlowSigner = null;
                Department _nextFlowSignerDepartment = null;
                Employee _sender;

                FormSummaryBuilder _summaryBuilder = new FormSummaryBuilder(_absents);

                LeaveForm _leaveForm = null;
                OverTimeForm _overTimeForm = null;
                PatchCardForm _patchCardForm = null;

                switch (_formType)
                {
                    case FormType.Leave:
                    case FormType.LeaveCancel:
                        if (_formType == FormType.Leave)
                        {
                            _leaveForm = _services.GetService<LeaveFormService>().FirstOrDefault(x => x.FormNo == _flow.FormNumber);
                        }
                        else
                        {
                            _leaveForm = _services.GetService<LeaveCancelService>().FirstOrDefault(x => x.FormNo == _flow.FormNumber).LeaveForm;
                        }

                        _sender = _services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == _leaveForm.EmployeeID);
                        if (_flow.SignType == "D")
                        {
                            _nextFlowSignerDepartment = _services.GetService<DepartmentService>().FirstOrDefault(x => x.CompanyID == _leaveForm.CompanyID && x.DepartmentCode == _flow.SignerID);
                        }
                        else
                        {
                            _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _leaveForm.CompanyID && x.EmployeeNO == _flow.SignerID);
                            if (_nextFlowSigner == null)
                            {
                                _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.EmployeeNO == _flow.SignerID && x.LeaveDate == null);
                            }
                        }

                        _body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave));
                        break;

                    case FormType.OverTime:
                    case FormType.OverTimeCancel:
                        if (_formType == FormType.OverTime)
                        {
                            _overTimeForm = _services.GetService<OverTimeFormService>().FirstOrDefault(x => x.FormNo == _flow.FormNumber);
                        }
                        else
                        {
                            _overTimeForm = _services.GetService<OverTimeCancelService>().FirstOrDefault(x => x.FormNo == _flow.FormNumber).OverTimeForm;
                        }

                        _sender = _services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == _overTimeForm.EmployeeID);
                        if (_flow.SignType == "D")
                        {
                            _nextFlowSignerDepartment = _services.GetService<DepartmentService>().FirstOrDefault(x => x.CompanyID == _overTimeForm.CompanyID && x.DepartmentCode == _flow.SignerID);
                        }
                        else
                        {
                            _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _overTimeForm.CompanyID && x.EmployeeNO == _flow.SignerID);
                            if (_nextFlowSigner == null)
                            {
                                _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.EmployeeNO == _flow.SignerID && x.LeaveDate == null);
                            }
                        }

                        _body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime));
                        break;

                    case FormType.PatchCard:
                    case FormType.PatchCardCancel:
                        if (_formType == FormType.PatchCard)
                        {
                            _patchCardForm = _services.GetService<PatchCardFormService>().FirstOrDefault(x => x.FormNo == _flow.FormNumber);
                        }
                        else
                        {
                            _patchCardForm = _services.GetService<PatchCardCancelService>().FirstOrDefault(x => x.FormNo == _flow.FormNumber).PatchCardForm;
                        }
                        _sender = _services.GetService<EmployeeService>().FirstOrDefault(x => x.ID == _patchCardForm.EmployeeID);
                        if (_flow.SignType == "D")
                        {
                            _nextFlowSignerDepartment = _services.GetService<DepartmentService>().FirstOrDefault(x => x.CompanyID == _patchCardForm.CompanyID && x.DepartmentCode == _flow.SignerID);
                        }
                        else
                        {
                            _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _patchCardForm.CompanyID && x.EmployeeNO == _flow.SignerID);
                            if (_nextFlowSigner == null)
                            {
                                _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.EmployeeNO == _flow.SignerID && x.LeaveDate == null);
                            }
                        }

                        _body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard));
                        break;

                    default:
                        throw new Exception();
                }

                if (_flow.SignType == "D")
                {
                    foreach (Employee _signer in _services.GetService<EmployeeService>().Where(x => x.DepartmentID == _nextFlowSignerDepartment.ID && x.Enabled))
                    {
                        _rcpt.Add(_signer.Email);
                    }
                }
                else
                {
                    _rcpt.Add(_nextFlowSigner.Email);
                }

                string _formTypeName = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType).DisplayName;
                //抓取申請人假單姓名 Irving 20170324
                var CUserName = _services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _flow.CUser).Select(x => x.EmployeeName).FirstOrDefault();
                _subject = _subject.Replace("[FormType]", CUserName + _formTypeName);
                _body = _body.Replace("[FormType]", _formTypeName);
                _body = _body.Replace("[Sender]", _sender.EmployeeName);

                //20170327修改 by Daniel，Request.Url.Authority會附加port在網址上，這樣會出現如https://hrportal.feds.com.tw:8000的狀況
                //改用Request.Url.Host
                //string _webURL = "https://" + HttpContext.Current.Request.Url.Authority.ToString();

                //20180321 Daniel 加上Application路徑(如果網站在網路底下還有切目錄的話，原來的方式會連到最外層)
                //string _webURL = "https://" + HttpContext.Current.Request.Url.Host;
                //再調整手機外網跟簽核專用站都不需要Port跟ApplicationPath
                string siteType = SiteFunctionInfo.SiteType.ToLower();
                string scheme = HttpContext.Current.Request.Url.Scheme;
                int port = HttpContext.Current.Request.Url.Port;
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
                    applicationPath = HttpContext.Current.Request.ApplicationPath;
                }
                //string strPort = ((scheme.ToLower() == "https" || (scheme.ToLower() != "https" && port == 80)) ? "" : (":" + port.ToString());

                //https就不用加上ApplicationPath了(一般走https會是網域就是直接進Portal，不會再設定子目錄)
                //string applicationPath = (scheme.ToLower() == "https") ? "" : HttpContext.Current.Request.ApplicationPath;

                string _webURL = scheme + "://" + HttpContext.Current.Request.Url.Host + strPort + applicationPath;

                string description = "系統網站";

                //20171221 Daniel 增加通知Mail內的連結可直接登入的機制
                string directLoginFlag = _services.GetService<SystemSettingService>().GetSettingValue("SignFormDirectLogin");
                if (!string.IsNullOrWhiteSpace(directLoginFlag) && directLoginFlag.ToLower() == "true") //開放直接登入時，需轉換Mail內連結
                {
                    _webURL = EncryptLoginURL(_webURL, _nextFlowSigner.EmployeeNO, _nextFlowSigner.Company.Company_ID.Value, DateTime.Now);
                    description = "連結簽核畫面";
                }

                //_body = _body + "<br/><a href=" + _webURL +" >系統網站</a>";
                _body = _body + "<br/><a href=\"" + _webURL + "\">" + description + "</a>";

                SendMail(_rcpt.ToArray(), null, null, _subject, _body, true);
            }
            catch
            {
            }
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
            string AESKeyString = System.Configuration.ConfigurationSettings.AppSettings["URLEncryptKey"];
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
    }
}
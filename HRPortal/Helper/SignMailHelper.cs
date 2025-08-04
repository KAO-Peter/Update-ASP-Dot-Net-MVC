using HRPortal.DBEntities;
using HRPortal.MultiLanguage;
using HRPortal.Services;
using HRPortal.Services.Models;
using HRPortal.SignFlow.SignLists;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using YoungCloud.SignFlow.Model;

namespace HRPortal.Helper
{
    public class SignMailHelper : HttpApplication
    {
        private HRPortal_Services _services;
        private Dictionary<string, string> _absents;
        private Dictionary<string, string> _absentsEN;


        public SignMailHelper(Dictionary<string, string> absents, Dictionary<string, string> absentsEN = null)
        {
            _services = new HRPortal_Services();
            _absents = absents;
            _absentsEN = absentsEN;
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

                FormSummaryBuilder _summaryBuilder = new FormSummaryBuilder(_absents, _absentsEN);

                LeaveForm _leaveForm = null;
                OverTimeForm _overTimeForm = null;
                PatchCardForm _patchCardForm = null;

                string formSummary="";
                string formSummaryEN="";

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
                                _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.EmployeeNO == _flow.SignerID && x.LeaveDate==null);
                            }
                        }

                        formSummary = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave);
                        formSummaryEN = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave));
                      
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

                        formSummary = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime);
                        formSummaryEN = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime, "en-US");

                        //_body = _body.Replace("[FormSummaryEN]", _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime));
                    
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

                        formSummary = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard);
                        formSummaryEN = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard));
                        
                        break;
                    default:
                        throw new Exception();
                }

                _body = _body.Replace("[FormSummary]", formSummary);
                _body = _body.Replace("[FormSummaryEN]", formSummaryEN);

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

                //20190528 Daniel 增加取得英文表單名稱
                Option optionFormType = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType);
                //string _formTypeName = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType).DisplayName;
                string _formTypeName = optionFormType == null ? "" : optionFormType.DisplayName;
                string _formTypeNameEN = optionFormType == null ? "" : optionFormType.DisplayResourceName;
                
                
                //抓取申請人假單姓名 Irving 20170324
                //var CUserName = _services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _flow.CUser).Select(x=>x.EmployeeName).FirstOrDefault();
                
                Employee user = _services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _flow.CUser).FirstOrDefault();
                string CUserName = user == null ? "" : user.EmployeeName;
                string CUserNameEN = user == null ? "" : user.EmployeeEnglishName;
                
                _subject = _subject.Replace("[FormType]", CUserName + _formTypeName);
                _subject = _subject.Replace("[FormTypeEN]", CUserNameEN + " " + _formTypeNameEN);
                
                _body = _body.Replace("[FormType]", _formTypeName);
                _body = _body.Replace("[Sender]", _sender.EmployeeName);
                _body = _body.Replace("[FormTypeEN]", _formTypeNameEN);
                _body = _body.Replace("[SenderEN]", _sender.EmployeeEnglishName);
                

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
                string strPort="";
                string applicationPath="";
                
                if (siteType=="mobile" || siteType=="outside" || siteType=="signonly" || scheme.ToLower() == "https")
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

                string _portalWebUrl = ConfigurationManager.AppSettings["PortalWebUrl"].ToString();
                string _webURL = _portalWebUrl + strPort + applicationPath;
                //string _webURL = scheme + "://" + HttpContext.Current.Request.Url.Host + strPort + applicationPath;

                string description = "系統網站";
                string descriptionEN = " HR Portal";
      
                //20171221 Daniel 增加通知Mail內的連結可直接登入的機制
                string directLoginFlag = _services.GetService<SystemSettingService>().GetSettingValue("SignFormDirectLogin");
                if (!string.IsNullOrWhiteSpace(directLoginFlag) && directLoginFlag.ToLower() == "true") //開放直接登入時，需轉換Mail內連結
                {
                    _webURL = EncryptLoginURL(_webURL, _nextFlowSigner.EmployeeNO, _nextFlowSigner.Company.Company_ID.Value, DateTime.Now);
                    description = "連結簽核畫面";
                    descriptionEN = " Go to approval page";
                }

                //_body = _body + "<br/><a href=" + _webURL +" >系統網站</a>";
                _body = _body + "<br/><a href=\"" + _webURL + "\">" + description + descriptionEN + "</a>";
                
                SendMail(_rcpt.ToArray(), null, null, _subject, _body, true);
            }
            catch
            {

            }
        }

        public void SendMailOnFormApproved(SignFlowRecModel _flow)
        {
            try
            {
                FormType _formType = (FormType)Enum.Parse(typeof(FormType), _flow.FormType);

                string _subject = Resource.FormApprovedMailSubject;
                
                //抓取申請人姓名 Irving 20170314
                //var Signername = _services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _flow.SenderID).Select(x => x.EmployeeName).FirstOrDefault();
               
                //string _body = Signername+Resource.FormApprovedMailBody_;
                string _body = Resource.FormApprovedMailBody_;

                List<string> _rcpt = new List<string>();

                Employee _nextFlowSigner;
                Employee _sender;

                FormSummaryBuilder _summaryBuilder = new FormSummaryBuilder(_absents, _absentsEN);

                LeaveForm _leaveForm = null;
                OverTimeForm _overTimeForm = null;
                PatchCardForm _patchCardForm = null;

                string formSummary = "";
                string formSummaryEN = "";

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
                        _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _leaveForm.CompanyID && x.EmployeeNO == _flow.SignerID);

                        formSummary = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave);
                        formSummaryEN = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave));
                       
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
                        _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _overTimeForm.CompanyID && x.EmployeeNO == _flow.SignerID);

                        formSummary = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime);
                        formSummaryEN = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime));
                       
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
                        _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _patchCardForm.CompanyID && x.EmployeeNO == _flow.SignerID);

                        formSummary = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard);
                        formSummaryEN = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard));
                       
                        break;
                    default:
                        throw new Exception();
                }

                _body = _body.Replace("[FormSummary]", formSummary);
                _body = _body.Replace("[FormSummaryEN]", formSummaryEN);

                //20190528 Daniel 增加取得英文表單名稱
                Option optionFormType = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType);
                //string _formTypeName = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType).DisplayName;
                string _formTypeName = optionFormType == null ? "" : optionFormType.DisplayName;
                string _formTypeNameEN = optionFormType == null ? "" : optionFormType.DisplayResourceName;
              
                _rcpt.Add(_sender.Email);

                //_subject = _subject.Replace("[FormType]",Signername+ _formTypeName);
                _subject = _subject.Replace("[FormType]", _sender.EmployeeName + _formTypeName);
                _subject = _subject.Replace("[FormTypeEN]", _sender.EmployeeEnglishName + " " + _formTypeNameEN);

                _body = _body.Replace("[FormType]", _formTypeName);
                _body = _body.Replace("[Sender]", _sender.EmployeeName);
                _body = _body.Replace("[FormTypeEN]", _formTypeNameEN);
                _body = _body.Replace("[SenderEN]", _sender.EmployeeEnglishName);

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
                //string _webURL = "https://" + HttpContext.Current.Request.Url.Authority.ToString();

                string description = "系統網站";
                string descriptionEN = " HR Portal";

                _body = _body + "<br/><a href=\"" + _webURL + "\">" + description + descriptionEN + "</a>";
                SendMail(_rcpt.ToArray(), null, null, _subject, _body, true);
            }
            catch
            {

            }
        }

        public void SendMailOnFlowRejected(SignFlowRecModel _flow)
        {
            try
            {
                FormType _formType = (FormType)Enum.Parse(typeof(FormType), _flow.FormType);

                string _subject = Resource.FlowRejectedMailSubject;
                string _body = Resource.FlowRejectedMailBody;

                List<string> _rcpt = new List<string>();

                Employee _nextFlowSigner = null;
                Department _nextFlowSignerDepartment = null;
                Employee _sender;

                FormSummaryBuilder _summaryBuilder = new FormSummaryBuilder(_absents, _absentsEN);

                LeaveForm _leaveForm = null;
                OverTimeForm _overTimeForm = null;
                PatchCardForm _patchCardForm = null;

                string formSummary = "";
                string formSummaryEN = "";

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
                        }
                        
                        formSummary = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave);
                        formSummaryEN = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave, "en-US");
                        
                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave));
                        
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
                        }

                        formSummary = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime);
                        formSummaryEN = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime));
                      
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
                        }

                        formSummary = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard);
                        formSummaryEN = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard));
                       
                        break;
                    default:
                        throw new Exception();
                }

                _body = _body.Replace("[FormSummary]", formSummary);
                _body = _body.Replace("[FormSummaryEN]", formSummaryEN);

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

                //20190528 Daniel 增加取得英文表單名稱
                Option optionFormType = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType);
                //string _formTypeName = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType).DisplayName;
                string _formTypeName = optionFormType == null ? "" : optionFormType.DisplayName;
                string _formTypeNameEN = optionFormType == null ? "" : optionFormType.DisplayResourceName;
              
                //_subject = _subject.Replace("[FormType]", _formTypeName);
                _subject = _subject.Replace("[FormType]", _sender.EmployeeName + _formTypeName);
                _subject = _subject.Replace("[FormTypeEN]", _sender.EmployeeEnglishName + " " + _formTypeNameEN);

                _body = _body.Replace("[FormType]", _formTypeName);
                _body = _body.Replace("[Sender]", _sender.EmployeeName);
                _body = _body.Replace("[FormTypeEN]", _formTypeNameEN);
                _body = _body.Replace("[SenderEN]", _sender.EmployeeEnglishName);

                //20180322 Daniel 退件沒有附上網站連結，底下這行不需要了
                //string _webURL = "https://" + HttpContext.Current.Request.Url.Authority.ToString();
                
                SendMail(_rcpt.ToArray(), null, null, _subject, _body, true);
            }
            catch
            {

            }
        }

        public void SendMailOnFormReturned(SignFlowRecModel _flow)
        {
            try
            {
                FormType _formType = (FormType)Enum.Parse(typeof(FormType), _flow.FormType);

                string _subject = Resource.FormReturnedMailSubject;
                //抓取申請人姓名 Irving 20170314
                //var Signername = _services.GetService<EmployeeService>().GetAll().Where(x => x.EmployeeNO == _flow.SenderID).Select(x => x.EmployeeName).FirstOrDefault();
                //string _body = Signername + Resource.FormReturnedMailBody_;

                string _body = Resource.FormReturnedMailBody_;

                List<string> _rcpt = new List<string>();

                Employee _nextFlowSigner;
                Employee _sender;

                FormSummaryBuilder _summaryBuilder = new FormSummaryBuilder(_absents);

                LeaveForm _leaveForm = null;
                OverTimeForm _overTimeForm = null;
                PatchCardForm _patchCardForm = null;

                string formSummary = "";
                string formSummaryEN = "";

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
                        _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _leaveForm.CompanyID && x.EmployeeNO == _flow.SignerID);
                        
                        formSummary = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave);
                        formSummaryEN = _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave, "en-US");
                       
                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_leaveForm, FormType.Leave));
                        
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
                        _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _overTimeForm.CompanyID && x.EmployeeNO == _flow.SignerID);

                        formSummary = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime);
                        formSummaryEN = _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime, "en-US");

                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_overTimeForm, FormType.OverTime));
                        
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
                        _nextFlowSigner = _services.GetService<EmployeeService>().FirstOrDefault(x => x.CompanyID == _patchCardForm.CompanyID && x.EmployeeNO == _flow.SignerID);

                        formSummary = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard);
                        formSummaryEN = _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard, "en-US");
                        
                        //_body = _body.Replace("[FormSummary]", _summaryBuilder.BuildSummary(_patchCardForm, FormType.PatchCard));
                        
                        break;
                    default:
                        throw new Exception();
                }

                _body = _body.Replace("[FormSummary]", formSummary);
                _body = _body.Replace("[FormSummaryEN]", formSummaryEN);

                //20190528 Daniel 增加取得英文表單名稱
                Option optionFormType = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType);
                //string _formTypeName = _services.GetService<OptionService>().GetOptionListByGroup("FormType").FirstOrDefault(x => x.OptionValue == _flow.FormType).DisplayName;
                string _formTypeName = optionFormType == null ? "" : optionFormType.DisplayName;
                string _formTypeNameEN = optionFormType == null ? "" : optionFormType.DisplayResourceName;
              
                _rcpt.Add(_sender.Email);

                //_subject = _subject.Replace("[FormType]", Signername+_formTypeName);
                _subject = _subject.Replace("[FormType]", _sender.EmployeeName + _formTypeName);
                _subject = _subject.Replace("[FormTypeEN]", _sender.EmployeeEnglishName + " " + _formTypeNameEN);

                _body = _body.Replace("[FormType]", _formTypeName);
                _body = _body.Replace("[Sender]", _sender.EmployeeName);
                _body = _body.Replace("[FormTypeEN]", _formTypeNameEN);
                _body = _body.Replace("[SenderEN]", _sender.EmployeeEnglishName);

                //20170519 Start Daniel，增加退簽時寄信給簽核過程經手的主管(原簽核者，非代理人)
                using (HRPortalSignFlowQueryHelper _queryHelper = new HRPortalSignFlowQueryHelper())
                {
                    //依據表單號碼取得簽核流程
                    IList<SignFlowRecModel> _signFlowList = _queryHelper.SignFlowRecQueryHelper.GetSignFlowByFormNumber(_flow.FormNumber).Where(x => x.SignStatus != "W").ToList();
                    
                    //取出流程中所有簽核者的員工編號與公司代碼
                    List<Tuple<string,string>> empNoList = _signFlowList.Select(x => new Tuple<string,string> (x.SignerID, x.SignCompanyID )).Distinct().ToList();
                    
                    //取得所有簽核者的Employee資訊，離職的就不寄了
                    List<Employee> signEmpList = _services.GetService<EmployeeService>().GetEmployeeListByEmpNoList(empNoList);

                    //退簽通知只需要通知最後一次正常的流程裡面的簽核主管與原簽核者
                    //倒過來找到原簽核者就停
                    List<string> RecipientList=new List<string>();
                    foreach (SignFlowRecModel sfr in _signFlowList.Reverse()) 
                    {
                        Employee empRecipient = signEmpList.FirstOrDefault(x => x.EmployeeNO == sfr.SignerID);
                        if (empRecipient != null)
                        {
                            RecipientList.Add(empRecipient.Email);

                        }//回到原簽核者就跳出
                        if (sfr.FormLevelID == "0")
                        {
                            break;
                        }
                    }

                    //寄信通知，每個人只寄一封
                    foreach (string rcptEmail in RecipientList)
                    {
                        SendMail(new string[] { rcptEmail }, null, null, _subject, _body, false);
                    }
                }
                
                //SendMail(_rcpt.ToArray(), null, null, _subject, _body, false);
                //20170519 End

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
                    //ViewBag.EncryptedSecret = targetEncrypted;
                    resultURL += directLoginController + @"/" + directLoginMethod + @"/?ES=" + targetEncrypted;
                }

            }
            return resultURL;

        }
    }
}
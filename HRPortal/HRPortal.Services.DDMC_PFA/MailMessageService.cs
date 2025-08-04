using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HRPortal.DBEntities.DDMC_PFA;

namespace HRPortal.Services.DDMC_PFA
{
    public class MailMessageService : BaseCrudService<PfaMailMessage>
    {
        PfaSignerListService pfaSignerListService;

        public MailMessageService(HRPortal_Services services)
            : base(services)
        {
            pfaSignerListService = new PfaSignerListService(services);
        }

        public List<PfaMailMessage> GetPendingMessageByFromMailAddress(string fromMailAddress)
        {
            PfaMailAccount _account = this.Services.GetService<MailAccountService>().GetMailAccount(fromMailAddress);
            List<PfaMailMessage> _messages = GetAll().Where(x => x.FromAccountID == _account.ID && x.HadSend == false).ToList();
            return _messages;
        }

        public void SetPfaSignerListToMailMessage()
        {
            string[] sourceType = { "4", "5", "6", "7" };
            var now = DateTime.Now.Date;
            var pfaSignerList = Services.GetService<PfaSignerListService>().GetAll().Where(x=> (DbFunctions.TruncateTime(x.FirstDate) == now || 
                                                                                                DbFunctions.TruncateTime(x.SecondDate) == now || 
                                                                                                DbFunctions.TruncateTime(x.LastDate) == now)).ToList();
            if (pfaSignerList.Any())
            {
                var _account = Services.GetService<MailAccountService>().GetAll().FirstOrDefault();
                if (_account != null)
                {
                    var messagesList = GetAll().Where(x => x.FromAccountID == _account.ID && DbFunctions.TruncateTime(x.CreateTime) == now &&
                                                           sourceType.Contains(x.SourceType)).ToList();
                    foreach (var pfaSigner in pfaSignerList)
                    {
                        var tempMessagesList = messagesList.Where(x => x.PfaCycleID == pfaSigner.PfaCycleID &&
                                                                       x.EmployeeID == pfaSigner.EmployeeID).ToList();

                        switch (pfaSigner.SignType)
                        {
                            case "1": // 自評
                                #region 自評
                                var pfaSelfSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.PfaCycleEmp.PfaCycleID == pfaSigner.PfaCycleID &&
                                                                                                                                   x.PreSignEmpID == pfaSigner.EmployeeID && x.IsSelfEvaluation == true &&
                                                                                                                                   x.Status != "e");
                                if (pfaSelfSignProcess == null)
                                {
                                    pfaSigner.IsMeat = false;
                                    pfaSignerListService.Update(pfaSigner);
                                }
                                else
                                {
                                    pfaSigner.IsMeat = true;
                                    pfaSignerListService.Update(pfaSigner);

                                    var employee = Services.GetService<EmployeeService>().GetAll().FirstOrDefault(x => x.ID == pfaSigner.EmployeeID);
                                    if (employee != null)
                                    {
                                        var sendMail = new PfaMailMessage
                                        {
                                            ID = Guid.NewGuid(),
                                            PfaCycleID = pfaSigner.PfaCycleID,
                                            EmployeeID = pfaSigner.EmployeeID,
                                            FromAccountID = _account.ID,
                                            Rcpt = employee.Email,
                                            Subject = "績效考核通知",
                                            IsHtml = false,
                                            IsCancel = false,
                                            HadSend = false,
                                            CreateTime = DateTime.Now,
                                        };
                                        if (pfaSigner.LastDate == now)
                                        {
                                            var tempMessages = tempMessagesList.FirstOrDefault(x => x.SourceType == "7");
                                            if (tempMessages != null)
                                                continue;
                                            sendMail.SourceType = "7"; // 7.逾期通知
                                            sendMail.Body = "提醒您，考績表自評已逾期，請立刻填寫，謝謝。";
                                        }
                                        else 
                                        {
                                            var tempMessages = tempMessagesList.FirstOrDefault(x => x.SourceType == "4");
                                            if (tempMessages != null)
                                                continue;
                                            var mm = pfaSigner.SecondDate.Value.ToString("MM");
                                            var dd = pfaSigner.SecondDate.Value.ToString("dd");
                                            sendMail.SourceType = "4"; // 4.自評稽催通知
                                            sendMail.Body = string.Format("提醒您，考績表需於{0}月{1}日自評完畢，請確認是否已經完成，謝謝。", mm, dd);
                                        }
                                        Create(sendMail);
                                    }
                                }
                                #endregion
                                break;
                            case "2": // 初核
                                #region 初核
                                var pfaFirstSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.PfaCycleEmp.PfaCycleID == pfaSigner.PfaCycleID &&
                                                                                                                                    x.PreSignEmpID == pfaSigner.EmployeeID && 
                                                                                                                                    x.IsFirstEvaluation == true && x.IsSecondEvaluation == false &&
                                                                                                                                    x.Status != "e");
                                if (pfaFirstSignProcess == null)
                                {
                                    pfaSigner.IsMeat = false;
                                    pfaSignerListService.Update(pfaSigner);
                                }
                                else
                                {
                                    pfaSigner.IsMeat = true;
                                    pfaSignerListService.Update(pfaSigner);

                                    var employee = Services.GetService<EmployeeService>().GetAll().FirstOrDefault(x => x.ID == pfaSigner.EmployeeID);
                                    if (employee != null)
                                    {
                                        var mm = pfaSigner.SecondDate.Value.ToString("MM");
                                        var dd = pfaSigner.SecondDate.Value.ToString("dd");
                                        var sendMail = new PfaMailMessage
                                        {
                                            ID = Guid.NewGuid(),
                                            PfaCycleID = pfaSigner.PfaCycleID,
                                            EmployeeID = pfaSigner.EmployeeID,
                                            FromAccountID = _account.ID,
                                            Rcpt = employee.Email,
                                            Subject = "績效考核通知",
                                            IsHtml = false,
                                            IsCancel = false,
                                            HadSend = false,
                                            CreateTime = DateTime.Now,
                                        };

                                        var tempMessages = tempMessagesList.FirstOrDefault(x => x.SourceType == "5");
                                        if (tempMessages != null)
                                            continue;

                                        sendMail.SourceType = "5"; // 5.初核主管稽催
                                        sendMail.Body = string.Format("提醒您，考績表需於{0}月{1}日簽核完畢，請確認是否已經完成，謝謝。", mm, dd);
                                        Create(sendMail);
                                    }
                                }
                                #endregion
                                break;
                            case "3": // 複核
                                #region 複核
                                var pfaSecondSignProcess = Services.GetService<PfaSignProcessService>().GetAll().FirstOrDefault(x => x.PfaCycleEmp.PfaCycleID == pfaSigner.PfaCycleID &&
                                                                                                                                     x.PreSignEmpID == pfaSigner.EmployeeID &&
                                                                                                                                     x.IsSecondEvaluation == true &&
                                                                                                                                     x.Status != "e");
                                if (pfaSecondSignProcess == null)
                                {
                                    pfaSigner.IsMeat = false;
                                    pfaSignerListService.Update(pfaSigner);
                                }
                                else
                                {
                                    pfaSigner.IsMeat = true;
                                    pfaSignerListService.Update(pfaSigner);

                                    var employee = Services.GetService<EmployeeService>().GetAll().FirstOrDefault(x => x.ID == pfaSigner.EmployeeID);
                                    if (employee != null)
                                    {
                                        var mm = pfaSigner.SecondDate.Value.ToString("MM");
                                        var dd = pfaSigner.SecondDate.Value.ToString("dd");
                                        var sendMail = new PfaMailMessage
                                        {
                                            ID = Guid.NewGuid(),
                                            PfaCycleID = pfaSigner.PfaCycleID,
                                            EmployeeID = pfaSigner.EmployeeID,
                                            FromAccountID = _account.ID,
                                            Rcpt = employee.Email,
                                            Subject = "績效考核通知",
                                            IsHtml = false,
                                            IsCancel = false,
                                            HadSend = false,
                                            CreateTime = DateTime.Now,
                                        };

                                        var tempMessages = tempMessagesList.FirstOrDefault(x => x.SourceType == "6");
                                        if (tempMessages != null)
                                            continue;

                                        sendMail.SourceType = "6"; // 6.複核主管稽催
                                        sendMail.Body = string.Format("提醒您，考績表需於{0}月{1}日簽核完畢，請確認是否已經完成，謝謝。", mm, dd);
                                        Create(sendMail);
                                    }
                                }
                                #endregion
                                break;
                            default:
                                continue;
                        }
                    }
                }
            }
        }

        public void MarkMailAsSend(PfaMailMessage mail)
        {
            mail.HadSend = true;
            mail.SendTimeStamp = DateTime.Now;

            this.Update(mail);
        }

        public void MarkMailAsError(PfaMailMessage mail, string errorMessage)
        {
            mail.ErrorTimeStamp = DateTime.Now;
            mail.ErrorMessage = errorMessage;

            this.Update(mail);
        }

        public int CreateMail(string from, string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            PfaMailMessage _message = new PfaMailMessage();

            PfaMailAccount _account = this.Services.GetService<MailAccountService>().GetMailAccount(from);
            _message.ID = Guid.NewGuid();
            _message.FromAccountID = _account.ID;
            _message.Rcpt = CombineMailAddressList(rcpt);
            _message.Cc = CombineMailAddressList(cc);
            _message.Bcc = CombineMailAddressList(bcc);
            _message.Subject = subject;
            _message.Body = body;
            _message.IsHtml = isHtml;
            _message.HadSend = false;
            _message.CreateTime = DateTime.Now;

            return this.Create(_message);
        }

        private string CombineMailAddressList(string[] addressList)
        {
            if (addressList == null || addressList.Count() <= 0)
                return null;

            string _result = string.Empty;
            foreach (string _address in addressList.Distinct())
            {
                if (!string.IsNullOrEmpty(_result))
                {
                    _result += ",";
                }
                _result += _address;
            }
            return _result;
        }

        public PfaMailMessage GetMailMessageByID(Guid ID)
        {
            return GetAll().Where(x => x.ID == ID).FirstOrDefault();
        }
    }
}

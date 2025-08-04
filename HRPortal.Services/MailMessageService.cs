//using HRPortal.Models;
using HRPortal.DBEntities;
using HRPortal.Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services
{
    public class MailMessageService : BaseCrudService<MailMessage>
    {
        public MailMessageService(HRPortal_Services services)
            : base(services)
        {
        }

        public List<MailMessage> GetPendingMessageByFromMailAddress(string fromMailAddress)
        {
            MailAccount _account = this.Services.GetService<MailAccountService>().GetMailAccount(fromMailAddress);
            List<MailMessage> _messages = GetAll().Where(x => x.FromAccountID == _account.ID && x.HadSend == false).ToList();
            return _messages;
        }

        public void MarkMailAsSend(MailMessage mail)
        {
            mail.HadSend = true;
            mail.SendTimeStamp = DateTime.Now;

            this.Update(mail);
        }

        public void MarkMailAsError(MailMessage mail, string errorMessage)
        {
            mail.ErrorTimeStamp = DateTime.Now;
            mail.ErrorMessage = errorMessage;

            this.Update(mail);
        }

        public int CreateMail(string from, string[] rcpt, string[] cc, string[] bcc, string subject, string body, bool isHtml)
        {
            MailMessage _message = new MailMessage();

            MailAccount _account = this.Services.GetService<MailAccountService>().GetMailAccount(from);
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

        public MailMessage GetMailMessageByID(Guid ID)
        {
            return GetAll().Where(x => x.ID == ID).FirstOrDefault();
        }
    }
}

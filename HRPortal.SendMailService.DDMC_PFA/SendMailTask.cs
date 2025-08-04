using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA;

namespace HRPortal.SendMailService.DDMC_PFA
{
    public class SendMailTask : IDisposable
    {
        private HRPortal_Services _services;

        public SendMailTask()
        {
            _services = new HRPortal_Services();
        }

        public void SendPendingMail()
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = CustomizeCertificateValidation;
                using (MailMessageService _mailService = _services.GetService<MailMessageService>())
                {
                    _mailService.SetPfaSignerListToMailMessage();
                    List<PfaMailAccount> _accounts = _services.GetService<MailAccountService>().GetAll().ToList();

                    foreach (PfaMailAccount _account in _accounts)
                    {
                        List<PfaMailMessage> _mails = _mailService.GetPendingMessageByFromMailAddress(_account.MailAddress);
                        if (_mails.Count > 0)
                        {
                            using (SmtpClient _client = SetSmtpClient(_account))
                            {
                                foreach (PfaMailMessage _mail in _mails)
                                {
                                    try
                                    {
                                        _client.Send(GenerateMail(_mail, _account.MailAddress));
                                        _mailService.MarkMailAsSend(_mail);
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
            }
            catch(Exception ex)
            {
            }
        }

        private static bool CustomizeCertificateValidation(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private SmtpClient SetSmtpClient(PfaMailAccount account)
        {
            SmtpClient _client = new SmtpClient(account.SmtpServer, account.SmtpServerPort);

            _client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            _client.UseDefaultCredentials = false;

            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(account.UserName, account.UserPassword);
            _client.EnableSsl = account.SslEnabled;
            _client.Credentials = credentials;

            return _client;
        }

        private System.Net.Mail.MailMessage GenerateMail(HRPortal.DBEntities.DDMC_PFA.PfaMailMessage message, string fromMailAddress)
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
    }
}

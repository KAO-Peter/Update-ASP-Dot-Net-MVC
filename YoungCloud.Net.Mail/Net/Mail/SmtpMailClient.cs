using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace YoungCloud.Net.Mail
{
    /// <summary>
    /// The smtp mail utility.
    /// </summary>
    public class SmtpMailClient : Disposable, ISmtpMailClient
    {
        private event SendCompletedEventHandler m_SendCompletedEventDelegate = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailClient">SmtpMailClient</see> class.
        /// </summary>
        public SmtpMailClient()
        {
            Message = new MailMessage();
            Client = new SmtpClient();
            Client.Credentials = CredentialCache.DefaultNetworkCredentials;
            Message.BodyEncoding = Encoding.UTF8;
            Message.IsBodyHtml = true;
            Message.Priority = MailPriority.Normal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailClient">SmtpMailClient</see> class.
        /// </summary>
        /// <param name="host">The host address of mail server.</param>
        /// <param name="port">The port of mail server.</param>
        public SmtpMailClient(string host, int port)
            : this()
        {
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailClient">SmtpMailClient</see> class.
        /// </summary>
        /// <param name="host">The host address of mail server.</param>
        /// <param name="mailMessage">The smtp mail data.</param>
        public SmtpMailClient(string host, MailMessage mailMessage)
            : this()
        {
            Host = host;
            Message = mailMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpMailClient">SmtpMailClient</see> class.
        /// </summary>
        /// <param name="host">The host address of mail server.</param>
        /// <param name="port">The port of mail server.</param>
        /// <param name="mailMessage">The smtp mail data.</param>
        public SmtpMailClient(string host, int port, MailMessage mailMessage)
            : this(host, mailMessage)
        {
            Port = port;
        }

        /// <summary>
        /// Add attachment file.
        /// </summary>
        /// <param name="filePath">The full path of attachment file.</param>
        public void AddAttachmentFile(string filePath)
        {
            Attachment _Attachment = new Attachment(filePath);
            Message.Attachments.Add(_Attachment);
        }

        /// <summary>
        /// Add image link.
        /// </summary>
        /// <param name="contentId">The content id of image.</param>
        /// <param name="imageRealPath">The real path of image.</param>
        public void AddImageLink(string contentId, string imageRealPath)
        {
            using (LinkedResource _LinkedResource = new LinkedResource(imageRealPath))
            {
                _LinkedResource.ContentId = contentId;
                using (AlternateView _AlternateView = AlternateView.CreateAlternateViewFromString("<img src=cid:" + contentId + ">", null, "text/html"))
                {
                    _AlternateView.LinkedResources.Add(_LinkedResource);
                    Message.AlternateViews.Add(_AlternateView);
                }
            }
        }

        /// <summary>
        /// The Bcc address list.(separate by ;)
        /// </summary>
        public string Bcc
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                string[] _Addresses = value.Split(';');
                for (int i = 0; i < _Addresses.Length; i++)
                {
                    if (_Addresses[i].IsLegalMailAddress())
                    {
                        Message.Bcc.Add(new MailAddress(_Addresses[i]));
                    }
                }
            }
        }

        /// <summary>
        /// The mail body.
        /// </summary>
        public string Body
        {
            private get;
            set;
        }

        /// <summary>
        /// The encoding of mail body.(Default setting is UTF-8)
        /// </summary>
        public Encoding BodyEncoding
        {
            set
            {
                Message.BodyEncoding = value;
            }
        }

        /// <summary>
        /// The Cc address list.(separate by ;)
        /// </summary>
        public string CC
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                string[] _Addresses = value.Split(';');
                for (int i = 0; i < _Addresses.Length; i++)
                {
                    if (_Addresses[i].IsLegalMailAddress())
                    {
                        Message.CC.Add(new MailAddress(_Addresses[i]));
                    }
                }
            }
        }

        /// <summary>
        /// The instance of SmtpClient.
        /// </summary>
        private SmtpClient Client
        {
            get;
            set;
        }

        /// <summary>
        /// The security setting of smtp server.
        /// </summary>
        public NetworkCredential Credentials
        {
            set
            {
                Client.Credentials = value;
            }
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoded from Dispose method or not.</param>
        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Message.Dispose();
                    Client.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Enable the smtp mail ssl setting or not.
        /// </summary>
        public bool EnableSsl
        {
            set
            {
                Client.EnableSsl = value;
            }
        }

        /// <summary>
        /// The sender mail address.
        /// </summary>
        public string From
        {
            set
            {
                if (value.IsLegalMailAddress())
                {
                    Message.From = new MailAddress(value);
                }
                else
                {
                    throw new Exception("Invalidate mail address.");
                }
            }
        }

        /// <summary>
        /// The host address of mail server.
        /// </summary>
        public string Host
        {
            set
            {
                Client.Host = value;
            }
        }

        /// <summary>
        /// The mail body is html format or not.(Default is true)
        /// </summary>
        public bool IsBodyHtml
        {
            set
            {
                Message.IsBodyHtml = value;
            }
        }

        /// <summary>
        /// The instance of MailMessage.
        /// </summary>
        private MailMessage Message
        {
            get;
            set;
        }

        /// <summary>
        /// The port of mail server.
        /// </summary>
        public int Port
        {
            set
            {
                Client.Port = value;
            }
        }

        /// <summary>
        /// To set the authorization information of mail server.
        /// </summary>
        /// <param name="user">The user account of server.</param>
        /// <param name="password">The password of user.</param>
        public void SetAuthorization(string user, string password)
        {
            Client.Credentials = new NetworkCredential(user, password);
        }

        /// <summary>
        /// Send out the mail message.
        /// </summary>
        /// <example>
        ///     <code>
        ///         ISmtpMailClient _Mail = new SmtpMailClient("smtp.gmail.com", 587);
        ///         _Mail.SetAuthorization("notice@xxx.com", "1234");
        ///         _Mail.Subject = "Test";
        ///         _Mail.Body = "Test Mail by my self";
        ///         _Mail.To = "notice@xxx.com";
        ///         _Mail.From = "notice@xxx.com";
        ///         _Mail.EnableSsl = true;
        ///         _Mail.Send();
        ///     </code>
        /// </example>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public void Send()
        {
            Send(false);
        }

        /// <summary>
        /// Send out the mail message.
        /// </summary>
        /// <param name="isSendAsSync">Is sended as sync way or not.</param>
        /// <example>
        ///     <code>
        ///         ISmtpMailClient _Mail = new SmtpMailClient("smtp.gmail.com", 587);
        ///         _Mail.SetAuthorization("notice@xxx.com", "1234");
        ///         _Mail.Subject = "Test";
        ///         _Mail.Body = "Test Mail by my self";
        ///         _Mail.To = "notice@xxx.com";
        ///         _Mail.From = "notice@xxx.com";
        ///         _Mail.EnableSsl = true;
        ///         _Mail.Send(false);
        ///     </code>
        /// </example>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public void Send(bool isSendAsSync)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(SmtpMailClient).Name);
            }

            if (Message.IsBodyHtml)
            {
                Message.Body = Body.Replace("\r\n", "<br/>");
            }
            else
            {
                Message.Body = Body;
            }

            if (isSendAsSync)
            {
                Client.SendCompleted += new SendCompletedEventHandler(Mail_SendCompleted);
                Client.SendAsync(Message, "");
            }
            else
            {
                Client.Send(Message);
            }
        }

        private void Mail_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (m_SendCompletedEventDelegate != null)
                {
                    m_SendCompletedEventDelegate.Invoke(sender, e);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Delegate event for send mail completed.
        /// </summary>
        public event SendCompletedEventHandler SendCompletedEventDelegate
        {
            add
            {
                m_SendCompletedEventDelegate += value;
            }
            remove
            {
                m_SendCompletedEventDelegate -= value;
            }
        }

        /// <summary>
        /// The priority of mail.(Default is normal.)
        /// </summary>
        public MailPriority SmtpMailPriority
        {
            set
            {
                Message.Priority = value;
            }
        }

        /// <summary>
        /// The mail subject.
        /// </summary>
        public string Subject
        {
            set
            {
                Message.Subject = value;
            }
        }

        /// <summary>
        /// The receiver mail address list.(separate by ;)
        /// </summary>
        public string To
        {
            set
            {
                string[] _Addresses = value.Split(';');
                for (int i = 0; i < _Addresses.Length; i++)
                {
                    if (_Addresses[i].IsLegalMailAddress())
                    {
                        Message.To.Add(new MailAddress(_Addresses[i]));
                    }
                }
            }
        }
    }
}
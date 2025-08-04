using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace YoungCloud.Net.Mail
{
    /// <summary>
    /// The definition of smtp mail instance.
    /// </summary>
    public interface ISmtpMailClient : IDisposable
    {
        /// <summary>
        /// Add attachment file.
        /// </summary>
        /// <param name="filePath">The full path of attachment file.</param>
        void AddAttachmentFile(string filePath);
        /// <summary>
        /// Add image link.
        /// </summary>
        /// <param name="contentId">The content id of image.</param>
        /// <param name="imageRealPath">The real path of image.</param>
        void AddImageLink(string contentId, string imageRealPath);
        /// <summary>
        /// The Bcc address list.(separate by ;)
        /// </summary>
        string Bcc { set; }
        /// <summary>
        /// The mail body.
        /// </summary>
        string Body { set; }
        /// <summary>
        /// The encoding of mail body.(Default setting is UTF-8)
        /// </summary>
        Encoding BodyEncoding { set; }
        /// <summary>
        /// The Cc address list.(separate by ;)
        /// </summary>
        string CC { set; }
        /// <summary>
        /// The security setting of smtp server.
        /// </summary>
        NetworkCredential Credentials { set; }
        /// <summary>
        /// Enable the smtp mail ssl setting or not.
        /// </summary>
        bool EnableSsl { set; }
        /// <summary>
        /// The sender mail address.
        /// </summary>
        string From { set; }
        /// <summary>
        /// The host address of mail server.
        /// </summary>
        string Host { set; }
        /// <summary>
        /// The mail body is html format or not.(Default is true)
        /// </summary>
        bool IsBodyHtml { set; }
        /// <summary>
        /// The port number of mail server.
        /// </summary>
        int Port { set; }
        /// <summary>
        /// The priority of mail.(Default is normal.)
        /// </summary>
        MailPriority SmtpMailPriority { set; }
        /// <summary>
        /// Send out the mail message.
        /// </summary>
        void Send();
        /// <summary>
        /// Send out the mail message.
        /// </summary>
        /// <param name="isSendAsSync">Is sended as sync way or not.</param>
        void Send(bool isSendAsSync);
        /// <summary>
        /// The event of SendCompleted.
        /// </summary>
        event SendCompletedEventHandler SendCompletedEventDelegate;
        /// <summary>
        /// To set the authorization information of mail server.
        /// </summary>
        /// <param name="user">The user account of server.</param>
        /// <param name="password">The password of user.</param>
        void SetAuthorization(string user, string password);
        /// <summary>
        /// The mail subject.
        /// </summary>
        string Subject { set; }
        /// <summary>
        /// The receiver mail address list.(separate by ;)
        /// </summary>
        string To { set; }
    }
}
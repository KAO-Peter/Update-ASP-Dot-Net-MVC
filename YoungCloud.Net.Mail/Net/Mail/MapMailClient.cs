using System;
using System.Text.RegularExpressions;

namespace YoungCloud.Net.Mail
{
    /// <summary>
    /// The client utility of imap mail service.
    /// </summary>
    public partial class MapMailClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapMailClient">IMapMailClient</see> class.
        /// </summary>
        /// <param name="host">The host of imap server.</param>
        /// <param name="port">The port of service.</param>
        public MapMailClient(string host, int port)
        {
            Client = new MapClient(host, port);
        }

        private MapClient Client
        {
            get;
            set;
        }

        /// <summary>
        /// To get unread mail count of mailbox.
        /// </summary>
        /// <param name="mailboxName">The name of mailbox.</param>
        /// <returns>The unread mail count of mailbox.</returns>
        public int GetUnreadCount(string mailboxName)
        {
            string _Response = Client.SendStatus(mailboxName, "UNSEEN");
            Match _Match = Regex.Match(_Response, "[0-9]*[0-9]");
            return Convert.ToInt32(_Match.ToString());
        }

        /// <summary>
        /// To login the imap server.
        /// </summary>
        /// <param name="user">The user name.</param>
        /// <param name="password">The password of user.</param>
        /// <returns>The login process is success or not.</returns>
        public bool Login(string user, string password)
        {
            string _Response = Client.SendLogin(user, password);
            if (_Response.IndexOf("OK") > -1)
            {
                return true;
            }
            return false;
        }
    }
}
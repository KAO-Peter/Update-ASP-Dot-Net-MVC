using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace YoungCloud.Net.Mail
{
    /// <summary>
    /// The client utility of IMap Kiranacal.
    /// </summary>
    /// <remarks>
    /// Reference : <a href="http://www.faqs.org/rfcs/rfc3501.html">RFC 3501 - INTERNET MESSAGE ACCESS KiranaCOL - VERSION 4rev1</a>.
    /// </remarks>
    public partial class MapClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapClient">IMapClient</see> class.
        /// </summary>
        public MapClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapClient">IMapClient</see> class.
        /// </summary>
        /// <param name="host">The host of imap server.</param>
        /// <param name="port">The port of service.</param>
        public MapClient(string host, int port)
        {
            Connect(host, port);
        }

        private TcpClient Client
        {
            get;
            set;
        }

        /// <summary>
        /// Connect to imap server.
        /// </summary>
        /// <param name="host">The host of imap server.</param>
        /// <param name="port">The port of service.</param>
        /// <returns>The return string from server.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string Connect(string host, int port)
        {
            Client = new TcpClient(host, port);
            NetworkStream _TcpStream = Client.GetStream();
            SslStream _SslStream = new SslStream(_TcpStream);
            _SslStream.AuthenticateAsClient(host);
            if (_SslStream.IsAuthenticated)
            {
                Stream = _SslStream;
            }
            else
            {
                Stream = _TcpStream;
            }
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);
            byte[] _Data = new byte[Client.ReceiveBufferSize];
            int _Length = Stream.Read(_Data, 0, _Data.Length);
            return Encoding.ASCII.GetString(_Data, 0, _Data.Length).TrimEnd();
        }

        /// <summary>
        /// Disconnect to the server.
        /// </summary>
        /// <returns>The return string from server.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string Disconnect()
        {
            return SendLogout();
        }

        private string GetResponse(string tag)
        {
            string _Return = "";
            StringBuilder _Builder = new StringBuilder();
            while (true)
            {
                byte[] _Data = new byte[Client.ReceiveBufferSize];
                int _Length = Stream.Read(_Data, 0, _Data.Length);
                _Return = Encoding.ASCII.GetString(_Data, 0, _Data.Length).TrimEnd();
                _Builder.Append(_Return);
                if (_Return.IndexOf(tag) > -1)
                {
                    break;
                }
            }
            return _Builder.ToString();
        }

        /// <summary>
        /// Service have connected or not.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (Client == null)
                {
                    return false;
                }
                return Client.Connected;
            }
        }

        /// <summary>
        /// The CAPABILITY command requests a listing of capabilities that the server supports.
        /// </summary>
        /// <returns>
        ///     OK - capability completed.<br/>
        ///     BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendCapability()
        {
            return SendCommand("CAPABILITY");
        }

        /// <summary>
        /// The CHECK command requests a checkpoint of the currently selected mailbox.
        /// </summary>
        /// <returns>
        ///     OK - check completed.<br/>
        ///     BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendCheck()
        {
            return SendCommand("CHECK");
        }

        /// <summary>
        /// The CLOSE command permanently removes all messages that have the
        /// \Deleted flag set from the currently selected mailbox,
        /// and returns to the authenticated state from the selected state.
        /// </summary>
        /// <returns>
        ///     OK - close completed, now in authenticated state.<br/>
        ///     BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendClose()
        {
            return SendCommand("CLOSE");
        }

        /// <summary>
        /// Send a client command to imap server.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>The return string from server.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendCommand(string commandText)
        {
            return SendCommand("$", commandText);
        }

        /// <summary>
        /// Send a client command to imap server.
        /// </summary>
        /// <param name="tag">The start tag string of command.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>The return string from server.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendCommand(string tag, string commandText)
        {
            Writer.WriteLine(string.Format("{0} {1}", tag, commandText));
            Writer.Flush();
            return GetResponse(tag);
        }

        /// <summary>
        /// The CREATE command creates a mailbox with the given name.
        /// </summary>
        /// <param name="mailboxName">The name of new mailbox.</param>
        /// <returns>
        /// OK - create completed.<br/>
        /// NO - create failure: can't create mailbox with that name.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendCreate(string mailboxName)
        {
            return SendCreate(mailboxName, "");
        }

        /// <summary>
        /// The CREATE command creates a mailbox with the given name.
        /// </summary>
        /// <param name="mailboxName">The name of new mailbox.</param>
        /// <param name="parentName">The parent mailbox name of new mailbox.</param>
        /// <returns>
        /// OK - create completed.<br/>
        /// NO - create failure: can't create mailbox with that name.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendCreate(string mailboxName, string parentName)
        {
            if (string.IsNullOrEmpty(parentName))
            {
                return SendCommand(string.Format("CREATE {0}", mailboxName));
            }
            else
            {
                return SendCommand(string.Format("CREATE {0}/{1}", parentName, mailboxName));
            }
        }

        /// <summary>
        /// The DELETE command permanently removes the mailbox with the given name.
        /// </summary>
        /// <param name="mailboxName">The name of mailbox.</param>
        /// <returns>
        /// OK - delete completed.<br/>
        /// NO - delete failure: can't delete mailbox with that name.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendDelete(string mailboxName)
        {
            return SendCommand(string.Format("DELETE {0}", mailboxName));
        }

        /// <summary>
        /// The SELECT command selects a mailbox that is identified as read-only.
        /// </summary>
        /// <param name="mailboxName">Mailbox name.</param>
        /// <returns>
        /// OK - select completed, now in selected state.<br/>
        /// NO - select failure, now in authenticated state: no such mailbox, can't access mailbox.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendExamine(string mailboxName)
        {
            return SendCommand(string.Format("EXAMINE {0}", mailboxName));
        }

        /// <summary>
        /// The EXPUNGE command permanently removes all messages that have the
        /// \Deleted flag set from the currently selected mailbox.
        /// </summary>
        /// <returns>
        /// OK - expunge completed.<br/>
        /// NO - expunge failure: can't expunge (e.g., permission denied).<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendExpunge()
        {
            return SendCommand("EXPUNGE");
        }

        /// <summary>
        /// The LIST command returns a subset of names from the complete set of all names available to the client.
        /// </summary>
        /// <param name="referenceName">Reference name.</param>
        /// <param name="mailboxName">Mailbox name with possible wildcards.</param>
        /// <returns>
        /// OK - list completed.<br/>
        /// NO - list failure: can't list that reference or name.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendList(string referenceName, string mailboxName)
        {
            return SendCommand(string.Format("List {0} {1}", referenceName, mailboxName));
        }

        /// <summary>
        /// The LOGIN command identifies the client to the server and carries the plaintext password authenticating this user.
        /// </summary>
        /// <param name="user">User name.</param>
        /// <param name="password">The password of user.</param>
        /// <returns>
        ///     OK - login completed, now in authenticated state.<br/>
        ///     NO - login failure: user name or password rejected.<br/>
        ///     BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendLogin(string user, string password)
        {
            return SendCommand(string.Format("LOGIN {0} {1}", user, password));
        }

        /// <summary>
        /// The LOGOUT command informs the server that the client is done with the connection.
        /// </summary>
        /// <returns>
        ///     OK - logout completed.<br/>
        ///     BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendLogout()
        {
            return SendCommand("LOGOUT");
        }

        /// <summary>
        /// The NOOP command always succeeds. It does nothing.
        /// </summary>
        /// <returns>
        /// OK - noop completed.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendNoop()
        {
            return SendCommand("NOOP");
        }

        /// <summary>
        /// The RENAME command changes the name of a mailbox.
        /// </summary>
        /// <param name="mailboxName">Existing mailbox name.</param>
        /// <param name="newMailboxName">New mailbox name.</param>
        /// <returns>
        /// OK - rename completed.<br/>
        /// NO - rename failure: can't rename mailbox with that name,can't rename to mailbox with that name.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendRename(string mailboxName, string newMailboxName)
        {
            return SendCommand(string.Format("RENAME {0} {1}", mailboxName, newMailboxName));
        }

        /// <summary>
        /// The SELECT command selects a mailbox so that messages in the mailbox can be accessed.
        /// </summary>
        /// <param name="mailboxName">Mailbox name.</param>
        /// <returns>
        /// OK - select completed, now in selected state.<br/>
        /// NO - select failure, now in authenticated state: no such mailbox, can't access mailbox.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendSelect(string mailboxName)
        {
            return SendCommand(string.Format("SELECT {0}", mailboxName));
        }

        /// <summary>
        /// The STATUS command requests the status of the indicated mailbox.
        /// </summary>
        /// <param name="name">Mailbox name.</param>
        /// <param name="itemNames">Status data item names.</param>
        /// <returns>
        /// OK - status completed.<br/>
        /// NO - status failure: no status for that name.<br/>
        /// BAD - command unknown or arguments invalid.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public string SendStatus(string name, string itemNames)
        {
            return SendCommand(string.Format("STATUS {0} ({1})", name, itemNames));
        }

        private StreamReader Reader
        {
            get;
            set;
        }

        private Stream Stream
        {
            get;
            set;
        }

        private StreamWriter Writer
        {
            get;
            set;
        }
    }
}
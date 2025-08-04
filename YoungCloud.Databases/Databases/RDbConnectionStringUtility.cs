using System.Text.RegularExpressions;
using YoungCloud.Security.Cryptography;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The utility of database connection string.
    /// </summary>
    public abstract class RDbConnectionStringUtility : IRDbConnectionStringUtility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RDbConnectionStringUtility">RDbConnectionStringUtility</see> class.
        /// </summary>
        protected RDbConnectionStringUtility()
        {
        }

        /// <summary>
        /// To decrypt the password part of connection string.
        /// </summary>
        /// <param name="connectionString">The connection string with cipher password text.</param>
        /// <param name="utility">The reversiable cryptography utility.</param>
        /// <returns>The connection string with plain password text.</returns>
        public virtual string ToPasswordDecrypted(string connectionString, IReversiableCryptographyUtility utility)
        {
            if (!connectionString.EndsWith(";")) { connectionString += ";"; }
            Match _Match = Regex.Match(connectionString, "(Pwd=(?<Pwd>[^;]*);)|(Password=(?<Pwd>[^;]*);)", RegexOptions.IgnoreCase);
            string _Pwd = _Match.Groups["Pwd"].ToString();
            if (string.IsNullOrEmpty(_Pwd))
            {
                return connectionString;
            }
            if (utility == null)
            {
                return connectionString;
            }
            return connectionString.Replace(_Match.Value, _Match.Value.Replace(_Pwd, utility.DecryptString(_Pwd)));
        }

        /// <summary>
        /// To encrypt the password part of connection string.
        /// </summary>
        /// <param name="connectionString">The connection string with plain password text.</param>
        /// <param name="utility">The reversiable cryptography utility.</param>
        /// <returns>The connection string with cipher password text.</returns>
        public virtual string ToPasswordEncrypted(string connectionString, IReversiableCryptographyUtility utility)
        {
            if (!connectionString.EndsWith(";")) { connectionString += ";"; }
            Match _Match = Regex.Match(connectionString, "(Pwd=(?<Pwd>[^;]*);)|(Password=(?<Pwd>[^;]*);)", RegexOptions.IgnoreCase);
            string _Pwd = _Match.Groups["Pwd"].ToString();
            if (string.IsNullOrEmpty(_Pwd))
            {
                return connectionString;
            }
            if (utility == null)
            {
                return connectionString;
            }
            string _CipherText = utility.EncryptString(_Pwd);
            string _Password = _Match.Value.Replace(_Pwd, _CipherText);
            return connectionString.Replace(_Match.Value, _Password);
        }
    }
}
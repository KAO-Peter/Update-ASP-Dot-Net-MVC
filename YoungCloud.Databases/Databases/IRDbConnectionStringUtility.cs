
using YoungCloud.Security.Cryptography;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The definition of database connection string utility.
    /// </summary>
    public interface IRDbConnectionStringUtility
    {
        /// <summary>
        /// To decrypt the password part of connection string.
        /// </summary>
        /// <param name="connectionString">The connection string with cipher password text.</param>
        /// <param name="utility">The reversiable cryptography utility.</param>
        /// <returns>The connection string with plain password text.</returns>
        string ToPasswordDecrypted(string connectionString, IReversiableCryptographyUtility utility);
        /// <summary>
        /// To encrypt the password part of connection string.
        /// </summary>
        /// <param name="connectionString">The connection string with plain password text.</param>
        /// <param name="utility">The reversiable cryptography utility.</param>
        /// <returns>The connection string with cipher password text.</returns>
        string ToPasswordEncrypted(string connectionString, IReversiableCryptographyUtility utility);
    }
}
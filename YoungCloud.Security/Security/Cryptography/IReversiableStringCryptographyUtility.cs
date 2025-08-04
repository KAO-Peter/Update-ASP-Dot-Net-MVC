using System.Security;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The definition of utility that is used for string type cryptographic.
    /// </summary>
    public interface IReversiableStringCryptographyUtility : IIrreversiableStringCryptographyUtility
    {
        /// <summary>
        /// Decrypt the content inside <see cref="SecureString">SecureString</see> object.
        /// </summary>
        /// <param name="secureString">The instance of <see cref="SecureString">SecureString</see>.</param>
        /// <returns>The <see cref="SecureString">SecureString</see> object that content have been decrypted.</returns>
        SecureString DecryptSecureString(SecureString secureString);
        /// <summary>
        /// Decrypt cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns>The plain text.</returns>
        string DecryptString(string cipherText);
    }
}
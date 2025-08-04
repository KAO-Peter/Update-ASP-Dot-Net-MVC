using System;
using System.Security;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The definition of utility that is used for irreversible cryptographic.
    /// </summary>
    public interface IIrreversiableStringCryptographyUtility : IDisposable
    {
        /// <summary>
        /// Encrypt the content inside <see cref="SecureString">SecureString</see> object.
        /// </summary>
        /// <param name="secureString">The instance of <see cref="SecureString">SecureString</see>.</param>
        /// <returns>The <see cref="SecureString">SecureString</see> object the content have been encrypted.</returns>
        SecureString EncryptSecureString(SecureString secureString);
        /// <summary>
        /// Encrypt plain text to clipher text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>The clipher text.</returns>
        string EncryptString(string plainText);
    }
}
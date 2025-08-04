using System;
using System.Text;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The base object ob reversiable cryptography utilities.
    /// </summary>
    public abstract class ReversiableCryptographyUtilityBase : ReversiableStringCryptographyUtilityBase, IReversiableCryptographyUtility
    {
        /// <summary>
        /// Decrypt byte data.
        /// </summary>
        /// <param name="data">The data wanna to decrypt.</param>
        /// <returns>The data have been decrypted.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public abstract byte[] Decrypt(byte[] data);

        /// <summary>
        /// Decrypt cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns>The plain text.</returns>
        public override string DecryptString(string cipherText)
        {
            return UnicodeEncoding.Unicode.GetString(Decrypt(Convert.FromBase64String(cipherText)));
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoked from Dispose method or not.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Encrypt byte data.
        /// </summary>
        /// <param name="data">The data wanna to encrypt.</param>
        /// <returns>The data have been encrypted.</returns>
        public abstract byte[] Encrypt(byte[] data);

        /// <summary>
        /// Encrypt plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>The cipher text.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public override string EncryptString(string plainText)
        {
            return Convert.ToBase64String(Encrypt(UnicodeEncoding.Unicode.GetBytes(plainText)));
        }
    }
}
using System.Security;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The base object ob reversiable string cryptography utilities.
    /// </summary>
    public abstract class ReversiableStringCryptographyUtilityBase : IrreversiableStringCryptographyUtilityBase, IReversiableStringCryptographyUtility
    {
        /// <summary>
        /// Decrypt the content inside <see cref="SecureString">SecureString</see> object.
        /// </summary>
        /// <param name="secureString">The instance of <see cref="SecureString">SecureString</see>.</param>
        /// <returns>The <see cref="SecureString">SecureString</see> object that content have been decrypted.</returns>
        public virtual SecureString DecryptSecureString(SecureString secureString)
        {
            return SecureStringUtility.ToSecureString(DecryptString(secureString.ToUnsecureString()).ToCharArray());
        }

        /// <summary>
        /// Decrypt cipher text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns>The plain text.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public abstract string DecryptString(string cipherText);

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoked from Dispose method or not.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
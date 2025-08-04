using System.Security;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The base object ob irreversiable string cryptography utilities.
    /// </summary>
    public abstract class IrreversiableStringCryptographyUtilityBase : Disposable, IIrreversiableStringCryptographyUtility
    {
        /// <summary>
        /// Encrypt the content inside <see cref="SecureString">SecureString</see> object.
        /// </summary>
        /// <param name="secureString">The instance of <see cref="SecureString">SecureString</see>.</param>
        /// <returns>The <see cref="SecureString">SecureString</see> object that content have been encrypted.</returns>
        public virtual SecureString EncryptSecureString(SecureString secureString)
        {
            return SecureStringUtility.ToSecureString(EncryptString(secureString.ToUnsecureString()).ToCharArray());
        }

        /// <summary>
        /// Encrypt plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>The cipher text.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public abstract string EncryptString(string plainText);

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
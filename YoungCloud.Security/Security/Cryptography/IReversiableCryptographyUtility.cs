
namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The definition of utility that is used for reversiable cryptographic.
    /// </summary>
    public interface IReversiableCryptographyUtility : IReversiableStringCryptographyUtility
    {
        /// <summary>
        /// Decrypt byte data.
        /// </summary>
        /// <param name="data">The data wanna to decrypt.</param>
        /// <returns>The data have been decrypted.</returns>
        byte[] Decrypt(byte[] data);
        /// <summary>
        /// Encrypt byte data.
        /// </summary>
        /// <param name="data">The data wanna to encrypt.</param>
        /// <returns>The data have been encrypted.</returns>
        byte[] Encrypt(byte[] data);
    }
}
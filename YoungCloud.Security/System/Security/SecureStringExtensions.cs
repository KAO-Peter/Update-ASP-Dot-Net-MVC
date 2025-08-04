using YoungCloud.Security;

namespace System.Security
{
    /// <summary>
    /// The extension methods of SecureString object.
    /// </summary>
    public static class SecureStringExtensions
    {
        /// <summary>
        /// Converts secured string to unsecure string.
        /// </summary>
        /// <param name="secureString">The secured string.</param>
        /// <returns>The unsecured string.</returns>
        public static string ToUnsecureString(this SecureString secureString)
        {
            return SecureStringUtility.ToUnsecureString(secureString);
        }
    }
}
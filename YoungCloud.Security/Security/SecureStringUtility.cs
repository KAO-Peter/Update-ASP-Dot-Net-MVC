using System;
using System.Runtime.InteropServices;
using System.Security;

namespace YoungCloud.Security
{
    /// <summary>
    /// The utility of SecureString object.
    /// </summary>
    public partial class SecureStringUtility
    {
        /// <summary>
        /// Converts unsecure string to secured string.
        /// </summary>
        /// <param name="unsecureString">The string unsecured.</param>
        /// <returns>The string secured.</returns>
        public static SecureString ToSecureString(string unsecureString)
        {
            if (string.IsNullOrEmpty(unsecureString))
            {
                throw new ArgumentNullException("unsecureString");
            }
            return ToSecureString(unsecureString.ToCharArray());
        }

        /// <summary>
        /// Converts char array to secured string.
        /// </summary>
        /// <param name="unsecureChars">The unsecured char array.</param>
        /// <returns>The string secured.</returns>
        public static SecureString ToSecureString(char[] unsecureChars)
        {
            if (unsecureChars == null)
            {
                throw new ArgumentNullException("unsecureChars");
            }
            else if (unsecureChars.Length == 0)
            {
                throw new ArgumentNullException("unsecureChars");
            }
            SecureString _RtnSecureString = new SecureString();
            for (int i = 0; i < unsecureChars.Length; i++)
            {
                _RtnSecureString.AppendChar(unsecureChars[i]);
                unsecureChars[i] = '0';
            }
            _RtnSecureString.MakeReadOnly();
            return _RtnSecureString;
        }

        /// <summary>
        /// Converts secured string to unsecure string.
        /// </summary>
        /// <param name="secureString">The secured string.</param>
        /// <returns>The unsecured string.</returns>
        public static string ToUnsecureString(SecureString secureString)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException("secureString");
            }

            IntPtr _StringPoint = IntPtr.Zero;
            try
            {
                _StringPoint = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(_StringPoint);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(_StringPoint);
            }
        }
    }
}
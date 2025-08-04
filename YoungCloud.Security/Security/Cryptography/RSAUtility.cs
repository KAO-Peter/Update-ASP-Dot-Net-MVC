using System;
using System.Security.Cryptography;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The utility of RSA symmetric encryption algorithm.
    /// </summary>
    public class RSAUtility : ReversiableCryptographyUtilityBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RSAUtility">RSAUtility</see> class.
        /// </summary>
        /// <param name="xmlKey">The key of RSA symmetric encryption algorithm.</param>
        public RSAUtility(string xmlKey)
        {
            Key = xmlKey;
        }

        /// <summary>
        /// Decrypt byte data.
        /// </summary>
        /// <param name="data">The data wanna to decrypt.</param>
        /// <returns>The data have been decrypted.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public override byte[] Decrypt(byte[] data)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RSAUtility).Name);
            }

            using (RSACryptoServiceProvider _RSA = new RSACryptoServiceProvider())
            {
                _RSA.FromXmlString(Key);
                return _RSA.Decrypt(data, false);
            }
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoked from Dispose method or not.</param>
        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Key = string.Empty;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Encrypt byte data.
        /// </summary>
        /// <param name="data">The data wanna to encrypt.</param>
        /// <returns>The data have been encrypted.</returns>
        /// <exception cref="ObjectDisposedException">The exception of try to use a disposed utility.</exception>
        public override byte[] Encrypt(byte[] data)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RSAUtility).Name);
            }

            using (RSACryptoServiceProvider _RSA = new RSACryptoServiceProvider())
            {
                _RSA.FromXmlString(Key);
                return _RSA.Encrypt(data, false);
            }
        }

        private string Key
        {
            get;
            set;
        }
    }
}
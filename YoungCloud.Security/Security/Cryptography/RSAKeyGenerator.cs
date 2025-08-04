using System.Security.Cryptography;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The key generator of RSA asymmetric encryption algorithm.
    /// </summary>
    public class RSAKeyGenerator : Disposable, IAsymmetricKeyGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RSAKeyGenerator">RSAKeyGenerator</see> class.
        /// </summary>
        public RSAKeyGenerator()
        {
            using (RSACryptoServiceProvider _RSA = new RSACryptoServiceProvider())
            {
                PrivateKey = _RSA.ToXmlString(true);
                PublicKey = _RSA.ToXmlString(false);
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
                PublicKey = string.Empty;
                PrivateKey = string.Empty;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// The private key of RSA asymmetric encryption algorithm.
        /// </summary>
        public string PrivateKey
        {
            get;
            private set;
        }

        /// <summary>
        /// The public key of RSA asymmetric encryption algorithm.
        /// </summary>
        public string PublicKey
        {
            get;
            private set;
        }
    }
}
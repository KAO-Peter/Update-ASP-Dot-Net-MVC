using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HRProtal.Helper
{
    [Serializable]
    public class RSAKeyPair
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
    }

    public class RSAHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RSAKeyGenerator">RSAKeyGenerator</see> class.
        /// </summary>
        public RSAKeyPair GenerateRSAKey()
        {
            RSAKeyPair _key = new RSAKeyPair();
            using (RSACryptoServiceProvider _RSA = new RSACryptoServiceProvider())
            {
                _key.PrivateKey = _RSA.ToXmlString(true);
                _key.PublicKey = _RSA.ToXmlString(false);
            }
            return _key;
        }
    }
}

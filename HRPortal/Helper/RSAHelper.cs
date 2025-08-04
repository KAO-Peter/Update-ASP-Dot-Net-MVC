using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YoungCloud.Security.Cryptography;

namespace HRProtal.Core
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
        public static RSAKeyPair GenerateRSAKey()
        {
            RSAKeyPair _key = new RSAKeyPair();
            using (RSAKeyGenerator _generator = new RSAKeyGenerator())
            {
                _key.PrivateKey = _generator.PrivateKey;
                _key.PublicKey = _generator.PublicKey;
            }
            return _key;
        }

        public static string Encrypt(string text, string key)
        {
            using (RSAUtility _rsa = new RSAUtility(key))
            {
                return _rsa.EncryptString(text);
            }
        }

        public static string Decrypt(string text, string key)
        {
            using (RSAUtility _rsa = new RSAUtility(key))
            {
                return _rsa.DecryptString(text);
            }
        }
    }
}

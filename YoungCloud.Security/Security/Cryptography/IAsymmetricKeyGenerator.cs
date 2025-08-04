using System;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The definition of asymmetric encryption algorithm key generator.
    /// </summary>
    public interface IAsymmetricKeyGenerator : IDisposable
    {
        /// <summary>
        /// The private key.
        /// </summary>
        string PrivateKey { get; }
        /// <summary>
        /// The public key.
        /// </summary>
        string PublicKey { get; }
    }
}
using System;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The key pack of symmetric encryption algorithm.
    /// </summary>
    public interface ISymmetricKeyPack : IDisposable
    {
        /// <summary>
        /// The iv of encryption algorithm.
        /// </summary>
        byte[] IV { get; }
        /// <summary>
        /// The key of encryption algorithm.
        /// </summary>
        byte[] Key { get; }
    }
}
using System;

namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The key generator definition of symmetric encryption algorithm.
    /// </summary>
    public interface ISymmetricKeyGenerator : IDisposable
    {
        /// <summary>
        /// Convert Key or initialization vector (IV) string back to byte array.
        /// </summary>
        /// <param name="value">The value of Key or Initialization vector (IV) string.</param>
        /// <returns>The byte array of Key or Initialization vector (IV).</returns>
        byte[] ConvertToBytes(string value);
        /// <summary>
        /// Convert Key or initialization vector (IV) byte array to string.
        /// </summary>
        /// <param name="value">The Key or initialization vector (IV) byte array.</param>
        /// <returns>The Key or initialization vector (IV) string.</returns>
        string ConvertToString(byte[] value);
        /// <summary>
        /// Generates a random initialization vector (IV) to use for the algorithm.
        /// </summary>
        /// <returns>Initialization vector (IV).</returns>
        byte[] GenerateIV();
        /// <summary>
        /// Generates a random initialization vector (IV) string to use for the algorithm.
        /// </summary>
        /// <returns>Initialization vector (IV).</returns>
        string GenerateIVString();
        /// <summary>
        ///  Generates a random key to use for the algorithm.
        /// </summary>
        /// <returns>Key.</returns>
        byte[] GenerateKey();
        /// <summary>
        ///  Generates a random key string to use for the algorithm.
        /// </summary>
        /// <returns>Key.</returns>
        string GenerateKeyString();
        /// <summary>
        /// The Initialization vector(IV) of encryption algorithm.
        /// </summary>
        byte[] IV { get; }
        /// <summary>
        /// The key of encryption algorithm.
        /// </summary>
        byte[] Key { get; }
    }
}
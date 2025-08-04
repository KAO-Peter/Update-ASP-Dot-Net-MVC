
namespace YoungCloud.Security.Cryptography
{
    /// <summary>
    /// The factory of reversiable cryptography utilities.
    /// </summary>
    public static class ReversiableCryptographyUtilityFactory
    {
        /// <summary>
        /// Get the instance of IReversiableCryptographyUtility.
        /// </summary>
        /// <param name="type">The algorithm type.</param>
        /// <param name="key">The key of algorithm.</param>
        /// <returns>The instance of IReversiableCryptographyUtility</returns>
        public static IReversiableCryptographyUtility Get(ReversiableCryptographyAlgorithmType type, string key)
        {
            switch (type)
            {
                case ReversiableCryptographyAlgorithmType.RSA:
                    return new RSAUtility(key);
                default:
                    return null;
            }
        }
    }
}
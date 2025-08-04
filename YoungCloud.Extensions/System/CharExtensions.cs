namespace System
{
    /// <summary>
    /// The extension methods of <see cref="Char">Char</see> object.
    /// </summary>
    public static class CharExtensions
    {
        /// <summary>
        /// Convert char to ASCII code.
        /// </summary>
        /// <param name="chr">The value of char.</param>
        /// <returns>The ASCII code of char.</returns>
        public static int ToAsc(this char chr)
        {
            return Convert.ToInt32(chr);
        }

        /// <summary>
        /// Convert char to Hex string.
        /// </summary>
        /// <param name="chr">The char.</param>
        /// <returns>The Hex string of char.</returns>
        public static string ToHex(this char chr)
        {
            return String.Format("{0:X}", chr.ToAsc());
        }
    }
}
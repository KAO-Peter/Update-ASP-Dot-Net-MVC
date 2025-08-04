namespace System
{
    /// <summary>
    /// The extension mthods of <see cref="Byte">Byte</see> object.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// To concatenate two byte array.
        /// </summary>
        /// <param name="source">The source byte array.</param>
        /// <param name="plus">The extra byte array.</param>
        /// <returns>The concatenation byte array of two.</returns>
        public static byte[] Concat(this byte[] source, byte[] plus)
        {
            if (plus == null)
            {
                return source;
            }
            else if (source == null && plus != null)
            {
                return plus;
            }
            byte[] _NewSource = new byte[source.Length + plus.Length];
            source.CopyTo(_NewSource, 0);
            plus.CopyTo(_NewSource, source.Length);
            return _NewSource;
        }
    }
}
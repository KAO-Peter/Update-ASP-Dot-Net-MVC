namespace System.Collections
{
    /// <summary>
    /// The extension methods of <see cref="BitArray">BitArray</see> object.
    /// </summary>
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Convert BitArray to Byte array.
        /// </summary>
        /// <param name="bits">The instance of BitArray.</param>
        /// <returns>The instance of Byte array.</returns>
        public static byte[] ToBytes(this BitArray bits)
        {
            if (Math.IEEERemainder(bits.Length, 8) != 0 || bits.Length == 0)
            {
                throw new ArgumentException("The length of BitArray is incorrect.");
            }
            byte[] _Bytes = new byte[bits.Length / 8];
            int _Index = 0;
            int _Value = 0;
            for (int i = 0; i < bits.Length; i += 8)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i + j < bits.Length)
                    {
                        _Value += bits[i + j] == true ? (int)Math.Pow(2, j) : 0;
                    }
                }
                _Bytes[_Index] = Convert.ToByte(_Value);
                _Value = 0;
                _Index++;
            }
            return _Bytes;
        }
    }
}
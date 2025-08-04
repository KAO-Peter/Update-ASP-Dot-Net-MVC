using System.IO;

namespace System.Runtime.Serialization.Formatters.Binary
{
    /// <summary>
    /// The extension methods of <see cref="BinaryFormatter">BinaryFormatter</see> object.
    /// </summary>
    public static class BinaryFormatterExtensions
    {
        /// <summary>
        /// Convert bytes array to object instance.
        /// </summary>
        /// <param name="formatter">The instance of <see cref="BinaryFormatter">BinaryFormatter</see>.</param>
        /// <param name="bytes">The byte array of object.</param>
        /// <returns>object instance.</returns>
        public static object ToObject(this BinaryFormatter formatter, byte[] bytes)
        {
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                _MemoryStream.Write(bytes, 0, bytes.Length);
                _MemoryStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(_MemoryStream);
            }
        }

        /// <summary>
        /// Convert object to byte array.
        /// </summary>
        /// <param name="formatter">The instance of <see cref="BinaryFormatter">BinaryFormatter</see>.</param>
        /// <param name="data">The instance of object.</param>
        /// <returns>object data in byte array.</returns>
        public static byte[] ToBytes(this BinaryFormatter formatter, object data)
        {
            if (data == null)
            {
                return null;
            }
            using (MemoryStream _MemoryStream = new MemoryStream())
            {
                formatter.Serialize(_MemoryStream, data);
                return _MemoryStream.ToArray();
            }
        }
    }
}
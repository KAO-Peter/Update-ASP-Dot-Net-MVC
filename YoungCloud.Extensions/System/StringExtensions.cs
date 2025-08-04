using System.Text.RegularExpressions;

namespace System
{
    /// <summary>
    /// The extension mthods of <see cref="String">String</see> object.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// To add a string item into array.
        /// </summary>
        /// <param name="source">The source string array.</param>
        /// <param name="plus">The string wanna to add into array.</param>
        /// <returns>The new string array.</returns>
        public static string[] Add(this string[] source, string plus)
        {
            if (string.IsNullOrEmpty(plus))
            {
                return source;
            }
            else if (source == null && !string.IsNullOrEmpty(plus))
            {
                return new string[] { plus };
            }
            string[] _NewSource = new string[source.Length + 1];
            source.CopyTo(_NewSource, 0);
            _NewSource[_NewSource.Length - 1] = plus;
            return _NewSource;
        }

        /// <summary>
        /// To concatenate two string array.
        /// </summary>
        /// <param name="source">The source string array.</param>
        /// <param name="plus">The extra string array.</param>
        /// <returns>The concatenation string array of two.</returns>
        public static string[] Concat(this string[] source, string[] plus)
        {
            if (plus == null)
            {
                return source;
            }
            else if (source == null && plus != null)
            {
                return plus;
            }
            string[] _NewSource = new string[source.Length + plus.Length];
            source.CopyTo(_NewSource, 0);
            plus.CopyTo(_NewSource, source.Length);
            return _NewSource;
        }

        /// <summary>
        /// Replaces each format item in a specified String with the text equivalent of a corresponding object's value.
        /// </summary>
        /// <param name="source">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the String equivalent of the corresponding instances of Object in args.</returns>
        public static string FormatWith(this string source, params object[] args)
        {
            return FormatWith(source, null, args);
        }

        /// <summary>
        /// Replaces each format item in a specified String with the text equivalent of a corresponding object's value.
        /// </summary>
        /// <param name="source">A String containing zero or more format items.</param>
        /// <param name="provider">An IFormatProvider that supplies culture-specific formatting information.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the String equivalent of the corresponding instances of Object in args.</returns>
        public static string FormatWith(this string source, IFormatProvider provider, params object[] args)
        {
            return string.Format(provider, source, args);
        }

        /// <summary>
        /// Get string from another.
        /// </summary>
        /// <param name="str">The string to be parsed.</param>
        /// <param name="start">The start index of the start.</param>
        /// <param name="length">The substring length.</param>
        /// <returns>The substring.</returns>
        public static string Mid(this string str, int start, int length)
        {
            checked
            {
                if (start <= 0)
                {
                    throw new ArgumentException("start");
                }
                if (length < 0)
                {
                    throw new ArgumentException("Length");
                }
                if (length == 0 || str == null)
                {
                    return "";
                }
                int _Length = str.Length;
                if (start > _Length)
                {
                    return "";
                }
                if (start + length > _Length)
                {
                    return str.Substring(start - 1);
                }
                return str.Substring(start - 1, length);
            }
        }

        /// <summary>
        /// Convert string to ASCII code array.
        /// </summary>
        /// <param name="str">The string that wanna to convert.</param>
        /// <returns>The ASCII code array of string.</returns>
        public static int[] ToAsc(this string str)
        {
            int[] _RtnAsc = new int[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                _RtnAsc[i] = str[i].ToAsc();
            }
            return _RtnAsc;
        }

        /// <summary>
        /// Convert string to Hex string.
        /// </summary>
        /// <param name="str">The string wanna to be convert.</param>
        /// <returns>The hex string array.</returns>
        public static string[] ToHex(this string str)
        {
            string[] _RtnHex = new string[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                _RtnHex[i] = str[i].ToHex();
            }
            return _RtnHex;
        }

        /// <summary>
        /// Get the ip address from string.
        /// </summary>
        /// <param name="text">The string that want to be checked.</param>
        /// <returns>The ip address string.</returns>
        public static string GetIPAddress(this string text)
        {
            Match _Match = Regex.Match(text, @"((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)");
            if (_Match.Success)
            {
                return _Match.Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Check the input email address is legal or not.
        /// </summary>
        /// <param name="mailAddress">The mail address for check.</param>
        /// <returns>The mail address is legal or not.</returns>
        public static bool IsLegalMailAddress(this string mailAddress)
        {
            return Regex.IsMatch(mailAddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}
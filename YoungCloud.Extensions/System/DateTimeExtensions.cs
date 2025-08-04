using System.Globalization;

namespace System
{
    /// <summary>
    /// The extension mthods of <see cref="DateTime">DateTime</see> object.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// To get the utc offset of DateTime.
        /// </summary>
        /// <param name="datetime">The source DateTime instance.</param>
        /// <returns>The utc offset of DateTime.</returns>
        public static TimeSpan GetUtcOffset(this DateTime datetime)
        {
            return TimeZoneInfo.Local.GetUtcOffset(datetime);
        }

        /// <summary>
        /// To get the utc offset text format of DateTime.
        /// </summary>
        /// <param name="datetime">The source DateTime instance.</param>
        /// <returns>The utc offset text format of DateTime.</returns>
        public static string GetUtcOffsetText(this DateTime datetime)
        {
            TimeSpan _UtcOffset = datetime.GetUtcOffset();
            return string.Format("{0}:{1}:{2}",
                _UtcOffset.Hours.ToString("+00;-00", CultureInfo.InvariantCulture),
                _UtcOffset.Minutes.ToString("00;00", CultureInfo.InvariantCulture),
                _UtcOffset.Seconds.ToString("00;00", CultureInfo.InvariantCulture)
                );
        }
    }
}
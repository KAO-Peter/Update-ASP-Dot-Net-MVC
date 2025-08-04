namespace System
{
    /// <summary>
    /// The extension methods of <see cref="Double">Double</see> object.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Get the interger part of assigned double.
        /// </summary>
        /// <param name="value">The value of double.</param>
        /// <returns>The interger part of double value.</returns>
        public static double ToFix(this double value)
        {
            if (value >= 0.0)
            {
                return Math.Floor(value);
            }
            return -Math.Floor(-value);
        }
    }
}
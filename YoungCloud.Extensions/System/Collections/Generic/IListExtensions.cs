namespace System.Collections.Generic
{
    /// <summary>
    /// The extension methods of <see cref="IList">IList</see> object.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Add item array into IList.
        /// </summary>
        /// <typeparam name="T">The type of items.</typeparam>
        /// <param name="list">The instance of IList.</param>
        /// <param name="items">The item array instance.</param>
        public static void AddRange<T>(this IList<T> list, T[] items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// Convert IList to Array.
        /// </summary>
        /// <typeparam name="T">The type of items.</typeparam>
        /// <param name="list">The instance of IList.</param>
        /// <returns>The type instance array.</returns>
        public static T[] ToArray<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return null;
            }
            T[] _RtnArray = new T[list.Count];
            list.CopyTo(_RtnArray, 0);
            return _RtnArray;
        }
    }
}
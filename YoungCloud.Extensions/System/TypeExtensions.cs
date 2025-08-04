namespace System
{
    /// <summary>
    /// The extension mthods of <see cref="Type">Type</see> object.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Type is nullable or not.
        /// </summary>
        /// <param name="type">The source type instance.</param>
        /// <returns>Type is nullable or not.</returns>
        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType)
            {
                return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
            }
            else
            {
                return true;
            }
        }
    }
}
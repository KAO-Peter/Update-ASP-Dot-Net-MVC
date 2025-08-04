using System.Collections;
using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The collection of IDbDataParameter.
    /// </summary>
    public interface IDbDataParameterCollection : ICollection
    {
        /// <summary>
        /// Add IDbDataParameter instance into collection.
        /// </summary>
        /// <param name="parameter">The instance of IDbDataParameter.</param>
        void Add(IDbDataParameter parameter);
        /// <summary>
        /// Add IDbDataParameter instance range into collection.
        /// </summary>
        /// <param name="parameters">The instance array of IDbDataParameter.</param>
        void AddRange(IDbDataParameter[] parameters);
        /// <summary>
        /// To clear data in collection.
        /// </summary>
        void Clear();
        /// <summary>
        /// Get or set the instance of IDbDataParameter.
        /// </summary>
        /// <param name="index">The index of instance in the collection.</param>
        /// <returns>The instance of IDbDataParameter.</returns>
        IDbDataParameter this[int index] { get; set; }
        /// <summary>
        /// Conver the collection to array.
        /// </summary>
        /// <returns></returns>
        IDbDataParameter[] ToArray();
    }
}
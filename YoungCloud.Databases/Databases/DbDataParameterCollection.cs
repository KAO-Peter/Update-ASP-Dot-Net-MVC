using System;
using System.Collections;
using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The collection of IDbDataParameter.
    /// </summary>
    public class DbDataParameterCollection : CollectionBase, IDbDataParameterCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataParameterCollection">DbDataParameterCol</see> class.
        /// </summary>
        public DbDataParameterCollection()
        {
        }

        /// <summary>
        /// Add IDbDataParameter instance into collection.
        /// </summary>
        /// <param name="parameter">The instance of IDbDataParameter.</param>
        public void Add(IDbDataParameter parameter)
        {
            List.Add(parameter);
        }

        /// <summary>
        /// Add IDbDataParameter instance range into collection.
        /// </summary>
        /// <param name="parameters">The instance array of IDbDataParameter.</param>
        public void AddRange(IDbDataParameter[] parameters)
        {
            foreach (IDbDataParameter parameter in parameters)
            {
                List.Add(parameter);
            }
        }

        /// <summary>
        /// Get or set the instance of IDbDataParameter.
        /// </summary>
        /// <param name="index">The index of instance in the collection.</param>
        /// <returns>The instance of IDbDataParameter.</returns>
        public IDbDataParameter this[int index]
        {
            get
            {
                return (IDbDataParameter)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Conver the collection to array.
        /// </summary>
        /// <returns>The instance array of IDbDataParameter.</returns>
        public IDbDataParameter[] ToArray()
        {
            Array _Array = Array.CreateInstance(typeof(IDbDataParameter), List.Count);
            List.CopyTo(_Array, 0);
            return (IDbDataParameter[])_Array;
        }
    }
}
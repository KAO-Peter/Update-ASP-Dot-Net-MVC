using System;
using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The definition of Data entity base class.
    /// </summary>
    public interface IDataEntityClass : IDisposable
    {
        /// <summary>
        /// System.Data.DataRow.
        /// </summary>
        DataRow DataRow { get; set; }
    }
}
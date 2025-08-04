using System;
using System.Data.Common;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The defintiton of database access utility base object .
    /// </summary>
    public partial interface IRDbUtility : IDisposable
    {
        /// <summary>
        /// The instance of DbProviderFactory.
        /// </summary>
        DbProviderFactory Factory { get; }
    }
}
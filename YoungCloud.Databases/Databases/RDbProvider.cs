using System;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The supported database type.
    /// </summary>
    [Serializable]
    public enum RDbProvider
    {
        /// <summary>
        /// Microsoft SQL Server (SqlClient).
        /// </summary>
        MsSql = 1,
    }
}
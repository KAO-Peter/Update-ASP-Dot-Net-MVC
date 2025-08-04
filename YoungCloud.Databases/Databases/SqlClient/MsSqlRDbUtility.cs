using System;

namespace YoungCloud.Databases.SqlClient
{
    /// <summary>
    /// The database access utility via Sql provider.
    /// </summary>
    [Serializable]
    public class MsSqlRDbUtility : RDbUtility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlRDbUtility">MSSqlRdbUtility</see> class.
        /// </summary>
        /// <param name="connectionString">ConnectString of database.</param>
        public MsSqlRDbUtility(string connectionString)
            : base(RDbProvider.MsSql, connectionString)
        {
        }
    }
}
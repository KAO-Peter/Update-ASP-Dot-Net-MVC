using System;

namespace YoungCloud.Databases.SqlClient
{
    /// <summary>
    /// The object of database access utility via Sql provider that support transaction.
    /// </summary>
    [Serializable]
    public class MsSqlRDbUtilityWithTransaction : RDbUtilityWithTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlRDbUtilityWithTransaction">MSSqlWithTransactionRdbUtility</see> class.
        /// </summary>
        /// <param name="connectionString">The connection string of database.</param>
        /// <exception cref="NullConnectionStringException">This exception throws when connection string is null or empty.</exception>
        public MsSqlRDbUtilityWithTransaction(string connectionString)
            : base(RDbProvider.MsSql, connectionString)
        {
        }
    }
}
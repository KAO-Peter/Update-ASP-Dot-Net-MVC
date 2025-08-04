using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using YoungCloud.Databases.SqlClient;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The factory of Database objects.
    /// </summary>
    public sealed class RDbObjectFactory
    {
        /// <summary>
        /// Get the DbProviderFactory instance.
        /// </summary>
        /// <returns>The instance of <see cref="SqlClientFactory">SqlClientFactory</see>.</returns>
        public static DbProviderFactory GetFactory()
        {
            return GetFactory(RDbProvider.MsSql);
        }

        /// <summary>
        /// Get the DbProviderFactory instance.
        /// </summary>
        /// <param name="provider">The RDbProvider type.</param>
        /// <returns>The instance of DbProviderFactory.</returns>
        public static DbProviderFactory GetFactory(RDbProvider provider)
        {
            switch (provider)
            {
                case RDbProvider.MsSql:
                    return SqlClientFactory.Instance;
                default:
                    return SqlClientFactory.Instance;
            }
        }

        /// <summary>
        /// Get the provider type by providerName.
        /// </summary>
        /// <param name="providerName">The name of provider.</param>
        /// <returns><see cref="RDbProvider">RDbProvider</see>.</returns>
        public static RDbProvider GetProviderTypeByName(string providerName)
        {
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    return RDbProvider.MsSql;
                default:
                    return RDbProvider.MsSql;
            }
        }

        /// <summary>
        /// Get the RdbConnectionStringUtility instance.
        /// </summary>
        /// <param name="provider">The RDbProvider type.</param>
        /// <returns>The instance of IRDbConnectionStringUtility.</returns>
        public static IRDbConnectionStringUtility GetRdbConnectionStringUtility(RDbProvider provider)
        {
            switch (provider)
            {
                case RDbProvider.MsSql:
                    return new MsSqlRDbConnectionStringUtility();
                default:
                    return new MsSqlRDbConnectionStringUtility();
            }
        }

        /// <summary>
        /// Get the IRDbUtility instance.
        /// </summary>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The instance of <see cref="MsSqlRDbUtility">MsSqlRDbUtility</see>.</returns>
        public static IRDbUtility GetRDbUtility(string connectionString)
        {
            return GetRDbUtility(RDbProvider.MsSql, connectionString);
        }

        /// <summary>
        /// Get the IRDbUtility instance.
        /// </summary>
        /// <param name="provider">The RDbProvider type.</param>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The instance of IRDbUtility.</returns>
        public static IRDbUtility GetRDbUtility(RDbProvider provider, string connectionString)
        {
            switch (provider)
            {
                case RDbProvider.MsSql:
                    return new MsSqlRDbUtility(connectionString);
                default:
                    return new MsSqlRDbUtility(connectionString);
            }
        }

        /// <summary>
        /// Get the IRDbUtility instance.
        /// </summary>
        /// <param name="providerName">The provider full name.</param>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The instance of IRDbUtility.</returns>
        public static IRDbUtility GetRDbUtility(string providerName, string connectionString)
        {
            return GetRDbUtility(GetProviderTypeByName(providerName), connectionString);
        }

        /// <summary>
        /// Get the IRDbUtilityWithTransaction instance.
        /// </summary>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The instance of <see cref="MsSqlRDbUtilityWithTransaction">MsSqlRDbUtilityWithTransaction</see>.</returns>
        public static IRDbUtilityWithTransaction GetRDbUtilityWithTransaction(string connectionString)
        {
            return GetRDbUtilityWithTransaction(RDbProvider.MsSql, connectionString);
        }

        /// <summary>
        /// Get the IRDbUtilityWithTransaction instance.
        /// </summary>
        /// <param name="provider">The RDbProvider type.</param>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The instance of IRDbUtilityWithTransaction.</returns>
        public static IRDbUtilityWithTransaction GetRDbUtilityWithTransaction(RDbProvider provider, string connectionString)
        {
            switch (provider)
            {
                case RDbProvider.MsSql:
                    return new MsSqlRDbUtilityWithTransaction(connectionString);
                default:
                    return new MsSqlRDbUtilityWithTransaction(connectionString);
            }
        }

        /// <summary>
        /// Get the IRDbUtilityWithTransaction instance.
        /// </summary>
        /// <param name="providerName">The provider full name.</param>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The instance of IRDbUtilityWithTransaction.</returns>
        public static IRDbUtilityWithTransaction GetRDbUtilityWithTransaction(string providerName, string connectionString)
        {
            return GetRDbUtilityWithTransaction(GetProviderTypeByName(providerName), connectionString);
        }
    }
}
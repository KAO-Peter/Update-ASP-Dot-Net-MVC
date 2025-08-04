using System;
using System.Data.Common;

namespace YoungCloud.Databases
{
    /// <summary>
    /// To create db connection.
    /// </summary>
    public partial class DbConnectionFactory
    {
        /// <summary>
        /// Creates the specified connection string name with config setting.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the db provider.</param>
        /// <returns>Db connection.</returns>
        public static DbConnection Create(string connectionString, string providerName)
        {
            // Assume failure.
            DbConnection connection = null;

            // Create the DbProviderFactory and DbConnection. 
            try
            {
                DbProviderFactory factory =
                    DbProviderFactories.GetFactory(providerName);

                connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                // Set the connection to null if it was created. 
                if (connection != null)
                {
                    connection = null;
                }
                Console.WriteLine(ex.Message);
            }

            // Return the connection. 
            return connection;
        }
    }
}

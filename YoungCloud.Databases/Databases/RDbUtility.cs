using System.Data.Common;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The base object of database access utility.
    /// </summary>
    public abstract partial class RDbUtility : Disposable, IRDbUtility
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RDbUtility">RdbUtility</see> class.
        /// </summary>
        /// <param name="provider">The supported database type.</param>
        /// <param name="connectionString">ConnectString of database.</param>
        /// <exception cref="NullConnectionStringException">This exception throws when connection string is null or empty.</exception>
        protected RDbUtility(RDbProvider provider, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new NullConnectionStringException("ConnectionString is null or empty.");
            }
            ConnectionString = connectionString;
            Factory = RDbObjectFactory.GetFactory(provider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RDbUtility">RdbUtility</see> class.
        /// </summary>
        /// <param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="connectionString">ConnectString of database.</param>
        /// <exception cref="NullConnectionStringException">The exception throws when connection string is null or empty.</exception>
        protected RDbUtility(DbProviderFactory factory, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new NullConnectionStringException("ConnectionString is null or empty.");
            }
            ConnectionString = connectionString;
            Factory = factory;
        }

        /// <summary>
        /// ConnectString of database.
        /// </summary>
        protected string ConnectionString
        {
            get;
            private set;
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoked from Dispose method or not.</param>
        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    ConnectionString = string.Empty;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// The instance of DbProviderFactory.
        /// </summary>
        public DbProviderFactory Factory
        {
            get;
            private set;
        }
    }
}
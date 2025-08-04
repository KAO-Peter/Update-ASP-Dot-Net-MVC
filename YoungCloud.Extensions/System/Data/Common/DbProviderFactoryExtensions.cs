namespace System.Data.Common
{
    /// <summary>
    /// The extension methods of <see cref="DbProviderFactory">DbProviderFactory</see> object.
    /// </summary>
    public static class DbProviderFactoryExtensions
    {
        /// <summary>
        /// Create the DbDataAdapter instance.
        /// </summary>
        /// <param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="command">The instance of DbCommand.</param>
        /// <returns>The DbDataAdapter instance.</returns>
        public static DbDataAdapter CreateDataAdapter(this DbProviderFactory factory, DbCommand command)
        {
            DbDataAdapter _Adapter = factory.CreateDataAdapter();
            _Adapter.SelectCommand = command;
            return _Adapter;
        }

        /// <summary>
        /// Create the DbConnection instance.
        /// </summary>
        /// <param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="connectionString">The connection string of database.</param>
        /// <returns>The DbConnection instance.</returns>
        public static DbConnection CreateConnection(this DbProviderFactory factory, string connectionString)
        {
            DbConnection _Connection = factory.CreateConnection();
            _Connection.ConnectionString = connectionString;
            return _Connection;
        }

        /// <summary>
        /// Create the DbParameter instance.
        /// </summary>
        /// <param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="parameterName">The name of parameter.</param>
        /// <param name="parameterValue">The value of parameter.</param>
        /// <returns>The DbParameter instance.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when input parameterName is null or empty.</exception>
        public static DbParameter CreateParameter(this DbProviderFactory factory, string parameterName, object parameterValue)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }
            DbParameter _Paramerter = factory.CreateParameter();
            _Paramerter.ParameterName = parameterName;
            _Paramerter.Value = parameterValue;
            return _Paramerter;
        }

        /// <summary>
        /// Create the DbParameter instance.
        /// </summary>
        /// <param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="parameterName">The name of parameter.</param>
        /// <param name="parameterValue">The value of parameter.</param>
        /// <param name="dbType"><see cref="DbType">DbType</see>.</param>
        /// <param name="parameterDirection"><see cref="ParameterDirection">ParameterDirection</see>.</param>
        /// <returns>The DbParameter instance.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when input parameterName is null or empty.</exception>
        public static DbParameter CreateParameter(this DbProviderFactory factory, string parameterName, object parameterValue, DbType dbType, ParameterDirection parameterDirection)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                throw new ArgumentNullException("parameterName");
            }
            DbParameter _Parameter = CreateParameter(factory, parameterName, parameterValue);
            _Parameter.DbType = dbType;
            _Parameter.Direction = parameterDirection;
            return _Parameter;
        }

        /// <summary>
        /// Create the DbParameter instance array.
        /// </summary><param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="count">The count of DbParameter instance that wanna to create.</param>
        /// <returns>The DbParameter instance array.</returns>
        public static DbParameter[] CreateParameters(this DbProviderFactory factory, int count)
        {
            DbParameter[] _Params = new DbParameter[count];
            for (int i = 0; i < count; i++)
            {
                _Params[i] = factory.CreateParameter();
            }
            return _Params;
        }
    }
}
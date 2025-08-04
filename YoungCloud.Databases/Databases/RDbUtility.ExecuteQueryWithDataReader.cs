using System;
using System.Data;
using System.Data.Common;

namespace YoungCloud.Databases
{
    public partial class RDbUtility
    {
        /// <summary>
        /// To execute command for query.
        /// </summary>
        /// <param name="dbConnection">The connection used to get data.</param>
        /// <param name="command">The instance of CommandInfo object.</param>
        /// <returns>The result data in DataReader object.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public IDataReader ExecuteQueryWithDataReader(DbConnection dbConnection, ICommandInfo command)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RDbUtility).Name);
            }
            if (string.IsNullOrEmpty(command.CommandText))
            {
                throw new ArgumentNullException("CommandText");
            }

            DbCommand _Command = dbConnection.CreateCommand();
            _Command.CommandType = command.CommandType;
            _Command.CommandText = command.CommandText;
            if (command.Timeout != 0)
            {
                _Command.CommandTimeout = command.Timeout;
            }
            _Command.Parameters.AddRange(command.Parameters.ToArray());
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            return _Command.ExecuteReader();
        }

        /// <summary>
        /// To execute command for query by command text type.
        /// </summary>
        /// <param name="dbConnection">The connection used to get data.</param>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The result data in DataReader object.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public IDataReader ExecuteQueryWithDataReaderByCommandText(DbConnection dbConnection, string commandText, params IDbDataParameter[] parameters)
        {
            return ExecuteQueryWithDataReaderByCommandText(dbConnection, commandText, 0, parameters);
        }

        /// <summary>
        /// To execute command for query by command text type.
        /// </summary>
        /// <param name="dbConnection">The connection used to get data.</param>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The result data in DataReader object.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public IDataReader ExecuteQueryWithDataReaderByCommandText(DbConnection dbConnection, string commandText, int cmdTimeout, params IDbDataParameter[] parameters)
        {
            ICommandInfo _Command = new CommandInfoOfText(commandText, cmdTimeout, parameters);
            return ExecuteQueryWithDataReader(dbConnection, _Command);
        }

        /// <summary>
        /// To execute command for query by stored procedure type.
        /// </summary>
        /// <param name="dbConnection">The connection used to get data.</param>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The result data in DataReader object.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public IDataReader ExecuteQueryWithDataReaderByStoredProcedure(DbConnection dbConnection, string spName, params IDbDataParameter[] parameters)
        {
            return ExecuteQueryWithDataReaderByStoredProcedure(dbConnection, spName, 0, parameters);
        }

        /// <summary>
        /// To execute command for query by stored procedure type.
        /// </summary>
        /// <param name="dbConnection">The connection used to get data.</param>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The result data in DataReader object.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public IDataReader ExecuteQueryWithDataReaderByStoredProcedure(DbConnection dbConnection, string spName, int cmdTimeout, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            ICommandInfo _Command = new CommandInfoOfStoredProcedure(spName, cmdTimeout, parameters);
            return ExecuteQueryWithDataReader(dbConnection, _Command);
        }
    }
}
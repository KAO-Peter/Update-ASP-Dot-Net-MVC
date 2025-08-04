using System;
using System.Data;
using System.Data.Common;

namespace YoungCloud.Databases
{
    public partial class RDbUtility
    {
        /// <summary>
        /// To execute command without query.
        /// </summary>
        /// <param name="command">The instance of CommandInfo object.</param>
        /// <returns>The data number that been effected.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public virtual int ExecuteNonQuery(ICommandInfo command)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RDbUtility).Name);
            }
            if (string.IsNullOrEmpty(command.CommandText))
            {
                throw new ArgumentNullException("CommandTest");
            }

            using (DbConnection _Connection = Factory.CreateConnection(ConnectionString))
            {
                DbCommand _Command = _Connection.CreateCommand();
                _Command.CommandType = command.CommandType;
                _Command.CommandText = command.CommandText;
                if (command.Timeout != 0)
                {
                    _Command.CommandTimeout = command.Timeout;
                }
                _Command.Parameters.AddRange(command.Parameters.ToArray());
                _Connection.Open();
                return _Command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// To execute command without query by command text type.
        /// </summary>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The data number that been effected.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public int ExecuteNonQueryByCommandText(string commandText, params IDbDataParameter[] parameters)
        {
            return ExecuteNonQueryByCommandText(commandText, 0, parameters);
        }

        /// <summary>
        /// To execute command without query by command text type.
        /// </summary>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The data number that been effected.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public int ExecuteNonQueryByCommandText(string commandText, int cmdTimeout, params IDbDataParameter[] parameters)
        {
            ICommandInfo _Command = new CommandInfoOfText(commandText, cmdTimeout, parameters);
            return ExecuteNonQuery(_Command);
        }

        /// <summary>
        /// To execute command without query by stored procedure type.
        /// </summary>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The data number that been effected.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public int ExecuteNonQueryByStoredProcedure(string spName, params IDbDataParameter[] parameters)
        {
            return ExecuteNonQueryByStoredProcedure(spName, 0, parameters);
        }

        /// <summary>
        /// To execute command without query by stored procedure type.
        /// </summary>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The data number that been effected.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public int ExecuteNonQueryByStoredProcedure(string spName, int cmdTimeout, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            ICommandInfo _Command = new CommandInfoOfStoredProcedure(spName, cmdTimeout, parameters);
            return ExecuteNonQuery(_Command);
        }
    }
}
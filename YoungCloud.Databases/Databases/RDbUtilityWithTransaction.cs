using System;
using System.Data;
using System.Data.Common;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The base object of database access utility that support transaction.
    /// </summary>
    public abstract class RDbUtilityWithTransaction : RDbUtility, IRDbUtilityWithTransaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RDbUtilityWithTransaction">RdbUtilityWithTransaction</see> class.
        /// </summary>
        /// <param name="provider">The supported database type.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="NullConnectionStringException">This exception throws when connection string is null or empty.</exception>
        protected RDbUtilityWithTransaction(RDbProvider provider, string connectionString)
            : base(provider, connectionString)
        {   
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new NullConnectionStringException("ConnectionString is null or empty.");
            }
            Connection = Factory.CreateConnection(ConnectionString);
            Connection.Open();
            Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RDbUtilityWithTransaction">RdbUtilityWithTransaction</see> class.
        /// </summary>
        /// <param name="factory">The instance of <see cref="DbProviderFactory">DbProviderFactory</see>.</param>
        /// <param name="connectionString">ConnectString of database.</param>
        /// <exception cref="NullConnectionStringException">This exception throws when connection string is null or empty.</exception>
        protected RDbUtilityWithTransaction(DbProviderFactory factory, string connectionString)
            : base(factory, connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new NullConnectionStringException("ConnectionString is null or empty.");
            }
            Connection = Factory.CreateConnection(ConnectionString);
            Connection.Open();
            Transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
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
                    try
                    {
                        Transaction.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                    
                    try
                    {
                        Connection.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// To execute update commands.
        /// </summary>
        /// <param name="commandInfos">The array of ICommandInfo instance.</param>
        /// <returns>The effect data row count array of each command.</returns>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public override int[] ExecuteBatchUpdate(ICommandInfo[] commandInfos)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RDbUtilityWithTransaction).Name);
            }

            int[] _Result = new int[commandInfos.Length]; 
            for (int i = 0; i < commandInfos.Length; i++)
            {
                DbCommand _Command = Connection.CreateCommand();
                _Command.CommandText = commandInfos[i].CommandText;
                _Command.CommandType = commandInfos[i].CommandType;
                _Command.Parameters.AddRange(commandInfos[i].Parameters.ToArray());
                if (commandInfos[i].Timeout != 0)
                {
                    _Command.CommandTimeout = commandInfos[i].Timeout;
                }
                _Result[i] = _Command.ExecuteNonQuery();
            }
            return _Result;            
        }

        /// <summary>
        /// To execute command without query.
        /// </summary>
        /// <param name="command">The instance of CommandInfo object.</param>
        /// <returns>The data number that been effected.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public override int ExecuteNonQuery(ICommandInfo command)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RDbUtilityWithTransaction).Name);
            }
            if (string.IsNullOrEmpty(command.CommandText))
            {
                throw new ArgumentNullException("CommandText");
            }

            DbCommand _Command = Connection.CreateCommand();
            _Command.Connection = Connection;
            _Command.Transaction = Transaction;
            _Command.CommandType = command.CommandType;
            _Command.CommandText = command.CommandText;
            if (command.Timeout != 0)
            {
                _Command.CommandTimeout = command.Timeout;
            }
            _Command.Parameters.AddRange(command.Parameters.ToArray());
            return _Command.ExecuteNonQuery();            
        }

        /// <summary>
        /// To execute command without query and return output type parameters.
        /// </summary>
        /// <param name="command">The instance of CommandInfo object.</param>
        /// <returns>The collection of parameters.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public override IDataParameterCollection ExecuteNonQueryWithParameters(ICommandInfo command)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RDbUtilityWithTransaction).Name);
            }
            if (string.IsNullOrEmpty(command.CommandText))
            {
                throw new ArgumentNullException("CommandText");
            }

            DbCommand _Command = Connection.CreateCommand();
            _Command.CommandType = command.CommandType;
            _Command.CommandText = command.CommandText;
            if (command.Timeout != 0)
            {
                _Command.CommandTimeout = command.Timeout;
            }
            _Command.Parameters.AddRange(command.Parameters.ToArray());
            _Command.ExecuteNonQuery();
            return _Command.Parameters;
        }

        /// <summary>
        /// To commit transaction.
        /// </summary>
        public void Commit()
        {
            Transaction.Commit();
        }

        /// <summary>
        /// To rollback transaction.
        /// </summary>
        public void Rollback()
        {
            Transaction.Rollback();
        }

        /// <summary>
        /// Get the state of connection.
        /// </summary>
        public ConnectionState State
        {
            get
            {
                return Connection.State;
            }
        }

        /// <summary>
        /// The connection instance.
        /// </summary>
        protected DbConnection Connection { get; set; }

        /// <summary>
        /// The transaction instance.
        /// </summary>
        protected DbTransaction Transaction { get; set; }    
    }
}
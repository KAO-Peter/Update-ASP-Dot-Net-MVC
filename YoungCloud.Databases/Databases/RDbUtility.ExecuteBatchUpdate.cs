using System;
using System.Data.Common;

namespace YoungCloud.Databases
{
    public abstract partial class RDbUtility
    {
        /// <summary>
        /// To execute update commands.
        /// </summary>
        /// <param name="commandInfos">The collection of ICommandInfo instance.</param>
        /// <returns>The effect data row count array of each command.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public int[] ExecuteBatchUpdate(ICommandInfoCollection commandInfos)
        {
            if (commandInfos == null)
            {
                throw new ArgumentNullException("commandInfos");
            }
            return ExecuteBatchUpdate(commandInfos.ToArray());
        }

        /// <summary>
        /// To execute update commands.
        /// </summary>
        /// <param name="commandInfos">The array of ICommandInfo instance.</param>
        /// <returns>The effect data row count array of each command.</returns>
        /// <exception cref="ArgumentNullException">This exception throws when argument is null.</exception>
        /// <exception cref="ObjectDisposedException">This exception throws when try to execute database command with a disposed utility.</exception>
        public virtual int[] ExecuteBatchUpdate(ICommandInfo[] commandInfos)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(typeof(RDbUtility).Name);
            }

            if (commandInfos == null)
            {
                throw new ArgumentNullException("commandInfos");
            }

            int[] _Result = new int[commandInfos.Length];
            using (DbConnection _Connection = Factory.CreateConnection(ConnectionString))
            {
                DbCommand _Command = _Connection.CreateCommand();
                _Connection.Open();
                for (int i = 0; i < commandInfos.Length; i++)
                {
                    _Command.Parameters.Clear();
                    _Command.CommandType = commandInfos[i].CommandType;
                    _Command.CommandText = commandInfos[i].CommandText;
                    if (commandInfos[i].Timeout != 0)
                    {
                        _Command.CommandTimeout = commandInfos[i].Timeout;
                    }
                    _Command.Parameters.AddRange(commandInfos[i].Parameters.ToArray());
                    _Result[i] = _Command.ExecuteNonQuery();
                }
                return _Result;
            }
        }
    }
}
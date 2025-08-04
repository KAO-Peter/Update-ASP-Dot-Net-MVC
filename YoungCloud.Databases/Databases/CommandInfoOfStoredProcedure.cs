using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The information of stored procedure command.
    /// </summary>
    public class CommandInfoOfStoredProcedure : CommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoOfStoredProcedure">CommandInfoOfStoredProcedure</see> class.
        /// </summary>
        /// <param name="spName">The stored procedure name of command.</param>
        public CommandInfoOfStoredProcedure(string spName)
            : base(spName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoOfStoredProcedure">CommandInfoOfStoredProcedure</see> class.
        /// </summary>
        /// <param name="spName">The stored procedure name of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        public CommandInfoOfStoredProcedure(string spName, params IDbDataParameter[] parameters)
            : base(spName, parameters)
        {
        }        

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoOfStoredProcedure">CommandInfoOfStoredProcedure</see> class.
        /// </summary>
        /// <param name="spName">The stored procedure name of command.</param>
        /// <param name="commandTimeout">The timeout seconds of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        public CommandInfoOfStoredProcedure(string spName, int commandTimeout, params IDbDataParameter[] parameters)
            : base(spName, commandTimeout, parameters)
        {
        }

        /// <summary>
        /// The type of command.
        /// </summary>
        public override CommandType CommandType
        {
            get { return CommandType.StoredProcedure; }
        }
    }
}
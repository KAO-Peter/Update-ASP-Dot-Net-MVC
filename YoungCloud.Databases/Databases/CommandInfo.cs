using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The information of command.
    /// </summary>
    public abstract class CommandInfo : ICommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo">CommandInfo</see> class.
        /// </summary>
        /// <param name="commandText">The text or stored procedure name of command.</param>
        protected CommandInfo(string commandText)
        {
            Parameters = new DbDataParameterCollection();
            CommandText = commandText;
            Timeout = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo">CommandInfo</see> class.
        /// </summary>
        /// <param name="commandText">The text or stored procedure name of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        protected CommandInfo(string commandText, params IDbDataParameter[] parameters)
            : this(commandText)
        {
            Parameters.AddRange(parameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo">CommandInfo</see> class.
        /// </summary>
        /// <param name="commandText">The text or stored procedure name of command.</param>
        /// <param name="commandTimeout">The timeout seconds of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        protected CommandInfo(string commandText, int commandTimeout, params IDbDataParameter[] parameters)
            : this(commandText,parameters)
        {
            Timeout = commandTimeout;
        }

        /// <summary>
        /// The text or stored procedure name of command.
        /// </summary>
        public string CommandText
        {
            get;
            set;
        }

        /// <summary>
        /// The seconds of command timeout.
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// The type of command.
        /// </summary>
        public abstract CommandType CommandType
        {
            get;
        }

        /// <summary>
        /// The collection of IDbDataParameter.
        /// </summary>
        public IDbDataParameterCollection Parameters
        {
            get;
            set;
        }
    }
}
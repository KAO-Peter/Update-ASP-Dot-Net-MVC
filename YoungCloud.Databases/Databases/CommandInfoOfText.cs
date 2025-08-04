using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The information of text command.
    /// </summary>
    public class CommandInfoOfText : CommandInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoOfText">CommandInfoOfText</see> class.
        /// </summary>
        /// <param name="commandText">The text of command.</param>
        public CommandInfoOfText(string commandText)
            : base(commandText)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoOfText">CommandInfoOfText</see> class.
        /// </summary>
        /// <param name="commandText">The text of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        public CommandInfoOfText(string commandText, params IDbDataParameter[] parameters)
            : base(commandText, parameters)
        {
        }        

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfo">CommandInfo</see> class.
        /// </summary>
        /// <param name="commandText">The text of command.</param>
        /// <param name="commandTimeout">The timeout seconds of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        public CommandInfoOfText(string commandText, int commandTimeout, params IDbDataParameter[] parameters)
            : base(commandText,commandTimeout ,parameters)
        {
        }

        /// <summary>
        /// The type of command.
        /// </summary>
        public override CommandType CommandType
        {
            get { return CommandType.Text; }
        }
    }
}
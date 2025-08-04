using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The definition of command information.
    /// </summary>
    public interface ICommandInfo
    {
        /// <summary>
        /// The text or stored procedure name of command.
        /// </summary>
        string CommandText { get; set; }
        /// <summary>
        /// The collection of IDbDataParameter.
        /// </summary>
        IDbDataParameterCollection Parameters { get; set; }
        /// <summary>
        /// The seconds of command timeout.
        /// </summary>
        int Timeout { get; set; }
        /// <summary>
        /// The type of command.
        /// </summary>
        CommandType CommandType { get;}
    }
}
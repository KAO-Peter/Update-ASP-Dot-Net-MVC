using System.Collections;
using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The definition of ICommandInfo instance collection.
    /// </summary>
    public interface ICommandInfoCollection : ICollection
    {
        /// <summary>
        /// Add command.
        /// </summary>
        /// <param name="commandInfo">The information of command.</param>
        void Add(ICommandInfo commandInfo);
        /// <summary>
        /// Add command.
        /// </summary>
        /// <param name="type">The type of command.</param>
        /// <param name="commandText">The text or stored procedure name of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        void Add(CommandType type, string commandText, params IDbDataParameter[] parameters);
        /// <summary>
        /// Add ICommandInfo instance range into collection.
        /// </summary>
        /// <param name="commandInfos">The instance array of ICommandInfo.</param>
        void AddRange(ICommandInfo[] commandInfos);
        /// <summary>
        /// To clear data in collection.
        /// </summary>
        void Clear();
        /// <summary>
        /// Get or set the instance of ICommandInfo.
        /// </summary>
        /// <param name="index">The index of instance in the collection.</param>
        /// <returns>The instance of ICommandInfo.</returns>
        ICommandInfo this[int index] { get; set; }
        /// <summary>
        /// Conver the collection to array.
        /// </summary>
        /// <returns>The instance array of ICommandInfo.</returns>
        ICommandInfo[] ToArray();
    }
}
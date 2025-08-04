using System;
using System.Collections;
using System.Data;

namespace YoungCloud.Databases
{
    /// <summary>
    /// The collection of ICommandInfo instance.
    /// </summary>
    public class CommandInfoCollection : CollectionBase, ICommandInfoCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandInfoCollection">CommandInfoCollection</see> class.
        /// </summary>
        public CommandInfoCollection()
        {
        }

        /// <summary>
        /// Add command.
        /// </summary>
        /// <param name="commandInfo">The information of command.</param>
        public void Add(ICommandInfo commandInfo)
        {
            if (commandInfo != null)
            {
                List.Add(commandInfo);
            }
        }

        /// <summary>
        /// Add command.
        /// </summary>
        /// <param name="type">The type of command.</param>
        /// <param name="commandText">The text or stored procedure name of command.</param>
        /// <param name="parameters">The array of IDbDataParameter instance.</param>
        public void Add(CommandType type, string commandText, params IDbDataParameter[] parameters)
        {
            switch (type)
            {
                case CommandType.StoredProcedure:
                    Add(new CommandInfoOfStoredProcedure(commandText, parameters));
                    break;
                case CommandType.Text:
                    Add(new CommandInfoOfText(commandText, parameters));
                    break;
                default:
                    throw new NotSupportedException();
            }
            
        }

        /// <summary>
        /// Add ICommandInfo instance range into collection.
        /// </summary>
        /// <param name="commandInfos">The instance array of ICommandInfo.</param>
        public void AddRange(ICommandInfo[] commandInfos)
        {
            if (commandInfos == null) { return; }
            foreach (ICommandInfo commandInfo in commandInfos)
            {
                List.Add(commandInfo);
            }
        }

        /// <summary>
        /// Get or set the instance of ICommandInfo.
        /// </summary>
        /// <param name="index">The index of instance in the collection.</param>
        /// <returns>The instance of ICommandInfo.</returns>
        public ICommandInfo this[int index]
        {
            get
            {
                return (ICommandInfo)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Conver the collection to array.
        /// </summary>
        /// <returns>The instance array of ICommandInfo.</returns>
        public ICommandInfo[] ToArray()
        {
            if (List.Count == 0)
            {
                return null;
            }
            Array _Array = Array.CreateInstance(typeof(ICommandInfo), List.Count);
            List.CopyTo(_Array, 0);
            return (ICommandInfo[])_Array;
        }
    }
}
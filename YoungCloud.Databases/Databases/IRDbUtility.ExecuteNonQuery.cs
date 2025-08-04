using System.Data;

namespace YoungCloud.Databases
{
    public partial interface IRDbUtility
    {
        /// <summary>
        /// To execute command without query.
        /// </summary>
        /// <param name="command">The instance of CommandInfo object.</param>
        /// <returns>The data number that been effected.</returns>
        int ExecuteNonQuery(ICommandInfo command);
        /// <summary>
        /// To execute command without query by command text type.
        /// </summary>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The data number that been effected.</returns>
        int ExecuteNonQueryByCommandText(string commandText, params IDbDataParameter[] parameters);
        /// <summary>
        /// To execute command without query by command text type.
        /// </summary>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The data number that been effected.</returns>
        int ExecuteNonQueryByCommandText(string commandText, int cmdTimeout, params IDbDataParameter[] parameters);
        /// <summary>
        /// To execute command without query by stored procedure type.
        /// </summary>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The data number that been effected.</returns>
        int ExecuteNonQueryByStoredProcedure(string spName, params IDbDataParameter[] parameters);
        /// <summary>
        /// To execute command without query by stored procedure type.
        /// </summary>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The data number that been effected.</returns>
        int ExecuteNonQueryByStoredProcedure(string spName, int cmdTimeout, params IDbDataParameter[] parameters);
    }
}
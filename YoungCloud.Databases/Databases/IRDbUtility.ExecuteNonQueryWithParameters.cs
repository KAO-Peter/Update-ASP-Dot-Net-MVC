using System.Data;

namespace YoungCloud.Databases
{
    public partial interface IRDbUtility
    {
        /// <summary>
        /// To execute command without query and return output type parameters.
        /// </summary>
        /// <param name="command">The instance of CommandInfo object.</param>
        /// <returns>The collection of parameters.</returns>
        IDataParameterCollection ExecuteNonQueryWithParameters(ICommandInfo command);
        /// <summary>
        /// To execute command without query and return output type parameters by command text type.
        /// </summary>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The collection of parameters.</returns>
        IDataParameterCollection ExecuteNonQueryWithParametersByCommandText(string commandText, params IDbDataParameter[] parameters);
        /// <summary>
        /// To execute command without query and return output type parameters by command text type.
        /// </summary>
        /// <param name="commandText">The sql text of command.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The collection of parameteres.</param>
        /// <returns>The collection of parameters.</returns>
        IDataParameterCollection ExecuteNonQueryWithParametersByCommandText(string commandText, int cmdTimeout, params IDbDataParameter[] parameters);
        /// <summary>
        /// To execute command without query and return output type parameters by stored procedure type.
        /// </summary>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The collection of parameters.</returns>
        IDataParameterCollection ExecuteNonQueryWithParametersByStoredProcedure(string spName, params IDbDataParameter[] parameters);
        /// <summary>
        /// To execute command without query and return output type parameters by stored procedure type.
        /// </summary>
        /// <param name="spName">The name of stored procedure.</param>
        /// <param name="cmdTimeout">The timeout setting of execution.</param>
        /// <param name="parameters">The parameter collection of stored procedure.</param>
        /// <returns>The collection of parameters.</returns>
        IDataParameterCollection ExecuteNonQueryWithParametersByStoredProcedure(string spName, int cmdTimeout, params IDbDataParameter[] parameters);
    }
}
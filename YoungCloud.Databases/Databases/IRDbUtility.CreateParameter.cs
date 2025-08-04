using System.Data;

namespace YoungCloud.Databases
{
    public partial interface IRDbUtility
    {
        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <returns>The instance of parameter.</returns>
        IDbDataParameter CreateParameter();
        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <param name="parameterName">The name of parameter.</param>
        /// <param name="parameterValue">The data value of parameter.</param>
        /// <returns>The instance of parameter.</returns>
        IDbDataParameter CreateParameter(string parameterName, object parameterValue);
        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <param name="parameterName">The name of parameter.</param>
        /// <param name="parameterValue">The data value of parameter.</param>
        /// <param name="dbType">The data type of parameter.</param>
        /// <param name="parameterDirection">The direction of parameter.</param>        
        /// <returns>The instance of parameter.</returns>
        IDbDataParameter CreateParameter(string parameterName, object parameterValue, DbType dbType, ParameterDirection parameterDirection);
        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <param name="count">The number of parameter instance that wanna be created.</param>
        /// <returns>The collection of parameter instance.</returns>
        IDbDataParameter[] CreateParameters(int count);
    }
}
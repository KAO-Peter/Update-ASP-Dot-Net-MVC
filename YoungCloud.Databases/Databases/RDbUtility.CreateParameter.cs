using System.Data;
using System.Data.Common;

namespace YoungCloud.Databases
{
    public abstract partial class RDbUtility
    {
        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <returns>The instance of parameter.</returns>
        public virtual IDbDataParameter CreateParameter()
        {
            return Factory.CreateParameter();
        }

        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <param name="parameterName">The name of parameter.</param>
        /// <param name="parameterValue">The data value of parameter.</param>
        /// <returns>The instance of parameter.</returns>
        public virtual IDbDataParameter CreateParameter(string parameterName, object parameterValue)
        {
            return Factory.CreateParameter(parameterName, parameterValue);
        }

        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <param name="parameterName">The name of parameter.</param>
        /// <param name="parameterValue">The data value of parameter.</param>
        /// <param name="dbType">The data type of parameter.</param>
        /// <param name="parameterDirection">The direction of parameter.</param>        
        /// <returns>The instance of parameter.</returns>
        public virtual IDbDataParameter CreateParameter(string parameterName, object parameterValue, DbType dbType, ParameterDirection parameterDirection)
        {
            return Factory.CreateParameter(parameterName, parameterValue, dbType, parameterDirection);
        }

        /// <summary>
        /// To create parameter instance.
        /// </summary>
        /// <param name="count">The number of parameter instance that wanna be created.</param>
        /// <returns>The collection of parameter instance.</returns>
        public virtual IDbDataParameter[] CreateParameters(int count)
        {
            return Factory.CreateParameters(count);
        }
    }
}
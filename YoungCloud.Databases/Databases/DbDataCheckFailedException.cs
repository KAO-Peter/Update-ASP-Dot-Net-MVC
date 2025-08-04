using System;
using System.Runtime.Serialization;
using YoungCloud.Exceptions;

namespace YoungCloud.Databases
{
    /// <summary>
    /// This exception throws when data check is failed in database procedure.
    /// </summary>
    [Serializable]
    public class DbDataCheckFailedException : ExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        public DbDataCheckFailedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        public DbDataCheckFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        public DbDataCheckFailedException(string message, int errorCode)
            : base(message, errorCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        public DbDataCheckFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        public DbDataCheckFailedException(string message, int errorCode, Exception innerException)
            : base(message, errorCode, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public DbDataCheckFailedException(string message, params IExtraInfo[] extraInfos)
            : base(message, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public DbDataCheckFailedException(string message, IExtraInfoCollection extraInfoCol)
            : base(message, extraInfoCol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public DbDataCheckFailedException(string message, int errorCode, params IExtraInfo[] extraInfos)
            : base(message, errorCode, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbDataCheckFailedException">DbDataCheckFailedException</see> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">SerializationInfo</see> to populate with data. </param>
        /// <param name="context">The destination <see cref="StreamingContext">StreamingContext</see> for this serialization.</param>
        protected DbDataCheckFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
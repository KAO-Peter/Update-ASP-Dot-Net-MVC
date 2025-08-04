using System;
using System.Runtime.Serialization;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// Message of system to client or log.
    /// </summary>
    [Serializable]
    public class LogMessageException : ExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        public LogMessageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        public LogMessageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        public LogMessageException(string message, int errorCode)
            : base(message, errorCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        public LogMessageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        public LogMessageException(string message, int errorCode, Exception innerException)
            : base(message, errorCode, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public LogMessageException(string message, params IExtraInfo[] extraInfos)
            : base(message, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public LogMessageException(string message, IExtraInfoCollection extraInfoCol)
            : base(message, extraInfoCol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public LogMessageException(string message, int errorCode, params IExtraInfo[] extraInfos)
            : base(message, errorCode, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageException">SystemMessage</see> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">SerializationInfo</see> to populate with data. </param>
        /// <param name="context">The destination <see cref="StreamingContext">StreamingContext</see> for this serialization.</param>
        protected LogMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
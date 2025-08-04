using System;
using System.Runtime.Serialization;
using YoungCloud.Exceptions;

namespace YoungCloud.Databases
{
    /// <summary>
    /// This excetpion throws when DataRow is null in DataEntityClass object.
    /// </summary>
    [Serializable]
    public class DataRowNullReferenceException : ExceptionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        public DataRowNullReferenceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        public DataRowNullReferenceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        public DataRowNullReferenceException(string message, int errorCode)
            : base(message, errorCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        public DataRowNullReferenceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        public DataRowNullReferenceException(string message, int errorCode, Exception innerException)
            : base(message, errorCode, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public DataRowNullReferenceException(string message, params IExtraInfo[] extraInfos)
            : base(message, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public DataRowNullReferenceException(string message, IExtraInfoCollection extraInfoCol)
            : base(message, extraInfoCol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public DataRowNullReferenceException(string message, int errorCode, params IExtraInfo[] extraInfos)
            : base(message, errorCode, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public DataRowNullReferenceException(string message, int errorCode, IExtraInfoCollection extraInfoCol)
            : base(message, errorCode, extraInfoCol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public DataRowNullReferenceException(string message, Exception innerException, params IExtraInfo[] extraInfos)
            : base(message, innerException, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public DataRowNullReferenceException(string message, Exception innerException, IExtraInfoCollection extraInfoCol)
            : base(message, innerException, extraInfoCol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public DataRowNullReferenceException(string message, int errorCode, Exception innerException, params IExtraInfo[] extraInfos)
            : base(message, innerException, extraInfos)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public DataRowNullReferenceException(string message, int errorCode, Exception innerException, IExtraInfoCollection extraInfoCol)
            : base(message, errorCode, innerException, extraInfoCol)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRowNullReferenceException">DataRowNullReferenceException</see> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">SerializationInfo</see> to populate with data. </param>
        /// <param name="context">The destination <see cref="StreamingContext">StreamingContext</see> for this serialization.</param>
        protected DataRowNullReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
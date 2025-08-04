using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// The base object of exception.
    /// </summary>
    [Serializable]
    public class ExceptionBase : Exception
    {
        private IExtraInfoCollection m_ExtraInfoCol = new ExtraInfoCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        public ExceptionBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        public ExceptionBase(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        public ExceptionBase(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        public ExceptionBase(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        public ExceptionBase(string message, int errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public ExceptionBase(string message, params IExtraInfo[] extraInfos)
            : base(message)
        {
            ExtraInfos = extraInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public ExceptionBase(string message, IExtraInfoCollection extraInfoCol)
            : base(message)
        {
            ExtraInfoCol = extraInfoCol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public ExceptionBase(string message, int errorCode, params IExtraInfo[] extraInfos)
            : base(message)
        {
            ErrorCode = errorCode;
            ExtraInfos = extraInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public ExceptionBase(string message, int errorCode, IExtraInfoCollection extraInfoCol)
            : base(message)
        {
            ErrorCode = errorCode;
            ExtraInfoCol = extraInfoCol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public ExceptionBase(string message, Exception innerException, params IExtraInfo[] extraInfos)
            : base(message, innerException)
        {
            ExtraInfos = extraInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public ExceptionBase(string message, Exception innerException, IExtraInfoCollection extraInfoCol)
            : base(message, innerException)
        {
            ExtraInfoCol = extraInfoCol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfos">The extra information array of exception.</param>
        public ExceptionBase(string message, int errorCode, Exception innerException, params IExtraInfo[] extraInfos)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ExtraInfos = extraInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class.
        /// </summary>
        /// <param name="message">The message of exception.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="innerException">The threw innerException.</param>
        /// <param name="extraInfoCol">The extra information collection of exception.</param>
        public ExceptionBase(string message, int errorCode, Exception innerException, IExtraInfoCollection extraInfoCol)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ExtraInfoCol = extraInfoCol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionBase">BaseException</see> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">SerializationInfo</see> to populate with data. </param>
        /// <param name="context">The destination <see cref="StreamingContext">StreamingContext</see> for this serialization.</param>
        protected ExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            ExtraInfoCol = (IExtraInfoCollection)info.GetValue("ExtraInfoCol", typeof(ExtraInfoCollection));
            ErrorCode = info.GetInt32("ErrorCode");
        }

        /// <summary>
        /// Get the serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo">SerializationInfo</see> to populate with data. </param>
        /// <param name="context">The destination <see cref="StreamingContext">StreamingContext</see> for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ExtraInfoCol", ExtraInfoCol);
            info.AddValue("ErrorCode", ErrorCode);
        }

        /// <summary>
        /// The extra information collection of exception.
        /// </summary>
        public IExtraInfoCollection ExtraInfoCol 
        {
            get
            {
                return m_ExtraInfoCol;
            }
            set
            {
                m_ExtraInfoCol = value;
            }
        }

        /// <summary>
        /// The extra information array of exception.
        /// </summary>
        public IExtraInfo[] ExtraInfos
        {
            get
            {
                return ExtraInfoCol.ToArray();
            }
            set
            {
                ExtraInfoCol.Clear();
                ExtraInfoCol.AddRange(value);
            }
        }

        /// <summary>
        /// The code of exception.
        /// </summary>
        public int ErrorCode { get; protected set; }
    }
}
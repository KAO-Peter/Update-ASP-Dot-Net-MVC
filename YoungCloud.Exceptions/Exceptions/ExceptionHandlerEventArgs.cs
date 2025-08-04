using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// The event arguments of ExceptionHandlerDelegate.
    /// </summary>
    public class ExceptionHandlerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerEventArgs">ExceptionHandlerEventArgs</see> class.
        /// </summary>
        /// <param name="level">The instance of <see cref="ExceptionLevel">ExceptionLevel</see>.</param>
        /// <param name="e">The instanceof exception.</param>
        public ExceptionHandlerEventArgs(ExceptionLevel level, Exception e)
        {
            Level = level;
            OccuredException = e;
            LogIfIsDebug = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerEventArgs">ExceptionHandlerEventArgs</see> class.
        /// </summary>
        /// <param name="level">The instance of <see cref="ExceptionLevel">ExceptionLevel</see>.</param>
        /// <param name="e">The instanceof exception.</param>
        /// <param name="currentContext">The context of current thread.(ex. HttpContext)</param>
        public ExceptionHandlerEventArgs(ExceptionLevel level, Exception e, object currentContext)
            : this(level, e)
        {
            CurrentContext = currentContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerEventArgs">ExceptionHandlerEventArgs</see> class.
        /// </summary>
        /// <param name="level">The instance of <see cref="ExceptionLevel">ExceptionLevel</see>.</param>
        /// <param name="e">The instanceof exception.</param>
        /// <param name="infos">The collection of IExceptionHandlerInfoData instances.</param>
        public ExceptionHandlerEventArgs(ExceptionLevel level, Exception e, params IExceptionHandlerInfoData[] infos)
            : this(level, e)
        {
            foreach (IExceptionHandlerInfoData _Info in infos)
            {
                _Info.ExceptionLevel = level;
                if (_Info is ITxtFileExceptionHandlerInfoData)
                {
                    TxtFileHandlerData = (ITxtFileExceptionHandlerInfoData)_Info;
                    continue;
                }
                if (_Info is IEventLogExceptionHandlerInfoData)
                {
                    EventLogHandlerData = (IEventLogExceptionHandlerInfoData)_Info;
                    continue;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerEventArgs">ExceptionHandlerEventArgs</see> class.
        /// </summary>
        /// <param name="level">The instance of <see cref="ExceptionLevel">ExceptionLevel</see>.</param>
        /// <param name="e">The instanceof exception.</param>
        /// <param name="currentContext">The context of current thread.(ex. HttpContext)</param>
        /// <param name="infos">The collection of IExceptionHandlerInfoData instances.</param>
        public ExceptionHandlerEventArgs(ExceptionLevel level, Exception e, object currentContext, params IExceptionHandlerInfoData[] infos)
            : this(level, e, infos)
        {
            CurrentContext = currentContext;
        }

        /// <summary>
        /// The context of current thread.
        /// </summary>
        public object CurrentContext { get; set; }

        /// <summary>
        /// The event log log handler info data.
        /// </summary>
        public IEventLogExceptionHandlerInfoData EventLogHandlerData
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the instance of occured exception.
        /// </summary>
        /// <returns>The instance of occured exception.</returns>
        public Exception GetException()
        {
            return OccuredException;
        }

        /// <summary>
        /// The instance of ExceptionLevel.
        /// </summary>
        public ExceptionLevel Level
        {
            get;
            private set;
        }

        /// <summary>
        /// To write log if is debug mode.
        /// </summary>
        public bool LogIfIsDebug
        {
            get;
            set;
        }

        /// <summary>
        /// The text file log handler info data.
        /// </summary>
        public ITxtFileExceptionHandlerInfoData TxtFileHandlerData
        {
            get;
            private set;
        }

        private Exception OccuredException
        {
            get;
            set;
        }
    }
}
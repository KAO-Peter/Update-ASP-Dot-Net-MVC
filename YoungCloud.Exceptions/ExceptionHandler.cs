using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using YoungCloud.Exceptions;

namespace YoungCloud
{
    /// <summary>
    /// The exception handler of system.
    /// </summary>
    public static class ExceptionHandler
    {
        /// <summary>
        /// The exception handler delegate of program.
        /// </summary>
        public static ExceptionHandlerDelegate ExceptionHandle = null;
        /// <summary>
        /// Is running in dubug mode or not.
        /// </summary>
        public static bool IsDebug = false;
        /// <summary>
        /// To handle exception by asynchronous threads or not.
        /// </summary>
        public static bool IsAsyncHandler = true;

        /// <summary>
        /// The excepton handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="e">The instance of <see cref="ExceptionHandlerEventArgs">ExceptionHandlerEventArgs</see>.</param>
        public static void Handle(object sender, ExceptionHandlerEventArgs e)
        {
            if (ExceptionHandle != null)
            {
                if (IsAsyncHandler)
                {
                    ExceptionHandle.BeginInvoke(sender, e, HandlerCallBack, null);
                }
                else
                {
                    ExceptionHandle.Invoke(sender, e);
                }
            }
            else
            {                
                if (!e.LogIfIsDebug)
                {
                    throw e.GetException();
                }
            }
        }

        private static void HandlerCallBack(IAsyncResult result)
        {
            try
            {
                AsyncResult _Result = (AsyncResult)result;
                ExceptionHandlerDelegate _Delegate = (ExceptionHandlerDelegate)_Result.AsyncDelegate;
                //maybe need do something, case by case.
                _Delegate.EndInvoke(result);
            }
            catch (Exception)
            {                
            }
        }

        /// <summary>
        /// The excepton handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="e">The instance of exception.</param>
        public static void Handle(object sender, Exception e)
        {
            Handle(sender, e, ExceptionLevel.Error);
        }

        /// <summary>
        /// The excepton handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="e">The instance of exception.</param>
        /// <param name="level">The level of exception.</param>
        public static void Handle(object sender, Exception e, ExceptionLevel level)
        {
            Handle(sender, new ExceptionHandlerEventArgs(level, e));
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="e">The instance of log.</param>
        public static void Log(object sender, Exception e)
        {
            Handle(sender, e, ExceptionLevel.Info);
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="message">The message of log.</param>
        public static void Log(object sender, string message)
        {
            Log(sender, new LogMessageException(message));
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="message">The message of log.</param>
        /// <param name="extraInfos">The array of IExtraInfo instance.</param>
        public static void Log(object sender, string message, IExtraInfo[] extraInfos)
        {
            Log(sender, new LogMessageException(message, extraInfos));
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="message">The message of log.</param>
        /// <param name="extraInfos">The collection of IExtraInfo instance.</param>
        public static void Log(object sender, string message, IExtraInfoCollection extraInfos)
        {
            Log(sender, new LogMessageException(message, extraInfos));
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="e">The instance of log.</param>
        public static void LogIfIsDebug(object sender, Exception e)
        {
            ExceptionHandlerEventArgs _Args = new ExceptionHandlerEventArgs(ExceptionLevel.Info, e);
            _Args.LogIfIsDebug = true;
            Handle(sender, _Args);
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="message">The message of log.</param>
        public static void LogIfIsDebug(object sender, string message)
        {
            LogIfIsDebug(sender, new LogMessageException(message));
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="message">The message of log.</param>
        /// <param name="extraInfos">The array of IExtraInfo instance.</param>
        public static void LogIfIsDebug(object sender, string message, IExtraInfo[] extraInfos)
        {
            LogIfIsDebug(sender, new LogMessageException(message, extraInfos));
        }

        /// <summary>
        /// The log handler of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="message">The message of log.</param>
        /// <param name="extraInfos">The collection of IExtraInfo instance.</param>
        public static void LogIfIsDebug(object sender, string message, IExtraInfoCollection extraInfos)
        {
            LogIfIsDebug(sender, message, extraInfos);
        }

        /// <summary>
        /// To save source code information into Exception.Data.
        /// </summary>
        /// <param name="e">The instance of exception.</param>
        public static void SaveCodeInfo(Exception e)
        {
            StackTrace _StackTrace = new StackTrace(e, true);
            StackFrame _StackFrame = _StackTrace.GetFrame(0);
            StackFrame _TempStackFrame = null;
            string _CodeFileName = _StackFrame.GetFileName();
            string _TempCodeFileName = string.Empty;
            for (int i = 0; i < _StackTrace.FrameCount; i++)
            {
                _TempStackFrame = _StackTrace.GetFrame(i);
                _TempCodeFileName = _TempStackFrame.GetFileName();
                if (!string.IsNullOrEmpty(_TempCodeFileName))
                {
                    _StackFrame = _TempStackFrame;
                    _CodeFileName = _TempCodeFileName;
                }
            }
            if (_StackFrame == null)
            {
                return;
            }

            int _ErrorLine = _StackFrame.GetFileLineNumber();

            e.Data.Add("Error code information begin", "<<<<<<<<<<<<<<<<<<<<<<<<<");
            e.Data.Add("Occurred at", "Line {0} and Column {1}".FormatWith(_ErrorLine.ToString(), _StackFrame.GetFileColumnNumber().ToString()));

            if (!string.IsNullOrEmpty(_CodeFileName))
            {
                string[] _SourceCodeLines = File.ReadAllLines(_CodeFileName);
                e.Data.Add("Code Line {0}".FormatWith(_ErrorLine.ToString()), _SourceCodeLines[_ErrorLine - 1]);
            }

            e.Data.Add("Error code information end", ">>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }
    }
}
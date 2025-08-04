using System;
using YoungCloud.Exceptions;

namespace YoungCloud.Configurations.Exceptions
{
    /// <summary>
    /// The configured exception handler.
    /// </summary>
    public class ConfiguredExceptionHandler
    {
        /// <summary>
        /// The delegate of ExceptionHandlerSection provider.
        /// </summary>
        public static Func<ExceptionHandlerSection> GetExceptionHandlerSection = null;
        /// <summary>
        /// The delegate of event log exception handler.
        /// </summary>
        public static ExceptionHandlerDelegate EventLogExceptionHandler = null;
        /// <summary>
        /// The delegate of email exception handler.
        /// </summary>
        public static ExceptionHandlerDelegate MailExceptionHandler = null;
        /// <summary>
        /// The delegate of text file exception handler.
        /// </summary>
        public static ExceptionHandlerDelegate TextFileExceptionHandler = null;

        /// <summary>
        /// To initial settings from configuration.
        /// </summary>
        public static void InitialHandler()
        {
            ExceptionHandler.ExceptionHandle = ConfiguredExceptionHandler.Handle;
            try
            {
                ExceptionHandlerSection _HandlerSetting = null;
                if (GetExceptionHandlerSection == null)
                {
                    _HandlerSetting = ConfigurationUtility.GetExceptionHandlerSection();
                }
                else
                {
                    _HandlerSetting = GetExceptionHandlerSection.Invoke();
                }
                ExceptionHandler.IsDebug = _HandlerSetting.IsDebug;
                ExceptionHandler.IsAsyncHandler = _HandlerSetting.HandleByAsync;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// To handle exception of system.
        /// </summary>
        /// <param name="sender">The object instance that invoke this handle process.</param>
        /// <param name="e">The instance of ExceptionHandlerEventArg.</param>
        public static void Handle(object sender, ExceptionHandlerEventArgs e)
        {
            ExceptionHandlerSection _HandlerSetting = null;
            try
            {
                if (GetExceptionHandlerSection == null)
                {
                    _HandlerSetting = ConfigurationUtility.GetExceptionHandlerSection();
                }
                else
                {
                    _HandlerSetting = GetExceptionHandlerSection.Invoke();
                }
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                if (_HandlerSetting.TextFileLog.Enabled)
                {
                    if (TextFileExceptionHandler == null)
                    {
                        ITxtFileExceptionHandlerInfoData _Data = null;
                        if (e.TxtFileHandlerData == null)
                        {
                            _Data = new TxtFileExceptionHandlerInfoData();
                            _Data.ApplicationName = _HandlerSetting.ApplicationName;
                            _Data.ExceptionLevel = e.Level;
                            _Data.FileName = string.Format("{0}_{1}.txt", DateTime.Now.ToString("yyyyMMdd_HH"), _Data.ApplicationName);
                            _Data.Path = _HandlerSetting.TextFileLog.Path.Replace(@"~\", AppDomain.CurrentDomain.BaseDirectory);
                        }
                        else
                        {
                            _Data = e.TxtFileHandlerData;
                        }
                        ExceptionHandlerAdapter.Handle(sender.GetType(), e.GetException(), _Data);
                        if (e.LogIfIsDebug && ExceptionHandler.IsDebug && _Data.ExceptionLevel >= ExceptionLevel.Error)
                        {
                            _Data.FileName = string.Format("{0}_{1}_{2}.txt", DateTime.Now.ToString("yyyyMMdd_HH"), _Data.ApplicationName, _Data.ExceptionLevel.ToString());
                            ExceptionHandlerAdapter.Handle(sender.GetType(), e.GetException(), _Data);
                        }
                    }
                    else
                    {
                        TextFileExceptionHandler.Invoke(sender, e);
                    }
                }
            }
            catch (Exception)
            {
            }
            
            try
            {
                if (_HandlerSetting.EventLog.Enabled)
                {
                    if (EventLogExceptionHandler == null)
                    {
                        IEventLogExceptionHandlerInfoData _Data = null;
                        if (e.EventLogHandlerData == null)
                        {
                            _Data = new EventLogExceptionHandlerInfoData();
                            _Data.ApplicationName = _HandlerSetting.ApplicationName;
                            _Data.ExceptionLevel = e.Level;
                            _Data.Caption = _HandlerSetting.EventLog.Caption;
                            _Data.Location = _HandlerSetting.EventLog.Location;
                        }
                        else
                        {
                            _Data = e.EventLogHandlerData;
                        }
                        ExceptionHandlerAdapter.Handle(sender.GetType(), e.GetException(), _Data);
                    }
                    else
                    {
                        EventLogExceptionHandler.Invoke(sender, e);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
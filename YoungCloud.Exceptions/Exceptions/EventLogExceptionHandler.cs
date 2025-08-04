using System;
using System.Diagnostics;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 將例外物件資料寫入Windows Event Log的物件。
    /// </summary>
    [Serializable]
    public class EventLogExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// 處理例外物件的功能。
        /// </summary>
        /// <param name="typeEntity">發生例外的組件類別。</param>
        /// <param name="exceptionEntity">例外物件。</param>
        /// <param name="iInfo">例外處理機制的額外資訊需求介面定義。</param>
        public void Handle(Type typeEntity, Exception exceptionEntity, IExceptionHandlerInfoData iInfo)
        {

            IEventLogExceptionHandlerInfoData _Info = iInfo as IEventLogExceptionHandlerInfoData;
            if (_Info == null)
            {
                return;
            }

            if (typeEntity == null) { return; }
            if (exceptionEntity == null) { return; }
            if (iInfo == null) { return; }
            //檢查Event Log的Source是否存在
            if (!EventLog.SourceExists(_Info.Caption))
            {
                //不存在就建立
                EventSourceCreationData _Data = new EventSourceCreationData(_Info.Caption, _Info.Location);
                EventLog.CreateEventSource(_Data);
            }
            //設定Event Log的事件類別
            EventLogEntryType _LogType;
            switch (iInfo.ExceptionLevel)
            {
                case ExceptionLevel.Debug:
                case ExceptionLevel.Info:
                    _LogType = EventLogEntryType.Information;
                    break;
                case ExceptionLevel.Warn:
                    _LogType = EventLogEntryType.Warning;
                    break;
                default:
                    _LogType = EventLogEntryType.Error;
                    break;
            }
            //建立訊息字串
            ExceptionMessageBuilder _Builder = new ExceptionMessageBuilder(HandleType.EventLog);
            //寫Log
            EventLog.WriteEntry(_Info.Caption, _Builder.Build(typeEntity, exceptionEntity, iInfo), _LogType);
        }
    }
}
using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// EventLog例外處理機制的額外資訊需求物件。
    /// </summary>
    [Serializable]
    public class EventLogExceptionHandlerInfoData : ExceptionInfoData, IEventLogExceptionHandlerInfoData
    {
        /// <summary>
        /// Source。
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// 例外處理方式列舉。
        /// </summary>
        public override HandleType Type
        {
            get
            {
                return HandleType.EventLog;
            }
        }

        /// <summary>
        /// LogName。
        /// </summary>
        public string Location
        {
            get;
            set;
        }
    }
}
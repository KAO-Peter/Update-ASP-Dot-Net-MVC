using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 例外處理機制的額外資訊需求物件。
    /// </summary>
    [Serializable]
    public abstract class ExceptionInfoData : IExceptionHandlerInfoData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionInfoData">ExceptionInfoData</see> class.
        /// </summary>
        protected ExceptionInfoData()
        {
            this.ExceptionLevel = ExceptionLevel.Error;
        }

        /// <summary>
        /// 應用程式名稱.
        /// </summary>
        public string ApplicationName
        {
            get;
            set;
        }

        /// <summary>
        /// 例外處理方式列舉。
        /// </summary>
        public abstract HandleType Type
        {
            get;
        }

        /// <summary>
        /// 例外物件級別列舉.
        /// </summary>
        public ExceptionLevel ExceptionLevel
        {
            get;
            set;
        }
    }
}
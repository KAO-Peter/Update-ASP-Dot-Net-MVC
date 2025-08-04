using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 處理例外的物件。
    /// </summary>
    [Serializable]
    public static class ExceptionHandlerAdapter
    {
        /// <summary>
        /// 處理例外物件的功能。
        /// </summary>
        /// <param name="typeEntity">發生例外的組件類別。</param>
        /// <param name="exceptionEntity">例外物件。</param>
        /// <param name="iInfo">例外處理機制的額外資訊需求介面定義。</param>
        public static void Handle(Type typeEntity, Exception exceptionEntity, IExceptionHandlerInfoData iInfo)
        {
            if (typeEntity == null) { return; }
            if (exceptionEntity == null) { return; }

            IExceptionHandler _Handler;
            switch (iInfo.Type)
            {
                case HandleType.LogTxtFile: //將例外物件資料寫入文字檔
                    _Handler = new TxtFileExceptionHandler();
                    break;
                default:                    //將例外物件資料寫入Windows Event Log
                    _Handler = new EventLogExceptionHandler();
                    break;
            }
            _Handler.Handle(typeEntity, exceptionEntity, iInfo);
        }
    }
}
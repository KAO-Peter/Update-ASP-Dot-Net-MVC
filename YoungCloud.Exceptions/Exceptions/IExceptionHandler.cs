using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 處理例外的物件定義。
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// 處理例外物件的功能。
        /// </summary>
        /// <param name="typeEntity">發生例外的組件類別。</param>
        /// <param name="exceptionEntity">例外物件。</param>
        /// <param name="iInfo">例外處理機制的額外資訊需求介面定義。</param>
        void Handle(Type typeEntity, Exception exceptionEntity, IExceptionHandlerInfoData iInfo);
    }
}
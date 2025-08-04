using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 建立訊息字串的元件介面定義。
    /// </summary>
    public interface IExceptionMessageBuilder
    {
        /// <summary>
        /// 建立訊息字串的功能。
        /// </summary>
        /// <param name="typeEntity">發生例外的組件類別。</param>
        /// <param name="exceptionEntity">例外物件。</param>
        /// <param name="iInfo">例外處理機制的額外資訊需求介面定義。</param>
        /// <returns>訊息字串。</returns>
        string Build(Type typeEntity, Exception exceptionEntity, IExceptionHandlerInfoData iInfo);
    }
}

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 例外處理機制的額外資訊需求介面定義。
    /// </summary>
    public interface IExceptionHandlerInfoData
    {
        /// <summary>
        /// 應用程式名稱。
        /// </summary>
        string ApplicationName { get; set; }
        /// <summary>
        /// 例外處理方式列舉。
        /// </summary>
        HandleType Type { get; }
        /// <summary>
        /// 例外物件級別列舉。
        /// </summary>
        ExceptionLevel ExceptionLevel { get; set; }
    }
}
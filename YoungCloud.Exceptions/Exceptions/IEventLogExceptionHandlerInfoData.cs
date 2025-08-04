
namespace YoungCloud.Exceptions
{
    /// <summary>
    /// EventLog例外處理機制的額外資訊需求物件定義。
    /// </summary>
    public interface IEventLogExceptionHandlerInfoData : IExceptionHandlerInfoData
    {
        /// <summary>
        /// Source。
        /// </summary>
        string Caption { get; set; }
        /// <summary>
        /// LogName。
        /// </summary>
        string Location { get; set; }
    }
}

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 文字檔例外處理機制的額外資訊需求物件定義。
    /// </summary>
    public interface ITxtFileExceptionHandlerInfoData : IExceptionHandlerInfoData
    {
        /// <summary>
        /// 檔案名稱.
        /// </summary>
        string FileName { get; set; }
        /// <summary>
        /// 檔案實體路徑(不含檔名).
        /// </summary>
        string Path { get; set; }
    }
}
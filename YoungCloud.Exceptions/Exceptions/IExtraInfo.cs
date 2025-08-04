
namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 發生例外的成員額外資訊物件介面定義。
    /// </summary>
    public interface IExtraInfo
    {
        /// <summary>
        /// 額外資訊類型。
        /// </summary>
        ExtraInfoType InfoType { get; set; }
        /// <summary>
        /// 額外資訊標題。
        /// </summary>
        string Caption { get; set; }
        /// <summary>
        /// 額外資訊說明。
        /// </summary>
        string Description { get; set; }
    }
}
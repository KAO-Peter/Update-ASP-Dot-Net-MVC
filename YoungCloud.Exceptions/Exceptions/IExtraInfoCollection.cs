using System.Collections;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 發生例外的成員額外資訊物件集合定義。
    /// </summary>
    public interface IExtraInfoCollection : ICollection
    {
        /// <summary>
        /// 將額外資訊物件加入。
        /// </summary>
        /// <param name="infoType">額外資訊類型。</param>
        /// <param name="caption">額外資訊標題。</param>
        /// <param name="description">額外資訊說明。</param>
        void Add(ExtraInfoType infoType, string caption, string description);
        /// <summary>
        /// 將額外資訊物件加入。
        /// </summary>
        /// <param name="extraInfo">額外資訊物件。</param>
        void Add(IExtraInfo extraInfo);
        /// <summary>
        /// 將額外資訊物件加入。
        /// </summary>
        /// <param name="caption">額外資訊標題。</param>
        /// <param name="description">額外資訊說明。</param>
        void Add(string caption, string description);
        /// <summary>
        /// 將額外資訊物件集合加入。
        /// </summary>
        /// <param name="extraInfos">額外資訊物件集合。</param>
        void AddRange(IExtraInfo[] extraInfos);
        /// <summary>
        /// To clear data in collection.
        /// </summary>
        void Clear();
        /// <summary>
        /// 取得或設定指定索引值的額外資訊物件物件。
        /// </summary>
        /// <param name="index">索引值。</param>
        /// <returns>額外資訊物件物件。</returns>
        IExtraInfo this[int index] { get; set; }
        /// <summary>
        /// Conver the ExtraInfo collection data to array.
        /// </summary>
        /// <returns>The ExtraInfo array.</returns>
        IExtraInfo[] ToArray();
    }
}
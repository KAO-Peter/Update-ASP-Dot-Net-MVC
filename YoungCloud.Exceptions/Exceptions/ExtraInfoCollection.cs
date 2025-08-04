using System;
using System.Collections;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 發生例外的成員額外資訊物件集合。
    /// </summary>
    [Serializable]
    public class ExtraInfoCollection : CollectionBase, IExtraInfoCollection
    {
        /// <summary>
        /// 建構子。
        /// </summary>
        public ExtraInfoCollection()
        {
        }

        /// <summary>
        /// 將額外資訊物件加入。
        /// </summary>
        /// <param name="infoType">額外資訊類型。</param>
        /// <param name="caption">額外資訊標題。</param>
        /// <param name="description">額外資訊說明。</param>
        public void Add(ExtraInfoType infoType, string caption, string description)
        {
            Add(new ExtraInfo(infoType, caption, description));
        }

        /// <summary>
        /// 將額外資訊物件加入。
        /// </summary>
        /// <param name="extraInfo">額外資訊物件。</param>
        public void Add(IExtraInfo extraInfo)
        {
            List.Add(extraInfo);
        }

        /// <summary>
        /// 將額外資訊物件加入。
        /// </summary>
        /// <param name="caption">額外資訊標題。</param>
        /// <param name="description">額外資訊說明。</param>
        public void Add(string caption, string description)
        {
            Add(new ExtraInfo(caption, description));
        }

        /// <summary>
        /// 將額外資訊物件集合加入。
        /// </summary>
        /// <param name="extraInfos">額外資訊物件集合。</param>
        public void AddRange(IExtraInfo[] extraInfos)
        {
            foreach (IExtraInfo _Info in extraInfos)
            {
                List.Add(_Info);
            }
        }

        /// <summary>
        /// 取得或設定指定索引值的額外資訊物件物件。
        /// </summary>
        /// <param name="index">索引值。</param>
        /// <returns>額外資訊物件物件。</returns>
        public IExtraInfo this[int index]
        {
            get
            {
                return (IExtraInfo)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Conver the ExtraInfo collection data to array.
        /// </summary>
        /// <returns>The ExtraInfo array.</returns>
        public IExtraInfo[] ToArray()
        {
            Array _Array = Array.CreateInstance(typeof(IExtraInfo), List.Count);
            List.CopyTo(_Array, 0);
            return (IExtraInfo[])_Array;
        }
    }
}
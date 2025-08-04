using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 發生例外的成員額外資訊物件。
    /// </summary>
    [Serializable]
    public class ExtraInfo : IExtraInfo
    {
        /// <summary>
        /// 建構子。
        /// </summary>
        public ExtraInfo()
        {
        }

        /// <summary>
        /// 建構子。
        /// </summary>
        /// <param name="caption">額外資訊標題。</param>
        /// <param name="description">額外資訊說明。</param>
        /// <remarks>ExtraInfoType預設值為Argument。</remarks>
        public ExtraInfo(string caption, string description)
        {
            this.Caption = caption;
            this.Description = description;
            this.InfoType = ExtraInfoType.Argument;
        }

        /// <summary>
        /// 建構子。
        /// </summary>
        /// <param name="infoType">額外資訊類型。</param>
        /// <param name="caption">額外資訊標題。</param>
        /// <param name="description">額外資訊說明。</param>
        public ExtraInfo(ExtraInfoType infoType, string caption, string description)
        {
            this.InfoType = infoType;
            this.Caption = caption;
            this.Description = description;
        }

        /// <summary>
        /// 額外資訊類型。
        /// </summary>
        /// <remarks>預設值為Argument。</remarks>
        public ExtraInfoType InfoType
        {
            get;
            set;
        }

        /// <summary>
        /// 額外資訊標題。
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// 額外資訊說明。
        /// </summary>
        public string Description
        {
            get;
            set;
        }
    }
}
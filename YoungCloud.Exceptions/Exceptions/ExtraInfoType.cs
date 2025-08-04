using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 例外額外資訊類型的列舉。
    /// </summary>
    [Serializable]
    public enum ExtraInfoType
    {
        /// <summary>
        /// 參數類型。
        /// </summary>
        Argument = 0,
        /// <summary>
        /// 資訊類型。
        /// </summary>
        Information = 1
    }
}
using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 例外物件級別列舉。
    /// </summary>
    [Serializable]
    public enum ExceptionLevel
    {
        /// <summary>
        /// 除錯等級。
        /// </summary>
        Debug = 0,
        /// <summary>
        /// 資訊等級。
        /// </summary>
        Info = 1,
        /// <summary>
        /// 警告等級。
        /// </summary>
        Warn = 2,
        /// <summary>
        /// 錯誤等級。
        /// </summary>
        Error = 3,
        /// <summary>
        /// 毀滅等級。
        /// </summary>
        Fatal = 4
    }
}
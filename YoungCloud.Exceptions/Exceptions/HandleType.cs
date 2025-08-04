using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 例外處理方式列舉。
    /// </summary>
    [Serializable]
    public enum HandleType
    {
        /// <summary>
        /// 寫入Windows Eventlog。
        /// </summary>
        EventLog = 0,
        /// <summary>
        /// 寫入文字Log檔。
        /// </summary>
        LogTxtFile = 1,
    }
}
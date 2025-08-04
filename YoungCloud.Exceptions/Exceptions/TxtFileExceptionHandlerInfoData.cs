using System;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 文字檔例外處理機制的額外資訊需求物件。
    /// </summary>
    [Serializable]
    public class TxtFileExceptionHandlerInfoData : ExceptionInfoData, ITxtFileExceptionHandlerInfoData
    {
        /// <summary>
        /// 檔案名稱.
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// 例外處理方式列舉。
        /// </summary>
        public override HandleType Type
        {
            get
            {
                return HandleType.LogTxtFile;
            }
        }

        /// <summary>
        /// 檔案實體路徑(不含檔名).
        /// </summary>
        public string Path
        {
            get;
            set;
        }
    }
}
using System;
using System.Text;

namespace YoungCloud.Exceptions
{
    /// <summary>
    /// 建立訊息字串的元件介面定義。
    /// </summary>
    [Serializable]
    public class ExceptionMessageBuilder : IExceptionMessageBuilder
    {
        private const string TEXT_SEPARATOR = "*********************************************";
        private const string TEXT_FILE_SEPARATOR = "=============================================";
        private HandleType m_HandleType = HandleType.LogTxtFile;        //預設例外處理方式。


        /// <summary>
        /// 建構子。
        /// </summary>
        public ExceptionMessageBuilder()
        {
        }
        /// <summary>
        /// 建構子。
        /// </summary>
        /// <param name="handleType">例外處理方式列舉。</param>
        public ExceptionMessageBuilder(HandleType handleType)
        {
            m_HandleType = handleType;
        }

        /// <summary>
        /// 建立訊息字串的功能。
        /// </summary>
        /// <param name="typeEntity">發生例外的組件類別。</param>
        /// <param name="exceptionEntity">例外物件。</param>
        /// <param name="iInfo">例外處理機制的額外資訊需求介面定義。</param>
        /// <returns>訊息字串。</returns>
        public string Build(Type typeEntity, Exception exceptionEntity, IExceptionHandlerInfoData iInfo)
        {
            if (typeEntity == null) { return ""; }
            if (exceptionEntity == null) { return ""; }
            if (iInfo == null) { return ""; }
            StringBuilder _StringBuilder = new StringBuilder();
            //分隔線
            _StringBuilder.AppendFormat("{0}{1}", Environment.NewLine, TEXT_SEPARATOR);
            //發生例外的應用程式名稱
            _StringBuilder.AppendFormat("{0}ApplicationName : {1}", Environment.NewLine, iInfo.ApplicationName);
            //發生例外的組件名稱
            _StringBuilder.AppendFormat("{0}AssemblyName : {1}", Environment.NewLine, typeEntity.Assembly.FullName);
            //發生例外的模組名稱
            _StringBuilder.AppendFormat("{0}ModuleName : {1}", Environment.NewLine, typeEntity.Module.ScopeName);
            //發生例外的類別名稱
            _StringBuilder.AppendFormat("{0}TypeName : {1}.{2}", Environment.NewLine, typeEntity.Namespace, typeEntity.Name);
            //例外級別
            _StringBuilder.AppendFormat("{0}Exception Level : {1}", Environment.NewLine, iInfo.ExceptionLevel.ToString());
            //發生時間
            _StringBuilder.AppendFormat("{0}Occasion : {1}", Environment.NewLine, DateTime.Now.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss"));

            //分隔線
            _StringBuilder.AppendFormat("{0}{1}", Environment.NewLine, TEXT_SEPARATOR);
            Exception e = exceptionEntity;
            do
            {
                //例外類別
                _StringBuilder.AppendFormat("{0}Exception Type : {1}{2}{3}", Environment.NewLine, e.GetType().Namespace, ".", e.GetType().Name);
                //發生例外的應用程式或物件名稱
                _StringBuilder.AppendFormat("{0}Exception Source : {1}", Environment.NewLine, e.Source);
                //例外訊息
                _StringBuilder.AppendFormat("{0}Exception Message : {1}", Environment.NewLine, e.Message);                

                if (e is ExceptionBase)
                { //判斷Exception是不是有繼承自BaseException
                    ExceptionBase _BaseException = e as ExceptionBase;

                    //例外代碼
                    _StringBuilder.AppendFormat("{0}Error Code : {1}", Environment.NewLine, _BaseException.ErrorCode);   

                    if (_BaseException.ExtraInfos != null)
                    {
                        for (int i = 0; i < _BaseException.ExtraInfos.Length; i++)
                        {
                            //額外資訊
                            _StringBuilder.AppendFormat("{0}{1} : {2} = {3}", Environment.NewLine, _BaseException.ExtraInfos[i].InfoType.ToString(), _BaseException.ExtraInfos[i].Caption, _BaseException.ExtraInfos[i].Description);
                        }
                    }
                } 
                
                foreach (string _Key in e.Data.Keys)
                {
                    //額外資訊
                    _StringBuilder.AppendFormat("{0}{1} : {2} = {3}", Environment.NewLine, ExtraInfoType.Information.ToString(), _Key, e.Data[_Key].ToString());
                }

                //例外StackTrace
                _StringBuilder.AppendFormat("{0}Exception StackTrace : ", Environment.NewLine);
                _StringBuilder.AppendFormat("{0}{1}", Environment.NewLine, e.StackTrace);

                //分隔線
                _StringBuilder.AppendFormat("{0}{1}", Environment.NewLine, TEXT_SEPARATOR);

                e = e.InnerException;
            } while (e != null);
            if (m_HandleType == HandleType.LogTxtFile)
            {
                //分隔線
                _StringBuilder.AppendFormat("{0}{1}{1}", Environment.NewLine, TEXT_FILE_SEPARATOR);
            }
            return _StringBuilder.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HRPortal.SendMailService
{
    public class Log
    {

        static string _path;
        static string _enable;

        public static string Path
        {
            
            get {
                if (String.IsNullOrEmpty(_path)) _path = System.AppDomain.CurrentDomain.BaseDirectory;
                return _path; 
            }
            set { _path = value; }
        }

        public static string Enable
        {

            get
            {
                return _enable;
            }
            set { _enable = value; }
        }


        #region WriteLog
        /// <summary>
        /// 記錄Log
        /// </summary>
        /// <param name="sMsg">訊息內容</param>
        public static void WriteLog(string sMsg, string Prefix = "")
        {           
            string sFileName = Prefix + DateTime.Now.ToString("yyyyMMdd") + ".txt";

            if (Enable.ToUpper() != "Y") return;

            if (!System.IO.Directory.Exists(Path)) System.IO.Directory.CreateDirectory(Path);

            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(Path + sFileName, true, System.Text.Encoding.Default);
                sw.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]  " + sMsg);
            }
            catch { }
            finally
            {
                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        #endregion
    }
}

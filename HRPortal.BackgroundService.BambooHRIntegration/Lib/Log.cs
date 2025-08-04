using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace HRPortal.BackgroundService.BambooHRIntegration
{
    public class Log
    {
        private static bool EnableLog = ConfigurationManager.AppSettings["EnableLog"].ToString().ToUpper() == "Y" ? true : false;
        private static string LogPath = ConfigurationManager.AppSettings["LogPath"].ToString();
        
        #region WriteLog
        /// <summary>
        /// 記錄Log
        /// </summary>
        /// <param name="sMsg">訊息內容</param>
        public static void WriteLog(string sMsg, string Prefix = "")
        {
            if (!EnableLog)
            {
                return;
            }

            string sFileName = DateTime.Now.ToString("yyyyMMdd");
            sFileName += (string.IsNullOrWhiteSpace(Prefix) ? "" : "_") + Prefix + ".txt";

            try
            {
                if (!System.IO.Directory.Exists(LogPath))
                {
                    System.IO.Directory.CreateDirectory(LogPath);
                }
            }
            catch (Exception ex)
            {
                return;
            }

            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(LogPath + sFileName, true, System.Text.Encoding.Default);
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

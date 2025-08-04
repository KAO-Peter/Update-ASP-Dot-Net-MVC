using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace HRPortal.MaintenanceService
{
    class Log
    {
        #region WriteLog
        /// <summary>
        /// 記錄Log
        /// </summary>
        /// <param name="sMsg">訊息內容</param>
        public static void WriteLog(string sMsg, string Prefix = "")
        {
            string sEnableLog = ConfigurationManager.AppSettings[Prefix + "EnableLog"] ?? "";
            string sFileLogPath = ConfigurationManager.AppSettings[Prefix + "LogPath"] ?? "";


            string sFileName = DateTime.Now.ToString("yyyyMMdd");
            sFileName += (string.IsNullOrWhiteSpace(Prefix) ? "" : "_") + Prefix + ".txt";

            if (sEnableLog.ToUpper() != "Y") return;
            try
            {
                if (!System.IO.Directory.Exists(sFileLogPath))
                {
                    System.IO.Directory.CreateDirectory(sFileLogPath);
                }
            }
            catch (Exception ex)
            {
                return;
            }

            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(sFileLogPath + sFileName, true, System.Text.Encoding.Default);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using HRPortal.MaintenanceService;
using System.Diagnostics;

namespace HRPortal.MaintenanceService
{
    public class WarmUpTask
    {
        public WarmUpTask()
        {
        }

        public void Execute()
        {
            //針對傳入的URL清單，一一呼叫
            using (WebClient client = new WebClient())
            {
                string warmUpUrls= ConfigurationManager.AppSettings["WarmUpURLs"] ?? "";
                if (!string.IsNullOrWhiteSpace(warmUpUrls))
                {
                    string[] URL_List = warmUpUrls.Split(',');

                    for (int i = 0; i < URL_List.Length; i++)
                    {
                        try
                        {
                            Stopwatch clock = Stopwatch.StartNew();
                            string result = client.DownloadString(URL_List[i]);
                            clock.Stop();
                            //int len = (result.Length > 10) ? 10 : result.Length;
                            int len = result.Length;
                            string resultW = (result.Length > 10) ? "" : result + " ";
                          
                            Log.WriteLog(URL_List[i] + "：" + resultW + clock.ElapsedMilliseconds.ToString() + "ms", "WarmUp");
                            
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(URL_List[i] + "：發生錯誤，錯誤訊息－" + ex.Message, "WarmUp");
                        }
                    }
                }
            }
        }

    }
}

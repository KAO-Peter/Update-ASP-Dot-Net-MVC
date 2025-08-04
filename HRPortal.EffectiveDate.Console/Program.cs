using HRPortal.ApiAdapter;
using HRPortal.EffectiveDateSchedule;
using System;
using System.Configuration;
using System.IO;

namespace HRPortal.EffectiveDate.Console
{
     class Program
    {
        private static StreamWriter _logWriter;

        static void Main(string[] args)
        {
            _logWriter = new StreamWriter(File.OpenWrite(string.Format(@"C:\\PortalLog\\EffectiveDate{0}.log", DateTime.Now.ToString("yyyyMMdd_HHmm"))));
            HRMApiAdapter.GetHostUri = GetPortalURL;

            EffectiveDateTask _task = new EffectiveDateTask();
            _task.OnMessage += WriteLog;
            _task.Run().Wait();
            _logWriter.Close();
        }

        private static void WriteLog(string message)
        {
            System.Console.WriteLine(message);
            _logWriter.WriteLine(message);
            _logWriter.Flush();
        }

        public static string GetPortalURL()
        {
            return ConfigurationManager.AppSettings["HRMApiUrl"];
        }
    }
}

using HRPortal.ApiAdapter;
using System;
using System.Configuration;
using System.IO;

namespace HRPortal.JobSignOffAgents.Console
{
    class Program
    {
        private static StreamWriter _logWriter;

        static void Main(string[] args)
        {
            _logWriter = new StreamWriter(File.OpenWrite(string.Format(@"C:\\PortalLog\\JobSignOffAgents{0}.log", DateTime.Now.ToString("yyyyMMdd_HHmm"))));
            HRMApiAdapter.GetHostUri = GetPortalURL;

            JobSignOffAgentsTask _task = new JobSignOffAgentsTask();
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
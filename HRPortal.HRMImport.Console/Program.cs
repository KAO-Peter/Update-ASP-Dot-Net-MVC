using HRPortal.ApiAdapter;
using System;
using System.Configuration;
using System.IO;

namespace HRPortal.HRMImport.Console
{
    class Program
    {
        //private static FileStream _logFileStream;
        private static StreamWriter _logWriter;

        static void Main(string[] args)
        {
            string path = ConfigurationManager.AppSettings["LogPath"];
            if (String.IsNullOrEmpty(path)) path = System.AppDomain.CurrentDomain.BaseDirectory;

            //_logWriter = new StreamWriter(File.OpenWrite(string.Format("{0}.log", DateTime.Now.ToString("yyyyMMdd_HHmmss"))));
            _logWriter = new StreamWriter(File.OpenWrite(string.Format("{0}{1}.log", path, DateTime.Now.ToString("yyyyMMdd_HHmmss"))));
            HRMApiAdapter.GetHostUri = GetHostUri;

            ImportTask _task = new ImportTask();
            _task.OnMessage += WriteLog;
            _task.Run().Wait();
        }

        public static string GetHostUri()
        {
            return ConfigurationManager.AppSettings["HRMApiUrl"];
        }

        private static void WriteLog(string message)
        {
            System.Console.WriteLine(message);
            _logWriter.WriteLine(message);
        }
    }
}

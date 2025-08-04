using HRPortal.ApiAdapter;
using System.Configuration;
using System.ServiceProcess;

namespace HRPortal.SendMailService
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new Service() 
            };
            HRMApiAdapter.GetHostUri = GetHostUri;

            ServiceBase.Run(ServicesToRun);
        }

        public static string GetHostUri()
        {
            return ConfigurationManager.AppSettings["HRMApiUrl"];
        }
    }
}

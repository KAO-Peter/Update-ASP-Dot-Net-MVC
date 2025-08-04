using System.Configuration;

namespace HRPortal.Services.DDMC_PFA.Common
{
    public class AppSettingsMail
    {
        public static SendMailSettings Get()
        {
            return new SendMailSettings
            {
                TestRcpt = ConfigurationManager.AppSettings["PfaSendMailTestRcpt"],
                Subject = ConfigurationManager.AppSettings["PfaSendMailSubject"],
                Message = ConfigurationManager.AppSettings["PfaSendMailMessage"],
                IsCancel = ConfigurationManager.AppSettings["PfaSendMailIsCancel"],
                PortalWebUrl = ConfigurationManager.AppSettings["PortalWebUrl"],

                PfaSendBackMessage = ConfigurationManager.AppSettings["PfaSendBackMessage"]

            };
        }

    }

    public class SendMailSettings
    {
        public string TestRcpt { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string IsCancel { get; set; }
        public string PortalWebUrl { get; set; }
        public string PfaSendBackMessage { get; set; }
    }
}

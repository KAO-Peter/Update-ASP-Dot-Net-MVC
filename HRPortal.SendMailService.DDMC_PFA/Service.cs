using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace HRPortal.SendMailService.DDMC_PFA
{
    public partial class Service : ServiceBase
    {
        public Timer _timer;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            int _sendMailIntervalSecond = int.Parse(ConfigurationManager.AppSettings["SendMailIntervalSecond"]);

            _timer = new Timer(_sendMailIntervalSecond * 1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            using(SendMailTask _task = new SendMailTask())
            {
                _task.SendPendingMail();
            }

            _timer.Start();
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}

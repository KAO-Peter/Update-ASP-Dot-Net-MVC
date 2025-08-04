using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace HRPortal.SendMailService
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
            string logpath = Convert.ToString(ConfigurationManager.AppSettings["LogPath"]);
            string logenable = Convert.ToString(ConfigurationManager.AppSettings["LogEnable"]);

            Log.Enable = logenable;
            Log.Path = logpath;

            _timer = new Timer(_sendMailIntervalSecond * 1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                Log.WriteLog("timer stop");

                using (SendMailTask _task = new SendMailTask())
                {
                    _task.SendPendingMail();
                }

                _timer.Start();
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.Message);
            }
            finally
            {
                if (!_timer.Enabled)
                {
                    _timer.Start();
                    Log.WriteLog("timer start");
                }
            }
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}

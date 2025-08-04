using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using HRPortal.MaintenanceService;

namespace HRPortal.MaintenanceService
{
    public partial class MainService : ServiceBase
    {
        private Timer _warmUpTimer;

        public MainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            int _warmUpIntervalSecond = int.Parse(ConfigurationManager.AppSettings["WarmUpIntervalSecond"]);
            _warmUpTimer = new Timer(_warmUpIntervalSecond * 1000);
            _warmUpTimer.Elapsed += _warmUpTimer_Elapsed;
            _warmUpTimer.Start();
        }


        protected void _warmUpTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _warmUpTimer.Stop();

            WarmUpTask _task = new WarmUpTask();
            _task.Execute();
            
            _warmUpTimer.Start();
        }

        protected override void OnStop()
        {
            _warmUpTimer.Stop();
            _warmUpTimer.Dispose();
        }
    }
}

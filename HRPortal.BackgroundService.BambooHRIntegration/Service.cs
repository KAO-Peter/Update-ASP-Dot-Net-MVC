using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using HRPortal.ApiAdapter;
using HRPortal.Services;
using YoungCloud.Configurations.AutofacSettings;
using HRPortal.Services.Models.BambooHR;

namespace HRPortal.BackgroundService.BambooHRIntegration
{
    public partial class Service : ServiceBase
    {
        const int GLOBAL_TIMER_INTERVAL = 20; //檢查同步員工資料的Timer時間，每20秒跑一次Timer，但設定時間是到分鐘，所以原則上時間到一定會觸發
        private Timer _timerSyncEmployee;
        private Timer _timerCheckTimeOffStatus;
        private string _syncEmployeeDailyTime;
        private DateTime? _lastSyncDate = null;
        //private DateTime? _checkTimeOffStatusDate = null;
        private bool _enableLog = false;
        private string _logPath = "";
        private string _localIP = "";
        private string _hostURI = "";
        private string _lastCheckTimeOffResponse = ""; //紀錄上次檢查所有BambooHR架單的回傳結果，以供是否存Log的判斷(不同才存Log，減少使用空間)
        
        public Service()
        {
            InitializeComponent();
            HRMApiAdapter.GetHostUri = GetHostUri;
            //AutofacInitializer.Configure(AutofacInitializer.InitializeRegister);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                AutofacInitializer.Configure(AutofacInitializer.InitializeRegister);
            }
            catch (Exception ex)
            {
                Log.WriteLog("Autofac Error：" + ex.Message);
                if (ex is System.Reflection.ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as System.Reflection.ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;
                    Log.WriteLog("LoaderExceptions：" + string.Join("###", loaderExceptions.Select(x => x.Message).ToList()));
                }
                
            }
#if DEBUG
            Log.WriteLog("目前建置組態為DEBUG");
#endif
            Log.WriteLog("啟動背景服務...");
            int checkTimeOffIntervalSecond = int.Parse(ConfigurationManager.AppSettings["CheckTimeOffIntervalSecond"]);
            this._syncEmployeeDailyTime = ConfigurationManager.AppSettings["SyncEmployeeDailyTime"].ToString();
            Log.WriteLog("CheckTimeOffIntervalSecond=" + checkTimeOffIntervalSecond.ToString() + ", SyncEmployeeDailyTime=" + _syncEmployeeDailyTime);

            this._localIP = Util.GetLocalIPAddress();
            _timerSyncEmployee = new Timer(GLOBAL_TIMER_INTERVAL * 1000);
            _timerSyncEmployee.Elapsed += _timerSyncEmployee_Elapsed;
            _timerSyncEmployee.Start();

            _timerCheckTimeOffStatus = new Timer(checkTimeOffIntervalSecond * 1000);
            _timerCheckTimeOffStatus.Elapsed += _timerCheckTimeOffStatus_Elapsed;
            _timerCheckTimeOffStatus.Start();
        }

        //同步BambooHR員工資料
        protected void _timerSyncEmployee_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timerSyncEmployee.Stop();

            DateTime now = DateTime.Now;
            if (now.Date != this._lastSyncDate && now.ToString("HH:mm") == this._syncEmployeeDailyTime)
            {
                this._lastSyncDate = now.Date; //一天跑一次，紀錄上次跑完的日期
                try
                {
                    SyncEmployeeTask syncTask = new SyncEmployeeTask(this._localIP);
                    syncTask.Execute();
                }
                catch(Exception ex) 
                {
                    Log.WriteLog("SyncEmployee Error：" + ex.Message);
                }

                Log.WriteLog("最近一次同步日期：" + this._lastSyncDate.Value.ToString("yyyy-MM-dd"));
            }

            _timerSyncEmployee.Start();
        }

        //檢查假單狀態，依據狀況判斷是否要新增Portal假單或是更新簽核狀態
        protected void _timerCheckTimeOffStatus_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timerCheckTimeOffStatus.Stop();
            try
            {
                /*
                if (_checkTimeOffStatusDate == null || _checkTimeOffStatusDate.Value != DateTime.Now.Date)
                {
                    _checkTimeOffStatusDate = DateTime.Now.Date;
                    Log.WriteLog("服務運作中(" + _checkTimeOffStatusDate.Value.ToString("yyyy-MM-dd") + ")");
                }
                */

                CheckTimeOffStatusTask checkTimeOffTask = new CheckTimeOffStatusTask(this._localIP, this._lastCheckTimeOffResponse);
                BackgroundServiceResult finalResult = checkTimeOffTask.Execute();
                this._lastCheckTimeOffResponse = finalResult != null ? finalResult.Remark : "";
                //Log.WriteLog("上次回傳結果：" + this._lastCheckTimeOffResponse.Length.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteLog("Error：" + ex.Message);
            }
            finally
            {
                _timerCheckTimeOffStatus.Start();
            }
            
        }

        protected override void OnStop()
        {
            this._timerSyncEmployee.Dispose();
            this._timerCheckTimeOffStatus.Dispose();
            Log.WriteLog("背景服務已停止。");
        }

        private string GetHostUri()
        {
            using (HRPortal_Services _service = new HRPortal_Services())
            {
                if (string.IsNullOrWhiteSpace(this._hostURI))
                {
                    this._hostURI = _service.GetService<SystemSettingService>().GetSettingValue("HRMApiUri");
                }

                return this._hostURI;

                /*
                return "http://localhost:52530/";
                */
            }
        }
    }
}

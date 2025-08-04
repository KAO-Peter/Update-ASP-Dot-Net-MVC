using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.Services;
using HRPortal.Services.Models.BambooHR;
using HRPortal.BambooHRIntegration;

namespace HRPortal.BackgroundService.BambooHRIntegration
{
    public class SyncEmployeeTask
    {
        private string _localIP;
        private LogInfo _logInfo;

        public SyncEmployeeTask(string IP)
        {
            this._localIP = IP;
        }

        public void Execute()
        {
            LogInfo info = new LogInfo()
            {
                UserIP = this._localIP,
                Controller = "SyncEmployeeTask(Service)",
                Action = "Execute"
            };

            //以BambooHR為源頭，更新HR Portal對應員工資料
            Log.WriteLog("同步BambooHR Employee資料開始...");

            BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info);
            string result = BambooHRService.SyncBambooHREmployee();


            if (string.IsNullOrWhiteSpace(result))
            {
                Log.WriteLog("同步BambooHR Employee資料結束。");
            }
            else
            {
                Log.WriteLog("同步BambooHR Employee資料發生錯誤：" + result);
            }

            //同步BambooHR User資料
            Log.WriteLog("同步BambooHR User資料開始...");
            
            result = BambooHRService.SyncBambooHRUser();

            if (string.IsNullOrWhiteSpace(result))
            {
                Log.WriteLog("同步BambooHR User資料結束。");
            }
            else
            {
                Log.WriteLog("同步BambooHR User資料發生錯誤：" + result);
            }
            
        }
    }
}

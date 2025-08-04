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
    public class CheckTimeOffStatusTask
    {
        private string _localIP;
        //private LogInfo _logInfo;
        private string _lastCheckTimeOffResponse = "";

        public CheckTimeOffStatusTask(string IP, string LastCheckTimeOffResponse = "")
        {
            this._localIP = IP;
            this._lastCheckTimeOffResponse = LastCheckTimeOffResponse;
        }

        public BackgroundServiceResult Execute()
        {
            LogInfo info = new LogInfo()
            {
                UserIP = this._localIP,
                Controller = "CheckTimeOffStatus(Service)", //調整Controller長度，限制是30個字元
                Action = "Execute"
            };

            //Log.WriteLog("檢查BambooHR假單狀態開始...");
            DateTime checkStartTime = DateTime.Now;

            BambooHRIntegrationService BambooHRService = new BambooHRIntegrationService(info, this._lastCheckTimeOffResponse);
            BackgroundServiceResult finalResult = BambooHRService.BambooHRCheckTimeOffStatusAndSync();
            
            DateTime checkEndTime = DateTime.Now;

            if (finalResult.Success)
            {
                double totalSeconds = Math.Round((checkEndTime - checkStartTime).TotalSeconds, 2, MidpointRounding.AwayFromZero);
                if (totalSeconds > 30) //超過30秒再紀錄
                {
                    Log.WriteLog("檢查BambooHR假單狀態結束，本次檢查假單共花費" + totalSeconds.ToString() + "秒");
                }
            }
            else
            {
                string msg = "檢查BambooHR假單狀態發生錯誤：" + finalResult.ErrorMessage;
                Log.WriteLog(msg);
            }
            
#if DEBUG
            if (!string.IsNullOrWhiteSpace(finalResult.Result))
            {
                string debugMsg = "Debug訊息：" + finalResult.Result;
                Log.WriteLog(debugMsg);
            }
#endif
      
            return finalResult;
        }
    }
}

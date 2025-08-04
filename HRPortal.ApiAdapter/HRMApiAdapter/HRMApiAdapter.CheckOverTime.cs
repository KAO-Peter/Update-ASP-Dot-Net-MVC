using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<CheckOverTimeResponse> CheckOverTime(string companyCode, string empId,
            DateTime beginTime, DateTime endTime, bool checkFlag, bool toRest, bool haveDinning, bool eatFee, bool enableSettingEatingTime = false, int CutTime = 0, bool isCheckDutyCard = false,
            double InProcessingAmt = 0, string LanguageCookie = "zh-TW")
        {
            string _uri = new Uri(new Uri(_hostUri), _checkOverTimeUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{BeginTime}", beginTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{EndTime}", endTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{CheckFlag}", checkFlag.ToString());
            _uri = _uri.Replace("{ToRest}", toRest.ToString());
            _uri = _uri.Replace("{HaveDinning}", enableSettingEatingTime == true ? (haveDinning == false ? "N" : "O") : (haveDinning ? "Y" : "N"));
            _uri = _uri.Replace("{EatFee}", eatFee.ToString());
            _uri = _uri.Replace("{CutTime}", CutTime.ToString());
            _uri = _uri.Replace("{isCheckDutyCard}", isCheckDutyCard.ToString()); //20160819 加入是否檢核刷卡
            _uri = _uri.Replace("{InProcessingAmt}", InProcessingAmt.ToString());
            _uri = _uri.Replace("{LanguageCookie}", LanguageCookie);


            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CheckOverTimeResponse>(_response);
        }
    }
}

using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<RequestResult> PostOverTime(string formNo, string companyCode, string empId,
            DateTime beginTime, DateTime endTime, bool checkFlag, bool toRest, bool haveDinning, bool eatFee, string reason, string overtimeId, bool enableSettingEatingTime, int cutTime, bool isCheckDutyCard)
        {
            PostOverTimeData _data = new PostOverTimeData();
            _data.FormNo = formNo;
            _data.CompanyCode = companyCode;
            _data.EmpID = empId;
            _data.BeginTime = beginTime;
            _data.EndTime = endTime;
            _data.CheckFlag = checkFlag;
            _data.ToRest = toRest;
            _data.ToPay = toRest;
            _data.HaveDinning = enableSettingEatingTime == true ? (haveDinning ? "O" : "N") : (haveDinning ? "Y" : "N");
            _data.EatFee = eatFee;
            _data.Reason = reason;
            _data.Overtime_ID = overtimeId;
            _data.CutTime = cutTime;
            _data.isCheckDutyCard = isCheckDutyCard;

            string _uri = new Uri(new Uri(_hostUri), _postOverTimeUri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

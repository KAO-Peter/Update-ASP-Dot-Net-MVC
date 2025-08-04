using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        //20190530 Daniel 增加傳入語系
        public static async Task<CheckLeaveResponse> CheckAbsentAmount(string companyCode, string leaveCode, string empId, DateTime beginTime, DateTime endTime, string isCheck = "true", string LanguageCookie = "zh-TW") //20160811 加入檢核參數 by Bee
        {
            string _uri = new Uri(new Uri(_hostUri), _checkAbsentAmountUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{LeaveCode}", leaveCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{BeginTime}", beginTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{EndTime}", endTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{isCheck}", isCheck);
            _uri = _uri.Replace("{LanguageCookie}", LanguageCookie);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CheckLeaveResponse>(_response);
        }

    }
}

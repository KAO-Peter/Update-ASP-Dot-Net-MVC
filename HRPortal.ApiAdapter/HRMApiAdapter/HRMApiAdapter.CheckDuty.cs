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
        public static async Task<RequestResult> CheckDuty(string companyCode, string empId,
            DateTime time, int type)
        {
            string _uri = new Uri(new Uri(_hostUri), _checkDutyUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{ClockInTime}", time.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{ClockInWay}", (type == 1 ? "01" : "02"));

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

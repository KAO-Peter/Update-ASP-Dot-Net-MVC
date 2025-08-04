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
        public static async Task<List<OverTimeReasonItem>> GetOverTimeReasons(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getOverTimeReasonUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<OverTimeReasonItem>>(_response);
        }
    }
}

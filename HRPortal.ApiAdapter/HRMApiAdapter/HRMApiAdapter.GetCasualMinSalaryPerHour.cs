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
        public static async Task<string> GetCasualMinSalaryPerHour(string CompanyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getCasualMinSalaryPerHour).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
          
            string _response = await SendRequest(HttpMethod.Get, _uri);
            
            //return JsonConvert.DeserializeObject<string>(_response);
            //因為回傳單純字串，直接轉換引號
            return _response.Replace("\"", "");
        }
    }
}

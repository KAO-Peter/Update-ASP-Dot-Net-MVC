using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        /// <summary>
        /// 拿假別
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetAnnualLeaveCode(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAnnualLeaveCodeUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject < List<string>>(_response);
        }

    }
}

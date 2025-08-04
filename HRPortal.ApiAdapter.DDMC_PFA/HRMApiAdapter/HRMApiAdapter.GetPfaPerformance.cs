using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData;

namespace HRPortal.ApiAdapter.DDMC_PFA
{
    public partial class HRMApiAdapter
    {
        /// <summary>
        /// 取得分數評等區間設定
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PfaPerformanceData>> GetPfaPerformance()
        {
            string _uri = new Uri(new Uri(_hostUri), _getPfaPerformanceUri).ToString();

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<PfaPerformanceData>>(_response);
        }
    }
}

using HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.DDMC_PFA
{
    public partial class HRMApiAdapter
    {
        /// <summary>
        /// 取得職務列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CodeData>> GetJobFunction()
        {
            string _uri = new Uri(new Uri(_hostUri), _getJobFunctionUri).ToString();

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<CodeData>>(_response);
        }
    }
}

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
        public static async Task<List<CasualCycleResult>> UpdateCasualCycle(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _updateCasualCycleUri).ToString();

            CasualCycleData data = new CasualCycleData();
            data.CompanyCode = companyCode;

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<List<CasualCycleResult>>(_response);
        }
    }
}

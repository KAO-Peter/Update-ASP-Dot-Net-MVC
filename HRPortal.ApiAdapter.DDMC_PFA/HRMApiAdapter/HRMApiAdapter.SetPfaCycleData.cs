using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData;

namespace HRPortal.ApiAdapter.DDMC_PFA
{
    public partial class HRMApiAdapter
    {
        public static async Task<HRMResult> SetPfaCycleData(GetEmpPfaDataRequest data)
        {
            string _uri = new Uri(new Uri(_hostUri), _setPfaCycleDataUri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<HRMResult>(_response);
        }
    }
}

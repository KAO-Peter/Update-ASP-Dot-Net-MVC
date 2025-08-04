using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData;

namespace HRPortal.ApiAdapter.DDMC_PFA
{
    public partial class HRMApiAdapter
    {
        public static async Task<GetPfaCycleResponse> GetPfaCycle(string CompanyCode, string PfaFormNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _getPfaCycleUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{PfaFormNo}", PfaFormNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<GetPfaCycleResponse>(_response);
        }
    }
}

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
        public static async Task<List<GetEmpPfaDataResponse>> GetEmpPfaData(string CompanyCode, string PfaFormNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpPfaDataUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{PfaFormNo}", PfaFormNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<GetEmpPfaDataResponse>>(_response);
        }
    }
}

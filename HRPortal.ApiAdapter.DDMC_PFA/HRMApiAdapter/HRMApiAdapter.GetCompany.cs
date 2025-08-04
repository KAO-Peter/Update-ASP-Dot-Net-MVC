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
        /// 取得公司列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CompanyData>> GetCompany()
        {
            string _uri = new Uri(new Uri(_hostUri), _getCompanyUri).ToString();

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<CompanyData>>(_response);
        }

        public static async Task<CompanyData> GetCompanyByCode(string CompanyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getCompanyByCodeUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CompanyData>(_response);
        }
    }
}

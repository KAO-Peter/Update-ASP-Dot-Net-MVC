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
        /// <summary>
        /// 取得國籍列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<NationData>> GetNationData(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getNationUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<NationData>>(_response);
        }
    }
}

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
        /// 取得班別列表
        /// </summary>
        /// <returns></returns>
        public static async Task<List<ClassData>> GetClassData(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getClassUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<ClassData>>(_response);
        }
    }
}

using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        /// <summary>
        /// 取得緊急連絡人關係列表
        /// </summary>
        /// <returns></returns>     
        public static async Task<List<RelativesData>> GetRelatives(string CompanyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getRelativesUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<RelativesData>>(_response);
        }
    }
}

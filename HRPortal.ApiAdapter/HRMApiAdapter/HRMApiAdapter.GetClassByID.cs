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
        /// 取得班表時間
        /// </summary>
        /// <returns></returns>
        public static async Task<ClassData> GetClassByID(string companyCode, int classID)
        {
            string _uri = new Uri(new Uri(_hostUri), _getClassByIDUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{ClassID}", classID.ToString());

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<ClassData>(_response);
        }
    }
}

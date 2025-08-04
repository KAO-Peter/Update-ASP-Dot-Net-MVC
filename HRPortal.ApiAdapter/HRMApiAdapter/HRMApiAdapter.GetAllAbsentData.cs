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
        /// 拿假別資料
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<List<AbsentDetail>> GetAllAbsentData(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAllAbsentUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return (JsonConvert.DeserializeObject<List<AbsentDetail>>(_response));
        }

    }
}

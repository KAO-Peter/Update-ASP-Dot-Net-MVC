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
        /// 拿各類所得扣繳暨免扣繳憑單表單資料
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="beginYear"></param>
        /// <param name="endYear"></param>
        /// <returns></returns>
        public static async Task<List<IncomeTaxCycleGetRes>> GetIncomeTaxCycleInfoList(string companyCode, string beginYear, string endYear)
        {
            string _uri = new Uri(new Uri(_hostUri), _getIncomeTaxCycleInfoListUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{BeginYear}", beginYear);
            _uri = _uri.Replace("{EndYear}", endYear);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<IncomeTaxCycleGetRes>>(_response);
        }
    }
}

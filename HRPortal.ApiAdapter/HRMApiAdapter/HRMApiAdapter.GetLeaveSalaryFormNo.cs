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
        /// 讀取假單月結月份
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="fromNo"></param>
        /// <returns></returns>
        public static async Task<List<SalaryFormNo>> GetLeaveSalaryFormNo(string companyCode, string formNo, string statusData)
        {
            string _uri = new Uri(new Uri(_hostUri), _getLeaveSalaryFormNo).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{FormNo}", formNo);
            _uri = _uri.Replace("{StatusData}", statusData);
            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<SalaryFormNo>>(_response);
        }

    }
}

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
        /// 拿各類所得扣繳暨免扣繳憑單資料
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="empID"></param>
        /// <param name="incomeTaxYear"></param>
        /// <returns></returns>
        public static async Task<List<IncomeTaxGetRes>> GetIncomeTaxTransList(string companyCode, string empID, string incomeTaxYear)
        {
            string _uri = new Uri(new Uri(_hostUri), _getIncomeTaxTransListUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{IncomeTaxYear}", incomeTaxYear);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<IncomeTaxGetRes>>(_response);
        }
    }
}

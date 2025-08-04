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
        /// 拿假單資料
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<OvertimeFormData> GetOverTimeFormDetail(string companyCode, string formNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _getOverTimeFormDetailUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{FormNo}", formNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<OvertimeFormData>(_response);
        }
    }
}

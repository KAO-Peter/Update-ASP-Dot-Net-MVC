using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HRPortal.DBEntities;


namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        /// <summary>
        /// 拿假單詳細資料
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="FormNo"></param>
        /// <returns></returns>
        public static async Task<List<AbsentFormData>> GetLeaveFormDetail(string companyCode, string FormNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAbsentFormDetailUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{FormNo}", FormNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<AbsentFormData>>(_response);
        }
    }
}

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
        /// 拿假別
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="empId"></param>
        /// <param name="excuteDate"></param>
        /// <returns></returns>
        public static async Task<AbsentDetailAll> GetEmployeeAbsent2(string companyCode, string empId, DateTime excuteDate, string type = "")
        {
            string _uri = new Uri(new Uri(_hostUri), _getAbsent2Uri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{ExcuteDate}", excuteDate.ToString("yyyy/MM/dd"));
            _uri = _uri.Replace("{type}", type);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<AbsentDetailAll>(_response);
        }

    }
}

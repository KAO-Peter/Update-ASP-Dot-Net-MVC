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
        public static async Task<List<GetEmpScheduleClassTimeResponse>> GetEmpScheduleClassTime(string companyCode, string empId, DateTime excuteDate)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpScheduleClassTimeUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{ExcuteDate}", excuteDate.ToString("yyyy-MM-ddTHH:mm:ss"));

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<GetEmpScheduleClassTimeResponse>>(_response);
        }
    }
}

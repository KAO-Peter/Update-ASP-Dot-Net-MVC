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
        public static async Task<List<GetEmpScheduleClassTimeByStartEndTimeResponse>> GetEmpScheduleClassTimeByStartEndTime(string companyCode, string empId, DateTime startTime, DateTime endTime)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpScheduleClassTimeByStartEndTimeUrl).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{StartTime}", startTime.ToString("yyyy-MM-dd"));
            _uri = _uri.Replace("{EndTime}", endTime.ToString("yyyy-MM-dd"));

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<GetEmpScheduleClassTimeByStartEndTimeResponse>>(_response);
        }
    }
}

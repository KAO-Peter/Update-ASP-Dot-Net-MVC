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
        // 拿取在空中飄的補休
        public static async Task<List<EmpHolidayData>> GetUnLockRestLeaveInfo(string companyCode, string empId)
        {
            string _uri = new Uri(new Uri(_hostUri), _getUnLockRestLeaveInfoUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<EmpHolidayData>>(_response);
        }
    }
}
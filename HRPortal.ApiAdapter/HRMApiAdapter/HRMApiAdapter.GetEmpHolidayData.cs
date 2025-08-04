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
        // 拿員工個人假別資料標準檔
        public static async Task<List<EmpHolidayData>> GetEmpHolidayData(string companyCode, string empId, string absentID)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpHolidayDataUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{AbsentID}", absentID);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<EmpHolidayData>>(_response);
        }
    }
}

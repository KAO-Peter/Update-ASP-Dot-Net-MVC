using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<BambooHRCheckLeaveResponse> BambooHRCheckAbsentAmount(string companyCode, string absentCode, string empId, int empData_ID, DateTime beginTime, DateTime endTime, string inputAmount, string inputUnit)
        {
            string _uri = new Uri(new Uri(_hostUri), _bambooHRCheckAbsentAmountUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{AbsentCode}", absentCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{EmpData_ID}", empData_ID.ToString());
            _uri = _uri.Replace("{BeginTime}", beginTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{EndTime}", endTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{InputAmount}", inputAmount);
            _uri = _uri.Replace("{InputUnit}", inputUnit);
            //_uri = _uri.Replace("{LanguageCookie}", LanguageCookie);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<BambooHRCheckLeaveResponse>(_response);
        }
    }
}

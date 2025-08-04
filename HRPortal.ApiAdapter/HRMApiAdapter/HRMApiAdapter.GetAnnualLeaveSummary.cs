using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<DeptEmpLeaveSummaryItem> GetAnnualLeaveSummary(int year, string companyCode, string empId)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAnnualLeaveSummary).ToString();
            _uri = _uri.Replace("{Year}", year.ToString());
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            
            string _response = await SendRequest(HttpMethod.Post, _uri, empId);
            return JsonConvert.DeserializeObject<DeptEmpLeaveSummaryItem>(_response);
        }
    }
}
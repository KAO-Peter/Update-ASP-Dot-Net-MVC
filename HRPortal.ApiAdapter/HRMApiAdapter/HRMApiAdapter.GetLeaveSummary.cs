using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<DeptEmpLeaveSummaryItem> GetLeaveSummary(string companyCode, string deptCode, string empId, string StatusData, string HireId, int year)
        {
            string _uri = new Uri(new Uri(_hostUri), _getLeaveSummary).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{StatusData}", StatusData);
            _uri = _uri.Replace("{HireId}", HireId);
            _uri = _uri.Replace("{Year}", year.ToString());
           

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<DeptEmpLeaveSummaryItem>(_response);
        }

        public static async Task<DeptEmpLeaveSummaryItem> GetLeaveSummary(AbsentSummaryQueryRes data)
        {
            string _uri = new Uri(new Uri(_hostUri), _getLeaveSummary2).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<DeptEmpLeaveSummaryItem>(_response);
        }

    }
}

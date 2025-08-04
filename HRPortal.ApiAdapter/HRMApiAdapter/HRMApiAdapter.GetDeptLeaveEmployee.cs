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
        public static async Task<List<LeaveDetail>> GetDeptLeaveEmployee(string companyCode, string deptCode,
            DateTime? beginDate, DateTime? endDate, string empID = "")
        {
            string _uri = new Uri(new Uri(_hostUri), _getLeaveListUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{BeginTime}", beginDate.HasValue ? beginDate.Value.ToString("yyyy/MM/ddTHH:mm:ss") : string.Empty);
            _uri = _uri.Replace("{EndTime}", endDate.HasValue ? endDate.Value.ToString("yyyy/MM/ddTHH:mm:ss") : string.Empty);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<LeaveDetail>>(_response);
        }
    }
}

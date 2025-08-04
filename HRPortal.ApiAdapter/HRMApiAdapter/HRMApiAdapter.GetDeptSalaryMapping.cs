using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<List<string>> GetDeptSalaryEmpIDList(string empID, string isHrOrAdmin = "")
        {
            string _uri = new Uri(new Uri(_hostUri), _getGetDeptSalaryEmpIDList).ToString();
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{IsHrOrAdmin}", isHrOrAdmin);
        
            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<string>>(_response);
        }
    }
}

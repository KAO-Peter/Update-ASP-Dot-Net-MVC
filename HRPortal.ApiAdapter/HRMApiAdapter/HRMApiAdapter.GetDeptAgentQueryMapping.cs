using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<List<string>> GetAgentDeptList(string deptCode, string empID)
        {
            string _uri = new Uri(new Uri(_hostUri), _getGetAgentDeptList).ToString();
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{EmpID}", empID);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<string>>(_response);
        }
    }
}

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
        public static async Task<List<EmployeeBasicData>> GetEmployeeBasicData(string companyCode, string deptCode, string empId)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmplyeeBasicDataUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{EmpID}", empId);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            List<EmployeeBasicData> _result = JsonConvert.DeserializeObject<List<EmployeeBasicData>>(_response);
            if (_result.Count > 0)
            {
                return _result;
            }
            else
            {
                return null;
            }
        }
    }
}

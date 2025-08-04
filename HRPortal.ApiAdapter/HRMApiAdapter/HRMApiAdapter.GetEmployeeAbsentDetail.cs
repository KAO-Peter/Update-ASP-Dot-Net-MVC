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
        public static async Task<EmployeeAbsentItems> GetEmployeeAbsentDetail(string companyCode, string empId, string formNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmployeeAbsentDetail).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{FormNo}", formNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<EmployeeAbsentItems>(_response);
        }

        public static async Task<EmployeeAbsentItemsDDMC> GetEmployeeAbsentDetailDDMC(string companyCode, string empId, string formNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmployeeAbsentDetailDDMC).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{FormNo}", formNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<EmployeeAbsentItemsDDMC>(_response);
        }
    }
}

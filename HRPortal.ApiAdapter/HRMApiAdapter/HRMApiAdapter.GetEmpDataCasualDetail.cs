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
        public static async Task<GetEmpDataCasualDetail> GetEmpDataCasualDetail(string companyCode, string deptCode, string empNo = "")
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpDataCasualDetailUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{EmpNo}", empNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<GetEmpDataCasualDetail>(_response);
        }
    }
}

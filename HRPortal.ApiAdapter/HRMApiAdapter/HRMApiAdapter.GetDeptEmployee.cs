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
        /// <summary>
        /// 部門所有員工
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public static async Task<List<EmployeeData>> GetDeptEmployee(string companyCode, string deptCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmplyeeUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DeptCode}", (deptCode != null ? deptCode : string.Empty));
            _uri = _uri.Replace("{EmpID}", string.Empty);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<EmployeeData>>(_response);
        }
    }
}

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
        public static async Task<SalaryChangeNoteInfo> GetSalaryChangeNoteInfo(string companyCode, string empId, string ChangeDate)
        {
            //string CompanyCode, String EmpID, string ChangeDate
            string _uri = new Uri(new Uri(_hostUri), _getSalaryChangeNoteInfo).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{ChangeDate}", ChangeDate);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<SalaryChangeNoteInfo>(_response);
        }
    }
}

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
        public static async Task<GetEmpDataCasual> GetCasualList(string companyCode, string empID = "", string idNumber = "", int page = 1, int pageSize = 10)
        {
            string _uri = new Uri(new Uri(_hostUri), _getCasualListUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{IDNumber}", idNumber);
            _uri = _uri.Replace("{Page}", page.ToString());
            _uri = _uri.Replace("{PageSize}", pageSize.ToString());

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<GetEmpDataCasual>(_response);
        }
    }
}

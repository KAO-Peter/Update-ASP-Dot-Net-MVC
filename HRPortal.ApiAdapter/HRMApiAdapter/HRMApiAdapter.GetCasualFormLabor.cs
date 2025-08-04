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
        public static async Task<CasualFormLabor> GetCasualFormLabor(string CompanyCode, string DeptCode, string ExcuteDate, string TimeType, string EmpID, string EmpName)
        {
            string _uri = new Uri(new Uri(_hostUri), _getCasualFormLaborUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{DeptCode}", DeptCode);
            _uri = _uri.Replace("{ExcuteDate}", ExcuteDate);
            _uri = _uri.Replace("{TimeType}", TimeType);
            _uri = _uri.Replace("{EmpID}", EmpID);
            _uri = _uri.Replace("{EmpName}", EmpName);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CasualFormLabor>(_response);
        }
    }
}

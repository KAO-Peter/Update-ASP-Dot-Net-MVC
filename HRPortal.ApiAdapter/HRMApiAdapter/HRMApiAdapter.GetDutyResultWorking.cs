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
        public static async Task<GetDutyResultWorking2> GetDutyResultWorking(string companyCode, string deptCode = "", string dateStart = "", string dateEnd = "", string empID = "", string empName = "", int page = 1, int pageSize = 10)
        {
            string _uri = new Uri(new Uri(_hostUri), _getDutyResultWorkingUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{DateStart}", dateStart);
            _uri = _uri.Replace("{DateEnd}", dateEnd);
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{EmpName}", empName);           
            _uri = _uri.Replace("{Page}", page.ToString());
            _uri = _uri.Replace("{PageSize}", pageSize.ToString());

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<GetDutyResultWorking2>(_response);
        }
    }
}

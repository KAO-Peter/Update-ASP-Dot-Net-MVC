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
        public static async Task<CasualDailyPayrollChecklist> GetCasualDailyPayrollChecklist(string CompanyCode, string DeptCode, string EmpID, string Date1, string Date2, string MonthlyOrCash, string ReportType)
        {
            string _uri = new Uri(new Uri(_hostUri), _getCasualDailyPayrollChecklistUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{DeptCode}", DeptCode);
            _uri = _uri.Replace("{EmpID}", EmpID);
            _uri = _uri.Replace("{Date1}", Date1);
            _uri = _uri.Replace("{Date2}", Date2);
            _uri = _uri.Replace("{MonthlyOrCash}", MonthlyOrCash);
            _uri = _uri.Replace("{ReportType}", ReportType);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CasualDailyPayrollChecklist>(_response);
        }
    }
}

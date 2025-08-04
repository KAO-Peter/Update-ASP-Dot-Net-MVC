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
        public static async Task<CasualFormMonthly> GetCasualFormMonthly(string CompanyCode, string DeptCode, string Grouping, string Date1, string Date2, string MonthlyOrCash)
        {
            string _uri = new Uri(new Uri(_hostUri), _getCasualFormMonthlyUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{DeptCode}", DeptCode);
            _uri = _uri.Replace("{Grouping}", Grouping);
            _uri = _uri.Replace("{Date1}", Date1);
            _uri = _uri.Replace("{Date2}", Date2);
            _uri = _uri.Replace("{MonthlyOrCash}", MonthlyOrCash);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CasualFormMonthly>(_response);
        }
    }
}

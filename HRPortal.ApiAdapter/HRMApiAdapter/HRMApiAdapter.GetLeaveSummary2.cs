using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<DeptEmpLeaveSummaryItem> GetLeaveSummary2(AbsentSummaryQueryRes data)
        {
            string _uri = new Uri(new Uri(_hostUri), _getLeaveSummary2).ToString();
            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<DeptEmpLeaveSummaryItem>(_response);
        }
    }
}

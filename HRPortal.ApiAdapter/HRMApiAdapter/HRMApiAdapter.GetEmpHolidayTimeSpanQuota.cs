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
        public static async Task<List<EmpHolidayTimeSpanQuotaData>> GetEmpHolidayTimeSpanQuota(QueryEmpHolidayTimeSpanQuotaObj data)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpHolidayTimeSpanQuota).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<List<EmpHolidayTimeSpanQuotaData>>(_response);
        }
    }
}

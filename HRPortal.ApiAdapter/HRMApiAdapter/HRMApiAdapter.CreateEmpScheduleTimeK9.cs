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
        public static async Task<EmpScheduleTimeK9Result> CreateEmpScheduleTimeK9(EmpScheduleTimeK9Data data)
        {
            string _uri = new Uri(new Uri(_hostUri), _createEmpScheduleTimeK9Uri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<EmpScheduleTimeK9Result>(_response);
        }
    }
}

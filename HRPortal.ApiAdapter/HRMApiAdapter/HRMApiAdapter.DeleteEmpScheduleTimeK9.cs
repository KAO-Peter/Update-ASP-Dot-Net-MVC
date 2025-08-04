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
        public static async Task<EmpScheduleTimeK9DelResult> DeleteEmpScheduleTimeK9(EmpScheduleTimeK9DelData data)
        {
            string _uri = new Uri(new Uri(_hostUri), _deleteEmpScheduleTimeK9Uri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<EmpScheduleTimeK9DelResult>(_response);
        }
    }
}

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
        public static async Task<string> WakeUpHRMAPI()
        {
            string _uri = new Uri(new Uri(_hostUri), _warmUpAPIUri).ToString();

            string _response = await SendRequest(HttpMethod.Get, _uri);

            return "Done!";
        }
    }
}

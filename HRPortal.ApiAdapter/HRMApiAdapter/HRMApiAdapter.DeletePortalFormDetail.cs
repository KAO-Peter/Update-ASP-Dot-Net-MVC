using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System.Net.Http;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<RequestResult> DeletePortalFormDetail(string FormNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _DeletePortalFormDetailUri).ToString();
            _uri = _uri.Replace("{FormNo}", FormNo);
                      
            string _response = await SendRequest(HttpMethod.Post, _uri);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

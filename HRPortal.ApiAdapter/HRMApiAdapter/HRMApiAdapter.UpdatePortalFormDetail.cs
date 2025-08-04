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
        public static async Task<RequestResult> UpdatePortalFormDetail(string FormNo, string Status, string UpdateEmpID = "")
        {
            string _uri = new Uri(new Uri(_hostUri), _UpdatePortalFormDetailUri).ToString();
            _uri = _uri.Replace("{FormNo}", FormNo);
            _uri = _uri.Replace("{Status}", Status);
            _uri = _uri.Replace("{UpdateEmpID}", UpdateEmpID);

            string _response = await SendRequest(HttpMethod.Post, _uri);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

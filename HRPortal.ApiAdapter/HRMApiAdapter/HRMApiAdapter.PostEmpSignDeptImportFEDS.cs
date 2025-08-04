using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task PostEmpSignDeptImportFEDS(List<BpmModelForFEDS> BpmModel)
        {
            string _uri = new Uri(new Uri(_hostUri), _postEmpSignDeptImportUri).ToString();
            var _response = await SendRequest(HttpMethod.Post, _uri, BpmModel);
            
        }
    }
}

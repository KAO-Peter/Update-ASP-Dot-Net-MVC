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
        public static async Task<DeleteResponse> DeleteOverTime(string companyCode, string empId, string formNo)
        {
            DeleteRequestData _data = new DeleteRequestData();
            _data.CompanyCode = companyCode;
            _data.EmpID = empId;
            _data.FormNo = formNo;

            string _uri = new Uri(new Uri(_hostUri), _postDeleteOverTimeUri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<DeleteResponse>(_response);
        }
    }
}

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
        public static async Task<EmpChangeK9Result> CreateEmpChangeK9(string companyCode, string empID, DateTime excuteDate)
        {
            string _uri = new Uri(new Uri(_hostUri), _createEmpChangeK9Uri).ToString();

            EmpChangeK9Data data = new EmpChangeK9Data();
            data.CompanyCode = companyCode;
            data.EmpID = empID;
            data.ExcuteDate = excuteDate;
            //data.UserID = userID;

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<EmpChangeK9Result>(_response);
        }
    }
}

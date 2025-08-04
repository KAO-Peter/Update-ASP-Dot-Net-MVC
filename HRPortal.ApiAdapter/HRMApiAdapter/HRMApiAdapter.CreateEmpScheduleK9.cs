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
        public static async Task<EmpScheduleK9Result> CreateEmpScheduleK9(string companyCode, string empID, DateTime excuteDate, int class_ID)
        {
            string _uri = new Uri(new Uri(_hostUri), _createEmpScheduleK9Uri).ToString();

            EmpScheduleK9Data data = new EmpScheduleK9Data();
            data.CompanyCode = companyCode;
            data.EmpID = empID;
            data.ExcuteDate = excuteDate;
            data.Class_ID = class_ID;

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<EmpScheduleK9Result>(_response);
        }
    }
}

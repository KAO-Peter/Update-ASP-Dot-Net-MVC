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
        public static async Task<EmpLaborK9Result> CreateEmpLaborK9(string companyCode, string empID, DateTime excuteDate, double sumSalary, Guid casualFormID, string status)
        {
            string _uri = new Uri(new Uri(_hostUri), _createEmpLaborK9Uri).ToString();

            EmpLaborK9Data data = new EmpLaborK9Data();
            data.CompanyCode = companyCode;
            data.EmpID = empID;
            data.ExcuteDate = excuteDate;
            data.SumSalary = sumSalary;
            //data.UserID = userID;
            data.Portal_CasualFormID = casualFormID;
            data.Status = status;

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<EmpLaborK9Result>(_response);
        }
    }
}

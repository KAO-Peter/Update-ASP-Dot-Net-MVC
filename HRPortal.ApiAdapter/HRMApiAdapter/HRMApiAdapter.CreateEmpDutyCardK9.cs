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
        public static async Task<EmpDutyCardK9Result> CreateEmpDutyCardK9(string companyCode, string empID, DateTime excuteDate, string cardNo, bool isDel = false)
        {
            string _uri = new Uri(new Uri(_hostUri), _createEmpDutyCardK9Uri).ToString();

            EmpDutyCardK9Data data = new EmpDutyCardK9Data();
            data.CompanyCode = companyCode;
            data.EmpID = empID;
            data.ExcuteDate = excuteDate;
            data.CardNo = cardNo;
            //data.CardKeep = cardKeep;
            data.IsDel = isDel;

            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<EmpDutyCardK9Result>(_response);
        }
    }
}

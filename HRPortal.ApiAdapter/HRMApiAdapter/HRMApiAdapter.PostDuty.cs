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
        public static async Task<RequestResult> PostDuty(string companyCode, string empId, DateTime time,
            int type, int reasonType, string reason = "")
        {
            PostDutyData _data = new PostDutyData();
            _data.CompanyCode = companyCode;
            _data.EmpID = empId;
            _data.ClockInTime = time;
            _data.ClockInWay = (type == 1 ? "01" : "02");
            _data.ClockInReason = reasonType.ToString();
            _data.Reason = reason;

            string _uri = new Uri(new Uri(_hostUri), _postDutyUri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

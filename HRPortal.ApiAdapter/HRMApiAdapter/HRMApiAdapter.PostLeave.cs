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
        public static async Task<RequestResult> PostLeave(string formNo, string companyCode, string empId,
            DateTime beginTime, DateTime endTime, string leaveCode, string absentReason, string agentId)
        {
            PostLeaveData _data = new PostLeaveData();
            _data.FormNo = formNo;
            _data.CompanyCode = companyCode;
            _data.EmpID = empId;
            _data.BeginTime = beginTime;
            _data.EndTime = endTime;
            _data.LeaveCode = leaveCode;
            _data.AbsentReason = absentReason;
            _data.AgentID = agentId;

            string _uri = new Uri(new Uri(_hostUri), _postLeaveUri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

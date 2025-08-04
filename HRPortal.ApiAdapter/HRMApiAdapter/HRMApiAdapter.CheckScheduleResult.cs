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
        public static async Task<CheckLeaveResponse> CheckScheduleResult(string companyCode, string DepartmentCode, string empId,
            string leaveCode,string agentId, DateTime NowTime, DateTime beginTime, DateTime endTime, string isCheck = "true") //20160811 加入檢核參數 by Bee
        {
            string _uri = new Uri(new Uri(_hostUri), _checkScheduleResultUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{DepartmentCode}", DepartmentCode);
            _uri = _uri.Replace("{EmpID}", empId);
            _uri = _uri.Replace("{LeaveCode}", leaveCode);
            _uri = _uri.Replace("{AgentID}", agentId ?? string.Empty);
            _uri = _uri.Replace("{NowTime}", NowTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{BeginTime}", beginTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{EndTime}", endTime.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{AbsentReason}", string.Empty);
            _uri = _uri.Replace("{isCheck}", isCheck);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<CheckLeaveResponse>(_response);
        }
    }
}

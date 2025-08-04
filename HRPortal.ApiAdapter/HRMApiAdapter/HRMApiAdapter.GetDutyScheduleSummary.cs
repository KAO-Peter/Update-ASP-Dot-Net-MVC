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
        /// <summary>
        /// 取得部門人員出缺勤彙總表
        /// </summary>
        /// <returns></returns>     
        public static async Task<List<DutyScheduleSummary>> GetDutyScheduleSummary(string CompanyCode, string DeptCode, string EmpID, string StatusData, DateTime? BeginTime, DateTime? EndTime, string HireNo = "")
        {
            string _uri = new Uri(new Uri(_hostUri), _getDutyScheduleSummaryUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{DeptCode}", DeptCode);
            _uri = _uri.Replace("{EmpID}", EmpID);
            _uri = _uri.Replace("{StatusData}", StatusData);
            _uri = _uri.Replace("{BeginTime}", BeginTime.HasValue ? BeginTime.Value.ToString("yyyy/MM/dd hh:mm:ss") : string.Empty);
            _uri = _uri.Replace("{EndTime}", EndTime.HasValue ? EndTime.Value.ToString("yyyy/MM/dd hh:mm:ss") : string.Empty);
            _uri = _uri.Replace("{HireNo}", HireNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject< List<DutyScheduleSummary>>(_response);
            
        }
    }
}

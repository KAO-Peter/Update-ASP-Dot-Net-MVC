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
        /// <summary>
        /// 拿加班單資料
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<List<AbsentFormData>> GetOverTimeFormData(string companyCode, string empID, string deptCode, string StatusData, DateTime? startTime, DateTime? endTime, string LanguageCookie = "zh-TW")
        {
            string _uri = new Uri(new Uri(_hostUri), _getOverTimeFormDataUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{DeptCode}", deptCode);
            _uri = _uri.Replace("{StatusData}", StatusData);
            _uri = _uri.Replace("{StartTime}", startTime.HasValue ? startTime.Value.ToString("yyyy/MM/ddTHH:mm:ss") : string.Empty);
            _uri = _uri.Replace("{EndTime}", endTime.HasValue ? endTime.Value.ToString("yyyy/MM/ddTHH:mm:ss") : string.Empty);
            _uri = _uri.Replace("{LanguageCookie}", LanguageCookie);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<AbsentFormData>>(_response);
        }
    }
}

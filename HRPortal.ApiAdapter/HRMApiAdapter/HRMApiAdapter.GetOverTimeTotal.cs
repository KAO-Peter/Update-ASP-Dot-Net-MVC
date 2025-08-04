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
        /// 拿加班單總時數
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<EmpOverTimeTotal> GetOverTimeFormData(string empID, DateTime? startTime, DateTime? endTime)
        {
            string _uri = new Uri(new Uri(_hostUri), _getOverTimeTotalUri).ToString();
            _uri = _uri.Replace("{EmpID}", empID);
            _uri = _uri.Replace("{StartTime}", startTime.HasValue ? startTime.Value.ToString("yyyy/MM/ddTHH:mm:ss") : string.Empty);
            _uri = _uri.Replace("{EndTime}", endTime.HasValue ? endTime.Value.ToString("yyyy/MM/ddTHH:mm:ss") : string.Empty);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<EmpOverTimeTotal>(_response);
            
        }
    }
}

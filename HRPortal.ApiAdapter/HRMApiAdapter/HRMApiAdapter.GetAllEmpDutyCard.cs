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
        /// 取得使用中出勤卡號
        /// </summary>
        /// <returns></returns>
        public static async Task<List<EmpDutyCardInfoData>> GetAllEmpDutyCard(string companyCode, DateTime excuteDate)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAllEmpDutyCardUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{ExcuteDate}", excuteDate.ToString("yyyy/MM/dd"));

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<EmpDutyCardInfoData>>(_response);
        }
    }
}

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
        /// 取得BambooHR需要的假單資料(歷史假單匯入用)
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AbsentDataForBambooHR>> GetAbsentDataForBambooHR(string CompanyCode, string EmpID, List<string> AbsentCodeList, DateTime BeginDate, DateTime EndDate)
        {
            AbsentDataForBambooHRQueryObj _data = new AbsentDataForBambooHRQueryObj()
            {
                CompanyCode = CompanyCode,
                EmpID = EmpID,
                AbsentCodeList = AbsentCodeList,
                BeginDate = BeginDate,
                EndDate = EndDate
            };
            string _uri = new Uri(new Uri(_hostUri), _getAbsentDataForBambooHR).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<List<AbsentDataForBambooHR>>(_response);
        }
    }
}

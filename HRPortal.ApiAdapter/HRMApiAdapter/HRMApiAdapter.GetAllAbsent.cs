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
        /// 拿假別
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> GetAllAbsent(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAllAbsentUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);

            //return (JsonConvert.DeserializeObject<List<AbsentDetail>>(_response)).ToDictionary(x => x.Code, x => x.Name);
            //20190520 Daniel 增加英文語系處理-->改用新函式處理
            return (JsonConvert.DeserializeObject<List<AbsentDetail>>(_response)).ToDictionary(x => x.Code, x => x.Name);

        }


        /// <summary>
        /// 取得公司所有假別(含英文名稱)
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static async Task<List<AbsentType>> GetAllAbsentType(string companyCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getAllAbsentUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);

            //return (JsonConvert.DeserializeObject<List<AbsentDetail>>(_response)).ToDictionary(x => x.Code, x => x.Name);
            //20190520 Daniel 增加英文語系處理
            return (JsonConvert.DeserializeObject<List<AbsentDetail>>(_response)).Select(x => new AbsentType() { AbsentCode = x.Code, AbsentName = x.Name, AbsentEnglishName = x.AbsentNameEn }).ToList();

        }

    }
}

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
        public static async Task<double> GetCanUseCount(string CompanyCode, 
            string EmpID, DateTime begin, DateTime end, string AbsentCode)
        {
            string _uri = new Uri(new Uri(_hostUri), _getEmpCanUseCount).ToString();
            _uri = _uri.Replace("{CompanyCode}", CompanyCode);
            _uri = _uri.Replace("{EmpID}", EmpID);
            _uri = _uri.Replace("{begin}", begin.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{end}", end.ToString("yyyy-MM-ddTHH:mm:ss"));
            _uri = _uri.Replace("{AbsentCode}", AbsentCode);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return Convert.ToDouble(_response);//(JsonConvert.DeserializeObject<double>(_response));
        }

    }
}

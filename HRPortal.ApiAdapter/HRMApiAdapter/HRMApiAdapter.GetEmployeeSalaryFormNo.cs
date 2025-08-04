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
        /// <param name="empId"></param>
        /// <param name="excuteDate"></param>
        /// <returns></returns>
        public static async Task<List<SalaryFormNo>> GetEmployeeSalaryFormNo(string companyCode, string empId, DateTime? beginMonth, DateTime? endMonth)
        {
            string _uri = new Uri(new Uri(_hostUri), _getSalaryFormNo).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            if (beginMonth.HasValue)
            {
                _uri = _uri.Replace("{SalaryBeginMonth}", beginMonth.Value.ToString("yyyy/MM"));
            }
            else
            {
                _uri = _uri.Replace("{SalaryBeginMonth}", string.Empty);
            }
            if (endMonth.HasValue)
            {
                _uri = _uri.Replace("{SalaryEndMonth}", endMonth.Value.ToString("yyyy/MM"));
            }
            else
            {
                _uri = _uri.Replace("{SalaryEndMonth}", string.Empty);
            }

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<SalaryFormNo>>(_response);
        }

        public static async Task<List<SalaryFormNo>> GetEmployeeSalaryFormNoAllStatus(string companyCode, string empId, DateTime? beginMonth, DateTime? endMonth)
        {
            string _uri = new Uri(new Uri(_hostUri), _getSalaryFormNoAllStatus).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{EmpID}", empId);
            if (beginMonth.HasValue)
            {
                _uri = _uri.Replace("{SalaryBeginMonth}", beginMonth.Value.ToString("yyyy/MM"));
            }
            else
            {
                _uri = _uri.Replace("{SalaryBeginMonth}", string.Empty);
            }
            if (endMonth.HasValue)
            {
                _uri = _uri.Replace("{SalaryEndMonth}", endMonth.Value.ToString("yyyy/MM"));
            }
            else
            {
                _uri = _uri.Replace("{SalaryEndMonth}", string.Empty);
            }

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return JsonConvert.DeserializeObject<List<SalaryFormNo>>(_response);
        }
    }
}

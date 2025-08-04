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
        public static async Task<List<DepartmentSalarySummaryItem>> GetDepartmentSalarySummary(string companyCode, List<string> departmentCode, string formNo)
        {
            DepartmentSalarySummaryPostData _data = new DepartmentSalarySummaryPostData()
            {
                CompanyCode = companyCode,
                DeptCode = departmentCode,
                FormNo = formNo
            };
            string _uri = new Uri(new Uri(_hostUri), _getDepartmentSalarySummary).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<List<DepartmentSalarySummaryItem>>(_response);
        }
    }
}
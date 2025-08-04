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
        public static async Task<List<DepartmentLeaveSummaryItem>> GeneralGetLeaveEmployee(string companyCode, string DeptCodee, List<string> departmentCode, string empId, DateTime beginDate, DateTime endDate)
        {
            DepartmentLeaveSummaryPostData _data = new DepartmentLeaveSummaryPostData()
            {
                CompanyCode = companyCode,
                DeptCodee = DeptCodee,
                DeptCode = departmentCode,
                empId = empId,
                beginDate = beginDate,
                endDate = endDate
            };
            string _uri = new Uri(new Uri(_hostUri), _postGeneralLeaveListUri).ToString();
            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<List<DepartmentLeaveSummaryItem>>(_response);
        }
    }
}

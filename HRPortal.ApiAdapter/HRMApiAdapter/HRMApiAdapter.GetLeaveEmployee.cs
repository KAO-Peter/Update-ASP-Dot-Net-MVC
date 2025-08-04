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
        /// 
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="DeptCodee">使用者所屬部門ID</param>
        /// <param name="departmentCode">使用者要簽核的部門ID集合</param>
        /// <param name="empId"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="empIds">指定被使用者簽核的員工代號集合</param>
        /// <returns></returns>
        public static async Task<List<DepartmentLeaveSummaryItem>> GetLeaveEmployee(string companyCode, string DeptCodee, 
            List<string> departmentCode, string empId, DateTime beginDate, DateTime endDate , List<string> empIds)
        {
            DepartmentLeaveSummaryPostData _data = new DepartmentLeaveSummaryPostData()
            {
                CompanyCode = companyCode,
                DeptCodee=DeptCodee,
                DeptCode = departmentCode,
                empId = empId,
                beginDate = beginDate,
                endDate = endDate , 
                empIds = empIds
            };
            string _uri = new Uri(new Uri(_hostUri), _postLeaveListUri).ToString();
            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<List<DepartmentLeaveSummaryItem>>(_response);
        }
    }
}

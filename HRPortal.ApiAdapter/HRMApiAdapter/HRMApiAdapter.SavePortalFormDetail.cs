using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRPortal.ApiAdapter.HRMApiAdapterData;
using Newtonsoft.Json;
using System.Net.Http;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<RequestResult> SavePortalFormDetail(string LeaveForm_ID, string FormNo, int EmpData_ID, string EmpID, DateTime BeginTime, DateTime EndTime, string AbsentUnit, double WorkHours, double TotalAmount, List<EmpAbsentCheckDetail> EmpAbsentCheckDetailList, string CreateEmpID)
        {
            string _uri = new Uri(new Uri(_hostUri), _SavePortalFormDetailUri).ToString();

            PortalFormDetailData data = new PortalFormDetailData();

            data.LeaveForm_ID = LeaveForm_ID;
            data.FormNo = FormNo;
            data.EmpData_ID = EmpData_ID;
            data.EmpID = EmpID;
            data.BeginTime = BeginTime;
            data.EndTime = EndTime;
            data.AbsentUnit = AbsentUnit;
            data.WorkHours = WorkHours;
            data.TotalAmount = TotalAmount;
            data.EmpAbsentCheckDetailList = EmpAbsentCheckDetailList;
            data.CreateEmpID = CreateEmpID;


            string _response = await SendRequest(HttpMethod.Post, _uri, data);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

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
        //20170712 Daniel 加上EmpID供Log紀錄
        public static async Task<RequestResult> SaveDutyResultWorking(DutyResultWork[] data, string CompanyCode, string EmpID = "")
        {
            SaveDutyResultWorking SaveData = new SaveDutyResultWorking { CompanyCode = CompanyCode, EmpID = EmpID, Data = data };

            string _uri = new Uri(new Uri(_hostUri), _saveDutyResultWorkingUri).ToString();

          
            string _response = await SendRequest(HttpMethod.Post, _uri, SaveData);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }

        //20170712 Daniel 加上EmpID供Log紀錄
        //
        public static async Task<RequestResult> SaveDutyResultWorking2(DutyResultWork2[] data, string CompanyCode, string EmpID = "")
        {
            //SaveDutyResultWorking SaveData = new SaveDutyResultWorking { CompanyCode = CompanyCode, EmpID = EmpID, Data = data };

            //string _uri = new Uri(new Uri(_hostUri), _saveDutyResultWorkingUri).ToString();

            SaveDutyResultWorking2 SaveData = new SaveDutyResultWorking2 { CompanyCode = CompanyCode, EmpID = EmpID, Data = data };

            string _uri = new Uri(new Uri(_hostUri), _saveDutyResultWorking2Uri).ToString();

            string _response = await SendRequest(HttpMethod.Post, _uri, SaveData);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

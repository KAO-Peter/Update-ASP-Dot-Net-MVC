using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.DDMC_PFA
{
    public partial class HRMApiAdapter
    {
        private static string _hostUri
        {
            get
            {
                if (GetHostUri != null)
                    return GetHostUri();
                else
                    throw new Exception("No delegate function for get host uri.");
            }
        }
        
        private static string _getCompanyUri = "api/Employee/GetCompanyList";
        private static string _getCompanyByCodeUri = "api/Employee/GetCompanyByCode?CompanyCode={CompanyCode}";
        private static string _getAllDDepartmentUri = "api/Employee/GetAllDeptList?CompanyCode={CompanyCode}";

        //績效考核批號
        private static string _getPfaCycleFormNoUri = "api/PfaCycle/GetPfaCycleFormNo?CompanyCode={CompanyCode}";
        private static string _getPfaCycleUri = "api/PfaCycle/GetPfaCycle?CompanyCode={CompanyCode}&PfaFormNo={PfaFormNo}";
        private static string _getEmpPfaDataUri = "api/PfaCycle/GetEmpPfaData?CompanyCode={CompanyCode}&PfaFormNo={PfaFormNo}";
        private static string _setPfaCycleDataUri = "api/PfaCycle/SetPfaCycleData";
        private static string _getPfaPerformanceUri = "api/PfaCycle/GetPfaPerformanceList";
        private static string _getHireUri = "api/Employee/GetHireList"; 
        private static string _getPositionUri = "api/Employee/GetPositionList";
        private static string _getGradeUri = "api/Employee/GetGradeList";
        private static string _getJobTitleUri = "api/Employee/GetJobTitleList";
        private static string _getJobFunctionUri = "api/Employee/GetJobFunctionList";

        public static Func<string> GetHostUri;

        private static async Task<string> SendRequest(HttpMethod method, string uri, object data = null)
        {
            string _responseBody = string.Empty;

            using (HttpClient _client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpResponseMessage _response;

                try
                {
                    if (method == HttpMethod.Get)
                    {
                        _response = await _client.GetAsync(uri);
                    }
                    else if (method == HttpMethod.Post)
                    {
                        _response = await _client.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    _response.EnsureSuccessStatusCode();
                    _responseBody = await _response.Content.ReadAsStringAsync();
                }
                catch (Exception ex) // SSL certificate error
                {
                    throw ex;
                }
            }

            return _responseBody;
        }
    }
}

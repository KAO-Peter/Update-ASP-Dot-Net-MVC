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
        public static async Task<RequestResult> PostModifyEmpData(string CompanyCode, string EmpID, string RegisterAddress, string Address, string TelephoneNumber,
            string CellphoneNumber, string EmergencyName, string EmergencyTelephoneNumber, string EmergencyAddress, string CompanyEmail, string RegisterTelephoneNumber,string EmergencyRelation)
        {
            PostModifyEmpData _data = new PostModifyEmpData();
            //後臺名稱    //前台名稱
            _data.EmpID = EmpID;
            _data.CompanyCode = CompanyCode;
            _data.RegisterAddress = RegisterAddress;
            _data.RegisterTel = RegisterTelephoneNumber;
            _data.Address = Address;
            _data.Tel = TelephoneNumber;
            _data.Mobile = CellphoneNumber;
            _data.EmergencyName = EmergencyName;
            _data.EmergencyPhone = EmergencyTelephoneNumber;
            _data.EmergencyAddress = EmergencyAddress;
            _data.CompanyEmail = CompanyEmail;
            _data.EmergencyRelation = EmergencyRelation;
            

            string _uri = new Uri(new Uri(_hostUri), _PostModifyEmpDataUri).ToString();
            string _response = await SendRequest(HttpMethod.Post, _uri, _data);
            return JsonConvert.DeserializeObject<RequestResult>(_response);
        }
    }
}

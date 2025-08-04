using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter
{
    public partial class HRMApiAdapter
    {
        public static async Task<bool> GetCheckFormNo(string companyCode, string FormNo)
        {
            string _uri = new Uri(new Uri(_hostUri), _checkFormNoUri).ToString();
            _uri = _uri.Replace("{CompanyCode}", companyCode);
            _uri = _uri.Replace("{FormNo}", FormNo);

            string _response = await SendRequest(HttpMethod.Get, _uri);
            return _response == "true";
        }
    }
}

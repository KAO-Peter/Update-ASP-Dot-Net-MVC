using System.Collections.Generic;

namespace HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData
{
    public class GetEmpPfaDataRequest
    {
        /// <summary>
        /// 公司別代碼
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 員工績效考核資料
        /// </summary>
        public List<GetEmpPfaDataResponse> GetEmpPfaData { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    /// <summary>
    /// 查詢保費員工基本資料
    /// </summary>
    public class EmpInsureGetReq
    {
        /// <summary>
        /// 公司名稱 (中文)
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司代碼
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        ///  勞保保險證號碼
        /// </summary>
        public string LaborNo { get; set; }
        /// <summary>
        ///  健保保險證號碼
        /// </summary>
        public string HealthNo { get; set; }
        /// <summary>
        ///  年度(民國年)
        /// </summary>
        public string HealthYear { get; set; }
        /// <summary>
        ///  姓名
        /// </summary>
        public string EmpName { get; set; }
        /// <summary>
        ///  身份證字號
        /// </summary>
        public string IDNumber { get; set; }
        /// <summary>
        ///  員工工號
        /// </summary>
        public string EmpID { get; set; }
    }
}

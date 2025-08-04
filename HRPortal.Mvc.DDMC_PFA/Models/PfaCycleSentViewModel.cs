using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 初核送出資料
    /// </summary>
    public class PfaCycleSentDataViewModel
    {
        /// <summary>
        ///  績效考核ID
        /// </summary>
        public Guid PfaCycleID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 考績年度
        /// </summary>
        public string PfaYear { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 簽核意見
        /// </summary>
        public string Assessment { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public List<SelectListItem> PfaFormNoList { get; set; }

        /// <summary>
        /// 初核送出
        /// </summary>
        public List<PfaCycleSentViewModel> PfaCycleDeptSent { get; set; }

        /// <summary>
        /// 複核送出
        /// </summary>
        public List<PfaCycleOrgSentViewModel> PfaCycleOrgSent { get; set; }
    }

    /// <summary>
    /// 初核送出
    /// </summary>
    public class PfaCycleSentViewModel
    {
        /// <summary>
        /// 考核部門ID
        /// </summary>
        public Guid PfaDeptID { get; set; }

        /// <summary>
        /// 考核部門代號
        /// </summary>
        public string PfaDeptCode { get; set; }

        /// <summary>
        /// 考核部門名稱
        /// </summary>
        public string PfaDeptName { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int Ordering { get; set; }

        /// <summary>
        /// 應評核人數
        /// </summary>
        public int FirstAll { get; set; }

        /// <summary>
        /// 已評核人數
        /// </summary>
        public int FirstFinal { get; set; }

        /// <summary>
        /// 簽核狀態
        /// </summary>
        public string SignStatus { get; set; }
        /// <summary>
        /// 簽核狀態 已送出
        /// </summary>
        public string SignStatus_Y { get { return "Y"; } }
        /// <summary>
        /// 簽核狀態 未送出
        /// </summary>
        public string SignStatus_N { get { return "N"; } }
    }

    /// <summary>
    /// 複核送出
    /// </summary>
    public class PfaCycleOrgSentViewModel
    {
        /// <summary>
        /// 部門組織ID
        /// </summary>
        public Guid PfaOrgID { get; set; }

        /// <summary>
        /// 部門組織代號
        /// </summary>
        public string PfaOrgCode { get; set; }

        /// <summary>
        /// 部門組織名稱
        /// </summary>
        public string PfaOrgName { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int Ordering { get; set; }

        /// <summary>
        /// 應配比人數
        /// </summary>
        public int RationAll { get; set; }

        /// <summary>
        /// 已評核人數
        /// </summary>
        public int SecondFinal { get; set; }

        /// <summary>
        /// 是否配比正確
        /// </summary>
        public string IsRation { get; set; }

        /// <summary>
        /// 簽核狀態
        /// </summary>
        public string SignStatus { get; set; }
    }
}

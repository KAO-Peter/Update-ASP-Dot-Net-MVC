using System;
using System.Collections.Generic;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核簽核附件資料
    /// </summary>
    public class PfaSignUploadDataViewModel
    {
        /// <summary>
        /// 上傳權限
        /// </summary>
        public bool IsUpload { get; set; }

        /// <summary>
        /// 績效考核簽核附件
        /// </summary>
        public List<PfaSignUploadViewModel> PfaSignUpload { get; set; }
    }

    /// <summary>
    /// 績效考核簽核附件
    /// </summary>
    public class PfaSignUploadViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// 考核簽核流程ID
        /// </summary>
        public Guid? PfaSignProcessID { get; set; }

        /// <summary>
        /// 考核員工資料ID
        /// </summary>
        public Guid? PfaCycleEmpID { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int Ordering { get; set; }
    }
}

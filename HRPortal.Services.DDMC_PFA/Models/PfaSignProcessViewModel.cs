using System;
using System.Collections.Generic;

namespace HRPortal.Services.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核簽核流程資料
    /// </summary>
    public class PfaSignProcessDataViewModel: Result
    {
        /// <summary>
        /// 考核員工電子郵件
        /// </summary>
        public string EmpEmail { get; set; }

        //員工ID
        public Guid EmployeeID { get; set; }

        /// <summary>
        /// 績效考核簽核流程
        /// </summary>
        public List<PfaSignProcessViewModel> PfaSignProcess { get; set; }
    }

    /// <summary>
    /// 績效考核簽核流程
    /// </summary>
    public class PfaSignProcessViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 考核員工資料ID
        /// </summary>
        public Guid PfaCycleEmpID { get; set; }

        /// <summary>
        /// 簽核步驟
        /// </summary>
        public int SignStep { get; set; }

        /// <summary>
        /// 簽核關卡
        /// </summary>
        public Guid SignLevelID { get; set; }

        /// <summary>
        /// 簽核關卡代號
        /// </summary>
        public string SignLevelCode { get; set; }

        /// <summary>
        /// 簽核關卡名稱
        /// </summary>
        public string SignLevelName { get; set; }

        /// <summary>
        /// 是否自評
        /// </summary>
        public bool IsSelfEvaluation { get; set; }

        /// <summary>
        /// 是否初核
        /// </summary>
        public bool IsFirstEvaluation { get; set; }

        /// <summary>
        /// 是否複核
        /// </summary>
        public bool IsSecondEvaluation { get; set; }

        /// <summary>
        /// 是否核決
        /// </summary>
        public bool IsThirdEvaluation { get; set; }

        /// <summary>
        /// 上傳權限
        /// </summary>
        public bool IsUpload { get; set; }

        /// <summary>
        /// 身份類別ID
        /// </summary>
        public Guid PfaEmpTypeID { get; set; }

        /// <summary>
        /// 是否代理自評
        /// </summary>
        public bool IsAgent { get; set; }

        /// <summary>
        /// 是否需配比人數
        /// </summary>
        public bool IsRatio { get; set; }

        /// <summary>
        /// 簽核人ID(原始)
        /// </summary>
        public Guid OrgSignEmpID { get; set; }

        /// <summary>
        /// 簽核人ID(預計)
        /// </summary>
        public Guid PreSignEmpID { get; set; }

        /// <summary>
        /// 簽核人員工電子郵件
        /// </summary>
        public string PreSignEmpEmail { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }
    }
}

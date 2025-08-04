using System;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaSignRecordViewModel
    {
        /// <summary>
        /// 排序
        /// </summary>
        public string Order { get; set; }
        /// <summary>
        /// 簽核順序
        /// </summary>
        public int SignStep { get; set; }
        /// <summary>
        /// 簽核層級
        /// </summary>
        public Guid SignLevelID { get; set; }
        public string SignLevelName { get; set; }
        /// <summary>
        /// 考核關卡
        /// </summary>
        public string AssessmentName { get; set; }
        /// <summary>
        /// 簽核狀態
        /// </summary>
        public string StatusCode { get; set; }
        public string StatusName { get; set; }
        /// <summary>
        /// 簽核人ID(原始)
        /// </summary>
        public Guid OrgSignEmpID { get; set; }
        /// <summary>
        /// 簽核人ID(預計)
        /// </summary>
        public Guid PreSignEmpID { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        /// <summary>
        /// 簽核意見
        /// </summary>
        public string Assessment { get; set; }
        /// <summary>
        /// 簽核日期
        /// </summary>
        public string SignTime { get; set; }
        /// <summary>
        /// 代理
        /// </summary>
        public bool IsAgent { get; set; }

        public bool IsSelfEvaluation { get; set; }
        public bool IsFirstEvaluation { get; set; }
        public bool IsSecondEvaluation { get; set; }
        public bool IsThirdEvaluation { get; set; }

        public bool IsHrEvaluation { get; set; }
    }
}

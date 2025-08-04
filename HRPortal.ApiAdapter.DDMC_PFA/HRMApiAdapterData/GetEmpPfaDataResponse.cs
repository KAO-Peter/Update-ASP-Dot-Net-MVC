using System;

namespace HRPortal.ApiAdapter.DDMC_PFA.HRMApiAdapterData
{
    /// <summary>
    /// 員工績效考核資料
    /// </summary>
    public class GetEmpPfaDataResponse
    {
        /// <summary>
        /// 員工代碼
        /// </summary>
        public string EmpID { get; set; }

        /// <summary>
        /// 部門代碼
        /// </summary>
        public string DeptCode { get; set; }

        /// <summary>
        /// 組織代碼
        /// </summary>
        public string PfaOrgCode { get; set; }

        /// <summary>
        /// 組織名稱
        /// </summary>
        public string PfaOrgName { get; set; }

        /// <summary>
        /// 職稱代碼
        /// </summary>
        public string JobTitleCode { get; set; }

        /// <summary>
        /// 職務代碼
        /// </summary>
        public string JobFunctionCode { get; set; }

        /// <summary>
        /// 職等代碼
        /// </summary>
        public string GradeCode { get; set; }

        /// <summary>
        /// 職級代碼
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// 雇用別代碼
        /// </summary>
        public string HireCode { get; set; }

        /// <summary>
        /// 教育程度
        /// </summary>
        public string Education { get; set; }

        /// <summary>
        /// 學校名稱
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// 科系名稱
        /// </summary>
        public string DeptDescription { get; set; }

        /// <summary>
        /// 是否取得學位
        /// </summary>
        public bool? Degree { get; set; }

        /// <summary>
        /// 事假
        /// </summary>
        public double? PersonalLeave { get; set; }

        /// <summary>
        /// 病假
        /// </summary>
        public double? SickLeave { get; set; }

        /// <summary>
        /// 遲到早退次數
        /// </summary>
        public double? LateLE { get; set; }

        /// <summary>
        /// 曠職時數
        /// </summary>
        public double? AWL { get; set; }

        /// <summary>
        /// 本薪
        /// </summary>
        public double? Salary01 { get; set; }

        /// <summary>
        /// 主管加給
        /// </summary>
        public double? Salary02 { get; set; }

        /// <summary>
        /// 交通津貼
        /// </summary>
        public double? Salary03 { get; set; }

        /// <summary>
        /// 免稅伙食津貼
        /// </summary>
        public double? Salary04 { get; set; }

        /// <summary>
        /// 備用
        /// </summary>
        public double? Salary05 { get; set; }

        /// <summary>
        /// 備用
        /// </summary>
        public double? Salary06 { get; set; }

        /// <summary>
        /// 全薪
        /// </summary>
        public double? FullSalary { get; set; }

        /// <summary>
        /// 前一年等第
        /// </summary>
        public string Performance1 { get; set; }

        /// <summary>
        /// 前兩年等第
        /// </summary>
        public string Performance2 { get; set; }

        /// <summary>
        /// 前三年等第
        /// </summary>
        public string Performance3 { get; set; }

        /// <summary>
        /// 評等區間代碼
        /// </summary>
        public string PfaPerformanceCode { get; set; }

        /// <summary>
        /// Performance
        /// </summary>
        public string Performance { get; set; }

        /// <summary>
        /// band
        /// </summary>
        public string band { get; set; }

        /// <summary>
        /// 佔比
        /// </summary>
        public int? Rates { get; set; }

        /// <summary>
        /// Multiplier
        /// </summary>
        public decimal? Multiplier { get; set; }

        /// <summary>
        /// Scores
        /// </summary>
        public double? Scores { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }
    }
}

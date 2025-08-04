using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核員工資料
    /// </summary>
    public class PfaCycleEmpViewModel
    {
        public PfaCycleEmpViewModel()
        {
            SelfScore = true;
            FirstScore = true;
            LastScore = true;
            FinalScore = true;
        }

        /// <summary>
        /// ID
        /// </summary>
        public Guid? ID { get; set; }

        /// <summary>
        /// 績效考核批號ID
        /// </summary>
        public Guid? PfaCycleID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 出勤日期起迄
        /// </summary>
        public string DutyBeginEndDate { get; set; }

        /// <summary>
        /// 員工ID
        /// </summary>
        public Guid EmployeeID { get; set; }

        /// <summary>
        /// 員工編號
        /// </summary>
        public string EmployeeNo { get; set; }

        /// <summary>
        /// 員工姓名
        /// </summary>
        public string EmployeeName { get; set; }

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
        /// 部門組織ID
        /// </summary>
        public Guid? PfaOrgID { get; set; }

        /// <summary>
        /// 部門組織代號
        /// </summary>
        public string PfaOrgCode { get; set; }

        /// <summary>
        /// 部門組織名稱
        /// </summary>
        public string PfaOrgName { get; set; }

        /// <summary>
        /// 雇用別ID
        /// </summary>
        public Guid? HireID { get; set; }

        /// <summary>
        /// 雇用別代號
        /// </summary>
        public string HireCode { get; set; }

        /// <summary>
        /// 雇用別名稱
        /// </summary>
        public string HireName { get; set; }

        /// <summary>
        /// 職稱ID
        /// </summary>
        public Guid? JobTitleID { get; set; }

        /// <summary>
        /// 職稱代號
        /// </summary>
        public string JobTitleCode { get; set; }

        /// <summary>
        /// 職稱名稱
        /// </summary>
        public string JobTitleName { get; set; }

        /// <summary>
        /// 職務ID
        /// </summary>
        public Guid? JobFunctionID { get; set; }

        /// <summary>
        /// 職務代號
        /// </summary>
        public string JobFunctionCode { get; set; }

        /// <summary>
        /// 職務名稱
        /// </summary>
        public string JobFunctionName { get; set; }

        /// <summary>
        /// 職等ID
        /// </summary>
        public Guid? GradeID { get; set; }

        /// <summary>
        /// 職等代號
        /// </summary>
        public string GradeCode { get; set; }

        /// <summary>
        /// 職等名稱
        /// </summary>
        public string GradeName { get; set; }

        /// <summary>
        /// 到職日
        /// </summary>
        public string ArriveDate { get; set; }

        /// <summary>
        /// 離職日
        /// </summary>
        public string LeaveDate { get; set; }

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
        /// 前三年等第
        /// </summary>
        public string StrPerformance { get; set; }

        /// <summary>
        /// 簽核類別ID
        /// </summary>
        public Guid? SignTypeID { get; set; }

        /// <summary>
        /// 簽核類別代號
        /// </summary>
        public string SignTypeCode { get; set; }

        /// <summary>
        /// 簽核類別名稱
        /// </summary>
        public string SignTypeName { get; set; }

        /// <summary>
        /// 身份類別名稱
        /// </summary>
        public string PfaEmpTypeName { get; set; }

        /// <summary>
        /// 代理自評
        /// </summary>
        public bool IsAgent { get; set; }

        /// <summary>
        /// 是否需配比人數
        /// </summary>
        public bool IsRatio { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 個人工作意願及建議事項&年度工作說明
        /// </summary>
        public string SelfAppraisal { get; set; }

        /// <summary>
        /// 自評工作績效分數(自評寫入)
        /// </summary>
        public double? SelfIndicator { get; set; }

        /// <summary>
        /// 自評總分(自評寫入)
        /// </summary>
        public double? PfaSelfScore { get; set; }

        /// <summary>
        /// 主管工作績效分數(初核寫入)
        /// </summary>
        public double? ManagerIndicator { get; set; }

        /// <summary>
        /// 主管勝任能力分數(初核寫入)
        /// </summary>
        public double? ManagerAbility { get; set; }

        /// <summary>
        /// 主管綜合考評(初核寫入)
        /// </summary>
        public string FirstAppraisal { get; set; }

        /// <summary>
        /// 初核總分(初核寫入) => ManagerIndicator + ManagerAbility	
        /// </summary>
        public double? PfaFirstScore { get; set; }

        /// <summary>
        /// 績效等第(初)(初核寫入)
        /// </summary>
        public Guid? FirstPerformance_ID { get; set; }

        /// <summary>
        /// 績效等第(初)(初核寫入)
        /// </summary>
        public string FirstPerformanceName { get; set; }

        /// <summary>
        /// 與過去表現比較(初核寫入)
        /// </summary>
        public Guid? PastPerformance { get; set; }

        /// <summary>
        /// 評價目前工作(初核寫入)
        /// </summary>
        public Guid? NowPerformance { get; set; }

        /// <summary>
        /// 未來發展評斷(初核寫入)
        /// </summary>
        public Guid? Development { get; set; }

        /// <summary>
        /// 主管綜合考評(初核寫入)
        /// </summary>
        public string LastAppraisal { get; set; }

        /// <summary>
        /// 複核總分(複核寫入)
        /// </summary>
        public double? PfaLastScore { get; set; }

        /// <summary>
        /// 核決總分(核決寫入)
        /// </summary>
        public double? PfaFinalScore { get; set; }

        /// <summary>
        /// 主管綜合核決(核決寫入)
        /// </summary>
        public string FinalAppraisal { get; set; }

        /// <summary>
        /// 績效等第ID(複核)
        /// </summary>
        public Guid? LastPerformance_ID { get; set; }

        /// <summary>
        /// 績效等第(複核)
        /// </summary>
        public string LastPerformanceName { get; set; }

        /// <summary>
        /// 績效等第ID(核決)
        /// </summary>
        public Guid? FinalPerformance_ID { get; set; }

        /// <summary>
        /// 績效等第(核決)
        /// </summary>
        public string FinalPerformanceName { get; set; }

        /// <summary>
        /// 簽核類別
        /// </summary>
        public List<SelectListItem> SignTypeList { get; set; }

        #region 權限
        public bool SelfScore { get; set; }
        public bool FirstScore { get; set; }
        public bool LastScore { get; set; }
        public bool FinalScore { get; set; }

        #endregion

    }
}

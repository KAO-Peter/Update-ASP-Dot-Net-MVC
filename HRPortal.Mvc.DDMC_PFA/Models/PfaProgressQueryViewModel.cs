using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaProgressQueryViewModel
    {
        public PfaProgressQueryViewModel() 
        {
            CanShow = new CanShowCol();
        }
        /// <summary>
        /// 績效考核員工ID
        /// </summary>
        public Guid PfaCycleEmpID { get; set; }

        /// <summary>
        /// 考核簽核批號ID
        /// </summary>
        public Guid PfaCycleID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 部門類別ID
        /// </summary>
        public Guid? PfaOrgID { get; set; }

        /// <summary>
        /// 部門組織名稱
        /// </summary>
        public string PfaOrgName { get; set; }

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
        /// 送簽日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 自評總分(自評寫入)
        /// </summary>
        public double? PfaSelfScore { get; set; }

        /// <summary>
        /// 初核總分(初核寫入) => ManagerIndicator + ManagerAbility	
        /// </summary>
        public double? PfaFirstScore { get; set; }

        /// <summary>
        /// 複核總分(複核寫入)
        /// </summary>
        public double? PfaLastScore { get; set; }

        /// <summary>
        /// 核決總分(核決寫入)
        /// </summary>
        public double? PfaFinalScore { get; set; }

        /// <summary>
        /// 簽核狀態
        /// </summary>
        public string SignStatus { get; set; }

        /// <summary>
        /// 簽核狀態名稱
        /// </summary>
        public string StrSignStatus { get; set; }

        /// <summary>
        /// 目前簽核人-員編
        /// </summary>
        public string PreSignEmpNo { get; set; }

        /// <summary>
        /// 目前簽核人-姓名
        /// </summary>
        public string PreSignEmpName { get; set; }

        public ICollection<PfaSignProcess> PfaSignProcesses { get; set; }

        public CanShowCol CanShow {  get; set; }

        public class CanShowCol
        {
            public CanShowCol() 
            {
                SelfScore = false;
                FirstScore = false; 
                LastScore = false;
                FinalScore = false;
            }
            public bool SelfScore { get; set; }
            public bool FirstScore { get; set; }
            public bool LastScore { get; set; }
            public bool FinalScore { get; set; }
        }
    }

    public class PfaProgressQueryPDFViewModel
    {
        public PfaProgressQueryPDFViewModel() 
        {
            this.PfaSignUpload = new List<PfaSignUploadViewModel>();
        }
        /// <summary>
        /// 
        /// </summary>
        public Guid PfaCycleEmpID { get; set; }

        /// <summary>
        /// 考核簽核批號ID
        /// </summary>
        public Guid PfaCycleID { get; set; }

        /// <summary>
        /// 考核簽核批號年度
        /// </summary>
        public string PfaYear { get; set; }

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
        /// 考核部門
        /// </summary>
        public Guid PfaDeptID { get; set; }

        /// <summary>
        /// 部門名稱
        /// </summary>
        public string PfaDeptName { get; set; }

        /// <summary>
        /// 職務ID
        /// </summary>
        public Guid? JobFunctionID { get; set; }

        /// <summary>
        /// 職務
        /// </summary>
        public string JobFunctionName { get; set; }

        /// <summary>
        /// 職稱ID
        /// </summary>
        public Guid? JobTitleID { get; set; }

        /// <summary>
        /// 職稱
        /// </summary>
        public string JobTitleName { get; set; }

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
        /// 性別
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 學校名稱
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// 科系名稱
        /// </summary>
        public string DeptDescription { get; set; }

        /// <summary>
        /// 考勤記錄
        /// </summary>
        public string DutyBeginEndDate { get; set; }

        /// <summary>
        /// 到職日
        /// </summary>
        public DateTime ArriveDate { get; set; }

        /// <summary>
        /// 遲到早退次數
        /// </summary>
        public double? LateLE { get; set; }

        /// <summary>
        /// 事假
        /// </summary>
        public double? PersonalLeave { get; set; }

        /// <summary>
        /// 病假
        /// </summary>
        public double? SickLeave { get; set; }

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
        /// 合計
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
        /// 身份類別名稱
        /// </summary>
        public string PfaEmpTypeName { get; set; }

        /// <summary>
        /// 個人工作意願(自評寫入)
        /// </summary>
        public string SelfAppraisal { get; set; }

        /// <summary>
        /// 自評總分(自評寫入)
        /// </summary>
        public double? PfaSelfScore { get; set; }

        /// <summary>
        /// 績效考核員工訓練紀錄
        /// </summary>
        public List<PfaEmpTrainingViewModel> PfaEmpTraining { get; set; }

        /// <summary>
        /// 績效考核員工工作績效
        /// </summary>
        public List<PfaEmpIndicatorViewModel> PfaEmpIndicator { get; set; }

        /// <summary>
        /// 績效考核能力行為指標
        /// </summary>
        public List<PfaEmpAbilityViewModel> PfaEmpAbility { get; set; }

        /// <summary>
        /// 初核總分(初核寫入) => ManagerIndicator + ManagerAbility	
        /// </summary>
        public double? PfaFirstScore { get; set; }

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
        public string FirstAppraisal { get; set; }

        /// <summary>
        /// 主管綜合考評(複核寫入)
        /// </summary>
        public string LastAppraisal { get; set; }

        /// <summary>
        /// 主管綜合考評(核決寫入)
        /// </summary>
        public string FinalAppraisal { get; set; }

        /// <summary>
        /// 複核總分(複核寫入)
        /// </summary>
        public double? PfaLastScore { get; set; }

        /// <summary>
        /// 核決總分(核決寫入)
        /// </summary>
        public double? PfaFinalScore { get; set; }


        /// <summary>
        /// 與過去表現比較
        /// </summary>
        public List<SelectListItem> PastPerformanceData { get; set; }

        /// <summary>
        /// 評價目前工作
        /// </summary>
        public List<SelectListItem> NowPerformanceData { get; set; }

        /// <summary>
        /// 未來發展評斷
        /// </summary>
        public List<SelectListItem> DevelopmentData { get; set; }


        /// <summary>
        /// 績效考核簽核附件
        /// </summary>
        public List<PfaSignUploadViewModel> PfaSignUpload { get; set; }
    }
}
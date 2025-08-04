using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaCycleRationViewModel
    {
        public Guid ID { get; set; }

        /// <summary>
        /// 績效考核批號ID
        /// </summary>
        public Guid PfaCycleID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 績效考核組織ID
        /// </summary>
        public Guid PfaOrgID { get; set; }

        /// <summary>
        /// 組織名稱
        /// </summary>
        public string PfaOrgName { get; set; }

        /// <summary>
        /// 組織應配比人數
        /// </summary>
        public int? OrgTotal { get; set; }

        /// <summary>
        /// 狀態 (新增時=未送簽，啟動送單=改考評中，全批號簽核結束=考評完成)
        /// <para>m:未送簽</para>
        /// <para>a:考評中</para>
        /// <para>e:考評完成</para>
        /// <para>y:已鎖定</para>
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PfaCycleRationTitleViewModel> RationDetail { get; set; }
    }

    public class PfaCycleRationTitleViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 評等區間代碼
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 評等區間名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 主管分配人數
        /// </summary>
        public int? Staffing { get; set; }
    }

    public class PfaCycleRationCreateViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 績效考核批號ID
        /// </summary>
        public Guid PfaCycleID { get; set; }

        /// <summary>
        /// 績效考核批號清單
        /// </summary>
        public List<SelectListItem> CycleList { get; set; }

        /// <summary>
        /// 績效考核組織ID
        /// </summary>
        public Guid PfaOrgID { get; set; }

        /// <summary>
        /// 績效考核組織清單
        /// </summary>
        public List<SelectListItem> OrgList { get; set; }

        /// <summary>
        /// 部門清單
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 組織應配比人數
        /// </summary>
        public int? OrgTotal { get; set; }

        public PfaCycleRationDetailCreateViewModel[] Data { get; set; }

        /// <summary>
        /// 分配人數合計
        /// </summary>
        public int? StaffingTotal { get; set; }

        /// <summary>
        /// 核對試算合計
        /// </summary>
        public decimal? ScoreTotal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Action { get; set; }
    }

    public class PfaCycleRationDetailCreateViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 績效考核批號ID
        /// </summary>
        public Guid PfaCycleRationID { get; set; }

        /// <summary>
        /// 績效考核評等ID
        /// </summary>
        public Guid PfaPerformanceID { get; set; }

        /// <summary>
        /// 評等區間代碼
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 評等區間名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

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
        /// 分數起
        /// </summary>
        public decimal? ScoresStart { get; set; }

        /// <summary>
        /// 分數迄
        /// </summary>
        public decimal? ScoresEnd { get; set; }

        /// <summary>
        /// 總人數
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 預算人數
        /// </summary>
        public decimal? BudgetTotal { get; set; }

        /// <summary>
        /// 主管分配人數
        /// </summary>
        public int? Staffing { get; set; }

        /// <summary>
        /// 核對試算數
        /// </summary>
        public decimal? TotalScore { get; set; }
    }

    public class OrgChangeItem
    {
        /// <summary>
        /// 部門名稱
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 組織應配比人數
        /// </summary>
        public int Total { get; set; }


        public float BudgetTotal { get; set; }
    }

    public class PfaRatioJobTitleViewModel
    {
        public List<JobTitleItem> JobTitleItems { get; set; }
    }
}
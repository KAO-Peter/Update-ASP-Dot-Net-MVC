using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaAbilityViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 身份類別ID
        /// </summary>
        public Guid PfaEmpTypeID { get; set; }

        /// <summary>
        /// 身份類別名稱
        /// </summary>
        public string PfaEmpTypeName { get; set; }

        /// <summary>
        /// 勝任能力code
        /// </summary>
        public string PfaAbilityCode { get; set; }

        /// <summary>
        /// 勝任能力名稱
        /// </summary>
        public string PfaAbilityName { get; set; }

        /// <summary>
        /// 定義
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 總分
        /// </summary>
        public int? TotalScore { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public Guid CompanyID { get; set; }

        /// <summary>
        /// 明細檔資料
        /// </summary>
        public List<PfaAbilityDetailViewModel> AbilityDetail { get; set; }
        //public string AbilityDetail { get; set; }
    }

    public class PfaAbilityDetailViewModel
    {
        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 關鍵行為指標
        /// </summary>
        public string PfaAbilityKey { get; set; }
    }

    public class PfaAbilityCreateViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid PfaEmpTypeID { get; set; }

        /// <summary>
        /// 身份類別
        /// </summary>
        public List<SelectListItem> EmpTypeList { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 勝任能力code
        /// </summary>
        public string PfaAbilityCode { get; set; }

        /// <summary>
        /// 勝任能力名稱
        /// </summary>
        public string PfaAbilityName { get; set; }

        /// <summary>
        /// 總分
        /// </summary>
        public int? TotalScore { get; set; }

        /// <summary>
        /// 定義
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 勝任能力明細
        /// </summary>
        public AbilityDetailCreateViewModel[] Data { get; set; }
    }

    public class AbilityDetailCreateViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 勝任能力ID
        /// </summary>
        public Guid PfaAbilityID { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 關鍵行為指標
        /// </summary>
        public string PfaAbilityKey { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        public int? UpperLimit { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        public int? LowerLimit { get; set; }
    }
}
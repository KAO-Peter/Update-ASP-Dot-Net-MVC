using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaIndicatorViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 指標代碼
        /// </summary>
        public string PfaIndicatorCode { get; set; }

        /// <summary>
        /// 指標代碼名稱
        /// </summary>
        public string PfaIndicatorName { get; set; }

        /// <summary>
        /// 定義
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 權重
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public Guid CompanyID { get; set; }

        /// <summary>
        /// 程度說明
        /// </summary>
        public List<PfaIndicatorDetailViewModel> PfaIndicatorDetail { get; set; }
    }

    public class PfaIndicatorDetailViewModel
    {
        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }

        /// <summary>
        /// 程度說明
        /// </summary>
        public string PfaIndicatorKey { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        public decimal? UpperLimit { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        public decimal? LowerLimit { get; set; }
    }

    public class PfaIndicatorCreateViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        [DisplayName("排序")]
        [Required]
        public int? Ordering { get; set; }

        /// <summary>
        /// 指標代碼
        /// </summary>
        [DisplayName("指標代碼")]
        [Required(ErrorMessage = "指標代碼不可空白")]
        public string PfaIndicatorCode { get; set; }

        /// <summary>
        /// 指標代碼名稱
        /// </summary>
        public string PfaIndicatorName { get; set; }

        /// <summary>
        /// 權重
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// 定義
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 勝任能力明細
        /// </summary>
        public PfaIndicatorDetailCreateViewModel[] Data { get; set; }
    }

    public class PfaIndicatorDetailCreateViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 工作績效ID
        /// </summary>
        public Guid PfaIndicatorID { get; set; }

        /// <summary>
        /// 關鍵行為指標
        /// </summary>
        public string PfaIndicatorKey { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        public decimal? UpperLimit { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        public decimal? LowerLimit { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        public int? Ordering { get; set; }
    }
}
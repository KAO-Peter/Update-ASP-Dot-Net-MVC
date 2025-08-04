using System;
using System.ComponentModel.DataAnnotations;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaPerformanceViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public Guid CompanyID { get; set; }

        /// <summary>
        /// 評等區間代碼
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 評等區間名稱
        /// </summary>
        public string Name { get; set; }

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
        public string Scores { get; set; }
    }

    public class CreatePfaPerformance
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "請輸入ID")]
        public Guid ID { get; set; }


        /// <summary>
        /// 評等區間代碼
        /// </summary>
        [Required(ErrorMessage = "請輸入評等區間代碼")]
        [StringLength(50, ErrorMessage = "欄位長度不得大於 50 個字元")]
        public string Code { get; set; }

        /// <summary>
        /// 評等區間名稱
        /// </summary>
        public string Name { get; set; }

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

        [Required(ErrorMessage = "請輸入公司別")]
        /// <summary>
        /// 公司ID
        /// </summary>
        public Guid CompanyID { get; set; }
    }
}
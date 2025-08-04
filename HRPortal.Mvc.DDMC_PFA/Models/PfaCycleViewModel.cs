using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    /// <summary>
    /// 績效考核簽核批號
    /// </summary>
    public class PfaCycleViewModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 考績年度
        /// </summary>
        public string PfaYear { get; set; }

        /// <summary>
        /// 在職基礎日期
        /// </summary>
        public DateTime? ServeBasedate { get; set; }

        /// <summary>
        /// 出勤日期起
        /// </summary>
        public DateTime? DutyBeginDate { get; set; }

        /// <summary>
        /// 出勤日期迄
        /// </summary>
        public DateTime? DutyEndDate { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        public string Desription { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 啟動日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 分公司ID
        /// </summary>
        public Guid CompanyID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public List<SelectListItem> PfaFormNoList { get; set; }
    }
}

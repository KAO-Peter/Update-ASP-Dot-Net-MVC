using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaCycleRejectQueryViewModel
    {
        /// <summary>
        ///  績效考核ID
        /// </summary>
        public Guid PfaCycleID { get; set; }

        /// <summary>
        /// 績效考核批號
        /// </summary>
        public string PfaFormNo { get; set; }

        /// <summary>
        /// 考績年度
        /// </summary>
        public string PfaYear { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 部門 LIST
        /// </summary>
        public List<PfaCycleOrgSentViewModel> PfaOrgtList { get; set; }


    }

}

using System;
using System.Collections.Generic;

namespace HRPortal.Mvc.DDMC_PFA.Models
{
    public class PfaCycleEmpBatchViewModel
    {
        /// <summary>
        /// 績效考核批號ID
        /// </summary>
        public Guid? PfaCycleID { get; set; }

        /// <summary>
        /// 部門組織ID
        /// </summary>
        public Guid? PfaOrgID { get; set; }

        /// <summary>
        /// 評等人數配比資料
        /// </summary>
        public PfaCycleRationDataViewModel PfaCycleRationData { get; set; }

        /// <summary>
        /// 考核人員資料
        /// </summary>
        public List<PfaCycleEmpSignViewModel> PfaCycleEmp { get; set; }
    }
}

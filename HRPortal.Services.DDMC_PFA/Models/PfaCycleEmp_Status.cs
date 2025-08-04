using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Models
{
    public class PfaCycleEmp_Status
    {
        /// <summary>
        /// m:未送簽
        /// </summary>
        public const string NotSubmittedForApproval = "m";

        /// <summary>
        /// a:簽核中
        /// </summary>
        public const string InApprovalProcess = "a";

        /// <summary>
        /// e:已審核
        /// </summary>
        public const string Approved = "e";

        /// <summary>
        /// y:已鎖定
        /// </summary>
        public const string Locked = "y";

    }

}
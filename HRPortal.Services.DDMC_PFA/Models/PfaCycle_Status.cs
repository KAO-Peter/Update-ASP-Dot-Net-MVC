using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Models
{
    public class PfaCycle_Status
    {

        /// <summary>
        /// m:未送簽
        /// </summary>
        public const string NotSubmitted = "m";

        /// <summary>
        /// a:簽核中
        /// </summary>
        public const string InApprovalProcess = "a";

        /// <summary>
        /// e:考評完成
        /// </summary>
        public const string PfaFinish = "e";

        /// <summary>
        /// y:已鎖定
        /// </summary>
        public const string Lock = "y";

    }

}
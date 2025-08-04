using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Models
{
    public class PfaSigner_SignType
    {
        /// <summary>
        /// 1: 自評
        /// </summary>
        public const string SelfAssessment = "1";

        /// <summary>
        /// 2: 初核
        /// </summary>
        public const string FirstReview = "2";

        /// <summary>
        /// 3: 複核
        /// </summary>
        public const string SecondaryReview = "3";

        /// <summary>
        /// 4: 核決
        /// </summary>
        public const string FinalApproval = "4";
    }

}
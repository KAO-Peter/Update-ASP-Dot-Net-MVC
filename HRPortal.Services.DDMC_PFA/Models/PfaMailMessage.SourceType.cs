using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Models
{
    public class PfaMailMessage_SourceType
    {

        /// <summary>
        /// 1: 一般送簽
        /// </summary>
        public const string GeneralSubmission = "1";

        /// <summary>
        /// 2: 代理自簽
        /// </summary>
        public const string AgentSelfSubmission = "2";

        /// <summary>
        /// 3: 指定轉簽
        /// </summary>
        public const string DesignatedTransfer = "3";

        /// <summary>
        /// 4: 自評稽催通知
        /// </summary>
        public const string SelfAssessmentNotification = "4";

        /// <summary>
        /// 5: 初核主管稽催
        /// </summary>
        public const string InitialSupervisorFollowUp = "5";

        /// <summary>
        /// 6: 複核主管稽催
        /// </summary>
        public const string ReviewSupervisorFollowUp = "6";

        /// <summary>
        /// 7: 逾期通知
        /// </summary>
        public const string OverdueNotification = "7";

        /// <summary>
        /// 8: 核決
        /// </summary>
        public const string FinalSupervisorFollowUp = "8";


    }

}
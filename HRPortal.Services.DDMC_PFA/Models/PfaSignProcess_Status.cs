using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Models
{
    /// <summary>
    /// PfaOptionService
    /// 
    /// select * from PfaOption o
    /// left join PfaOptionGroup og on og.id = o.PfaOptionGroupID
    /// where og.OptionGroupCode = 'SignStatus'
    /// </summary>
    public class PfaSignProcess_Status
    {

        /// <summary>
        /// m:未收件
        /// </summary>
        public const string NotReceived = "m";

        /// <summary>
        /// c:待評核
        /// </summary>
        public const string PendingReview = "c";

        /// <summary>
        /// t:待核決
        /// </summary>
        public const string PendingThirdReview = "t";

        /// <summary>
        /// a:已評核
        /// </summary>
        public const string Reviewed = "a";

        /// <summary>
        /// e:已送出
        /// </summary>
        public const string Submitted = "e";

        /// <summary>
        /// r:退回修改
        /// </summary>
        public const string ReturnedForModification = "r";

        /// <summary>
        /// b:已退回
        /// </summary>
        public const string Returned = "b";
    }

}
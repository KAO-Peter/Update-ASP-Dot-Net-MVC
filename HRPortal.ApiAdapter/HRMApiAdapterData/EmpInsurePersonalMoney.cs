using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.ApiAdapter.HRMApiAdapterData
{
    /// <summary>
    ///  保費明細清單
    /// </summary>
    public class EmpInsurePersonalMoney
    {
        /// <summary>
        ///  姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  屬性 本人 or 眷屬
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///  身份證字號
        /// </summary>
        public string IDNumber { get; set; }

        /// <summary>
        ///   勞保保費
        /// </summary>
        public double LaborInsurePersonalMoney { get; set; }

        /// <summary>
        ///   健保保費
        /// </summary>
        public double HealthInsurePersonalMoney { get; set; }

        /// <summary>
        ///   補充保費
        /// </summary>
        public double EmpHealthAddPersonalMoney { get; set; }//補充保費
    }
}

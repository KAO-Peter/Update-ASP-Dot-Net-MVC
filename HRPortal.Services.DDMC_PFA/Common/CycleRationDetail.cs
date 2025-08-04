using HRPortal.DBEntities.DDMC_PFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Common
{
    public class CycleRationDetail
    {
        /// <summary>
        /// 檢查配比
        /// </summary>
        /// <param name="orgTotal">pfaCycleRation.OrgTotal</param>
        /// <param name="pfaCycleRationDetails">{Multiplier, LastFinal}[]</param>
        /// <returns></returns>
        public static bool CheckSecondEvaluationRatio(int? orgTotal, List<PfaCycleRationDetail> pfaCycleRationDetails, out decimal totalScore)
        {
            if (orgTotal.HasValue == false || orgTotal.Value <= 0) 
            {
                throw new Exception("缺少必要的 OrgTotal ");
            }

            totalScore = 0;
            foreach (var pfaCycleRationDetail in pfaCycleRationDetails)
            {
                if (pfaCycleRationDetail.Multiplier.HasValue
                    && pfaCycleRationDetail.Multiplier.Value > 0
                    && pfaCycleRationDetail.SecondFinal.HasValue
                    && pfaCycleRationDetail.SecondFinal.Value > 0)
                {
                    totalScore += pfaCycleRationDetail.SecondFinal.Value
                                  * pfaCycleRationDetail.Multiplier.Value;
                }

            }

            return totalScore <= orgTotal;
        }


        public static bool CheckSecondEvaluationRatio(int? OrgTotal, List<PfaCycleRationDetail> pfaCycleRationDetails)
        {
            decimal totalScore = 0;

            return CheckSecondEvaluationRatio(OrgTotal, pfaCycleRationDetails, out totalScore);
        }

        public static bool CheckThirdEvaluationRatio(int? orgTotal, List<PfaCycleRationDetail> pfaCycleRationDetails, out decimal totalScore)
        {
            if (orgTotal.HasValue == false || orgTotal.Value <= 0)
            {
                throw new Exception("缺少必要的 OrgTotal ");
            }

            totalScore = 0;
            foreach (var pfaCycleRationDetail in pfaCycleRationDetails)
            {
                if (pfaCycleRationDetail.Multiplier.HasValue
                    && pfaCycleRationDetail.Multiplier.Value > 0
                    && pfaCycleRationDetail.ThirdFinal.HasValue
                    && pfaCycleRationDetail.ThirdFinal.Value > 0)
                {
                    totalScore += pfaCycleRationDetail.ThirdFinal.Value
                                  * pfaCycleRationDetail.Multiplier.Value;
                }

            }

            return totalScore <= orgTotal;
        }

        public static bool CheckThirdEvaluationRatio(int? OrgTotal, List<PfaCycleRationDetail> pfaCycleRationDetails)
        {
            decimal totalScore = 0;

            return CheckThirdEvaluationRatio(OrgTotal, pfaCycleRationDetails, out totalScore);
        }
    }
}

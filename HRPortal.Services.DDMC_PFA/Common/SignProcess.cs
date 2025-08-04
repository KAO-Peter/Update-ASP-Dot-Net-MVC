using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRPortal.Services.DDMC_PFA.Common
{
    public class SignProcess
    {
        /// <summary>
        /// 取得下一關
        /// 依照 SignStep 排序，取得最小 且 未收件的
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycleEmpID"></param>
        /// <param name="selfId"></param>
        /// <returns></returns>
        public static PfaSignProcess GetNextProcess(DbSet<PfaSignProcess> pfaSignProcess, Guid pfaCycleEmpID, Guid selfId)
        {
            return pfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmpID
                                && x.Status == PfaSignProcess_Status.NotReceived
                                && x.ID != selfId
                                ).OrderBy(x => x.SignStep).FirstOrDefault();
        }

        /// <summary>
        /// 取得上一關
        /// 依照 SignStep 排序，取得最大 且 SignStep 小於 now SignStep
        /// </summary>
        /// <param name="pfaSignProcess"></param>
        /// <param name="pfaCycleEmpID"></param>
        /// <param name="nowSignStep"></param>
        /// <returns></returns>
        public static PfaSignProcess GetPreviousProcess(DbSet<PfaSignProcess> pfaSignProcess, Guid pfaCycleEmpID, int nowSignStep)
        {
            return pfaSignProcess.Where(x => x.PfaCycleEmpID == pfaCycleEmpID
                                    && x.SignStep < nowSignStep)
                                .OrderByDescending(x => x.SignStep)
                                .FirstOrDefault();
        }



    }
}

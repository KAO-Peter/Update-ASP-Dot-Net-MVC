using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public partial class PfaCycleService : BaseCrudService<PfaCycle>
    {



        /// <summary>
        /// 鎖定
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="UID"></param>
        /// <returns></returns>
        public Result PfaCycleLock(Guid Id, Guid UID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var pfaCycleEmpList = db.PfaCycleEmp.Where(x => x.PfaCycleID == Id).ToList();
                        foreach (var pfaCycleEmp in pfaCycleEmpList)
                        {
                            pfaCycleEmp.Status = PfaCycleEmp_Status.Locked; // y:已鎖定
                            pfaCycleEmp.ModifiedBy = UID;
                            pfaCycleEmp.ModifiedTime = DateTime.Now;
                        }

                        var pfaCycle = db.PfaCycle.FirstOrDefault(x => x.ID == Id);
                        pfaCycle.Status = PfaCycleEmp_Status.Locked; // y:已鎖定
                        pfaCycle.ModifiedBy = UID;
                        pfaCycle.ModifiedTime = DateTime.Now;

                        db.SaveChanges();

                        result.success = true;
                        result.message = "資料鎖定成功";
                        result.log = string.Format("資料鎖定成功");

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = string.Format("資料鎖定失敗,{0}", ex.Message);
                        result.log = string.Format("資料鎖定失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}
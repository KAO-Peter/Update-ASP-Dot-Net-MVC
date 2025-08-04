using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaSignUploadService : BaseCrudService<PfaSignUpload>
    {
        public PfaSignUploadService(HRPortal_Services services) : base(services)
        {
        }

        /// <summary>
        /// 上傳
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Result CreatePfaSignUpload(List<PfaSignUpload> data)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        db.PfaSignUpload.AddRange(data);
                        db.SaveChanges();

                        result.success = true;
                        result.message = "上傳成功";
                        result.log = "上傳成功";

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "上傳失敗";
                        result.log = string.Format("上傳失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 上傳刪除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Result DelPfaSignUpload(Guid Id)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        var data = db.PfaSignUpload.FirstOrDefault(x => x.ID == Id);
                        if (File.Exists(data.Href))
                            File.Delete(data.Href);

                        db.PfaSignUpload.Remove(data);
                        db.SaveChanges();

                        result.success = true;
                        result.message = "刪除成功";
                        result.log = "刪除成功";

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "刪除失敗";
                        result.log = string.Format("刪除失敗,Message:{0},{1}", ex.Message, ex.StackTrace);
                    }
                }
            }

            return result;
        }
    }
}

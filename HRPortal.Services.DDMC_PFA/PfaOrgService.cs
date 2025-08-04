using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaOrgService : BaseCrudService<PfaOrg>
    {
        public PfaOrgService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaOrg> GetPfaOrgData(string txtOrgCode, string txtOrgName)
        {
            var result = GetAll();

            if (!string.IsNullOrEmpty(txtOrgCode))
            {
                result = result.Where(x => x.PfaOrgCode.Contains(txtOrgCode));
            }
            if (!string.IsNullOrEmpty(txtOrgName))
            {
                result = result.Where(x => x.PfaOrgName.Contains(txtOrgName));
            }
            return result.OrderBy(x => x.CompanyID).ThenBy(x => x.Ordering).ThenBy(x => x.PfaOrgCode).ToList();
        }

        public PfaOrg GetPfaOrg(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public bool IsExist(string code)
        {
            return Where(x => x.PfaOrgCode == code).Any();
        }

        public Result DeletePfaOrg(Guid PfaOrgID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region Delete PfaOrgDept
                        var PfaOrgDept = db.PfaOrgDept.Where(x => x.PfaOrgID == PfaOrgID);
                        foreach (var item in PfaOrgDept)
                        {
                            db.PfaOrgDept.Remove(item);
                        }
                        db.SaveChanges();
                        #endregion

                        #region Delete PfaOrg
                        var PfaOrg = db.PfaOrg.Where(x => x.ID == PfaOrgID);
                        foreach (var item in PfaOrg)
                        {
                            db.PfaOrg.Remove(item);
                        }
                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "刪除成功";
                        result.log = string.Format("刪除成功,ID:{0}", PfaOrgID);

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "刪除失敗";
                        result.log = string.Format("刪除失敗,ID:{0},Message:{1},{2}", PfaOrgID, ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}
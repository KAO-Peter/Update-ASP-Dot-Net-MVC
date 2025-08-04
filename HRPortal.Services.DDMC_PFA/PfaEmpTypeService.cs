using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaEmpTypeService : BaseCrudService<PfaEmpType>
    {
        public PfaEmpTypeService(HRPortal_Services services) : base(services)
        {
        }

        public List<PfaEmpType> GetPfaEmpTypeData(string txtEmpTypeCode, string txtEmpTypeName)
        {
            var result = GetAll();

            if (!string.IsNullOrEmpty(txtEmpTypeCode))
            {
                result = result.Where(x => x.PfaEmpTypeCode.Contains(txtEmpTypeCode));
            }
            if (!string.IsNullOrEmpty(txtEmpTypeName))
            {
                result = result.Where(x => x.PfaEmpTypeName.Contains(txtEmpTypeName));
            }
            return result.OrderBy(x => x.CompanyID).ThenBy(x => x.Ordering).ThenBy(x => x.PfaEmpTypeCode).ToList();
        }

        public bool IsExist(string code)
        {
            return Where(x => x.PfaEmpTypeCode == code).Any();
        }

        public PfaEmpType GetPfaEmpType(Guid? id)
        {
            return Where(x => x.ID == id).FirstOrDefault();
        }

        public Result DeletePfaEmpType(Guid PfaEmpTypeID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region Delete PfaEmpTypeTargets
                        var PfaEmpTypeDept = db.PfaEmpTypeTargets.Where(x => x.PfaEmpTypeID == PfaEmpTypeID);
                        db.PfaEmpTypeTargets.RemoveRange(PfaEmpTypeDept);
                        db.SaveChanges();
                        #endregion

                        #region Delete PfaEmpType
                        var PfaEmpType = db.PfaEmpType.Where(x => x.ID == PfaEmpTypeID);
                        db.PfaEmpType.RemoveRange(PfaEmpType);
                        db.SaveChanges();
                        #endregion

                        result.success = true;
                        result.message = "刪除成功";
                        result.log = string.Format("刪除成功,ID:{0}", PfaEmpTypeID);

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "刪除失敗";
                        result.log = string.Format("刪除失敗,ID:{0},Message:{1},{2}", PfaEmpTypeID, ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}
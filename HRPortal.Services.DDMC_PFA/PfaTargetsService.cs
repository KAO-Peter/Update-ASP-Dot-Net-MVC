using HRPortal.DBEntities.DDMC_PFA;
using HRPortal.Services.DDMC_PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace HRPortal.Services.DDMC_PFA
{
    public class PfaTargetsService : BaseCrudService<PfaTargets>
    {
        public PfaTargetsService(HRPortal_Services services) : base(services)
        {
        }

        public List<Guid> GetJobTitleID(Guid id)
        {
            return GetAll().Where(x => x.SignTypeID == id).Select(x => x.JobTitleID).ToList();
        }

        public Result SaveJobTitle(Guid SignTypeID, string SelJboTitleID, Guid employeeID)
        {
            Result result = new Result() { success = false, message = string.Empty, log = string.Empty };

            using (NewHRPortalEntitiesDDMC_PFA db = new NewHRPortalEntitiesDDMC_PFA())
            {
                using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                {
                    try
                    {
                        #region 把適用職稱清掉
                        if (!string.IsNullOrEmpty(SelJboTitleID))
                        {
                            string[] SelTitleList = SelJboTitleID.Split('|');
                            foreach (var item in SelTitleList)
                            {
                                Guid TitleID = Guid.Parse(item);
                                var Title = db.PfaTargets.Where(x => x.SignTypeID != SignTypeID && x.JobTitleID == TitleID);
                                db.PfaTargets.RemoveRange(Title);
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        #region Delete Old PfaTargets
                        var oldPfaEmpTypeTargets = db.PfaTargets.Where(x => x.SignTypeID == SignTypeID);
                        db.PfaTargets.RemoveRange(oldPfaEmpTypeTargets);
                        db.SaveChanges();
                        #endregion

                        #region Add New PfaTargets
                        if (!string.IsNullOrEmpty(SelJboTitleID))
                        {
                            string[] SelDeptList = SelJboTitleID.Split('|');
                            foreach (var item in SelDeptList)
                            {
                                Guid TitleID = Guid.Parse(item);

                                PfaTargets obj = new PfaTargets();
                                obj.ID = Guid.NewGuid();
                                obj.SignTypeID = SignTypeID;
                                obj.JobTitleID = TitleID;
                                obj.CreatedBy = employeeID;
                                obj.CreatedTime = DateTime.Now;

                                db.PfaTargets.Add(obj);
                                db.SaveChanges();
                            }
                        }
                        #endregion

                        result.success = true;
                        result.message = "適用職稱儲存成功";
                        result.log = string.Format("適用職稱儲存成功,SignTypeID:{0}", SignTypeID);

                        //完成交易
                        tx.Complete();
                    }
                    catch (Exception ex)
                    {
                        result.success = false;
                        result.message = "適用職稱儲存失敗";
                        result.log = string.Format("適用職稱儲存失敗,SignTypeID:{0},Message:{1},{2}", SignTypeID, ex.Message, ex.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}
